using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.ViewMac
{
	//todo display error info in grid for rules that became invalid due to menu change
	public partial class WorkDetectorRulesWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public WorkDetectorRulesWindowController(IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WorkDetectorRulesWindowController(NSCoder coder) : base (coder)
		{
			Initialize();
		}
		
		// Call to load from the XIB/NIB file
		public WorkDetectorRulesWindowController() : base ("WorkDetectorRulesWindow")
		{
			Initialize();
		}
		
		// Shared initialization code
		void Initialize()
		{
		}

		#endregion
		
		//strongly typed window accessor
		public new WorkDetectorRulesWindow Window
		{
			get
			{
				return (WorkDetectorRulesWindow)base.Window;
			}
		}

		private readonly WorkDetectorRuleEditSheetController editCtrl = new WorkDetectorRuleEditSheetController();
		private readonly ClientMenu menu;
		private readonly WorkDetectorRulesDataSource dataSource;
		private readonly DesktopCapture lastCapture;
		private NSTableViewObserver observer;

		public WorkDetectorRule[] Rules
		{
			get
			{ 
				var datasource = tblRules.DataSource as WorkDetectorRulesDataSource;
				return datasource == null ? null : datasource.Rules.Select(n => n.Clone()).ToArray();
			}
		}

		public bool ShouldSaveRules { get; private set; }

		public WorkDetectorRulesWindowController(IEnumerable<WorkDetectorRule> rules, ClientMenu menu, DesktopCapture capture)
			: this()
		{
			this.menu = menu;
			dataSource = new WorkDetectorRulesDataSource(rules);
			lastCapture = capture;
		}

		public void ShowWindow()
		{
			this.Window.Center();
			this.Window.MakeKeyAndOrderFront(this);
			NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
		}

		public override void AwakeFromNib()
		{
			if (dataSource != null)
				tblRules.DataSource = dataSource;
			tblRules.DoubleClick += HandleDoubleClick;
			//tblRules.SelectionDidChange += HandleSelectionDidChange; //rows would be invisible if used, without assigning the tblRules.Delegate... (don't ask me why) so we use an observer instead
			observer = new NSTableViewObserver(tblRules);
			observer.SelectionChanged += HandleSelectionDidChange;
			tblRules.Delegate = observer;
			HandleSelectionDidChange(observer, EventArgs.Empty); //buttons' state initialization 
			this.Window.Title = Labels.AutoRules_Title;
			var cols = tblRules.TableColumns(); 
			cols[0].HeaderCell.Title = Labels.AutoRules_HeaderRuleType;
			cols[1].HeaderCell.Title = Labels.AutoRules_HeaderRelatedId;
			cols[2].HeaderCell.Title = Labels.AutoRules_HeaderName;
			cols[3].HeaderCell.Title = Labels.AutoRules_HeaderTitleRule;
			cols[4].HeaderCell.Title = Labels.AutoRules_HeaderProcessRule;
			cols[5].HeaderCell.Title = Labels.AutoRules_HeaderUrlRule;
			cols[6].HeaderCell.Title = Labels.AutoRules_HeaderIsPermanent;
			cols[7].HeaderCell.Title = Labels.AutoRules_HeaderIsRegex;
			cols[8].HeaderCell.Title = Labels.AutoRules_HeaderIgnoreCase;
			cols[9].HeaderCell.Title = Labels.AutoRules_HeaderIsEnabled;
			tblRules.HeaderView.NeedsDisplay = true;
			btnCancel.Title = Labels.Cancel;
			btnOk.Title = Labels.Ok;
		}

		private void HandleSelectionDidChange(object sender, EventArgs e)
		{
			if (tblRules.DataSource == null)
				return;
			var idx = tblRules.SelectedRow;
			var rules = ((WorkDetectorRulesDataSource)tblRules.DataSource).Rules;
			var selectedItem = (idx >= rules.Count || idx < 0) ? null : rules[idx];
			btnRemove.Enabled = selectedItem != null;
			var canMoveUp = selectedItem != null
				&& idx > 0
				&& (!IsLearningRule(selectedItem) || IsLearningRule(rules[idx - 1]));
			var canMoveDown = selectedItem != null
				&& idx < rules.Count - 1
				&& (IsLearningRule(selectedItem) || !IsLearningRule(rules[idx + 1]));
			btnUp.Enabled = canMoveUp;
			btnDown.Enabled = canMoveDown;
		}

		private static bool IsLearningRule(WorkDetectorRule rule) //learning rules should be after all other rules
		{
			return Tct.ActivityRecorderClient.Rules.RuleManagementService.IsLearning(rule.RuleType);
		}

		private void HandleDoubleClick(object sender, EventArgs e)
		{
			var row = tblRules.ClickedRow;
			var rules = ((WorkDetectorRulesDataSource)tblRules.DataSource).Rules;
			if (row >= rules.Count || row < 0)
				return;
			Edit(rules[row]);
		}

		private void Edit(WorkDetectorRule rule, bool isNew = false)
		{
			if (rule == null)
				return;
			editCtrl.Edit(rule, menu, this, () =>
			{
				if (isNew)
				{
					var rules = ((WorkDetectorRulesDataSource)tblRules.DataSource).Rules;
					int insertAt;
					if (IsLearningRule(rule)) //Add to the end if its a learning rule
					{
						insertAt = rules.Count;
					}
					else //if it's not a learning rule then
					{
						insertAt = 0;
						for (int i = 0; i < rules.Count; i++)
						{
							if (IsLearningRule(rules[i]))
								break; //insert after the last non-learning rule
							insertAt = i + 1;
						}
					}
					rules.Insert(insertAt, rule);
					tblRules.ReloadData();
					tblRules.SelectRow(insertAt, false); //select the newly inserted rule
				}
				else
				{
					tblRules.ReloadData();
				}
			}
			);
		}

		partial void AddClicked(NSObject sender)
		{
			var work = MenuHelper.FlattenDistinctWorkDataThatHasId(menu).FirstOrDefault();
			if (work == null)
			{
				var alert = NSAlert.WithMessage(
						Labels.AutoRules_NotificationNoWorkErrorTitle,
						Labels.Ok,
						"",
						"",
						Labels.AutoRules_NotificationNoWorkErrorBody
				);
				alert.BeginSheet(this.Window);
				return;
			}
			var aw = lastCapture == null ? null : lastCapture.GetActiveWindow();
			Edit(
				new WorkDetectorRule() 
				{ 
					RuleType = WorkDetectorRuleType.TempStartWork,
					RelatedId = work.WorkData.Id.Value,
					ProcessRule = aw == null || aw.ProcessName == null ? Labels.AutoRules_ExampleExe.Replace(".exe",".app") : aw.ProcessName, 
					TitleRule =  aw == null || aw.Title == null ? "*" : aw.Title, 
					UrlRule =  aw == null || aw.Url == null ? "*" : aw.Url,
					IsEnabled = true, 
					IgnoreCase = true, 
				},
				true
			);
		}

		partial void RemoveClicked(NSObject sender)
		{
			var row = tblRules.SelectedRow;
			var rules = ((WorkDetectorRulesDataSource)tblRules.DataSource).Rules;
			if (row >= rules.Count || row < 0)
				return;
			rules.RemoveAt(row);
			tblRules.ReloadData();
			tblRules.SelectRow(row - 1, false);
		}

		partial void UpClicked(NSObject sender)
		{
			var row = tblRules.SelectedRow;
			var rules = ((WorkDetectorRulesDataSource)tblRules.DataSource).Rules;
			if (row >= rules.Count || row < 1)
				return;
			var item = rules[row];
			rules.RemoveAt(row);
			rules.Insert(row - 1, item);
			tblRules.ReloadData();
			tblRules.SelectRow(row - 1, false);
		}

		partial void DownClicked(NSObject sender)
		{
			var row = tblRules.SelectedRow;
			var rules = ((WorkDetectorRulesDataSource)tblRules.DataSource).Rules;
			if (row >= rules.Count - 1 || row < 0)
				return;
			var item = rules[row];
			rules.RemoveAt(row);
			rules.Insert(row + 1, item);
			tblRules.ReloadData();
			tblRules.SelectRow(row + 1, false);
		}

		partial void OkClicked(NSObject sender)
		{
			ShouldSaveRules = true;
			this.Window.PerformClose(this);
		}

		partial void CancelClicked(NSObject sender)
		{
			this.Window.PerformClose(this);
		}

		private class NSTableViewObserver : MonoMac.AppKit.NSTableViewDelegate
		{
			private NSTableView table;

			public event EventHandler SelectionChanged;

			public NSTableViewObserver(NSTableView tbl)
			{
				table = tbl;
			}
			
			public override void SelectionDidChange(NSNotification notification)
			{
				var del = SelectionChanged;
				if (del != null)
					del(table, EventArgs.Empty);
			}
		}

	}
}

