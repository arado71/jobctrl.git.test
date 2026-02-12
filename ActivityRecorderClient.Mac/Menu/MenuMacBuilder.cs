using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using log4net;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.ViewMac;

namespace Tct.ActivityRecorderClient.Menu
{
	public class MenuMacBuilder
	{
		public event EventHandler<WorkDataEventArgs> MenuClick;

		public readonly NSMenuItem PlaceHolder = new NSMenuItem("-");
		private readonly int builderId;
		private readonly NSMenu nsMenu;
		private readonly List<NSMenuItem> currentMenuItems = new List<NSMenuItem>(); //we need references for these in MonoMac to avoid GC
		private readonly NSMenuItem topPriority = new NSMenuItem(Labels.Menu_TopPriorityWorks) { Submenu = new NSMenu() };
		private readonly NSMenuItem topEndDate = new NSMenuItem(Labels.Menu_TopEndDateWorks) { Submenu = new NSMenu() };
		private readonly NSMenuItem topTargetWorkTime = new NSMenuItem(Labels.Menu_TopTargetWorkTimeWorks) { Submenu = new NSMenu() };
		private readonly Dictionary<NSMenuItem,List<NSMenuItem>> topMenuItems = new Dictionary<NSMenuItem, List<NSMenuItem>>(); //we need references for these in MonoMac to avoid GC

		private TotalWorkTimeStats TotalWorkTimeStats { get; set; }

		public ClientMenu ClientMenu { get; private set; }

		public MenuMacBuilder(NSMenu menuToUse)
		{
			if (menuToUse == null)
				throw new ArgumentNullException();
			builderId = this.GetHashCode();
			nsMenu = menuToUse;
			topMenuItems.Add(topPriority, new List<NSMenuItem>());
			topMenuItems.Add(topEndDate, new List<NSMenuItem>());
			topMenuItems.Add(topTargetWorkTime, new List<NSMenuItem>());
		}

		public void UpdateTargetEndDatePercentages()
		{
			RefreshTopTargetEndDateItems(ClientMenu, false);
		}

		public void UpdateTargetTotalWorkTimePercentages(TotalWorkTimeStats totalStats)
		{
			TotalWorkTimeStats = totalStats;
			RefreshTopTargetWorkTimeItems(ClientMenu, TotalWorkTimeStats, false);
		}

		public void UpdateMenu(ClientMenu menu)
		{
			if (menu == null)
				return;
			ClientMenu = menu;
			int firstIdx = int.MaxValue;
			int i = nsMenu.Count - 1;
			while (i >= 0)
			{
				var menuItem = nsMenu.ItemAt(i);
				if (menuItem.Tag == builderId)
				{
					firstIdx = i;
					nsMenu.RemoveItemAt(i);
				}
				i--;
			}
			if (firstIdx == int.MaxValue)
			{
				firstIdx = nsMenu.IndexOf(PlaceHolder);
				if (firstIdx > -1)
				{
					nsMenu.RemoveItemAt(firstIdx);
				}
				else
				{
					firstIdx = 0;
				}
			}

			currentMenuItems.Clear();
			var newMenu = GetDynamicMenu(menu, currentMenuItems);

			FlattenMenu(newMenu, ConfigManager.LocalSettingsForUser.MenuFlattenFactor);

			AddTopMenuItemsIfFirstTime(firstIdx);
			RefreshTopPriorityItems(ClientMenu, true);
			RefreshTopTargetEndDateItems(ClientMenu, true);
			RefreshTopTargetWorkTimeItems(ClientMenu, TotalWorkTimeStats, true);

			for (int j = newMenu.Count - 1; j >= 0; j--)
			{
				nsMenu.InsertItematIndex(newMenu[j], firstIdx);
			}
			if (newMenu.Count == 0)
			{
				nsMenu.InsertItematIndex(PlaceHolder, firstIdx);
			}
		}

		private List<NSMenuItem> GetDynamicMenu(ClientMenu tree, List<NSMenuItem> created)
		{
			var result = new List<NSMenuItem>();
			if (tree.Works == null)
				return result;
			foreach (WorkData itemData in tree.Works)
			{
				var menuItems = BuildTree(itemData, new List<WorkData>(), created);
				if (menuItems != null)
				{
					result.Add(menuItems);
					created.Add(menuItems);
				}
			}
			return result;
		}

		private NSMenuItem BuildTree(WorkData tree, List<WorkData> visitedElements, List<NSMenuItem> created)
		{
			if (tree == null)
				return null;
			//if (visitedElements.Contains(tree)) return null; //equlas might be overriden later
			foreach (var visitedElement in visitedElements) //avoid infinite loop
			{
				if (ReferenceEquals(tree, visitedElement))
					return null;
			}
			visitedElements.Add(tree);
			NSMenuItem menuItem;
			if (string.IsNullOrEmpty(tree.Name))
			{
				//hax but not used anyway...
				menuItem = new NSMenuItem("----" + tree.Name) { Tag = builderId };
				//return NSMenuItem.SeparatorItem //cannot set { Tag = builderId };
				created.Add(menuItem);
				return menuItem;
			}
			if (tree.Id.HasValue)
			{
				menuItem = new NSMenuItem(tree.Name, (_, __) => OnMenuClick(new WorkDataEventArgs(tree))) { Tag = builderId };
			}
			else
			{
				menuItem = new NSMenuItem(tree.Name) { Tag = builderId };
			}
			created.Add(menuItem); //hold reference to the menuItem
			if (tree.Children == null)
				return menuItem;
			foreach (var item in tree.Children)
			{
				var childItem = BuildTree(item, visitedElements, created);
				if (childItem != null)
				{
					if (menuItem.Submenu == null)
					{
						menuItem.Submenu = new NSMenu();
					}
					menuItem.Submenu.AddItem(childItem);
				}
			}
			return menuItem;
		}

		private void OnMenuClick(WorkDataEventArgs e)
		{
			EventHandler<WorkDataEventArgs> click = MenuClick;
			if (click != null)
				click(this, e);
		}

		private void FlattenMenu(List<NSMenuItem> menu, int factor)
		{
			using (var fakeParent = new NSMenuItem()) //create a fake parent to store the items in menu
			{
				fakeParent.Submenu = new NSMenu();
				foreach (var item in menu)
				{
					fakeParent.Submenu.AddItem(item);
				}

				foreach (var item in menu)
				{
					if (item.IsSeparatorItem)
						continue;
					FlattenItemAtIdx(fakeParent, item, fakeParent.Submenu.IndexOf(item), factor);
				}

				menu.Clear();
				menu.AddRange(fakeParent.Submenu.ItemArray());
				fakeParent.Submenu.RemoveAllItems(); //clear references from items to fakeParent
			}
		}

		private int FlattenItemAtIdx(NSMenuItem parent, NSMenuItem child, int childIdxInParent, int factor)
		{
			int inserted = 0;
			//first flatten child (if it has any DropDownItems)
			if (child.Submenu == null)
				return inserted;
			for (int i = 0; i < child.Submenu.Count; i++)
			{
				var grandChild = child.Submenu.ItemAt(i);
				if (grandChild == null)
					continue;
				var newlyInseted = FlattenItemAtIdx(child, grandChild, i, factor);
				i += newlyInseted; //skip newly inserted items (they are already flattened) but we won't add it to 'inserted' because its only for one level
			}

			var grandChildCount = child.Submenu.Count;
			if (grandChildCount > factor || grandChildCount == 0)
				return inserted; //we cannot flatten anymore (if it has only spearators then we will leave them as is [YAGNI])
			//we can flatten all DropDownItems
			for (int i = child.Submenu.Count - 1; i >= 0; i--)
			{
				var grandChild = child.Submenu.ItemAt(i);
				child.Submenu.RemoveItemAt(i); //remove from original position. Insert below would also do the remove (which is kinda confusing), so to make this more readable we first remove then we insert if necessary
				if (grandChild == null)
					continue;
				//bring grandChild one level up (we don't care about separators)
				grandChild.Title = child.Title + WorkDataWithParentNames.DefaultSeparator + grandChild.Title;
				parent.Submenu.InsertItematIndex(grandChild, childIdxInParent + 1); //we go backwards so the order is the same (inserting here would also remove from child's DropDownItems if it still had an owner).
				inserted++;
			}
			Debug.Assert(child.Submenu.Count == 0);
			child.Submenu.Dispose();
			//child.Submenu = null; //cannot set it to null
			//remove child if not clickable (it shouldn't be clickable so this is kinda YAGNI)
			//if (!clickHandlers.ContainsKey(child))
			//{
			parent.Submenu.RemoveItemAt(childIdxInParent);
			inserted--;
			//}
			return inserted;
		}

		private bool firstTime = true;

		private void AddTopMenuItemsIfFirstTime(int firstIdx)
		{
			if (!firstTime)
				return;
			var placeIdx = firstIdx - 1;
			if (placeIdx <= -1)
				return;
			nsMenu.InsertItematIndex(NSMenuItem.SeparatorItem, ++placeIdx);
			nsMenu.InsertItematIndex(topPriority, ++placeIdx);
			nsMenu.InsertItematIndex(topEndDate, ++placeIdx);
			nsMenu.InsertItematIndex(topTargetWorkTime, ++placeIdx);
			firstTime = false;
		}

		private void RefreshTopPriorityItems(ClientMenu menu, bool refreshNames)
		{
			var topItems = MenuBuilderHelper.GetTopPriorityWorkData(menu).ToList();
			var maxPriority = topItems.Select(n => n.WorkData.Priority).FirstOrDefault();
			if (!maxPriority.HasValue || maxPriority.Value <= 0 || topItems.Count == 0)
			{
				topPriority.Submenu.RemoveAllItems(); //no need to unregister click handlers
				topPriority.Enabled = false;
				topMenuItems[topPriority].Clear(); //clear references
				return;
			}
			RefreshTopItems(topItems, topPriority, (menuItem, workDataWithNames) =>
			{
				if (refreshNames)
					menuItem.Title = workDataWithNames.FullName;
				menuItem.WorkData = workDataWithNames.WorkData;
				var bValue = menuItem.WorkData.Priority / (float)maxPriority;
				var bText = menuItem.WorkData.Priority.ToString();
				menuItem.SetBarValueAndText(bValue, bText);
				menuItem.ToolTip = NSMenuItemWithWorkData.GetWorkDataDesc(workDataWithNames.WorkData);
			});
		}

		private void RefreshTopTargetEndDateItems(ClientMenu menu, bool refreshNames)
		{
			var topItems = MenuBuilderHelper.GetTopTargetEndDateWorkData(menu).ToList();
			RefreshTopItems(topItems, topEndDate, (menuItem, workDataWithNames) =>
			{
				if (refreshNames)
					menuItem.Title = workDataWithNames.FullName;
				menuItem.WorkData = workDataWithNames.WorkData;
				menuItem.BarValue = MenuBuilderHelper.GetTargetEndDatePct(menuItem.WorkData);
				menuItem.ToolTip = NSMenuItemWithWorkData.GetWorkDataDesc(workDataWithNames.WorkData);
			});
		}

		private void RefreshTopTargetWorkTimeItems(ClientMenu menu, TotalWorkTimeStats totalStats, bool refreshNames)
		{
			var topItems = MenuBuilderHelper.GetTopTargetWorkTimeWorkData(menu, totalStats).ToList();
			RefreshTopItems(topItems, topTargetWorkTime, (menuItem, workDataWithNames) =>
			{
				if (refreshNames)
					menuItem.Title = workDataWithNames.FullName;
				menuItem.WorkData = workDataWithNames.WorkData;
				menuItem.BarValue = MenuBuilderHelper.GetTargetWorkTimePct(menuItem.WorkData, totalStats);
				var stat = MenuBuilderHelper.GetWorkStatForId(totalStats, workDataWithNames.WorkData.Id.Value);
				if (stat != null)
				{
					menuItem.ToolTip = NSMenuItemWithWorkData.GetWorkDataDesc(workDataWithNames.WorkData)
						+ Environment.NewLine + Labels.WorkData_WorkedHours + ": " + stat.TotalWorkTime.TotalHours.ToString("0.#");
				}
				else
				{
					menuItem.ToolTip = NSMenuItemWithWorkData.GetWorkDataDesc(workDataWithNames.WorkData);
				}
			});
		}

		private void RefreshTopItems(List<WorkDataWithParentNames> topItems, NSMenuItem parentItem, Action<NSMenuItemWithWorkData, WorkDataWithParentNames> refreshAction)
		{
			var currentControls = parentItem.Submenu.ItemArray().OfType<NSMenuItemWithWorkData>().ToList();
			if (!topItems.Select(n => n.WorkData.Id.Value).SequenceEqual(currentControls.Select(n => n.Tag)))
			{
				//if the order doesn't match
				var currentControlsDict = currentControls.ToDictionary(n => n.WorkData.Id.Value);
				parentItem.Submenu.RemoveAllItems(); //no need to unregister click handlers
				parentItem.Enabled = (topItems.Count != 0);
				topMenuItems[parentItem].Clear(); //clear references

				for (int i = 0; i < topItems.Count; i++)
				{
					NSMenuItemWithWorkData menuItem;
					if (!currentControlsDict.TryGetValue(topItems[i].WorkData.Id.Value, out menuItem))
					{
						menuItem = new NSMenuItemWithWorkData(topItems[i].FullName)
						{
							WorkData = topItems[i].WorkData,
						};
						menuItem.Click += (sender, e) => OnMenuClick(e);
						topMenuItems[parentItem].Add(menuItem);  //we need to hold references in mono
					}
					refreshAction(menuItem, topItems[i]);
					parentItem.Submenu.AddItem(menuItem);
				}
			}
			else //same order (very common case and we can avoid flickering)
			{
				Debug.Assert(topItems.Count == currentControls.Count);
				for (int i = 0; i < currentControls.Count; i++)
				{
					refreshAction(currentControls[i], topItems[i]);
				}
			}
		}
	}
}

