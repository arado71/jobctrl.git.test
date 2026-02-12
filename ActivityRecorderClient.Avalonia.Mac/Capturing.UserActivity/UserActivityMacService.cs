using Avalonia.Threading;
using log4net;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.UserActivity
{
	/// <summary>
	/// User activity mac service. Qucik and dirty implementation.
	/// </summary>
	/// <remarks>
	/// Global monitor won't capture events if JobCTRL is active, so we also need a Local monitor for that.
	/// </remarks>
	//http://developer.apple.com/library/mac/#DOCUMENTATION/Cocoa/Conceptual/EventOverview/MonitoringEvents/MonitoringEvents.html
	//http://developer.apple.com/library/mac/#documentation/Carbon/Reference/QuartzEventServicesRef/Reference/reference.html
	//http://stackoverflow.com/questions/6657511/osx-carbon-quartz-event-taps-to-get-keyoard-input
	public class UserActivityMacService : IUserActivityService
	{
		//private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private NSObject evtMonitor;
		private NSObject localEvtMonitor;
		private int recording;
		private int keyboardAct;
		private int mouseAct;
		private int? lastKeyboardActivity;
		private int? lastMouseActivity;

		public UserActivityMacService()
		{
			var mask = NSEventMask.KeyUp | NSEventMask.MouseMoved | NSEventMask.ScrollWheel
				| NSEventMask.LeftMouseUp | NSEventMask.RightMouseUp | NSEventMask.OtherMouseUp;
			evtMonitor = NSEvent.AddGlobalMonitorForEventsMatchingMask(mask, HookEvent);
			if (evtMonitor == null)
				throw new Exception("Unable to add global event monitor");

			DebugEx.EnsureGuiThread(); // local registration must be called on the UI thread
			localEvtMonitor = NSEvent.AddLocalMonitorForEventsMatchingMask(mask, LocalHookEvent);
		}

		public void Start()
		{
			Interlocked.Exchange(ref recording, 1);
		}

		public void Stop()
		{
			Interlocked.Exchange(ref recording, 0);
		}

		public void GetAndResetCounters(out int keyboardActivity, out int mouseActivity)
		{
			keyboardActivity = Interlocked.Exchange(ref keyboardAct, 0);
			mouseActivity = Interlocked.Exchange(ref mouseAct, 0);
		}

		private bool isDisposed;

		public void Dispose()
		{
			if (isDisposed)
				return;
			isDisposed = true;
			NSEvent.RemoveMonitor(evtMonitor);
			evtMonitor.Dispose();

			DebugEx.EnsureGuiThread();
			NSEvent.RemoveMonitor(localEvtMonitor);
			localEvtMonitor.Dispose();
		}

		private void HookEvent(NSEvent evt)
		{
			if (Interlocked.CompareExchange(ref recording, 0, 0) == 0)
				return;
			if (evt.Type == NSEventType.KeyUp)
			{
				lastKeyboardActivity = Environment.TickCount;
				Interlocked.Increment(ref keyboardAct);
			}
			else
			{
				lastMouseActivity = Environment.TickCount;
				Interlocked.Increment(ref mouseAct);
			}
		}

		private NSEvent LocalHookEvent(NSEvent evt)
		{
			HookEvent(evt);
			return evt;
		}

		public int? GetLastActivity()
		{
			return lastMouseActivity > lastKeyboardActivity ? lastMouseActivity : lastKeyboardActivity;
		}

		public int? GetLastMouseActivityTime()
		{
			return lastMouseActivity;
		}

		public int? GetLastKeyboardActivityTime()
		{
			return lastKeyboardActivity;
		}
	}
}

