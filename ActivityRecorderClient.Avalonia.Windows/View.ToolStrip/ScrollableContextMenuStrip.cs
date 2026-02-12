using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	/// <summary>
	/// A ContextMenuStrip that you can scoll with the mouse wheel.
	/// ToolStripDropDownMenu of ToolStripMenuItems are also scrolled when mouse is over them.
	/// (others YAGNI atm.)
	/// </summary>
	/// <remarks>
	/// Scrolling will be messed up if Avalable = false item is at the top of the screen (and scrolling down), but that is an MS bug.
	/// </remarks>
	//todo fix if downScrollButton or upScrollButton is pressed then focus is lost and scrolling won't work because OnMouseWheel is not called
	public class ScrollableContextMenuStrip : ContextMenuStrip
	{
		private static readonly MethodInfo scrollMethod = typeof(ToolStripDropDownMenu).GetMethod(
													"ScrollInternal",
													BindingFlags.Instance | BindingFlags.NonPublic,
													Type.DefaultBinder,
													new Type[] { typeof(bool) },
													null
													);

		private static readonly FieldInfo downScrollButtonField = typeof(ToolStripDropDownMenu).GetField(
													"downScrollButton",
													BindingFlags.Instance | BindingFlags.NonPublic
													);

		private static readonly FieldInfo upScrollButtonField = typeof(ToolStripDropDownMenu).GetField(
													"upScrollButton",
													BindingFlags.Instance | BindingFlags.NonPublic
													);

		private const int scrolledItems = 3;

		public ScrollableContextMenuStrip()
		{
		}

		public ScrollableContextMenuStrip(IContainer container)
			: base(container)
		{
		}

		protected override void OnOpened(EventArgs e)
		{
			this.Focus(); //focus is needed for scrolling
			base.OnOpened(e);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			//find the top most menu under the mouse and scoll that or scroll the root context menu
			var topMenuToScroll = GetOpenedScrollableChildMenus(this)
				.Reverse() //we have to consider the last menu first because its on the top
				.Where(n => IsMouseOver(n))
				.DefaultIfEmpty(this) //if found no child menus under the mouse then scroll this menu
				.First();

			ScrollMenu(topMenuToScroll, e.Delta > 0, scrolledItems);

			base.OnMouseWheel(e);
		}

		private static IEnumerable<ToolStripDropDownMenu> GetOpenedScrollableChildMenus(ToolStripDropDownMenu rootMenu)
		{
			var root = rootMenu;
			while (root != null)
			{
				root = root.Items
					.OfType<ToolStripMenuItem>() //we only scoll child menus of ToolStripMenuItem
					.Where(n => n.DropDown.Visible)
					.Select(n => n.DropDown)
					.OfType<ToolStripDropDownMenu>() //we only scroll ToolStripDropDownMenus
					.FirstOrDefault();
				if (root != null)
				{
					yield return root;
				}
			}
		}

		public void BringToFrontAll() //don't ask me why, but this seems to cause the least flickering when popup is shown
		{
			var opened = GetOpenedScrollableChildMenus(this).ToList();
			if (opened.Count > 1) //there is a chance that dropdown is over this context menu
			{
				NotificationWinService.SetInactiveTopMost(opened[opened.Count - 1]);
				//opened[opened.Count - 1].BringToFront(); //we'll loose scrolling ability if we are not over the last opened dropdown
			}
			else //bringig context menu to the front won't hide anything
			{
				NotificationWinService.SetInactiveTopMost(this);
				//this.BringToFront();
			}
		}

		private static bool IsMouseOver(Control ctrl)
		{
			if (ctrl == null) return false;
			return ctrl.ClientRectangle.Contains(ctrl.PointToClient(Cursor.Position));
		}

		private static void ScrollMenu(ToolStripDropDownMenu ctrl, bool up, int times)
		{
			int i = times;
			while (--i >= 0 && ScrollMenu(ctrl, up)) { /*do nothing*/ }
		}

		private static bool ScrollMenu(ToolStripDropDownMenu ctrl, bool up)
		{
			if (up)
			{
				var upScrollButton = upScrollButtonField.GetValue(ctrl) as ToolStripControlHost;
				if (upScrollButton == null || !upScrollButton.Visible || !upScrollButton.Enabled) return false;
			}
			else
			{
				var downScrollButton = downScrollButtonField.GetValue(ctrl) as ToolStripControlHost;
				if (downScrollButton == null || !downScrollButton.Visible || !downScrollButton.Enabled) return false;
			}

			scrollMethod.Invoke(ctrl, new object[] { up });
			return true;
		}
	}
}
