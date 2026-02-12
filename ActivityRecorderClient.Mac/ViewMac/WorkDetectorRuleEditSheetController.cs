using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Rules;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public partial class WorkDetectorRuleEditSheetController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public WorkDetectorRuleEditSheetController(IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WorkDetectorRuleEditSheetController(NSCoder coder) : base (coder)
		{
			Initialize();
		}
		
		// Call to load from the XIB/NIB file
		public WorkDetectorRuleEditSheetController() : base ("WorkDetectorRuleEditSheet")
		{
			Initialize();
		}

		private TaggableNSMenuItem<WorkDetectorRuleType>[] ruleMenuItems;
		private TaggableNSMenuItem<WorkDataWithParentNames>[] currentWorks = new TaggableNSMenuItem<WorkDataWithParentNames>[0];

		// Shared initialization code
		void Initialize()
		{
			ruleMenuItems = new [] {
				GetMenuItemForType(WorkDetectorRuleType.TempStartWork),
				GetMenuItemForType(WorkDetectorRuleType.TempStopWork),
				GetMenuItemForType(WorkDetectorRuleType.DoNothing),
				GetMenuItemForType(WorkDetectorRuleType.EndTempEffect),
				GetMenuItemForType(WorkDetectorRuleType.CreateNewRuleAndEndTempEffect),
			};
		}

		private TaggableNSMenuItem<WorkDetectorRuleType> GetMenuItemForType(WorkDetectorRuleType ruleType)
		{
			return new TaggableNSMenuItem<WorkDetectorRuleType>(
				RuleManagementService.GetLongNameFor(ruleType),
				miSelectWorkType_Clicked
			) { TagObject = ruleType, Tag = (int)ruleType };
		}
		
		#endregion
		
		//strongly typed window accessor
		public new WorkDetectorRuleEditSheet Window
		{
			get
			{
				return (WorkDetectorRuleEditSheet)base.Window;
			}
		}

		private ClientMenuLookup menuLookup = new ClientMenuLookup();
		private WorkDetectorRule originalRule;
		private bool isOkPressed;
		private Action okAction;
		private Action cancelAction;
		private WorkDetectorRulesWindowController sender;

		public override void AwakeFromNib()
		{
			btnCancel.Title = Labels.Cancel;
			btnOk.Title = Labels.Ok;
			fcProcessRule.Title = Labels.AutoRules_HeaderProcessRuleLong + ":";
			fcTitleRule.Title = Labels.AutoRules_HeaderTitleRuleLong + ":";
			fcUrlRule.Title = Labels.AutoRules_HeaderUrlRuleLong + ":";
			lblRuleType.StringValue = Labels.AutoRules_HeaderRuleTypeLong + ":";
			lblWork.StringValue = Labels.Work + ":";
			cbIgnoreCase.Title = Labels.AutoRules_HeaderIgnoreCaseLong;
			cbIsEnabled.Title = Labels.AutoRules_HeaderIsEnabledLong;
			cbIsPermanent.Title = Labels.AutoRules_HeaderIsPermanentLong;
			cbIsRegex.Title = Labels.AutoRules_HeaderIsRegexLong;

			puRuleType.Menu.RemoveAllItems();
			foreach (var item in ruleMenuItems)
			{
				puRuleType.Menu.AddItem(item);
			}
		}

		//we don't handle if menu is change during this sheet is shown
		public void Edit(WorkDetectorRule rule, ClientMenu menu, WorkDetectorRulesWindowController sender, Action okAction)
		{
			if (sender == null)
				throw new ArgumentNullException("sender");
			Edit(rule, menu, sender, PointF.Empty, okAction, null);
		}

		public void Edit(WorkDetectorRule rule, ClientMenu menu, PointF topLeftPoint, Action okAction, Action cancelAction)
		{
			Edit(rule, menu, null, topLeftPoint, okAction, cancelAction);
		}

		private void Edit(WorkDetectorRule rule, ClientMenu menu, WorkDetectorRulesWindowController sender, PointF topLeftPoint, Action okAction, Action cancelAction)
		{
			if (sender != null)
			{
				this.Window.MakeKeyWindow();
			}
			else
			{
				//mismatch between carbon and cocoa coordinates
				var point = new PointF(topLeftPoint.X, NSScreen.MainScreen.Frame.Height - topLeftPoint.Y);
				this.Window.SetFrameTopLeftPoint(point);
				this.Window.MakeKeyAndOrderFront(this);
				NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
			}

			//set control values (no databinging here atm.)
			puWorks.Menu.RemoveAllItems();
			Array.ForEach(currentWorks, n => n.Dispose());
			currentWorks = MenuHelper.FlattenDistinctWorkDataThatHasId(menu)
				.Select(n => new TaggableNSMenuItem<WorkDataWithParentNames>(n.FullName + " (" + n.WorkData.Id.Value + ")",
					miSelectWork_Clicked) {TagObject = n, Tag = n.WorkData.Id.Value}
			)
				.ToArray(); //hold reference to these items
			foreach (var item in currentWorks)
			{
				puWorks.Menu.AddItem(item);
			}

			this.sender = sender;
			this.okAction = okAction;
			this.cancelAction = cancelAction;
			isOkPressed = false;
			originalRule = rule;
			menuLookup.ClientMenu = menu;
			fcProcessRule.StringValue = rule.ProcessRule ?? ""; //Convert.ToString won't work in mono and null would throw
			fcTitleRule.StringValue = rule.TitleRule ?? "";
			fcUrlRule.StringValue = rule.UrlRule ?? (rule.IsRegex ? ".*" : "*");
			puRuleType.SelectItemWithTag((int)rule.RuleType);
			cbIgnoreCase.State = rule.IgnoreCase ? NSCellStateValue.On : NSCellStateValue.Off;
			cbIsEnabled.State = rule.IsEnabled ? NSCellStateValue.On : NSCellStateValue.Off;
			cbIsPermanent.State = rule.IsPermanent ? NSCellStateValue.On : NSCellStateValue.Off;
			cbIsRegex.State = rule.IsRegex ? NSCellStateValue.On : NSCellStateValue.Off;

			var isInvalidWork = false;
			if (RuleManagementService.IsWorkAvailableFor(rule.RuleType))
			{
				if (!puWorks.SelectItemWithTag(rule.RelatedId))
				{
					isInvalidWork = true;
				}
			}
			UpdateGuiForWorkDetectorRuleType(rule.RuleType);

			if (sender != null)
			{
				NSApplication.SharedApplication.BeginSheet(this.Window, sender.Window, EditEnded);
			}
			else
			{
				this.ShowWindow(this);
			}

			if (isInvalidWork)
			{
				var alert = NSAlert.WithMessage(
						Labels.AutoRules_InvalidWorkTitle,
						Labels.Ok,
						"",
						"",
						Labels.AutoRules_InvalidWorkBody
				);
				alert.BeginSheet(this.Window);
			}
			//NSApplication.SharedApplication.RunModalForWindow(this.Window);
			// sheet is up here.....
			
			// when StopModal is called will continue here ....
			//NSApplication.SharedApplication.EndSheet(this.Window);
			//this.Window.OrderOut(this);		
		}

		private void EditEnded()
		{
//			this.Window.OrderOut(this);
//			if (okAction != null && isOkPressed)
//				okAction();
		}

		private void CloseWindow()
		{
			if (sender != null)
			{
				NSApplication.SharedApplication.EndSheet(this.Window);
				this.Window.OrderOut(this);
			}
			else
			{
				this.Window.PerformClose(this);
			}
			var action = isOkPressed ? okAction : cancelAction;
			if (action != null)
				action();
		}

		private WorkDetectorRule GetRuleFromGui()
		{
			WorkDetectorRule result = new WorkDetectorRule();
			SetRuleFromGui(result);
			return result;
		}

		private void SetRuleFromGui(WorkDetectorRule dstRule)
		{
			dstRule.RuleType = ((TaggableNSMenuItem<WorkDetectorRuleType>)puRuleType.SelectedItem).TagObject;
			dstRule.RelatedId = puWorks.Enabled ? puWorks.SelectedTag : -1;
			dstRule.Name = GetNameForRule(dstRule.RuleType, dstRule.RelatedId);
			dstRule.ProcessRule = fcProcessRule.StringValue;
			dstRule.TitleRule = fcTitleRule.StringValue;
			dstRule.UrlRule = fcUrlRule.StringValue;
			dstRule.IgnoreCase = cbIgnoreCase.State == NSCellStateValue.On;
			dstRule.IsEnabled = cbIsEnabled.State == NSCellStateValue.On;
			dstRule.IsPermanent = cbIsPermanent.State == NSCellStateValue.On;
			dstRule.IsRegex = cbIsRegex.State == NSCellStateValue.On;
		}

		private string GetNameForRule(WorkDetectorRuleType ruleType, int relatedId)
		{
			if (RuleManagementService.IsWorkAvailableFor(ruleType))
			{
				return menuLookup.WorkDataById[relatedId].FullName;
			}
			else if (RuleManagementService.IsCategoryAvailableFor(ruleType))
			{
				return menuLookup.AllCategoriesById[relatedId].Name;
			}
			else
			{
				return RuleManagementService.GetLongNameFor(ruleType);
			}
		}

		partial void OkClicked(NSObject sender) //validate and write back values
		{
			try
			{
				var item = GetRuleFromGui();
				var testRules = WorkChangingRuleFactory.CreateFrom(item, menuLookup)
				.Select(n => new RuleMatcher<IWorkChangingRule>(n))
				.ToArray();
				if (testRules.Length < 0)
					throw new ArgumentOutOfRangeException(); //make sure testRules var is populated (and not optimized out)
				//valid rule
				SetRuleFromGui(originalRule);
				isOkPressed = true;
				CloseWindow();
			}
			catch (Exception ex)
			{
				var alert = NSAlert.WithMessage(
					Labels.AutoRules_InvalidRuleTitle,
					Labels.Ok,
					"",
					"",
					ex.Message
				);
				alert.BeginSheet(this.Window);
			}
		}

		partial void CancelClicked(NSObject sender)
		{
			CloseWindow();
		}

		private void miSelectWorkType_Clicked(object sender, EventArgs e)
		{
			var ruleType = ((TaggableNSMenuItem<WorkDetectorRuleType>)sender).TagObject;
			UpdateGuiForWorkDetectorRuleType(ruleType);
		}

		private void UpdateGuiForWorkDetectorRuleType(WorkDetectorRuleType ruleType)
		{
			puWorks.Enabled = RuleManagementService.IsWorkAvailableFor(ruleType);
			puWorks.Hidden = !puWorks.Enabled;
			lblWork.Hidden = !puWorks.Enabled;
		}

		private void miSelectWork_Clicked(object sender, EventArgs e)
		{
			//var work = ((TaggableNSMenuItem<WorkDataWithParentNames>)sender).TagObject;
		}
	}
}

