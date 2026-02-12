using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Desktop;
using Tct.ActivityRecorderClient.Capturing.EnvironmentInfo;
using Tct.ActivityRecorderClient.Capturing.Meeting;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Tct.ActivityRecorderClient.ClientErrorReporting;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Hotkeys;
using Tct.ActivityRecorderClient.InterProcess;
using Tct.ActivityRecorderClient.Meeting.Adhoc;
using Tct.ActivityRecorderClient.Meeting.CountDown;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Menu.Management;
using Tct.ActivityRecorderClient.Menu.Selector;
using Tct.ActivityRecorderClient.MessageNotifier;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.ProjectSync;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Rules.Actions;
using Tct.ActivityRecorderClient.Sleep;
using Tct.ActivityRecorderClient.SystemEvents;
using Tct.ActivityRecorderClient.Update;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient
{
	public interface IPlatformFactory
	{
		ISystemEventsService GetSystemEventsService();
		IDesktopCaptureService GetDesktopCaptureService(ISystemEventsService systemEventsService, IPluginCaptureService pluginCaptureService);
		ISleepRegulatorService GetSleepRegulatorService();
		INotificationService GetNotificationService();
		IUserActivityService GetUserActivityService();
		IEnvironmentInfoService GetEnvironmentInfoService();
		IUpdateService GetUpdateService();
		IRuleManagementService GetRuleManagementService(SynchronizationContext guiSynchronizationContext, CaptureCoordinator captureCoordinator);
		IPluginCaptureService GetPluginCaptureService();
		IMeetingCaptureService GetMeetingCaptureService();
		IAddressBookService GetAddressBookService();
		IProjectSyncService GetProjectSyncService();
		IMeetingCountDownService GetMeetingCountDownService(CurrentWorkController currentWorkController);
		IHotkeyService GetHotkeyService();
		IWorkSelectorService GetWorkSelectorService();
		IWorkManagementService GetWorkManagementService(CurrentWorkController currentWorkController, WorkItemManager workItemManager);
		IAdhocMeetingService GetAdhocMeetingService(CaptureCoordinator captureCoordinator);
		IWorkTimeService GetWorkTimeService(IWorkTimeQuery workTimeHistory);
		IRuleActionCoordinator GetRuleActionCoordinator();
		IEnumerable<IMenuPublisher> GetMenuPublishers();
		CaptureExtensionKey[] GetDefaultParallelPlugins();
		IErrorReporter GetErrorReporter();
		IInterProcessManager GetInterProcessManager(SynchronizationContext context, CurrentWorkController currentWorkController, MenuCoordinator menuCoordinator);
		SynchronizationContext GetGuiSynchronizationContext();
		IAutoStartHelper GetAutoStartHelper();
		IMessageService GetMessageService();
	}
}
