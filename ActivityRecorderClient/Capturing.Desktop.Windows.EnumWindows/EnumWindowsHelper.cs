using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows
{
	/// <summary>
	/// Helper class for getting all windows from the desktop.
	/// </summary>
	/// <remarks>
	/// Metro apps are not returned by EnumWindows so we have to use FindWindows to get them.
	/// In order to be backward compatible in win8+ we use EnumWindows first and then add new windows found by FindWindow.
	/// </remarks>
	public static class EnumWindowsHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		//private static readonly bool isVistaOrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;
		private static readonly bool isWindows8OrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 2);

		//Returned windows are in Z-order started from the most topmost window.
		public static List<DesktopWindow> GetWindowsInfo(bool isLocked)
		{
			var builder = new WindowInfoBuilder();
			builder.NeedTitle = !ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableTitleCapture);
			builder.NeedProcessId = !ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableProcessCapture);
			WinApi.EnumWindows(builder.ProcessEnumWindow, 0);
			builder.ProcessAdditionalWindows();
			builder.EnsureActiveWindow(isLocked); //legacy hax. It's good to have one active window for every capture (for time calculations and for rules)
			return builder.WindowsInfo;
		}
		public static IEnumerable<DesktopWindow> GetVisibleWindows()
		{
			var except = new[] { "explorer.exe", "MicrosoftEdge.exe", "ApplicationFrameHost.exe", "SystemSettings.exe" };
			return GetWindowsInfo(false).Where(e => !except.Contains(e.ProcessName));
		}

		private class WindowInfoBuilder
		{
			public readonly List<DesktopWindow> WindowsInfo = new List<DesktopWindow>();
			private readonly HashSet<IntPtr> win8EnumWindowHandles = new HashSet<IntPtr>(); //only relevant for win8 or later
			private readonly HashSet<IntPtr> win8AllHandles = new HashSet<IntPtr>(); //only relevant for win8 or later
			private int insertAtIdx;
			private bool hasActiveWindow;
			private IntPtr activeHWnd;
			private bool activeFound;
			public bool NeedTitle, NeedProcessId;

			public void ProcessAdditionalWindows()
			{
				if (!isWindows8OrLater) return;
				try
				{
					var i = 0;
					var hWnd = WinApi.FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, IntPtr.Zero);
					while (hWnd != IntPtr.Zero && i++ < 3000) //Enumerate only first 3 thousand windows to protect against infinite loop. 
					{
						ProcessAdditionalWindow(hWnd);
						hWnd = WinApi.FindWindowEx(IntPtr.Zero, hWnd, null, IntPtr.Zero);
					}

					if (hWnd != IntPtr.Zero)
					{
						log.WarnAndFail("Infinite loop detected.");
					}
				}
				catch (Exception ex)
				{
					log.Error("EnumWindowsByFindWindow failed.", ex);
				}
			}

			private void ProcessAdditionalWindow(IntPtr hWnd) //windows from findwindow are in z-order too, we assume the z-order is not changed since enumwindows
			{
				Debug.Assert(isWindows8OrLater);
				if (win8AllHandles.Contains(hWnd))
				{
					if (win8EnumWindowHandles.Contains(hWnd)) //Update insert position
					{
						while (insertAtIdx < WindowsInfo.Count && WindowsInfo[insertAtIdx].Handle != hWnd) insertAtIdx++;
						if (insertAtIdx < WindowsInfo.Count) insertAtIdx++;
					}
					return;
				}
				//new handle
				DesktopWindow window;
				if (TryGetDesktopWindow(hWnd, out window))
				{
					int cloaked;
					if (WinApi.DwmGetWindowAttribute(hWnd, WinApi.DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out cloaked, 4) == 0 && cloaked != 0) return; //Check if potential metro app window is visible or moved to the background.
					window.IsAdditionalWindow = true;
					WindowsInfo.Insert(insertAtIdx++, window);
				}
			}

			public bool ProcessEnumWindow(IntPtr hWnd, int lParam)
			{
				DesktopWindow window;
				if (TryGetDesktopWindow(hWnd, out window))
				{
					WindowsInfo.Add(window);
					if (isWindows8OrLater) win8EnumWindowHandles.Add(hWnd);
				}
				return true; //process next window
			}

			private bool TryGetDesktopWindow(IntPtr hWnd, out DesktopWindow window)
			{
				window = null;
				try
				{
					if (isWindows8OrLater) win8AllHandles.Add(hWnd);

					if (!hasActiveWindow)
					{
						activeHWnd = WinApi.GetForegroundWindow(); //returns IntPtr.Zero on locked machine -> no active window (can also happen when minimizing)
						hasActiveWindow = true;
					}

					var visible = WinApi.IsWindowVisible(hWnd);
					if (!visible) return false; //proceed to the next window if this is not visible
					var className = WindowTextHelper.GetClassName(hWnd);
					if (className == "TaskListThumbnailWnd") return false; //this is a transparent window on Win7 (shows censored ones / hides other windows
					if (isWindows8OrLater &&
					    (className == "EdgeUiInputWndClass" || className == "EdgeUiInputTopWndClass" ||
					     className == "Shell_LightDismissOverlay")) return false;	//these are transparent windows on Win8

					window = new DesktopWindow
					{
						Handle = hWnd,
						IsActive = activeHWnd != IntPtr.Zero && activeHWnd == hWnd,
						//Minimized = IsIconic(hWnd),
						IsMaximized = WinApi.IsZoomed(hWnd),
						WindowRect = GetWindowRect(hWnd),
						ClientRect = GetClientRectInScreenCoord(hWnd),
						Title = NeedTitle ? WindowTextHelper.GetWindowText(hWnd) : "*Suppressed*",
						ProcessId = NeedProcessId ? GetWindowThreadProcessIdWrapper(hWnd) : 0,
						//ClassName = GetClassName(hWnd),
						CreateDate = DateTime.UtcNow,
					};

					activeFound |= window.IsActive;
					return true;
				}
				catch (Exception ex)
				{
					log.Error("Unable to process window with hande " + hWnd, ex);
				}
				return false; //process next window
			}

			public void EnsureActiveWindow(bool isLocked)
			{
				if (activeFound) return;
				WindowsInfo.Insert(0, new DesktopWindow()
				{
					Handle = IntPtr.Zero,
					IsActive = true,
					WindowRect = new Rectangle(0, 0, 0, 0),
					ClientRect = new Rectangle(0, 0, 0, 0),
					Title = "",
					ProcessId = isLocked ? -1 : 0,
					CreateDate = DateTime.UtcNow,
				});
				activeFound = true;
			}
		}

		private static int GetWindowThreadProcessIdWrapper(IntPtr hWnd)
		{
			int procId;
			WinApi.GetWindowThreadProcessId(hWnd, out procId);
			return procId;
		}

		private static Rectangle GetClientRectInScreenCoord(IntPtr hWnd)
		{
			var rect = new WinApi.RECT();
			WinApi.GetClientRect(hWnd, ref rect);
			var point = new WinApi.POINT(rect.Left, rect.Top);
			WinApi.ClientToScreen(hWnd, ref point);
			return new Rectangle(point.x, point.y, rect.Right - rect.Left, rect.Bottom - rect.Top);
		}

		private static Rectangle GetWindowRect(IntPtr hWnd)
		{
			WinApi.RECT rect;
			//DwmGetWindowAttribute is 0,1 ms while GetWindowRect is 0,003ms per window and there is litte difference between the two... so skip using it atm.
			//if (!isVistaOrLater || DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out rect, 4 * 4) != 0)
			//{
			WinApi.GetWindowRect(hWnd, out rect);
			//}
			return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
		}

		private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
	}
}
