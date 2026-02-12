using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Microsoft.Win32;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.SystemEvents;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class SystemEventsCoordinator : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly SynchronizationContext context;
		private readonly CurrentWorkController currentWorkController;
		private readonly ISystemEventsService systemEventsService;
		private readonly IUserActivityService userActivityService;
		private volatile bool isWorkingBeforeStop;

		public SystemEventsCoordinator(ISystemEventsService systemEvents, CurrentWorkController workController, SynchronizationContext guiSynchronizationContext, IUserActivityService userActivity)
		{
			if (systemEvents == null || workController == null || guiSynchronizationContext == null || userActivity == null) throw new ArgumentNullException();
			context = guiSynchronizationContext;
			systemEventsService = systemEvents;
			currentWorkController = workController;
			userActivityService = userActivity;

			systemEventsService.PowerModeChanged += SystemEventsPowerModeChanged;
			systemEventsService.SessionSwitch += SystemEventsSessionSwitch;
		}

		public void Dispose()
		{
			systemEventsService.PowerModeChanged -= SystemEventsPowerModeChanged;
			systemEventsService.SessionSwitch -= SystemEventsSessionSwitch;
		}

		private void StopWork()
		{
			context.Post(_ => StopWorkFromGui(), null);
		}

		private void StopWorkFromGui()
		{
			isWorkingBeforeStop = currentWorkController.CurrentWorkState != WorkState.NotWorking;
			if (isWorkingBeforeStop)
			{
				currentWorkController.UserStopWork();
			}
		}

		private void StartWorkIfApplicable()
		{
			context.Post(_ =>
			{
				if ( ConfigManager.SetWorkStateAfterResume == SetWorkStateAfterResume.No || !isWorkingBeforeStop && ConfigManager.SetWorkStateAfterResume == SetWorkStateAfterResume.RetainPrevious ) return;
				currentWorkController.UserResumeWork(WorkStateChangeReason.AutoResume);
			}, null);
		}

		private void SystemEventsPowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			switch (e.Mode)
			{
				case PowerModes.Resume:
					log.Info("PowerModeChange: Resume");
					StartWorkIfApplicable();
					break;
				case PowerModes.Suspend:
					log.Info("PowerModeChange: Suspend");
					StopWork(); //performance critical
					break;
				default:
					return;
			}
		}

		private void SystemEventsSessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			log.Info("SessionSwitch: " + e.Reason);
			switch (e.Reason)
			{
				case SessionSwitchReason.ConsoleConnect:
					break;
				case SessionSwitchReason.ConsoleDisconnect:
					StopWork();
					break;
				case SessionSwitchReason.RemoteConnect:
					break;
				case SessionSwitchReason.RemoteDisconnect:
					StopWork();
					break;
				case SessionSwitchReason.SessionLogon:
					break;
				case SessionSwitchReason.SessionLogoff:
					break;
				case SessionSwitchReason.SessionLock:
					if (ConfigManager.IsWindows7) userActivityService.Stop();
					break;
				case SessionSwitchReason.SessionUnlock:
					if (ConfigManager.IsWindows7) userActivityService.Start();
					break;
				case SessionSwitchReason.SessionRemoteControl:
					break;
				default:
					return;
			}
		}
	}
}
