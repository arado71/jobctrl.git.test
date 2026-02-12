using System;
using System.Collections.Generic;
using System.Linq;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginFirefoxUrl : ICaptureExtension
	{
		public const string PluginId = "Internal.FirefoxUrl";
		public const string KeyUrl = "Url";

		public string Id
		{
			get { return PluginId; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public void SetParameter(string name, string value)
		{
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyUrl;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!string.Equals(processName, "firefox.exe", StringComparison.OrdinalIgnoreCase)) yield break;

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId);

			string url;
			if (UrlHelper.TryGetUrlFromWindow(hWnd, Browser.Firefox, out url))
			{
				yield return new KeyValuePair<string, string>(KeyUrl, url);
			}
		}
	}
}
