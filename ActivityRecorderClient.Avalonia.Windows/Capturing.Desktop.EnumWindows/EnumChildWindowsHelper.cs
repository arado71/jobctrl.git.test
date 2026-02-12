using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows
{
	public static class EnumChildWindowsHelper
	{
		private class ChildWindowInfoBuilder
		{
			public readonly List<ChildWindowInfo> ChildWindowsInfo = new List<ChildWindowInfo>();
			private readonly Func<ChildWindowInfo, bool> predicate;
			private readonly bool processAll;

			public ChildWindowInfoBuilder(Func<ChildWindowInfo, bool> predicate, bool processAll)
			{
				this.predicate = predicate;
				this.processAll = processAll;
			}

			public bool ProcessChildWindow(IntPtr hWnd, int lParam)
			{
				var window = new ChildWindowInfo
				{
					Handle = hWnd,
					ClassName = WindowTextHelper.GetClassName(hWnd),
					Caption = WindowTextHelper.GetWindowText(hWnd)
				};

				var interested = (predicate == null || predicate(window));
				if (interested)
				{
					ChildWindowsInfo.Add(window);
				}

				return processAll || !interested; //process next window
			}
		}

		public static ChildWindowInfo GetFirstChildWindowInfo(IntPtr parentHandle, Func<ChildWindowInfo, bool> predicate)
		{
			var builder = new ChildWindowInfoBuilder(predicate, false);
			WinApi.EnumChildWindows(parentHandle, builder.ProcessChildWindow, 0);
			return builder.ChildWindowsInfo.FirstOrDefault();
		}

		public static List<ChildWindowInfo> GetChildWindowInfo(IntPtr parentHandle, Func<ChildWindowInfo, bool> predicate)
		{
			var builder = new ChildWindowInfoBuilder(predicate, true);
			WinApi.EnumChildWindows(parentHandle, builder.ProcessChildWindow, 0);
			return builder.ChildWindowsInfo;
		}

		public static List<ChildWindowInfo> GetChildWindowInfo(IntPtr parentHandle)
		{
			var builder = new ChildWindowInfoBuilder(null, true);
			WinApi.EnumChildWindows(parentHandle, builder.ProcessChildWindow, 0);
			return builder.ChildWindowsInfo;
		}

		private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
	}
}
