using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tct.Java.Common
{
	class WinApi
	{
		public const uint WM_GETTEXT = 0x000D;
		public const uint WM_GETTEXTLENGTH = 0x000E;

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetClassName")]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetWindowTextLength")]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetWindowText")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString,
			int nMaxCount);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "SendMessage")]
		public static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "SendMessage")]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, StringBuilder lParam);
	}
}
