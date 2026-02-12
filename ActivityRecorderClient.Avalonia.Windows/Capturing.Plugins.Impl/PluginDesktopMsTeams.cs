using Accessibility;
using JobCTRL.Plugins;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	internal class PluginDesktopMsTeams : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const long inCallNotFoundCacheTicks = 1L * 60L * 10000000L; // 1 minutes

		[ThreadStatic] protected static CachedDictionary<IntPtr, AccessibleItem> cachedClkElementDict;
		[ThreadStatic] protected static Dictionary<IntPtr, long> cachedInCallNotFoundTicksDict;
#if DEBUG
		//This can be changed if the caching should be enabled in debug mode.
		protected readonly bool shouldCache = true;
#else
		protected readonly bool shouldCache = true;
#endif
		private readonly List<ElementIndexPathForWindow> clkPathForWindows = new List<ElementIndexPathForWindow>();

		public const string PluginId = "JobCTRL.DesktopMsTeams";

		string ICaptureExtension.Id => PluginId;

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return PluginConference.KeyConferenceService;
			yield return PluginConference.KeyConferenceState;
			yield return PluginConference.KeyConferenceTime;
			yield return PluginConference.KeyConferencePartyName;
			yield return PluginConference.KeyConferencePartyEmail;
		}

		public IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public void SetParameter(string name, string value)
		{
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!string.Equals(processName, "ms-teams.exe", StringComparison.OrdinalIgnoreCase)) yield break;
			yield return new KeyValuePair<string, string>(PluginConference.KeyConferenceService, PluginId);
			if (TryGetClkFromWindow(hWnd, out string result))
			{
				yield return new KeyValuePair<string, string>(PluginConference.KeyConferenceState, PluginConference.InCallStateName);
				yield return new KeyValuePair<string, string>(PluginConference.KeyConferenceTime, result);
			}

		}

		private bool TryGetClkFromWindow(IntPtr hWnd, out string clk)
		{
			string currMethod = "GetCachedValue";
			Exception exc = null;
			var result = false;
			var intentionalFail = false;
			try
			{
				var sw = Stopwatch.StartNew();
				try
				{
					if (cachedClkElementDict == null)
					{
						cachedClkElementDict = new CachedDictionary<IntPtr, AccessibleItem>(TimeSpan.FromMinutes(5), true);
						cachedInCallNotFoundTicksDict = new Dictionary<IntPtr, long>();
					}
					if (cachedInCallNotFoundTicksDict.TryGetValue(hWnd, out long ticks) && DateTime.UtcNow.Ticks - ticks < inCallNotFoundCacheTicks)
					{
						clk = "";
						intentionalFail = true;
						return false;
					}
					var title = WindowTextHelper.GetWindowText(hWnd); // lacks performance, maybe passed from desktopcapture?
					AccessibleItem cachedClkElement;
					if (cachedClkElementDict.TryGetValue(hWnd, out cachedClkElement))
					{
						try
						{
							result = TryGetNameFromElement(cachedClkElement, out clk);
							return result;
						}
						catch (Exception ex)
						{
							cachedClkElementDict.Remove(hWnd);
							log.Debug("Couldn't get clk from cached element.", ex);
						}
					}
					var clkPathForWindow = clkPathForWindows.FirstOrDefault(x => x.Hwnd == hWnd);
					if (clkPathForWindow != null)
					{
						try
						{
							var clkElement = clkPathForWindow.GetAccessibleItemFromPath();
							result = TryGetNameFromElement(clkElement, out clk);
							if (result && !string.IsNullOrEmpty(clk)) // If we have empty clk, then we should try again (most of the time)
							{
								return result;
							}
							else
							{
								clkPathForWindows.Remove(clkPathForWindow);
							}
						}
						catch (Exception ex)
						{
							clkPathForWindows.Remove(clkPathForWindow);
						}
					}
				} finally
				{
					log.Verbose($"Capturing from cache took {sw.Elapsed.TotalMilliseconds} ms.");
				}

				sw = Stopwatch.StartNew();
				AccessibleItem accClient = null;
				AccessibleItem child = null;
				var objs = new List<AccessibleItem>();
				Stack<int> path = new Stack<int>();
				try
				{
					currMethod = "GetIAccessibleFromWindow";
					accClient = new AccessibleItem(AccessibilityHelper.GetIAccessibleFromWindow(hWnd, AccessibilityHelper.ObjId.CLIENT), null, 0, 0);
					if (accClient == null) throw new NullReferenceException("accClient is null.");
					currMethod = "CheckIfIAccessibleIsAlert";
					if (AccessibilityHelper.AccRoleEquals(accClient.Role, AccessibilityHelper.AccRole.ROLE_SYSTEM_ALERT))
					{
						//In this case the window is a notification/bookmark window.
						//We don't have to log any error here so the result variable can be true.
						result = true;
						clk = null;
						return false;
					}
					currMethod = "1";
					child = accClient;

					currMethod = "FindClockElement";
					var clkElement = findClockElement(child, path, objs);
					currMethod = "CheckWetherClkElementFound";
					if (clkElement == null)
					{
						cachedInCallNotFoundTicksDict[hWnd] = DateTime.UtcNow.Ticks;
						throw new Exception("Clk element not found in the tree.");
					}
					if (shouldCache)
					{
						cachedClkElementDict.Set(hWnd, clkElement);
					}

					currMethod = "GetValue";
					result = TryGetNameFromElement(clkElement, out clk);
					if (shouldCache)
					{
						currMethod = "Cache";
						clkPathForWindows.Add(new ElementIndexPathForWindow { Path = path.Reverse().ToList(), Hwnd = hWnd });
					}
					return result;
				}
				catch (Exception ex)
				{
					log.Debug($"Couldn't get CLK, method ({currMethod})", ex);
					throw;
				}

				finally
				{
					log.Debug($"Capturing InCallState with Accessibility took {sw.Elapsed.TotalMilliseconds} ms. Path: {string.Join("-", path.Reverse())}");
				}
			}
			catch (Exception ex)
			{
				exc = ex;
				clk = null;
				return false;
			}
			finally
			{
				if (!result && !intentionalFail) log.DebugFormat("Unable to get clk hWnd {0} method {1}{2}{3}", hWnd, currMethod, exc != null ? Environment.NewLine : "", exc);
			}
		}

		private bool TryGetNameFromElement(AccessibleItem cachedClkElement, out string clk)
		{
			clk = null;
			if (cachedClkElement == null) return false;
			try
			{
				cachedClkElement.tryRefresh();
				clk = cachedClkElement.Name;
				return true;
			}
			catch (Exception ex)
			{
				log.Debug("Couldn't get value from cached url element.", ex);
				return false;
			}
		}

		private static AccessibleItem findClockElement(AccessibleItem element, Stack<int> path, List<AccessibleItem> objs)
		{
			object accRole;
			try
			{
				accRole = element.Role;
			}
			catch (COMException ex)
			{
				log.Debug("Error in getting role of accessible element.", ex);
				return null;
			}
			if (AccessibilityHelper.AccRoleEquals(accRole, AccessibilityHelper.AccRole.ROLE_SYSTEM_CLOCK))
			{
				foreach (var item in element.getChildren())
				{
					path.Push(item.ChildIndex);
					if (AccessibilityHelper.AccRoleEquals(item.Role, AccessibilityHelper.AccRole.ROLE_SYSTEM_STATICTEXT))
					{
						log.Verbose("Found the correct element.");
						return item;
					}
					foreach (var childItem in item.getChildren())
					{
						path.Push(childItem.ChildIndex);
						if (AccessibilityHelper.AccRoleEquals(childItem.Role, AccessibilityHelper.AccRole.ROLE_SYSTEM_STATICTEXT))
						{
							log.Verbose("Found the correct element.");
							return childItem;
						}
						path.Pop();
						objs.Remove(element);
					}
					path.Pop();
					objs.Remove(element);
				}
			}
			
			var children = element.getChildren();
			foreach (var item in children)
			{
				path.Push(item.ChildIndex);

				objs.Add(item);
				var res = findClockElement(item, path, objs);
				if (res != null) return res;
				path.Pop();
			}
			return null;
		}
	}
}
