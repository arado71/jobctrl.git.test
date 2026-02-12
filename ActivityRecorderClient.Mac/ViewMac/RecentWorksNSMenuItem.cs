using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.AppKit;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public class RecentWorksNSMenuItem : NSMenuItem
	{
		private static string RecentWorksPath { get { return "RecentWorks-" + ConfigManager.UserId; } }

		private const int maxRecentWorks = 10;
		private readonly ClientMenuLookup clientMenuLookup = new ClientMenuLookup();
		private readonly NSMenuItem miClear;
		private readonly List<NSMenuItem> recentMenuItems = new List<NSMenuItem>(); //we need references for these in MonoMac to avoid GC

		public RecentWorksNSMenuItem(string title)
			: base(title)
		{
			this.Submenu = new NSMenu();
			this.Submenu.AddItem(NSMenuItem.SeparatorItem);
			miClear = new NSMenuItem(Labels.Menu_DeleteRecentWorks, Clear_Click);
			this.Submenu.AddItem(miClear);
		}

		private void Clear_Click(object sender, EventArgs e)
		{
			RemoveInvalidRecentWorks(null);
		}

		public event EventHandler<WorkDataEventArgs> MenuClick;

		public void AddRecentWork(WorkData workData)
		{
			AddRecentWork(workData, true);
		}

		private string GetWorkDataFullName(WorkData workData)
		{
			if (workData == null)
				return null;
			if (!workData.Id.HasValue)
				return workData.Name;
			var data = clientMenuLookup.GetWorkDataWithParentNames(workData.Id.Value);
			return data == null ? workData.Name : data.FullName;
		}

		private void AddRecentWork(WorkData workData, bool saveChanges)
		{
			if (workData == null || !workData.Id.HasValue)
				return;
			var workDataInfo = this.Submenu //assume that workdata items come first
				.ItemArray()
				.OfType<NSMenuItemWithWorkData>()
				.Select(n => n.WorkData)
				.OfType<WorkData>()
				.Select((val, idx) => new { Value = val, Idx = idx })
				.Where(n => n.Value.Id == workData.Id);

			var first = workDataInfo.FirstOrDefault();
			if (first == null) //add new item
			{
				var item = new NSMenuItemWithWorkData(GetWorkDataFullName(workData)) { WorkData = workData };
				item.Click += (sender, e) => OnMenuClick(e);
				this.Submenu.InsertItematIndex(item, 0);
				recentMenuItems.Add(item); //hold ref
				while (this.Submenu.Count > maxRecentWorks)
				{
					recentMenuItems.Remove(this.Submenu.ItemAt(this.Submenu.Count - 1)); //release ref
					this.Submenu.RemoveItemAt(this.Submenu.Count - 1);
				}
			}
			else if (first.Idx != 0) //move recent item to the top
			{
				var item = this.Submenu.ItemAt(first.Idx);
				this.Submenu.RemoveItemAt(first.Idx);
				this.Submenu.InsertItematIndex(item, 0);
			}
			if (saveChanges)
				SaveRecentWorks();
		}

		private void OnMenuClick(WorkDataEventArgs e)
		{
			EventHandler<WorkDataEventArgs> click = MenuClick;
			if (click != null)
				click(this, e);
		}

		public void UpdateMenu(ClientMenu clientMenu)
		{
			clientMenuLookup.ClientMenu = clientMenu;
			RemoveInvalidRecentWorks(clientMenu);
		}

		private void RemoveInvalidRecentWorks(ClientMenu clientMenu)
		{
			for (int i = 0; i < this.Submenu.Count; i++)
			{
				var item = this.Submenu.ItemAt(i) as NSMenuItemWithWorkData;
				if (item == null)
					continue;
				var workData = item.WorkData;
				if (workData == null)
					continue;
				if (workData.Id.HasValue && !MenuHelper.MenuContainsId(clientMenu, workData.Id.Value))
				{
					recentMenuItems.Remove(item); //release ref
					//item.Click -= MenuItemClick; //no need to unregister
					this.Submenu.RemoveItemAt(i);
					i--;
				}
				else //update name if changed
				{
					item.Title = GetWorkDataFullName(workData);
					//updating the tag is not necessary atm., but make it consistent
					if (workData.Id.HasValue)
					{
						var workDataWithParent = clientMenuLookup.GetWorkDataWithParentNames(workData.Id.Value);
						if (workDataWithParent != null && workDataWithParent.WorkData != null)
						{
							item.WorkData = workDataWithParent.WorkData;
						}
					}
				}
			}
			SaveRecentWorks();
		}

		public void LoadRecentWorks(ClientMenu clientMenu)
		{
			clientMenuLookup.ClientMenu = clientMenu;
			if (!IsolatedStorageSerializationHelper.Exists(RecentWorksPath))
				return;
			List<WorkData> itemsLoaded;
			if (IsolatedStorageSerializationHelper.Load(RecentWorksPath, out itemsLoaded) && itemsLoaded != null)
			{
				itemsLoaded.Reverse();
				foreach (var itemToLoad in itemsLoaded)
				{
					AddRecentWork(itemToLoad, false);
				}
			}
			RemoveInvalidRecentWorks(clientMenu);
		}

		private void SaveRecentWorks()
		{
			List<WorkData> itemsToSave = this.Submenu
				.ItemArray()
				.OfType<NSMenuItemWithWorkData>()
				.Select(n => n.WorkData)
				.OfType<WorkData>()
				.Select(n => new WorkData() { Id = n.Id, Name = n.Name }) //copy without children to reduce size
				.ToList();
			IsolatedStorageSerializationHelper.Save(RecentWorksPath, itemsToSave);
		}

	}
}

