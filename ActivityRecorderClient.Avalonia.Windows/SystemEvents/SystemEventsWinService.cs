using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using log4net;
using Microsoft.Win32;

namespace Tct.ActivityRecorderClient.SystemEvents
{
	/// <summary>
	/// Class for raising system events.
	/// </summary>
	/// <remarks>
	/// This will raise Suspend and Resume twice, but that is ok for now. (And I hope one will reach the GUI on Win7)
	/// </remarks>
	public class SystemEventsWinService : ISystemEventsService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ManagementEventWatcher eventWatcher;

		public event Forms.PowerModeChangedEventHandler PowerModeChanged;
		public event Forms.SessionSwitchEventHandler SessionSwitch;

		public SystemEventsWinService()
		{
			Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEventsPowerModeChanged; //this is raised on the GUI thread
			Microsoft.Win32.SystemEvents.SessionSwitch += SystemEventsSessionSwitch; //this is raised on the GUI thread

			//redundant check because we have some lost suspends on Win7
			try
			{
				eventWatcher = new ManagementEventWatcher(new WqlEventQuery("Win32_PowerManagementEvent"));
				eventWatcher.EventArrived += EventWatcherEventArrived; //this is NOT raised on the GUI thread
				eventWatcher.Start();
			}
			catch (Exception ex) //if it fails this class is still usable so swallow ex
			{
				log.Error("Unable to initialize ManagementEventWatcher", ex);
			}
		}

		private static readonly PowerModeChangedEventArgs suspendEventArgs = new PowerModeChangedEventArgs(PowerModes.Suspend);
		private static readonly PowerModeChangedEventArgs resumeEventArgs = new PowerModeChangedEventArgs(PowerModes.Resume);
		private void EventWatcherEventArrived(object sender, EventArrivedEventArgs e)
		{
			int eventType = Convert.ToInt32(e.NewEvent.Properties["EventType"].Value);
			switch (eventType)
			{
				case 4: //Suspend      
					OnPowerModeChanged(suspendEventArgs);
					break;
				case 7: //Resume
					OnPowerModeChanged(resumeEventArgs);
					break;
			}
		}

		private volatile PowerModes lastMode;
		private void OnPowerModeChanged(PowerModeChangedEventArgs e)
		{
			if (lastMode == e.Mode)
			{
				log.Debug("Filtered multiple eventType: " + e.Mode);
				return;
			}
			lastMode = e.Mode;
			PowerModeChanged?.Invoke(this, new Forms.PowerModeChangedEventArgs((Forms.PowerModes)e.Mode));
		}

		private void OnSessionSwitch(SessionSwitchEventArgs e)
		{
			var handler = SessionSwitch;
			if (handler != null) handler(this, new Forms.SessionSwitchEventArgs((Forms.SessionSwitchReason)e.Reason));
		}

		private void SystemEventsPowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			OnPowerModeChanged(e);
		}

		private void SystemEventsSessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			OnSessionSwitch(e);
		}

		private int isDisposed;
		public void Dispose()
		{
			if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
			Microsoft.Win32.SystemEvents.PowerModeChanged -= SystemEventsPowerModeChanged;
			Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEventsSessionSwitch;

			if (eventWatcher == null) return;
			try
			{
				eventWatcher.Stop();
			}
			catch (Exception ex)
			{
				log.Error("Unable to stop eventWatcher", ex);
			}
			eventWatcher.EventArrived -= EventWatcherEventArrived;
			eventWatcher.Dispose();
			log.Info("Event service Disposed");
		}
	}
}
