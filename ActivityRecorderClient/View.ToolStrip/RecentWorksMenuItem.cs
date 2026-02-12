using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class RecentWorksMenuItem : ToolStripMenuItem
	{
		private static string RecentWorksPath { get { return "RecentWorks-" + ConfigManager.UserId; } }
		private ClientMenuLookup clientMenuLookup = new ClientMenuLookup();

		public RecentWorksMenuItem(string text)
			: base(text)
		{
			this.DropDownItems.Add(new ToolStripSeparator());
			var clear = new ToolStripMenuItem(Labels.Menu_DeleteRecentWorks);
			clear.Click += Clear_Click;
			this.DropDownItems.Add(clear);
			this.DropDown.KeyDown += HandleKeyDownForDropDown;
		}

		private void Clear_Click(object sender, EventArgs e)
		{
			while (this.DropDownItems.Count > 2)
			{
				this.DropDownItems.RemoveAtWithDispose(0);
			}
			
			RecentHelper.Clear();
		}

		public event EventHandler<WorkDataEventArgs> MenuClick;

		public event EventHandler<WorkDataEventArgs> MenuButtonClick;

		public void AddRecentWork(WorkData workData)
		{
			RecentHelper.AddRecent(workData);
			AddRecentToolstrip(workData);
		}

		private void HandleKeyDownForDropDown(object sender, KeyEventArgs e)
		{
			ClipboardHelper.HandleCtrlCKeyDownForDropDown(sender, e, clientMenuLookup);
		}

		private string GetWorkDataFullName(WorkData workData)
		{
			if (workData == null) return null;
			if (!workData.Id.HasValue) return workData.Name;
			var data = clientMenuLookup.GetWorkDataWithParentNames(workData.Id.Value);
			return data == null ? workData.Name : data.FullName;
		}

		private void AddRecentToolstrip(WorkData workData)
		{
			if (workData == null || !workData.Id.HasValue) return;
			var workDataInfo = this.DropDownItems //assume that workdata items come first
				.OfType<ToolStripMenuItemWithButton>()
				.Select(n => n.WorkData)
				.Select((val, idx) => new { Value = val, Idx = idx })
				.Where(n => n.Value.Id == workData.Id);

			var first = workDataInfo.FirstOrDefault();
			if (first == null) //add new item
			{
				var item = new ToolStripMenuItemWithButton(MenuBuilder.EscapeText(GetWorkDataFullName(workData))) { WorkData = workData, IsButtonVisible = true };
				item.MouseDown += MenuItemClick;
				item.Click += MenuItemClick;
				item.ButtonClick += MenuItemButtonClick;
				this.DropDownItems.Insert(0, item);
				TrimExcess();
			}
			else if (first.Idx != 0) //move recent item to the top
			{
				var item = this.DropDownItems[first.Idx];
				this.DropDownItems.RemoveAt(first.Idx);
				this.DropDownItems.Insert(0, item);
			}
		}

		public void TrimExcess()
		{
			while (this.DropDownItems.Count > ConfigManager.LocalSettingsForUser.MenuRecentItemsCount + 2)
			{
				this.DropDownItems.RemoveAtWithDispose(this.DropDownItems.Count - 3); //remove last item with workData
			}
		}

		private void MenuItemClick(object sender, EventArgs e)
		{
			var eArgs = new WorkDataEventArgs(((ToolStripMenuItemWithButton)sender).WorkData);
			OnMenuClick(eArgs);
		}

		private void OnMenuClick(WorkDataEventArgs e)
		{
			EventHandler<WorkDataEventArgs> click = MenuClick;
			if (click != null) click(this, e);
		}

		private void MenuItemButtonClick(object sender, EventArgs e)
		{
			var eArgs = new WorkDataEventArgs(((ToolStripMenuItemWithButton)sender).WorkData);
			OnMenuButtonClick(eArgs);
		}

		private void OnMenuButtonClick(WorkDataEventArgs e)
		{
			EventHandler<WorkDataEventArgs> click = MenuButtonClick;
			if (click != null) click(this, e);
		}

		public void UpdateMenu(ClientMenuLookup menuLookup)
		{
			clientMenuLookup = menuLookup;
			RemoveInvalidRecentWorks();
		}

		private void RemoveInvalidRecentWorks()
		{
			for (int i = 0; i < this.DropDownItems.Count; i++)
			{
				var item = this.DropDownItems[i] as ToolStripMenuItemWithButton;
				if (item == null) continue;
				var workData = item.WorkData;
				if (workData == null) continue;
				var workDataWithParent = workData.Id.HasValue ? clientMenuLookup.GetWorkDataWithParentNames(workData.Id.Value) : null;
				if (workDataWithParent == null || workDataWithParent.WorkData == null || !workDataWithParent.WorkData.IsVisibleInMenu)
				{
					this.DropDownItems[i].MouseDown -= MenuItemClick;
					this.DropDownItems[i].Click -= MenuItemClick;
					this.DropDownItems.RemoveAtWithDispose(i);
					i--;
				}
				else //update name if changed (and workData to be the most recent)
				{
					this.DropDownItems[i].Text = MenuBuilder.EscapeText(workDataWithParent.FullName);
					item.WorkData = workDataWithParent.WorkData;
				}
			}
		}

		public void LoadRecentWorks(ClientMenuLookup menuLookup)
		{
			clientMenuLookup = menuLookup;
			foreach (var itemToLoad in RecentHelper.GetRecents())
			{
				AddRecentToolstrip(itemToLoad.WorkData);
			}

			RemoveInvalidRecentWorks();
		}
	}
}
