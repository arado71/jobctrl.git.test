using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility;
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Url
{
	public class SafariUrlResolver : UrlResolver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public SafariUrlResolver()
			: base (log)
		{
		}

		protected override bool TryResolveUrlForWindow(AXObject axWindow, out string url)
		{
			url = axWindow.GetChildrenInRole("AXGroup")
				.SelectMany(n => n.GetChildrenInRole("AXGroup"))
				.SelectMany(n => n.GetChildrenInRole("AXGroup"))
				.SelectMany(n => n.GetChildrenInRole("AXScrollArea"))
				.SelectMany(n => n.GetChildrenInRole("AXWebArea"))
				.Select(n =>
			{
				AXError error;
				using (var axUrl = n.GetAttribute(AXAttribute.Url, out error))
				{
					if (error != AXError.Success)
					{
						log.Error("Unable to get url: " + error);
						return null;
					}
					using (var nsUrl = new NSUrl(axUrl.Handle))
					{
						return nsUrl.ToString();
					}
				}
			})
				.FirstOrDefault();
			return url != null;
		}

		protected override bool CanResolveUrl(DesktopWindow desktopWindow)
		{
			return (desktopWindow != null && desktopWindow.ProcessName == "Safari.app");
		}
	}
}

