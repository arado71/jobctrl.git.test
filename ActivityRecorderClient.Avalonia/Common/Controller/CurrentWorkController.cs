using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Configuration;
using Tct.ActivityRecorderClient.Meeting.Adhoc;
using Tct.ActivityRecorderClient.Meeting.CountDown;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Menu.Management;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Controller
{
	/// <summary>
	/// Manages the current work on the GUI thread
	/// </summary>
	public class CurrentWorkController : CurrentWorkControllerBase, IMutualWorkTypeService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string LastWorkDataPath { get { return "LastWorkData-" + ConfigManager.UserId; } }

		private static readonly TimeSpan nfInvalidWorkDuration = TimeSpan.FromMinutes(1);
		private static readonly TimeSpan nfNoWorkDuration = TimeSpan.FromMinutes(1);
		private static readonly TimeSpan nfMoreWorksInCatDuration = TimeSpan.FromSeconds(20);

		public static readonly Color WorkingColor = Color.FromArgb(255, 119, 185, 0);
		public static readonly Color NotWorkingColor = Color.FromArgb(255, 46, 18);

		public MutualWorkTypeCoordinator MutualWorkTypeCoordinator { get { return mutualWorkTypeContainer; } }
		public IMeetingCountDownService MeetingCountDownService { get { return meetingCountDownService; } }
		public IAdhocMeetingService AdhocMeetingService { get { return adhocMeetingService; } }

		private readonly WorkAndLayoutGuiCoordinator workAndLayoutGuiCoordinator;
		private readonly INotificationService notificationService;
		private readonly IMeetingCountDownService meetingCountDownService;
		private readonly IAdhocMeetingService adhocMeetingService;
		private readonly MutualWorkTypeCoordinator mutualWorkTypeContainer;
		private ClientMenuLookup clientMenuLookup = new ClientMenuLookup();
		private WorkData queuedWorkChange;
		private CaptureCoordinator parent;

		public CurrentWorkController(WorkAndLayoutGuiCoordinator workAndLayoutGuiCoordinator, ClientMenuLookup menuLookup, INotificationService notificationService, TimeManager timeManager, CaptureCoordinator captureCoordinator)
		{
			this.workAndLayoutGuiCoordinator = workAndLayoutGuiCoordinator;
			this.notificationService = notificationService;
			mutualWorkTypeContainer = new MutualWorkTypeCoordinator(timeManager, notificationService);
			clientMenuLookup = menuLookup;
			meetingCountDownService = Platform.Factory.GetMeetingCountDownService(this);
			mutualWorkTypeContainer.Add(meetingCountDownService);
			mutualWorkTypeContainer.Add(this);
			parent = captureCoordinator;
			adhocMeetingService = Platform.Factory.GetAdhocMeetingService(captureCoordinator);
			mutualWorkTypeContainer.Add(adhocMeetingService);
			IsCurrentWorkValid = true;
		}

		private TimeSpan nfMenuChangeDuration
		{
			get { return GetValidDuration(ConfigManager.LocalSettingsForUser.MenuChangeWarnDuration); }
		}

		private TimeSpan nfNotWorkingDuration
		{
			get { return GetValidDuration(ConfigManager.LocalSettingsForUser.NotWorkingWarnDuration); }
		}

		private TimeSpan nfNewWorkDuration
		{
			get { return GetValidDuration(ConfigManager.LocalSettingsForUser.WorkingWarnDuration); }
		}

		//Current work name or the last user selected work if not working
		private string currentOrLastWorkName;
		public string CurrentOrLastWorkName
		{
			get { return currentOrLastWorkName; }
			private set { UpdateField(ref currentOrLastWorkName, value, "CurrentOrLastWorkName"); }
		}

		private string currentOrLastWorkNameInTwoLines;
		public string CurrentOrLastWorkNameInTwoLines
		{
			get { return currentOrLastWorkNameInTwoLines; }
			private set { UpdateField(ref currentOrLastWorkNameInTwoLines, value, "CurrentOrLastWorkNameInTwoLines"); }
		}

		private bool isCurrentWorkValid;
		public bool IsCurrentWorkValid
		{
			get { return isCurrentWorkValid; }
			private set { UpdateField(ref isCurrentWorkValid, value, "IsCurrentWorkValid"); }
		}

		private bool isShuttingDown;
		public bool IsShuttingDown
		{
			get { return isShuttingDown; }
			set { UpdateField(ref isShuttingDown, value, "IsShuttingDown"); }
		}

		private bool isRuleOverrideEnabled;
		public bool IsRuleOverrideEnabled
		{
			get { return isRuleOverrideEnabled; }
			set { UpdateField(ref isRuleOverrideEnabled, value, "IsRuleOverrideEnabled"); }
		}

		private bool isOnline = true;
		public bool IsOnline
		{
			get { return isOnline; }
			set { UpdateField(ref isOnline, value, "IsOnline"); }
		}

		public bool IsChangeReasonWasUserSelectOrResume
		{
			get
			{
				return LastWorkStateChangeReason == WorkStateChangeReason.UserSelect
					   || LastWorkStateChangeReason == WorkStateChangeReason.UserResume;
			}
		}

		protected override void StopWork()
		{
			workAndLayoutGuiCoordinator.StopWork();
		}

		protected override void StartWork(WorkData workData)
		{
			workAndLayoutGuiCoordinator.StartWork(workData);
		}

		public override void UserStartWork(WorkData workData)
		{
			Debug.Assert(workData != null && workData.Id.HasValue);
			WorkDataWithParentNames realWorkData;
			if (clientMenuLookup.WorkDataById.TryGetValue(workData.Id.Value, out realWorkData) && realWorkData.WorkData.ManualAddWorkDuration.HasValue)
			{
				meetingCountDownService.StartWork(realWorkData.WorkData, true, false); //permanent but we don't force start so user has to confirm (will see a msgbox)
				return;
			}

			using (MutualWorkTypeCoordinator.StartStateChangeTransaction())
			{
				//CheckIfAutomaticRuleMode();
				base.UserStartWork(workData);
			}
		}

		public override void UserStopWork()
		{
			using (MutualWorkTypeCoordinator.StartStateChangeTransaction())
			{
				base.UserStopWork();
			}
		}

		public override void TempStopWork()
		{
			using (MutualWorkTypeCoordinator.StartStateChangeTransaction())
			{
				base.TempStopWork();
			}
		}

		public override void UserResumeWork(WorkStateChangeReason wscr = WorkStateChangeReason.UserResume)
		{
			if (IsShuttingDown) return;
			using (MutualWorkTypeCoordinator.StartStateChangeTransaction())
			{
				base.UserResumeWork(wscr);
			}
		}

		public void StartOrQueueWork(WorkData workData)
		{
			if (workData == null || !workData.Id.HasValue) return;
			ResetQueuedWork();
			queuedWorkChange = workData;
			log.InfoFormat("Queuing work {0} to start when applicable", queuedWorkChange.Id);
			StartQueuedWorkIfApplicable();
		}

		private void StartQueuedWorkIfApplicable()
		{
			if (queuedWorkChange == null) return;
			WorkDataWithParentNames targetWork;
			if (clientMenuLookup != null && clientMenuLookup.WorkDataById.TryGetValue(queuedWorkChange.Id.Value, out targetWork))
			{
				log.InfoFormat("Starting queued work {0}", queuedWorkChange.Id);
				UserStartWork(targetWork.WorkData);
			}
		}

		private void ResetQueuedWork()
		{
			if (queuedWorkChange == null) return;
			log.Info("Clearing queued work");
			queuedWorkChange = null;
		}

		public override void TempEndEffect()
		{
			if (meetingCountDownService.IsWorking && !meetingCountDownService.IsPermanent)
			{
				log.Debug("RequestStopWork CountDownTemp End"); //mutualWorkTypeContainer.RequestStopWork(false, "CountDownTemp End"); //we don't want to flood the log while confirmation diag is open
				using (MutualWorkTypeCoordinator.StartStateChangeTransaction())
				{
					var info = meetingCountDownService.RequestStopWork(false); //do we need to force ?
					if (info.CanStartWork && info.ResumeWorkOnClose)
					{
						UserResumeWork();
					}

					return;
				}
			}

			using (MutualWorkTypeCoordinator.StartStateChangeTransaction())
			{
				base.TempEndEffect();
			}
		}

		public void TempOrPermStartWork(WorkData workData, bool isPermanent, bool isEnabledWhileNotWorking)
		{
			if (meetingCountDownService.IsWorking && (meetingCountDownService.IsPermanent && !ConfigManager.ForceCountdownRules)
				|| adhocMeetingService.IsWorking)
			{
				return; //don't try to cancel meeting forms due to rules
			}
			if (meetingCountDownService.IsWorking && meetingCountDownService.IsPermanent)
			{
				if (!isEnabledWhileNotWorking) return;
				using (MutualWorkTypeCoordinator.StartStateChangeTransaction())
				{
					var info = meetingCountDownService.RequestStopWork(true);
					if (info.CanStartWork && info.ResumeWorkOnClose)
					{
						UserResumeWork();
					}
				}
			} else if (meetingCountDownService.IsWorking && !meetingCountDownService.IsPermanent && meetingCountDownService.CurrentWorkItem.WorkId != workData.Id)
			{
				TempEndEffect(); //we have to resume work first (from countdown) in order to switch to an other work
			}
			WorkDataWithParentNames realWorkData;
			if ((IsWorking || isEnabledWhileNotWorking)
				&& clientMenuLookup.WorkDataById.TryGetValue(workData.Id.Value, out realWorkData) && realWorkData.WorkData.ManualAddWorkDuration.HasValue)
			{
				meetingCountDownService.StartWork(realWorkData.WorkData, isPermanent, true); //we force start because rule can activate this
				return;
			}
			using (MutualWorkTypeCoordinator.StartStateChangeTransaction())
			{
				if (isPermanent)
				{
					PermStartWork(workData, isEnabledWhileNotWorking);
				}
				else
				{
					TempStartWork(workData, isEnabledWhileNotWorking);
				}
			}
		}

		public void TempOrPermStartCategory(int categoryId, bool isPermanent, bool isEnabledWhileNotWorking)
		{
			if (CurrentWorkState == WorkState.NotWorking && !isEnabledWhileNotWorking && !meetingCountDownService.IsWorking) return;
			var workIdOrNull = LastUserSelectedOrPermWork != null ? LastUserSelectedOrPermWork.Id : null; //start category based on LastUserSelectedOrPermWork (inconsistent with meetingCountDownService)
			if (!workIdOrNull.HasValue)
			{
				//warn cannot change category because there are no valid previous works
				log.Debug("Cannot change to category " + categoryId + ": there are no valid previous works");
				return;
			}
			var possibleWorks = clientMenuLookup.GetWorksForCategoryId(workIdOrNull.Value, categoryId);
			if (possibleWorks == null || possibleWorks.Count == 0)
			{
				//warn category is empty
				log.Debug("Cannot change to category " + categoryId + ": category is empty");
				return;
			}
			var currentWorkId = CurrentWork != null
				? CurrentWork.Id
				: meetingCountDownService.CurrentWorkItem != null
					? meetingCountDownService.CurrentWorkItem.WorkId
					: null;
			if (currentWorkId != null && possibleWorks.Select(n => n.WorkData.Id.Value).Contains(currentWorkId.Value))
			{
				//current work is in this category so do nothing (warn if more possible works ?)
				return;
			}
			if (possibleWorks.Count == 1)
			{
				//if we have one possible work then we are done
				TempOrPermStartWork(possibleWorks[0].WorkData, isPermanent, isEnabledWhileNotWorking);
				return;
			}
			//we have multiple possible works... we have to decide which work to select
			var choosenWork = possibleWorks
				.OrderByDescending(n => n.WorkData.Priority ?? -1)
				.ThenBy(n => n.WorkData.EndDate ?? DateTime.MaxValue)
				.ThenBy(n => n.WorkData.Name)
				.First();
			TempOrPermStartWork(choosenWork.WorkData, isPermanent, isEnabledWhileNotWorking);
			notificationService.ShowNotification(NotificationKeys.MoreWorksInCat + categoryId, nfMoreWorksInCatDuration,
												Labels.NotificationMoreWorksInCategoryTitle,
												Labels.NotificationMoreWorksInCategoryBody);
		}

		protected override void OnPropertyChanged(string propertyName)
		{
			if (propertyName == "CurrentWork")
			{
				IsCurrentWorkValid = CurrentWork == null || clientMenuLookup.GetWorkDataWithParentNames(CurrentWork.Id.Value) != null;
				var currentName = GetWorkName(CurrentWork, clientMenuLookup);
				CurrentOrLastWorkName = CurrentWorkState == WorkState.NotWorking || CurrentWorkState == WorkState.NotWorkingTemp
									? GetWorkName(LastUserSelectedOrPermWork, clientMenuLookup)
									: currentName;
				log.Info("Current work changed to " + currentName + " (" + CurrentWorkState + " " + LastWorkStateChangeReason + ")");
				CurrentOrLastWorkNameInTwoLines = CurrentWorkState == WorkState.NotWorking || CurrentWorkState == WorkState.NotWorkingTemp
									? GetWorkName(LastUserSelectedOrPermWork, clientMenuLookup, WorkDataWithParentNames.NewLineSeparator)
									: GetWorkName(CurrentWork, clientMenuLookup, WorkDataWithParentNames.NewLineSeparator);

				switch (LastWorkStateChangeReason)
				{
					case WorkStateChangeReason.UserSelect:
						ResetQueuedWork();
						if (CurrentWorkState == WorkState.Working)
						{
							ShowWorkingNotification(true);
						}
						else
						{
							Debug.Assert(CurrentWorkState == WorkState.NotWorking);
							ShowNotWorkingNotification(true);
						}
						break;
					case WorkStateChangeReason.UserResume:
					case WorkStateChangeReason.AutoResume:
						Debug.Assert(CurrentWorkState == WorkState.Working);
						ResetQueuedWork();
						ShowWorkingNotification(true);
						break;
					case WorkStateChangeReason.AutodetectedTemp:
						if (CurrentWorkState == WorkState.WorkingTemp)
						{
							ShowWorkingNotification(false);
						}
						else
						{
							Debug.Assert(CurrentWorkState == WorkState.NotWorkingTemp);
							ShowNotWorkingNotification(false);
						}
						break;
					case WorkStateChangeReason.AutodetectedEndTempEffect:
						Debug.Assert(CurrentWorkState == WorkState.Working);
						ShowWorkingNotification(false);
						break;
					case WorkStateChangeReason.AutodetectedPerm:
						Debug.Assert(CurrentWorkState == WorkState.Working);
						ResetQueuedWork();
						ShowWorkingNotification(false);
						break;
					default:
						log.ErrorAndFail("Invalid work state change reason");
						break;
				}
			}
			base.OnPropertyChanged(propertyName);
		}

		public WorkDataWithParentNames GetWorkDataWithParentNames(int workId)
		{
			WorkDataWithParentNames workData;
			return clientMenuLookup.WorkDataById.TryGetValue(workId, out workData) ? workData : null;
		}

		public WorkDataWithParentNames GetProjectForLastUserSelectedOrPermWork()
		{
			if (LastUserSelectedOrPermWork == null || !LastUserSelectedOrPermWork.Id.HasValue) return null;
			var workId = LastUserSelectedOrPermWork.Id.Value;
			WorkDataWithParentNames workData;
			return clientMenuLookup.ProjectByWorkId.TryGetValue(workId, out workData) ? workData : null;
		}

		private static string GetWorkName(WorkData workData, ClientMenuLookup lookup)
		{
			return GetWorkName(workData, lookup, WorkDataWithParentNames.DefaultSeparator);
		}

		private static string GetWorkName(WorkData workData, ClientMenuLookup lookup, string workSeparator)
		{
			if (workData == null) return Labels.Menu_NoWorkToContinue;
			var workDataWithParents = workData.Id.HasValue ? lookup.GetWorkDataWithParentNames(workData.Id.Value) : null;
			return workDataWithParents == null ? workData.Name : workDataWithParents.GetFullName(WorkDataWithParentNames.DefaultSeparator, workSeparator);
		}

		private static TimeSpan GetValidDuration(int duration)
		{
			return duration > 0 ? TimeSpan.FromMilliseconds(duration) : TimeSpan.Zero;
		}

		private bool IsIgnoredWork(WorkData workData, ClientMenuLookup lookup)
		{
			Debug.Assert(workData.Id != null);
			Debug.Assert(queuedWorkChange == null || queuedWorkChange.Id != null);
			var queuedChange = queuedWorkChange != null && queuedWorkChange.Id.Value == workData.Id.Value;
			return lookup.IsDynamicWork(workData.Id.Value) || !workData.IsVisibleInMenu || queuedChange;
		}

		public void MenuChanged(ClientMenuLookup menuLookup, ClientMenu oldMenu, IWorkManagementService workManagementService)
		{
			//todo fix that we won't update current/lastwork name if changed
			clientMenuLookup = menuLookup; //The data in ClientMenu won't change only the reference (so its kinda immutable)
			bool shortDisp;
			var oldMenuLookup = new ClientMenuLookup { ClientMenu = oldMenu };
			var menuDiff = MenuHelper.GetMenuDifference(menuLookup, oldMenuLookup);
			var changeString = MenuHelper.GetMenuChangeString(menuLookup, oldMenuLookup, menuDiff, IsIgnoredWork, workManagementService, out shortDisp);
			log.Info("Menu Change details: " + (shortDisp ? "Skip" : "") + changeString);
			foreach (var closedWork in menuDiff.DeletedWorks)
			{
				if (closedWork.WorkData == null || closedWork.WorkData.Id == null ||
					!closedWork.WorkData.IsVisibleInMenu || oldMenuLookup.IsDynamicWork(closedWork.WorkData.Id.Value)) continue;
				RecentClosedHelper.AddRecent(closedWork);
			}
			foreach (var openedWork in menuDiff.NewWorks)
			{
				if (openedWork.WorkData == null || openedWork.WorkData.Id == null ||
					!openedWork.WorkData.IsVisibleInMenu || menuLookup.IsDynamicWork(openedWork.WorkData.Id.Value)) continue;
				RecentClosedHelper.RemoveRecent(openedWork);
			}

			if (changeString.GetText() != "" && !shortDisp && ConfigManager.LocalSettingsForUser.MenuChangeWarnDuration >= 0) //display popup only if it contains visible info for the user.
			{
				notificationService.ShowNotification(NotificationKeys.MenuChange + Guid.NewGuid(), nfMenuChangeDuration,
													 Labels.NotificationMenuChangedTitle,
													 changeString);
			}

			if (queuedWorkChange != null)
			{
				StartQueuedWorkIfApplicable();
			}

			//update LastUserSelectedOrPermWork if it was a temp work
			ClosedTempWorkReplaceAction(LastUserSelectedOrPermWork, clientMenuLookup, (orig, sameWork) => LastUserSelectedOrPermWork = sameWork);
			//if we've just closed a temp work then switch to its counter-part
			ClosedTempWorkReplaceAction(CurrentWork, clientMenuLookup, (orig, sameWork) =>
				{
					log.Info("Switching to a work with the same assignData");
					var wd = new WorkData() { Id = sameWork.Id, Name = sameWork.Name, AssignData = orig.AssignData }; //we add assign data to indicate this is the same work as the local one
					TempOrPermStartWork(wd, CurrentWorkState == WorkState.Working, false);
				});

			bool isLastUserWorkValidForMenu = IsWorkValidForMenuAndHasValue(LastUserSelectedOrPermWork, clientMenuLookup);
			bool isCurrentWorkValidForMenu = IsWorkValidForMenuAndHasValue(CurrentWork, clientMenuLookup);
			if (CurrentWork != null) //todo fix if the current work is deleted then added back then the SavedLastWorkData will be null
			{
				IsCurrentWorkValid = isCurrentWorkValidForMenu;
				if (isCurrentWorkValidForMenu) notificationService.HideNotification(NotificationKeys.InvalidWork);
			}
			if (!isLastUserWorkValidForMenu)
			{
				LastUserSelectedOrPermWork = clientMenuLookup.DefaultWork != null ? clientMenuLookup.DefaultWork.WorkData : null; //delete wrong LastUserSelectedWork
				isLastUserWorkValidForMenu = IsWorkValidForMenuAndHasValue(LastUserSelectedOrPermWork, clientMenuLookup); //check if it is valid now
				log.Info("LastUserSelectedWork is invalid" + (isLastUserWorkValidForMenu ? " using default work instead" : ""));
			}
			if (CurrentWorkState == WorkState.Working && !isCurrentWorkValidForMenu)
			{
				log.Info("CurrentWork is invalid and Working");
				if (isLastUserWorkValidForMenu)
				{
					TempOrPermStartWork(LastUserSelectedOrPermWork, true, false); //start valid LastUserSelectedOrPermWork
				}
				else
				{
					ShowChangeWorkNotification(CurrentWork); //nothing we can do
				}
			}
			else if (CurrentWorkState == WorkState.WorkingTemp && !isCurrentWorkValidForMenu)
			{
				log.Info("CurrentWork is invalid and WorkingTemp");
				if (isLastUserWorkValidForMenu)
				{
					TempEndEffect(); //switch back to valid work (error will be raised after trying to change again)
				}
				else //both current and last are invalid
				{
					ShowChangeWorkNotification(CurrentWork); //nothing we can do
				}
			}
		}

		private void ClosedTempWorkReplaceAction(WorkData workData, ClientMenuLookup clientMenuLookup, Action<WorkData, WorkData> replaceAction)
		{
			if (workData != null
				&& !MenuCoordinator.IsWorkIdFromServer(workData.Id.Value)
				&& !IsWorkValidForMenuAndHasValue(workData, clientMenuLookup)
				&& workData.AssignData != null)
			{
				bool ignored;
				var sameWork = clientMenuLookup.GetWorkForAssignData(workData.AssignData, out ignored);
				if (sameWork != null)
				{
					replaceAction(workData, sameWork.WorkData);
				}
			}
		}

		private bool mutualWorkPreventedStart;
		protected override bool CanStartWork(ref WorkData workData, WorkState newWorkState, WorkStateChangeReason changeReason)
		{
			mutualWorkPreventedStart = false;

			if (CurrentWorkState == WorkState.NotWorking)
			{
				if (!MutualWorkTypeCoordinator.RequestStopWork(false, "Normal Start").CanStartWork)
				{
					mutualWorkPreventedStart = true;
					return false;
				}
			}

			if (!IsTrueOrErrorAndFail(IsValidWork(workData), "Invalid work in CanStartWork")) return false;
			if (!ConfigManager.IsWorkEnabledOutsideWorkTimeStartEnd && !parent.GetIsWorkTime(DateTime.UtcNow))
			{
				log.Debug("IsWorkEnabledOutsideWorkTimeStartEnd disabled outside working time");
				return false;
			}
			int workId = workData.Id.Value;
			var realWorkData = clientMenuLookup.GetWorkDataWithParentNames(workId);
			if (realWorkData != null)
			{
				Debug.Assert(realWorkData.WorkData.Id.Value == workId);
				Debug.Assert(MenuCoordinator.IsWorkIdFromServer(workId) || realWorkData.WorkData.AssignData != null);

				workData = realWorkData.WorkData; //we use the real work data with all properties set (like AssignData [only for local works], ManualWorkTimeDuration etc.)
				SetWorkDataWithAssignDataIfSwitchingFromLocalToServerWork(workId, ref workData); //add AssignData to server work if we switch from local one so CaptureManager can reuse phaseId
				return true;
			}

			if (!MenuCoordinator.IsWorkIdFromServer(workId) && workData.AssignData == null)
			{
				log.ErrorAndFail("Temp work id " + workId + " without AssignData"); //we won't be able to upload this...
				return false;
			}

			//not the best approach (can we fall back to an other work?)
			if (CurrentWorkState == WorkState.NotWorkingTemp && changeReason == WorkStateChangeReason.AutodetectedEndTempEffect) //we dont allow to stay offline because of menu change
			{
				//we allow to change work but mark it as invalid (so we can ignore at recent works)
				log.Info("Allow to change to invalid work " + workId);
				ShowChangeWorkNotification(workData);
				return true;
			}

			if (CurrentWorkState == WorkState.NotWorkingTemp
				&& (changeReason == WorkStateChangeReason.AutodetectedTemp || changeReason == WorkStateChangeReason.AutodetectedPerm)) //we dont allow to stay offline because of menu change
			{
				//don't allow to change to invalid autodetected rule but allow to change invalid user selected one
				TempEndEffect();
				return false;
			}

			return false;
		}

		private void SetWorkDataWithAssignDataIfSwitchingFromLocalToServerWork(int newWorkId, ref WorkData workData)
		{
			if (CurrentWork != null
				&& CurrentWork.AssignData != null
				&& !MenuCoordinator.IsWorkIdFromServer(CurrentWork.Id.Value)
				&& MenuCoordinator.IsWorkIdFromServer(newWorkId)
				)
			{
				bool ignored;
				var awd = clientMenuLookup.GetWorkForAssignData(CurrentWork.AssignData, out ignored);
				if (awd != null && awd.WorkData.Id == newWorkId)
				{
					var wdToSet = awd.WorkData.Clone(); //WorkData is 'immutable' so don't modify directly
					wdToSet.AssignData = CurrentWork.AssignData;
					workData = wdToSet;
				}
			}
		}

		protected override void CannotStartWork(WorkData workData, WorkState newWorkState, WorkStateChangeReason changeReason)
		{
			var workIdStr = workData == null || !workData.Id.HasValue ? "" : " (" + workData.Id.Value + ")";
			Color? color = CurrentWorkState == WorkState.NotWorking || CurrentWorkState == WorkState.NotWorkingTemp
							? NotWorkingColor
							: new Color?();

			if (mutualWorkPreventedStart)
			{
				log.Info("Cannot start work" + workIdStr + " (MutualWorkRunning " + changeReason + ")");
				return;
			}

			log.Info("Cannot start work" + workIdStr + " (" + changeReason + " " + newWorkState + ")");

			switch (changeReason)
			{
				case WorkStateChangeReason.UserSelect:
					notificationService.HideNotification(NotificationKeys.NoWork);
					notificationService.ShowNotification(NotificationKeys.NoWork, nfInvalidWorkDuration,
														 Labels.NotificationCannotStartUserSelectedWorkTitle,
														 string.Format(Labels.NotificationCannotStartUserSelectedWorkBody, workIdStr),
														 color);
					break;
				case WorkStateChangeReason.UserResume:
					if (!ConfigManager.IsWorkEnabledOutsideWorkTimeStartEnd &&
					    !parent.GetIsWorkTime(DateTime.UtcNow)) break;
					notificationService.HideNotification(NotificationKeys.NoWork);
					notificationService.ShowNotification(NotificationKeys.NoWork, nfNoWorkDuration,
														 Labels.NotificationCannotResumeWorkTitle,
														 Labels.NotificationCannotResumeWorkBody,
														 color);
					break;
				case WorkStateChangeReason.AutodetectedPerm:
				case WorkStateChangeReason.AutodetectedTemp:
					notificationService.ShowNotification(GetInvalidWorkKey(workData), nfInvalidWorkDuration,
														 Labels.NotificationCannotStartAutomaticWorkTitle,
														 string.Format(Labels.NotificationCannotStartAutomaticWorkBody, workIdStr));
					break;
				case WorkStateChangeReason.AutodetectedEndTempEffect:
					notificationService.ShowNotification(GetInvalidWorkKey(workData), nfInvalidWorkDuration,
														 Labels.NotificationCannotSwitchBackToLastUserWorkTitle,
														 string.Format(Labels.NotificationCannotSwitchBackToLastUserWorkBody, workIdStr),
														 color);
					break;
				default:
					log.ErrorAndFail("Invalid change reason: " + changeReason);
					break;
			}
		}

		private static string GetInvalidWorkKey(WorkData workData)
		{
			return NotificationKeys.InvalidWork + (workData == null ? "" : workData.Id.ToString());
		}

		private void ShowChangeWorkNotification(WorkData workData)
		{
			var workIdStr = workData == null || !workData.Id.HasValue ? "" : " (" + workData.Id.Value + ")";
			notificationService.ShowNotification(NotificationKeys.InvalidWork, nfInvalidWorkDuration,
												 Labels.NotificationShouldChangeFromInvalidWorkTitle,
												 string.Format(Labels.NotificationShouldChangeFromInvalidWorkBody, workIdStr),
												 NotWorkingColor); //trick the user with not working color
		}

		private bool ShowChangeWorkNotificationIfApplicable()
		{
			if (!IsCurrentWorkValid)
			{
				ShowChangeWorkNotification(CurrentWork);
				return true;
			}
			return false;
		}

		private static bool IsWorkValidForMenuAndHasValue(WorkData workData, ClientMenuLookup menuLookup)
		{
			return IsValidWork(workData) && menuLookup.GetWorkDataWithParentNames(workData.Id.Value) != null;
		}

		private static bool IsValidWork(WorkData workData)
		{
			return workData != null && workData.Id.HasValue;
		}

		private static bool IsTrueOrErrorAndFail(bool expression, string errorMsg)
		{
			if (!expression)
			{
				log.ErrorAndFail(errorMsg);
			}
			return expression;
		}

		public void LoadLastWorkData(ClientMenuLookup menuLookup)
		{
			clientMenuLookup = menuLookup;
			log.Info("Current WorkIds: " + string.Join(", ", clientMenuLookup.WorkDataById.Keys.OrderBy(n => n).Select(n => n.ToString()).ToArray()));
			var lastWorkData = clientMenuLookup.DefaultWork != null ? clientMenuLookup.DefaultWork.WorkData : null;
			if (IsolatedStorageSerializationHelper.Exists(LastWorkDataPath))
			{
				WorkData workData;
				if (IsolatedStorageSerializationHelper.Load(LastWorkDataPath, out workData) && workData != null)
				{
					if (workData.Id.HasValue)
					{
						var workDataWithParentNames = clientMenuLookup.GetWorkDataWithParentNames(workData.Id.Value);
						if (workDataWithParentNames != null && workDataWithParentNames.WorkData != null)
						{
							lastWorkData = workDataWithParentNames.WorkData;
						}
					}
				}
			}
			LastUserSelectedOrPermWork = lastWorkData;
			if (LastUserSelectedOrPermWork != null && CurrentWorkState == WorkState.NotWorking)
			{
				OnPropertyChanged("CurrentWork"); //hax to display name of the lastUserSelectedWork
				ShowNotWorkingNotification(true);
			}
		}

		public override WorkData LastUserSelectedOrPermWork
		{
			get
			{
				return base.LastUserSelectedOrPermWork;
			}
			protected set
			{
				base.LastUserSelectedOrPermWork = value;
				SaveLastWorkData(value);
			}
		}

		private static void SaveLastWorkData(WorkData workData)
		{
			if (workData != null)
			{
				IsolatedStorageSerializationHelper.Save(LastWorkDataPath, workData);
			}
			else if (IsolatedStorageSerializationHelper.Exists(LastWorkDataPath))
			{
				IsolatedStorageSerializationHelper.Delete(LastWorkDataPath);
			}
		}

		private bool ShowNotWorkingNotification(bool isUserInitiated)
		{
			if (ConfigManager.LocalSettingsForUser.NotWorkingWarnDuration < 0) return true;
			lastNotWorkingShow = Environment.TickCount;
			notificationService.HideNotification(NotificationKeys.NewWork);
			string title = isUserInitiated ? Labels.NotificationNotWorkingUserTitle : Labels.NotificationNotWorkingAutomaticTitle;
			string body = isUserInitiated ? AppConfig.Current.GetLocalizationStringOverride(nameof(Labels.NotificationNotWorkingUserBody), Labels.NotificationNotWorkingUserBody) : Labels.NotificationNotWorkingAutomaticBody;
			return notificationService.ShowNotification(NotificationKeys.NotWorking, nfNotWorkingDuration,
														title,
														body,
														NotWorkingColor);
		}

		private bool ShowWorkingNotification(bool isUserInitiated)
		{
			if (ConfigManager.LocalSettingsForUser.WorkingWarnDuration < 0) return true;
			lastCurrentWorkShow = Environment.TickCount;
			notificationService.HideNotification(NotificationKeys.NotWorking);
			if (IsCurrentWorkValid)
			{
				notificationService.HideNotification(NotificationKeys.InvalidWork); //we can only start a valid work
				notificationService.HideNotification(GetInvalidWorkKey(CurrentWork));
			}
			notificationService.HideNotification(NotificationKeys.NoWork);
			notificationService.HideNotification(NotificationKeys.NewWork);
			string title = isUserInitiated ? AppConfig.Current.GetLocalizationStringOverride(nameof(Labels.NotificationChangeWorkUserTitle), Labels.NotificationChangeWorkUserTitle) : Labels.NotificationChangeWorkAutomaticTitle;
			return !ConfigManager.LocalSettingsForUser.IsWorkingWarnDisplayable && LastWorkState != WorkState.NotWorking || notificationService.ShowNotification(NotificationKeys.NewWork, nfNewWorkDuration, title,
						string.Format(LastWorkState != WorkState.NotWorking ? Labels.NotificationWorkingBody : AppConfig.Current.GetLocalizationStringOverride(nameof(Labels.NotificationWorkingBody),
							Labels.NotificationWorkingBody), CurrentOrLastWorkName),
						WorkingColor);
		}

		public void HideNotWorkingNotification()
		{
			notificationService.HideNotification(NotificationKeys.NotWorking);
		}

		public bool ShowPeriodicNotificationIfApplicable()
		{
			if (ShowChangeWorkNotificationIfApplicable())
			{
				return true;
			}
			var notWorking = ShowPeriodicNotWorking();
			var working = ShowPeriodicWorking();
			return notWorking || working;
		}

		private int? lastNotWorkingShow;
		private bool ShowPeriodicNotWorking()
		{
			var isWorking = CurrentWorkState == WorkState.Working || CurrentWorkState == WorkState.WorkingTemp;
			if (ShouldShowPeriodicNotification(ConfigManager.LocalSettingsForUser.NotWorkingWarnInterval, isWorking, ref lastNotWorkingShow))
			{
				var createdNew = ShowNotWorkingNotification(CurrentWorkState == WorkState.NotWorking);
				return createdNew;
			}
			return false;
		}

		private int? lastCurrentWorkShow;
		private bool ShowPeriodicWorking()
		{
			var isWorking = CurrentWorkState == WorkState.Working || CurrentWorkState == WorkState.WorkingTemp;
			if (ShouldShowPeriodicNotification(ConfigManager.LocalSettingsForUser.WorkingWarnInterval, !isWorking, ref lastCurrentWorkShow))
			{
				var createdNew = notificationService.ShowNotification(NotificationKeys.NewWork, nfNewWorkDuration,
																	  AppConfig.Current.GetLocalizationStringOverride(nameof(Labels.NotificationWorkingTitle), Labels.NotificationWorkingTitle),
																	  string.Format(AppConfig.Current.GetLocalizationStringOverride(nameof(Labels.NotificationWorkingBody), Labels.NotificationWorkingBody), CurrentOrLastWorkName),
																	  WorkingColor);
				return createdNew;
			}
			return false;
		}

		private static bool ShouldShowPeriodicNotification(int interval, bool shouldReset, ref int? lastShow)
		{
			if (interval <= 0) return false;
			if (shouldReset)
			{
				lastShow = null;
				return false;
			}
			if (!lastShow.HasValue)
			{
				lastShow = Environment.TickCount;
				return false;
			}
			if ((uint)(Environment.TickCount - lastShow.Value) > interval)
			{
				lastShow = Environment.TickCount;
				return true;
			}
			return false;
		}

		public bool IsWorking
		{
			get { return CurrentWorkState != WorkState.NotWorking; }
		}

		public string StateString
		{
			get
			{
				return IsWorking ? "working" : null;
			}
		}

		public MutualWorkTypeInfo RequestStopWork(bool isForced)
		{
			var resumeWorkOnClose = false;
			if (IsWorking)
			{
				resumeWorkOnClose = true;
				UserStopWork();
				HideNotWorkingNotification(); //hide current not working notification
			}
			return new MutualWorkTypeInfo() { CanStartWork = !IsWorking, ResumeWorkOnClose = resumeWorkOnClose };
		}

		public void RequestKickWork()
		{
			if (!IsWorking) return;
			UserStopWork();
			HideNotWorkingNotification();
		}

		public bool PermStartWorkByCompositeKey(string workKey, List<string> projectKeys, bool isEnabledWhileNotWorking = false)
		{
			bool ignored;
			var work = clientMenuLookup.GetWorkForCompositeKey(workKey, projectKeys, out ignored);
			if (work != null)
			{
				using (MutualWorkTypeCoordinator.StartStateChangeTransaction())
				{
					PermStartWork(work.WorkData, isEnabledWhileNotWorking);
				}

				return true;
			}
			return false;
		}

	}
}