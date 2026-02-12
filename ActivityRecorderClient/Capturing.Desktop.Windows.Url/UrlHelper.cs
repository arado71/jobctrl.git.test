using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	/// <summary>
	/// Thread-safe class for resolving urls from different browsers.
	/// </summary>
	public static class UrlHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly IUrlResolver firefoxResolver = new FirefoxUrlResolver();
		private static readonly IUrlResolver iExplorerResolver = new InternetExplorerUrlResolver();
		private static readonly IUrlResolver edgeResolver = new EdgeUrlResolver();
		private static readonly IUrlResolver blinkResolver = new EdgeBlinkUrlResolver();
		private static readonly Dictionary<Browser, ChromiumUrlResolverBase> chromiumResolvers = new Dictionary<Browser, ChromiumUrlResolverBase>
		{
			{ Browser.Brave, new BraveUrlResolver() },
			{ Browser.Chrome, new ChromeUrlResolver() },
			{ Browser.Opera, new OperaUrlResolver() },
			{ Browser.Vivaldi, new VivaldiUrlResolver() },
			{ Browser.Dragon, new DragonUrlResolver() },
		};
		private static readonly int maxClassNameLength;
		private static readonly CachedDictionary<Tuple<IntPtr, string>, string> urlCache = new CachedDictionary<Tuple<IntPtr, string>, string>(TimeSpan.FromMinutes(1), true);

		private static readonly IList<string> classNameExceptions = new List<string>()
		{
			"tooltips_class32",
			"SysShadow",
			"SysDragImage",
			"MozillaDropShadowWindowClass",
			"MozillaDialogClass"
		}
		.AsReadOnly(); //list is not modified so it is safe to enumerate without locks

		static UrlHelper()
		{
			maxClassNameLength = classNameExceptions.Select(n => n.Length).Max();
		}

		public static bool TryGetUrlFromWindow(IntPtr hWnd, Browser browser, out string url)
		{
			DebugEx.EnsureSta();
			try
			{
				var title = WindowTextHelper.GetWindowText(hWnd); // TODO: title should be served by caller
				var key = new Tuple<IntPtr, string>(hWnd, title);
				lock (urlCache)
				{
					if (urlCache.TryGetValue(key, out url)) return true;
				}
				var className = WindowTextHelper.GetClassName(hWnd);
				if (!classNameExceptions.Contains(className)) //don't try to resolve tooltips or any other window than regular browser window
				{
					var result = false;
					switch (browser)
					{
						case Browser.Firefox:
							result = firefoxResolver.TryGetUrl(hWnd, out url);
							break;
						case Browser.InternetExplorer:
							result = iExplorerResolver.TryGetUrl(hWnd, out url);
							break;
						case Browser.Edge:
							if (!title.EndsWith("Microsoft Edge")) break;
							result = edgeResolver.TryGetUrl(hWnd, out url);
							break;
						case Browser.EdgeBlink:
							result = blinkResolver.TryGetUrl(hWnd, out url);
							break;
					}
					if (!result && chromiumResolvers.TryGetValue(browser, out var resolver))
					{
						if (string.IsNullOrEmpty(title))
						{
							url = null;
							result = true;
						}
						else
						{
							result = resolver.TryGetUrl(hWnd, out url);
						}
					}
					if (result)
					{
						lock (urlCache)
						{
							urlCache.Set(key, url);
						}
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Unexpected error in TryGetUrlFromWindow", ex);
			}
			url = null;
			return false;
		}

		public static Browser GetBrowserFromProcessName(string processName)
		{
			if (string.Equals("firefox.exe", processName, StringComparison.OrdinalIgnoreCase))
			{
				return Browser.Firefox;
			}
			else if (string.Equals("iexplore.exe", processName, StringComparison.OrdinalIgnoreCase))
			{
				return Browser.InternetExplorer;
			}
			else if (string.Equals("microsoftedge.exe", processName, StringComparison.OrdinalIgnoreCase))
			{
				return Browser.Edge;
			}
			else if (string.Equals("msedge.exe", processName, StringComparison.OrdinalIgnoreCase))
			{
				return Browser.EdgeBlink;
			}
			else
			{
				foreach (var urlResolver in chromiumResolvers.Values)
				{
					if(string.Equals(urlResolver.ProcessName, processName, StringComparison.OrdinalIgnoreCase))
					{
						return urlResolver.Browser;
					}
				}
				return Browser.Unknown;
			}
		}

		public static string GetProcessNameChromiumBrowser(Browser browser)
		{
			if(chromiumResolvers.TryGetValue(browser, out var resolver))
			{
				return resolver.ProcessName;
			}
			throw new ArgumentException("Browser is not a chromium browser.");
		}
	}
}
