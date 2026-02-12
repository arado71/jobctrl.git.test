using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using log4net;
using Microsoft.Win32.SafeHandles;

namespace JcMon2.SystemAdapter
{
	public static class ProcessNameHelper
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ProcessNameHelper));
		private static readonly bool isVistaOrLater = true;

		public static bool IsProcessRunning(int processId)
		{
			try
			{
				if (isVistaOrLater)
				{
					using (
						var safeHandle = SafeProcessHandle.OpenProcess(
							(int)ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, processId))
					{
						if (!safeHandle.IsInvalid)
						{
							return true;
						}
					}
				}
				else
				{
					using (var process = Process.GetProcessById(processId))
					{
						return true;
					}
				}
			}
			catch (Exception ex)
			{
			}
			return false;
		}

		public static bool TryGetProcessName(int processId, out string processName)
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
							return TryQueryProcessFileName(safeHandle, out processName);
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
		private static bool TryQueryFullProcessImageName(SafeProcessHandle handle, out string path)
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
				log.Error("Unexpected error in Win7 process name detection", ex);
			}
			fileName = null;
			return false;
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern bool QueryFullProcessImageName(SafeProcessHandle hProcess, uint dwFlags, StringBuilder lpExeName, ref int lpdwSize);

		//http://msdn.microsoft.com/en-us/library/windows/desktop/ms684880(v=vs.85).aspx
		private enum ProcessAccessFlags
		{
			PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
		}

		[SuppressUnmanagedCodeSecurity]
		internal sealed class SafeObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			private SafeObjectHandle()
				: base(true)
			{ }

			internal SafeObjectHandle(IntPtr preexistingHandle, bool ownsHandle)
				: base(ownsHandle)
			{
				base.SetHandle(preexistingHandle);
			}

			protected override bool ReleaseHandle()
			{
				return WinApi.CloseHandle(base.handle);
			}
		}

		[SuppressUnmanagedCodeSecurity]
		internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			// Fields
			internal static SafeProcessHandle InvalidHandle = new SafeProcessHandle(IntPtr.Zero);

			// Methods
			internal SafeProcessHandle()
				: base(true)
			{
			}

			internal SafeProcessHandle(IntPtr handle)
				: base(true)
			{
				base.SetHandle(handle);
			}

			internal void InitialSetHandle(IntPtr h)
			{
				base.handle = h;
			}

			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal static extern SafeProcessHandle OpenProcess(int access, bool inherit, int processId);

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
			private static extern bool CloseHandle(IntPtr handle);

			protected override bool ReleaseHandle()
			{
				return CloseHandle(base.handle);
			}
		}
	}
}
