using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public static class Extensions
	{
		public static DesktopWindow GetActiveWindow(this DesktopCapture capture)
		{
			return capture != null && capture.DesktopWindows != null
				? capture.DesktopWindows.Where(n => n.IsActive).FirstOrDefault()
				: null;
		}

		public static IEnumerable<DesktopWindow> GetDesktopWindowsNotNull(this DesktopCapture capture)
		{
			return capture != null && capture.DesktopWindows != null
				? capture.DesktopWindows
				: Enumerable.Empty<DesktopWindow>();
		}
	}
}
