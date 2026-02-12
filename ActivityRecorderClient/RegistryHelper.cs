using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Tct.ActivityRecorderClient
{
	//Helper class for reading registry key with different views
	//http://msdn.microsoft.com/en-us/library/windows/desktop/aa384253%28v=vs.85%29.aspx
	public static class RegistryHelper
	{
		public static object GetValueFromEitherView(RegistryHive hKey, string keyName, string altKeyName, string keyValue)
		{
			return RegistryHelper.GetValue(hKey, RegistryView.Registry32, keyName, altKeyName, keyValue)
				?? RegistryHelper.GetValue(hKey, RegistryView.Registry64, keyName, altKeyName, keyValue);
		}

		private static object GetValue(RegistryHive hKey, RegistryView view, string keyName, string altKeyName, string keyValue)
		{
#if NET4
			using (var baseKey = RegistryKey.OpenBaseKey(hKey, view))
			using (var key = baseKey.OpenSubKey(keyName))
			{
				return key != null ? key.GetValue(keyValue) : null;
			}
#else
			if (!Is64BitOperatingSystem && view == RegistryView.Registry64) view = RegistryView.Registry32;	//Fall back to 32bit view when there is no sense of 64 bit view (such as OpenBaseKey with NET4)
			if (IntPtr.Size == 4 && view == RegistryView.Registry64) throw new NotImplementedException("64 bit registry key was requested from 32 bit application under .NET framework 3.5");
			var keyNameToUse = (IntPtr.Size == 8 && view == RegistryView.Registry32) ? altKeyName : keyName;
			return Registry.GetValue(GetBaseKeyName(hKey) + @"\" + keyNameToUse, keyValue, null);
#endif
		}

#if !NET4
		public static readonly bool Is64BitOperatingSystem = GetIs64BitOperatingSystem();

		private static string GetBaseKeyName(RegistryHive hKey)
		{
			switch (hKey)
			{
				case RegistryHive.ClassesRoot:
					return "HKEY_CLASSES_ROOT";
				case RegistryHive.CurrentUser:
					return "HKEY_CURRENT_USER";
				case RegistryHive.LocalMachine:
					return "HKEY_LOCAL_MACHINE";
				case RegistryHive.Users:
					return "HKEY_USERS";
				case RegistryHive.PerformanceData:
					return "HKEY_PERFORMANCE_DATA";
				case RegistryHive.CurrentConfig:
					return "HKEY_CURRENT_CONFIG";
				case RegistryHive.DynData:
					return "HKEY_DYN_DATA";
				default:
					throw new ArgumentOutOfRangeException("hKey");
			}
		}

		//This should be placed in EnvironmentInfoService but this RegistryHelper.cs file is linked in OutlookInteropService so it can't be
		#region Is64BitOperatingSystem (IsWow64Process)

		/// <summary>
		/// The function determines whether the current operating system is a 
		/// 64-bit operating system.
		/// </summary>
		/// <returns>
		/// The function returns true if the operating system is 64-bit; 
		/// otherwise, it returns false.
		/// </returns>
		private static bool GetIs64BitOperatingSystem()
		{
			if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
			{
				return true;
			}
			else  // 32-bit programs run on both 32-bit and 64-bit Windows
			{
				// Detect whether the current process is a 32-bit process 
				// running on a 64-bit system.
				bool flag;
				return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
					IsWow64Process(GetCurrentProcess(), out flag)) && flag);
			}
		}

		/// <summary>
		/// The function determins whether a method exists in the export 
		/// table of a certain module.
		/// </summary>
		/// <param name="moduleName">The name of the module</param>
		/// <param name="methodName">The name of the method</param>
		/// <returns>
		/// The function returns true if the method specified by methodName 
		/// exists in the export table of the module specified by moduleName.
		/// </returns>
		static bool DoesWin32MethodExist(string moduleName, string methodName)
		{
			IntPtr moduleHandle = GetModuleHandle(moduleName);
			if (moduleHandle == IntPtr.Zero)
			{
				return false;
			}
			return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr GetModuleHandle(string moduleName);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		static extern IntPtr GetProcAddress(IntPtr hModule,
			[MarshalAs(UnmanagedType.LPStr)]string procName);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

		#endregion

#endif
	}

#if !NET4
	public enum RegistryView
	{
		Default,
		Registry32,
		Registry64,
	}
#endif

}
