using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Meeting;
using Tct.ActivityRecorderClient.Meeting;
using Tct.ActivityRecorderClient.Meeting.Adhoc;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View.Navigation;
using Timer = System.Threading.Timer;

namespace Tct.ActivityRecorderClient.View.Presenters
{
	public class OfflineWorkPresenter : IDisposable
	{
		private static readonly long gapForSensingUserActivity = TimeSpan.FromSeconds(3).Ticks / TimeSpan.TicksPerMillisecond;  // TickCount is in milliseconds
		private static readonly Regex emailPatternMatcher = new Regex("\\s*((?<name>(\"[^\"]*\")|([^\\[<\"]*[^\\s]))\\s*[<\\[])?(?<addr>[a-zA-Z0-9._%+-]+@(([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]*\\.)+[a-zA-Z0-9]{2,17}))[\\]>]?[,;|\\s]*");

		private readonly IOfflineWorkView view;
		private readonly IAdhocMeetingService service;
		private readonly Timer idleTimer;

		private readonly IAddressBookService addressBookService;
		private readonly List<MeetingInfoEx> meetingInfos = new List<MeetingInfoEx>();
		private readonly List<MeetingInfoEx> deletedMeetings = new List<MeetingInfoEx>();
		private MeetingInfoEx runningMeeting, selectedMeeting, nonAccountableInfo;
		private HashSet<int> newlyAddedRecentWorkIds = new HashSet<int>();
		private bool isActivityCheckCancelled;
		private bool isAddressBookServiceInitialized;
		private DateTime prevDay = DateTime.Today;
		private DateTime lastSetFromMenu;
		private bool isDisposing;
		private bool duringPostponedLoading;

		public OfflineWorkPresenter(IOfflineWorkView view, IAdhocMeetingService service, IAddressBookService addressBookService)
		{
			this.view = view;
			this.service = service;
			this.addressBookService = addressBookService;
			service.OnShowGui += AdhocMeetingService_OnShowGui;
			service.OnAbortAndClose += AdhocMeetingService_OnAbortAndClose;
			service.OnUpdateMenu += AdhocMeetingService_OnUpdateMenu;
			service.OnSetWork += AdhocMeetingService_OnSetWork;
			service.OnShowMessageBox += AdhocMeetingService_OnShowMessageBox;
			service.OnKickWork += AdhocMeetingService_OnKickWork;
			service.GetCurrentMeetingWork = GetCurrentMeetingWork;
			service.GetConfirmedMeetings = GetConfirmedMeetings;
			service.GetDeletedMeetings = GetDeletedMeetings;
			service.GetModifiedMeetings = GetModifiedMeetings;
			service.OnUserActivityWhileMeeting += AdhocMeetingService_OnUserActivityWhileMeeting;
			service.OnOverCountedAndDeleted += AdhocMeetingService_OnOverCountedAndDeleted;

			idleTimer = new Timer(idleTimerTick, null, 200, 250);
			/** for current work
			var baseStopCounter = sgp.StopCounter;
			sgp.StopCounter = () =>
			{
				baseStopCounter();
				view.SetAlternativeMenu(null, null);
			};
			if (!sgp.IsPostponedPopup) view.SetAlternativeMenu(MenuWorkItemClick, Labels.AddMeeting_SelectWorkCaption);
			 */
			//form.OnStopMeetingTimer += StopOnGoingMeetingTimer;
		}

		//private void MenuWorkItemClick(WorkDataEventArgs arg)
		//{
		//	if (AdhocMeetingService.CanSelectWork(arg.WorkData)) view.SetWork(arg.WorkData);
		//}

		private void AdhocMeetingService_OnShowGui(object sender, EventArgs e)
		{
			DebugEx.EnsureGuiThread();
			view.PopupView();
		}

		private void AdhocMeetingService_OnAbortAndClose(object sender, SingleValueEventArgs<bool> e)
		{
			DebugEx.EnsureGuiThread();
			view.AbortAndClose(e.Value);
		}

		void AdhocMeetingService_OnUserActivityWhileMeeting(object sender, SingleValueEventArgs<bool> e)
		{
			// hack, 'coz of calling approach, activation is awaited for a short time
			// possible race because runningMeeting set on GUI thread
			if (runningMeeting == null || Environment.TickCount < runningMeeting.Counter.StartTicks + gapForSensingUserActivity) return;
			if (DateTime.UtcNow - lastSetFromMenu < TimeSpan.FromMilliseconds(500)) return;
			if (isActivityCheckCancelled) return;
			view.RunOnGui(()=> view.HandleUserActivity(e.Value));
		}

		private void AdhocMeetingService_OnUpdateMenu(object sender, SingleValueEventArgs<ClientMenuLookup> e)
		{
			DebugEx.EnsureGuiThread();
			//update tab states
			PerformUpdateMenu(e.Value);
		}

		private void PerformUpdateMenu(ClientMenuLookup menuLookup)
		{
			foreach (var info in meetingInfos)
			{
				info.ClientMenuLookup = menuLookup;
				if (info.NavigationWork != null)
				{
					var workDataWithParentNames = info.ClientMenuLookup.GetWorkDataWithParentNames(info.NavigationWork.Work.WorkData.Id.Value);
					info.NavigationWork = workDataWithParentNames != null ? new NavigationWork(info.Navigator, workDataWithParentNames) : null;
					if (info.NavigationWork != null && !AdhocMeetingService.CanSelectWork(info.NavigationWork.Work.WorkData))
					{
						info.NavigationWork = null;
					}
				}
			}
		}

		private WorkData GetCurrentMeetingWork()
		{
			return runningMeeting?.NavigationWork?.Work.WorkData;
		}

		private ManualMeetingItem[] GetConfirmedMeetings()
		{
			var meetings = new List<ManualMeetingItem>();
			foreach (var meetingInfo in meetingInfos.Where(m => m.CardStyle != CardStyle.CannotAccountable && m.CardStyle != CardStyle.Deleted && m.NavigationWork?.Work.WorkData.Id != null))
			{
				var data = new ManualMeetingItem(
					new ManualMeetingData()
					{
						WorkId = meetingInfo.NavigationWork?.Work.WorkData.Id ?? 0,
						StartTime = meetingInfo.StartTime,
						EndTime = meetingInfo.StartTime + meetingInfo.Duration,
						Title = meetingInfo.Subject,
						Description = meetingInfo.Comment,
						AttendeeEmails = emailPatternMatcher.Matches(meetingInfo.Participants).Cast<Match>().Select(n => n.Groups["addr"].Value.ToLower()).Distinct().ToList(),
					})
				{
					Id = meetingInfo.Id,
					UserId = ConfigManager.UserId,
					AssignData = meetingInfo.NavigationWork?.Work.WorkData.AssignData,
				};
				meetings.Add(data);
			}
			return meetings.ToArray();
		}

		private ManualMeetingItem[] GetDeletedMeetings()
		{
			var meetings = new List<ManualMeetingItem>();
			foreach (var meetingInfo in meetingInfos.Where(m => m.CardStyle == CardStyle.Deleted))
			{
				var data = new ManualMeetingItem(
					new ManualMeetingData()
					{
						StartTime = meetingInfo.StartTime,
						EndTime = meetingInfo.EndTime,
					})
				{
					Id = meetingInfo.Id,
					UserId = ConfigManager.UserId,
				};
				meetings.Add(data);
			}
			return meetings.ToArray();
		}

		private ManualMeetingItem[] GetModifiedMeetings()
		{
			var meetings = new List<ManualMeetingItem>();
			foreach (var meetingInfo in meetingInfos.Where(m => m.IsChanged && m.CardStyle != CardStyle.CannotAccountable).Union(deletedMeetings))
			{
				var data = new ManualMeetingItem(
					new ManualMeetingData()
					{
						WorkId = meetingInfo.NavigationWork?.Work.WorkData.Id ?? 0,
						StartTime = meetingInfo.StartTime,
						EndTime = meetingInfo.CardStyle == CardStyle.Deleted ? meetingInfo.StartTime : meetingInfo.EndTime,
						Title = meetingInfo.Subject,
						Description = meetingInfo.Comment,
						AttendeeEmails = emailPatternMatcher.Matches(meetingInfo.Participants).Cast<Match>().Select(n => n.Groups["addr"].Value.ToLower()).Distinct().ToList(),
					})
				{
					Id = meetingInfo.Id,
					UserId = ConfigManager.UserId,
					AssignData = meetingInfo.NavigationWork?.Work.WorkData.AssignData,
					NonAccountableDuration = meetingInfo.NonAccountableDuration,
				};
				meetings.Add(data);
			}
			return meetings.ToArray();
		}

		internal void DeleteCard(MeetingInfo meetingInfo, bool isDeleted)
		{
			meetingInfo.CardStyle = isDeleted ? CardStyle.Deleted : meetingInfo.IsAnyInvalid ? CardStyle.Incomplete : CardStyle.Normal;
			var infoEx = meetingInfo as MeetingInfoEx;
			infoEx.IsChanged = true;
			view.ModifyIntervalColor(infoEx.LeftSplitterIndex, meetingInfo.CardStyle);
			ValidateInput(meetingInfo); // because validation error doesn't matter in deleted state
			RefreshTotal();
		}

		private void AdhocMeetingService_OnSetWork(object sender, SingleValueEventArgs<Tuple<Guid, WorkDataWithParentNames>> e)
		{
			DebugEx.EnsureGuiThread();
			// queueing this steps
			view.RunOnGui(() =>
			{
				var meeting = meetingInfos.FirstOrDefault(m => m.Id == e.Value.Item1);
				if (meeting == null) return; // item has been already deleted or not shown yet
				SetWork(meeting, e.Value.Item2);
				SetCardStyleAndCloseOthersWhenExpanded(meeting, meeting.CardStyle == CardStyle.Selected ? CardStyle.Selected : meeting.IsAnyInvalid ? CardStyle.Incomplete : CardStyle.Normal);
			});
		}

		private void SetWork(MeetingInfoEx meeting, WorkDataWithParentNames work)
		{
			meeting.NavigationWork = new NavigationWork(meeting.Navigator, work) { CanFavorite = false };
			ValidateInput(meeting);
		}

		private void AdhocMeetingService_OnShowMessageBox(object sender, SingleValueEventArgs<KeyValuePair<string, string>> e)
		{
			DebugEx.EnsureGuiThread();
			view.ShowMessageBox(e.Value.Key, e.Value.Value);
		}

		private void AdhocMeetingService_OnKickWork(object sender, EventArgs e)
		{
			DebugEx.EnsureGuiThread();
			StopCounterOnView();
		}

		private void StopCounterOnView()
		{
			idleTimerTick(null);
			runningMeeting = null;
			view.UpdateStopWatch(false);
			view.SetAlternativeMenu(null, null);
		}

		public void Dispose()
		{
			isDisposing = true;
			service.OnShowGui -= AdhocMeetingService_OnShowGui;
			service.OnAbortAndClose -= AdhocMeetingService_OnAbortAndClose;
			service.OnUpdateMenu -= AdhocMeetingService_OnUpdateMenu;
			service.OnSetWork -= AdhocMeetingService_OnSetWork;
			service.OnShowMessageBox -= AdhocMeetingService_OnShowMessageBox;
			service.OnKickWork -= AdhocMeetingService_OnKickWork;
			service.OnUserActivityWhileMeeting -= AdhocMeetingService_OnUserActivityWhileMeeting;
			service.GetCurrentMeetingWork = null;
			service.GetConfirmedMeetings = null;
			service.OnOverCountedAndDeleted -= AdhocMeetingService_OnOverCountedAndDeleted;
			idleTimer.Dispose();
			displayTimer?.Dispose();
		}

		internal void Show()
		{
			view.ShowView();
		}

		public void ViewClosed(OfflineWindowCloseReason closeReason)
		{
			service.OnClosed(closeReason);
			view.SetAlternativeMenu(null, null);
			service.CloseGui();
		}

		public void StartWork(StartWorkGUIparams sgp)
		{
			var info = new MeetingInfoEx()
			{
				Id = sgp.MeetingId,
				Counter = !sgp.Counter.IsStopped ? sgp.Counter : null,
				StartTime = sgp.Counter.StartTime,
				EndTime = sgp.Counter.EndTime,
				Duration = sgp.Counter.GetDuration(),
				NonAccountableDuration = sgp.Counter.GetPausedDuration(),
				OfflineWorkType = sgp.IsPoppedUpAfterInactivity ? OfflineWorkType.AfterInactivity : OfflineWorkType.ManuallyStarted,
				Subject = string.Empty,
				Comment = string.Empty,
				Participants = string.Empty,
				ClientMenuLookup = sgp.MenuLookup,
				IsAddressbookVisible = addressBookService.IsAddressBookServiceAvailable,
			};
			AddMeeting(info, sgp.IsPostponedPopup);
			if (!sgp.IsPostponedPopup) view.SetAlternativeMenu(ClickOnMenu, Labels.AddMeeting_SelectWorkCaption);

		}

		private void ClickOnMenu(WorkDataEventArgs e)
		{
			if (runningMeeting == null || e == null || !e.WorkData.Id.HasValue) return;
			var work = runningMeeting.ClientMenuLookup.GetWorkDataWithParentNames(e.WorkData.Id.Value);
			if (work == null || !AdhocMeetingService.CanSelectWork(work.WorkData)) return;
			SetWork(runningMeeting, work);
			SetCardStyleAndCloseOthersWhenExpanded(runningMeeting, CardStyle.Selected);
			view.ModifyIntervalColor(runningMeeting.LeftSplitterIndex, runningMeeting.CardStyle);
			lastSetFromMenu = DateTime.UtcNow;
		}

		private void AddMeeting(MeetingInfoEx info, bool isPostponedPopup)
		{
			UpdateCard(info);
			ValidateInput(info);
			duringPostponedLoading = isPostponedPopup;
			if (!isPostponedPopup)
			{
				view.ActivateView();
				runningMeeting = info;
			}
			SetCardStyleAndCloseOthersWhenExpanded(info, isPostponedPopup ? info.IsAnyInvalid ? CardStyle.Incomplete : CardStyle.Normal : CardStyle.Selected);
			view.AddMeetingCard(info, nonAccountableInfo != null ? meetingInfos.Where(m => m.CardStyle != CardStyle.CannotAccountable).OrderBy(m => m.StartTime).Select(m => m.Id).LastOrDefault() : Guid.Empty);
			var closeNeighb = meetingInfos.FirstOrDefault(m => m.EndTime == info.StartTime);
			if (closeNeighb == null)
				info.LeftSplitterIndex = view.AddInterval(info.StartTime, info.CardStyle);
			info.RightSplitterIndex = view.AddInterval(info.EndTime, CardStyle.None);
			if (closeNeighb != null) 
			{
				view.ModifyIntervalEnd(closeNeighb.RightSplitterIndex, true);
				info.LeftSplitterIndex = closeNeighb.RightSplitterIndex;
				view.ModifyIntervalColor(info.LeftSplitterIndex, info.CardStyle);
			}
			info.Navigator.OnNavigate += NavigatorOnNavigate;
			meetingInfos.Add(info);
			RefreshTotal();
		}

		private void NavigatorOnNavigate(object sender, SingleValueEventArgs<NavigationBase> singleValueEventArgs)
		{
			if (!(sender is MyNavigator navigator)) return;
			CardClicked(navigator.Meeting);
			if (navigator.Meeting.CardStyle == CardStyle.Selected) view.DropdownTaskList(navigator.Meeting.Id);
		}

		private void idleTimerTick(object state)
		{
			view.RunOnGui(() =>
			{
				if (isDisposing) return;
				if (prevDay != DateTime.Today)
				{
					foreach (var info in meetingInfos)
					{
						UpdateCard(info);
					}
					prevDay = DateTime.Today;
				}

				if (runningMeeting != null) RefreshTotal();
			});
		}

		private void RefreshTotal()
		{
			DebugEx.EnsureGuiThread();
			if (runningMeeting != null)
			{
				UpdateCard(runningMeeting);
				ValidateInput(runningMeeting);
				view.ModifyIntervalTime(runningMeeting.RightSplitterIndex, runningMeeting.EndTime);
				view.UpdateStopWatch(true);
				RefreshIntervalAttributes();
			}
			else view.UpdateStopWatch(false);
			var total = TimeSpan.FromMilliseconds(meetingInfos.Where(m => m.CardStyle != CardStyle.CannotAccountable && m.CardStyle != CardStyle.Deleted).Sum(m => m.Duration.TotalMilliseconds));
			var nonAccTotal = TimeSpan.FromMilliseconds(meetingInfos.Where(m => m.CardStyle != CardStyle.CannotAccountable && m.CardStyle != CardStyle.Deleted).Sum(m => m.NonAccountableDuration.TotalMilliseconds));
			if (nonAccountableInfo != null)
			{
				if (nonAccTotal != TimeSpan.Zero)
				{
					nonAccountableInfo.Duration = nonAccTotal;
					UpdateCard(nonAccountableInfo);
				}
				else
				{
					view.DeleteMeetingCard(nonAccountableInfo.Id);
					nonAccountableInfo = null;
				}
			}
			else
			{
				if (nonAccTotal != TimeSpan.Zero)
				{
					nonAccountableInfo = new MeetingInfoEx() { Id =  Guid.NewGuid(), CardStyle = CardStyle.CannotAccountable, Duration = nonAccTotal };
					UpdateCard(nonAccountableInfo);
					view.AddMeetingCard(nonAccountableInfo, Guid.Empty);
				}
			}
			view.UpdateTotal(FormatDuration(total, false));
		}

		private static void UpdateCard(MeetingInfo meetingInfo)
		{
			var ex = meetingInfo as MeetingInfoEx;
			Debug.Assert(ex != null);
			ex.HasBadge = ex.NonAccountableDuration != TimeSpan.Zero;
			ex.DurationText = Tuple.Create(FormatDuration(ex.Duration, true), FormatDuration(ex.Duration, false));
			ex.StartTimeText = Tuple.Create(FormatDateTime(ex.StartTime, true), FormatDateTime(ex.StartTime, false));
			var endTime = ex.EndTime;
			ex.EndTimeText = Tuple.Create(FormatDateTime(endTime, true), FormatDateTime(endTime, false));
		}

		internal static string FormatDuration(TimeSpan value, bool isShort)
		{
			if (!isShort) return $"{Math.Truncate(value.TotalHours):00}:{value.Minutes:00}'{value.Seconds:00}";
			var roundedMins = TimeSpan.FromMinutes(Math.Round(value.TotalMinutes));
			return Math.Truncate(roundedMins.TotalHours) >= 100f ? $"99+h" : $"{Math.Truncate(roundedMins.TotalHours):00}:{roundedMins.Minutes:00}";
		}

		internal static string FormatDateTime(DateTime value, bool isShort)
		{
			var localTime = value.ToLocalTime();
			var sb = new StringBuilder();
			if (!isShort || localTime.Date != DateTime.Now.Date)
			{
				sb.Append(isShort ? localTime.ToString("MM/dd") : localTime.ToShortDateString());
				sb.Append('\n');
			}
			sb.Append(isShort ? localTime.ToString("HH:mm") : localTime.ToLongTimeString());
			return sb.ToString();
		}

		public void CardClicked(MeetingInfo meetingInfo)
		{
			if (meetingInfo.CardStyle == CardStyle.Deleted || meetingInfo.CardStyle == CardStyle.CannotAccountable) return;
			duringPostponedLoading = false;
			var meetingInfoEx = meetingInfo as MeetingInfoEx;
			Debug.Assert(meetingInfoEx != null);
			SetCardStyleAndCloseOthersWhenExpanded(meetingInfoEx);
			view.ModifyIntervalColor(meetingInfoEx.LeftSplitterIndex, meetingInfo.CardStyle);
		}

		private void SetCardStyleAndCloseOthersWhenExpanded(MeetingInfoEx meetingInfo)
		{
			SetCardStyleAndCloseOthersWhenExpanded(meetingInfo, meetingInfo.CardStyle == CardStyle.Selected || duringPostponedLoading ? meetingInfo.IsAnyInvalid ? CardStyle.Incomplete : CardStyle.Normal : CardStyle.Selected);
		}

		private void SetCardStyleAndCloseOthersWhenExpanded(MeetingInfoEx meetingInfo, CardStyle style)
		{
			if (style == CardStyle.Selected)
			{
				foreach (var other in meetingInfos.Where(m => m != meetingInfo && m.CardStyle == CardStyle.Selected))
				{
					other.CardStyle = other.IsAnyInvalid ? CardStyle.Incomplete : CardStyle.Normal;
					view.ModifyIntervalColor(other.LeftSplitterIndex, other.CardStyle);
				}
				selectedMeeting = meetingInfo;
			}
			else selectedMeeting = null;
			meetingInfo.CardStyle = style;
			view.ModifyIntervalColor(meetingInfo.LeftSplitterIndex, style);
			RefreshIntervalAttributes();
		}

		public void AutoReturnFromMeeting()
		{
			DebugEx.EnsureGuiThread();
			service.AutoReturnFromMeeting();
			StopCounterOnView();
		}

		public bool CanSelectWork(WorkData work)
		{
			return AdhocMeetingService.CanSelectWork(work);
		}

		public IEnumerable<int> RecentWorkIdsSelector(ClientMenuLookup menuLookup)
		{
			List<int> res;
			if (ConfigManager.AdHocMeetingDefaultSelectedTaskId.HasValue && AdhocMeetingService.CheckMenuContainsWorkId(menuLookup, ConfigManager.AdHocMeetingDefaultSelectedTaskId.Value))
				res = new List<int> { ConfigManager.AdHocMeetingDefaultSelectedTaskId.Value };
			else
				res = new List<int>();
			res.AddRange(newlyAddedRecentWorkIds.Concat(AdhocMeetingService.RecentWorkIdSelectorForAddMeeting(menuLookup)
				.Where(w => !ConfigManager.AdHocMeetingDefaultSelectedTaskId.HasValue ||
				            w != ConfigManager.AdHocMeetingDefaultSelectedTaskId.Value)
				.Except(newlyAddedRecentWorkIds)));
			return res;
		}

		public void WorkSelectionChanged(MeetingInfo info, object value)
		{
			var work = value as WorkDataWithParentNames;
			if (work?.WorkData?.Id != null)
			{
				info.NavigationWork = new NavigationWork(((MeetingInfoEx)info).Navigator, work) { CanFavorite = false };
				newlyAddedRecentWorkIds.Add(work.WorkData.Id.Value);
			}
			else
			{
				var lastId = info.NavigationWork?.Work?.WorkData?.Id;
				if (lastId != null) newlyAddedRecentWorkIds.Remove(lastId.Value);
				info.NavigationWork = null;
			}
		}

		public void ValidateInput(MeetingInfo meetingInfo)
		{
			var ex = meetingInfo as MeetingInfoEx;
			Debug.Assert(ex != null);
			var start = ex.StartTime;
			var dur = ex.Duration;
			var idleInterval = ex.Duration;
			var isEmailsValid = string.IsNullOrEmpty(emailPatternMatcher.Replace(ex.Participants, "").Trim());
			var maxIntervalMilliseconds = ex.OfflineWorkType == OfflineWorkType.AfterInactivity ? ConfigManager.DuringWorkTimeIdleManualInterval : ConfigManager.MaxManualMeetingInterval;
			var maxInterval = TimeSpan.FromMilliseconds(maxIntervalMilliseconds < 0 ? 0 : maxIntervalMilliseconds);
			var isInsideLimit = maxIntervalMilliseconds == 0 || dur.TotalMilliseconds <= maxIntervalMilliseconds;
			var isDurationValid = dur > TimeSpan.Zero && isInsideLimit;
			var isSubjectValid = (!ConfigManager.IsMeetingSubjectMandatory || ex.Subject.Length > 0) && ex.Subject.Length <= 200;
			var isDescriptionValid = ex.Comment.Length <= 1000;
			var isParticipantLengthValid = ex.Participants.Length <= 2000;
			meetingInfo.IsTaskSelectionInvalid = meetingInfo.NavigationWork?.Work == null;
			meetingInfo.IsSubjectLengthInvalid = !isSubjectValid;
			meetingInfo.IsDescriptionLengthInvalid = !isDescriptionValid;
			meetingInfo.IsEmailsLengthInvalid = !isParticipantLengthValid;
			meetingInfo.IsEmailFormatError = !isEmailsValid;
			meetingInfo.IsDurationInvalid = !isDurationValid;
			meetingInfo.IsDurationExceedLimit = !isInsideLimit;
			var isCardsValid = meetingInfos.All(m => !m.IsAnyInvalid || m.CardStyle == CardStyle.CannotAccountable || m.CardStyle == CardStyle.Deleted);
			view.IsRecordActionAvailable = isCardsValid;
		}

		public bool ConfirmClosing(OfflineWindowCloseReason closeReason, bool ownerIsDuringExit)
		{
			if (ownerIsDuringExit || closeReason == OfflineWindowCloseReason.SubmitWorks || closeReason == OfflineWindowCloseReason.RequestStop) return true;
			if (closeReason != OfflineWindowCloseReason.CancelWorks && closeReason != OfflineWindowCloseReason.QueryShutdown)
			{
				var meetingsWithUnfilledTask = meetingInfos.Where(m => m.CardStyle != CardStyle.CannotAccountable && m.CardStyle != CardStyle.Deleted && m.NavigationWork?.Work.WorkData.Id == null).ToList();
				if (meetingsWithUnfilledTask.Count > 0)
				{
					foreach (var infoEx in meetingsWithUnfilledTask)
					{
						ValidateInput(infoEx);
					}
					return false;
				}
			}
			isActivityCheckCancelled = true;
			var res = view.ShowCloseConfirmationDialog();
			isActivityCheckCancelled = false;
			return res;
		}

		private void AdhocMeetingService_OnOverCountedAndDeleted(object sender, EventArgs e)
		{
			view.RunOnGui(() =>
			{
				if (runningMeeting == null) return;
				view.DeleteMeetingCard(runningMeeting.Id);
				meetingInfos.Remove(runningMeeting);
				view.RemoveInterval(runningMeeting.LeftSplitterIndex);
				view.RemoveInterval(runningMeeting.RightSplitterIndex);
				RefreshIntervalAttributes();
				runningMeeting = null;
				RefreshTotal();
				if (meetingInfos.Count == 0) view.AbortAndClose(true);
			});
		}

		public void IntervalSelected(int index)
		{
			var meetingInfo = meetingInfos.FirstOrDefault(m => m.LeftSplitterIndex == index);
			if (meetingInfo == null || meetingInfo.CardStyle == CardStyle.Selected) return;
			CardClicked(meetingInfo);
		}

		public void SplitInterval()
		{
			if (selectedMeeting == null || selectedMeeting.Duration.TotalMinutes < 2) return;
			var end = selectedMeeting.EndTime;
			var newduration = TimeSpan.FromMinutes(Math.Round(selectedMeeting.Duration.TotalMinutes / 2));
			var newNonAccountable = TimeSpan.FromMinutes(Math.Round(selectedMeeting.NonAccountableDuration.TotalMinutes / 2));
			var info = new MeetingInfoEx()
			{
				Id = Guid.NewGuid(),
				Counter = selectedMeeting.Counter,
				StartTime = selectedMeeting.StartTime + newduration + newNonAccountable,
				EndTime = end,
				Duration = end - selectedMeeting.StartTime - newduration - selectedMeeting.NonAccountableDuration /*- newNonAccountable + newNonAccountable */,
				NonAccountableDuration = selectedMeeting.NonAccountableDuration - newNonAccountable,
				OfflineWorkType = selectedMeeting.OfflineWorkType,
				Subject = string.Empty,
				Comment = string.Empty,
				Participants = string.Empty,
				ClientMenuLookup = selectedMeeting.ClientMenuLookup,
				RightSplitterIndex = selectedMeeting.RightSplitterIndex,
				IsAddressbookVisible = addressBookService.IsAddressBookServiceAvailable,
				IsChanged = true,
			};
			selectedMeeting.Counter = null;
			selectedMeeting.Duration = newduration;
			selectedMeeting.EndTime = selectedMeeting.StartTime + newduration + newNonAccountable;
			selectedMeeting.NonAccountableDuration = newNonAccountable;
			UpdateCard(selectedMeeting);
			if (selectedMeeting == runningMeeting) runningMeeting = info;
			UpdateCard(info);
			meetingInfos.Add(info);
			ValidateInput(info);
			info.CardStyle = info.IsAnyInvalid ? CardStyle.Incomplete : CardStyle.Normal;
			view.AddMeetingCard(info, selectedMeeting.Id);
			RefreshTotal();
			info.LeftSplitterIndex = selectedMeeting.RightSplitterIndex = view.InsertInterval(selectedMeeting.LeftSplitterIndex, info.StartTime, info.CardStyle, true);
			RefreshIntervalAttributes();
		}

		public void MergeInterval()
		{
			if (selectedMeeting == null) return;
			var next = meetingInfos.FirstOrDefault(m => m.StartTime >= selectedMeeting.EndTime && m.StartTime - selectedMeeting.EndTime < TimeSpan.FromSeconds(1)); // ~equals, avoid rounding problems
			if (next == null) return;
			view.DeleteMeetingCard(next.Id);
			var duration = selectedMeeting.Duration + next.Duration;
			var nonAccDur = selectedMeeting.NonAccountableDuration + next.NonAccountableDuration;
			var endTime = next.EndTime;
			selectedMeeting.Counter = next.Counter;
			selectedMeeting.Duration = duration;
			selectedMeeting.NonAccountableDuration = nonAccDur;
			selectedMeeting.EndTime = endTime;
			if (runningMeeting == next) runningMeeting = selectedMeeting;
			meetingInfos.Remove(next);
			next.CardStyle = CardStyle.Deleted;
			deletedMeetings.Add(next);
			UpdateCard(selectedMeeting);
			ValidateInput(selectedMeeting);
			view.RemoveInterval(next.LeftSplitterIndex);
			selectedMeeting.RightSplitterIndex = next.RightSplitterIndex;
			RefreshTotal();
			RefreshIntervalAttributes();
		}

		private readonly HashSet<MeetingInfo> displaySet = new HashSet<MeetingInfo>();
		private Timer displayTimer;
		public void IntervalTimeChanged(int index, DateTime time)
		{
			var endInfo = meetingInfos.FirstOrDefault(m => m.RightSplitterIndex == index);
			var startInfo = meetingInfos.FirstOrDefault(m => m.LeftSplitterIndex == index);
			if (endInfo != null)
			{
				var ratio = endInfo.NonAccountableDuration.TotalMilliseconds + endInfo.Duration.TotalMilliseconds;
				var dur = (time - endInfo.StartTime).TotalMilliseconds;
				endInfo.NonAccountableDuration = TimeSpan.FromMilliseconds(ratio > 0.0 ? dur * (endInfo.NonAccountableDuration.TotalMilliseconds / ratio) : 0.0);
				endInfo.Duration = TimeSpan.FromMilliseconds(ratio > 0.0 ? dur * (endInfo.Duration.TotalMilliseconds / ratio) : 0.0);
				endInfo.EndTime = time;
				AddDisplaySet(endInfo);
			}
			if (startInfo != null)
			{
				var ratio = startInfo.NonAccountableDuration.TotalMilliseconds + startInfo.Duration.TotalMilliseconds;
				var dur = (startInfo.Duration + startInfo.NonAccountableDuration - (time - startInfo.StartTime)).TotalMilliseconds;
				startInfo.NonAccountableDuration = TimeSpan.FromMilliseconds(ratio > 0.0 ? dur * (startInfo.NonAccountableDuration.TotalMilliseconds / ratio) : 0.0);
				startInfo.Duration = TimeSpan.FromMilliseconds(ratio > 0.0 ? dur * (startInfo.Duration.TotalMilliseconds / ratio) : 0.0);
				startInfo.StartTime = time;
				AddDisplaySet(startInfo);
			}
			RefreshTotal();
		}

		private void AddDisplaySet(MeetingInfo meetingInfo)
		{
			lock (displaySet)
			{
				var cnt = displaySet.Count;
				displaySet.Add(meetingInfo);
				if (displayTimer == null)
					displayTimer = new Timer(DisplayTimerCallback, null, 100, Timeout.Infinite);
				else if (cnt == 0)
					displayTimer.Change(100, Timeout.Infinite);
			}
		}

		private void DisplayTimerCallback(object state)
		{
			lock (displaySet)
			{
				foreach (var meetingInfo in displaySet)
				{
					view.RunOnGui(() => {
						UpdateCard(meetingInfo);
						ValidateInput(meetingInfo);
					});
				}
				displaySet.Clear();
				displayTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}
		}

		private void RefreshIntervalAttributes()
		{
			view.IsSplitButtonEnabled = selectedMeeting != null && selectedMeeting.Duration.TotalMinutes >= 2;
			var next = selectedMeeting != null ? meetingInfos.FirstOrDefault(m => m.StartTime >= selectedMeeting.EndTime && m.StartTime - selectedMeeting.EndTime < TimeSpan.FromSeconds(1)) : null;
			view.IsMergeButtonEnabled = meetingInfos.Count > 1 ? selectedMeeting != null && next != null : (bool?)null;
			view.IsIntervalSplitterEnabled = meetingInfos.Count > 1;
		}

		public void MeetingInfoChanged(MeetingInfo info)
		{
			var startTime = (info as MeetingInfoEx)?.StartTime ?? DateTime.MaxValue;
			foreach (var infoEx in meetingInfos.Where(m => m.StartTime > startTime).OrderBy(m => m.StartTime).ToList())
			{
				var changed = false;
				if (string.IsNullOrEmpty(infoEx.Subject) && !string.IsNullOrEmpty(info.Subject))
				{
					infoEx.Subject = info.Subject;
					changed = true;
				}
				if (string.IsNullOrEmpty(infoEx.Comment) && !string.IsNullOrEmpty(info.Comment))
				{
					infoEx.Comment = info.Comment;
					changed = true;
				}
				if (string.IsNullOrEmpty(infoEx.Participants) && !string.IsNullOrEmpty(info.Participants))
				{
					infoEx.Participants = info.Participants;
					changed = true;
				}
				if (changed)
				{
					ValidateInput(infoEx);
					if (infoEx.CardStyle == CardStyle.Incomplete || infoEx.CardStyle == CardStyle.Normal)
					{
						infoEx.CardStyle = infoEx.IsAnyInvalid ? CardStyle.Incomplete : CardStyle.Normal;
						view.ModifyIntervalColor(infoEx.LeftSplitterIndex, infoEx.CardStyle);
					}
					infoEx.IsChanged = true;
				}
			}
			(info as MeetingInfoEx).IsChanged = true;
		}

		public void AddressbookSelected(MeetingInfo info, IntPtr handle)
		{
			if (info.IsWaitCursorOnAddressbook) return;
			info.IsWaitCursorOnAddressbook = true;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				if (!isAddressBookServiceInitialized)
				{
					addressBookService.Initialize();
					isAddressBookServiceInitialized = true;
				}

				var selectedAttendees = addressBookService.DisplaySelectNamesDialog(handle);
				var selectedAttendeesEmails = selectedAttendees != null ? String.Join("; ", selectedAttendees.Select(a => a.Email).ToArray()) : null;
				view.RunOnGui(() =>
				{
					if (!String.IsNullOrEmpty(selectedAttendeesEmails)) info.Participants += (String.IsNullOrEmpty(info.Participants) ? "" : "; ") + selectedAttendeesEmails;
					info.IsWaitCursorOnAddressbook = false;
				});
			}, null);
		}
	}

	public class MyNavigator : INavigator
	{
		public MyNavigator(MeetingInfo meeting)
		{
			Meeting = meeting;
		}

		public MeetingInfo Meeting { get; }
		public event EventHandler<SingleValueEventArgs<NavigationBase>> OnNavigate;
		public void Up()
		{
			throw new NotImplementedException();
		}

		public void Goto(LocationKey navigation, bool leaveTrail = true)
		{
			OnNavigate?.Invoke(this, null);
		}
	}

	public class MeetingInfoEx : MeetingInfo
	{
		private MeetingWorkTimeCounter counter;
		private DateTime startTime, endTime;
		private TimeSpan duration, nonAccountableDuration;
		public MeetingWorkTimeCounter Counter { get => counter; set => counter = value; }
		public DateTime StartTime { get => startTime; set => startTime = value; }
		public DateTime EndTime { get => counter != null ? counter.EndTime : endTime; set => endTime = value; }
		public TimeSpan Duration { get => counter != null ? counter.GetDuration() - duration : duration; set => duration = counter != null ? counter.GetDuration() - value : value; }
		public TimeSpan NonAccountableDuration { get => counter != null ? counter.GetPausedDuration() - nonAccountableDuration : nonAccountableDuration; set => nonAccountableDuration = counter != null ? counter.GetPausedDuration() - value : value; }
		public int LeftSplitterIndex { get; set; }
		public int RightSplitterIndex { get; set; }
		public bool IsChanged { get; internal set; }
		public MyNavigator Navigator { get; }

		public MeetingInfoEx()
		{
			Navigator = new MyNavigator(this);
		}
	}
}
