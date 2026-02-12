using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderService.Collector;
using Tct.ActivityRecorderService.EmailStats;

namespace Tct.ActivityRecorderService.OnlineStats
{
	/// <summary>
	/// Calcualtes work stats from DB data for a given interval. (used for calculating Today's stats)
	/// It has no idea about users, timezones, day change etc.
	/// </summary>
	public class OnlineTodaysWorkTimeBuilder //TodaysWorkTimeBuilder??
	{
		private readonly OnlineStatsManager onlineStatsManager;
		private readonly IntervalAggregator<WorkItemAggregateKey> workItemAggregator = new IntervalAggregator<WorkItemAggregateKey>();
		private readonly List<IManualWorkItem> manualWorkItems = new List<IManualWorkItem>();
		private readonly List<IMobileWorkItem> mobileWorkItems = new List<IMobileWorkItem>();
		private readonly Dictionary<int, ComputerActivity> todaysComputerActivity = new Dictionary<int, ComputerActivity>();
		private readonly Dictionary<long, MobileActivity> todaysMobileActivity = new Dictionary<long, MobileActivity>();
		private readonly StopwatchLite workItemClearSw = new StopwatchLite(TimeSpan.FromHours(1));
		private readonly ComputerStatusCalculator compStatusCaclulator;
		private static readonly ThreadLocal<StringCipher> cipher = new ThreadLocal<StringCipher>(() => { log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Debug("new StringCipher created for thread"); return new StringCipher(); });

		public int UserId { get; private set; }
		public UserStatInfo UserInfo { get; private set; }
		public IEnumerable<IComputerWorkItem> WorkItems
		{
			get
			{
				return workItemAggregator.GetIntervalsWithKeys()
					.SelectMany(n => n.Value, (par, val) => new ComputerWorkItem(par.Key.WorkId, val.StartDate, val.EndDate))
					.Cast<IComputerWorkItem>();
			}
		}
		public IEnumerable<IManualWorkItem> ManualWorkItems { get { return manualWorkItems; } }
		public IEnumerable<IMobileWorkItem> MobileWorkItems { get { return mobileWorkItems; } }

		public OnlineTodaysWorkTimeBuilder(UserStatInfo userStatInfo, OnlineStatsManager onlineStatsManager)
		{
			this.onlineStatsManager = onlineStatsManager;
			Debug.Assert(userStatInfo != null);
			UserId = userStatInfo.Id;
			UserInfo = userStatInfo;
			compStatusCaclulator = new ComputerStatusCalculator(UserId);
		}

		public void AddWorkItem(WorkItem item)
		{
			ClearOldDataIfApplicable();
			compStatusCaclulator.AddWorkItem(item);
			ComputerActivity currActivity;
			if (!todaysComputerActivity.TryGetValue(item.ComputerId, out currActivity))
			{
				currActivity = new ComputerActivity() { LastScreenShots = new List<ScreenShot>(), RecentActivityBuilder = new AggregateActivityBuilder() };
				todaysComputerActivity.Add(item.ComputerId, currActivity);
			}
			if (currActivity.LastSnapshot < item.EndDate) //if this is a new data (this heuristic is far from perfect but ok atm.)
			{
				currActivity.LastSnapshot = item.EndDate;
				currActivity.LastKeyboardActivity = item.KeyboardActivity;
				currActivity.LastMouseActivity = item.MouseActivity;
				currActivity.IsRemoteDesktop = item.IsRemoteDesktop;
				currActivity.IsVirtualMachine = item.IsVirtualMachine;
				currActivity.IPAddress = item.IPAddress;
				currActivity.LocalIPAddresses = item.LocalIPAddresses;
				if (item.ActiveWindows != null && item.ActiveWindows.Count != 0)
				{
					currActivity.LastActiveWindow = item.ActiveWindows[item.ActiveWindows.Count - 1];
				}
				if (item.ScreenShots != null && item.ScreenShots.Count != 0)
				{
					currActivity.LastScreenShots.Clear();
					if (item.ScreenShots.Count == 1) //fast path
					{
						currActivity.LastScreenShots.Add(item.ScreenShots[0]);
					}
					else if (item.ScreenShots.Count == 2) //fast path 2
					{
						if (item.ScreenShots[0].ScreenNumber != item.ScreenShots[1].ScreenNumber)
						{
							currActivity.LastScreenShots.AddRange(item.ScreenShots);
						}
						else
						{
							currActivity.LastScreenShots.Add(item.ScreenShots[1]);
						}
					}
					else //slow path
					{
						var screenShots = item.ScreenShots.ToLookup(n => n.ScreenNumber);
						foreach (var lookup in screenShots)
						{
							var lastShot = lookup.LastOrDefault(); //no sort for speed
							if (lastShot != null)
							{
								currActivity.LastScreenShots.Add(lastShot);
							}
						}
					}
				}
			}
			currActivity.RecentActivityBuilder.AddWorkItem(item); //we won't hold any reference to the workitem here
			//must calculate currActivity.RecentKeyboardActivityPerMinute and currActivity.RecentMouseActivityPerMinute
			//when the data is requested and we know the user's time

			//we cannot null out ScreenShots, ActiveWindows because someone might need these after we processed them
			//but creating and storing new workItems without those data is still too heavyweight
			/* WorkItems are quite heavy... at least if you have 480260 (so don't store them...)
			0337972c   480263     28815780 System.Data.Linq.EntitySet`1[[Tct.ActivityRecorderService.ScreenShot, ActivityRecorderService]]
			03379290   480263     28815780 System.Data.Linq.EntitySet`1[[Tct.ActivityRecorderService.ActiveWindow, ActivityRecorderService]]
			0506de6c   960520     30736640 System.Action`1[[Tct.ActivityRecorderService.ScreenShot, ActivityRecorderService]]
			0506dd8c   960520     30736640 System.Action`1[[Tct.ActivityRecorderService.ActiveWindow, ActivityRecorderService]]
			03330c1c   480260     49947040 Tct.ActivityRecorderService.WorkItem
			79ba4944   646110     83763076 System.Byte[] */
			workItemAggregator.Add(new WorkItemAggregateKey(item.WorkId, item.ComputerId, item.PhaseId), item.StartDate, item.EndDate); //todo opt mem this can take up to 1 MB of memory per user
		}

		public void StartComputerWork(int workId, int computerId, DateTime createDate, DateTime userTime, DateTime serverTime)
		{
			compStatusCaclulator.StartComputerWork(workId, computerId, createDate, userTime, serverTime);
		}

		public void StopComputerWork(int computerId, DateTime createDate, DateTime userTime, DateTime serverTime)
		{
			compStatusCaclulator.StopComputerWork(computerId, createDate, userTime, serverTime);
		}

		public void RefreshAggregateWorkItemIntervals(IEnumerable<AggregateWorkItemIntervalCovered> items)
		{
			var aggrCorItems = items.Select(i => new Tuple<WorkItemAggregateKey, StartEndDateTime>(new WorkItemAggregateKey(i.WorkId, i.ComputerId, i.PhaseId), new StartEndDateTime(i.StartDate, i.EndDate))).ToList();
			workItemAggregator.Refresh(aggrCorItems);
		}

		public void RefreshManualWorkItems(IEnumerable<IManualWorkItem> items)
		{
			manualWorkItems.Clear();
			foreach (var manualWorkItem in items)
			{
				manualWorkItems.Add(manualWorkItem);
			}
		}

		public void RefreshMobileWorkItems(IEnumerable<IMobileWorkItem> items)
		{
			mobileWorkItems.Clear();
			foreach (var mobileWorkItem in items)
			{
				mobileWorkItems.Add(mobileWorkItem);
			}
		}

		public void RefreshTodaysMobileActivity(IEnumerable<MobileLocationInfo> locations, IEnumerable<MobileActivityInfo> activities) //we have locations for larger interval than the current day
		{
			foreach (var mobileLocationInfoByImei in locations.OrderByDescending(n => n.CreateDate).ToLookup(n => n.Imei))
			{
				MobileActivity curr;
				if (!todaysMobileActivity.TryGetValue(mobileLocationInfoByImei.Key, out curr))
				{
					curr = new MobileActivity();
					todaysMobileActivity.Add(mobileLocationInfoByImei.Key, curr);
				}
				if (curr.Locations == null)
				{
					curr.Locations = new List<LocationInfo>();
				}
				else
				{
					curr.Locations.Clear();
				}
				curr.Locations.AddRange(mobileLocationInfoByImei.Select(n =>
						new LocationInfo()
						{
							Latitude = n.Latitude ?? double.Parse(cipher.Value.Decrypt(n.LatitudeEncrypted), CultureInfo.InvariantCulture),
							Longitude = n.Longitude ?? double.Parse(cipher.Value.Decrypt(n.LongitudeEncrypted), CultureInfo.InvariantCulture),
							Accuracy = n.Accuracy,
							CreateDate = n.CreateDate,
						}));
			}
			foreach (var mobileActivityInfoByImei in activities.ToLookup(n => n.Imei))
			{
				MobileActivity curr;
				if (!todaysMobileActivity.TryGetValue(mobileActivityInfoByImei.Key, out curr))
				{
					curr = new MobileActivity();
					todaysMobileActivity.Add(mobileActivityInfoByImei.Key, curr);
				}
				curr.RecentActivityBuilder.RefreshMobileActivityInfo(mobileActivityInfoByImei);
			}
		}

		//drop old ever growing data to avoid memory leak
		private void ClearOldDataIfApplicable()
		{
			if (!workItemClearSw.IsIntervalElapsedSinceLastCheck()) return;
			var minTime = DateTime.UtcNow.AddDays(-1).AddHours(-1); //we need data only for today
			workItemAggregator.TruncateIntervalsBefore(minTime); //drop old aggregated data
			foreach (var computerActivity in todaysComputerActivity) //we must also clear recent activities
			{
				computerActivity.Value.RecentActivityBuilder.ClearDataBefore(minTime);
			}
			//todo mobile data leaks technically.... (todaysMobileActivity: Location, Activity etc.) prevent it.
		}

		public void UpdateTodaysStatsInDetailedUserStats(DetailedUserStats statsToUpdate, DateTime startDate, DateTime endDate)
		{
			ClearOldDataIfApplicable();
			var currManualWorkItems = manualWorkItems
				.Where(n => startDate < n.EndDate)
				.Where(n => n.StartDate < endDate);
			var currMobileWorkItems = mobileWorkItems
				.Where(n => startDate < n.EndDate)
				.Where(n => n.StartDate < endDate);

			statsToUpdate.ComputerStatsByCompId = new Dictionary<int, DetailedComputerStats>();
			statsToUpdate.MobileStatsByMobileId = new Dictionary<long, DetailedMobileStats>();
			statsToUpdate.ManuallyAddedStats = new DetailedIntervalStats() { TodaysWorkIntervalsByWorkId = new Dictionary<int, WorkWithIntervals>(), };
			statsToUpdate.HolidayStats = new DetailedIntervalStats() { TodaysWorkIntervalsByWorkId = new Dictionary<int, WorkWithIntervals>(), };
			statsToUpdate.SickLeaveStats = new DetailedIntervalStats() { TodaysWorkIntervalsByWorkId = new Dictionary<int, WorkWithIntervals>(), };
			statsToUpdate.TodaysWorksByWorkId = new Dictionary<int, BriefWorkStats>();
			statsToUpdate.TodaysWorkTime = new BriefNetWorkTimeStats();
			statsToUpdate.OnlineComputers = new List<int>();
			statsToUpdate.IPAddresses = new List<string>();
			statsToUpdate.LocalIPAddresses = new List<string>();
			statsToUpdate.TodaysStartDate = null;
			statsToUpdate.TodaysEndDate = null;

			var inverseQueryInterval = new IntervalConcatenator();
			inverseQueryInterval.Add(DateTime.MinValue, startDate);
			inverseQueryInterval.Add(endDate, DateTime.MaxValue);

			var comIntervals = new IntervalConcatenator();
			var mobIntervals = new IntervalConcatenator();

			//we don't handle deletion by WorkId atm.
			var comCorrIntervals = new IntervalConcatenator();
			var ivrCorrIntervals = new IntervalConcatenator();
			var mobCorrIntervals = new IntervalConcatenator();
			foreach (var manualWorkItem in currManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteComputerInterval))
			{
				comCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
			}
			foreach (var manualWorkItem in currManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteIvrInterval))
			{
				ivrCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
			}
			foreach (var manualWorkItem in currManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteMobileInterval))
			{
				mobCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
			}
			foreach (var manualWorkItem in currManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteInterval))
			{
				comCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
				ivrCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
				mobCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
			}
			//we don't care about dates outside of the query interval
			comCorrIntervals.Subtract(inverseQueryInterval);
			ivrCorrIntervals.Subtract(inverseQueryInterval);
			mobCorrIntervals.Subtract(inverseQueryInterval);

			foreach (var mobileWorkItem in currMobileWorkItems)
			{
				var startEndDate = GetIntersectStartEndDateTime(mobileWorkItem.StartDate, mobileWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				var workIntervals = new IntervalConcatenator();
				workIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);
				workIntervals.Subtract(mobCorrIntervals);

				UpdateIntervalStats(statsToUpdate, WorkType.Mobile, mobileWorkItem.WorkId, workIntervals, null, mobileWorkItem.Imei, mobileWorkItem.IsBeacon? IntervalManualSubType.Beacon : IntervalManualSubType.Mobile);
				mobIntervals.Merge(workIntervals);
			}

			foreach (var workItemAggr in workItemAggregator.GetIntervalsWithKeys()) //don't recalculate computer stats every time ??? this is soo fast... ~0.05ms
			{
				foreach (var startEndDateTime in workItemAggr.Value)
				{
					var startEndDate = GetIntersectStartEndDateTime(startEndDateTime.StartDate, startEndDateTime.EndDate, startDate, endDate);
					if (!startEndDate.HasValue) continue;
					var workIntervals = new IntervalConcatenator();
					workIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);
					workIntervals.Subtract(comCorrIntervals);

					UpdateIntervalStats(statsToUpdate, WorkType.Computer, workItemAggr.Key.WorkId, workIntervals, workItemAggr.Key.ComputerId);
					comIntervals.Merge(workIntervals);
				}
			}

			var sumIntervals = comIntervals.Clone();
			sumIntervals.Merge(mobIntervals);

			foreach (var manualWorkItem in currManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddWork))
			{
				var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				var workIntervals = new IntervalConcatenator();
				workIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);

				var isMeeting = manualWorkItem.SourceId.HasValue
					&& (manualWorkItem.SourceId.Value == (byte)ManualWorkItemSourceEnum.MeetingAdd
						|| manualWorkItem.SourceId.Value == (byte)ManualWorkItemSourceEnum.ServerAdhocMeeting
						);
				UpdateIntervalStats(statsToUpdate, WorkType.ManuallyAdded, manualWorkItem.WorkId ?? -1, workIntervals, subType: isMeeting? IntervalManualSubType.Meeting : IntervalManualSubType.ManualWorkItem, comment: manualWorkItem.Comment);
				sumIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);
			}

			foreach (var manualWorkItem in currManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddHoliday))
			{
				var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				var workIntervals = new IntervalConcatenator();
				workIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);

				UpdateIntervalStats(statsToUpdate, WorkType.Holiday, manualWorkItem.WorkId ?? -1, workIntervals);
			}

			foreach (var manualWorkItem in currManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddSickLeave))
			{
				var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				var workIntervals = new IntervalConcatenator();
				workIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);

				UpdateIntervalStats(statsToUpdate, WorkType.SickLeave, manualWorkItem.WorkId ?? -1, workIntervals);
			}

			sumIntervals.Subtract(inverseQueryInterval);
			comIntervals.Subtract(inverseQueryInterval);
			mobIntervals.Subtract(inverseQueryInterval);

			statsToUpdate.TodaysWorkTime.NetWorkTime = sumIntervals.Duration()
				+ statsToUpdate.TodaysWorkTime.HolidayTime
				+ statsToUpdate.TodaysWorkTime.SickLeaveTime;

			var realWorkStartEnd = sumIntervals.GetBoundaries(); //don't calculate holidays and sickleaves into this
			if (realWorkStartEnd.HasValue)
			{
				statsToUpdate.TodaysStartDate = realWorkStartEnd.Value.StartDate;
				statsToUpdate.TodaysEndDate = realWorkStartEnd.Value.EndDate;
			}

			//set CurrentWorks and Status 
			OnlineStatusAndCurrentWorksHelper.UpdateDetailedUserStatsFromIntervals(statsToUpdate, compStatusCaclulator, onlineStatsManager.MobileStatusManager, UserInfo);
			if (statsToUpdate.Status != OnlineStatus.Offline)
			{
				//TodaysEndDate can be in the future because of manual work items, so we have to null it out if the user is online...
				statsToUpdate.TodaysEndDate = null;
				//handle new works from statusCaclulator where StartComputerWork received but no workitems yet
				//also handle this kind of scenario for mobile works when user is working on a work which is not in MobileWorkItems yet
				foreach (var missingWork in statsToUpdate.CurrentWorks
					.Where(n => !statsToUpdate.TodaysWorksByWorkId.ContainsKey(n.WorkId)))
				{
					statsToUpdate.TodaysWorksByWorkId.Add(missingWork.WorkId, new BriefWorkStats() { WorkId = missingWork.WorkId, WorkTimeStats = new BriefWorkTimeStats() });
				}
			}
			//set RecentComputerActivity
			foreach (var compStats in statsToUpdate.ComputerStatsByCompId.Values)
			{
				if (compStats.IsOnline)
				{
					statsToUpdate.OnlineComputers.Add(compStats.ComputerId);
				}
				ComputerActivity currStats;
				if (!todaysComputerActivity.TryGetValue(compStats.ComputerId, out currStats))
				{
					//logerror ?
					continue;
				}
				compStats.RecentComputerActivity = currStats.Clone();
				//set recent activities
				var userTime = compStatusCaclulator.GetUserTime(compStats.ComputerId);
				var userTimeMin = new DateTime(userTime.Year, userTime.Month, userTime.Day, userTime.Hour, userTime.Minute, 00);
				userTimeMin = userTimeMin == userTime ? userTimeMin : userTimeMin.AddMinutes(1); //include the current minute so we can see the change 'online'
				var aggrActivities = currStats.RecentActivityBuilder.GetMinutelyAggregatedActivity(userTimeMin.AddMinutes(-21), userTimeMin); //return the last 20+1 mins
				compStats.RecentComputerActivity.RecentKeyboardActivityPerMinute = aggrActivities.Select(n => n.KeyboardActivity).Reverse().ToList();
				compStats.RecentComputerActivity.RecentMouseActivityPerMinute = aggrActivities.Select(n => n.MouseActivity).Reverse().ToList();
				//set quick info
				if (!compStats.IsOnline) continue; //we only set these for online computers
				statsToUpdate.HasComputerActivity |= compStats.RecentComputerActivity.RecentKeyboardActivityPerMinute.Take(UserInfo.LowestLevelOfInactivityInMins).Any(n => n != 0) || compStats.RecentComputerActivity.RecentMouseActivityPerMinute.Take(UserInfo.LowestLevelOfInactivityInMins).Any(n => n != 0); //if we have activity in the last 4+1 mins
				statsToUpdate.HasRemoteDesktop |= currStats.IsRemoteDesktop;
				statsToUpdate.HasVirtualMachine |= currStats.IsVirtualMachine;
				if (currStats.IPAddress != null) statsToUpdate.IPAddresses.Add(currStats.IPAddress); //we don't have ip for data loaded from DB also we don't update IPAddress from StartComputerWork which is not ideal but ok atm. (so we might be online without knowing the IPAddress)
				if (currStats.LocalIPAddresses != null) statsToUpdate.LocalIPAddresses = new HashSet<string>(statsToUpdate.LocalIPAddresses.Concat(currStats.LocalIPAddresses)).ToList();
			}
			//set RecentMobileActivity (with applicable locations and activities)
			foreach (var mobileActivity in todaysMobileActivity)
			{
				DetailedMobileStats detailedMobile;
				if (!statsToUpdate.MobileStatsByMobileId.TryGetValue(mobileActivity.Key, out detailedMobile))
				{
					//we can have locations without worktimes
					detailedMobile = new DetailedMobileStats() { MobileId = mobileActivity.Key, TodaysWorkIntervalsByWorkId = new Dictionary<int, WorkWithIntervals>(), };
					statsToUpdate.MobileStatsByMobileId.Add(mobileActivity.Key, detailedMobile);
				}
				if (detailedMobile.RecentMobileActivity == null) detailedMobile.RecentMobileActivity = new MobileActivity();
				detailedMobile.RecentMobileActivity.Locations = mobileActivity.Value.Locations == null ? new List<LocationInfo>(0) : mobileActivity.Value.Locations.SkipWhile(n => n.CreateDate >= endDate).TakeWhile(n => n.CreateDate >= startDate).ToList();
				var now = DateTime.UtcNow;
				detailedMobile.RecentMobileActivity.RecentActivityPerMinute = mobileActivity.Value.RecentActivityBuilder.GetMinutelyAggregatedActivityReversed(now.AddMinutes(-21), now);
			}
		}

		private static void UpdateIntervalStats(DetailedUserStats detailedUserStats, WorkType workType, int workId, IntervalConcatenator newIntervals, int? computerId = null, long? mobileId = null, IntervalManualSubType subType = IntervalManualSubType.ManualWorkItem, string comment = null)
		{
			Debug.Assert(detailedUserStats != null);
			var workDuration = newIntervals.Duration();
			var workIntervals = newIntervals.GetIntervals();
			if (workIntervals.Count == 0) return;
			switch (workType)
			{
				case WorkType.Computer:
					Debug.Assert(computerId.HasValue);
					DetailedComputerStats detailedComputerStats;
					if (!detailedUserStats.ComputerStatsByCompId.TryGetValue(computerId.Value, out detailedComputerStats))
					{
						detailedComputerStats = new DetailedComputerStats() { ComputerId = computerId.Value, TodaysWorkIntervalsByWorkId = new Dictionary<int, WorkWithIntervals>(), };
						detailedUserStats.ComputerStatsByCompId.Add(computerId.Value, detailedComputerStats);
					}
					UpdateWorkWithIntervalsDict(detailedComputerStats.TodaysWorkIntervalsByWorkId, workId, workIntervals);
					detailedUserStats.TodaysWorkTime.ComputerWorkTime += workDuration;
					break;
				case WorkType.Mobile:
					Debug.Assert(mobileId.HasValue);
					DetailedMobileStats detailedMobileStats;
					if (!detailedUserStats.MobileStatsByMobileId.TryGetValue(mobileId.Value, out detailedMobileStats))
					{
						detailedMobileStats = new DetailedMobileStats() { MobileId = mobileId.Value, TodaysWorkIntervalsByWorkId = new Dictionary<int, WorkWithIntervals>(), };
						detailedUserStats.MobileStatsByMobileId.Add(mobileId.Value, detailedMobileStats);
					}
					UpdateWorkWithIntervalsDict(detailedMobileStats.TodaysWorkIntervalsByWorkId, workId, workIntervals, subType);
					detailedUserStats.TodaysWorkTime.MobileWorkTime += workDuration;
					break;
				case WorkType.Ivr:
					log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Warn($"WorkType.Ivr obsolete workId: {workId}");
					break;
				case WorkType.ManuallyAdded:
					UpdateWorkWithIntervalsDict(detailedUserStats.ManuallyAddedStats.TodaysWorkIntervalsByWorkId, workId, workIntervals, subType, comment);
					detailedUserStats.TodaysWorkTime.ManuallyAddedWorkTime += workDuration;
					break;
				case WorkType.Holiday:
					UpdateWorkWithIntervalsDict(detailedUserStats.HolidayStats.TodaysWorkIntervalsByWorkId, workId, workIntervals);
					detailedUserStats.TodaysWorkTime.HolidayTime += workDuration;
					break;
				case WorkType.SickLeave:
					UpdateWorkWithIntervalsDict(detailedUserStats.SickLeaveStats.TodaysWorkIntervalsByWorkId, workId, workIntervals);
					detailedUserStats.TodaysWorkTime.SickLeaveTime += workDuration;
					break;
				default:
					throw new ArgumentOutOfRangeException("workType");
			}
			UpdateWorkTimesDict(detailedUserStats.TodaysWorksByWorkId, workType, workId, workDuration);
		}

		private static void UpdateWorkTimesDict(Dictionary<int, BriefWorkStats> worksByWorkId, WorkType workType, int workId, TimeSpan workDuration)
		{
			BriefWorkStats briefWorkStats;
			if (!worksByWorkId.TryGetValue(workId, out briefWorkStats))
			{
				briefWorkStats = new BriefWorkStats() { WorkId = workId, WorkTimeStats = new BriefWorkTimeStats(), };
				worksByWorkId.Add(workId, briefWorkStats);
			}
			switch (workType)
			{
				case WorkType.Computer:
					briefWorkStats.WorkTimeStats.ComputerWorkTime += workDuration;
					break;
				case WorkType.Ivr:
					log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Warn($"WorkType.Ivr obsolete workId: {workId}");
					break;
				case WorkType.ManuallyAdded:
					briefWorkStats.WorkTimeStats.ManuallyAddedWorkTime += workDuration;
					break;
				case WorkType.Holiday:
					briefWorkStats.WorkTimeStats.HolidayTime += workDuration;
					break;
				case WorkType.SickLeave:
					briefWorkStats.WorkTimeStats.SickLeaveTime += workDuration;
					break;
				case WorkType.Mobile:
					briefWorkStats.WorkTimeStats.MobileWorkTime += workDuration;
					break;
				default:
					throw new ArgumentOutOfRangeException("workType");
			}
		}

		private static void UpdateWorkWithIntervalsDict(Dictionary<int, WorkWithIntervals> workIntervalsByWorkId, int workId, List<IntervalConcatenator.Interval> newIntervals, IntervalManualSubType subType = IntervalManualSubType.ManualWorkItem, string comment = null)
		{
			WorkWithIntervals workWithIntervals;
			if (!workIntervalsByWorkId.TryGetValue(workId, out workWithIntervals))
			{
				workWithIntervals = new WorkWithIntervals() { WorkId = workId, Intervals = new List<Interval>(), };
				workIntervalsByWorkId.Add(workId, workWithIntervals);
			}
			workWithIntervals.Intervals.AddRange(newIntervals.Select(n => new Interval() { StartDate = n.StartDate, EndDate = n.EndDate, SubType = subType, Comment = comment }));
		}

		private static StartEndDateTime? GetIntersectStartEndDateTime(DateTime firstStart, DateTime firstEnd, DateTime secondStart, DateTime secondEnd)
		{
			if (firstEnd < firstStart || secondEnd < secondStart) return null; //invalid intervals
			var result = new StartEndDateTime(
				firstStart < secondStart ? secondStart : firstStart, //MAX(firstStart, secondStart)
				secondEnd < firstEnd ? secondEnd : firstEnd);        //MIN(secondEnd, firstEnd)
			return result.EndDate < result.StartDate ? new StartEndDateTime?() : result;
		}

		private class ComputerWorkItem : IComputerWorkItem
		{
			private readonly int workId;
			private readonly DateTime startDate;
			private readonly DateTime endDate;

			public ComputerWorkItem(int workId, DateTime startDate, DateTime endDate)
			{
				this.workId = workId;
				this.startDate = startDate;
				this.endDate = endDate;
			}

			public int WorkId { get { return workId; } }
			public DateTime StartDate { get { return startDate; } }
			public DateTime EndDate { get { return endDate; } }
		}

		private struct WorkItemAggregateKey : IEquatable<WorkItemAggregateKey>
		{
			public readonly int WorkId;
			public readonly int ComputerId;
			public readonly Guid PhaseId;

			public WorkItemAggregateKey(int workId, int computerId, Guid phaseId)
			{
				WorkId = workId;
				ComputerId = computerId;
				PhaseId = phaseId;
			}

			public override int GetHashCode()
			{
				int result = 17;
				result = 31 * result + WorkId.GetHashCode();
				result = 31 * result + ComputerId.GetHashCode();
				result = 31 * result + PhaseId.GetHashCode();
				return result;
			}

			public bool Equals(WorkItemAggregateKey other)
			{
				return WorkId == other.WorkId
					&& ComputerId == other.ComputerId
					&& PhaseId == other.PhaseId;
			}

			public override bool Equals(object obj)
			{
				if (Object.ReferenceEquals(obj, null))
					return false;

				if (obj is WorkItemAggregateKey)
				{
					return this.Equals((WorkItemAggregateKey)obj);
				}
				return false;
			}
		}
	}
}
