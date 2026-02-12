using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using Accessibility;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Firefox;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	//http://majordan.net/adobe/msaa_documentation/langref/flash/accessibility/constants.html
	public class FirefoxUrlResolver : IUrlResolver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly object thisLock = new object();

		public bool TryGetUrl(IntPtr hWnd, out string url)
		{
			var style = (long)AccessibilityHelper.GetWindowLong(hWnd, AccessibilityHelper.GWL_STYLE);
			var isPopup = (style & AccessibilityHelper.WS_POPUP) == AccessibilityHelper.WS_POPUP && ((style & AccessibilityHelper.WS_MINIMIZEBOX) == 0 || (style & AccessibilityHelper.WS_MAXIMIZEBOX) == 0);
			// workaround to avoid shadow firefox windows
			if (isPopup)
			{
				url = null; 
				return false;
			}
			int tries = 3;
			bool allOk;
			lock (thisLock)
			{
				do
				{
					allOk = TryGetUrlImpl(hWnd, tries == 1, out url);
				} while (--tries > 0 && !allOk);
			}
			if (allOk)
			{
				if (isExtensionGetUrlLogged)
				{
					isExtensionGetUrlLogged = false;
					log.Debug("URL successfully captured with regular method");
				}
				return true;
			}
			return TryGetUrlFromExtension(out url);
		}

		private static bool workedOnce;
		private static bool isAccNavigateNotImpl;
		private static bool isUIAFallbackNeeded;
		private static bool isAccessibleDisabled; // capturing with accessible will be disabled permanently because it leaks memory if this feature disabled in ff
		private static int lastCheck = 0;
		private static bool isExtensionGetUrlLogged = false;
		private static bool TryGetUrlImpl(IntPtr hWnd, bool logging, out string url)
		{
			if (lastCheck > 0 && Environment.TickCount - lastCheck > 5 * 60000)
			{
				isAccessibleDisabled = isUIAFallbackNeeded = isAccNavigateNotImpl = false;
				lastCheck = 0;
			}
			var currMethod = "GetUrlWithAutomation";
			try
			{
				if (isUIAFallbackNeeded && TryGetUrlWithAutomation(hWnd, logging, out url)) return true;
			}
			catch (Exception ex)
			{
				if (logging && log.IsDebugEnabled)
				{
					log.Debug("Error while fetching the url with UIAutomation", ex);
				}
			} 

			currMethod = "GetIAccessibleFromWindow";
			try
			{
				var accClient = AccessibilityHelper.GetIAccessibleFromWindow(hWnd, AccessibilityHelper.ObjId.CLIENT); //throws COMException (yes it does...)
				if (accClient != null) //todo this needs some research, after some time we always get null obj (E_FAIL) only ff restart solves the problem
				{
					if (!isAccNavigateNotImpl && !isAccessibleDisabled)
						try
						{
							//todo this needs some research, it seems that after some time we always receive ArgumentException even after restarting ff, only jc restart solves the problem
							currMethod = "accClient.accNavigate";
							var accContent = (IAccessible) accClient.accNavigate((int) AccessibilityHelper.NavRelation.EMBEDS, 0); //throws ArgumentException
							currMethod = "accContent.accValue";
							url = accContent.accValue[0]; //throws COMException
							workedOnce = workedOnce || (url != null & url != "about:blank"); //Don't use those slower versions if v0 version has worked already
						}
						catch (NotImplementedException)
						{
							log.Warn("accClient.accNavigate not implemented, using another method");
							isAccNavigateNotImpl = true;
							lastCheck = Environment.TickCount;
							url = null;
						}
						catch (NullReferenceException ex)
						{
							if (ex.Source != "Accessibility") throw;
							log.Warn("Accessibility disabled in Firefox, using another method");
							isAccessibleDisabled = true;
							lastCheck = Environment.TickCount;
							if (TryGetUrlWithAutomation(hWnd, logging, out url))
							{
								isUIAFallbackNeeded = true;
								return true;
							}
						}
					else url = null;
					if (!workedOnce && !isAccessibleDisabled) //addons like ColorZilla ("about:blank") and FireBug (null) used to mess up detection, so try an other method
					{
						var res = TryGetUrlImplAccTreeV3(accClient, ref url, out currMethod); //this works for ff v33
						if (!res)
						{
							if (logging) log.Debug("Cannot get " + currMethod);
							res = TryGetUrlImplAccTree(accClient, ref url, out currMethod); //this is slower and may change with newer versions
							if (!res)
							{
								if (logging) log.Debug("Cannot get " + currMethod);
								res = TryGetUrlImplAccTreeV2(accClient, ref url, out currMethod); //this works for me atm. (ff v17) but ColorZilla and FireBug don't cause any issues anymore (at least for me) so use V1 first...
								if (!res && logging)
								{
									log.Debug("Cannot get " + currMethod);
								}
							}
						}
						return res;
					}
					if(!string.IsNullOrEmpty(url))return true;
				}
				//if (logging) log.Debug("Cannot get IAccessible from handle");
			}
			catch (Exception ex)
			{
				if (ex.GetType() == typeof(COMException) || ex.GetType() == typeof(ArgumentException) || ex.GetType() == typeof(NotImplementedException))
				{
					try
					{
						if (TryGetUrlWithAutomation(hWnd, logging, out url))
						{
							isUIAFallbackNeeded = true;
							return true;
						}
						if (logging && log.IsDebugEnabled)
						{
							log.Debug("Error while fetching the url (" + currMethod + ")", ex);
						}
						Debug.Print(ex.Message);
					}
					catch (Exception ex1)
					{
						if (logging && log.IsDebugEnabled)
						{
							log.Debug("Error while fetching the url with UIAutomation", ex1);
						}
						Debug.Print(ex1.Message);
					}
				}
				else
				{
					if (logging) log.Error("Unknown error while fetching the url (" + currMethod + ")", ex);
					Debug.Fail(ex.Message);
				}
			}

			url = null;
			return false;
		}

		private static bool TryGetUrlFromExtension(out string url)
		{
			try
			{
				if (!isExtensionGetUrlLogged)
				{
					log.Info("Trying to get url with DOMCapture Extension");
					isExtensionGetUrlLogged = true;
				}
				url = FirefoxProxy.ExecScript(FirefoxProxy.ExtensionCommand.GetActiveTabUrl);
				if (!string.IsNullOrEmpty(url))
					return true;
				return false;
			}
			catch (Exception ex)
			{
				log.Debug("Error in getting url with DOMCapture Extension: ", ex);
			}

			url = null;
			return false;
		}

		[ThreadStatic]
		private static CachedDictionary<IntPtr, AutomationElement> cachedUrlElementDict; //since searching for AutomationElements can be slow we have to cache them
		private static readonly PropertyCondition toolbarCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar);
		private static readonly PropertyCondition comboCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox);
		private static readonly PropertyCondition editCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
		private static bool TryGetUrlWithAutomation(IntPtr hWnd, bool logging, out string url)
		{
			if (cachedUrlElementDict == null)
			{
				cachedUrlElementDict = new CachedDictionary<IntPtr, AutomationElement>(TimeSpan.FromMinutes(5), true);
			}

			DebugEx.EnsureSta(); //FindFirst/TreeWalker leaks memory in MTA
			AutomationElement cachedUrlElement;
			if (cachedUrlElementDict.TryGetValue(hWnd, out cachedUrlElement))
			{
				try
				{
					if (TryGetUrlFromElement(cachedUrlElement, log, out url)) return true;
					var enabled = cachedUrlElement.Current.IsEnabled; //is the cached value still valid?
					throw new Exception("cannot fetch url from cached element");
				}
				catch (ElementNotAvailableException)
				{
					cachedUrlElementDict.Remove(hWnd);
				}
			}
			
			url = null;
			var element = AutomationElement.FromHandle(hWnd);
			if (element == null) throw new Exception("cannot find root window element");

			var toolBars = element.FindAll(TreeScope.Children, toolbarCondition);
			if (toolBars == null || toolBars.Count < 3)
				throw new Exception("cannot find url toolbar");
			var addrToolBar = toolBars[2];
			if (addrToolBar == null) throw new Exception("url toolbar is null");


			var urlCombo = addrToolBar.FindFirst(TreeScope.Children, comboCondition);
			if (urlCombo == null) throw new Exception("cannot find url combo");

			cachedUrlElement = urlCombo.FindFirst(TreeScope.Children, editCondition);
			if (cachedUrlElement == null || (bool)cachedUrlElement.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) //don't return url if we have focus (as it can change rapidly when typing)
				throw new Exception("cannot find url edit control or has keyboard focus");

			cachedUrlElementDict.Add(hWnd, cachedUrlElement);
			return TryGetUrlFromElement(cachedUrlElement, log, out url);
		}

		private static bool TryGetUrlFromElement(AutomationElement element, ILog logOvr, out string value)
		{
			if (element == null || (bool)element.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) //don't return url if we have focus (as it can change rapidly when typing)
			{
				value = null;
				return false;
			}
			if (!AutomationHelper.TryGetValueFromElement(element, logOvr, out value)) return false;
			if (!value.Contains(@"://")) value = "http://" + value;
			return true;
		}

		private static bool TryGetUrlImplAccTreeV2(IAccessible accClient, ref string url, out string currMethod)
		{
			currMethod = "first normal grouping V2";
			var c0 = AccessibilityHelper.GetAccessibleChildren(accClient).Cast<IAccessible>().Where(n => AccessibilityHelper.AccRoleEquals(n.accRole[0], AccessibilityHelper.AccRole.ROLE_SYSTEM_GROUPING) && (AccessibilityHelper.AccStates)n.accState[0] == AccessibilityHelper.AccStates.STATE_SYSTEM_NORMAL).FirstOrDefault();
			if (c0 == null) return false;
			try
			{
				return TryGetUrlImplAccTree(c0, ref url, out currMethod);
			}
			finally
			{
				currMethod += " V2";
			}
		}

		private static bool TryGetUrlImplAccTree(IAccessible accClient, ref string url, out string currMethod)
		{
			currMethod = "property page";
			var c1 = AccessibilityHelper.GetAccessibleChildren(accClient).Cast<IAccessible>().Where(n => AccessibilityHelper.AccRoleEquals(n.accRole[0], AccessibilityHelper.AccRole.ROLE_SYSTEM_PROPERTYPAGE)).FirstOrDefault();
			if (c1 == null) return false;
			try
			{
				return TryGetUrlImplAccTreeV3(c1, ref url, out currMethod);
			}
			finally
			{
				currMethod = currMethod.Substring(0, currMethod.Length - 3); //Removing " V3" at the end
			}
		}

		private static bool TryGetUrlImplAccTreeV3(IAccessible accClient, ref string url, out string currMethod)
		{
			currMethod = "normal grouping V3";
			var c2 = AccessibilityHelper.GetAccessibleChildren(accClient).Cast<IAccessible>().Where(n => AccessibilityHelper.AccRoleEquals(n.accRole[0], AccessibilityHelper.AccRole.ROLE_SYSTEM_GROUPING) && (AccessibilityHelper.AccStates)n.accState[0] == AccessibilityHelper.AccStates.STATE_SYSTEM_NORMAL).FirstOrDefault();
			if (c2 == null) return false;
			currMethod = "normal property page V3";
			var c3 = AccessibilityHelper.GetAccessibleChildren(c2).Cast<IAccessible>().Where(n => AccessibilityHelper.AccRoleEquals(n.accRole[0], AccessibilityHelper.AccRole.ROLE_SYSTEM_PROPERTYPAGE) && (AccessibilityHelper.AccStates)n.accState[0] == AccessibilityHelper.AccStates.STATE_SYSTEM_NORMAL).FirstOrDefault();
			if (c3 == null) return false;
			currMethod = "child for normal property page V3";
			var c4 = AccessibilityHelper.GetAccessibleChildren(c3).Cast<IAccessible>().FirstOrDefault();	//n.accRole[0] returns "browser" for this element
			if (c4 == null) return false;
			currMethod = "grandchild for normal property page V3";
			var c5 = AccessibilityHelper.GetAccessibleChildren(c4).Cast<IAccessible>().FirstOrDefault();
			if (c5 == null) return false;
			currMethod = "c5.accValue V3";
			url = c5.accValue[0];
			return true;
		}
	}
}
