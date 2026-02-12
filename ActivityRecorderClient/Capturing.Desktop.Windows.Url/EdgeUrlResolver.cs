using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Edge;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public class EdgeUrlResolver : IUrlResolver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly object thisLock = new object();
		[ThreadStatic]
		private static Dictionary<IntPtr, Tuple<AutomationElement, int>> cachedUrlElementDict; //since searching for AutomationElements can be slow we have to cache them
		private static bool isExtensionGetUrlLogged = false;

		public bool TryGetUrl(IntPtr hWnd, out string url)
		{
			if (TryGetUrlWithAutomation(hWnd, out url)) return true;
			return false;
			// extension based capture disabled due to stability problems:
			// return TryGetUrlFromExtension(out url); 
		}

		private bool TryGetUrlWithAutomation(IntPtr hWnd, out string url)
		{
			if (cachedUrlElementDict == null)
			{
				cachedUrlElementDict = new Dictionary<IntPtr, Tuple<AutomationElement, int>>();
			}
			var threshold = Environment.TickCount - 3 * 60000; // older than 3 mins
			foreach (var hwnd2remove in cachedUrlElementDict.Where(c => c.Value.Item2 < threshold).Select(c => c.Key).ToList())
			{
				cachedUrlElementDict.Remove(hwnd2remove);
			}

			var result = false;
			try
			{
				DebugEx.EnsureSta(); //FindFirst/TreeWalker leaks memory in MTA
				AutomationElement cachedUrlElement;
				if (cachedUrlElementDict.TryGetValue(hWnd, out var item))
				{
					cachedUrlElement = item.Item1;
					if (cachedUrlElement == null)
					{
						url = null;
						return false;
					}
					try
					{
						result = TryGetUrlFromElement(cachedUrlElement, out url);
						cachedUrlElementDict[hWnd] = new Tuple<AutomationElement, int>(cachedUrlElement, Environment.TickCount); //refreshing timestamp
						if (result) return true;
						var enabled = cachedUrlElement.Current.IsEnabled; //is the cached value still valid?
						return false;
					}
					catch (ElementNotAvailableException)
					{
						cachedUrlElementDict.Remove(hWnd);
					}
				}
				url = null;
				var element = AutomationElement.FromHandle(hWnd);
				if (element == null) return false;

				Condition editTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);

				var specificChild = element.GetChildByIndex(1);
				if (specificChild != null && specificChild.Current.ControlType.Id == ControlType.Window.Id && specificChild.Current.Name == "Microsoft Edge")
				{
					cachedUrlElement = specificChild.FindFirst(TreeScope.Children, editTypeCond);
					if (cachedUrlElement == null) return false;
					cachedUrlElementDict.Add(hWnd, new Tuple<AutomationElement, int>(cachedUrlElement, Environment.TickCount));
					result = TryGetUrlFromElement(cachedUrlElement, out url);
					if (result && isExtensionGetUrlLogged)
					{
						isExtensionGetUrlLogged = false;
						log.Debug("URL successfully captured with regular method");
					}
					return result;
				}
				return false;
			}
			catch (Exception ex)
			{
				url = null;
				log.DebugFormat("Unable to get url hWnd {0}\n{1}", hWnd, ex);
				return false;
			}
		}

		private static bool TryGetUrlFromExtension(out string url)
		{
			try
			{
				if (!isExtensionGetUrlLogged)
				{
					log.Debug("Trying to get url with DOMCapture Extension");
					isExtensionGetUrlLogged = true;
				}
				url = EdgeProxy.ExecScript(EdgeProxy.ExtensionCommand.GetActiveTabUrl);
				return true;
			}
			catch (Exception ex)
			{
				url = null;
				log.Debug("Error in getting url with DOMCapture Extension: ", ex);
				return false;
			}
		}

		private static bool TryGetUrlFromElement(AutomationElement element, out string url)
		{
			var result = TryGetValueFromElement(element, log, out url);
			if (url != null && !url.Contains("://"))
				url = "http://" + url; // hax for rules
			return result;
		}

		private static bool TryGetValueFromElement(AutomationElement element, ILog logOvr, out string value)
		{
			if (element == null || (bool)element.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) //don't return url if we have focus (as it can change rapidly when typing)
			{
				value = null;
				return false;
			}
			return AutomationHelper.TryGetValueFromElement(element, logOvr, out value);
		}

	}
}
