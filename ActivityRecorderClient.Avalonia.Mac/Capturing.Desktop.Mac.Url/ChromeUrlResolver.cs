using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Foundation;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Url
{
	public class ChromeUrlResolver : UrlResolver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ChromeUrlResolver()
			: base(log)
		{
		}

		protected override bool TryResolveUrlForWindow(AXObject axWindow, out string url)
		{
			if (!IsBrowserWindow(axWindow))
			{
				url = null;
				return false;
			}

			// Unfortunately this quicker method doesn't always work as the AXURL property sometimes 'stuck' when switching tabs (in Brave?) and has some delay on Chrome
			/*url = axWindow
					.GetChildrenInRole("AXGroup")
					.Select(n =>
					{
						var url = n.GetUrlStringForAttribute(AXAttribute.Url, out AXError error);
						if (error != AXError.Success)
						{
							return null;
						}
						return url;
					})
					.Where(n => n != null)
					.FirstOrDefault();

			if (url != null) return true;*/

			url = axWindow
					.GetDescendantsInRole("AXGroup")
					.SelectMany(n => n.GetChildrenInRole("AXToolbar"))
					.SelectMany(n => n.GetDescendantsInRole("AXGroup"))
					.SelectMany(n => n.GetChildrenInRole("AXTextField"))
					.Select(n =>
					{
						var rawUrl = n.GetStringValueForAttribute(AXAttribute.Value, out AXError error);
						if (error != AXError.Success)
						{
							log.Error("Unable to get url: " + error);
							return null;
						}
						//chrome doesn't include http:// in urls so emulate it
						return GetFixedUrl(rawUrl);
					})
					.Where(n => !string.IsNullOrEmpty(n))
					.FirstOrDefault();
			if (url != null) return true;

			// for pop-up window we don't have the link atm. so use this text
			url = axWindow
					.GetDescendantsInRole("AXGroup")
					.SelectMany(n => n.GetChildrenInRole("AXStaticText"))
					.Select(n =>
					{
						var rawUrl = n.GetStringValueForAttribute(AXAttribute.Value, out AXError error);
						if (error != AXError.Success)
						{
							log.Error("Unable to get url: " + error);
							return null;
						}
						//chrome doesn't include http:// in urls so emulate it
						return GetFixedUrl(rawUrl);
					})
					.Where(n => !string.IsNullOrEmpty(n))
					.FirstOrDefault();

			return url != null;
		}

		private bool IsBrowserWindow(AXObject axWindow)
		{
			AXError error;
			var subrole = axWindow.GetStringValueForAttribute(AXAttribute.Subrole, out error);
			if (error != AXError.Success)
			{
				log.Error("Unable to get subrole for window: " + error);
				return false;
			}
			return subrole == "AXStandardWindow";
		}

		protected override bool CanResolveUrl(DesktopWindow desktopWindow)
		{
			return (desktopWindow != null && (desktopWindow.ProcessName == "Google Chrome.app" || desktopWindow.ProcessName == "Brave Browser.app"));
		}

		internal static string GetFixedUrl(string rawUrl)
		{
			rawUrl = rawUrl == null ? null : rawUrl.Trim(); //trim because this is just a text from the location bar
			return (string.IsNullOrEmpty(rawUrl)
				|| rawUrl.StartsWith("about:")
				|| (rawUrl.Contains("://")) && Uri.CheckSchemeName(rawUrl.Split(new[] { "://" }, StringSplitOptions.None)[0])) //The '://' string can be in the qurey string and we still want to fix that url, so simple Contains is not enough
				? rawUrl
				: "http://" + rawUrl;
		}
	}
}
