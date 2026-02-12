using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Desktop;
using Tct.ActivityRecorderClient.Capturing.EnvironmentInfo;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Tct.ActivityRecorderClient.Capturing.Meeting;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
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
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.ProjectSync;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Sleep;
using Tct.ActivityRecorderClient.SystemEvents;
using Tct.ActivityRecorderClient.Update;
using Tct.ActivityRecorderClient.View;
using Tct.ActivityRecorderClient.WorktimeHistory;
using Tct.ActivityRecorderClient.Rules.Actions;
using Tct.ActivityRecorderClient.MessageNotifier;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient
{
    internal static class PlatformWin
    {
        public static readonly IPlatformFactory Factory = new PlatformFactory();

        public class PlatformFactory : IPlatformFactory
        {
            public ActivityRecorderForm MainForm { get; set; }

            public ISystemEventsService GetSystemEventsService()
            {
                return new SystemEventsWinService();
            }

            public IDesktopCaptureService GetDesktopCaptureService(ISystemEventsService systemEventsService, IPluginCaptureService pluginCaptureService)
            {
                return new DesktopCaptureWinService(systemEventsService, pluginCaptureService);
            }

            public ISleepRegulatorService GetSleepRegulatorService()
            {
                return new SleepRegulatorWinService();
            }

            private static INotificationService notificationInstance = null;
            public INotificationService GetNotificationService()
            {
                if (notificationInstance == null) notificationInstance = new NotificationWinService();
                return notificationInstance;
            }

            public IUserActivityService GetUserActivityService()
            {
                return UserActivityWinService.Instance;
            }

            public IEnvironmentInfoService GetEnvironmentInfoService()
            {
                return new EnvironmentInfoWinService();
            }

            public IUpdateService GetUpdateService()
            {
                return UpdateWinService.Instance;
            }

            public IRuleManagementService GetRuleManagementService(SynchronizationContext guiSynchronizationContext, CaptureCoordinator captureCoordinator)
            {
                Debug.Assert(MainForm != null);
                return new RuleManagementWinService(guiSynchronizationContext, captureCoordinator, GetNotificationService(), MainForm);
            }

            public IPluginCaptureService GetPluginCaptureService()
            {
                return new PluginCaptureWinService();
            }

            public IMeetingCaptureService GetMeetingCaptureService()
            {
                return new MeetingCaptureDummyService();
            }

            public IAddressBookService GetAddressBookService()
            {
                return new AddressBookServiceDummyService();

            }

            public IProjectSyncService GetProjectSyncService()
            {
#if ProjectSync
				Debug.Assert(workTimeInstance != null, "workTimeInstance not initialized"); // hax quick and dirty solution
				return new ProjectSyncWinService(workTimeInstance as WorkTimeService, GetNotificationService());
#else
                return new ProjectSyncDummyService();
#endif
            }

            public IHotkeyService GetHotkeyService()
            {
                return HotkeyWinService.Instance;
            }

            public IWorkSelectorService GetWorkSelectorService()
            {
                Debug.Assert(MainForm != null);
                return new WorkSelectorWinService(MainForm);
            }

            private static IWorkManagementService workManagementInstance = null;
            public IWorkManagementService GetWorkManagementService(CurrentWorkController currentWorkController, WorkItemManager workItemManager)
            {
                Debug.Assert(MainForm != null);
                if (workManagementInstance == null) workManagementInstance = new WorkManagementWinService(GetNotificationService(), currentWorkController, workItemManager, MainForm);
                return workManagementInstance;
            }

            public IAdhocMeetingService GetAdhocMeetingService(CaptureCoordinator captureCoordinator)
            {
                Debug.Assert(MainForm != null);
                return new AdhocMeetingWinService(MainForm, captureCoordinator);
            }

            public IMeetingCountDownService GetMeetingCountDownService(CurrentWorkController currentWorkController)
            {
                Debug.Assert(MainForm != null);
                return new MeetingCountDownWinService(GetNotificationService(), currentWorkController, MainForm);
            }

            private static IWorkTimeService workTimeInstance = null;
            public IWorkTimeService GetWorkTimeService(IWorkTimeQuery workTimeHistory)
            {
                Debug.Assert(MainForm != null);
                if (workTimeInstance == null)
                {
#if NET4
					workTimeInstance = new WorkTimeWinService(workTimeHistory, MainForm);
#else
                    workTimeInstance = new WorkTimeDummyService(workTimeHistory);
#endif
                }

                return workTimeInstance;
            }

            public IEnumerable<IMenuPublisher> GetMenuPublishers()
            {
                yield break;
            }

            public IRuleActionCoordinator GetRuleActionCoordinator()
            {
                return new RuleActionDummyCoordinator();
            }

            public CaptureExtensionKey[] GetDefaultParallelPlugins()
            {
                return new[] {
                    new CaptureExtensionKey(PluginChromeUrl.PluginId, PluginChromeUrl.KeyUrl),
                    new CaptureExtensionKey(PluginFirefoxUrl.PluginId, PluginFirefoxUrl.KeyUrl),
                    new CaptureExtensionKey(PluginInternetExplorerUrl.PluginId, PluginInternetExplorerUrl.KeyUrl),
                    new CaptureExtensionKey(PluginEdgeUrl.PluginId, PluginEdgeUrl.KeyUrl),
                    new CaptureExtensionKey(PluginEdgeBlinkUrl.PluginId, PluginEdgeBlinkUrl.KeyUrl),
                    new CaptureExtensionKey(PluginBraveUrl.PluginId, PluginBraveUrl.KeyUrl),
                    new CaptureExtensionKey(PluginDragonUrl.PluginId, PluginDragonUrl.KeyUrl),
                    new CaptureExtensionKey(PluginOperaUrl.PluginId, PluginOperaUrl.KeyUrl),
                    new CaptureExtensionKey(PluginVivaldiUrl.PluginId, PluginVivaldiUrl.KeyUrl),
                };
            }

            public IErrorReporter GetErrorReporter()
            {
                return new ClientErrorWinReporter();
            }

            public IInterProcessManager GetInterProcessManager(SynchronizationContext context, CurrentWorkController currentWorkController, MenuCoordinator menuCoordinator)
            {
                return new InterProcessDummyManager();
            }

            public SynchronizationContext GetGuiSynchronizationContext()
            {
                return MainForm?.GuiContext;
            }

            public IAutoStartHelper GetAutoStartHelper()
            {
                return new AutoStartHelper();
            }

            public IMessageService GetMessageService()
            {
                return MainForm?.MessageService;
            }
        }
    }


    public class ProjectSyncDummyService : IProjectSyncService
    {
        public void ShowSync()
        {
            throw new NotImplementedException();
        }

        public void ShowInfo(string text)
        {
            throw new NotImplementedException();
        }
    }

    public class AddressBookServiceDummyService : IAddressBookService
    {
        public bool IsAddressBookServiceAvailable => false;

        public List<MeetingAttendee> DisplaySelectNamesDialog(nint parentWindowHandle)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }
    }

    public class RuleActionDummyCoordinator : IRuleActionCoordinator
    {
        public bool ContainsRuleAction(IWorkChangingRule rule)
        {
            return false;
        }

        public IDisposable GetExecuter(IWorkChangingRule rule, AssignData assignData)
        {
            return null;
        }
    }

    public class InterProcessDummyManager : IInterProcessManager
    {
        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void UpdateMenu(ClientMenu clientMenu)
        {
        }
    }

    public class MeetingCaptureDummyService : IMeetingCaptureService
    {
        public string[] ProcessNames => [];

        public List<FinishedMeetingEntry> CaptureMeetings(IList<string> calendarAccountEmails, DateTime startDate, DateTime endDate)
        {
            return new List<FinishedMeetingEntry>();
        }

        public void Dispose()
        {
        }

        public string GetVersionInfo()
        {
            return "";
        }

        public void Initialize()
        {
        }
    }
}
