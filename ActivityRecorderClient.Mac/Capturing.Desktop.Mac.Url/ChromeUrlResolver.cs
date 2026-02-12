using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MonoMac.Foundation;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Url
{
	public class ChromeUrlResolver : UrlResolver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ChromeUrlResolver()
			: base (log)
		{
		}

		protected override bool TryResolveUrlForWindow(AXObject axWindow, out string url)
		{
			if (!IsBrowserWindow(axWindow))
			{
				url = null;
				return false;
			}
			url = axWindow.GetChildrenInRole("AXToolbar")
				.SelectMany(n => n.GetChildrenInRole("AXTextField"))
				.Select(n =>
			{
				AXError error;
				var rawUrl = n.GetStringValueForAttribute(AXAttribute.Value, out error);
				if (error != AXError.Success)
				{
					log.Error("Unable to get url: " + error);
					return null;
				}
				//chrome doesn't include http:// in urls so emulate it
				return GetFixedUrl(rawUrl);
			})
				.FirstOrDefault();
			return url != null;
		}

		private bool IsBrowserWindow(AXObject axWindow)
		{
			AXError error;
			var role = axWindow.GetStringValueForAttribute(AXAttribute.Role, out error);
			if (error != AXError.Success)
			{
				log.Error("Unable to get role for window: " + error);
				return false;
			}
			return role == "AXWindow";
		}

		protected override bool CanResolveUrl(DesktopWindow desktopWindow)
		{
			return (desktopWindow != null && desktopWindow.ProcessName == "Google Chrome.app");
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

