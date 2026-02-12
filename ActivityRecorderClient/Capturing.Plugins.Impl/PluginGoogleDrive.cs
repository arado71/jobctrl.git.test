using JobCTRL.Plugins;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;
using Tct.ActivityRecorderClient.Google;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	class PluginGoogleDrive : ICaptureExtension
	{
		public const string PluginId = "JobCTRL.GoogleDrive";
		public string Id => PluginId;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string KeyPath = "GoogleDocumentPath";
		public const string KeyMimeType = "GoogleDocumentType";

		public IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public void SetParameter(string name, string value)
		{
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyPath;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			Stopwatch sw = Stopwatch.StartNew();
			string path = null;
			string mimeType = null;
			string url;
			bool urlQueryResult = false;
			switch (processName)
			{
				case string pn when pn.Equals("firefox.exe", StringComparison.OrdinalIgnoreCase):
					urlQueryResult = UrlHelper.TryGetUrlFromWindow(hWnd, Browser.Firefox, out url);
					break;
				case string pn when pn.Equals("microsoftedge.exe", StringComparison.OrdinalIgnoreCase):
					urlQueryResult = UrlHelper.TryGetUrlFromWindow(hWnd, Browser.Edge, out url);
					break;
				case string pn when pn.Equals("chrome.exe", StringComparison.OrdinalIgnoreCase):
					urlQueryResult = UrlHelper.TryGetUrlFromWindow(hWnd, Browser.Chrome, out url);
					break;
				case string pn when pn.Equals("iexplore.exe", StringComparison.OrdinalIgnoreCase):
					urlQueryResult = UrlHelper.TryGetUrlFromWindow(hWnd, Browser.InternetExplorer, out url);
					break;
				default:
					yield break;
			}

			if (urlQueryResult && !string.IsNullOrEmpty(url))
			{
				try
				{
					var match = Regex.Match(url, "http[s]?:\\/\\/docs.google.com\\/[^\\/]+\\/d\\/([^\\/]+)\\/");
					if (match.Groups.Count < 2) yield break;
					var googleDriveId = match.Groups[1].Value;
					path = GoogleDrive.GetDatasFromFileId(googleDriveId, out mimeType);
				}
				catch (Exception e)
				{
					log.Debug("There was an error during the Google Drive path capture.", e);
				}
				finally
				{
					log.Verbose($"Capturing Google Drive path took {sw.Elapsed.TotalMilliseconds:0.##}");
				}
				if (path != null)
					yield return new KeyValuePair<string, string>(KeyPath, path);
				if(mimeType != null)
					yield return new KeyValuePair<string, string>(KeyMimeType, mimeType);
			}
		}
	}
}
