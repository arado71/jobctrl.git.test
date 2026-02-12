using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.View;
using Tct.ActivityRecorderClient.View.ToolStrip;

namespace Tct.ActivityRecorderClient.Menu
{
	/// <summary>
	/// Creates/Updates menu to a ContextMenuStrip from a ClientMenu on the GUI thread
	/// </summary>
	public class MenuBuilder : IDisposable
	{
		public event EventHandler<WorkDataEventArgs> MenuClick;
		public event EventHandler<WorkDataEventArgs> MenuButtonClick;
		public readonly ToolStripItem PlaceHolder = new ToolStripMenuItem() { Visible = false };

		private readonly ContextMenuStrip guiMenuStrip;
		//todo manage recent works here... private readonly ToolStripItem recentWorks
		private readonly ToolStripMenuItem topPriority = new ToolStripMenuItem(Labels.Menu_TopPriorityWorks) { Available = false };
		private readonly ToolStripMenuItem topEndDate = new ToolStripMenuItem(Labels.Menu_TopEndDateWorks) { Available = false };
		private readonly ToolStripMenuItem topTargetWorkTime = new ToolStripMenuItem(Labels.Menu_TopTargetWorkTimeWorks) { Available = false };
		private ClientMenuLookup clientMenuLookup = new ClientMenuLookup();

		private SimpleWorkTimeStats SimpleWorkTimeStats { get; set; }
		private TaskReasons taskReasons;
		private ClientMenu ClientMenu { get { return clientMenuLookup.ClientMenu; } }

		public MenuBuilder(ContextMenuStrip guiMenuStrip)
		{
			if (guiMenuStrip == null) throw new ArgumentNullException("guiMenuStrip");
			this.guiMenuStrip = guiMenuStrip;
			guiMenuStrip.KeyDown += HandleKeyDownForDropDown;
			topPriority.DropDown.KeyDown += HandleKeyDownForDropDown;
			topEndDate.DropDown.KeyDown += HandleKeyDownForDropDown;
			topTargetWorkTime.DropDown.KeyDown += HandleKeyDownForDropDown;
		}

		private void HandleKeyDownForDropDown(object sender, KeyEventArgs e)
		{
			ClipboardHelper.HandleCtrlCKeyDownForDropDown(sender, e, clientMenuLookup);
		}

		//changing menu while guiMenuStrip is shown has some weird side effects (e.g. won't hide on click), so we have to set Visible to false manually
		public void UpdateMenu(ClientMenuLookup menuLookup)
		{
			if (guiMenuStrip == null || menuLookup == null || menuLookup.ClientMenu == null) return;
			clientMenuLookup = menuLookup;
			guiMenuStrip.SuspendLayout(); //performance reasons
			int firstIdx = int.MaxValue;
			int i = guiMenuStrip.Items.Count - 1;
			while (i >= 0)
			{
				if (ReferenceEquals(guiMenuStrip.Items[i].Tag, this))
				{
					firstIdx = i;
					guiMenuStrip.Items.RemoveAtWithDispose(i);
				}
				i--;
			}
			if (firstIdx == int.MaxValue)
			{
				firstIdx = guiMenuStrip.Items.IndexOf(PlaceHolder);
				if (firstIdx > -1)
				{
					guiMenuStrip.Items.RemoveAtWithDispose(firstIdx);
				}
				else
				{
					firstIdx = 0;
				}
			}

			var newMenu = GetDynamicMenu(clientMenuLookup.ClientMenu);

			FlattenMenu(newMenu, ConfigManager.LocalSettingsForUser.MenuFlattenFactor);

			AddTopMenuItemsIfFirstTime(firstIdx);
			RefreshTopPriorityItems(ClientMenu, true);
			RefreshTopTargetEndDateItems(ClientMenu, true);
			RefreshTopTargetWorkTimeItems(ClientMenu, SimpleWorkTimeStats, true);

			for (int j = newMenu.Count - 1; j >= 0; j--)
			{
				guiMenuStrip.Items.Insert(firstIdx, newMenu[j]);
			}
			if (newMenu.Count == 0)
			{
				guiMenuStrip.Items.Insert(firstIdx, PlaceHolder);
			}
			guiMenuStrip.ResumeLayout();
		}

		public void UpdateTargetEndDatePercentages()
		{
			RefreshTopTargetEndDateItems(ClientMenu, false);
		}

		public void UpdateTargetTotalWorkTimePercentages(SimpleWorkTimeStats totalStats)
		{
			SimpleWorkTimeStats = totalStats;
			RefreshTopTargetWorkTimeItems(ClientMenu, SimpleWorkTimeStats, false);
		}

		private bool firstTime = true;
		private void AddTopMenuItemsIfFirstTime(int firstIdx)
		{
			if (!firstTime) return;
			var placeIdx = firstIdx - 1;
			if (placeIdx <= -1) return;
			guiMenuStrip.Items.Insert(++placeIdx, new ToolStripSeparator());
			guiMenuStrip.Items.Insert(++placeIdx, topPriority);
			guiMenuStrip.Items.Insert(++placeIdx, topEndDate);
			guiMenuStrip.Items.Insert(++placeIdx, topTargetWorkTime);
			firstTime = false;
		}

		private void RefreshTopPriorityItems(ClientMenu menu, bool refreshNames)
		{
			var topItems = MenuBuilderHelper.GetTopPriorityWorkData(menu).ToList();
			var maxPriority = topItems.Select(n => n.WorkData.Priority).FirstOrDefault();
			if (!maxPriority.HasValue || maxPriority.Value <= 0 || topItems.Count == 0)
			{
				topPriority.DropDownItems.ClearWithDispose(); //no need to unregister click handlers
				topPriority.Available = false;
				return;
			}
			RefreshTopItems(topItems, topPriority, (menuItem, workDataWithNames) =>
													{
														if (refreshNames) menuItem.Text = EscapeText(workDataWithNames.FullName);
														menuItem.WorkData = workDataWithNames.WorkData;
														menuItem.Value = menuItem.WorkData.Priority / (float)maxPriority;
														menuItem.BarText = menuItem.WorkData.Priority.ToString();
													});
		}

		private void RefreshTopTargetEndDateItems(ClientMenu menu, bool refreshNames)
		{
			var topItems = MenuBuilderHelper.GetTopTargetEndDateWorkData(menu).ToList();
			RefreshTopItems(topItems, topEndDate, (menuItem, workDataWithNames) =>
													{
														if (refreshNames) menuItem.Text = EscapeText(workDataWithNames.FullName);
														menuItem.WorkData = workDataWithNames.WorkData;
														menuItem.Value = MenuBuilderHelper.GetTargetEndDatePct(menuItem.WorkData);
													});
		}

		private void RefreshTopTargetWorkTimeItems(ClientMenu menu, SimpleWorkTimeStats totalStats, bool refreshNames)
		{
			var topItems = MenuBuilderHelper.GetTopTargetWorkTimeWorkData(menu, totalStats).ToList();
			RefreshTopItems(topItems, topTargetWorkTime, (menuItem, workDataWithNames) =>
													{
														if (refreshNames) menuItem.Text = EscapeText(workDataWithNames.FullName);
														menuItem.WorkData = workDataWithNames.WorkData;
														menuItem.Value = MenuBuilderHelper.GetTargetWorkTimePct(menuItem.WorkData, totalStats);
														var stat = MenuBuilderHelper.GetWorkStatForId(totalStats, menuItem.WorkData.Id.Value);
														if (stat != null)
														{
															menuItem.ToolTipText = ToolStripMenuItemForWorkData.GetWorkDataDesc(menuItem.WorkData, stat.TotalWorkTime);
														}
													});
		}

		public void UpdateReasonStats(TaskReasons value)
		{
			taskReasons = value;
			if (ConfigManager.LocalSettingsForUser.HighlightNonReasonedWork)
				UpdateMenu(clientMenuLookup);
		}

		private void RefreshTopItems(List<WorkDataWithParentNames> topItems, ToolStripMenuItem parentItem, Action<ToolStripMenuItemForWorkData, WorkDataWithParentNames> refreshAction)
		{
			var currentControls = parentItem.DropDownItems.OfType<ToolStripMenuItemForWorkData>().ToList();
			if (!topItems.Select(n => n.WorkData.Id.Value).SequenceEqual(currentControls.Select(n => n.WorkData.Id.Value)))
			{
				//if the order doesn't match
				var currentControlsDict = currentControls.ToDictionary(n => n.WorkData.Id.Value);
				parentItem.DropDownItems.Clear(); //no need to unregister click handlers
				parentItem.Available = (topItems.Count != 0);

				for (int i = 0; i < topItems.Count; i++)
				{
					ToolStripMenuItemForWorkData menuItem;
					if (!currentControlsDict.TryGetValue(topItems[i].WorkData.Id.Value, out menuItem))
					{
						menuItem = new ToolStripMenuItemForWorkData(topItems[i].WorkData)
						{
							Text = EscapeText(topItems[i].FullName),
							IsButtonVisible = true,
						};
						menuItem.MouseDown += MenuClickForToolStripMenuItemForWorkData;
						menuItem.Click += MenuClickForToolStripMenuItemForWorkData;
						menuItem.ButtonClick += MenuButtonClickForToolStripMenuItemForWorkData;
					}
					else
					{
						currentControlsDict.Remove(topItems[i].WorkData.Id.Value); //we re-use this item so don't Dispose
					}
					refreshAction(menuItem, topItems[i]);
					parentItem.DropDownItems.Add(menuItem);
				}
				foreach (var item in currentControlsDict.Values)
				{
					item.Dispose(); //Dispose unused menuitems
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

		private List<ToolStripItem> GetDynamicMenu(ClientMenu tree)
		{
			var result = new List<ToolStripItem>();
			if (tree.Works == null) return result;
			foreach (WorkData itemData in tree.Works)
			{
				var menuItems = BuildTree(itemData, new List<WorkData>());
				if (menuItems != null)
				{
					result.Add(menuItems);
				}
			}
			return result;
		}

		private ToolStripItem BuildTree(WorkData tree, List<WorkData> visitedElements)
		{
			if (tree == null || !IsVisibleInMenu(tree)) return null;
			//if (visitedElements.Contains(tree)) return null; //equlas might be overriden later
			foreach (var visitedElement in visitedElements) //avoid infinite loop
			{
				if (ReferenceEquals(tree, visitedElement)) return null;
			}
			visitedElements.Add(tree);
			if (string.IsNullOrEmpty(tree.Name))
			{
				return new ToolStripSeparator() { Tag = this };
			}
			var menuItem = new ToolStripMenuItemWithButton(EscapeText(tree.Name)) { Tag = this, WorkData = tree };
			if (tree.Id.HasValue)
			{
				menuItem.MouseDown += MenuClickForToolStripMenuItemWithButton;
				menuItem.Click += MenuClickForToolStripMenuItemWithButton;
				menuItem.ButtonClick += MenuButtonClickForToolStripMenuItemWithButton;
				menuItem.IsButtonVisible = true;
				int cnt = 1;
				if (ConfigManager.LocalSettingsForUser.HighlightNonReasonedWork && taskReasons != null && taskReasons.ReasonsByWorkId != null)
				{
					List<Reason> reasons;
					if (taskReasons.ReasonsByWorkId.TryGetValue(tree.Id.Value, out reasons))
						cnt = reasons.Count;
					else
						cnt = 0;
				}
				if (cnt == 0)
					menuItem.ForeColor = System.Drawing.Color.Red;
			}
			if (tree.Children == null) return menuItem;
			if (tree.Children.Count > 0) menuItem.DropDown.KeyDown += HandleKeyDownForDropDown;
			foreach (var item in tree.Children)
			{
				var childItem = BuildTree(item, visitedElements);
				if (childItem != null)
				{
					menuItem.DropDownItems.Add(childItem);
				}
			}
			return menuItem;
		}

		private bool IsVisibleInMenu(WorkData tree)
		{
			return tree != null && (tree.Id.HasValue ? IsVisibleWork(tree) : HasAnyVisibleChildren(tree));
		}

		private bool IsVisibleWork(WorkData work)
		{
			return work != null && work.Id.HasValue && work.IsVisibleInMenu &&
				   (ConfigManager.LocalSettingsForUser.ShowDynamicWorks || !clientMenuLookup.IsDynamicWork(work.Id.Value));
		}

		private bool HasAnyVisibleChildren(WorkData tree)
		{
			var hasVisible = false;
			var queue = new Queue<WorkData>();
			queue.Enqueue(tree);
			while (queue.Count > 0)
			{
				var curr = queue.Dequeue();
				if (curr == null) continue;
				if (curr.Id.HasValue && IsVisibleWork(curr))
				{
					hasVisible = true;
					break;
				}
				if (curr.Children != null)
				{
					foreach (var child in curr.Children)
					{
						queue.Enqueue(child);
					}
				}
			}
			return hasVisible;
		}

		private void FlattenMenu(List<ToolStripItem> menu, int factor)
		{
			var fakeParent = new ToolStripMenuItem(); //create a fake parent to store the items in menu
			foreach (var item in menu)
			{
				fakeParent.DropDownItems.Add(item);
			}

			foreach (var item in menu.OfType<ToolStripMenuItem>())
			{
				FlattenItemAtIdx(fakeParent, item, fakeParent.DropDownItems.IndexOf(item), factor);
			}

			menu.Clear();
			menu.AddRange(fakeParent.DropDownItems.Cast<ToolStripItem>());
			fakeParent.DropDownItems.Clear(); //clear references from items to fakeParent
		}

		private int FlattenItemAtIdx(ToolStripMenuItem parent, ToolStripMenuItem child, int childIdxInParent, int factor)
		{
			int inserted = 0;
			//first flatten child (if it has any DropDownItems)
			for (int i = 0; i < child.DropDownItems.Count; i++)
			{
				var grandChild = child.DropDownItems[i] as ToolStripMenuItem;
				if (grandChild == null) continue;
				var newlyInseted = FlattenItemAtIdx(child, grandChild, i, factor);
				i += newlyInseted; //skip newly inserted items (they are already flattened) but we won't add it to 'inserted' because its only for one level
			}

			var grandChildCount = child.DropDownItems.OfType<ToolStripMenuItem>().Count();
			if (grandChildCount > factor || grandChildCount == 0) return inserted; //we cannot flatten anymore (if it has only spearators then we will leave them as is [YAGNI])
			//we can flatten all DropDownItems
			for (int i = child.DropDownItems.Count - 1; i >= 0; i--)
			{
				var grandChild = child.DropDownItems[i] as ToolStripMenuItem;
				child.DropDownItems.RemoveAt(i); //remove from original position. Insert below would also do the remove (which is kinda confusing), so to make this more readable we first remove then we insert if necessary
				if (grandChild == null) continue;
				//bring grandChild one level up (we don't care about separators)
				grandChild.Text = child.Text + WorkDataWithParentNames.DefaultSeparator + grandChild.Text;
				parent.DropDownItems.Insert(childIdxInParent + 1, grandChild); //we go backwards so the order is the same (inserting here would also remove from child's DropDownItems if it still had an owner).
				inserted++;
			}
			Debug.Assert(child.DropDownItems.Count == 0);
			//remove child if not clickable (it shouldn't be clickable atm.)
			parent.DropDownItems.RemoveAtWithDispose(childIdxInParent);
			inserted--;
			return inserted;
		}

		public static string EscapeText(string value)
		{
			return value == null ? null : value.Replace("&", "&&");
		}

		private void OnMenuClick(WorkDataEventArgs e)
		{
			Debug.Assert(e != null && e.WorkData != null);
			EventHandler<WorkDataEventArgs> click = MenuClick;
			if (click != null) click(this, e);
		}

		private void OnMenuButtonClick(WorkDataEventArgs e)
		{
			Debug.Assert(e != null && e.WorkData != null);
			EventHandler<WorkDataEventArgs> click = MenuButtonClick;
			if (click != null) click(this, e);
		}

		private void MenuClickForToolStripMenuItemWithButton(object sender, EventArgs e)
		{
			var toolStrip = sender as ToolStripMenuItemWithButton;
			if (toolStrip != null)
			{
				OnMenuClick(new WorkDataEventArgs(toolStrip.WorkData));
			}
			else Debug.Fail("Wrong type");
		}

		private void MenuButtonClickForToolStripMenuItemWithButton(object sender, EventArgs e)
		{
			var toolStrip = sender as ToolStripMenuItemWithButton;
			if (toolStrip != null)
			{
				OnMenuButtonClick(new WorkDataEventArgs(toolStrip.WorkData));
			}
			else Debug.Fail("Wrong type");
		}

		private void MenuClickForToolStripMenuItemForWorkData(object sender, EventArgs e)
		{
			var toolStrip = sender as ToolStripMenuItemForWorkData;
			if (toolStrip != null)
			{
				OnMenuClick(new WorkDataEventArgs(toolStrip.WorkData));
			}
			else Debug.Fail("Wrong type");
		}

		private void MenuButtonClickForToolStripMenuItemForWorkData(object sender, EventArgs e)
		{
			var toolStrip = sender as ToolStripMenuItemForWorkData;
			if (toolStrip != null)
			{
				OnMenuButtonClick(new WorkDataEventArgs(toolStrip.WorkData));
			}
			else Debug.Fail("Wrong type");
		}

		private bool isDisposed;
		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			guiMenuStrip.KeyDown -= HandleKeyDownForDropDown;
		}
	}
}
