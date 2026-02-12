using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Microsoft.Win32;

namespace Tct.ActivityRecorderClient.SystemEvents
{
	public class SystemEventsMacService : ISystemEventsService
	{
		private static readonly PowerModeChangedEventArgs sleepEventArgs = new PowerModeChangedEventArgs(PowerModes.Suspend);
		private static readonly PowerModeChangedEventArgs resumeEventArgs = new PowerModeChangedEventArgs(PowerModes.Resume);

		public event PowerModeChangedEventHandler PowerModeChanged;
		public event SessionSwitchEventHandler SessionSwitch; //todo

		private readonly NSObject sleepKey;
		private readonly NSObject resumeKey;

		public SystemEventsMacService()
		{
			using (var sleepString = new NSString("NSWorkspaceWillSleepNotification"))
			using (var resumeString = new NSString("NSWorkspaceDidWakeNotification"))
			{
				sleepKey = NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(sleepString, Sleep);
				resumeKey = NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(resumeString, Resume);
			}
			if (sleepKey == null || resumeKey == null)
			{
				this.Dispose();
				throw new Exception("Unable to add observer for " +
				                    ((sleepKey == null && resumeKey == null)
				 						? "sleep and resume"
				 						: (sleepKey == null ? "sleep" : "resume"))
				                    + " notifications");
			}
		}

		private void Sleep(NSNotification notification)
		{
			OnPowerModeChanged(sleepEventArgs);
		}

		private void Resume(NSNotification notification)
		{
			OnPowerModeChanged(resumeEventArgs);
		}

		private bool isDisposed;

		public void Dispose()
		{
			if (isDisposed)
				return;
			isDisposed = true;
			if (sleepKey != null)
			{
				NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver(sleepKey);
				sleepKey.Dispose();
			}
			if (resumeKey != null)
			{
				NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver(resumeKey);
				resumeKey.Dispose();
			}
		}

		private void OnPowerModeChanged(PowerModeChangedEventArgs e)
		{
			var handler = PowerModeChanged;
			if (handler != null)
				handler(this, e);
		}

	}
}

