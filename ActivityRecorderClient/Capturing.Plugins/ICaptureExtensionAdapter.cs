using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public interface ICaptureExtensionAdapter : IDisposable
	{
		PluginStartInfo CaptureExtensionSettings { get; }
		void SetCaptureExtensions(List<DesktopWindow> windowsInfo, Func<DesktopWindow, bool> shouldCaptureWindow);
		Dictionary<IntPtr, KeyValuePair<CaptureExtensionKey, string>[]> Capture(
			List<DesktopWindow> windowsInfo, Func<DesktopWindow, bool> shouldCaptureWindow);
		IEnumerable<KeyValuePair<string, string>> Capture(IntPtr handle, int pid, string processName);
		void SetParameter(string name, string value);
	}
}
