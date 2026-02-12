using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public static class AppVersionLogger
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static HashSet<IntPtr> knownHwnds = new HashSet<IntPtr>();
		private static HashSet<uint> knownPids = new HashSet<uint>();
		private static HashSet<string> knownPaths = new HashSet<string>();
		private static readonly object lockObject = new object();

		public static void LogAssemblyVersionFromHwnd(IntPtr hWnd)
		{
			lock (lockObject)
			{
				if (knownHwnds.Contains(hWnd)) return;
				WinApi.GetWindowThreadProcessId(hWnd, out uint pid);
				LogAssemblyVersionFromProcId(pid);
				knownHwnds.Add(hWnd);
			}
		}

		public static void LogAssemblyVersionEdge(IntPtr hWnd)
		{
			if (knownHwnds.Contains(hWnd)) return;
			if (!WindowTextHelper.GetWindowText(hWnd).EndsWith("Microsoft Edge")) return;
			var edgeWin = EnumChildWindowsHelper.GetFirstChildWindowInfo(hWnd, w => w.Caption == "Microsoft Edge");
			if (edgeWin == null) return;
			LogAssemblyVersionFromHwnd(edgeWin.Handle);
		}

		public static void LogAssemblyVersionFromProcId(uint procId, string optionalName = null)
		{
			if (knownPids.Contains(procId)) return;

			using (
				var safeHandle = SafeProcessHandle.OpenProcess(
					(int)ProcessNameHelper.ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, (int)procId))
			{
				if (safeHandle.IsInvalid)
				{
					int errCode = Marshal.GetLastWin32Error();
					log.Warn("Unable to open process (" + errCode + "):" +
							 new System.ComponentModel.Win32Exception(errCode).Message);
					return;
				}
				ProcessNameHelper.TryQueryFullProcessImageName(safeHandle, out var path);
				lock (lockObject)
				{
					if (knownPaths.Contains(path)) return;
					knownPaths.Add(path);
					knownPids.Add(procId);
				}
				var versionInfo = FileVersionInfo.GetVersionInfo(path);
				var appVerInfo = new AppVersionInfo(optionalName ?? versionInfo.ProductName, versionInfo.ProductVersion);
				log.InfoFormat("{0} version: {1}, path: {2}", appVerInfo.Name, appVerInfo.Version, path);
				TelemetryHelper.RecordFeature("ApplicationVersion", JsonHelper.SerializeData(appVerInfo));
			}
		}
	}
}
