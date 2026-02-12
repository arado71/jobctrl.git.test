using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class AutomationCapture
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AutomationCapture));

		private readonly Dictionary<IntPtr, Dictionary<int, AutomationElement>> elementCache = new Dictionary<IntPtr, Dictionary<int, AutomationElement>>();
		private readonly List<Func<IntPtr, int, bool>> windowSelectorsFuncs = new List<Func<IntPtr, int, bool>>();
		private readonly List<Func<AutomationElement, AutomationElement>> elementSelectorFuncs = new List<Func<AutomationElement, AutomationElement>>();
		private readonly List<int> elementCacheIndexes = new List<int>();
		private Func<AutomationElement, string> elementStringSelector;
		private bool isLastElementCached;

		public string Name { get; private set; }

		public double LastRunTime { get; private set; }

		public AutomationCapture(string name)
		{
			Name = name;
		}

		public void AddWindowSelector(Func<IntPtr, int, bool> windowSelector)
		{
			windowSelectorsFuncs.Add(windowSelector);
		}

		public void AddElementSelector(Func<AutomationElement, AutomationElement> elementSelector, bool isCached)
		{
			if (isCached) elementCacheIndexes.Add(elementSelectorFuncs.Count);
			elementSelectorFuncs.Add(elementSelector);
		}

		public void AddElementSelector(Func<AutomationElement, string> elementStringSelector, bool isCached)
		{
			this.elementStringSelector = elementStringSelector;
			isLastElementCached = isCached;
		}

		private string GetValidCacheResult(IntPtr hwnd)
		{
			Dictionary<int, AutomationElement> windowCachedElements;
			if (elementCache.TryGetValue(hwnd, out windowCachedElements))
			{
				AutomationElement element;
				if (isLastElementCached && windowCachedElements.TryGetValue(-1, out element) && IsElementValid(element))
				{
					return elementStringSelector(element);
				}
			}

			return null;
		}

		private AutomationElement GetClosestCachedElement(IntPtr hwnd, out int index)
		{
			index = 0;
			Dictionary<int, AutomationElement> windowCachedElements;
			if (elementCache.TryGetValue(hwnd, out windowCachedElements))
			{
				foreach (var elementCacheIndex in ((IEnumerable<int>)elementCacheIndexes).Reverse())
				{
					AutomationElement element;
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
					element = AutomationElement.FromHandle(targetWindow.Value);
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

		private static bool IsElementValid(AutomationElement element)
		{
			try
			{
				var processId = element.Current.ProcessId;
				return true;
			}
			catch (ElementNotAvailableException)
			{
				return false;
			}
		}

		private void SetElementCache(IntPtr hWnd, int level, AutomationElement element)
		{
			Dictionary<int, AutomationElement> windowElementCache;
			if (!elementCache.TryGetValue(hWnd, out windowElementCache))
			{
				windowElementCache = new Dictionary<int, AutomationElement>();
				elementCache.Add(hWnd, windowElementCache);
			}

			windowElementCache[level] = element;
		}

		private AutomationElement ProcessElements(AutomationElement root, IntPtr window, int indexOffset)
		{
			log.Verbose("Searching for automation children");
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
				log.Verbose("Automation child found");
			}

			log.Verbose("Target automation element found");
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
