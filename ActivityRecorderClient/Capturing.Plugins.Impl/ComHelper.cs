using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public static class ComHelper
	{
		public static void FinalRelease(ref object comObject)
		{
			if (comObject == null || !Marshal.IsComObject(comObject)) return;
			Marshal.FinalReleaseComObject(comObject);
			comObject = null;
		}

		public static void Release(ref object comObject)
		{
			if (comObject == null || !Marshal.IsComObject(comObject)) return;
			Marshal.ReleaseComObject(comObject);
			comObject = null;
		}

		public static void FinalRelease<T>(ref T comObject) where T : class
		{
			if (comObject == null || !Marshal.IsComObject(comObject)) return;
			Marshal.FinalReleaseComObject(comObject);
			comObject = null;
		}

		public static void Release<T>(ref T comObject) where T : class
		{
			if (comObject == null || !Marshal.IsComObject(comObject)) return;
			Marshal.ReleaseComObject(comObject);
			comObject = null;
		}
	}
}
