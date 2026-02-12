using JC.IAccessibilityLib;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class IAccessibilityCapture
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<IntPtr, Dictionary<int, AccessibleItem>> elementCache = new Dictionary<IntPtr, Dictionary<int, AccessibleItem>>();
		private readonly List<Func<IntPtr, int, bool>> windowSelectorsFuncs = new List<Func<IntPtr, int, bool>>();
		private readonly List<Func<AccessibleItem, AccessibleItem>> elementSelectorFuncs = new List<Func<AccessibleItem, AccessibleItem>>();
		private readonly List<int> elementCacheIndexes = new List<int>();
		private Func<AccessibleItem, string> elementStringSelector;
		private bool isLastElementCached;

		public string Name { get; private set; }

		public double LastRunTime { get; private set; }

		public IAccessibilityCapture(string name)
		{
			Name = name;
		}

		public void AddWindowSelector(Func<IntPtr, int, bool> windowSelector)
		{
			windowSelectorsFuncs.Add(windowSelector);
		}

		public void AddElementSelector(Func<AccessibleItem, AccessibleItem> elementSelector, bool isCached)
		{
			if (isCached) elementCacheIndexes.Add(elementSelectorFuncs.Count);
			elementSelectorFuncs.Add(elementSelector);
		}

		public void AddElementSelector(Func<AccessibleItem, string> elementStringSelector, bool isCached)
		{
			this.elementStringSelector = elementStringSelector;
			isLastElementCached = isCached;
		}

		private string GetValidCacheResult(IntPtr hwnd)
		{
			Dictionary<int, AccessibleItem> windowCachedElements;
			if (elementCache.TryGetValue(hwnd, out windowCachedElements))
			{
				AccessibleItem element;
				if (isLastElementCached && windowCachedElements.TryGetValue(-1, out element) && IsElementValid(element))
				{
					return elementStringSelector(element);
				}
			}
			return null;
		}

		private AccessibleItem GetClosestCachedElement(IntPtr hwnd, out int index)
		{
			index = 0;
			Dictionary<int, AccessibleItem> windowCachedElements;
			if (elementCache.TryGetValue(hwnd, out windowCachedElements))
			{
				foreach (var elementCacheIndex in ((IEnumerable<int>)elementCacheIndexes).Reverse())
				{
					AccessibleItem element;
					if (windowCachedElements.TryGetValue(elementCacheIndex, out element))
					{
						if (!IsElementValid(element))
						{
							log.Verbose("Entry is invalid, removing from cache");
							windowCachedElements.Remove(elementCacheIndex);
							element = null;
						}
					}

					if (element != null)
					{
						index = elementCacheIndex + 1;
						return element;
					}
				}
			}

			return null;
		}

		public string Capture(IntPtr window)
		{
			var sw = Stopwatch.StartNew();
			var result = CaptureImpl(window);
			LastRunTime = sw.Elapsed.TotalMilliseconds;
			return result;
		}

		public string CaptureImpl(IntPtr window)
		{
			int index;

			var cachedResult = GetValidCacheResult(window);
			if (cachedResult != null) return cachedResult;

			var element = GetClosestCachedElement(window, out index);
			if (element == null)
			{
				var targetWindow = ProcessWindows(window);
				if (targetWindow == null) return null;
				try
				{

					element = new AccessibleItem(Desktop.Windows.AccessibilityHelper.GetIAccessibleFromWindow(targetWindow.Value, Desktop.Windows.AccessibilityHelper.ObjId.WINDOW), null, 0, 0);
				}
				catch (Exception e)
				{
					log.Error("Unrecognized error", e);
					return null;
				}
			}

			if (element == null) return null;

			element = ProcessElements(element, window, index);
			if (element == null) return null;

			if (isLastElementCached)
			{
				SetElementCache(window, -1, element);
				log.Verbose("Target element cached");
			}

			return elementStringSelector(element);
		}

		private static bool IsElementValid(AccessibleItem element)
		{
			return true;
		}

		private void SetElementCache(IntPtr hWnd, int level, AccessibleItem element)
		{
			Dictionary<int, AccessibleItem> windowElementCache;
			if (!elementCache.TryGetValue(hWnd, out windowElementCache))
			{
				windowElementCache = new Dictionary<int, AccessibleItem>();
				elementCache.Add(hWnd, windowElementCache);
			}

			windowElementCache[level] = element;
		}

		private AccessibleItem ProcessElements(AccessibleItem root, IntPtr window, int indexOffset)
		{
			log.Verbose("Searching for IAccessible children");
			var element = root;
			var i = 0;
			foreach (var childSelector in elementSelectorFuncs.Skip(indexOffset))
			{
				element = childSelector(element);
				if (element == null) return null;
				if (elementCacheIndexes.Contains(i))
				{
					SetElementCache(window, i, element);
					log.Verbose("Element " + i + " cached");
				}
				++i;
				log.Verbose("IAccessible child found");
			}

			log.Verbose("Target IAccessible element found");
			return element;
		}

		private IntPtr? ProcessWindows(IntPtr root)
		{
			log.Verbose("Searching for window children");
			var window = root;
			if (windowSelectorsFuncs.Count == 0) throw new ArgumentException();
			if (!windowSelectorsFuncs[0](window, 0)) return null;
			log.Verbose("Root window passes");
			foreach (var windowChildSelector in windowSelectorsFuncs.Skip(1))
			{
				var found = false;
				var i = 0;
				foreach (var child in GetChildren(window))
				{
					if (windowChildSelector(child, i++))
					{
						log.Verbose("Window child found");
						found = true;
						window = child;
						break;
					}
				}

				if (!found) return null;
			}

			log.Verbose("Target window found");
			return window;
		}

		private static IEnumerable<IntPtr> GetChildren(IntPtr hWnd)
		{
			var lastChild = IntPtr.Zero;
			while ((lastChild = WinApi.FindWindowEx(hWnd, lastChild, null, null)) != IntPtr.Zero)
			{
				yield return lastChild;
			}
		}
	}
}
