using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Desktop;
using Tct.ActivityRecorderClient.Capturing.EnvironmentInfo;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Sleep;
using Tct.ActivityRecorderClient.SystemEvents;
using Tct.ActivityRecorderClient.Update;
using Tct.ActivityRecorderClient.ViewMac;

namespace Tct.ActivityRecorderClient
{
	public static class Platform
	{
		public static readonly IPlatformFactory Factory = new PlatformMacFactory();

		public class PlatformMacFactory : IPlatformFactory
		{
			public ControllerCollection OwnedControllers { get; set; }

			public ISystemEventsService GetSystemEventsService()
			{
				return new SystemEventsMacService();
			}

			public IDesktopCaptureService GetDesktopCaptureService(ISystemEventsService systemEventsService)
			{
				return new DesktopCaptureMacService(systemEventsService);
			}

			public ISleepRegulatorService GetSleepRegulatorService()
			{
				return new SleepRegulatorMacService();
			}

			public INotificationService GetNotificationService()
			{
				return new NotificationMacService();
			}

			public IUserActivityService GetUserActivityService()
			{
				return new UserActivityMacService();
			}

			public IEnvironmentInfoService GetEnvironmentInfoService()
			{
				return new EnvironmentInfoMacService();
			}

			public IUpdateService GetUpdateService()
			{
				return new UpdateMacService();
			}

			public IRuleManagementService GetRuleManagementService(CaptureCoordinator captureCoordinator)
			{
				return new RuleManagementMacService(captureCoordinator, OwnedControllers);
			}
		}
	}
}
