using System;
using System.Threading;
using log4net;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.Capturing.UserActivity
{
	/// <summary>
	/// User activity mac service. Qucik and dirty implementation.
	/// </summary>
	/// <remarks>
	/// Global monitor won't capture events if JobCTRL is active, but Local monitor would crash.
	/// </remarks>
	//http://developer.apple.com/library/mac/#DOCUMENTATION/Cocoa/Conceptual/EventOverview/MonitoringEvents/MonitoringEvents.html
	//http://developer.apple.com/library/mac/#documentation/Carbon/Reference/QuartzEventServicesRef/Reference/reference.html
	//http://stackoverflow.com/questions/6657511/osx-carbon-quartz-event-taps-to-get-keyoard-input
	public class UserActivityMacService : IUserActivityService
	{
		//private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private NSObject evtMonitor;
		private int recording;
		private int keyboardAct;
		private int mouseAct;

		public UserActivityMacService()
		{
			evtMonitor = NSEvent.AddGlobalMonitorForEventsMatchingMask(NSEventMask.KeyUp
				| NSEventMask.MouseMoved | NSEventMask.ScrollWheel
			    | NSEventMask.LeftMouseUp | NSEventMask.RightMouseUp | NSEventMask.OtherMouseUp, HookEvent);
			if (evtMonitor == null)
				throw new Exception("Unable to add global event monitor");
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
		}

		private void HookEvent(NSEvent evt)
		{
			if (Interlocked.CompareExchange(ref recording, 0, 0) == 0)
				return;
			if (evt.Type == NSEventType.KeyUp)
			{
				Interlocked.Increment(ref keyboardAct);
			}
			else
			{
				Interlocked.Increment(ref mouseAct);
			}
		}
	}
}

