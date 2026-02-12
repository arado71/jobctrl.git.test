using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Rules.Generation;
using Tct.ActivityRecorderClient.ViewMac;

namespace Tct.ActivityRecorderClient.Rules
{
	public class RuleManagementMacService : RuleManagementService
	{
		private readonly ControllerCollection ownedControllers;
		private bool isAutoRulesWindowVisible;

		public RuleManagementMacService(CaptureCoordinator captureCoordinator, ControllerCollection ownedControllers)
			: base(captureCoordinator)
		{
			this.ownedControllers = ownedControllers;
		}

		protected override void DisplayWorkDetectorRulesEditingGuiImpl(bool hotKeyPressed)
		{
			if (isAutoRulesWindowVisible)
				return;
			isAutoRulesWindowVisible = true;
			var ctrl = new WorkDetectorRulesWindowController(
				CaptureCoordinator.GetUserRules(),
				CaptureCoordinator.CurrentMenu,
				CaptureCoordinator.GetDesktopCapture()
			);
			ownedControllers.Add(ctrl, (_,__) => {
				isAutoRulesWindowVisible = false;
				if (!ctrl.ShouldSaveRules)
					return;
				CaptureCoordinator.SetUserRules(ctrl.Rules.ToList());
			}
			);
			ctrl.ShowWindow();
		}

		protected override void DisplayLearnRuleFromCaptureGuiImpl(WorkDetectorRule newRule, DesktopCapture desktopCapture, int? workId, string cancelKey, Point topLeftLocation)
		{
			if (isAutoRulesWindowVisible)
				return;
			isAutoRulesWindowVisible = true;
			var ctrl = new WorkDetectorRuleEditSheetController();
			ownedControllers.Add(ctrl, (_,__) => {
				isAutoRulesWindowVisible = false;
			}
			);
			newRule.RelatedId = workId.GetValueOrDefault();
			ctrl.Edit(newRule, CaptureCoordinator.CurrentMenu, topLeftLocation,
					() => {
				var currRules = CaptureCoordinator.GetUserRules();
				currRules.Add(newRule);
				CaptureCoordinator.SetUserRules(currRules); //there is a race here
			},
					() => {
				DelayNextDisplayLearnRuleFromCaptureGui(cancelKey);
			}
			);
		}

		protected override bool CanShowForm()
		{
			return !isAutoRulesWindowVisible;
		}

		private Size learnRuleFromCaptureGuiSize;

		protected override Size LearnRuleFromCaptureGuiSize
		{
			get
			{
				if (learnRuleFromCaptureGuiSize == Size.Empty)
				{
					using (var ctrl = new WorkDetectorRuleEditSheetController())
					{
						learnRuleFromCaptureGuiSize = new Size((int)ctrl.Window.Frame.Width, (int)ctrl.Window.Frame.Height);
					}
				}
				return learnRuleFromCaptureGuiSize;
			}
		}
	}
}

