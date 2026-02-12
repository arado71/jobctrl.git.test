using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.EmailStats
{
	public static class UrlFormatHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly string[] browserProcessNames = new[] { "firefox.exe", "iexplore.exe", "chrome.exe" };

		public static string GetNonNullUrlForBrowsers(string processName, string url, string nullUrlString)
		{
			if (browserProcessNames.Any(n => string.Equals(n, processName, StringComparison.OrdinalIgnoreCase)))
			{
				return url ?? nullUrlString;
			}
			return url;
		}

		public static string GetShortUrlFrom(string url)
		{
			if (url == null) return null;
			if (url.StartsWith("about:")) return url;
			if (url == "" || url == "http://" || url.Contains(' ')) return "";
			if (url == "[Censored]" || url == "Ismeretlen") return url; //special urls created in StatsDbHelper (don't throw expensive ex)
			try
			{
				var uri = new Uri(url);
				if (string.Equals(Uri.UriSchemeFile, uri.Scheme, StringComparison.OrdinalIgnoreCase))
				{
					return "file";
				}
				return uri.Host;
			}
			catch (Exception ex)
			{
				log.Debug("Unable to create Uri from url: " + url, ex);
				return url;
			}
		}

		private static readonly char[] separators = new[] { '?', '#' };
		public static string GetDbFriendlyUrl(string url)
		{
			if (url == null) return null;
			if (url == "" || url == "http://" || url.Contains(' ')) return ""; //chorme bugs
			if (url.StartsWith(Uri.UriSchemeHttp + Uri.SchemeDelimiter)
				|| url.StartsWith(Uri.UriSchemeHttps + Uri.SchemeDelimiter))
			{
				return url.Split(separators, 2)[0]; //truncate query and fragment
			}
			return url;
		}
	}
}
