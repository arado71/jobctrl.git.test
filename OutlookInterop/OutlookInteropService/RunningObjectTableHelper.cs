using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using log4net;

namespace OutlookInteropService
{
	public static class RunningObjectTableHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly bool isElevated = ProcessElevationHelper.IsElevated();

		//http://support.microsoft.com/kb/238610
		public static bool EnsureRotRegistration(string processName, out bool isElevationChangeRequired, out bool isProcessElevated)
		{
			isElevationChangeRequired = false;
			isProcessElevated = false;
			var hWnd = GetForegroundWindow();
			if (hWnd == IntPtr.Zero) return false;
			int procId;
			var foreThreadId = GetWindowThreadProcessId(hWnd, out procId);
			string activeProcessName;
			if (!ProcessNameHelper.TryGetProcessName(procId, hWnd, out activeProcessName)
				|| !string.Equals(activeProcessName, processName, StringComparison.OrdinalIgnoreCase)) return false;

			try
			{
				var windowClassNameBuilder = new StringBuilder(64);
				GetClassName(hWnd, windowClassNameBuilder, windowClassNameBuilder.Capacity);
				if (windowClassNameBuilder.ToString() == "MsoSplash")
					return false;
				var style = (long)GetWindowLong(hWnd, GWL_STYLE);
				var isPopup = (style & WS_POPUP) == WS_POPUP && ((style & WS_MINIMIZEBOX) == 0 || (style & WS_MAXIMIZEBOX) == 0);
				if (isPopup) return false;
				//AttachThreadInput would still work if the two processes are on different elevation level, but COM interop won't.
				//So to avoid endless foreground window changes we don't proceed if elevation levels are different
				isProcessElevated = ProcessElevationHelper.IsElevated(procId);
				if (isElevated != isProcessElevated)
				{
					isElevationChangeRequired = true;
					return true;
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to determine elevation level", ex);
				return false;
			}

			//we need to make outlook lose focus in order to register itself into ROT
			var currThreadId = GetCurrentThreadId();
			try
			{
				using (var form = new Form() { StartPosition = FormStartPosition.Manual, ShowInTaskbar = false, Width = 0, Height = 0, Left = -2000, Top = -2000 })
				{
					log.Info("Dummy form created");
					form.Show();

					//couldn't find any better solution for this in the given timeframe sorry Raymond. (but since we don't really want to steal focus there might be...)
					//http://blogs.msdn.com/b/oldnewthing/archive/2008/08/01/8795860.aspx
					if (foreThreadId != currThreadId)
					{
						bool res; //todo error handling
						log.Info("Attaching... " + foreThreadId + " - " + currThreadId);
						res = AttachThreadInput(foreThreadId, currThreadId, true);
						log.Info("Switching... | prev: " + res);
						res = SetForegroundWindow(form.Handle);
						log.Info("Detaching... | prev: " + res);
						res = AttachThreadInput(foreThreadId, currThreadId, false);
						log.Info("Switching back... | prev: " + res);
						res = SetForegroundWindow(hWnd);
						log.Info("Done. | prev: " + res);
					}
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error while creating dummy form", ex);
			}

			return true;
		}

		#region Native methods

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		[DllImport("kernel32.dll")]
		static extern uint GetCurrentThreadId();

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetClassName")]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		private const int GWL_STYLE = -16;
		private const uint GW_OWNER = 4;
		private const uint WS_POPUP = 0x80000000;
		private const uint WS_MINIMIZEBOX = 0x00020000;
		private const uint WS_MAXIMIZEBOX = 0x00010000;

		//http://stackoverflow.com/questions/3343724/how-do-i-pinvoke-to-getwindowlongptr-and-setwindowlongptr-on-32-bit-platforms
		private static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size == 4)
			{
				return GetWindowLong32(hWnd, nIndex);
			}
			return GetWindowLongPtr64(hWnd, nIndex);
		}

		[DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
		#endregion
	}
}
