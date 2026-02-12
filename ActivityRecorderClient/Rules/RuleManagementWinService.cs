using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Rules
{
	public class RuleManagementWinService : RuleManagementService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Form owner;
		private WorkDetectorRulesForm workDetectorRulesForm;
		private WorkDetectorRuleEditForm learningForm;
		private WorkDetectorRuleEditForm deleteRuleForm;
		private readonly WatchedWindowsManager watchedWindowsManager = new WatchedWindowsManager();
		private readonly List<IntPtr> handlesToRemove = new List<IntPtr>();

		public RuleManagementWinService(SynchronizationContext guiSynchronizationContext, CaptureCoordinator captureCoordinator, INotificationService notificationService, Form owner)
			: base(guiSynchronizationContext, captureCoordinator, notificationService)
		{
			this.owner = owner;
			watchedWindowsManager.WatchedWindowClosed += WatchedWindowClosed;
		}

		private void WatchedWindowClosed(object sender, SingleValueEventArgs<IntPtr> e)
		{
			//called from bg thread
			//if WorkDetectorRulesForm is shown during this call then saving that would bring back these deleted rules, we have to prevent that
			GuiSynchronizationContext.Post(_ =>
				{
					if (workDetectorRulesForm != null && !workDetectorRulesForm.IsDisposed)
					{
						handlesToRemove.Add(e.Value);
						log.Info("WorkDetectorRulesForm is shown but Window closed " + e.Value);
					}
					else
					{
						log.Debug("WorkDetectorRulesForm is NOT shown and Window closed " + e.Value);
					}
					CaptureCoordinator.RemoveUserRules(n => RemoveRuleWithHandleFilter(n, e.Value));
				}, null);
		}

		public override bool ShouldSkipLoadingUserRule(WorkDetectorRule n) //don't load learnt rules with window handles, because those might not be valid anymore
		{
			Dictionary<string, string> plg;
			string val;
			return n.CreatedFromLearningRule
				&& n.ExtensionRulesByIdByKey != null
				&& n.ExtensionRulesByIdByKey.TryGetValue(PluginWindowHandle.PluginId, out plg)
				&& plg.TryGetValue(PluginWindowHandle.KeyHandle, out val);
		}

		private static bool RemoveRuleWithHandleFilter(WorkDetectorRule n, IntPtr wHnd)
		{
			Dictionary<string, string> plg;
			string val;
			return n.CreatedFromLearningRule
				&& n.ExtensionRulesByIdByKey != null
				&& n.ExtensionRulesByIdByKey.TryGetValue(PluginWindowHandle.PluginId, out plg)
				&& plg.TryGetValue(PluginWindowHandle.KeyHandle, out val)
				&& val == wHnd.ToString();
		}

		private Size learnRuleFromCaptureGuiSize;
		protected override Size LearnRuleFromCaptureGuiSize
		{
			get
			{
				if (learnRuleFromCaptureGuiSize == Size.Empty)
				{
					using (var form = new WorkDetectorRuleEditForm())
					{
						learnRuleFromCaptureGuiSize = new Size(form.Width, form.Height - form.PanelHeight);
					}
				}
				return learnRuleFromCaptureGuiSize;
			}
		}

		protected override bool CanShowForm()
		{
			return (workDetectorRulesForm == null || workDetectorRulesForm.IsDisposed)
				&& (learningForm == null || learningForm.IsDisposed)
				&& (deleteRuleForm == null || deleteRuleForm.IsDisposed)
				&& !CaptureCoordinator.CurrentWorkController.IsShuttingDown;
		}

		protected override void DisplayWorkDetectorRulesEditingGuiImpl(bool hotKeyPressed)
		{
			var dc = hotKeyPressed ? CaptureCoordinator.GetDesktopCapture() : null; //ctor of WorkDetectorRulesForm will mess up active window
			using (workDetectorRulesForm = new WorkDetectorRulesForm()
			{
				DesktopCapture = dc,
				Owner = owner,
			})
			{
				workDetectorRulesForm.UpdateMenu(CaptureCoordinator.CurrentMenuLookup);
				var rules = CaptureCoordinator.GetUserRules();
				//since we have no column for validity date atm. skip loading outdated rules to the gui
				foreach (var rule in rules)
				{
					if (WorkDetector.IsRuleOutdated(rule))
					{
						log.Error("Skip displaying outdated rule " + rule);
						continue;
					}
					workDetectorRulesForm.Rules.Add(rule);
				}
				//if I click on a notfication and then press the shortcut for this form it will close when
				//the notfification closes if I use workDetectorRulesForm.ShowDialog(). 
				//Using workDetectorRulesForm.ShowDialog(this) solves this problem.
				log.Info("Showing WorkDetectorRulesEditingGuiImpl");
				var result = workDetectorRulesForm.ShowDialog(owner);
				log.Info("Closed WorkDetectorRulesEditingGuiImpl " + result);
				if (result != DialogResult.OK)
				{
					handlesToRemove.Clear(); //rules are not saved so we don't have to remove them again
					return;
				}

				var newRules = workDetectorRulesForm.Rules.ToList();
				foreach (var handleToRemove in handlesToRemove)
				{
					for (int i = 0; i < newRules.Count; i++)
					{
						if (!RemoveRuleWithHandleFilter(newRules[i], handleToRemove)) continue;
						log.Info("Removing user rule before saving " + newRules[i]);
						newRules.RemoveAt(i--); //remove rules which were deleted during the form was shown
					}
				}
				handlesToRemove.Clear();
				CaptureCoordinator.SetUserRules(newRules); //there is a race here
			}
		}

		protected override void DisplayLearnRuleFromCaptureGuiImpl(WorkDetectorRule rule, DesktopCapture desktopCapture, DesktopWindow matchedWindow, int? wokrId, string cancelKey, Point topLeftLocation)
		{
			learningForm = new WorkDetectorRuleEditForm()
			{
				MatchedWindow = matchedWindow, //the new rule should always match this window
				MatchedCapture = desktopCapture,
				Text = Labels.AutoRuleData_LearnNewRule,
				Owner = owner,
				CancelKey = cancelKey,
				CancelTime = TimeSpan.FromMinutes(1),
				TopMost = true,
				StartPosition = FormStartPosition.Manual,
				Location = topLeftLocation
			};
			learningForm.FormClosed += LearningFormClosed;
			learningForm.Edit(rule, wokrId, CaptureCoordinator.CurrentMenuLookup, WorkDetectorRuleEditForm.DisplayType.LearnNewRule, false); //not modal
			var actRecForm = owner as ActivityRecorderForm;
			if (actRecForm == null) return;
			actRecForm.SetAlternativeMenu(ActRecMenuItemClick, ActRecMenuButtonClick, Labels.AutoRuleData_LearnNewRuleSimple);
		}

		protected override void DisplayWorkDetectorRuleDeletingGuiImpl()
		{
			var dc = CaptureCoordinator.GetDesktopCapture();
			var matchingRule = CaptureCoordinator.GetMatchingRule(dc);

			if (matchingRule == null)
			{
				log.Info("DisplayWorkDetectorRuleDeletingGuiImpl: There is no active rule at the moment!");
				return;
			}
			var userRules = CaptureCoordinator.GetUserRules();
			if (!userRules.Any(n => matchingRule.Equals(n)))
			{
				log.Info("DisplayWorkDetectorRuleDeletingGuiImpl: Current rule can't be deleted!");
				return;
			}

			using (deleteRuleForm = new WorkDetectorRuleEditForm())
			{
				deleteRuleForm.Owner = owner;
				deleteRuleForm.Text = Labels.AutoRuleData_DeleteCurrentRule;
				log.Info("Showing DisplayWorkDetectorRuleDeletingGuiImpl");
				deleteRuleForm.Edit(matchingRule, null, CaptureCoordinator.CurrentMenuLookup, WorkDetectorRuleEditForm.DisplayType.DeleteRule, true);
				log.Info("Closed DisplayWorkDetectorRuleDeletingGuiImpl " + deleteRuleForm.DialogResult);
				if (deleteRuleForm.DialogResult != DialogResult.OK) return;
			}

			CaptureCoordinator.RemoveUserRules(n => matchingRule.Equals(n));
		}

		protected override void UpdateMenu(ClientMenuLookup menuLookup)
		{
			if (learningForm != null && !learningForm.IsDisposed)
			{
				learningForm.UpdateMenu(menuLookup);
			}
			if (workDetectorRulesForm != null && !workDetectorRulesForm.IsDisposed)
			{
				workDetectorRulesForm.UpdateMenu(menuLookup);
			}
		}

		private void ActRecMenuItemClick(WorkDataEventArgs e)
		{
			SetWorkIfApplicable(e, true);
		}

		private void ActRecMenuButtonClick(WorkDataEventArgs e)
		{
			SetWorkIfApplicable(e, false);
		}

		private void SetWorkIfApplicable(WorkDataEventArgs e, bool shouldClose)
		{
			Debug.Assert(learningForm != null && e.WorkData != null && e.WorkData.Id.HasValue);
			if (learningForm == null || e.WorkData == null || !e.WorkData.Id.HasValue || !e.WorkData.IsVisibleInRules) return;
			//if (e.WorkData.ManualAddWorkDuration.HasValue) return; //can have rules for works with ManualAddWorkDuration now...
			learningForm.SetWork(e.WorkData.Id.Value, shouldClose);
		}

		private void LearningFormClosed(object sender, FormClosedEventArgs e)
		{
			if (learningForm == null)
			{
				log.Info("Skip multiple closed events");
				return;
			}
			if (learningForm.DialogResult == DialogResult.OK)
			{
				CaptureCoordinator.AddUserRule(learningForm.Rule);

				Dictionary<string, string> plg;
				string val;
				if (learningForm.Rule.ExtensionRulesByIdByKey != null
					&& learningForm.Rule.ExtensionRulesByIdByKey.TryGetValue(PluginWindowHandle.PluginId, out plg)
					&& plg.TryGetValue(PluginWindowHandle.KeyHandle, out val)
					)
				{
					Debug.Assert(learningForm.MatchedWindow.Handle.ToString() == val); //todo don't do this...
					watchedWindowsManager.AddWathcedWindow(learningForm.MatchedWindow.Handle);
				}
			}
			else
			{
				DelayNextDisplayLearnRuleFromCaptureGui(learningForm.CancelKey, learningForm.CancelTime);
			}
			learningForm = null;
			var actRecForm = owner as ActivityRecorderForm;
			if (actRecForm == null) return;
			actRecForm.SetAlternativeMenu(null, null, null);
		}
	}
}
