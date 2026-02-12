using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Search;

namespace Tct.ActivityRecorderClient.View
{
	//based heavily on SearchWorksMenuItem i.e. copy+paste
	public class WorkSelectorComboBox : ComboBoxEx
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int maxItems = 30;
		private readonly StringMatcher matcher = new WorkNameMatcher();
		private readonly Timer delayTimer = new Timer();
		private ClientMenuLookup clientMenuLookup = new ClientMenuLookup();

		private static readonly Func<WorkData, bool> defaultCanSelectWork = _ => true;
		private Func<WorkData, bool> canSelectWorkPredicate;
		private Timer imeRestoreTimer;
		private string imeRestoreText;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<WorkData, bool> CanSelectWork
		{
			get { return canSelectWorkPredicate ?? defaultCanSelectWork; }
			set
			{
				canSelectWorkPredicate = value;
				UpdateDropDown();
			}
		}

		private static readonly Func<ClientMenuLookup, IEnumerable<int>> defaultRecentWorkIdsSelector = x => MenuHelper.FlattenDistinctWorkDataThatHasId(x.ClientMenu).Select(y => y.WorkData.Id.Value);
		private Func<ClientMenuLookup, IEnumerable<int>> recentWorkIdsSelector;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<ClientMenuLookup, IEnumerable<int>> RecentWorkIdsSelector
		{
			get { return recentWorkIdsSelector ?? defaultRecentWorkIdsSelector; }
			set
			{
				recentWorkIdsSelector = value;
				recentWorkIds = recentWorkIdsSelector != null
					? recentWorkIdsSelector(clientMenuLookup).ToList()
					: defaultRecentWorkIdsSelector(clientMenuLookup).ToList();
				UpdateDropDown();
			}
		}

		private List<int> recentWorkIds = new List<int>();

		public WorkSelectorComboBox()
		{
			CueBanner = Labels.SearchForWork;
			MaxDropDownItems = maxItems;
			delayTimer.Interval = 300;
			delayTimer.Tick += DelayTimerTick;
			UpdateItems(DroppedDown);
			this.SelectionChangeCommitted += AddSelectedToRecent;
			this.DropDownClosed += AddSelectedToRecent; //SelectionChangeCommitted won't fire if focus is lost due to tab key
		}

		public void SetSelectedItem(WorkDataWithParentNames work)
		{
			if (work == null || work.WorkData == null || !work.WorkData.Id.HasValue) return;
			SetSelectedWorkId(work.WorkData.Id.Value);
		}

		public void SetSelectedWorkId(int workId)
		{
			var matched = clientMenuLookup.GetWorkDataWithParentNames(workId);
			if (matched != null && CanSelectWork(matched.WorkData))
			{
				var recentIdx = recentWorkIds.IndexOf(workId);
				var dropDownIdx = DropDownIndexOf(workId);
				if (recentIdx == -1 || recentIdx >= maxItems || dropDownIdx == -1)
				{
					recentWorkIds.Remove(workId);
					recentWorkIds.Insert(0, workId);
					Text = "";
					UpdateItems(DroppedDown);
					if (Items.Count > 0)
					{
						SelectedIndex = 0;
					}
					else
					{
						log.ErrorAndFail("Cannot select workid " + workId);
					}
				}
				else
				{
					SelectedIndex = dropDownIdx;
				}
				if (Focused) SelectAll();
			}
			else
			{
				SelectedIndex = -1;
			}
		}

		private int DropDownIndexOf(int workId)
		{
			var dropDownIdx = -1;
			for (int i = 0; i < Items.Count; i++)
			{
				var workData = Items[i] as WorkDataWithParentNames;
				Debug.Assert(workData == null || workData.WorkData.Id != null);
				if (workData != null && workData.WorkData.Id.Value == workId)
				{
					dropDownIdx = i;
					break;
				}
			}
			return dropDownIdx;
		}

		protected override void OnFormat(ListControlConvertEventArgs e)
		{
			base.OnFormat(e);
			e.Value = WorkDataWithParentNames.DefaultSeparator + e.Value; //hax to prevent autocomplete if we start to type the begining of an item (XP doesn't like '\r')
		}

		private void DelayTimerTick(object sender, EventArgs e)
		{
			delayTimer.Stop();
			UpdateItems(true);
		}

		public void UpdateMenu(ClientMenuLookup menuLookup)
		{
			clientMenuLookup = menuLookup;
			UpdateMatcher();
			recentWorkIds = RecentWorkIdsSelector(clientMenuLookup).ToList();
			UpdateDropDown();
		}

		private void UpdateMatcher()
		{
			matcher.Clear();
			foreach (var workWithParent in clientMenuLookup.WorkDataById)
			{
				matcher.Add(workWithParent.Key, workWithParent.Value.FullName); //we cannot search for WorkDataWithParentNames.DefaultSeparator if it is in the name (but I can live with that)
			}
		}

		private void UpdateDropDown()
		{
			var selectedWork = SelectedItem as WorkDataWithParentNames;
			ClearSelectedItem();
			UpdateItems(DroppedDown);
			Debug.Assert(selectedWork == null || selectedWork.WorkData.Id != null);
			if (selectedWork != null) SetSelectedWorkId(selectedWork.WorkData.Id.Value);
		}

		private void UpdateItems(bool showDropDown)
		{
			ClearItems();
			UpdateItems(Text != "" ? matcher.GetMatches(Text) : recentWorkIds);

			if (Items.Count != 0)
			{
				//if (Owner != null && Owner.Visible) //timer can call this when the menu is closed
				//{
				if (showDropDown)
				{
					DroppedDown = true;
					//http://stackoverflow.com/questions/1093067/why-combobox-hides-cursor-when-droppeddown-is-set
					Cursor.Current = Cursors.Default;
				}
				//}
			}
		}

		private void UpdateItems(IEnumerable<int> workIds)
		{
			foreach (var matched in workIds
				.Select(n => clientMenuLookup.GetWorkDataWithParentNames(n))
				.Where(n => n != null && CanSelectWork(n.WorkData))
				.Take(maxItems))
			{
				Items.Add(matched);
			}
			if (Items.Count != 0)
			{
				this.SetComboScrollWidth();
			}
		}

		private void ClearItems()
		{
			for (int i = 0; i < Items.Count; i++)
			{
				Items.RemoveAt(i--);
			}
		}

		private void ClearSelectedItem()
		{
			var lang = InputLanguage.CurrentInputLanguage.Culture.TwoLetterISOLanguageName;
			bool do_not_restore_text = lang == "ko" || lang == "jp" || lang == "zh";
			string t = Text;
			int s = SelectionStart, l = SelectionLength;
			//if mouse is hovered over the dropdown of the combobox then selectedIndex is changed (without raising events)
			//in that case this would clear the textbox, in order to prevent that we save/restore the state of the textbox
			SelectedItem = null;
			if (do_not_restore_text)
				return;
			Text = t;
			SelectionStart = s;
			SelectionLength = l;
		}

		private void AddSelectedToRecent(object sender, EventArgs e)
		{
			var workData = SelectedItem as WorkDataWithParentNames;
			if (workData == null || !workData.WorkData.Id.HasValue) return;
			var workId = workData.WorkData.Id.Value;
			var selIdx = recentWorkIds.IndexOf(workId);
			if (selIdx == -1)
			{
				recentWorkIds.Insert(0, workId);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				if (SelectedItem == null)
				{
					if (Items.Count == 1 && DroppedDown) //if there is one element in the dropdown then select that
					{
						SelectedItem = Items[0];
					}
					else
					{
						e.Handled = true;
					}
				}
			}
			else if (e.KeyData == (Keys.Alt | Keys.F4))
			{
				//allow alt+f4 to close form
			}
			else if (e.KeyData == (Keys.ShiftKey | Keys.Shift)
				|| e.KeyData == (Keys.Control | Keys.C)
				|| e.KeyData == (Keys.ControlKey | Keys.Control)
				|| e.KeyValue == 17 || e.KeyValue == 18 //Alts
				|| e.KeyValue == 92 //Win key
				|| (e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F24)
				|| (e.Modifiers & (Keys.Alt | Keys.Control)) != 0
				)
			{
				e.Handled = true;
			}
			else if (e.KeyData != Keys.Up
				&& e.KeyData != Keys.Down
				&& e.KeyData != Keys.Left
				&& e.KeyData != Keys.Right
				&& e.KeyData != Keys.Home
				&& e.KeyData != Keys.End
				&& e.KeyData != Keys.PageUp
				&& e.KeyData != Keys.PageDown
				)
			{
				ClearSelectedItem();
				delayTimer.Stop();
				DroppedDown = false;
				if (e.KeyData != Keys.Escape)
				{
					ClearItems();
				}
			}
			else if (e.KeyData == Keys.Down || e.KeyData == Keys.Up)
			{
				DroppedDown = true;
			}
			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyData == (Keys.Control | Keys.C) && DroppedDown && SelectedIndex != -1)
			{
				SetClipBoardDataIfApplicable();
				DroppedDown = false;
			}
			if (!DroppedDown && SelectedItem == null)
			{
				delayTimer.Start();
			}

			base.OnKeyUp(e);
		}

		private void SetClipBoardDataIfApplicable()
		{
			var work = SelectedItem as WorkDataWithParentNames;
			if (work == null) return;
			ClipboardHelper.SetClipboardData(work);
		}

		protected override void OnClick(EventArgs e)
		{
			if (!DroppedDown) DroppedDown = true; //open dropdown if the textbox is clicked
			base.OnClick(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				delayTimer.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnLeave(EventArgs e)
		{
			var lang = InputLanguage.CurrentInputLanguage.Culture.TwoLetterISOLanguageName;
			if (lang == "ko" || lang == "jp" || lang == "zh")
			{
				imeRestoreText = Text;
				if (imeRestoreTimer == null)
				{
					imeRestoreTimer = new Timer() { Interval = 50 };
					imeRestoreTimer.Tick += ImeRestoreTimer_Tick;
				}

				imeRestoreTimer.Start();
			}
			base.OnLeave(e);
		}

		private void ImeRestoreTimer_Tick(object sender, EventArgs e)
		{
			imeRestoreTimer.Stop();
			Text = imeRestoreText;
		}
	}
}
