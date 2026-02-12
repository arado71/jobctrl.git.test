using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Rules;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class WindowExternalTextHelper : IWindowExternalTextHelper
	{
		private readonly Dictionary<IntPtr, string> extTextMap = new Dictionary<IntPtr, string>();
		private readonly WatchedWindowsManager watchedWindowsManager; 
		private readonly object thisLock = new object();
		private static readonly WindowExternalTextHelper instance = new WindowExternalTextHelper();

		private WindowExternalTextHelper()
		{
			watchedWindowsManager = new WatchedWindowsManager();
			watchedWindowsManager.WatchedWindowClosed += WatchedWindowsManagerWatchedWindowClosed;
		}

		public static WindowExternalTextHelper Instance { get { return instance; } }

		void WatchedWindowsManagerWatchedWindowClosed(object sender, SingleValueEventArgs<IntPtr> e)
		{
			lock (thisLock)
				extTextMap.Remove(e.Value);
		}

		public void AddTextToWindow(IntPtr hWnd, string text)
		{
			lock (thisLock)
			{
				extTextMap[hWnd] = text;
				watchedWindowsManager.AddWathcedWindow(hWnd);
			}
		}

		public void AddTextToCurrentWindow(string text)
		{
			var windows = EnumWindowsHelper.GetWindowsInfo(false);
			var current = windows.FirstOrDefault(w => w.IsActive);
			if (current != null)
				AddTextToWindow(current.Handle, text);
		}

		public string GetTextByWindow(IntPtr hWnd)
		{
			string ret;
			lock (thisLock)
				extTextMap.TryGetValue(hWnd, out ret);
			return ret;
		}
	}
}
