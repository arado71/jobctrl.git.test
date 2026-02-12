using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Stability;
using Tct.ActivityRecorderClient.Telemetry;
using ThreadState = System.Threading.ThreadState;

namespace Tct.ActivityRecorderClient.Capturing.Desktop
{
	/// <summary>
	/// Thread-safe class for capturing desktop layouts.
	/// We have to keep these in sync:
	/// DesktopCaptureManager -> WorkDetectorRules -> Censor -> CaptureManager (DesktopLayoutChanged, Stop, Start) [-> CurrentWorkController!
	/// DesktopCaptureManager - should take screenshots ?
	/// IDesktopCaptureService - should calculate visibilities and resolve urls for visible windows
	/// </summary>
	public class DesktopCaptureManager : StaPeriodicManager, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDesktopCaptureService desktopLayoutService; //all methods called from STA thread

		public int CaptureActiveWindowInterval { get { return ConfigManager.CaptureActiveWindowInterval; } }
		public int CaptureScreenShotInterval { get { return ConfigManager.CaptureScreenShotInterval; } }

		private readonly object thisLock = new object();
		private readonly object thisLockLayoutService = new object();
		private int isSendingVersion;
		private bool isSending;
		private WatchdogManager watchdogManager;
		private readonly int watchdogCheckInterval = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;

		public event EventHandler<DesktopCapturedEventArgs> DesktopCaptured;
		public event EventHandler<ThreadFrozenEventArgs> ThreadFrozen;

		public DesktopCaptureManager(Func<IDesktopCaptureService> desktopServiceFactory)
			: base(log, false, "DC")
		{
			if (desktopServiceFactory == null) throw new ArgumentNullException();
			desktopLayoutService = desktopServiceFactory(); //now we own desktopLayoutService so we have to Dispose it (that is why we don't simply pass an IDesktopCaptureService in the ctor)
			if (desktopLayoutService == null) throw new ArgumentNullException();
		}

		public DesktopCapture GetDesktopCapture()
		{
			DebugEx.EnsureSta();
			lock (thisLockLayoutService)
			{
				return desktopLayoutService.GetDesktopCapture(false);
			}
		}

		protected override void ManagerCallbackImpl()
		{
			using (TelemetryHelper.MeasureElapsed(TelemetryHelper.KeyCaptureTimer))
			{
				DebugEx.EnsureSta();
				if (watchdogManager != null) watchdogManager.Reset();
				int versionBefore, versionAfter;
				var isSendingBefore = GetIsSending(out versionBefore);
				var shouldSendCapture = isSendingBefore && ShouldSendCapture(); //shouldn't send capture if not working !  (because of the delaly of BG -> GUI marshall [we could send capture which was taken in offline state])
				var shouldTakeScreenShot = shouldSendCapture && (!ConfigManager.ClientDataCollectionSettings.HasValue || ConfigManager.ClientDataCollectionSettings.Value.HasFlag(ClientDataCollectionSettings.Screenshot)) && ShouldTakeShotForSend();
				DesktopCapture currentLayout;
				lock (thisLockLayoutService)
				{
					currentLayout = desktopLayoutService.GetDesktopCapture(shouldTakeScreenShot);
				}

				var isSendingAfter = GetIsSending(out versionAfter);
				if (versionBefore != versionAfter) return; //if the sending (i.e. working) state is changed during capture than drop it
				Debug.Assert(isSendingBefore == isSendingAfter);
				OnDesktopCaptured(currentLayout, shouldSendCapture, isSendingAfter); //it is still possible that shouldSendCapture is true but user will go offline and the capture will be dropped
			}
		}

		private bool GetIsSending(out int version)
		{
			lock (thisLock)
			{
				version = isSendingVersion;
				return isSending;
			}
		}

		public void SetIsSending(bool value)
		{
			lock (thisLock)
			{
				isSending = value;
				isSendingVersion++;
			}
		}

		private int nextSendTick = Environment.TickCount;
		private bool ShouldSendCapture()
		{
			var awInterval = CaptureActiveWindowInterval;
			if (awInterval <= 0) return false;
			var now = Environment.TickCount;

			if (nextSendTick - now > 0) return false;
			nextSendTick += awInterval;
			if (nextSendTick - now < 0) nextSendTick = now + awInterval; //we are behind schedule (we can be several times behind, so reset)
			return true;
		}

		private int nextShotTick = Environment.TickCount;
		private bool ShouldTakeShotForSend() //can only be called if we are sending a capture !
		{
			var awInterval = CaptureActiveWindowInterval;
			var ssInterval = CaptureScreenShotInterval;
			if (ssInterval <= 0) return false;
			var now = Environment.TickCount;

			if (ssInterval <= awInterval) //special case, we won't do any time checking, but take a screenshot for every send
			{
				nextShotTick = now + awInterval; //yes awInterval... (ssInterval can be very little)
				return true; //we canot have screenshots more frequent than active windows
			}

			if (nextShotTick - now > 0) return false;
			nextShotTick += ssInterval;
			if (nextShotTick - now < 0) nextShotTick = now + ssInterval; //we are behind schedule (we can be several times behind, so reset)
			return true;
		}

		public override void Start(int firstDueTime = 0)
		{
			base.Start(firstDueTime);
			var raiseInterval = ConfigManager.CapturingDeadlockInMins * 60 * 1000;
			if (raiseInterval == 0) return;
			if (watchdogManager != null)
			{
				watchdogManager.RaiseInterval = raiseInterval;
				watchdogManager.Start();
				return;
			}

			watchdogManager = new WatchdogManager(watchdogCheckInterval, raiseInterval);
			var watchedThread = StaThread;
			watchdogManager.MissingReset += (__, ___) =>
			{
				StackTrace stackTrace = null;
				try
				{
					if (watchedThread != null)
					{
						log.Info("Watched thread state is: " + watchedThread.ThreadState);
						var shouldSuspend = (watchedThread.ThreadState & (ThreadState.Suspended | ThreadState.SuspendRequested)) == 0;
						try
						{
#pragma warning disable 618
							if (shouldSuspend) watchedThread.Suspend(); //the process will go down, so obsolote method is not an issue here
							log.Info("Watched thread state is: " + watchedThread.ThreadState);
							stackTrace = new StackTrace(false); // TODO: mac
							log.Fatal("Thread failed to reset Watchdog, watched thead's stack is:" + Environment.NewLine + stackTrace.ToString());
							if (shouldSuspend) watchedThread.Resume();
							log.Info("Watched thread state is: " + watchedThread.ThreadState);
#pragma warning restore 618
						}
						catch (Exception ex)
						{
							log.Warn("Unable to suspend/resume thread", ex);
						}
					}
					else
					{
						log.Fatal("Thread failed to reset Watchdog");
					}
				}
				catch (Exception ex)
				{
					log.ErrorAndFail("Unable to get stack dump for watched thread", ex);
				}
				//todo this is too common atm. figure out sg smarter!
				//ThreadPool.QueueUserWorkItem(_ => { throw new Exception("Watchdog was not reset in time"); }); //bring down the process and hopefully they will send an error riport
				OnThreadFrozen(stackTrace);
			};
			watchdogManager.Start(watchdogCheckInterval);
		}

		public override void Stop()
		{
			if (watchdogManager != null) watchdogManager.Stop();
			base.Stop();
		}

		protected override int ManagerCallbackInterval
		{
			get { return ConfigManager.RuleMatchingInterval; }
		}

		private void OnDesktopCaptured(DesktopCapture dl, bool shouldSend, bool isWorking)
		{
			var handler = DesktopCaptured;
			if (handler != null) handler(this, new DesktopCapturedEventArgs(dl, shouldSend, isWorking));
		}

		private void OnThreadFrozen(StackTrace stackTrace)
		{
			var evt = ThreadFrozen;
			if (evt != null) evt(this, new ThreadFrozenEventArgs(StaThread, stackTrace));
		}

		private int isDisposed;
		public void Dispose()
		{
			DebugEx.EnsureSta();
			if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
			if (desktopLayoutService == null) return;
			desktopLayoutService.Dispose();
		}
	}
}
