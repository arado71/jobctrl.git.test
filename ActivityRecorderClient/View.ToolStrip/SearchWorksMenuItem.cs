using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Search;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class SearchWorksMenuItem : ToolStripControlHost
	{
		private const int maxItems = 30;
		private readonly StringMatcher matcher = new WorkNameMatcher();
		private readonly Timer delayTimer = new Timer();
		private ClientMenuLookup clientMenuLookup = new ClientMenuLookup();
		private ClientMenuLookup clientMenuLookupOwn = new ClientMenuLookup();

		public event EventHandler<WorkDataEventArgs> MenuClick;

		public ComboBoxEx ComboBox { get; private set; }

		public SearchWorksMenuItem()
			: base(new ComboBoxEx() { DrawMode = DrawMode.OwnerDrawFixed })
		{
			ComboBox = Control as ComboBoxEx;
			delayTimer.Interval = 300;
			delayTimer.Tick += DelayTimerTick;
			AutoSize = false;
			Width = 220;
			ComboBox.FormattingEnabled = true;
			ComboBox.Format += ComboBox_Format;
			ComboBox.CueBanner = Labels.SearchForWork;
			ComboBox.FlatStyle = FlatStyle.Standard;
			ComboBox.SelectionChangeCommitted += OnSelectionChangeCommitted;
			using (var mi = new ToolStripMenuItem())
			{
				ComboBox.Font = mi.Font; //use the default font of ToolStripMenuItem
			}
		}

		private void ComboBox_Format(object sender, ListControlConvertEventArgs e)
		{
			e.Value = WorkDataWithParentNames.DefaultSeparator + e.Value; //hax to prevent autocomplete if we start to type the begining of an item (XP doesn't like '\r')
		}

		private void DelayTimerTick(object sender, EventArgs e)
		{
			delayTimer.Stop();
			ClearItems();
			UpdateItems();
		}

		public void BringToFrontAll()
		{
			if (!ComboBox.DroppedDown) return;
			ComboBox.BringToFrontAll();
		}

		public void UpdateMenu(ClientMenuLookup menuLookup)
		{
			if (ConfigManager.LocalSettingsForUser.SearchOwnTasks && !ConfigManager.LocalSettingsForUser.SearchInClosed)
			{
				clientMenuLookup = menuLookup;
				UpdateMatcher(clientMenuLookup.WorkDataById.Where(kv => kv.Value.WorkData.IsVisibleInMenu).Select(kv => kv.Value));
			}
			clientMenuLookupOwn = menuLookup;
			ClearSelectedItem();
			ClearItems();
			UpdateItems();
		}

		public void UpdateAllWorks(List<WorkData> allWorkDatas)
		{
			clientMenuLookup = new ClientMenuLookup { ClientMenu = new ClientMenu { Works = allWorkDatas } };
			UpdateMatcher(clientMenuLookup.WorkDataById.Where(kv => !IsOwnWork(kv.Value) || GetOwnWork(kv.Value).WorkData.IsVisibleInMenu).Select(kv => kv.Value));
			ClearSelectedItem();
			ClearItems();
			UpdateItems();
		}

		private bool IsOwnWork(WorkDataWithParentNames work)
		{
			Debug.Assert(work != null && work.WorkData != null && work.WorkData.Id.HasValue);
			return clientMenuLookupOwn.WorkDataById.ContainsKey(work.WorkData.Id.Value);
		}

		private WorkDataWithParentNames GetOwnWork(WorkDataWithParentNames work)
		{
			Debug.Assert(work != null && work.WorkData != null && work.WorkData.Id.HasValue);
			return clientMenuLookupOwn.GetWorkDataWithParentNames(work.WorkData.Id.Value);
		}

		private void UpdateMatcher(IEnumerable<WorkDataWithParentNames> works)
		{
			matcher.Clear();
			foreach (var workWithParent in works)
			{
				matcher.Add(workWithParent.WorkData.Id.Value, workWithParent.FullName); //we cannot search for WorkDataWithParentNames.DefaultSeparator if it is in the name (but I can live with that)
			}
		}

		private void UpdateItems()
		{
			bool extendedItems = !ConfigManager.LocalSettingsForUser.SearchOwnTasks || ConfigManager.LocalSettingsForUser.SearchInClosed;
			foreach (var matchedId in matcher.GetMatches(Text).Take(maxItems))
			{
				var matched = clientMenuLookup.GetWorkDataWithParentNames(matchedId);
				if (matched != null)
				{
					Color itemColor = !extendedItems || IsOwnWork(matched) ? SystemColors.MenuText : SystemColors.InactiveCaptionText;
					ComboBox.Items.Add(new ComboBoxItemEx(matched, itemColor));
				}
			}
			if (ComboBox.Items.Count != 0)
			{
				ComboBox.SetComboScrollWidth();
				if (Owner != null && Owner.Visible) //timer can call this when the menu is closed
				{
					ComboBox.DroppedDown = true;
					//http://stackoverflow.com/questions/1093067/why-combobox-hides-cursor-when-droppeddown-is-set
					Cursor.Current = Cursors.Default;
				}
			}
		}

		private void ClearItems()
		{
			for (int i = 0; i < ComboBox.Items.Count; i++)
			{
				ComboBox.Items.RemoveAt(i--);
			}
		}

		private void ClearSelectedItem()
		{
			var t = ComboBox.Text;
			var s = ComboBox.SelectionStart;
			var l = ComboBox.SelectionLength;

			//if mouse is hovered over the dropdown of the combobox then selectedIndex is changed (without raising events)
			//in that case this would clear the textbox, in order to prevent that we save/restore the state of the textbox
			ComboBox.SelectedItem = null;

			ComboBox.Text = t;
			ComboBox.SelectionStart = s;
			ComboBox.SelectionLength = l;
		}

		private void WorkSelected()
		{
			var item = ComboBox.SelectedItem as ComboBoxItemEx;
			if (item == null) return;
			var work = item.Value as WorkDataWithParentNames;
			if (work == null || work.WorkData == null || !work.WorkData.Id.HasValue) return;
			OnMenuClick(new WorkDataEventArgs(work.WorkData, IsOwnWork(work)));
			ComboBox.Text = "";
			ComboBox.SelectedItem = null;
			ComboBox.DroppedDown = false;
			ClearItems();
			if (Owner != null) Owner.Hide();
		}

		protected void OnSelectionChangeCommitted(object sender, EventArgs e)
		{
			WorkSelected();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				if (ComboBox.SelectedItem == null)
				{
					if (ComboBox.Items.Count == 1 && ComboBox.DroppedDown) //if there is one element in the dropdown then select that
					{
						ComboBox.SelectedItem = ComboBox.Items[0];
					}
					else
					{
						e.Handled = true;
					}
				}
				//if the SelectedItem != null then enter will select it
			}
			else if (e.KeyData == (Keys.ShiftKey | Keys.Shift)
				|| e.KeyData == (Keys.Control | Keys.C)
				|| e.KeyData == (Keys.ControlKey | Keys.Control))
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
				ComboBox.DroppedDown = false;
				if (e.KeyData != Keys.Escape)
				{
					ClearItems();
				}
			}
			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyData == (Keys.Control | Keys.C) && this.ComboBox.DroppedDown && this.ComboBox.SelectedIndex != -1)
			{
				SetClipBoardDataIfApplicable();
				this.ComboBox.DroppedDown = false;
			}
			if (e.KeyData != Keys.Up
				&& e.KeyData != Keys.Down
				&& e.KeyData != Keys.Left
				&& e.KeyData != Keys.Right
				&& e.KeyData != Keys.Home
				&& e.KeyData != Keys.End
				&& e.KeyData != Keys.PageUp
				&& e.KeyData != Keys.PageDown
				&& e.KeyData != Keys.ShiftKey
				&& e.KeyData != (Keys.ShiftKey | Keys.LButton)
				&& e.KeyData != (Keys.Control | Keys.C)
				&& e.KeyData != Keys.Enter
				&& e.KeyData != Keys.Escape
				&& e.KeyData != Keys.PrintScreen
				)
			{
				delayTimer.Start();
			}
			base.OnKeyUp(e);
		}

		private void SetClipBoardDataIfApplicable()
		{
			var item = ComboBox.SelectedItem as ComboBoxItemEx;
			if (item == null) return;
			var work = item.Value as WorkDataWithParentNames;
			if (work == null) return;
			ClipboardHelper.SetClipboardData(work);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				delayTimer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void OnMenuClick(WorkDataEventArgs e)
		{
			EventHandler<WorkDataEventArgs> click = MenuClick;
			if (click != null) click(this, e);
		}
	}
}
