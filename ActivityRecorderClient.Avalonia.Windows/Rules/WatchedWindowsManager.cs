using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderClient.Rules
{
	//http://blogs.msdn.com/b/oldnewthing/archive/2007/07/17/3903614.aspx - How are window manager handles determined in Windows NT?
	/// <summary>
	/// Thread-safe class for detecting if a window is closed (for learning rules).
	/// </summary>
	/// <remarks>
	/// Not water-tight but should be enough on 32-64 bit systems
	/// </remarks>
	public class WatchedWindowsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public event EventHandler<SingleValueEventArgs<IntPtr>> WatchedWindowClosed;

		private readonly object thisLock = new object();
		private readonly HashSet<IntPtr> watchedHandles = new HashSet<IntPtr>();

		public int WatchedWindowsCount { get { lock (thisLock) { return watchedHandles.Count; } } }

		protected override int ManagerCallbackInterval
		{
			get { return 1000; }
		}

		public WatchedWindowsManager()
			: base(log, false)
		{
		}

		protected override void ManagerCallbackImpl()
		{
			if (WatchedWindowsCount == 0) return;
			var currentHandles = GetTopLevelWindowHandles();
			var closedHandles = new HashSet<IntPtr>();
			lock (thisLock)
			{
				watchedHandles.RemoveWhere(n =>
											{
												if (currentHandles.Contains(n)) return false;
												closedHandles.Add(n);
												return true;
											});
				if (watchedHandles.Count == 0)
				{
					Stop();
				}
			}
			foreach (var closedHandle in closedHandles)
			{
				try
				{
					log.Debug("Window closed wHnd " + closedHandle);
					OnWatchedWindowClosed(closedHandle);
				}
				catch (Exception ex)
				{
					log.ErrorAndFail("Unexpected error calling OnWatchedWindowClosed " + closedHandle, ex);
				}
			}
		}

		public bool AddWathcedWindow(IntPtr hWnd)
		{
			lock (thisLock)
			{
				if (watchedHandles.Count == 0)
				{
					Start();
				}
				return watchedHandles.Add(hWnd);
			}
		}

		private static HashSet<IntPtr> GetTopLevelWindowHandles()
		{
			return EnumWindowsHelper.GetWindowsInfo();
		}

		private void OnWatchedWindowClosed(IntPtr hWnd)
		{
			var del = WatchedWindowClosed;
			if (del == null) return;
			del(this, SingleValueEventArgs.Create(hWnd));
		}

		public static class EnumWindowsHelper
		{
			public static HashSet<IntPtr> GetWindowsInfo()
			{
				var builder = new WindowInfoBuilder();
				WinApi.EnumWindows(builder.ProcessWindow, 0);
				return builder.WindowsInfo;
			}

			private class WindowInfoBuilder
			{
				public readonly HashSet<IntPtr> WindowsInfo = new HashSet<IntPtr>();

				public bool ProcessWindow(IntPtr hWnd, int lParam)
				{
					var isValid = WinApi.IsWindow(hWnd);
					if (isValid)
					{
						WindowsInfo.Add(hWnd);
					}
					return true; //process next window
				}
			}
		}
	}
}
