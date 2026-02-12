using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tct.ActivityRecorderClient
{
	public static class SameAppNotifyHelper
	{
		public static readonly int WM_CUSTOMNOTIFY = WinApi.RegisterWindowMessage("WM_CUSTOMNOTIFY_" + ConfigManager.ApplicationName);
		public static readonly string WindowTitle = ConfigManager.ApplicationName + "_" + WM_CUSTOMNOTIFY;

		
		private static readonly WinApi.EnumWindowsProc callBackPtr = NotifyWithSameTitle;

		public static void Notify()
		{
			WinApi.EnumWindows(callBackPtr, 0);
		}

		private static bool NotifyWithSameTitle(IntPtr hwnd, int lParam)
		{
			StringBuilder sb = new StringBuilder(WindowTitle.Length + 1);
			WinApi.GetWindowText(hwnd, sb, sb.Capacity);
			if (sb.ToString() == WindowTitle)
			{
				WinApi.PostMessage(
					hwnd,
					WM_CUSTOMNOTIFY,
					IntPtr.Zero,
					IntPtr.Zero);
			}
			return true;
		}
	}
}
