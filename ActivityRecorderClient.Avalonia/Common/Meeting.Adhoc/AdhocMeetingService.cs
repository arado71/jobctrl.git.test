using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.View.Presenters;
// TODO: mac
using Timer = Tct.ActivityRecorderClient.Forms.Timer;

namespace Tct.ActivityRecorderClient.Meeting.Adhoc
{
	public abstract class AdhocMeetingService : IAdhocMeetingService, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static string OnGoingMeetingPath { get { return "OnGoingMeeting-" + ConfigManager.UserId; } }
		private static readonly TimeSpan onGoingUpdateInterval = TimeSpan.FromMinutes(1);
		private static readonly TimeSpan onGoingBookingInterval = TimeSpan.FromMinutes(5);
		private const int ONGOING_PERSIST_AND_SEND_IN_MINUTES = 3;
		private MeetingWorkTimeCounter counter;
		private bool resumeWorkOnClose;
		private bool isPoppedUpAfterInactivity;
		private bool isUnconfirmedPopup;
		private bool isLoadedBackInactivity;
		public bool IsPostponedPopup { get; private set; }
		private WorkData lastWorkBeforeMeeting;
		protected readonly CaptureCoordinator captureCoordinator;
		private Timer onGoingMeetingTimer;
		public Guid? CountingMeetingId { set; get; }
		public bool ManualMeeting { set; get; }
		public bool IsStopped { get { return !IsPostponedPopup && counter != null && counter.IsStopped; } }
		public bool IsWorking { get { return counter != null && !counter.IsStopped; } }
		public string StateString { get { return IsWorking ? (isPoppedUpAfterInactivity ? "adhoc" : "meeting") : null; } }
		private Guid lastOnGoingMeetingId;
		private OfflineWorkPresenter presenter;

		protected abstract OfflineWorkPresenter OpenGui();

		public event EventHandler<SingleValueEventArgs<bool>> OnAbortAndClose;
		public event EventHandler OnShowGui;
		public event EventHandler<SingleValueEventArgs<ClientMenuLookup>> OnUpdateMenu;
		public event EventHandler<SingleValueEventArgs<Tuple<Guid, WorkDataWithParentNames>>> OnSetWork;
		public event EventHandler<SingleValueEventArgs<KeyValuePair<string, string>>> OnShowMessageBox;
		public event EventHandler<SingleValueEventArgs<bool>> OnUserActivityWhileMeeting;
		public event EventHandler OnOverCountedAndDeleted;

		public Func<WorkData> GetCurrentMeetingWork { get; set; }
		public Func<ManualMeetingItem[]> GetConfirmedMeetings { get; set; }
		public Func<ManualMeetingItem[]> GetDeletedMeetings { get; set; }
		public Func<ManualMeetingItem[]> GetModifiedMeetings { get; set; }
		public event EventHandler OnKickWork;
		public bool IsGuiShown { get { return counter != null; } }

		protected AdhocMeetingService(CaptureCoordinator captureCoordinator)
		{
			this.captureCoordinator = captureCoordinator;
			this.captureCoordinator.CurrentMenuChanged += CurrentMenuChanged;
			this.captureCoordinator.OnUserActivity += CaptureCoordinatorOnUserActivity;
			this.captureCoordinator.ClientSettingsManager.SettingsChanged += ClientSettingsManager_OnSettingsChanged;
		}

		public void StartWork(int? includedIdleMins, int? workId)
		{
			StartWork(includedIdleMins, workId, null);
		}

		private void StartWork(int? includedIdleMins, int? workId, LoadWorkParams lp)
		{
			//if (includedIdleMins == null)
			//{
			//	if (manualMeetingService != null && !manualMeetingService.IsStopped)
			//	{
			//		log.Info("ShowGUI has been called");
			//		currentService = manualMeetingService;
			//		currentService.ShowGui();
			//		return;
			//	}
			//	var guid = CreateService();
			//	manualMeetingService = services[guid];
			//	manualMeetingService.ManualMeeting = true;
			//	currentService = manualMeetingService;
			//}
			//else
			//	currentService = services[CreateService()];
			StartEndDateTime? meetingTime = null;
			TimeSpan nonAccountableDuration;
			Guid meetingId;
			bool resumeWorkOnClosePreviously;
			if (lp != null)
			{
				workId = lp.WorkId > 0 ? (int?)lp.WorkId : null;
				meetingTime = lp.StartEndDateTime;
				meetingId = lp.Guid;
				CountingMeetingId = lp.OngoingMeeting ? (Guid?)meetingId : null;
				IsPostponedPopup = !lp.OngoingMeeting;
				isPoppedUpAfterInactivity = lp.IsPoppedUpAfterInactivity; // false: manual, true: inactivity meeting had been loaded
				isLoadedBackInactivity = lp.IsPoppedUpAfterInactivity;
				resumeWorkOnClosePreviously = lp.ResumeWorkOnClose;
				nonAccountableDuration = lp.NonAccountableDuration;
			}
			else
			{
				meetingId = Guid.NewGuid();
				CountingMeetingId = meetingId;
				IsPostponedPopup = false;
				isPoppedUpAfterInactivity = includedIdleMins.HasValue && !workId.HasValue && !meetingTime.HasValue; // IsIdleDuringWorkTime && OfflineWorkIsAllowed
				isLoadedBackInactivity = false;
				resumeWorkOnClosePreviously = false;
				nonAccountableDuration = TimeSpan.Zero;
			}
			log.Info("StartWork idle: " + includedIdleMins + " workId: " + workId + " meetingTime: " + meetingTime + " meetingId: " + meetingId);
			if (counter != null && !counter.IsStopped)
			{
				log.Info("Gui already shown");
				OnShowGui?.Invoke(this, EventArgs.Empty);
				SetAddMeetingWorkId(meetingId, workId);
				return;
			}
			lastWorkBeforeMeeting = captureCoordinator.CurrentWorkController.CurrentWork ?? captureCoordinator.CurrentWorkController.LastUserSelectedOrPermWork;
			using (captureCoordinator.CurrentWorkController.MutualWorkTypeCoordinator.StartStateChangeTransaction())
			{
				if (!IsPostponedPopup)
				{
					var mutual = captureCoordinator.CurrentWorkController.MutualWorkTypeCoordinator.RequestStopWork(false, "AddMeeting Start");
					if (!mutual.CanStartWork) return;
					if (!ConfigManager.IsWorkEnabledOutsideWorkTimeStartEnd && !captureCoordinator.GetIsWorkTime(DateTime.UtcNow))
					{
						log.Debug("IsWorkEnabledOutsideWorkTimeStartEnd disabled outside working time");
						return;
					}
					resumeWorkOnClose = mutual.ResumeWorkOnClose || resumeWorkOnClosePreviously;
				}

				isUnconfirmedPopup = meetingTime.HasValue;															// LoadWork: meetingTime is specified for UnfinishedOnGoingMeetings only (AKA postponed meeting)
				counter = isUnconfirmedPopup
					? (lp != null && lp.OngoingMeeting ? MeetingWorkTimeCounter.Restart(meetingTime)				// unfinished ongoing case
						: MeetingWorkTimeCounter.GetFinished(meetingTime.Value.StartDate, meetingTime.Value.EndDate, nonAccountableDuration))	// postponed case
					: MeetingWorkTimeCounter.StartNew(TimeSpan.FromMinutes(includedIdleMins.GetValueOrDefault()));
				Debug.Assert(counter != null);
				if (!isUnconfirmedPopup && isPoppedUpAfterInactivity)
				{
					var inactivityToDelete = GetOnGoingMeetingDataDeletion(counter.StartTime, includedIdleMins.Value);
					if (inactivityToDelete != null)
					{
						captureCoordinator.WorkItemManager.PersistAndSend(inactivityToDelete);
						log.Info("Ignored inactivity time started at " + inactivityToDelete.StartDate + " meetingId: " + meetingId);
					}
				}
				if (!IsPostponedPopup && Environment.TickCount - counter.StartTicks >= ConfigManager.DuringWorkTimeIdleManualInterval)
				{
					StopCounter();
					OnClosed(OfflineWindowCloseReason.RequestStop);
					log.Info($"Deleted all inactivity time from {counter.StartTime} duration: {counter.GetDuration()} before offline window popped up");
					return;
				}
				if (presenter == null)
				{
					presenter = OpenGui();
					presenter.Show();
				}
				// unnecessary!
				//isPoppedUpAfterInactivity = !isUnconfirmedPopup && includedIdleMins.HasValue;						// in case of postponed == false
				presenter.StartWork(new StartWorkGUIparams
					{
						MeetingId = meetingId,
						MenuLookup = captureCoordinator.CurrentMenuLookup,
						IsPoppedUpAfterInactivity = isPoppedUpAfterInactivity,
						IncludedIdleMins = includedIdleMins.GetValueOrDefault(),
						Counter = counter,
						IsPostponedPopup = IsPostponedPopup,
						IsInWorkBefore = resumeWorkOnClose,
						IsUnconfirmedPopup = isUnconfirmedPopup,
					});
				SetAddMeetingWorkId(meetingId, workId);
				if (isUnconfirmedPopup && !IsPostponedPopup && captureCoordinator.StartType != ApplicationStartType.EmergencyRestart)
				{
					OnShowMessageBox?.Invoke(this,
							SingleValueEventArgs.Create(
								new KeyValuePair<string, string>(Labels.AddMeeting_WarningOfUnfinishedOnGoingMeeting_Body,
									Labels.AddMeeting_WarningOfUnfinishedOnGoingMeeting_Title)));
				}
				if (/*!isUnconfirmedPopup && */!IsPostponedPopup)
				{
					log.Info("Starting OnGoingMeetingTimer, interval=" + onGoingUpdateInterval.TotalMilliseconds + " meetingId: " + meetingId);
					StartOnGoingMeetingTimer();
				}
			}
		}

		public void CloseGui()
		{
			presenter?.Dispose();
			presenter = null;
		}

		public MutualWorkTypeInfo RequestStopWork(bool isForced)
		{
			if (IsWorking && isForced)
			{
				OnAbortAndClose?.Invoke(this, SingleValueEventArgs.Create(true));
			}
			return new MutualWorkTypeInfo()
			{
				CanStartWork = !IsWorking,
				ResumeWorkOnClose = counter != null && resumeWorkOnClose,
			};
		}
		public void RequestKickWork()
		{
			if (!IsWorking) return;
			StopOnGoingMeetingTimer();
			counter.StopWork();
			OnKickWork?.Invoke(this, EventArgs.Empty);
			resumeWorkOnClose = false;
		}

		private readonly StopwatchLite lastPause = new StopwatchLite(TimeSpan.FromMinutes(1), true);
		public void PauseWork()
		{
			if (counter == null || counter.IsStopped) return;
			counter.PauseWork();
			if (lastPause.IsIntervalElapsedSinceLastCheck()) OnShowGui?.Invoke(this, EventArgs.Empty); //make sure the user notices that the worktime counter is paused
		}

		public void ResumeWork()
		{
			if (counter == null || counter.IsStopped) return;
			counter.ResumeWork();
		}

		private void StopCounter()
		{
			counter.StopWork();
			StopOnGoingMeetingTimer();
			IsolatedStorageSerializationHelper.Delete(OnGoingMeetingPath);
			resumeWorkOnClose = false; // this state isn't effective after the meeting stopped (=postponed) 
		}

		private void CaptureCoordinatorOnUserActivity(object sender, SingleValueEventArgs<bool> e)
		{
			if (presenter == null) return;
			if (IsPostponedPopup || !ConfigManager.AutoReturnFromMeeting) return;
			if (captureCoordinator.CurrentWorkController.IsWorking) return;
			if (counter.IsStopped) return;
			OnUserActivityWhileMeeting?.Invoke(this, SingleValueEventArgs.Create(e.Value));
		}

		public void AutoReturnFromMeeting()
		{
			if (counter == null) return; // activity after meeting handled
			counter.StopWork();
			StopOnGoingMeetingTimer();
			UpdatePostponedMeetingWorkItem();
			if (resumeWorkOnClose)
				captureCoordinator.CurrentWorkController.UserResumeWork(WorkStateChangeReason.AutoResume);
			IsPostponedPopup = true;
		}

		public void OnClosed(OfflineWindowCloseReason closeReason)
		{
			log.Info("Adhoc meeting form closed: " + closeReason);
			if (counter == null)
			{
				log.Info("Skip FormClosed");
				return; //FormClosed can fire more than once (e.g. on Windows shutdown)
			}
			StopOnGoingMeetingTimer();
			switch (closeReason)
			{
				case OfflineWindowCloseReason.QueryShutdown:		// due to closing stopped client 
					UpdatePostponedMeetings();
					return;
				case OfflineWindowCloseReason.RequestStop:  // due to RequestStopWork
					IsolatedStorageSerializationHelper.Delete(OnGoingMeetingPath);
					return;
				case OfflineWindowCloseReason.SubmitWorks:
					var confirmedMeetings = GetConfirmedMeetings();
					Debug.Assert(confirmedMeetings != null);
					foreach (var finalManualMeeting in confirmedMeetings)
					{
						captureCoordinator.WorkItemManager.PersistAndSend(finalManualMeeting);
						log.Info("Created adhoc meeting for workId " + finalManualMeeting.ManualMeetingData.WorkId + " between " + finalManualMeeting.StartDate + " and " + finalManualMeeting.EndDate + " with subject: " + finalManualMeeting.ManualMeetingData.Title + ", description: " + finalManualMeeting.ManualMeetingData.Description + " meetingId: " + finalManualMeeting.Id);
					}
					var deletedMeetings = GetDeletedMeetings();
					Debug.Assert(deletedMeetings != null);
					foreach (var deletedMeeting in deletedMeetings.Where(m => m.ManualMeetingData.IncludedIdleMinutes == 0)) // only for manually started meetings not for inactivity
					{
						var endDate = deletedMeeting.EndDate;
						deletedMeeting.ManualMeetingData.EndTime = deletedMeeting.ManualMeetingData.StartTime;
						captureCoordinator.WorkItemManager.PersistAndSend(deletedMeeting);
						log.Info($"Deleted temporary adhoc meeting between {deletedMeeting.StartDate} and {endDate} meetingId: {deletedMeeting.Id}");
					}
					AddMeetingHistory.RecentEmails.AddRange(
						confirmedMeetings.SelectMany(n => n.ManualMeetingData.AttendeeEmails).Except(AddMeetingHistory.RecentEmails));
					AddMeetingHistory.RecentSubjects.AddRange(
						confirmedMeetings.Select(n => n.ManualMeetingData.Title).Except(AddMeetingHistory.RecentSubjects));
					AddMeetingHistory.Save();
					AddRecentWorkIds(confirmedMeetings.Select(n => n.ManualMeetingData.WorkId));
					break;
				default: //OfflineWindowCloseReason.CancelWorks
					DeleteOngoingManualWorkitem();
					var meetings = GetConfirmedMeetings();
					Debug.Assert(meetings != null);
					var deletedMeetings1 = GetDeletedMeetings();
					Debug.Assert(deletedMeetings1 != null);
					foreach (var deletedMeeting in meetings.Union(deletedMeetings1).Where(m => m.ManualMeetingData.IncludedIdleMinutes == 0)) // only for manually started meetings not for inactivity
					{
						var endDate = deletedMeeting.EndDate;
						deletedMeeting.ManualMeetingData.EndTime = deletedMeeting.ManualMeetingData.StartTime;
						captureCoordinator.WorkItemManager.PersistAndSend(deletedMeeting);
						log.Info($"Deleted temporary adhoc meeting between {deletedMeeting.StartDate} and {endDate} meetingId: {deletedMeeting.Id}");
					}
					break;
			}
			if (!IsPostponedPopup) IsolatedStorageSerializationHelper.Delete(OnGoingMeetingPath);
			RemovePostponedMeetings();
			counter = null; //we are not working anymore (must be called before resume)
			if (!resumeWorkOnClose || closeReason == OfflineWindowCloseReason.RequestStop) return;
			log.Info("Resuming work, meetingId: " + CountingMeetingId);
			CountingMeetingId = null;
			captureCoordinator.CurrentWorkController.UserResumeWork();
		}

		private void DeleteOngoingManualWorkitem()
		{
			if (!isPoppedUpAfterInactivity)
			{
				ManualMeetingItem ignoredMeeting = GetOnGoingMeetingDataDeletion();
				captureCoordinator.WorkItemManager.PersistAndSend(ignoredMeeting);
				log.Info("Ignored adhoc meeting started at " + ignoredMeeting.StartDate + " meetingId: " + CountingMeetingId);
			}
		}

		private void UpdatePostponedMeetings()
		{
			if (GetConfirmedMeetings == null) return;
			foreach (var meeting in GetModifiedMeetings())
			{
				if (meeting.StartDate == meeting.EndDate)
				{
					IsolatedStoragePostponedMeetingsHelper.Delete(meeting.Id);
					captureCoordinator.WorkItemManager.PersistAndSend(meeting);
					log.Info($"Temporary adhoc meeting deleted between {meeting.StartDate} and {meeting.EndDate} meetingId: {meeting.Id}");
				}
				else
				{
					IsolatedStoragePostponedMeetingsHelper.Save(new PostponedMeetingItem(
						new ManualMeetingData()
						{
							WorkId = meeting.GetWorkId(),
							StartTime = meeting.StartDate,
							EndTime = meeting.EndDate,
						})
					{
						Id = meeting.Id,
						UserId = ConfigManager.UserId,
						AssignData = meeting.AssignData,
						IsPoppedUpAfterInactivity = isPoppedUpAfterInactivity,
						NonAccountableDuration = meeting.NonAccountableDuration,
					});
					log.Info($"Adhoc meeting updated between {meeting.StartDate} and {meeting.EndDate} meetingId: {meeting.Id}");
				}
			}
		}

		private void RemovePostponedMeetings()
		{
			try
			{
				IsolatedStoragePostponedMeetingsHelper.DeleteAll();
			}
			catch (Exception ex) when (ex is IOException || ex is IsolatedStorageException)
			{
				log.Warn("Can't delete existing postponed meetings", ex);
			}
		}

		public void Dispose()
		{
			captureCoordinator.CurrentMenuChanged -= CurrentMenuChanged;
			captureCoordinator.OnUserActivity -= CaptureCoordinatorOnUserActivity;
		}

		private void CurrentMenuChanged(object sender, MenuEventArgs e)
		{
			OnUpdateMenu?.Invoke(this, SingleValueEventArgs.Create(e.MenuLookup));
		}

		public static bool CanSelectWork(WorkData workData)
		{
			var currentMenu = MenuQuery.Instance.ClientMenuLookup.Value;
			return workData != null
				&& !workData.ManualAddWorkDuration.HasValue
				&& workData.IsWorkIdFromServer
				&& (ConfigManager.LocalSettingsForUser.ShowDynamicWorks || workData.Id == null || !currentMenu.IsDynamicWork(workData.Id.Value))
				&& workData.IsVisibleInAdhocMeeting;
		}

		private void SetAddMeetingWorkId(Guid meetingId, int? workId)
		{
			Debug.Assert(counter != null);
			WorkDataWithParentNames workDataWithParents;
			if (counter != null
				&& workId.HasValue
				&& (workDataWithParents = captureCoordinator.CurrentWorkController.GetWorkDataWithParentNames(workId.Value)) != null
				&& CanSelectWork(workDataWithParents.WorkData))
			{
				Debug.Assert(workDataWithParents.WorkData != null && workDataWithParents.WorkData.Id.HasValue);
				OnSetWork?.Invoke(this, SingleValueEventArgs.Create(new Tuple<Guid, WorkDataWithParentNames>(meetingId, workDataWithParents)));
			}
		}

		private void StartOnGoingMeetingTimer()
		{
			Debug.Assert(onGoingMeetingTimer == null);
			if (onGoingMeetingTimer != null) return;
			onGoingMeetingTimer = new Timer();
			onGoingMeetingTimer.Tick += OnGoingMeetingTimerTick;
			onGoingMeetingTimer.Interval = (int)onGoingUpdateInterval.TotalMilliseconds;
			UpdateManualMeetingWorkItem();
			onGoingMeetingTimer.Enabled = true;
		}

		protected void StopOnGoingMeetingTimer()
		{
			if (onGoingMeetingTimer == null) return;
			onGoingMeetingTimer.Enabled = false;
			onGoingMeetingTimer.Tick -= OnGoingMeetingTimerTick;
			onGoingMeetingTimer.Dispose();
			onGoingMeetingTimer = null;
		}

		private int actUpdateManualMeetingTimerTickCount = ONGOING_PERSIST_AND_SEND_IN_MINUTES;
		private void UpdateManualMeetingWorkItem()
		{
			Debug.Assert(counter != null);
			var onGoingMeeting = GetOnGoingMeetingData(lastWorkBeforeMeeting, onGoingBookingInterval);
			if (onGoingMeeting == null)
			{
				log.Info("Ongoing meeting cannot be created");
				return;
			}

			var onGoingMeetingWithoutUpdateInterval = GetOnGoingMeetingData(lastWorkBeforeMeeting, TimeSpan.Zero);
			if (onGoingMeetingWithoutUpdateInterval != null)
			{
				IsolatedStorageSerializationHelper.Save(OnGoingMeetingPath, onGoingMeetingWithoutUpdateInterval);
			}
			if (isPoppedUpAfterInactivity || isLoadedBackInactivity || actUpdateManualMeetingTimerTickCount++ < ONGOING_PERSIST_AND_SEND_IN_MINUTES) return;
			actUpdateManualMeetingTimerTickCount = 0;
			captureCoordinator.WorkItemManager.PersistAndSend(onGoingMeeting);
			log.Info("Updated meeting manual work time for workId " + onGoingMeeting.ManualMeetingData.WorkId + " between " + onGoingMeeting.StartDate + " and " + onGoingMeeting.EndDate + " meetingId: " + CountingMeetingId);
		}

		public void UpdatePostponedMeetingWorkItem()
		{
			IsolatedStorageSerializationHelper.Delete(AdhocMeetingService.OnGoingMeetingPath);
			if (IsPostponedPopup) return;
			Debug.Assert(counter != null);
			var actMeeting = GetPostponedMeetingData(null /*lastWorkBeforeMeeting*/);
			if (actMeeting == null)
			{
				log.Info("Postponed meeting cannot be created");
				return;
			}
			IsolatedStoragePostponedMeetingsHelper.Save(actMeeting);
		}

		private void OnGoingMeetingTimerTick(object sender, EventArgs e)
		{
			UpdateManualMeetingWorkItem();
			ForceCloseIfApplicable();
		}

		private ManualMeetingItem GetOnGoingMeetingDataDeletion(DateTime? originalStartTime = null, int includedIdleMinutes = 0) //todo why sending OnGoingMeetingDataDeletion even if we didn't send any OnGoingMeetingData?
		{
			Debug.Assert(counter != null);
			if (counter == null) return null;
			return new ManualMeetingItem(
				new ManualMeetingData()
				{
					WorkId = 0, //hax...
					StartTime = counter.StartTime, //StartTime = EndTime won't go to the website, it will only delete the OnGoing meeting (if this is arrived later...)
					EndTime = counter.StartTime,
					OriginalStartTime = originalStartTime,
					IncludedIdleMinutes = includedIdleMinutes,
				})
			{
				Id = Guid.NewGuid(),
				UserId = ConfigManager.UserId,
			};
		}

		private ManualMeetingItem GetOnGoingMeetingData(WorkData fallbackWork, TimeSpan updateInterval)
		{
			Debug.Assert(counter != null);
			if (counter == null) return null;
			var workData = GetCurrentMeetingWork() ?? fallbackWork;
			if (workData == null)
			{
				log.Info("No work for ongoing meeting data");
				return null;
			}
			return new ManualMeetingItem(
					new ManualMeetingData()
					{
						OnGoing = true,
						WorkId = workData.Id.Value,
						StartTime = counter.StartTime,
						EndTime = counter.StartTime + GetMaxDuration(counter.GetDuration() + updateInterval),
						//Title = tabWithWorkId.Subject == null || tabWithWorkId.Subject.Length <= 200 ? tabWithWorkId.Subject : tabWithWorkId.Subject.Substring(0, 200), //this might be invalid (so we could make it valid) but was never sent
						//Description = tabWithWorkId.Description, //this might be invalid and was never sent
						//AttendeeEmails = emailPatternMatcher.Matches(tabWithWorkId.Participants).Cast<Match>().Select(n => n.Groups["addr"].Value.ToLower()).Distinct().ToList(), //this might be invalid and was never sent
					})
			{
				Id = Guid.NewGuid(),
				UserId = ConfigManager.UserId,
				AssignData = workData.AssignData,
				IsPoppedUpAfterInactivity = isPoppedUpAfterInactivity,
				ResumeWorkOnClose = resumeWorkOnClose,
			};
		}

		private PostponedMeetingItem GetPostponedMeetingData(WorkData fallbackWork)
		{
			Debug.Assert(counter != null);
			Debug.Assert(CountingMeetingId.HasValue);
			if (counter == null || !CountingMeetingId.HasValue) return null;
			var workData = GetCurrentMeetingWork() ?? fallbackWork;
			//if (workData == null)
			//{
			//	log.Info("No work for ongoing meeting data");
			//	return null;
			//}
			return new PostponedMeetingItem(
					new ManualMeetingData()
					{
						WorkId = workData?.Id ?? 0,
						StartTime = counter.StartTime,
						EndTime = counter.StartTime + GetMaxDuration(counter.GetDuration())
					})
			{
				Id = CountingMeetingId.Value,
				UserId = ConfigManager.UserId,
				AssignData = workData?.AssignData,
				IsPoppedUpAfterInactivity = isPoppedUpAfterInactivity,
				NonAccountableDuration = counter.GetPausedDuration(),
			};
		}

		private TimeSpan GetMaxDuration(TimeSpan targetDuration)
		{
			var maxInterval = isPoppedUpAfterInactivity ? ConfigManager.DuringWorkTimeIdleManualInterval : ConfigManager.MaxManualMeetingInterval;
			if (maxInterval < 0) return TimeSpan.Zero;
			return maxInterval == 0 || maxInterval > targetDuration.TotalMilliseconds ? targetDuration : TimeSpan.FromMilliseconds(maxInterval);
		}


		public void CheckUnfinishedOnGoingMeeting(Action<Action> invoke)
		{
			ManualMeetingItem mmi;
			if (IsolatedStorageSerializationHelper.Exists(AdhocMeetingService.OnGoingMeetingPath) &&
				IsolatedStorageSerializationHelper.Load(AdhocMeetingService.OnGoingMeetingPath, out mmi))
			{
				if (mmi.EndDate - mmi.StartDate < TimeSpan.FromMinutes(1))
				{
					var deletionData = new ManualMeetingData() { WorkId = 0, StartTime = mmi.StartDate, EndTime = mmi.StartDate };
					var deletionItem = new ManualMeetingItem(deletionData) { Id = Guid.NewGuid(), UserId = ConfigManager.UserId };
					captureCoordinator.WorkItemManager.PersistAndSend(deletionItem);
					IsolatedStorageSerializationHelper.Delete(AdhocMeetingService.OnGoingMeetingPath);
					log.Info("Found a too short unfinished ongoing meeting (ignore)");
				}
				else
				{
					log.Info("Found an unfinished ongoing meeting (popup)");
					invoke(() => StartWork(null, null, new LoadWorkParams
					{
						WorkId = mmi.GetWorkId(),
						Guid = mmi.Id,
						StartEndDateTime = new StartEndDateTime(mmi.StartDate, mmi.EndDate),
						OngoingMeeting = true,
						IsPoppedUpAfterInactivity = mmi.IsPoppedUpAfterInactivity,
						ResumeWorkOnClose = mmi.ResumeWorkOnClose
					}));
				}
			}
		}

		public void CheckPostponedMeetings(Action<Action> invoke)
		{
			var ageLimit = DateTime.UtcNow.AddHours(-ConfigManager.ManualWorkItemEditAgeLimit);
			foreach(var ppm in IsolatedStoragePostponedMeetingsHelper.Items.OrderBy(i => i.StartDate).ToList())
			{
				if (ppm.EndDate - ppm.StartDate < TimeSpan.FromMinutes(1))
				{
					var deletionData = new ManualMeetingData() { WorkId = 0, StartTime = ppm.StartDate, EndTime = ppm.StartDate };
					var deletionItem = new ManualMeetingItem(deletionData) { Id = Guid.NewGuid(), UserId = ConfigManager.UserId };
					captureCoordinator.WorkItemManager.PersistAndSend(deletionItem);
					IsolatedStoragePostponedMeetingsHelper.Delete(ppm.Id);
					log.Info($"Found a too short postponed meeting (ignored, id: {ppm.Id})");
				}
				else if (ppm.StartDate < ageLimit)
				{
					// too old to delete the manual workitem
					IsolatedStoragePostponedMeetingsHelper.Delete(ppm.Id);
					log.Info($"Found a too old postponed meeting (ignored, id: {ppm.Id})");
				}
				else
				{
					log.Info($"Found a postponed meeting (popup, id: {ppm.Id})");
					invoke(() => StartWork(null, null, new LoadWorkParams
					{
						WorkId = ppm.GetWorkId(),
						Guid = ppm.Id,
						StartEndDateTime = new StartEndDateTime(ppm.StartDate, ppm.EndDate),
						OngoingMeeting = false,
						IsPoppedUpAfterInactivity = ppm.IsPoppedUpAfterInactivity,
						ResumeWorkOnClose = false,
						NonAccountableDuration = ppm.NonAccountableDuration,
					}));
				}
			}
		}

		private static List<int> RecentWorksInAddMeeting
		{
			get
			{
				return string.IsNullOrEmpty(ConfigManager.LocalSettingsForUser.RecentWorksInAddMeeting)
					? new List<int>()
					: ConfigManager.LocalSettingsForUser.RecentWorksInAddMeeting.Split(',').Select(int.Parse).ToList();
			}
			set
			{
				ConfigManager.LocalSettingsForUser.RecentWorksInAddMeeting = string.Join(",", value.Select(n => n.ToString()).ToArray());
			}
		}

		private static void AddRecentWorkIds(IEnumerable<int> workIds)
		{
			var recentWorkIds = RecentWorksInAddMeeting;

			foreach (var workId in workIds)
			{
				recentWorkIds.Remove(workId);
				recentWorkIds.Insert(0, workId);
			}
			while (recentWorkIds.Count > ConfigManager.LocalSettingsForUser.MenuRecentItemsCount)
				recentWorkIds.RemoveAt(recentWorkIds.Count - 1);

			RecentWorksInAddMeeting = recentWorkIds;
		}

		// ReSharper disable PossibleMultipleEnumeration
		public static IEnumerable<int> RecentWorkIdSelectorForAddMeeting(ClientMenuLookup clientMenuLookup)
		{
			var recentWorkIdsInAddMeeting = RecentWorksInAddMeeting;
			recentWorkIdsInAddMeeting.RemoveAll(
				n =>
					!clientMenuLookup.WorkDataById.ContainsKey(n)
					|| !clientMenuLookup.WorkDataById[n].WorkData.IsVisibleInAdhocMeeting
					|| (!ConfigManager.LocalSettingsForUser.ShowDynamicWorks && clientMenuLookup.IsDynamicWork(n)));

			var works = MenuHelper.FlattenDistinctWorkDataThatHasId(clientMenuLookup.ClientMenu);
			var workIdsForAdhoc = works.Where(w => w.WorkData.IsVisibleInAdhocMeeting).Select(w => w.WorkData.Id.Value).ToArray();
			var workIdsForAdhocOnly = works.Where(w => w.WorkData.IsVisibleInAdhocMeetingOnly).Select(w => w.WorkData.Id.Value);

			var result = recentWorkIdsInAddMeeting.Union(workIdsForAdhocOnly);
			if (workIdsForAdhoc.Length <= ConfigManager.LocalSettingsForUser.MenuRecentItemsCount || !result.Any())
				result = workIdsForAdhoc;

			return result;
		}

		public static bool CheckMenuContainsWorkId(ClientMenuLookup clientMenuLookup, int workId) => MenuHelper
			.FlattenDistinctWorkDataThatHasId(clientMenuLookup.ClientMenu)
			.Where(w => w.WorkData.IsVisibleInAdhocMeeting).Any(w => w.WorkData.Id == workId);

		private void ForceCloseIfApplicable()
		{
			if (counter == null || counter.IsStopped || !isPoppedUpAfterInactivity) return;
			var maxDisplayTime = TimeSpan.FromMilliseconds(ConfigManager.DuringWorkTimeIdleManualInterval);
			if (maxDisplayTime == TimeSpan.Zero) return;
			var currDisplayTime = TimeSpan.FromMilliseconds((uint)(Environment.TickCount - counter.StartTicks));
			if (currDisplayTime <= maxDisplayTime) return;
			log.Info("Force closing idle popup after " + currDisplayTime.ToHourMinuteSecondString() + " max " + maxDisplayTime.ToHourMinuteSecondString());
			StopCounter();
			OnOverCountedAndDeleted?.Invoke(this, EventArgs.Empty);
			DeleteOngoingManualWorkitem();
			if (!IsPostponedPopup) IsolatedStorageSerializationHelper.Delete(OnGoingMeetingPath);
		}

		private void ClientSettingsManager_OnSettingsChanged(object sender, SingleValueEventArgs<ClientSetting> e)
		{

			if (!ConfigManager.AdHocMeetingDefaultSelectedTaskId.HasValue || CheckMenuContainsWorkId(
				    captureCoordinator.CurrentMenuLookupUnsafe,
				    ConfigManager.AdHocMeetingDefaultSelectedTaskId.Value)) return;

			log.Debug($"WorkId: {ConfigManager.AdHocMeetingDefaultSelectedTaskId.Value} not assigned to user, try to assign it...");
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var res = AllWorksManager.TryAssignTask(new WorkData
					{ Id = ConfigManager.AdHocMeetingDefaultSelectedTaskId.Value });
				if (res != AssignTaskResult.Ok)
					log.Warn($"Can't assign workId: {ConfigManager.AdHocMeetingDefaultSelectedTaskId.Value}, result: {res}");
			});
		}

	}
	public class StartWorkGUIparams
	{
		public Guid MeetingId { set; get; }
		public ClientMenuLookup MenuLookup { set; get; }
		public bool IsPoppedUpAfterInactivity { set; get; }
		public int IncludedIdleMins { set; get; }
		public MeetingWorkTimeCounter Counter { set; get; }
		public bool IsPostponedPopup { set; get; }
		public bool IsInWorkBefore { set; get; }
		public bool IsUnconfirmedPopup { get; set; }
	}
	public class LoadWorkParams
	{
		public int WorkId { get; set; }
		public Guid Guid { get; set; }
		public StartEndDateTime StartEndDateTime { get; set; }
		public bool OngoingMeeting { get; set; }
		public bool IsPoppedUpAfterInactivity { get; set; }
		public bool ResumeWorkOnClose { get; set; }
		public TimeSpan NonAccountableDuration { get; set; }
	}

}
