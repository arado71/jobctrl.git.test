using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Foundation;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility;
using Tct.ActivityRecorderClient.Search;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Url
{
	public abstract class UrlResolver : IUrlResolver
	{
		private readonly ILog log;

		protected abstract bool CanResolveUrl(DesktopWindow desktopWindow);

		protected abstract bool TryResolveUrlForWindow(AXObject axWindow, out string url);

		protected UrlResolver(ILog logger)
		{
			log = logger;
		}

		public void SetUrls(IEnumerable<DesktopWindow> windowsInfo)
		{
			foreach (var lookup in windowsInfo.Where(n => CanResolveUrl(n)).ToLookup(n => n.ProcessId))
			{
				ResolveUrlsInProcess(lookup.Key, lookup.ToArray());
			}
		}

		private void ResolveUrlsInProcess(int pid, DesktopWindow[] windows)
		{
			try
			{
				AXError error;
				using (var axApp = AXObject.CreateFromApplication(pid))
				using (var axWindows = axApp.GetAttribute(AXAttribute.Windows, out error))
				{
					if (error != AXError.Success)
					{
						log.Error("Unable to get windows for pid " + pid + " error: " + error);
						return;
					}
					var axWindowArr = NSArray.ArrayFromHandleFunc<AXObject>(axWindows.Handle, n => new AXObject(n, false)); //owned by the Array
					try
					{
						//we have to match DesktopWindows with AXObjects, but we can have less DesktopWindows when windows are minimized but we can also have less
						//AXObjects (e.g. when in full screen). There is a high probability that both DesktopWindows and AXObjects are in the same Z-order
						Func<int, int> map = FindMapping(windows, axWindowArr);
						int skippedActiveIdx = -1;
						for (int i = 0; i < windows.Length; i++)
						{
							string url;
							int idx = map(i);
							if (idx == -1)
							{
								if (windows[i].IsActive)
								{
									skippedActiveIdx = i;
								}
								continue;
							}
							if (TryResolveUrlForWindow(axWindowArr[idx], out url))
							{
								windows[i].Url = url;
								// if we cannot resolve the active window, but can resolve a window behind it, probably the active was a tooltip window, so amend IsActive state
								if (skippedActiveIdx != -1)
								{
									windows[skippedActiveIdx].IsActive = false;
									windows[i].IsActive = true;
									skippedActiveIdx = -1;
								}
							}
						}
					}
					finally
					{
						Array.ForEach(axWindowArr, n => n.Dispose()); //we don't own it, but don't wanna get finalized
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to create AXObject for pid " + pid, ex);
			}
		}

		private Func<int, int> FindMapping(DesktopWindow[] windows, AXObject[] axWindowArr)
		{
			//we cannot assume that if the lengths are equal then we can use identity mapping.
			//e.g. we can have a normal window, a minimized window and an opened menu.
			//That means two windows in DesktopWindow[] (menu and normal) and two in AXObject[] (normal and minimized)
			var mapping = new Dictionary<int, int>(windows.Length);
			var axObjectsAndTitles = axWindowArr
				.Select(n =>
				{
					AXError error;
					var title = n.GetStringValueForAttribute(AXAttribute.Title, out error);
					if (error != AXError.Success)
					{
						log.Error("Unable to get title for axWindow error: " + error);
						title = null;
					}
					return new { Title = title, AXObject = n };
				})
				.ToList();
			var titles = axObjectsAndTitles.Select(n => n.Title).ToList();

			var usedIdxes = new HashSet<int>();
			for (int i = 0; i < windows.Length; i++)
			{
				var matches = PrefixMatcher.FindMatches(windows[i].Title, titles);
				//find the first unused idx for longest matching prefix
				var idx = matches.SkipWhile(n => usedIdxes.Contains(n.Index)).Select(n => n.Index).FirstOrDefault(-1);
				if (idx == -1)
				{
					if (!string.IsNullOrEmpty(windows[i].Title))
						log.Warn("Cannot find an axWindow with title: " + windows[i].Title);
				}
				usedIdxes.Add(idx);
				mapping.Add(i, idx);
			}

			return n => mapping[n];
		}
	}
}
