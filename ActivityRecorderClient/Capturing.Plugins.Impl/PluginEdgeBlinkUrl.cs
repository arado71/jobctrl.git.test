using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginEdgeBlinkUrl : ICaptureExtension
	{
		public const string PluginId = "Internal.EdgeBlinkUrl";
		public const string KeyUrl = "Url";

		public string Id => PluginId;

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
			if (!string.Equals(processName, "msedge.exe", StringComparison.OrdinalIgnoreCase)) yield break;

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId);

			if (UrlHelper.TryGetUrlFromWindow(hWnd, Browser.EdgeBlink, out var url))
			{
				yield return new KeyValuePair<string, string>(KeyUrl, url);
			}
		}
	}
}
