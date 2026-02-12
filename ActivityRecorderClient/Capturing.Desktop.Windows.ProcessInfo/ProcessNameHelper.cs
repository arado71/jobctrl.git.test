using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo
{
	static class ProcessNameHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly bool isVistaOrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;

		public static bool IsProcessRunning(int processId)
		{
			try
			{
				if (isVistaOrLater)
				{
					using (
						var safeHandle = SafeProcessHandle.OpenProcess(
							(int)ProcessAccessFlags.SYNCHRONIZE, false, processId))
					{
						if (!safeHandle.IsInvalid)
						{
							var ret = WaitForSingleObject(safeHandle.DangerousGetHandle(), 0);
							return ret == WAIT_TIMEOUT;
						}
					}
				}
				else
				{
					try
					{
						using (var process = Process.GetProcessById(processId))
						{
							return true;
						}

					}
					catch (ArgumentException)
					{
						// that's normal, process can't be found
					}
				}
			}
			catch (Exception ex)
			{
				log.Warn("Getting process info failed", ex);
			}
			return false;
		}

		public static bool TryGetProcessName(int processId, IntPtr hWnd, out string processName)
		{
			try
			{
				//casing is different for QueryFullProcessImageName so use it as a backup only atm.
				//299 Only part of a ReadProcessMemory or WriteProcessMemory request was completed
				//5 Access denied
				if (isVistaOrLater)
				{
					using (
						var safeHandle = SafeProcessHandle.OpenProcess(
							(int)ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, processId))
					{
						if (!safeHandle.IsInvalid)
						{
							var isSucc = TryQueryProcessFileName(safeHandle, out processName);
							if (isSucc && !string.IsNullOrEmpty(processName) && Path.GetFileName(processName).Equals("applicationframehost.exe", StringComparison.OrdinalIgnoreCase))
							{
								if (!TryGetUwpAppName(hWnd, processId, ref processName))
									processName = "";
								return true;
							}
							return isSucc;
						}
						int errCode = Marshal.GetLastWin32Error();
						log.Warn("Unable to open process (" + errCode + "):" +
								 new System.ComponentModel.Win32Exception(errCode).Message);
					}
				}
				else
				{
					using (var process = Process.GetProcessById(processId))
					{
						processName = process.MainModule.ModuleName;
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to get process name", ex);
			}
			processName = null;
			return false;
		}

		//http://msdn.microsoft.com/en-us/library/aa365247%28VS.85%29.aspx#maxpath
		private static readonly int[] sizes = new[] { 260, 1024, 32767 };
		public static bool TryQueryFullProcessImageName(SafeProcessHandle handle, out string path)
		{
			foreach (var size in sizes)
			{
				var currSize = size;
				var sb = new StringBuilder(currSize);
				if (!QueryFullProcessImageName(handle, 0, sb, ref currSize))
				{
					int errCode = Marshal.GetLastWin32Error();
					log.Debug("Unable to query process name (" + errCode + "):" + new System.ComponentModel.Win32Exception(errCode).Message);
					if (errCode == 122) continue; //ERROR_INSUFFICIENT_BUFFER
					break; //other error
				}
				path = sb.ToString();
				return true;
			}
			path = null;
			return false;
		}

		private static bool TryQueryProcessFileName(SafeProcessHandle handle, out string fileName)
		{
			try
			{
				string path;
				if (TryQueryFullProcessImageName(handle, out path))
				{
					fileName = Path.GetFileName(path);
					return true;
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in Win7 process name detection", ex);
			}
			fileName = null;
			return false;
		}

		private static readonly CachedDictionary<IntPtr, string> uwpNamesDict = new CachedDictionary<IntPtr, string>(TimeSpan.FromMinutes(5), true);
		private delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);
		private static bool TryGetUwpAppName(IntPtr hWnd, int processId, ref string path)
		{
			WINDOWINFO windowinfo = new WINDOWINFO();
			windowinfo.ownerpid = processId;
			windowinfo.childpid = -1;

			IntPtr pWindowinfo = Marshal.AllocHGlobal(Marshal.SizeOf(windowinfo));

			Marshal.StructureToPtr(windowinfo, pWindowinfo, false);

			EnumWindowProc lpEnumFunc = EnumChildWindowsCallback;
			EnumChildWindows(hWnd, lpEnumFunc, pWindowinfo);

			windowinfo = (WINDOWINFO)Marshal.PtrToStructure(pWindowinfo, typeof(WINDOWINFO));

			if (windowinfo.childpid < 0)
			{
				var isSucc = uwpNamesDict.TryGetValue(hWnd, out path);
				if (isSucc)
				{ // renew item when found
					uwpNamesDict.Set(hWnd, path);
				}
				return isSucc;
			}

			using (var proc = SafeProcessHandle.OpenProcess((int)ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, windowinfo.childpid))
			{
				if (proc == null || proc.IsClosed || proc.IsInvalid) return false;

				int capacity = 2000;
				StringBuilder sb = new StringBuilder(capacity);
				QueryFullProcessImageName(proc, 0, sb, ref capacity);

				Marshal.FreeHGlobal(pWindowinfo);

				path = Path.GetFileName(sb.ToString(0, capacity));
				uwpNamesDict.Set(hWnd, path);
				return true;
			}
		}

		private static bool EnumChildWindowsCallback(IntPtr hWnd, IntPtr lParam)
		{
			WINDOWINFO info = (WINDOWINFO)Marshal.PtrToStructure(lParam, typeof(WINDOWINFO));

			GetWindowThreadProcessId(hWnd, out int pID);

			if (pID != info.ownerpid)
				info.childpid = pID;

			Marshal.StructureToPtr(info, lParam, true);

			return true;
		}

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "WaitForSingleObject")]
		public static extern int WaitForSingleObject(IntPtr handle, int wait);
		public const int INFINITE = -1;
		public const int WAIT_ABANDONED = 0x80;
		public const int WAIT_OBJECT_0 = 0x00;
		public const int WAIT_TIMEOUT = 0x102;
		public const int WAIT_FAILED = -1;

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "QueryFullProcessImageName")]
		private static extern bool QueryFullProcessImageName(SafeProcessHandle hProcess, uint dwFlags, StringBuilder lpExeName, ref int lpdwSize);

		[DllImport("user32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		//http://msdn.microsoft.com/en-us/library/windows/desktop/ms684880(v=vs.85).aspx
		public enum ProcessAccessFlags
		{
			PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
			SYNCHRONIZE = 0x00100000,
		}

		private struct WINDOWINFO
		{
			public int ownerpid;
			public int childpid;
		}

	}
}
