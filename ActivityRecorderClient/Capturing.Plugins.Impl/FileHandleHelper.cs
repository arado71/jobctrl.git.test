using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using log4net;
using Microsoft.Win32.SafeHandles;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	//http://forum.sysinternals.com/topic18892.html
	//http://www.exploit-monday.com/2013/06/undocumented-ntquerysysteminformation.html
	/// <summary>
	/// Class for returning open files for a process id.
	/// </summary>
	public static class FileHandleHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static volatile byte handleTypeFile;

		public static IList<string> GetOpenFiles(int pid)
		{
			var result = new List<string>();
			var handleInfoSize = 0x10000;
			var handleInfo = IntPtr.Zero;
			SafeProcessHandle processHandle = null;

			try
			{
				processHandle = WinApi.OpenProcess(WinApi.ProcessAccessRights.PROCESS_DUP_HANDLE, false, pid);
				if (processHandle.IsInvalid) //Cannot open process
				{
					if (log.IsVerboseEnabled()) log.Verbose("Cannot open process: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
					return null;
				}

				handleInfo = Marshal.AllocHGlobal(handleInfoSize);
				WinApi.NT_STATUS ret;

				int returnLength;
				while ((ret = WinApi.NtQuerySystemInformation(WinApi.SYSTEM_INFORMATION_CLASS.SystemHandleInformation, handleInfo, handleInfoSize, out returnLength)) == WinApi.NT_STATUS.STATUS_INFO_LENGTH_MISMATCH)
				{
					handleInfoSize *= 2; //returnLength is not reliable here
					handleInfo = Marshal.ReAllocHGlobal(handleInfo, (IntPtr)handleInfoSize);
				}

				if (ret != WinApi.NT_STATUS.STATUS_SUCCESS) //NtQuerySystemInformation failed
				{
					if (log.IsVerboseEnabled()) log.Verbose("NtQuerySystemInformation failed: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
					return null;
				}

				long handleCount = IntPtr.Size == 4 ? Marshal.ReadInt32(handleInfo) : (int)Marshal.ReadInt64(handleInfo);
				long offset = IntPtr.Size;
				int size = Marshal.SizeOf(typeof(WinApi.SystemHandleEntry));
				log.VerboseFormat("Handle count: {0} Memory: {1}", handleCount, handleInfoSize);
				for (int i = 0; i < handleCount; i++)
				{
					var handleEntry = (WinApi.SystemHandleEntry)Marshal.PtrToStructure((IntPtr)((long)handleInfo + offset), typeof(WinApi.SystemHandleEntry));
					offset += size;

					if (handleEntry.OwnerProcessId != pid) continue;
					if (handleTypeFile != 0 && handleEntry.ObjectTypeNumber != handleTypeFile) continue; //if it's not a file handle

					WinApi.SafeObjectHandle handleDuplicate = null;
					var objectName = IntPtr.Zero;
					try
					{
						WinApi.DuplicateHandle(processHandle, (IntPtr)handleEntry.Handle, WinApi.GetCurrentProcess(), out handleDuplicate, 0, false, WinApi.DuplicateHandleOptions.NONE); //DUPLICATE_SAME_ACCESS
						if (handleDuplicate.IsInvalid) //Cannot duplicate handle
						{
							if (log.IsVerboseEnabled()) log.Verbose("Cannot duplicate handle: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
							continue;
						}

						if (handleTypeFile == 0 && !IsFileType(handleEntry, handleDuplicate)) continue;
						Debug.Assert(handleTypeFile != 0);

						WinApi.FileType fileType;
						if ((fileType = WinApi.GetFileType(handleDuplicate)) == WinApi.FileType.FileTypePipe) continue; //we cannot query namedpipe objects as they would hang
						if (fileType == WinApi.FileType.FileTypeUnknown)
						{
							var errCode = Marshal.GetLastWin32Error();
							if (errCode != 0)
							{
								if (log.IsVerboseEnabled()) log.Verbose("Cannot get file type: " + new Win32Exception(errCode).Message);
								continue;
							}
						}

						int objectNameLength = 1024;
						objectName = Marshal.AllocHGlobal(objectNameLength);
						if ((ret = WinApi.NtQueryObject(handleDuplicate, WinApi.OBJECT_INFORMATION_CLASS.ObjectNameInformation, objectName, objectNameLength, out returnLength)) == WinApi.NT_STATUS.STATUS_BUFFER_OVERFLOW)
						{
							objectNameLength = returnLength;
							objectName = Marshal.ReAllocHGlobal(objectName, (IntPtr)objectNameLength);
							ret = WinApi.NtQueryObject(handleDuplicate, WinApi.OBJECT_INFORMATION_CLASS.ObjectNameInformation, objectName, objectNameLength, out returnLength);
						}

						if (ret != WinApi.NT_STATUS.STATUS_SUCCESS) //Cannot query object
						{
							if (log.IsVerboseEnabled()) log.Verbose("Cannot query object: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
							continue;
						}

						var nameInfo = (WinApi.UNICODE_STRING)Marshal.PtrToStructure(objectName, typeof(WinApi.UNICODE_STRING));
						var name = Marshal.PtrToStringUni(nameInfo.Buffer, nameInfo.Length / 2);
						result.Add(DeviceNameHelper.GeFileNameFromDosDevice(name));
					}
					finally
					{
						if (objectName != IntPtr.Zero) Marshal.FreeHGlobal(objectName);
						if (handleDuplicate != null) handleDuplicate.Dispose();
					}
				}
			}
			finally
			{
				if (handleInfo != IntPtr.Zero) Marshal.FreeHGlobal(handleInfo);
				if (processHandle != null) processHandle.Dispose();
			}
			return result;
		}

		private static bool IsFileType(WinApi.SystemHandleEntry handleEntry, WinApi.SafeObjectHandle handleDuplicate)
		{
			if (handleTypeFile != 0) return handleEntry.ObjectTypeNumber == handleTypeFile;
			var typeInfo = IntPtr.Zero;
			try
			{
				int length = 200;
				typeInfo = Marshal.AllocHGlobal(length);
				var res = WinApi.NtQueryObject(handleDuplicate, WinApi.OBJECT_INFORMATION_CLASS.ObjectTypeInformation, typeInfo, length, out length);
				if (res == WinApi.NT_STATUS.STATUS_INFO_LENGTH_MISMATCH)
				{
					typeInfo = Marshal.ReAllocHGlobal(typeInfo, (IntPtr)length);
					res = WinApi.NtQueryObject(handleDuplicate, WinApi.OBJECT_INFORMATION_CLASS.ObjectTypeInformation, typeInfo, length, out length);
				}
				if (res == WinApi.NT_STATUS.STATUS_SUCCESS)
				{
					var typeEntry = (WinApi.OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure((IntPtr)((long)typeInfo), typeof(WinApi.OBJECT_TYPE_INFORMATION));
					if (typeEntry.Name.Length == 8) //File * 2
					{
						var name = Marshal.PtrToStringUni(typeEntry.Name.Buffer, typeEntry.Name.Length / 2);
						if (name == "File")
						{
							handleTypeFile = handleEntry.ObjectTypeNumber;
							Debug.Assert(handleTypeFile != 0);
							log.Info("File ObjectTypeNumber is " + handleTypeFile); //28 on win7 30 on win8 so can change...
							return true;
						}
					}
				}
				if (log.IsVerboseEnabled()) log.Verbose("Cannot get file type: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
				return false;

			}
			finally
			{
				if (typeInfo != IntPtr.Zero) Marshal.FreeHGlobal(typeInfo);
			}
		}

		/// <summary>
		/// Class for resolving Device names to Dos paths.
		/// </summary>
		/// <remarks>
		/// Lots of memory is wasted here, and we won't detect changes in device names.
		/// </remarks>
		private static class DeviceNameHelper
		{
			private const string networkDevicePrefix = "\\Device\\LanmanRedirector\\";
			private const string networkDevicePrefix2 = "\\Device\\Mup\\";
			private const int MAX_PATH = 260;

			private static Dictionary<string, string> deviceMap;

			public static string GeFileNameFromDosDevice(string deviceFileName)
			{
				string dosPath;
				return ConvertDevicePathToDosPath(deviceFileName, out dosPath) ? dosPath : deviceFileName;
			}

			private static bool ConvertDevicePathToDosPath(string devicePath, out string dosPath)
			{
				EnsureDeviceMap();
				int i = devicePath.Length;
				while (i > 0 && (i = devicePath.LastIndexOf('\\', i - 1)) != -1)
				{
					string drive;
					if (deviceMap.TryGetValue(devicePath.Substring(0, i), out drive))
					{
						dosPath = string.Concat(drive, devicePath.Substring(i));
						return dosPath.Length != 0;
					}
				}
				dosPath = string.Empty;
				return false;
			}

			private static void EnsureDeviceMap()
			{
				if (deviceMap == null)
				{
					var localDeviceMap = BuildDeviceMap();
					Interlocked.CompareExchange<Dictionary<string, string>>(ref deviceMap, localDeviceMap, null);
				}
			}

			private static Dictionary<string, string> BuildDeviceMap()
			{
				string[] logicalDrives = Environment.GetLogicalDrives();
				var localDeviceMap = new Dictionary<string, string>(logicalDrives.Length);
				var lpTargetPath = new StringBuilder(MAX_PATH);
				foreach (var drive in logicalDrives)
				{
					if (drive.Length < 2) continue;
					var lpDeviceName = drive.Substring(0, 2);
					WinApi.QueryDosDevice(lpDeviceName, lpTargetPath, MAX_PATH);
					localDeviceMap.Add(NormalizeDeviceName(lpTargetPath.ToString()), lpDeviceName);
				}
				localDeviceMap.Add(networkDevicePrefix.Substring(0, networkDevicePrefix.Length - 1), "\\");
				localDeviceMap.Add(networkDevicePrefix2.Substring(0, networkDevicePrefix2.Length - 1), "\\");
				return localDeviceMap;
			}

			private static string NormalizeDeviceName(string deviceName)
			{
				if (string.Compare(deviceName, 0, networkDevicePrefix, 0, networkDevicePrefix.Length, StringComparison.InvariantCulture) == 0)
				{
					var shareName = deviceName.Substring(deviceName.IndexOf('\\', networkDevicePrefix.Length) + 1);
					return string.Concat(networkDevicePrefix, shareName);
				}
				//do we need to handle networkDevicePrefix2 here ? (probably not)
				return deviceName;
			}
		}

		
	}
}
