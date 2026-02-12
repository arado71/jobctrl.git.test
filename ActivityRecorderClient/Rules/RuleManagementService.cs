using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Rules.Generation;

namespace Tct.ActivityRecorderClient.Rules
{
	public abstract class RuleManagementService : IRuleManagementService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan nfInvalidRuleDuration = TimeSpan.FromSeconds(20);
		private static readonly TimeSpan defaultExpirationTime = TimeSpan.FromMinutes(1);
		private readonly CaptureCoordinator captureCoordinator;
		private readonly INotificationService notificationService;
		protected CaptureCoordinator CaptureCoordinator { get { return captureCoordinator; } }
		private readonly SynchronizationContext guiSynchronizationContext;
		protected SynchronizationContext GuiSynchronizationContext { get { return guiSynchronizationContext; } }
		private readonly CachedDictionary<string, byte> cancelledCaptures = new CachedDictionary<string, byte>(defaultExpirationTime, true);
		private readonly LearningRuleBuilder learningRuleBuilder = new LearningRuleBuilder();

		protected RuleManagementService(SynchronizationContext guiSynchronizationContext, CaptureCoordinator captureCoordinator, INotificationService notificationService)
		{
			this.guiSynchronizationContext = guiSynchronizationContext;
			this.captureCoordinator = captureCoordinator;
			this.captureCoordinator.CurrentMenuChanged += CurrentMenuChanged;
			this.notificationService = notificationService;
		}

		protected abstract void DisplayWorkDetectorRulesEditingGuiImpl(bool hotKeyPressed);
		protected abstract void DisplayLearnRuleFromCaptureGuiImpl(WorkDetectorRule newRule, DesktopCapture desktopCapture, DesktopWindow matchedWindow, int? workId, string cancelKey, Point topLeftLocation);
		protected abstract void DisplayWorkDetectorRuleDeletingGuiImpl();
		protected abstract bool CanShowForm();
		protected abstract Size LearnRuleFromCaptureGuiSize { get; }
		protected abstract void UpdateMenu(ClientMenuLookup menuLookup);

		public virtual bool ShouldSkipLoadingUserRule(WorkDetectorRule rule)
		{
			return false;
		}

		public void DisplayWorkDetectorRulesEditingGui(bool hotKeyPressed)
		{
			if (!CanShowForm()) return;
			if (hotKeyPressed && ((ConfigManager.RuleRestrictions & RuleRestrictions.CannotCreateOrModifyRules) != 0)) return;
			DisplayWorkDetectorRulesEditingGuiImpl(hotKeyPressed);
		}

		public void DisplayLearnRuleFromCaptureGui(IWorkChangingRule matchingRule, DesktopCapture desktopCapture, DesktopWindow matchedWindow)
		{
			if (!CanShowForm()) return;
			string cancelKey;
			Rectangle parentRect;
			byte _;
			var rule = learningRuleBuilder.GetLearingRule(matchingRule, desktopCapture, matchedWindow, out cancelKey, out parentRect);
			if (rule == null || !rule.IsEnabled) return;
			if (cancelledCaptures.TryGetValue(cancelKey, out _)) return; //if cancelled
			var windowSize = LearnRuleFromCaptureGuiSize;
			var location = new Point(parentRect.X + parentRect.Width / 2 - windowSize.Width / 2, parentRect.Y + (parentRect.Height - windowSize.Height) / 3);
			var work = CaptureCoordinator.CurrentWorkController.CurrentWork;
			var workId = work == null ? new int?() : work.Id;
			log.Info("Displaying learning rule gui for: " + matchedWindow + " suggested rule: " + rule);
			DisplayLearnRuleFromCaptureGuiImpl(rule, desktopCapture, matchedWindow, workId, cancelKey, location);
		}

		public void DisplayWorkDetectorRuleDeletingGui()
		{
			if (!CanShowForm()) return;
			//if ((ConfigManager.RuleRestrictions & RuleRestrictions.CannotCreateOrModifyRules) != 0) return;	//CannotCreateOrModifyRules doesn't mean delete restriction
			DisplayWorkDetectorRuleDeletingGuiImpl();
		}

		protected void DelayNextDisplayLearnRuleFromCaptureGui(string cancelKey)
		{
			DelayNextDisplayLearnRuleFromCaptureGui(cancelKey, defaultExpirationTime);
		}

		protected void DelayNextDisplayLearnRuleFromCaptureGui(string cancelKey, TimeSpan expirationTime)
		{
			log.Info("Cancelled rule (" + expirationTime.TotalMinutes + "m): " + cancelKey.Replace(Environment.NewLine, " "));
			cancelledCaptures.Set(cancelKey, 1, expirationTime);
		}

		private void CurrentMenuChanged(object sender, MenuEventArgs e)
		{
			try
			{
				UpdateMenu(e.MenuLookup);
				var anyInvalid = CaptureCoordinator.GetUserRules().Any(rule => RuleManagementService.IsWorkAvailableFor(rule.RuleType) &&
																  !e.MenuLookup.WorkDataById.ContainsKey(rule.RelatedId));
				if (anyInvalid)
				{
					var msg = new MessageWithActions();
					msg.Append(Labels.NotificationInvalidRuleBody + " ");
					msg.Append(Labels.NotificationInvalidRuleAction, () => DisplayWorkDetectorRulesEditingGui(false));
					notificationService.ShowNotification(NotificationKeys.InvalidUserRules, nfInvalidRuleDuration, Labels.NotificationInvalidRuleTitle, msg);
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in UpdateMenu", ex);
			}
		}

		public void SetLearningRuleGenerators(IEnumerable<RuleGeneratorData> learningRuleGenerators)
		{
			learningRuleBuilder.SetGenerators(learningRuleGenerators);
		}

		public static string GetLongNameFor(WindowScopeType value)
		{
			switch (value)
			{
				case WindowScopeType.Active:
					return Labels.AutoRuleData_ActiveType;
				case WindowScopeType.VisibleOrActive:
					return Labels.AutoRuleData_VisibleOrActiveType;
				case WindowScopeType.Any:
					return Labels.AutoRuleData_AnyType;
				default:
					return Enum.GetName(typeof(WindowScopeType), value);
			}
		}

		public static string GetLongNameFor(WorkDetectorRuleType value)
		{
			switch (value)
			{
				case WorkDetectorRuleType.TempStartWork:
					return Labels.AutoRuleData_StartWorkType;
				case WorkDetectorRuleType.TempStopWork:
					return Labels.AutoRuleData_TempStopWork;
				case WorkDetectorRuleType.TempStartCategory:
					return Labels.AutoRuleData_StartCategoryType;
				case WorkDetectorRuleType.DoNothing:
					return Labels.AutoRuleData_DoNothing;
				case WorkDetectorRuleType.EndTempEffect:
					return Labels.AutoRuleData_EndTempEffect;
				case WorkDetectorRuleType.CreateNewRuleAndEndTempEffect:
					return Labels.AutoRuleData_CreateNewRuleAndEndTempEffect;
				case WorkDetectorRuleType.CreateNewRuleAndTempStartWork:
					return Labels.AutoRuleData_CreateNewRuleAndTempStartWork;
				default:
					return Enum.GetName(typeof(WorkDetectorRuleType), value);
			}
		}

		public static string GetShortNameFor(WorkDetectorRuleType value)
		{
			switch (value)
			{
				case WorkDetectorRuleType.TempStartWork:
					return Labels.AutoRules_TempStartWorkTypeShort;
				case WorkDetectorRuleType.TempStopWork:
					return Labels.AutoRules_TempStopWorkTypeShort;
				case WorkDetectorRuleType.TempStartCategory:
					return Labels.AutoRules_TempStartCategoryTypeShort;
				case WorkDetectorRuleType.DoNothing:
					return Labels.AutoRules_DoNothingTypeShort;
				case WorkDetectorRuleType.EndTempEffect:
					return Labels.AutoRules_EndTempEffectTypeShort;
				case WorkDetectorRuleType.CreateNewRuleAndEndTempEffect:
					return Labels.AutoRules_CreateNewRuleAndEndTempEffectTypeShort;
				case WorkDetectorRuleType.CreateNewRuleAndTempStartWork:
					return Labels.AutoRules_CreateNewRuleAndTempStartWorkTypeShort;
				default:
					return Enum.GetName(typeof(WorkDetectorRuleType), value);
			}
		}

		public static bool IsPermanentAvailableFor(WorkDetectorRuleType value)
		{
			return value == WorkDetectorRuleType.TempStartWork
				|| value == WorkDetectorRuleType.TempStartCategory
				|| value == WorkDetectorRuleType.CreateNewRuleAndTempStartWork
				|| value == WorkDetectorRuleType.CreateNewRuleAndEndTempEffect
				|| value == WorkDetectorRuleType.TempStartOrAssignWork
				|| value == WorkDetectorRuleType.TempStartOrAssignProject
				|| value == WorkDetectorRuleType.TempStartOrAssignProjectAndWork
				;
		}

		public static bool IsWorkAvailableFor(WorkDetectorRuleType value)
		{
			return value == WorkDetectorRuleType.TempStartWork
				|| value == WorkDetectorRuleType.CreateNewRuleAndTempStartWork;
		}

		public static bool IsCategoryAvailableFor(WorkDetectorRuleType value)
		{
			return value == WorkDetectorRuleType.TempStartCategory;
		}

		public static bool IsAdvancedViewNeededFor(WorkDetectorRule rule)
		{
			return rule.IsRegex
				|| rule.IsPermanent
				|| rule.WindowScope != WindowScopeType.Active
				|| (rule.RuleType != WorkDetectorRuleType.TempStartWork && rule.RuleType != WorkDetectorRuleType.TempStopWork);
		}

		public static bool IsLearning(WorkDetectorRuleType value)
		{
			return value == WorkDetectorRuleType.CreateNewRuleAndEndTempEffect
				|| value == WorkDetectorRuleType.CreateNewRuleAndTempStartWork;
		}

		private bool isDisposed;
		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			this.captureCoordinator.CurrentMenuChanged -= CurrentMenuChanged;
		}
	}
}
