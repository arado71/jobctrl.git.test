using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Accessibility;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows
{
	public static class AccessibilityHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly Guid guidAccessible = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
		public static IAccessible GetIAccessibleFromWindow(IntPtr hWnd, ObjId id)
		{
			var iid = guidAccessible;
			return (IAccessible)WinApi.AccessibleObjectFromWindow(hWnd, (uint)id, ref iid);
		}

		public static object[] GetAccessibleChildren(IAccessible accContainer)
		{
			// Get the number of child interfaces that belong to this object. 
			var childCount = 0;
			try
			{
				childCount = accContainer.accChildCount;
			}
			catch (Exception ex)
			{
				log.Debug("Unable to get child count", ex);
			}

			// Get the child accessible objects.
			var accObjects = new object[childCount];
			if (childCount != 0)
			{
				int count = 0;
				WinApi.AccessibleChildren(accContainer, 0, childCount, accObjects, ref count);
				if (count != accObjects.Length)
				{
					Array.Resize(ref accObjects, count);
				}
			}

			return accObjects;
		}

		//AccRole can return string instead of role constant (https://bugzilla.mozilla.org/show_bug.cgi?id=351293)(https://bugzilla.mozilla.org/show_bug.cgi?id=798492)
		public static bool AccRoleEquals(object accRole, AccessibilityHelper.AccRole expectedValue)
		{
			return (accRole is Int32 && (AccessibilityHelper.AccRole)accRole == expectedValue) || (accRole is string && string.Compare((string)accRole, ((int)expectedValue).ToString(), StringComparison.OrdinalIgnoreCase) == 0);
		}

		public static bool AccRoleEquals(object accRole, string expectedValue)
		{
			return accRole is string && (string)accRole == expectedValue;
		}

		public static bool accStateContains(object accState, AccStates expectedState)
		{
			return accState is Int32 && ((Int32)accState & (Int32)expectedState) != 0;
		}

		public enum ObjId : uint
		{
			WINDOW = 0x00000000,
			SYSMENU = 0xFFFFFFFF,
			TITLEBAR = 0xFFFFFFFE,
			MENU = 0xFFFFFFFD,
			CLIENT = 0xFFFFFFFC,
			VSCROLL = 0xFFFFFFFB,
			HSCROLL = 0xFFFFFFFA,
			SIZEGRIP = 0xFFFFFFF9,
			CARET = 0xFFFFFFF8,
			CURSOR = 0xFFFFFFF7,
			ALERT = 0xFFFFFFF6,
			SOUND = 0xFFFFFFF5,
		}

		public enum NavRelation
		{
			CONTROLLED_BY = 0x1000,
			CONTROLLER_FOR = 0x1001,
			LABEL_FOR = 0x1002,
			LABELLED_BY = 0x1003,
			MEMBER_OF = 0x1004,
			NODE_CHILD_OF = 0x1005,
			FLOWS_TO = 0x1006,
			FLOWS_FROM = 0x1007,
			SUBWINDOW_OF = 0x1008,
			EMBEDS = 0x1009,
			EMBEDDED_BY = 0x100a,
			POPUP_FOR = 0x100b,
			PARENT_WINDOW_OF = 0x100c,
			DEFAULT_BUTTON = 0x100d,
			DESCRIBED_BY = 0x100e,
			DESCRIPTION_FOR = 0x100f
		};

		[Flags]
		public enum AccStates
		{
			STATE_SYSTEM_NORMAL = 0,
			STATE_SYSTEM_UNAVAILABLE = 1,
			STATE_SYSTEM_SELECTED = 2,
			STATE_SYSTEM_FOCUSED = 4,
			STATE_SYSTEM_PRESSED = 8,
			STATE_SYSTEM_CHECKED = 0x10,
			STATE_SYSTEM_MIXED = 0x20,
			STATE_SYSTEM_READONLY = 0x40,
			STATE_SYSTEM_HOTTRACKED = 0x80,
			STATE_SYSTEM_DEFAULT = 0x100,
			STATE_SYSTEM_EXPANDED = 0x200,
			STATE_SYSTEM_COLLAPSED = 0x400,
			STATE_SYSTEM_BUSY = 0x800,
			STATE_SYSTEM_FLOATING = 0x1000,
			STATE_SYSTEM_MARQUEED = 0x2000,
			STATE_SYSTEM_ANIMATED = 0x4000,
			STATE_SYSTEM_INVISIBLE = 0x8000,
			STATE_SYSTEM_OFFSCREEN = 0x10000,
			STATE_SYSTEM_SIZEABLE = 0x20000,
			STATE_SYSTEM_MOVEABLE = 0x40000,
			STATE_SYSTEM_SELFVOICING = 0x80000,
			STATE_SYSTEM_FOCUSABLE = 0x100000,
			STATE_SYSTEM_SELECTABLE = 0x200000,
			STATE_SYSTEM_LINKED = 0x400000,
			STATE_SYSTEM_TRAVERSED = 0x800000,
			STATE_SYSTEM_MULTISELECTABLE = 0x1000000,
			STATE_SYSTEM_EXTSELECTABLE = 0x2000000,
			STATE_SYSTEM_ALERT_LOW = 0x4000000,
			STATE_SYSTEM_ALERT_MEDIUM = 0x8000000,
			STATE_SYSTEM_ALERT_HIGH = 0x10000000,
			STATE_SYSTEM_HASPOPUP = 0x40000000,
			STATE_SYSTEM_VALID = 0x1FFFFFFF
		}

		public enum AccRole
		{
			ROLE_SYSTEM_ALERT = 0x08,
			ROLE_SYSTEM_ANIMATION = 0x36,
			ROLE_SYSTEM_APPLICATION = 0x0e,
			ROLE_SYSTEM_BORDER = 0x13,
			ROLE_SYSTEM_BUTTONDROPDOWN = 0x38,
			ROLE_SYSTEM_BUTTONDROPDOWNGRID = 0x3a,
			ROLE_SYSTEM_BUTTONMENU = 0x39,
			ROLE_SYSTEM_CARET = 0x07,
			ROLE_SYSTEM_CELL = 0x1d,
			ROLE_SYSTEM_CHARACTER = 0x20,
			ROLE_SYSTEM_CHART = 0x11,
			ROLE_SYSTEM_CHECKBUTTON = 0x2c,
			ROLE_SYSTEM_CLIENT = 0x0a,
			ROLE_SYSTEM_CLOCK = 0x3d,
			ROLE_SYSTEM_COLUMN = 0x1b,
			ROLE_SYSTEM_COLUMNHEADER = 0x19,
			ROLE_SYSTEM_COMBOBOX = 0x2e,
			ROLE_SYSTEM_CURSOR = 0x06,
			ROLE_SYSTEM_DIAGRAM = 0x35,
			ROLE_SYSTEM_DIAL = 0x31,
			ROLE_SYSTEM_DIALOG = 0x12,
			ROLE_SYSTEM_DOCUMENT = 0x0f,
			ROLE_SYSTEM_DROPLIST = 0x2f,
			ROLE_SYSTEM_EQUATION = 0x37,
			ROLE_SYSTEM_GRAPHIC = 0x28,
			ROLE_SYSTEM_GRIP = 0x04,
			ROLE_SYSTEM_GROUPING = 0x14,
			ROLE_SYSTEM_HELPBALLOON = 0x1f,
			ROLE_SYSTEM_HOTKEYFIELD = 0x32,
			ROLE_SYSTEM_INDICATOR = 0x27,
			ROLE_SYSTEM_LINK = 0x1e,
			ROLE_SYSTEM_LIST = 0x21,
			ROLE_SYSTEM_LISTITEM = 0x22,
			ROLE_SYSTEM_MENUBAR = 0x02,
			ROLE_SYSTEM_MENUITEM = 0x0c,
			ROLE_SYSTEM_MENUPOPUP = 0x0b,
			ROLE_SYSTEM_OUTLINE = 0x23,
			ROLE_SYSTEM_OUTLINEITEM = 0x24,
			ROLE_SYSTEM_PAGETAB = 0x25,
			ROLE_SYSTEM_PAGETABLIST = 0x3c,
			ROLE_SYSTEM_PANE = 0x10,
			ROLE_SYSTEM_PROGRESSBAR = 0x30,
			ROLE_SYSTEM_PROPERTYPAGE = 0x26,
			ROLE_SYSTEM_PUSHBUTTON = 0x2b,
			ROLE_SYSTEM_RADIOBUTTON = 0x2d,
			ROLE_SYSTEM_ROW = 0x1c,
			ROLE_SYSTEM_ROWHEADER = 0x1a,
			ROLE_SYSTEM_SCROLLBAR = 0x03,
			ROLE_SYSTEM_SEPARATOR = 0x15,
			ROLE_SYSTEM_SLIDER = 0x33,
			ROLE_SYSTEM_SOUND = 0x05,
			ROLE_SYSTEM_SPINBUTTON = 0x34,
			ROLE_SYSTEM_SPLITBUTTON = 0x3e,
			ROLE_SYSTEM_STATICTEXT = 0x29,
			ROLE_SYSTEM_STATUSBAR = 0x17,
			ROLE_SYSTEM_TABLE = 0x18,
			ROLE_SYSTEM_TEXT = 0x2a,
			ROLE_SYSTEM_TITLEBAR = 0x01,
			ROLE_SYSTEM_TOOLBAR = 0x16,
			ROLE_SYSTEM_TOOLTIP = 0x0d,
			ROLE_SYSTEM_WHITESPACE = 0x3b,
			ROLE_SYSTEM_WINDOW = 0x09,
		}
		#region Native methods

		public const int GWL_STYLE = -16;
		public const uint GW_OWNER = 4;
		public const uint WS_POPUP = 0x80000000;
		public const uint WS_MINIMIZEBOX = 0x00020000;
		public const uint WS_MAXIMIZEBOX = 0x00010000;

		//http://stackoverflow.com/questions/3343724/how-do-i-pinvoke-to-getwindowlongptr-and-setwindowlongptr-on-32-bit-platforms
		public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size == 4)
			{
				return WinApi.GetWindowLong32(hWnd, nIndex);
			}
			return WinApi.GetWindowLongPtr64(hWnd, nIndex);
		}

		[DllImport("oleacc.dll")]
		public static extern IntPtr AccessibleObjectFromPoint(WinApi.POINT pt, [Out, MarshalAs(UnmanagedType.Interface)] out IAccessible accObj, [Out] out object ChildID);

		#endregion

	}
}
