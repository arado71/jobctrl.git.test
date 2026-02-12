using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using MonoMac.Foundation;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility;

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
			foreach (var lookup in  windowsInfo.Where(n=>CanResolveUrl(n)).ToLookup(n=>n.ProcessId))
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
						Func<int,int> map = FindMapping(windows, axWindowArr);
						for (int i = 0; i < windows.Length; i++)
						{
							string url;
							int idx = map(i);
							if (idx == -1)
								continue;
							if (TryResolveUrlForWindow(axWindowArr[idx], out url))
							{
								windows[i].Url = url;
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

		//private static Func<int,int> IdentityMapping = n => n;

		private Func<int, int> FindMapping(DesktopWindow[] windows, AXObject[] axWindowArr)
		{
			//we cannot assume that if the lengths are equal then we can use identity mapping.
			//e.g. we can have a normal window, a minimized window and an opened menu.
			//That means two windows in DesktopWindow[] (menu and normal) and two in AXObject[] (normal and minimized)
			//if (windows.Length == axWindowArr.Length)
			//{
			//	return IdentityMapping;
			//}
			var mapping = new Dictionary<int, int>(windows.Length);
			var titleLookup = axWindowArr
				.Select(n =>
			{
				AXError error;
				var title = n.GetStringValueForAttribute(AXAttribute.Title, out error);
				if (error != AXError.Success)
				{
					log.Error("Unable to get title for axWindow error: " + error);
					title = null;
				}
				return new { Title = title, AXObject = n};
			})
				.Where(n => n.Title != null)
				.ToLookup(n => n.Title, n => n.AXObject);

			var usedIdxes = new HashSet<int>();
			for (int i = 0; i < windows.Length; i++)
			{
				var matchingCount = titleLookup[windows[i].Title].Count();
				int idx;
				if (matchingCount == 1) //if title is unique in axWindows
				{
					var ax = titleLookup[windows[i].Title].Single();
					idx = Array.IndexOf(axWindowArr, ax);
					Debug.Assert(idx >= 0);
					if (usedIdxes.Contains(idx))
					{
						idx = -1;
						log.Warn("This title is already assigned to an other desktopwindow: " + windows[i].Title);
					}
				}
				else if (matchingCount > 1) //needs further testing (we have more axWindows with the same title)
				{
					//we could check for position and size but istead we assume Z order atm. (and hope there is no dummy window with the same title)
					var ax = titleLookup[windows[i].Title]
						.Where(n => !usedIdxes.Contains(Array.IndexOf(axWindowArr, n)))
						.FirstOrDefault();
					idx = Array.IndexOf(axWindowArr, ax);
				}
				else //cannot find a matching title
				{
					if (windows[i].Title != "")
						log.Warn("Cannot find an axWindow with title: " + windows[i].Title);
					//it is possible that the title has been changed between the two queries but we won't try to handle that atm.
					idx = -1;
				}
				usedIdxes.Add(idx);
				mapping.Add(i, idx);
			}

			return n => mapping[n];
		}
	}
}

