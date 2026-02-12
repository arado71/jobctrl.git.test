using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using Accessibility;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public class EdgeBlinkUrlResolver : IUrlResolver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private CachedDictionary<IntPtr, IAccessible> controlCache = new CachedDictionary<IntPtr, IAccessible>(TimeSpan.FromMinutes(5), true);
		private readonly int[] limits = new int[6];
		private bool isPathSuccessful = true;
		private bool isAccessibilityCaptureEnabled = true;
		private bool isCaptureKindAnnounced = false;
#if DEBUG
		//This can be changed if the caching should be enabled in debug mode.
		private const bool shouldCache = false;
#else
		private const bool shouldCache = true;
#endif

		public bool TryGetUrl(IntPtr hWnd, out string url)
		{
			if (!isAccessibilityCaptureEnabled) return TryGetUrlWithAutomation(hWnd, out url);
			var currMethod = 0;
			IAccessible accClient = null;
			IAccessible child = null;
			var objs = new List<IAccessible>();
			try
			{
				currMethod = 1;
				var style = (long) AccessibilityHelper.GetWindowLong(hWnd, AccessibilityHelper.GWL_STYLE);
				var isPopup = (style & AccessibilityHelper.WS_POPUP) == AccessibilityHelper.WS_POPUP && ((style & AccessibilityHelper.WS_MINIMIZEBOX) == 0 || (style & AccessibilityHelper.WS_MAXIMIZEBOX) == 0);
				if (isPopup)
				{
					url = null;
					return false;
				}

				if (controlCache.TryGetValue(hWnd, out var addressBar))
				{
					try
					{
						currMethod = 2;
						url = addressBar.accValue[0];
						return true;
					}
					catch (COMException)
					{
						// try find again
					}
				}

				if (!isCaptureKindAnnounced)
				{
					log.Debug("Trying to capture URL with accessibility...");
					isCaptureKindAnnounced = true;
				}
				currMethod = 3;
				Stack<int> path = new Stack<int>();
				accClient = AccessibilityHelper.GetIAccessibleFromWindow(hWnd, AccessibilityHelper.ObjId.CLIENT);
				if (accClient == null) throw new NullReferenceException("accClient is null.");
				currMethod = 4;
				if (AccessibilityHelper.AccRoleEquals(accClient.accRole[0], AccessibilityHelper.AccRole.ROLE_SYSTEM_ALERT))
				{
					//In this case the window is a notification/bookmark window.
					//We don't have to log any error here so the result variable can be true.
					url = null;
					return false;
				}
				currMethod = 5;
				child = (IAccessible)accClient.accChild[1];
				path.Push(1);

				currMethod = 6;
				var urlElement = findUrlElement(child, path, objs);
				currMethod = 7;

				if (urlElement == null)
					throw new Exception("Url element not found in the tree.");
				if (shouldCache)
				{
					controlCache.Set(hWnd, urlElement);
				}

				currMethod = 8;
				return TryGetValueFromElement(urlElement, out url);
			}
			catch (Exception ex)
			{
				log.Debug($"Couldn't get URL, method ({currMethod})", ex);
				isPathSuccessful = false;

				isAccessibilityCaptureEnabled = false;
				isCaptureKindAnnounced = false;

				if (!(ex is COMException)) throw;
				url = null;
				return false;
			}
			finally
			{
				if (accClient != null) Marshal.ReleaseComObject(accClient);
				if (child != null) Marshal.ReleaseComObject(child);
				foreach (var obj in objs)
				{
					Marshal.ReleaseComObject(obj);
				}
			}
		}

		private bool TryGetValueFromElement(IAccessible cachedUrlElement, out string rawUrl)
		{
			rawUrl = null;
			if (cachedUrlElement == null) return false;
			try
			{
				var state = cachedUrlElement.accState[0];
				if (AccessibilityHelper.accStateContains(state, AccessibilityHelper.AccStates.STATE_SYSTEM_FOCUSED)) return false;
				rawUrl = cachedUrlElement.accValue[0];
				return true;
			}
			catch (Exception ex)
			{
				log.Debug("Couldn't get value from cached url element.", ex);
				return false;
			}
		}

		private static IAccessible findUrlElement(IAccessible element, Stack<int> path, List<IAccessible> objs)
		{
			var role = element.accRole[0];
			if (AccessibilityHelper.AccRoleEquals(role, AccessibilityHelper.AccRole.ROLE_SYSTEM_TEXT) || AccessibilityHelper.AccRoleEquals(role, AccessibilityHelper.AccRole.ROLE_SYSTEM_COMBOBOX))
			{
				objs.Remove(element);
				return element;
			}
			for (var i = 1; i <= element.accChildCount; i++)
			{
				path.Push(i);
				var child = (IAccessible)element.accChild[i];
				objs.Add(child);
				var res = findUrlElement(child, path, objs);
				if (res != null) return res;
				path.Pop();
			}
			return null;
		}

		[ThreadStatic]
		private static CachedDictionary<IntPtr, AutomationElement> cachedUrlElementDict; //since searching for AutomationElements can be slow we have to cache them
		private static readonly PropertyCondition paneCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane);
		private static readonly PropertyCondition toolbarCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar);
		private static readonly PropertyCondition groupCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Group);
		private static readonly PropertyCondition editCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
		private static readonly PropertyCondition comboboxCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox);
		private static readonly AndCondition browserRootCondition = new AndCondition(paneCondition, new PropertyCondition(AutomationElement.ClassNameProperty, "BrowserRootView"));
		private static readonly AndCondition nonClientCondition = new AndCondition(paneCondition, new PropertyCondition(AutomationElement.ClassNameProperty, "NonClientView"));
		private static readonly AndCondition glassBrowserCondition = new AndCondition(paneCondition, new PropertyCondition(AutomationElement.ClassNameProperty, "GlassBrowserFrameView"));
		private static readonly AndCondition browserCondition = new AndCondition(paneCondition, new PropertyCondition(AutomationElement.ClassNameProperty, "BrowserView"));
		private static readonly AndCondition topContainerCondition = new AndCondition(paneCondition, new PropertyCondition(AutomationElement.ClassNameProperty, "TopContainerView"));
		private static readonly AndCondition toolbarViewCondition = new AndCondition(toolbarCondition, new PropertyCondition(AutomationElement.ClassNameProperty, "ToolbarView"));
		private static readonly AndCondition locationBarCondition = new AndCondition(groupCondition, new PropertyCondition(AutomationElement.ClassNameProperty, "LocationBarView"));

		private bool TryGetUrlWithAutomation(IntPtr hWnd, out string url)
		{
			if (!isCaptureKindAnnounced)
			{
				log.Debug("Trying to capture URL with automation...");
				isCaptureKindAnnounced = true;
			}

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

			try
			{
				url = null;
				var element = AutomationElement.FromHandle(hWnd);
				if (element == null) throw new Exception("cannot find root window element");

				var rootView = element.FindFirst(TreeScope.Children, browserRootCondition);
				if (rootView == null)
					throw new Exception("cannot find root view");

				var nonClient = rootView.FindFirst(TreeScope.Children, nonClientCondition);
				if (nonClient == null)
					throw new Exception("cannot find non-client view");

				var browser = nonClient.FindFirst(TreeScope.Children, browserCondition);
				if (browser == null)
				{
					var glassBrowser = nonClient.FindFirst(TreeScope.Children, glassBrowserCondition);
					if(glassBrowser == null)
					{
						throw new Exception("cannot find glass browser frame view");
					}
					browser = glassBrowser.FindFirst(TreeScope.Children, browserCondition);
				}
				if(browser == null)
					throw new Exception("cannot find browser view");

				var topContainer = browser.FindFirst(TreeScope.Children, topContainerCondition);
				if (topContainer == null)
					throw new Exception("cannot find top container view");

				var appBar = topContainer.FindFirst(TreeScope.Children, toolbarViewCondition);
				if (appBar == null)
					throw new Exception("cannot find app bar");

				var locBar = appBar.FindFirst(TreeScope.Children, locationBarCondition);
				if (locBar == null)
					throw new Exception("cannot find location bar view group");
				cachedUrlElement = locBar.FindFirst(TreeScope.Children, comboboxCondition);
				if(cachedUrlElement == null) cachedUrlElement = locBar.FindFirst(TreeScope.Children, editCondition);
				if (cachedUrlElement == null || (bool) cachedUrlElement.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) //don't return url if we have focus (as it can change rapidly when typing)
					throw new Exception("cannot find url edit control or has keyboard focus");

				cachedUrlElementDict.Add(hWnd, cachedUrlElement);
				return TryGetUrlFromElement(cachedUrlElement, log, out url);
			}
			catch (Exception ex)
			{
				log.Error("Capturing with automation failed", ex);
				isAccessibilityCaptureEnabled = true;
				isCaptureKindAnnounced = false;
				url = null;
				return false;
			}

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

	}

	public static class AccessibleExtensions
	{
		public const int ROLE_SYSTEM_GROUPING = 0x14;
		public const int ROLE_TOOL_BAR = 0x16;
		public const int ROLE_SYSTEM_TEXT = 0x2a;

		public static IAccessible GetFirstChildByName(this IAccessible accessible, string name)
		{
			return GetChildrenByName(accessible, name).FirstOrDefault();
		}

		public static IEnumerable<IAccessible> GetChildrenByName(this IAccessible accessible, string name)
		{
			for (var i = 0; i < accessible.accChildCount; i++)
				if (name == accessible.accName[i])
					yield return (IAccessible)accessible.accChild[i];
		}

		public static IAccessible GetFirstChildByRole(this IAccessible accessible, int role)
		{
			return GetChildrenByRole(accessible, role).FirstOrDefault();
		}

		public static IEnumerable<IAccessible> GetChildrenByRole(this IAccessible accessible, int role)
		{
			for (var i = 1; i <= accessible.accChildCount; i++)
				if (role == (int)accessible.accRole[i])
					yield return (IAccessible)accessible.accChild[i];
		}
	}
}
