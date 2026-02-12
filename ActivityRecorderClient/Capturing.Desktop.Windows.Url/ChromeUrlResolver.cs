using Accessibility;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public class ChromeUrlResolver : ChromiumUrlResolverBase
	{
		public override Browser Browser => Browser.Chrome;
		public override string ProcessName => "chrome.exe";
		private const string AutomationEnabledFeatureName = "ChromeAutomationUrl";
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private bool isAccessibilityCaptureEnabled = true;
		private bool isCaptureKindAnnounced = false;
		private int retryIAccessibilityCounter = 0;
		private int accessibilityNotWorkingCounter = 0;
		private static bool isExtensionGetUrlLogged = false;
		private readonly object thisLock = new object();

		public ChromeUrlResolver(): base(log) { }

		public override bool TryGetUrl(IntPtr hWnd, out string url)
		{
			string rawUrl;
			if (!isAccessibilityCaptureEnabled)
			{
				if (++retryIAccessibilityCounter > 10)
				{
					retryIAccessibilityCounter = 0;
					isAccessibilityCaptureEnabled = true;
					isCaptureKindAnnounced = false;
				}
				if (TryGetUrlWithAutomation(hWnd, out rawUrl))
				{
					url = GetFixedUrl(rawUrl);
					return true;
				}
				url = null;
				return false;
			}
			lock (thisLock)
			{
				if (!isCaptureKindAnnounced)
				{
					log.Debug("Trying to capture URL with accessibility...");
					isCaptureKindAnnounced = true;
				}
				try
				{
					if (TryGetUrlWithAccessibility(hWnd, out rawUrl))
					{
						accessibilityNotWorkingCounter = 0;
						//chrome doesn't include http:// in urls so emulate it
						url = GetFixedUrl(rawUrl);
						if (isExtensionGetUrlLogged)
						{
							isExtensionGetUrlLogged = false;
							log.Debug("URL successfully captured with regular method");
						}
						return true;
					}
				}
				catch (Exception ex)
				{
					rawUrl = null;
					if (FeatureSwitches.IsEnabled(AutomationEnabledFeatureName) && ++accessibilityNotWorkingCounter > 3)
					{
						isAccessibilityCaptureEnabled = false;
						isCaptureKindAnnounced = false;

					}
				}
				//else
				//{
				//	log.Debug("Cannot get the url with Accessibility.");
				//}

				url = null;
				return false; //TryGetUrlFromExtension(out url);
			}
		}


		#region deprecated_methods
		/*
		 * From 2019.01.09 we try to use Accessibility instead of UIAutomation. These are the deprecated Automation methods.
		 */
		//private static bool TryGetUrlOld(IntPtr hWnd, out string rawUrl)
		//{
		//	//this is not working if there is more windows with the same class
		//	//to reproduce: hide toolbar then open url popup then show the toolbar
		//	//var child = EnumChildWindowsHelper.GetFirstChildWindowInfo(hWnd, n => n.ClassName == "Chrome_AutocompleteEditView" || n.ClassName == "Chrome_OmniboxView");
		//	//this is not good either it seems that the order is undefined
		//	//var child = EnumChildWindowsHelper.GetChildWindowInfo(hWnd).Where(n => n.ClassName == "Chrome_AutocompleteEditView" || n.ClassName == "Chrome_OmniboxView").LastOrDefault();

		//	//FindWindowEx is better here... (because we are looking for a direct child window)
		//	var childPtr = FindWindowEx(hWnd, IntPtr.Zero, "Chrome_OmniboxView", IntPtr.Zero);
		//	if (childPtr != IntPtr.Zero)
		//	{
		//		if (AutomationHelper.TryGetValueFromHandle(childPtr, log, out rawUrl))
		//		{
		//			return true;
		//		}
		//	}
		//	rawUrl = null;
		//	return false;
		//}

		//from v29 we no longer have Omnibox window
		[ThreadStatic] private static CachedDictionary<IntPtr, AutomationElement> cachedUrlElementDictUia; //since searching for AutomationElements can be slow we have to cache them
		[ThreadStatic] private static CachedDictionary<KeyValuePair<AutomationElement, string>, string> cachedUrlTextUia; // caching previous url value for focused editor
		private static readonly Condition editTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
		private static readonly Condition custOrPaneTypeCond = new OrCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom), new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane));
		private static readonly Condition toolBarOrCustTypeCond = new OrCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar), new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));
		private static readonly Condition textTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text);
		private static readonly int[] defaultPath = new int[5] { 1, 1, 0, 0, 0 };
		private readonly int[] path = defaultPath.ToArray();
		private readonly int[] limits = new int[5];
		private bool isPathSuccessful = true;

		private bool TryGetUrlWithAutomation(IntPtr hWnd, out string url)
		{
			var d = 0;
			var result = false;
			string title;
			try
			{
				DebugEx.EnsureSta(); //FindFirst/TreeWalker leaks memory in MTA
				if (cachedUrlElementDictUia == null)
				{
					cachedUrlElementDictUia = new CachedDictionary<IntPtr, AutomationElement>(TimeSpan.FromMinutes(5), true);
					cachedUrlTextUia = new CachedDictionary<KeyValuePair<AutomationElement, string>, string>(TimeSpan.FromMinutes(5), true);
				}
				title = WindowTextHelper.GetWindowText(hWnd); // lacks performance, maybe passed from desktopcapture?
				AutomationElement cachedUrlElement;
				if (cachedUrlElementDictUia.TryGetValue(hWnd, out cachedUrlElement))
				{
					try
					{
						d = 1;
						var key = new KeyValuePair<AutomationElement, string>(cachedUrlElement, title);
						result = TryGetValueFromElement(cachedUrlElement, log, out url);
						if (!result)
						{
							if (url == "")
							{
								cachedUrlTextUia.Remove(key);
								result = true;
								return true;
							}
							if (!cachedUrlTextUia.TryGetValue(key, out url))
								return false;
							result = true;
						}
						cachedUrlTextUia.Set(key, url);
						return true;
					}
					catch (ElementNotAvailableException)
					{
						cachedUrlElementDict.Remove(hWnd);
					}
				}
				if (!isCaptureKindAnnounced)
				{
					log.Debug("Trying to capture URL with automation...");
					isCaptureKindAnnounced = true;
				}
				url = null;
				d = 2;
				var element = AutomationElement.FromHandle(hWnd);
				if (element == null) throw new Exception("no main window");

				//cachedUrlElement = element.FindFirst(TreeScope.Descendants, editTypeCond); //this takes 1200-1300 ms on my comp while the below method took 170-250ms
				//var walker = new TreeWalker(editTypeCond);
				//cachedUrlElement = walker.GetFirstChild(element);
				var childs = element.FindAll(TreeScope.Children, custOrPaneTypeCond);
				d = 3;
				limits[0] = childs.Count;
				var specificChild = childs[path[0]];
				childs = specificChild.FindAll(TreeScope.Children, Condition.TrueCondition);
				d = 4;
				limits[1] = childs.Count;
				specificChild = childs[path[1]];
				childs = specificChild.FindAll(TreeScope.Children, custOrPaneTypeCond);
				d = 5;

				limits[2] = childs.Count;
				specificChild = childs[path[2]];
				childs = specificChild.FindAll(TreeScope.Children, toolBarOrCustTypeCond);
				if (childs.Count == 0)
				{
					childs = specificChild.FindAll(TreeScope.Children, custOrPaneTypeCond);
					specificChild = childs[0];
					childs = specificChild.FindAll(TreeScope.Children, toolBarOrCustTypeCond);
				}
				d = 6;
				limits[3] = childs.Count;
				specificChild = childs[path[3]];
				childs = specificChild.FindAll(TreeScope.Children, custOrPaneTypeCond);
				d = 7;
				limits[4] = childs.Count;
				specificChild = childs[path[4]];
				cachedUrlElement = specificChild.FindFirst(TreeScope.Children, editTypeCond);
				if (cachedUrlElement == null)
				{
					d = 8;
					//we can have a popups with readonly url
					cachedUrlElement = specificChild.FindFirst(TreeScope.Children, textTypeCond);
				}

				if (cachedUrlElement == null) throw new Exception("no url element");
				d = 9;
				cachedUrlElementDictUia.Add(hWnd, cachedUrlElement);
				result = TryGetValueFromElement(cachedUrlElement, log, out url);
				if (result)
				{
					var key = new KeyValuePair<AutomationElement, string>(cachedUrlElement, title);
					cachedUrlTextUia.Set(key, url);
					if (!isPathSuccessful && url != null)
					{
						log.Debug($"Path is successful recognized ({string.Join("->", path)})");
						isPathSuccessful = true;
					}
				}
				if (url == "")
				{
					result = true;
				}
				return result;
			}
			catch (Exception ex)
			{
				log.Debug($"Couldn't get URL, method ({d}) path ({string.Join("->", path)})", ex);
				isPathSuccessful = false;
				if (d > 7) d = 7;
				for (var idx = d - 3; idx >= 0; idx--)
				{
					if (d != 3 + idx) break;
					path[idx]++;
					if (path[idx] < limits[idx]) continue;
					path[idx] = 0;
					d = 2 + idx;
				}

				if (path.SequenceEqual(defaultPath))
				{
					isAccessibilityCaptureEnabled = true;
					isCaptureKindAnnounced = false;
				}
				url = null;
				return false;
			}
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
		#endregion

	}
}
