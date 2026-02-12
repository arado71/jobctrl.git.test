using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService.OnlineStats
{
	//distributes data on per user basis in a thread-safe manner
	//preiodically refreshes updatable workitems
	//handles day change
	//removes unused user data
	public class OnlineStatsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly TimeSpan IdleAfter = TimeSpan.FromSeconds(ConfigManager.IdleAfterInSec);
		public static readonly TimeSpan TimedOutAfter = TimeSpan.FromSeconds(ConfigManager.TimedOutAfterInSec);
		public static readonly TimeSpan ActivityAverageInterval = TimeSpan.FromSeconds(ConfigManager.ActivityAverageIntervalInSec);

		private readonly CalendarManager calendarManager;
		public readonly MobileStatusManager MobileStatusManager = new MobileStatusManager();
		private readonly ConcurrentDictionary<int, ManagedOnlineUserStats> userStats = new ConcurrentDictionary<int, ManagedOnlineUserStats>();

		private int lastAggrUpdate = Environment.TickCount - ConfigManager.OnlineAggrDataUpdateInterval - 1;
		private StartEndDateTime lastAggrInterval = new StartEndDateTime(DateTime.MinValue, DateTime.MinValue);

		public OnlineStatsManager(CalendarManager calendarManager)
			: base(log)
		{
			this.calendarManager = calendarManager;
			ManagerCallbackInterval = ConfigManager.OnlineDataUpdateInterval;
			Debug.Assert(ConfigManager.OnlineDataUpdateInterval < TimeSpan.FromDays(1).TotalMilliseconds);
			try
			{
				UpdateUserStats();
			}
			catch (Exception ex) //an error here should not prevent the service from starting
			{
				log.Error("Swallowing unexpected error in ctor", ex);
			}
		}

		public override void Start()
		{
			base.Start();
			MobileStatusManager.Start();
		}

		public override void Stop()
		{
			MobileStatusManager.Stop();
			base.Stop();
		}

		public BriefUserStats GetBriefUserStats(int userId)
		{
			var detailed = GetDetailedUserStats(userId);
			if (detailed == null) return null;
			//todo research if there is a better way for this
			//drop data not needed
			return new BriefUserStats()
			{
				CurrentWorks = detailed.CurrentWorks,
				Status = detailed.Status,
				ThisMonthsWorkTime = detailed.ThisMonthsWorkTime,
				ThisWeeksWorkTime = detailed.ThisWeeksWorkTime,
				TodaysEndDate = detailed.TodaysEndDate,
				TodaysStartDate = detailed.TodaysStartDate,
				TodaysWorksByWorkId = detailed.TodaysWorksByWorkId,
				TodaysWorkTime = detailed.TodaysWorkTime,
				UserId = detailed.UserId,
				UserName = detailed.UserName,
				UserTimeZoneString = detailed.UserTimeZoneString,
				OnlineComputers = detailed.OnlineComputers,
				TodaysTargetNetWorkTime = detailed.TodaysTargetNetWorkTime,
				ThisWeeksTargetNetWorkTime = detailed.ThisWeeksTargetNetWorkTime,
				ThisMonthsTargetNetWorkTime = detailed.ThisMonthsTargetNetWorkTime,
				ThisWeeksTargetUntilTodayNetWorkTime = detailed.ThisWeeksTargetUntilTodayNetWorkTime,
				ThisMonthsTargetUntilTodayNetWorkTime = detailed.ThisMonthsTargetUntilTodayNetWorkTime,
				HasComputerActivity = detailed.HasComputerActivity,
				HasRemoteDesktop = detailed.HasRemoteDesktop,
				HasVirtualMachine = detailed.HasVirtualMachine,
				IPAddresses = detailed.IPAddresses,
				LocalIPAddresses = detailed.LocalIPAddresses,
				BatteryPercent = detailed.BatteryPercent,
				ConnectionType = detailed.ConnectionType,
			};
		}

		public DetailedUserStats GetDetailedUserStats(int userId)
		{
			ManagedOnlineUserStats stats;
			if (!userStats.TryGetValue(userId, out stats))
			{
				return null;
			}
			lock (stats.ThisLock)
			{
				return stats.GetDetailedUserStatsOrCached();
			}
		}

		public SimpleWorkTimeStats GetSimpleWorkTimeStatsFromOM(int userId, DateTime desiredEndDate)
		{
			ManagedOnlineUserStats stats;
			if (!userStats.TryGetValue(userId, out stats))
			{
				return null;
			}
			lock (stats.ThisLock)
			{
				return stats.StatsBuilder.GetSimpleWorkTimeStatsFromOM(desiredEndDate);
			}
		}

		//todo AddManualWorkItem ?

		public void AddWorkItem(WorkItem item)
		{
			ManagedOnlineUserStats stats;
			if (!userStats.TryGetValue(item.UserId, out stats))
			{
				log.Warn("Cannot add workitem for user " + item.UserId);
				return;
			}
			lock (stats.ThisLock)
			{
				stats.StatsBuilder.AddWorkItem(item);
			}
		}

		public void StartComputerWork(int userId, int workId, int computerId, DateTime createDate, DateTime userTime, DateTime serverTime)
		{
			ManagedOnlineUserStats stats;
			if (!userStats.TryGetValue(userId, out stats))
			{
				log.Warn("Cannot StartComputerWork for user " + userId);
				return;
			}
			lock (stats.ThisLock)
			{
				stats.StatsBuilder.StartComputerWork(workId, computerId, createDate, userTime, serverTime);
			}
		}

		public void StopComputerWork(int userId, int computerId, DateTime createDate, DateTime userTime, DateTime serverTime)
		{
			ManagedOnlineUserStats stats;
			if (!userStats.TryGetValue(userId, out stats))
			{
				log.Warn("Cannot StopComputerWork for user " + userId);
				return;
			}
			lock (stats.ThisLock)
			{
				stats.StatsBuilder.StopComputerWork(computerId, createDate, userTime, serverTime);
			}
		}


		public List<DateTime> GetUserWorkdays(int userId)
		{
			ManagedOnlineUserStats stats;
			if (!userStats.TryGetValue(userId, out stats))
			{
				return null;
			}
			lock (stats.ThisLock)
			{
				return stats.StatsBuilder.GetUserWorkdays();
			}
		}

		private class ManagedOnlineUserStats
		{
			public OnlineUserStatsBuilder StatsBuilder { get; set; }
			public readonly object ThisLock = new object(); //protects access of StatsBuilder and the cache

			private static readonly int cacheAge = (int)TimeSpan.FromSeconds(ConfigManager.OnlineDetailedStatsCacheAgeInSec).TotalMilliseconds; //ms
			private int lastUpdate = Environment.TickCount - cacheAge - 1;
			private DetailedUserStats cachedDetailedUserStats;
			public DetailedUserStats GetDetailedUserStatsOrCached()
			{
				lock (ThisLock)
				{
					if ((uint)(Environment.TickCount - lastUpdate) > cacheAge)
					{
						cachedDetailedUserStats = StatsBuilder.GetDetailedUserStats();
						lastUpdate = Environment.TickCount;
					}
					return cachedDetailedUserStats;
				}
			}
		}

		private void GetCurrentAndNewUserStats(out List<ManagedOnlineUserStats> currentStats, out List<ManagedOnlineUserStats> newStats)
		{
			var availableUserStats = StatsDbHelper.GetUserStatsInfo(null);
			var userStatsSnapshot = userStats.Values.ToDictionary(n => n.StatsBuilder.UserId);
			newStats = new List<ManagedOnlineUserStats>();
			foreach (var availStats in availableUserStats)
			{
				ManagedOnlineUserStats currStats;
				if (!userStatsSnapshot.TryGetValue(availStats.Id, out currStats))
				{
					newStats.Add(new ManagedOnlineUserStats() { StatsBuilder = new OnlineUserStatsBuilder(availStats, this, calendarManager), });
				}
				else //if we found it check if the statInfo changed
				{
					bool isStatChanged = false;
					lock (currStats.ThisLock)
					{
						if (currStats.StatsBuilder.UserInfo.StartOfDayOffset != availStats.StartOfDayOffset ||
							!currStats.StatsBuilder.UserInfo.TimeZone.Equals(availStats.TimeZone) ||
							currStats.StatsBuilder.UserInfo.FirstWorkDay != availStats.FirstWorkDay ||
							currStats.StatsBuilder.UserInfo.LowestLevelOfInactivityInMins != availStats.LowestLevelOfInactivityInMins ||
							currStats.StatsBuilder.UserInfo.TargetWorkTimeIntervals != null && availStats.TargetWorkTimeIntervals != null && 
								!currStats.StatsBuilder.UserInfo.TargetWorkTimeIntervals.SequenceEqual(availStats.TargetWorkTimeIntervals) || 
							currStats.StatsBuilder.UserInfo.TargetWorkTimeIntervals != null && availStats.TargetWorkTimeIntervals == null || 
							currStats.StatsBuilder.UserInfo.TargetWorkTimeIntervals == null && availStats.TargetWorkTimeIntervals != null)
						//if the date ranges are changed then recalculate everything
						{
							isStatChanged = true;
						}
						else
						{
							//refresh other relevant properies where recalcualtion is not required
							if (currStats.StatsBuilder.UserInfo.Name != availStats.Name) //only the name is changed (probably YAGNI)
							{
								currStats.StatsBuilder.UserInfo.Name = availStats.Name;
							}
							currStats.StatsBuilder.UserInfo.FirstName = availStats.FirstName;
							currStats.StatsBuilder.UserInfo.LastName = availStats.LastName;
							currStats.StatsBuilder.UserInfo.CalendarId = availStats.CalendarId;
							currStats.StatsBuilder.UserInfo.TargetWorkTime = availStats.TargetWorkTime;
						}
					}
					if (isStatChanged)
					{
						ManagedOnlineUserStats oldVal;
						userStats.TryRemove(availStats.Id, out oldVal);
						userStatsSnapshot.Remove(availStats.Id);
						newStats.Add(new ManagedOnlineUserStats() { StatsBuilder = new OnlineUserStatsBuilder(availStats, this, calendarManager), });
					}
				}
			}
			currentStats = userStatsSnapshot.Values.ToList();
		}

		//it used to be cheaper to update all users with one roundtrip to the DB (then loading data on demand) becase there was no index on userId - there is an index now (don't know what is cheaper now)
		private void UpdateUserStats() //takes ~5 sec in 2012 Oct (~300 users) ~10 sec (9 sec for just aggrwis) at the end of the month and ~500 ms at the start of the month in 2014 Feb (~800 users)
		{
			var sw = Stopwatch.StartNew();
			List<ManagedOnlineUserStats> currentStats, newStats;
			GetCurrentAndNewUserStats(out currentStats, out newStats);
			var allStats = currentStats.Concat(newStats);

			var now = DateTime.UtcNow;
			var queryIntervals = allStats.Aggregate(
				new QueryIntervals(new StartEndDateTime(now, now), new StartEndDateTime(now, now), new StartEndDateTime(now.Date, now.Date)),
				(res, item) =>
				{
					lock (item.ThisLock)
					{
						var intervals = item.StatsBuilder.GetReportIntervals(now);
						var otherMinStartDate = intervals.ThisMonth.StartDate < intervals.ThisWeek.StartDate ? intervals.ThisMonth.StartDate : intervals.ThisWeek.StartDate;
						var calMinStartDay = intervals.ThisMonthLocalDay.StartDate < intervals.ThisWeekLocalDay.StartDate ? intervals.ThisMonthLocalDay.StartDate : intervals.ThisWeekLocalDay.StartDate;
						var calMaxEndDay = intervals.ThisMonthLocalDay.EndDate > intervals.ThisWeekLocalDay.EndDate ? intervals.ThisMonthLocalDay.EndDate : intervals.ThisWeekLocalDay.EndDate;
						return new QueryIntervals(
							new StartEndDateTime(
								res.WorkItems.StartDate < intervals.Today.StartDate ? res.WorkItems.StartDate : intervals.Today.StartDate
								, res.WorkItems.EndDate > intervals.Today.EndDate ? res.WorkItems.EndDate : intervals.Today.EndDate
							),
							new StartEndDateTime(
								res.OtherItems.StartDate < otherMinStartDate ? res.OtherItems.StartDate : otherMinStartDate
								, res.OtherItems.EndDate > intervals.Today.EndDate ? res.OtherItems.EndDate : intervals.Today.EndDate
							),
							new StartEndDateTime(
								res.CalendarDays.StartDate < calMinStartDay ? res.CalendarDays.StartDate : calMinStartDay
								, res.CalendarDays.EndDate > calMaxEndDay ? res.CalendarDays.EndDate : calMaxEndDay
							)
						);
					}
				});

			//there are a lot of aggregate items and we don't expect to have a lot of offline workitems so don't update them all the time (Reduce load on DB and GC)
			var isAggrUpdateRequired = !lastAggrInterval.Equals(queryIntervals.OtherItems) || (uint)(Environment.TickCount - lastAggrUpdate) > ConfigManager.OnlineAggrDataUpdateInterval;

			var intervalItems = isAggrUpdateRequired
				? StatsDbHelper.GetAggregateWorkItemIntervalsByUserCovered(queryIntervals.OtherItems.StartDate, queryIntervals.OtherItems.EndDate)  //todo opt mem this can be up to 3 MB of memory per user
				: null;
			var manualItems = StatsDbHelper.GetManualWorkItemsByUserCovered(queryIntervals.OtherItems.StartDate, queryIntervals.OtherItems.EndDate);
			var mobileItems = StatsDbHelper.GetMobileWorkItemsByUserCovered(queryIntervals.OtherItems.StartDate, queryIntervals.OtherItems.EndDate);
			var mobileLocations = StatsDbHelper.GetMobileLocationInfoByUser(queryIntervals.WorkItems.StartDate, queryIntervals.WorkItems.EndDate);
			var mobileActivity = StatsDbHelper.GetMobileActivityInfoByUser(queryIntervals.WorkItems.StartDate, queryIntervals.WorkItems.EndDate);

			if (newStats.Count != 0)
			{
				//workitems only needed for new users
				//todo are we sure that we won't have duplicate workitems ? (added online and read from db)
				//gettig all workitems is 1-3sec for all users... is not that expensive, but not ideal most of the time
				//so get workitems for new users only thus reducing GC pressure (unless we have more new users than current e.g. service start)
				//(not sure about the DB side of individual gets as we have no proper index for this, but we don't expect many new users per minute)
				var allWorkItems = newStats.Count > currentStats.Count
					? StatsDbHelper.GetWorkItemsByUser(queryIntervals.WorkItems.StartDate, queryIntervals.WorkItems.EndDate)
					: null;

				foreach (var stat in newStats)
				{
					var workItems = allWorkItems != null
						? allWorkItems[stat.StatsBuilder.UserId]
						: StatsDbHelper.GetWorkItemsForUser(stat.StatsBuilder.UserId, queryIntervals.WorkItems.StartDate, queryIntervals.WorkItems.EndDate);

					var aggrItems = isAggrUpdateRequired
						? intervalItems[stat.StatsBuilder.UserId]
						: StatsDbHelper.GetAggregateWorkItemIntervalsForUserCovered(stat.StatsBuilder.UserId, queryIntervals.OtherItems.StartDate, queryIntervals.OtherItems.EndDate);

					lock (stat.ThisLock)
					{
						foreach (var workItem in workItems)
						{
							stat.StatsBuilder.AddWorkItem(workItem);
						}
						stat.StatsBuilder.RefreshAggregateWorkItemIntervals(aggrItems);
						stat.StatsBuilder.RefreshManualWorkItems(manualItems[stat.StatsBuilder.UserId]);
						stat.StatsBuilder.RefreshMobileWorkItems(mobileItems[stat.StatsBuilder.UserId]);
						stat.StatsBuilder.RefreshTodaysMobileActivity(mobileLocations[stat.StatsBuilder.UserId], mobileActivity[stat.StatsBuilder.UserId]);
					}
					userStats[stat.StatsBuilder.UserId] = stat; //add new user as soon as possible
				}
			}

			foreach (var stat in currentStats)
			{
				lock (stat.ThisLock)
				{
					if (isAggrUpdateRequired) stat.StatsBuilder.RefreshAggregateWorkItemIntervals(intervalItems[stat.StatsBuilder.UserId]);
					stat.StatsBuilder.RefreshManualWorkItems(manualItems[stat.StatsBuilder.UserId]);
					stat.StatsBuilder.RefreshMobileWorkItems(mobileItems[stat.StatsBuilder.UserId]);
					stat.StatsBuilder.RefreshTodaysMobileActivity(mobileLocations[stat.StatsBuilder.UserId], mobileActivity[stat.StatsBuilder.UserId]);
				}
			}

			if (isAggrUpdateRequired) //if we successfully updated user stats with aggr info (i.e. no exceptions)
			{
				lastAggrUpdate = Environment.TickCount;
				lastAggrInterval = queryIntervals.OtherItems;
			}
			log.Debug("UpdateUserStats " + (isAggrUpdateRequired ? "with" : "without") + " aggregate workitems finished in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
		}

		private struct QueryIntervals
		{
			public readonly StartEndDateTime WorkItems; //Todays date
			public readonly StartEndDateTime OtherItems; //ThisWeek union with ThisMonth
			public readonly StartEndDateTime CalendarDays;

			public QueryIntervals(StartEndDateTime workItems, StartEndDateTime otherItems, StartEndDateTime calendarDays)
			{
				WorkItems = workItems;
				OtherItems = otherItems;
				CalendarDays = calendarDays;
			}
		}

		protected override void ManagerCallbackImpl()
		{
			UpdateUserStats();
		}
	}
}
