using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Desktop
{
	public class DesktopCapturedEventArgs : EventArgs
	{
		public DesktopCapture DesktopCapture { get; private set; }
		public bool ShouldSendCapture { get; private set; }
		public bool IsWorking { get; private set; }

		public DesktopCapturedEventArgs(DesktopCapture desktopCapture, bool shouldSendCapture, bool isWorking)
		{
			DesktopCapture = desktopCapture;
			ShouldSendCapture = shouldSendCapture;
			IsWorking = isWorking;
		}
	}
}
