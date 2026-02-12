using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public class InternetExplorerUrlResolver : IUrlResolver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly object thisLock = new object();

		public bool TryGetUrl(IntPtr hWnd, out string url)
		{
			lock (thisLock)
			{
				if (TryGetUrlNew(hWnd, out url))
					return true;
				var child = GetAutomationWindow(hWnd);
				if (child != null)
				{
					if (AutomationHelper.TryGetValueFromHandle(child.Handle, log, out url))
						return true;
				}
				if (cachedUrlElementDict == null)
				{
					cachedUrlElementDict = new CachedDictionary<IntPtr, AutomationElement>(TimeSpan.FromMinutes(5), true);
					cachedUrlText = new CachedDictionary<KeyValuePair<AutomationElement, string>, string>(TimeSpan.FromMinutes(5), true);
				}

				//log.Debug("Cannot find the right window"); //too much noise
				url = null;
				return false;
			}
		}

		public static ChildWindowInfo GetAutomationWindow(IntPtr hWnd)
		{
			return EnumChildWindowsHelper.GetFirstChildWindowInfo(hWnd, n =>
			{
				if (n.ClassName == "Internet Explorer_Server")
				{
					var parent = WinApi.GetAncestor(n.Handle, WinApi.GetAncestorFlags.GetParent);
					Debug.Assert(parent != IntPtr.Zero);
					if (parent == IntPtr.Zero) return false; //if the className of the parent is "Internet Explorer_Hidden" then the window just has been closed, and we should return false, but that is so rare we rather skip the check
					var gradParent = WinApi.GetAncestor(parent, WinApi.GetAncestorFlags.GetParent);
					Debug.Assert(gradParent != IntPtr.Zero);
					if (gradParent == IntPtr.Zero) return false;
					var className = WindowTextHelper.GetClassName(gradParent);
					return className != "Shell Embedding"; //Developer tools creates 2 server objects in IE11 atm. F12BrowserToolWindow / Shell Embedding / Shell DocObject View / Internet Explorer_Server
				}
				return false;
			});
		}

		

		[ThreadStatic]
		private static CachedDictionary<IntPtr, AutomationElement> cachedUrlElementDict; //since searching for AutomationElements can be slow we have to cache them
		[ThreadStatic]
		private static CachedDictionary<KeyValuePair<AutomationElement, string>, string> cachedUrlText; // caching previous url value for focused editor
		private bool TryGetUrlNew(IntPtr hWnd, out string url)
		{
			var d = "Start";
			var result = false;
			Exception exc = null;
			string title;
			try
			{
				DebugEx.EnsureSta(); //FindFirst/TreeWalker leaks memory in MTA
				title = WindowTextHelper.GetWindowText(hWnd); // lacks performance, maybe passed from desktopcapture?
				AutomationElement cachedUrlElement;
				if (cachedUrlElementDict.TryGetValue(hWnd, out cachedUrlElement))
				{
					try
					{
						d = "get cached value";
						var key = new KeyValuePair<AutomationElement, string>(cachedUrlElement, title);
						result = TryGetValueFromElement(cachedUrlElement, log, out url);
						if (!result)
						{
							if (!cachedUrlText.TryGetValue(key, out url))
								return false;
							result = true;
						}
						cachedUrlText.Set(key, url);
						return true;
					}
					catch (ElementNotAvailableException)
					{
						cachedUrlElementDict.Remove(hWnd);
					}
				}
				url = null;
				d = "FromHandle";
				var element = AutomationElement.FromHandle(hWnd);
				if (element == null)
					return false;

				if (element.Current.ClassName != "IEFrame")
				{
					// hacking for non-visible iexplore windows
					result = true;
					return result;
				}

				Condition editTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);

				d = "1";
				var specificChild = element.GetFirstChildByClassName("ReBarWindow32", ControlType.Pane);
				if (specificChild == null)
					return false;
				d = "2";
				specificChild = specificChild.GetFirstChildByClassName("Address Band Root", ControlType.Pane);
				if (specificChild == null)
					return false;
				d = "3";
				cachedUrlElement = specificChild.FindFirst(TreeScope.Children, editTypeCond);

				if (cachedUrlElement == null)
					return false;
				d = "get value";
				cachedUrlElementDict.Add(hWnd, cachedUrlElement);
				result = TryGetValueFromElement(cachedUrlElement, log, out url);
				if (result)
				{
					var key = new KeyValuePair<AutomationElement, string>(cachedUrlElement, title);
					cachedUrlText.Set(key, url);
				}
				return result;
			}
			catch (Exception ex)
			{
				exc = ex;
				url = null;
				return false;
			}
			finally
			{
				if (!result)
					log.DebugFormat("Unable to get url hWnd {0} method {1}{2}{3}", hWnd, d, exc != null ? Environment.NewLine : "", exc);
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
	}
}
