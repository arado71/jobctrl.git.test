using Accessibility;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public abstract class ChromiumUrlResolverBase : IUrlResolver
	{
		public abstract string ProcessName { get; }

		public abstract Browser Browser { get; }

		[ThreadStatic] protected static CachedDictionary<IntPtr, IAccessible> cachedUrlElementDict;
		[ThreadStatic] protected static CachedDictionary<KeyValuePair<IAccessible, string>, string> cachedUrlText;

		private readonly ILog log;
		private bool shouldInitialize = true;
		private readonly List<ElementIndexPathForWindow> urlPathForWindows = new List<ElementIndexPathForWindow>();

#if DEBUG
		//This can be changed if the caching should be enabled in debug mode.
		protected readonly bool shouldCache = false;
#else
		protected readonly bool shouldCache = true;
#endif

		protected ChromiumUrlResolverBase(ILog log)
		{
			this.log = log;
		}

		protected virtual void Initialize()
		{

		}

		public virtual bool TryGetUrl(IntPtr hWnd, out string url)
		{
			if (shouldInitialize) { Initialize(); shouldInitialize = false; }
			string rawUrl;
			if (TryGetUrlWithAccessibility(hWnd, out rawUrl))
			{
				//chrome doesn't include http:// in urls so emulate it
				url = GetFixedUrl(rawUrl);
				return true;
			}

			url = null;
			return false;
		}
		
		protected bool TryGetUrlWithAccessibility(IntPtr hWnd, out string rawUrl)
		{
			string currMethod = "GetCachedValue";
			Exception exc = null;
			var result = false;
			try
			{
				var title = WindowTextHelper.GetWindowText(hWnd); // lacks performance, maybe passed from desktopcapture?
				IAccessible cachedUrlElement;
				if(cachedUrlElementDict == null)
				{
					cachedUrlElementDict = new CachedDictionary<IntPtr, IAccessible>(TimeSpan.FromMinutes(5), true);
					cachedUrlText = new CachedDictionary<KeyValuePair<IAccessible, string>, string>(TimeSpan.FromMinutes(5), true);
				}
				if (cachedUrlElementDict.TryGetValue(hWnd, out cachedUrlElement))
				{
					try
					{
						var key = new KeyValuePair<IAccessible, string>(cachedUrlElement, title);
						result = TryGetValueFromElement(cachedUrlElement, out rawUrl);
						if (!result)
						{
							if (rawUrl == "")
							{
								cachedUrlText.Remove(key);
								result = true;
								return true;
							}

							if (!cachedUrlText.TryGetValue(key, out rawUrl))
								return false;
							result = true;
						}

						cachedUrlText.Set(key, rawUrl);
						return true;
					}
					catch (Exception ex)
					{
						cachedUrlElementDict.Remove(hWnd);
						log.Debug("Couldn't get url from cached element.", ex);
					}
				}
				var urlPathForWindow = urlPathForWindows.FirstOrDefault(x => x.Hwnd == hWnd);
				if(urlPathForWindow != null)
				{
					try
					{
						var urlElement = urlPathForWindow.GetAccessibleElementFromPath();
						result = TryGetValueFromElement(urlElement, out rawUrl);
						if (result && !string.IsNullOrEmpty(rawUrl)) // If we have empty url, then we should try again (most of the time)
						{
							if (shouldCache)
							{
								currMethod = "Cache";
								var key = new KeyValuePair<IAccessible, string>(urlElement, title);
								cachedUrlText.Set(key, rawUrl);
							}
							return result;
						} else
						{
							urlPathForWindows.Remove(urlPathForWindow);
						}
					}
					catch	(Exception ex)
					{
						urlPathForWindows.Remove(urlPathForWindow);
					}
				}

				var sw = Stopwatch.StartNew();
				IAccessible accClient = null;
				IAccessible child = null;
				var objs = new List<IAccessible>();
				Stack<int> path = new Stack<int>();
				try
				{
					currMethod = "GetIAccessibleFromWindow";
					accClient = AccessibilityHelper.GetIAccessibleFromWindow(hWnd, AccessibilityHelper.ObjId.CLIENT);
					if (accClient == null) throw new NullReferenceException("accClient is null.");
					currMethod = "CheckIfIAccessibleIsAlert";
					if (AccessibilityHelper.AccRoleEquals(accClient.accRole[0], AccessibilityHelper.AccRole.ROLE_SYSTEM_ALERT))
					{
						//In this case the window is a notification/bookmark window.
						//We don't have to log any error here so the result variable can be true.
						result = true;
						rawUrl = null;
						return false;
					}
					currMethod = "1";
					child = accClient;

					currMethod = "FindUrlElement";
					var urlElement = findUrlElement(child, path, objs);
					currMethod = "CheckWetherUrlElementFound";
					if (urlElement == null)
						throw new Exception("Url element not found in the tree.");
					if (shouldCache)
					{
						cachedUrlElementDict.Set(hWnd, urlElement);
					}

					currMethod = "GetValue";
					result = TryGetValueFromElement(urlElement, out rawUrl);
					if (shouldCache)
					{
						urlPathForWindows.Add(new ElementIndexPathForWindow { Path = path.Reverse().ToList(), Hwnd = hWnd });
						currMethod = "Cache";
						var key = new KeyValuePair<IAccessible, string>(urlElement, title);
						cachedUrlText.Set(key, rawUrl);
					}
					return result;
				}
				catch (Exception ex)
				{
					log.Debug($"Couldn't get URL, method ({currMethod})", ex);
					throw;
				}

				finally
				{
					log.Debug($"Capturing url with Accessibility took {sw.Elapsed.TotalMilliseconds} ms. Path: {string.Join("-", path.Reverse())}");
					if (accClient != null) Marshal.ReleaseComObject(accClient);
					if (child != null) Marshal.ReleaseComObject(child);
					foreach (var accessible in objs)
					{
						Marshal.ReleaseComObject(accessible);
					}
				}
			}
			catch (Exception ex)
			{
				exc = ex;
				rawUrl = null;
				return false;
			}
			finally
			{
				if (!result) log.DebugFormat("Unable to get url hWnd {0} method {1}{2}{3}", hWnd, currMethod, exc != null ? Environment.NewLine : "", exc);
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
			object accRole;
			try
			{
				accRole = element.accRole[0];
			} catch (COMException ex)
			{
				return null;
			}
			if (AccessibilityHelper.AccRoleEquals(accRole, AccessibilityHelper.AccRole.ROLE_SYSTEM_TEXT) && element.accChildCount == 0)
			{
				objs.Remove(element);
				return element;
			}
			for (var i = 1; i <= element.accChildCount; i++)
			{
				path.Push(i);
				IAccessible child;
				try
				{
					child = (IAccessible)element.accChild[i];
				} catch (ArgumentException)
				{
					path.Pop();
					continue;
				}
				objs.Add(child);
				var res = findUrlElement(child, path, objs);
				if (res != null) return res;
				path.Pop();
			}
			return null;
		}

		internal const string GoogleSearch = "http://www.google.com/search?q="; //hax
		internal static string GetFixedUrl(string rawUrl) //from v29 urls and searches are mixed and there is no way to separate them atm.
		{
			if (string.IsNullOrEmpty(rawUrl)) return rawUrl;
			rawUrl = rawUrl.Trim(); //trim because this is just a text from the location bar
			if (rawUrl.Contains(' ')) return GoogleSearch + rawUrl.Replace(' ', '+'); //possible google search
			return (rawUrl.StartsWith("about:")
				|| (rawUrl.Contains("://")) && Uri.CheckSchemeName(rawUrl.Split(new[] { "://" }, StringSplitOptions.None)[0])) //The '://' string can be in the qurey string and we still want to fix that url, so simple Contains is not enough
				? rawUrl //this can also be a search (sad://valami) but we cannot detect that atm.
				: "http://" + rawUrl; //this might also be a seach but we cannot detect that for sure (e.g. valami.s - is a search)
		}
	}
}
