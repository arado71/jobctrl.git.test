using System;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Desktop
{
	public interface IDesktopCaptureService : IDisposable
	{
		DesktopCapture GetDesktopCapture(bool takeScreenShot);
	}
}
