using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Hotkeys;
using Timer = System.Threading.Timer;

namespace Tct.ActivityRecorderClient.Capturing.UserActivity
{
	public class UserActivityWinService : IUserActivityService
	{
		public HotkeySetting HotkeyPressed { get; set; }

		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan hookRefreshInterval = TimeSpan.FromMinutes(20); //interval for refreshing hooks even if it seems to work
		private static readonly TimeSpan hookCheckInterval = TimeSpan.FromMinutes(1); //interval for checking if hooks seems to work if not then refresh hooks
		private static readonly TimeSpan maxDiffBetweeHookAndLastInput = TimeSpan.FromSeconds(30); //if difference between hook and last input is bigger asume hook is not working
		private static readonly TimeSpan scrollButtonCheckInterval = TimeSpan.FromSeconds(1);

		private readonly object thisLock = new object();
		private readonly Queue<bool> isActiveQueue = new Queue<bool>();
		private readonly AutoResetEvent stateChangeWaitHandle = new AutoResetEvent(false);
		private readonly LastInputInfo lastInputInfo = new LastInputInfo();
		private readonly MachineInputDetector detector = new MachineInputDetector();
		private readonly UserActivityHook hook;
		private readonly Thread hookThread;
		private readonly Timer timerRefresh;
		private readonly Timer timerDetectorFlush;
		private bool isActive;
		private int mouseActivityCounter;
		private volatile int mouseLastX;
		private volatile int mouseLastY;
		private volatile int lastDelta;
		private int KeyboardActivityCounter;
		private List<HotkeySetting> Hotkeys;
		private volatile int defaultMouseActivity;
		private volatile int defaultKeyboardActivity;
		private volatile int lastHookActivity = Environment.TickCount;
		private volatile int lastStrictHookActivity = Environment.TickCount;
		private volatile int lastScrollBtnActivity = Environment.TickCount;
		private volatile int lastHookRefresh = Environment.TickCount;

		private volatile bool isWinKeyPressed, isControlPressed, isShiftPressed, isAltPressed;

		private UserActivityWinService()
		{
			hook = new UserActivityHook();
			hook.KeyUp += HookKeyUp;
			hook.KeyDown += HookKeyDown;
			hook.OnMouseActivity += HookOnMouseActivity;
			hookThread = new Thread(MainLoop) { IsBackground = true, Priority = ThreadPriority.Highest, Name = "HK" };
			hookThread.SetApartmentState(ApartmentState.STA);
			hookThread.Start();
			if (ConfigManager.IsWindows7) //refresh hooks on windows 7 to avoid unhooks due to timeouts (cannot find any better way)
			{
				timerRefresh = new Timer(RefreshHooks, null, hookCheckInterval, hookCheckInterval);
			}
			timerDetectorFlush = new Timer(DetectorFlush);
		}

		public static readonly UserActivityWinService Instance;

		static UserActivityWinService()
		{
			Instance = new UserActivityWinService();
		}
		public void SetHotkeys(List<HotkeySetting> hkl)
		{
			Hotkeys = hkl;
		}
		private void HookKeyDown(object sender, KeyEventArgs e)
		{
			lastStrictHookActivity = Environment.TickCount;
			switch (e.KeyCode)
			{
				case Keys.LControlKey:
				case Keys.RControlKey:
					isControlPressed = true;
					break;
				case Keys.Alt:
					isAltPressed = true;
					break;
				case Keys.LWin:
				case Keys.RWin:
					isWinKeyPressed = true;
					break;
				case Keys.LShiftKey:
				case Keys.RShiftKey:
					isShiftPressed = true;
					break;
			}
		}

		private void HookOnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			lastStrictHookActivity = Environment.TickCount;
			if (Math.Abs(e.X - mouseLastX) < ConfigManager.MouseMovingThreshold && Math.Abs(e.Y - mouseLastY) < ConfigManager.MouseMovingThreshold && e.Delta == 0)
				return;
			if (e.Button == System.Windows.Forms.MouseButtons.None && e.Delta == 0) return;
			if (e.Delta != 0)
			{
				if (TimeSpan.FromMilliseconds(Environment.TickCount - lastScrollBtnActivity) < scrollButtonCheckInterval) return;
				lastScrollBtnActivity = Environment.TickCount;
			}
			Interlocked.Increment(ref mouseActivityCounter);
			lastHookActivity = Environment.TickCount;
			lastMouseActivityTime = lastHookActivity;
			mouseLastX = e.X;
			mouseLastY = e.Y;
		}

#if DEBUG
		private int eventCount, detectorCount;

		private void DetectorTestDebug(bool act, int cnt)
		{
			if (act) Interlocked.Increment(ref eventCount);
			Interlocked.Add(ref detectorCount, cnt);
			log.Debug($"Detector response: {cnt}, all event: {eventCount}, all detector: {detectorCount}");
		}
#else
		[Conditional("DEBUG")] //no compile in release
		private static void DetectorTestDebug(bool act, int cnt) {}
#endif

		private void HookKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			lastStrictHookActivity = Environment.TickCount;
			if (e.KeyCode == Keys.L && isWinKeyPressed)
			{
				log.Debug("Win+L released");
				isWinKeyPressed = false;
				return;
			}

			if (!isHotKey(e))
			{
				var now = Environment.TickCount;
				var cnt = detector.NewEvent(now);
				DetectorTestDebug(true, cnt);
				if (cnt == 0)
				{
					timerDetectorFlush.Change(MachineInputDetector.PROPER_INTERVAL, Timeout.Infinite);
					return;
				}
				Interlocked.Add(ref KeyboardActivityCounter, cnt);
				lastHookActivity = now;
				lastKeyboardActivityTime = lastHookActivity;
			}
			switch (e.KeyCode)
			{
				case Keys.LControlKey:
				case Keys.RControlKey:
					isControlPressed = false;
					break;
				case Keys.Alt:
					isAltPressed = false;
					break;
				case Keys.LWin:
				case Keys.RWin:
					isWinKeyPressed = false;
					break;
				case Keys.LShiftKey:
				case Keys.RShiftKey:
					isShiftPressed = false;
					break;
			}
		}

		private void DetectorFlush(object state)
		{
			var cnt = detector.FlushEvents();
			log.Verbose("DetectorFlush cnt: " + cnt);
			if (cnt == 0) return;
			DetectorTestDebug(false, cnt);
			Interlocked.Add(ref KeyboardActivityCounter, cnt);
		}


		private bool isHotKey(KeyEventArgs e)
		{
			if (Hotkeys == null || Hotkeys.Count == 0) return false;
			for (int i = 0; i < Hotkeys.Count; i++)
			{
				if (Hotkeys[i].CompareTo((Forms.Keys)e.KeyCode, isAltPressed, isControlPressed, isShiftPressed, isWinKeyPressed) != 0) continue;
				return true;
			}
			return false;
		}

		private bool IsHookActive() //heuristic approach
		{
			try
			{
				return TimeSpan.FromMilliseconds(lastInputInfo.GetLastInputTicks() - lastStrictHookActivity) < maxDiffBetweeHookAndLastInput;
			}
			catch (Exception ex)
			{
				log.Warn("Unable to get last input info", ex);
				return false;
			}
		}

		private int? lastKeyboardActivityTime;
		private int? lastMouseActivityTime;

		public void GetAndResetCounters(out int lastKeyboardActivity, out int lastMouseActivity)
		{
			hook.UpdateRemoteDesktop();
			lastKeyboardActivity = Interlocked.Exchange(ref KeyboardActivityCounter, defaultKeyboardActivity);
			lastMouseActivity = Interlocked.Exchange(ref mouseActivityCounter, defaultMouseActivity);
		}

		public int? GetLastMouseActivityTime()
		{
			return lastMouseActivityTime;
		}
		public int? GetLastKeyboardActivityTime()
		{
			return lastKeyboardActivityTime;
		}

		public int? GetLastActivity()
		{
			if (defaultMouseActivity == -1 || defaultKeyboardActivity == -1 || !isActive)
			{
				return null;
			}

			return lastHookActivity;
		}

		private void RefreshHooks(object state)
		{
			var isHookActive = IsHookActive();
			var shouldRefresh = TimeSpan.FromMilliseconds(Environment.TickCount - lastHookRefresh) > hookRefreshInterval;
			if (isHookActive && !shouldRefresh) return;
			lastHookRefresh = Environment.TickCount; //there is a race here but we don't expect more RefreshHooks running at the same time, but if they do we don't care
			if (!isHookActive)
			{
				log.Debug("Hook is not active or working with elevated process and jc is not elevated");
			}

			lock (thisLock)
			{
				if (!isActive || IsDisposed) return;
				Stop();
				Start();
			}
		}

		private void MainLoop()
		{
			log.Info("Started hook thread");
			while (!IsDisposed)
			{
				if (!stateChangeWaitHandle.WaitOne()) //this should never happen
				{
					log.Error("WTF Hook");
					Thread.CurrentThread.Join(100); //pump and wait
					continue;
				}
				lock (thisLock)
				{
					if (IsDisposed)
					{
						if (isActive)
						{
							StopHooks();
						}
						break;
					}
					while (isActiveQueue.Count != 0)
					{
						isActive = isActiveQueue.Dequeue();
						if (isActive)
						{
							StartHooks();
						}
						else
						{
							StopHooks();
						}
					}
				}
			}
			log.Info("Stopped hook thread");
		}

		private bool isDisposed;
		private bool IsDisposed
		{
			get
			{
				lock (thisLock)
				{
					return isDisposed;
				}
			}
			set
			{
				lock (thisLock)
				{
					isDisposed = value;
				}
			}
		}

		private void EnqueueIsActive(bool isActiveParam)
		{
			bool shouldStartProcessing;
			lock (thisLock)
			{
				shouldStartProcessing = (isActiveQueue.Count == 0);
				isActiveQueue.Enqueue(isActiveParam);
			}
			if (shouldStartProcessing) stateChangeWaitHandle.Set();
		}

		public void Stop()
		{
			EnqueueIsActive(false);
		}

		public void Start()
		{
			EnqueueIsActive(true);
		}

		public void Dispose()
		{
			lock (thisLock) //avoid race
			{
				if (IsDisposed) return;
				IsDisposed = true;
			}
			log.Info("Disposing UserActivityWinService");
			stateChangeWaitHandle.Set();
			if (timerRefresh != null)
			{
				timerRefresh.Change(Timeout.Infinite, Timeout.Infinite);
				timerRefresh.Dispose();
			}
			timerDetectorFlush.Change(Timeout.Infinite, Timeout.Infinite);
			timerDetectorFlush.Dispose();
			hookThread.Join();
		}

		private void StartHooks()
		{
#if DEBUG
			return;
#endif
			log.Debug("Starting hooks");
			try
			{
				hook.StartMouseHook();
				defaultMouseActivity = 0;
			}
			catch (Exception ex)
			{
				defaultMouseActivity = -1;
				log.Error("Unable to start mouse hook", ex);
			}
			try
			{
				hook.StartKeyboardHook();
				defaultKeyboardActivity = 0;
			}
			catch (Exception ex)
			{
				defaultKeyboardActivity = -1;
				log.Error("Unable to start keyboard hook", ex);
			}
			log.Debug("Started hooks");
		}

		private void StopHooks()
		{
#if DEBUG
			return;
#endif
			log.Debug("Stopping hooks");
			defaultMouseActivity = 0;
			defaultKeyboardActivity = 0;
			try
			{
				hook.StopMouseHook(true);
			}
			catch (Exception ex)
			{
				log.Error("Unable to stop mouse hook", ex);
			}
			try
			{
				hook.StopKeyboardHook(true);
			}
			catch (Exception ex)
			{
				log.Error("Unable to stop keyboard hook", ex);
			}
			log.Debug("Stopped hooks");
		}

	}
}
