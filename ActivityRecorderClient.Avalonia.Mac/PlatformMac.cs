using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Desktop;
using Tct.ActivityRecorderClient.Capturing.EnvironmentInfo;
using Tct.ActivityRecorderClient.Capturing.Meeting;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Tct.ActivityRecorderClient.ClientErrorReporting;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Hotkeys;
using Tct.ActivityRecorderClient.InterProcess;
using Tct.ActivityRecorderClient.Mac.Meeting.Adhoc;
using Tct.ActivityRecorderClient.Meeting;
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
using Tct.ActivityRecorderClient.ViewMac;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient
{
	public static class PlatformMac
	{
		public static readonly IPlatformFactory Factory = new PlatformFactory();

		public class PlatformFactory : IPlatformFactory
		{
			public ControllerCollection OwnedControllers { get; set; }

            public SynchronizationContext GuiSynchronizationContext { get ; set; }

			public ISystemEventsService GetSystemEventsService()
			{
				return new SystemEventsMacService();
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
				return new RuleManagementMacService(captureCoordinator, OwnedControllers, GetGuiSynchronizationContext(), GetNotificationService());
			}

            // new interfaces

            public IDesktopCaptureService GetDesktopCaptureService(ISystemEventsService systemEventsService, IPluginCaptureService pluginCaptureService)
            {
				// TODO: mac - pluginCaptureService
				return new DesktopCaptureMacService(systemEventsService, pluginCaptureService);
			}

            public IRuleManagementService GetRuleManagementService(SynchronizationContext guiSynchronizationContext, CaptureCoordinator captureCoordinator)
            {
				return new DummyRuleManagementService();
			}

            public IPluginCaptureService GetPluginCaptureService()
            {
				return new PluginCaptureMacService();
			}

            public IMeetingCaptureService GetMeetingCaptureService()
            {
				return new DummyMeetingCaptureService();
			}

            public IAddressBookService GetAddressBookService()
            {
				return new DummyAddressBookService();
			}

            public IProjectSyncService GetProjectSyncService()
            {
				return new DummyProjectSyncService();
			}

            public IMeetingCountDownService GetMeetingCountDownService(CurrentWorkController currentWorkController)
            {
				return new DummyMeetingCountDownService();
			}

            public IHotkeyService GetHotkeyService()
            {
				return HotkeyMacService.Instance;
			}

            public IWorkSelectorService GetWorkSelectorService()
            {
                return new DummyWorkSelectorService();
			}

            public IWorkManagementService GetWorkManagementService(CurrentWorkController currentWorkController, WorkItemManager workItemManager)
            {
				return new DummyWorkManagementService();
			}

            public IAdhocMeetingService GetAdhocMeetingService(CaptureCoordinator captureCoordinator)
            {
				return new AdhocMeetingMacService(captureCoordinator);
			}

            public IWorkTimeService GetWorkTimeService(IWorkTimeQuery workTimeHistory)
            {
				return new DummyWorkTimeService();
			}

            public IRuleActionCoordinator GetRuleActionCoordinator()
            {
               return new DummyRuleActionCoordinator();
            }

            public IEnumerable<IMenuPublisher> GetMenuPublishers()
            {
                // TODO: mac
                yield break;
            }

            public CaptureExtensionKey[] GetDefaultParallelPlugins()
            {
				return [];
            }

            public IErrorReporter GetErrorReporter()
            {
                throw new NotImplementedException();
            }

            public IInterProcessManager GetInterProcessManager(SynchronizationContext context, CurrentWorkController currentWorkController, MenuCoordinator menuCoordinator)
            {
				return new DummyInterProcessManager();
			}

            public SynchronizationContext GetGuiSynchronizationContext()
            {
                return GuiSynchronizationContext ?? throw new Exception("GuiSynchronizationContext is null");
            }

            public IAutoStartHelper GetAutoStartHelper()
            {
				return new DummyAutoStartHelper();
			}

            public IMessageService GetMessageService()
            {
				return new DummyMessageService();
			}
        }
	}
}

// TODO: mac
class DummyWorkSelectorService : IWorkSelectorService
{
	public event EventHandler<Tct.ActivityRecorderClient.SingleValueEventArgs<WorkDataWithParentNames>> WorkSelected;

	public void ShowSelectWorkGui(ClientMenuLookup menuLookup, string title, string description)
	{
	}

	public void UpdateMenu(ClientMenuLookup menuLookup)
	{
	}
}

class DummyWorkManagementService : IWorkManagementService
{
	public event EventHandler MenuRefreshNeeded;
	public event EventHandler<Tct.ActivityRecorderClient.SingleValueEventArgs<TaskReasons>> OnTaskReasonsChanged;

	public void DisplayCloseWorkGui(WorkData workToClose)
	{
	}

	public void DisplayCreateWorkGui()
	{
	}

	public void DisplayReasonWorkGui(WorkData workToComment)
	{
	}

	public void DisplayUpdateWorkGui(WorkData workToUpdate)
	{
	}

	public bool DisplayWarnNotificationIfApplicable()
	{
		return false;
	}

	public void DisplayWorkDetailsGui(WorkData workToShow)
	{
	}

	public void Dispose()
	{
	}

	public TimeSpan? GetTotalWorkTimeForWork(int workId)
	{
		return null;
	}

	public void SetCannedCloseReasons(CannedCloseReasons reasons)
	{
	}

	public void SetSimpleWorkTimeStats(SimpleWorkTimeStats stats)
	{
	}

	public void Start()
	{
	}

	public void Stop()
	{
	}

	public void UpdateMenu(ClientMenuLookup menuLookup)
	{
	}
}

class DummyRuleManagementService : IRuleManagementService
{
	public void DisplayLearnRuleFromCaptureGui(IWorkChangingRule matchingRule, DesktopCapture desktopCapture, DesktopWindow matchedWindow)
	{
	}

	public void DisplayWorkDetectorRuleDeletingGui()
	{
	}

	public void DisplayWorkDetectorRulesEditingGui(bool hotKeyPressed)
	{
	}

	public void Dispose()
	{
	}

	public void SetLearningRuleGenerators(IEnumerable<RuleGeneratorData> learningRuleGenerators)
	{
	}

	public bool ShouldSkipLoadingUserRule(WorkDetectorRule rule)
	{
		return true;
	}
}

class DummyMeetingCaptureService : IMeetingCaptureService
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

class DummyAddressBookService : IAddressBookService
{
	public bool IsAddressBookServiceAvailable => false;

	public List<MeetingAttendee> DisplaySelectNamesDialog(nint parentWindowHandle)
	{
		return new List<MeetingAttendee>();
	}

	public void Dispose()
	{
	}

	public void Initialize()
	{
	}
}

class DummyWorkTimeService : IWorkTimeService
{
	public GeneralResult<bool> CreateWork(WorkDataWithParentNames work, Interval interval, string comment, bool force = false)
	{
		return new GeneralResult<bool>() { Result = false };
	}

	public GeneralResult<bool> DeleteInterval(Interval interval, string comment, bool force = false)
	{
		return new GeneralResult<bool>() { Result = false };
	}

	public GeneralResult<bool> DeleteWork(DeviceWorkInterval originalInterval, string comment, bool force = false)
	{
		return new GeneralResult<bool>() { Result = false };
	}

	public GeneralResult<IList<Interval>> GetFreeIntervals(Interval interval, Interval localException = null)
	{
		return new GeneralResult<IList<Interval>>() { Result = new List<Interval>() };
	}

	public GeneralResult<Interval> GetLocalDayInterval(DateTime localDate)
	{
		return new GeneralResult<Interval>() { Result = new Interval() };
	}

	public GeneralResult<DeviceWorkIntervalLookup> GetStats(Interval interval)
	{
		return new GeneralResult<DeviceWorkIntervalLookup>();
	}

	public GeneralResult<IEnumerable<WorkOrProjectWithParentNames>> GetWorkOrProjectWithParentNames(IEnumerable<int> workIds)
	{
		throw new NotImplementedException();
	}

	public GeneralResult<bool> ModifyInterval(Interval interval, WorkDataWithParentNames newWork, string comment, bool force = false)
	{
		throw new NotImplementedException();
	}

	public GeneralResult<bool> ModifyWork(DeviceWorkInterval originalInterval, WorkDataWithParentNames workData, IEnumerable<Interval> newIntervals, string comment, bool force = false)
	{
		throw new NotImplementedException();
	}

	public void ShowModification(DateTime? localDay = null)
	{
	}

	public void ShowModifyInterval(Interval localInterval)
	{
	}

	public void ShowModifyWork(DeviceWorkInterval workInterval)
	{
	}

	public GeneralResult<bool> UndeleteWork(DeviceWorkInterval workInterval)
	{
		throw new NotImplementedException();
	}
}

class DummyRuleActionCoordinator : IRuleActionCoordinator
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

class DummyAutoStartHelper : IAutoStartHelper
{
	public void Register(IUpdateService updateService)
	{
	}
}

class DummyMessageService : IMessageService
{
	public DateTime? SetPCReadAt(int messageId)
	{
		return null;
	}

	public void ShowMessages()
	{
	}
}

class DummyProjectSyncService : IProjectSyncService
{
	public void ShowInfo(string text)
	{
	}

	public void ShowSync()
	{
	}
}

class DummyMeetingCountDownService : IMeetingCountDownService
{
	public bool IsPermanent => false;

	public bool ResumeWorkOnClose => true;

	public ManualWorkItem CurrentWorkItem => throw new NotImplementedException();

	public bool IsWorking => false;

	public string StateString => "";

	public event EventHandler<Tct.ActivityRecorderClient.SingleValueEventArgs<ManualWorkItem>> ManualWorkItemCreated;

	public void CheckUnfinishedTimedTask()
	{
	}

	public void RequestKickWork()
	{
	}

	public MutualWorkTypeInfo RequestStopWork(bool isForced)
	{
		throw new NotImplementedException();
	}

	public void StartWork(WorkData workData, bool isPermanent, bool isForced)
	{
	}
}

class DummyInterProcessManager : IInterProcessManager
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
