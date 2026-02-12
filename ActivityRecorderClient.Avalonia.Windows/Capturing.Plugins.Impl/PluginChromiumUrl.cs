using JobCTRL.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public abstract class PluginChromiumUrl : ICaptureExtension
	{
		public abstract Browser Browser { get; }
		public abstract string Id { get; }
		public const string KeyUrl = "Url";

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyUrl;
		}

		public void SetParameter(string name, string value)
		{
			
		}

		public IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!string.Equals(processName, UrlHelper.GetProcessNameChromiumBrowser(Browser), StringComparison.OrdinalIgnoreCase)) yield break;

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId);

			string url;
			if (UrlHelper.TryGetUrlFromWindow(hWnd, Browser, out url))
			{
				yield return new KeyValuePair<string, string>(KeyUrl, url);
			}
		}
	}
}
