using System;
using System.Runtime.InteropServices;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility
{
	public static class CFStringHelper
	{
		public static string GetString(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;
			//assume handle is retained
			int len = CFStringGetLength(handle);
			var data = new char[len];
			for (int i = 0; i < len; i++)
			{
				data[i] = CFStringGetCharacterAtIndex(handle, i);
			}
			return new string(data, 0, len);
		}

		public static bool StrEquals(IntPtr handle, string str)
		{
			//assume handle is retained
			if (str == null)
				return handle == IntPtr.Zero;
			if (handle == IntPtr.Zero)
				return false;
			var natLen = CFStringGetLength(handle);
			if (str.Length != natLen)
				return false; //early out
			for (int i = 0; i < natLen; i++)
			{
				if (str[i] != CFStringGetCharacterAtIndex(handle, i))
					return false;
			}
			return true;
		}

		[DllImport (LibraryMac.CoreFundation.Path, CharSet=CharSet.Unicode)]
		private extern static char CFStringGetCharacterAtIndex(IntPtr handle, int p);

		[DllImport (LibraryMac.CoreFundation.Path)]
		private extern static int CFStringGetLength(IntPtr handle);
	}

}

