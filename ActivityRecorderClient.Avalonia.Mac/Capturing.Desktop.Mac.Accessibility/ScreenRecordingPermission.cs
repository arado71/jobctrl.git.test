using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility
{
	internal class ScreenRecordingPermission
	{
		[DllImport(LibraryMac.CoreGraphics.Path)]
		public static extern bool CGPreflightScreenCaptureAccess();

		[DllImport(LibraryMac.CoreGraphics.Path)]
		public static extern bool CGRequestScreenCaptureAccess();
	}
}
