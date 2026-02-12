using System;
using System.Collections.Generic;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Firefox
{
	public interface IFirefoxProxy : IDisposable
	{
		Dictionary<string, string> Capture(IntPtr hWnd, List<DomSettings> domCaptureSettings);
	}
}