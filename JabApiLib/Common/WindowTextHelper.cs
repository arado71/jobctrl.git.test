using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.Java.Common
{
	public static class WindowTextHelper
	{
		[ThreadStatic]
		private static StringBuilder localSb;

		public static string GetClassName(IntPtr hWnd)
		{
			if (localSb == null) localSb = new StringBuilder(64);
			while (true)
			{
				WinApi.GetClassName(hWnd, localSb, localSb.Capacity);
				if (localSb.Length != localSb.Capacity - 1)
				{
					return localSb.ToString();
				}
				localSb.Capacity *= 2;
			}
		}

		public static string GetWindowText(IntPtr hWnd)
		{
			if (localSb == null) localSb = new StringBuilder(64);
			int windowTextLength = WinApi.GetWindowTextLength(hWnd) + 1;
			if (windowTextLength > localSb.Capacity)
			{
				localSb.Capacity = windowTextLength;
			}

			WinApi.GetWindowText(hWnd, localSb, windowTextLength);
			return localSb.ToString();
		}

		public static string GetWindowTextMsg(IntPtr hWnd)
		{
			if (localSb == null) localSb = new StringBuilder(64);
			var windowTextLength = WinApi.SendMessage(hWnd, WinApi.WM_GETTEXTLENGTH, 0, 0) + 1;
			if (windowTextLength > localSb.Capacity)
			{
				localSb.Capacity = windowTextLength;
			}

			WinApi.SendMessage(hWnd, WinApi.WM_GETTEXT, windowTextLength, localSb);
			return localSb.ToString();
		}
	}
}
