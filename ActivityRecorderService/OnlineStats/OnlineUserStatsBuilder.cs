using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderService.Caching;
using Tct.ActivityRecorderService.Stats;

namespace Tct.ActivityRecorderService.OnlineStats
{
	/// <summary>
	/// Generates online stats for a user
	/// </summary>
	public class OnlineUserStatsBuilder
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public int UserId { get; private set; }
		public UserStatInfo UserInfo { get; private set; }
		private readonly OnlineTodaysWorkTimeBuilder todaysWorkTimeBuilder;
		private readonly CalendarManager calendarManager;
		private readonly IntervalWorkTimeStatsBuilder intervalWorkTimeStatsBuilder = new IntervalWorkTimeStatsBuilder();
		private readonly WorkTimeSpecificBuilder workTimeSpecificBuilder;
		private readonly Cache cache;
		private readonly ThreadSafeCachedFunc<int, CalendarLookup, WorktimeSchedulesLookup> worktimeSchedulesResolver = new ThreadSafeCachedFunc<int, CalendarLookup, WorktimeSchedulesLookup>(CreateWorktimeSchedulesLookup, TimeSpan.FromMilliseconds(ConfigManager.CalendarUpdateInterval));

		public OnlineUserStatsBuilder(UserStatInfo userStatInfo, OnlineStatsManager onlineStatsManager, CalendarManager calendarManager)
		{
			Debug.Assert(userStatInfo != null);
			UserId = userStatInfo.Id;
			UserInfo = userStatInfo;
			todaysWorkTimeBuilder = new OnlineTodaysWorkTimeBuilder(userStatInfo, onlineStatsManager);
			workTimeSpecificBuilder = new WorkTimeSpecificBuilder(this,
				() => worktimeSchedulesResolver.GetOrCalculateValue(UserId,
					calendarManager.GetCalenderLookup(UserInfo.CalendarId)));
			this.calendarManager = calendarManager;
			cache = new Cache(this);
		}

		public BriefUserStats GetBriefUserStats()
		{
			return GetDetailedUserStats();
		}

		public DetailedUserStats GetDetailedUserStats()
		{
			var reportIntervals = new ReportIntervals(DateTime.UtcNow, UserInfo);
			var result = new DetailedUserStats
							{
								UserId = UserId,
								UserName = UserInfo.LastName + ", " + UserInfo.FirstName,
								UserTimeZoneString = UserInfo.TimeZone.ToSerializedString()
							};
			var sw = Stopwatch.StartNew();
			todaysWorkTimeBuilder.UpdateTodaysStatsInDetailedUserStats(result, reportIntervals.Today.StartDate, reportIntervals.Today.EndDate);
			var elapsedToday = sw.Elapsed;
			sw.Restart();

			//don't refresh this weeks, this months work time everytime ??? but that is quite fast... but what is faster is caching...
			result.ThisWeeksWorkTime = result.TodaysWorkTime.Clone();
			if (reportIntervals.ThisWeek.StartDate < reportIntervals.Today.StartDate)
			{
				//var thisWeekBeforeToday = intervalWorkTimeStatsBuilder.GetIntervalWorkTime(reportIntervals.ThisWeek.StartDate,
				//																		   reportIntervals.Today.StartDate);
				var thisWeekBeforeToday = cache.GetIntervalWorkTimeForWeekOrCached(reportIntervals.ThisWeek.StartDate,
																						   reportIntervals.Today.StartDate);
				result.ThisWeeksWorkTime.ComputerWorkTime += thisWeekBeforeToday.ComputerWorkTime;
				result.ThisWeeksWorkTime.MobileWorkTime += thisWeekBeforeToday.MobileWorkTime;
				result.ThisWeeksWorkTime.ManuallyAddedWorkTime += thisWeekBeforeToday.ManuallyAddedWorkTime;
				result.ThisWeeksWorkTime.HolidayTime += thisWeekBeforeToday.HolidayTime;
				result.ThisWeeksWorkTime.SickLeaveTime += thisWeekBeforeToday.SickLeaveTime;
				result.ThisWeeksWorkTime.NetWorkTime += thisWeekBeforeToday.SumWorkTime;
			}
			var elapsedWeek = sw.Elapsed;
			sw.Restart();

			result.ThisMonthsWorkTime = result.TodaysWorkTime.Clone();
			if (reportIntervals.ThisMonth.StartDate < reportIntervals.Today.StartDate)
			{
				//var thisMonthBeforeToday = intervalWorkTimeStatsBuilder.GetIntervalWorkTime(reportIntervals.ThisMonth.StartDate,
				//																		   reportIntervals.Today.StartDate);
				var thisMonthBeforeToday = cache.GetIntervalWorkTimeForMonthOrCached(reportIntervals.ThisMonth.StartDate,
																						   reportIntervals.Today.StartDate);
				result.ThisMonthsWorkTime.ComputerWorkTime += thisMonthBeforeToday.ComputerWorkTime;
				result.ThisMonthsWorkTime.MobileWorkTime += thisMonthBeforeToday.MobileWorkTime;
				result.ThisMonthsWorkTime.ManuallyAddedWorkTime += thisMonthBeforeToday.ManuallyAddedWorkTime;
				result.ThisMonthsWorkTime.HolidayTime += thisMonthBeforeToday.HolidayTime;
				result.ThisMonthsWorkTime.SickLeaveTime += thisMonthBeforeToday.SickLeaveTime;
				result.ThisMonthsWorkTime.NetWorkTime += thisMonthBeforeToday.SumWorkTime;
			}
			var elapsedMonth = sw.Elapsed;
			sw.Restart();

			foreach (var briefWorkStats in result.TodaysWorksByWorkId.Values)
			{
				//briefWorkStats.WorkName = Caching.Works.WorkHierarchyService.Instance.GetWorkName(briefWorkStats.WorkId);
				briefWorkStats.WorkName = Caching.Works.WorkHierarchyService.Instance.GetWorkNameWithProjects(briefWorkStats.WorkId, 60);
			}
			var elapsedWorks = sw.Elapsed;

			result.TodaysTargetNetWorkTime = GetTargetNetWorkTime(reportIntervals.LocalReportDate, reportIntervals.LocalReportDate);
			result.ThisWeeksTargetNetWorkTime = GetTargetNetWorkTime(reportIntervals.ThisWeekLocalDay.StartDate, reportIntervals.ThisWeekLocalDay.EndDate);
			result.ThisMonthsTargetNetWorkTime = GetTargetNetWorkTime(reportIntervals.ThisMonthLocalDay.StartDate, reportIntervals.ThisMonthLocalDay.EndDate);
			result.ThisWeeksTargetUntilTodayNetWorkTime = GetTargetNetWorkTime(reportIntervals.ThisWeekLocalDay.StartDate, reportIntervals.LocalReportDate);
			result.ThisMonthsTargetUntilTodayNetWorkTime = GetTargetNetWorkTime(reportIntervals.ThisMonthLocalDay.StartDate, reportIntervals.LocalReportDate);

			/* too verbose...
			log.Debug("Generated stats for user " + UserId + " (day/week/month/works:sum) in "
				+ elapsedToday.TotalMilliseconds.ToString("0.000") + " / "
				+ elapsedWeek.TotalMilliseconds.ToString("0.000") + " / "
				+ elapsedMonth.TotalMilliseconds.ToString("0.000") + " / "
				+ elapsedWorks.TotalMilliseconds.ToString("0.000") + " : "
				+ (elapsedToday + elapsedWeek + elapsedMonth + elapsedWorks).TotalMilliseconds.ToString("0.000")
				+ "ms ");*/

			return result;
		}

		public SimpleWorkTimeStats GetSimpleWorkTimeStatsFromOM(DateTime desiredEndDate)
		{
			var reportIntervals = new ReportIntervals(DateTime.UtcNow, UserInfo);
			//get the earliest date for which we have data
			var startDate = reportIntervals.ThisMonth.StartDate < reportIntervals.ThisWeek.StartDate
				? reportIntervals.ThisMonth.StartDate
				: reportIntervals.ThisWeek.StartDate; //MIN
			//make sure that we have data for desiredEndDate in OM (should be between startDate and today's endDate)
			var endDate = reportIntervals.Today.EndDate < desiredEndDate
				? reportIntervals.Today.EndDate //we have data until this
				: desiredEndDate < startDate
					? startDate
					: desiredEndDate;

			if (reportIntervals.Today.StartDate >= endDate) //we have all data in intervalWorkTimeStatsBuilder
			{
				return SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(UserId, startDate, endDate,
																	intervalWorkTimeStatsBuilder.AggregateWorkItemIntervals,
																	intervalWorkTimeStatsBuilder.ManualWorkItems,
																	intervalWorkTimeStatsBuilder.MobileWorkItems);
			}
			else //we have to get todays data and older data from two places (todays data is more up to date)
			{
				//var olderStats = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(UserId, startDate, reportIntervals.Today.StartDate,
				//													intervalWorkTimeStatsBuilder.AggregateWorkItemIntervals,
				//													intervalWorkTimeStatsBuilder.IvrWorkItems,
				//													intervalWorkTimeStatsBuilder.ManualWorkItems,
				//													intervalWorkTimeStatsBuilder.MobileWorkItems);
				var olderStats = cache.GetSimpleWorkTimeStatsOrCached(startDate, reportIntervals.Today.StartDate);
				var todaysStat = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(UserId, reportIntervals.Today.StartDate, endDate,
																	todaysWorkTimeBuilder.WorkItems,
																	todaysWorkTimeBuilder.ManualWorkItems,
																	todaysWorkTimeBuilder.MobileWorkItems);
				//olderStats.MergeWith(todaysStat); //merge the results
				//return olderStats;
				todaysStat.MergeWith(olderStats); //merge the results
				return todaysStat;
			}
		}

		public void RefreshManualWorkItems(IEnumerable<IManualWorkItem> items)
		{
			intervalWorkTimeStatsBuilder.RefreshManualWorkItems(items);
			todaysWorkTimeBuilder.RefreshManualWorkItems(items);
		}

		public void RefreshAggregateWorkItemIntervals(IEnumerable<AggregateWorkItemIntervalCovered> items)
		{
			intervalWorkTimeStatsBuilder.RefreshAggregateWorkItemIntervals(items);
			todaysWorkTimeBuilder.RefreshAggregateWorkItemIntervals(items);
		}

		public void RefreshMobileWorkItems(IEnumerable<IMobileWorkItem> items)
		{
			intervalWorkTimeStatsBuilder.RefreshMobileWorkItems(items);
			todaysWorkTimeBuilder.RefreshMobileWorkItems(items);
		}

		public void RefreshTodaysMobileActivity(IEnumerable<MobileLocationInfo> locations, IEnumerable<MobileActivityInfo> activities)
		{
			todaysWorkTimeBuilder.RefreshTodaysMobileActivity(locations, activities);
		}

		public void AddWorkItem(WorkItem item)
		{
			//var sw = Stopwatch.StartNew();
			todaysWorkTimeBuilder.AddWorkItem(item);
			//log.Debug("AddWorkItem for user " + UserId + " finished in " + sw.Elapsed.TotalMilliseconds.ToString("0.000") + "ms ");
			workTimeSpecificBuilder.AddWorkItem(item);
		}

		public void StartComputerWork(int workId, int computerId, DateTime createDate, DateTime userTime, DateTime serverTime)
		{
			todaysWorkTimeBuilder.StartComputerWork(workId, computerId, createDate, userTime, serverTime);
		}

		public void StopComputerWork(int computerId, DateTime createDate, DateTime userTime, DateTime serverTime)
		{
			todaysWorkTimeBuilder.StopComputerWork(computerId, createDate, userTime, serverTime);
		}

		public ReportIntervals GetReportIntervals(DateTime utcNow)
		{
			return new ReportIntervals(utcNow, UserInfo);
		}

		public List<DateTime> GetUserWorkdays()
		{
			var calendarLookup = calendarManager.GetCalenderLookup(UserInfo.CalendarId);
			var timeScheduleLookup = worktimeSchedulesResolver.GetOrCalculateValue(UserId, calendarLookup);
			return timeScheduleLookup.GetWorkDays(DateTime.Today, DateTime.Today.AddDays(30));

		}

		private TimeSpan GetTargetNetWorkTime(DateTime startDayInclusive, DateTime endDayInclusive)
		{
			startDayInclusive = UserInfo.FirstWorkDay > startDayInclusive ? UserInfo.FirstWorkDay : startDayInclusive;
			var calendarLookup = calendarManager.GetCalenderLookup(UserInfo.CalendarId);
			var timeScheduleLookup = worktimeSchedulesResolver.GetOrCalculateValue(UserId, calendarLookup);
			return calendarLookup == null || startDayInclusive > endDayInclusive
				? TimeSpan.Zero
				: timeScheduleLookup.GetTargetWorkTime(startDayInclusive, endDayInclusive, UserInfo.TargetWorkTime, UserInfo.TargetWorkTimeIntervals);
		}

		private static WorktimeSchedulesLookup CreateWorktimeSchedulesLookup(int userId, CalendarLookup calendarLookup)
		{
			return new WorktimeSchedulesLookup(userId, calendarLookup);
		}

		//this is a quick and dirty cache
		//ideally we would cache based on changed intervals (deletions etc.)
		//assert userId is not changed
		private class Cache
		{
			private readonly CacheEntry<StartEndDateTime, SimpleWorkTimeStats> simple;
			private readonly CacheEntry<StartEndDateTime, IntervalWorkTimeStats> weekInterval;
			private readonly CacheEntry<StartEndDateTime, IntervalWorkTimeStats> monthInterval;

			public Cache(OnlineUserStatsBuilder parent)
			{
				simple = new CacheEntry<StartEndDateTime, SimpleWorkTimeStats>(
					TimeSpan.FromSeconds(ConfigManager.OnlineUserStatsOldCacheAgeInSec),
					startEndDate => SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(parent.UserId, startEndDate.StartDate, startEndDate.EndDate,
									   parent.intervalWorkTimeStatsBuilder.AggregateWorkItemIntervals,
									   parent.intervalWorkTimeStatsBuilder.ManualWorkItems,
									   parent.intervalWorkTimeStatsBuilder.MobileWorkItems));

				weekInterval = new CacheEntry<StartEndDateTime, IntervalWorkTimeStats>(
					TimeSpan.FromSeconds(ConfigManager.OnlineUserStatsOldCacheAgeInSec),
					startEndDate => parent.intervalWorkTimeStatsBuilder.GetIntervalWorkTime(startEndDate.StartDate, startEndDate.EndDate));

				monthInterval = new CacheEntry<StartEndDateTime, IntervalWorkTimeStats>(
					TimeSpan.FromSeconds(ConfigManager.OnlineUserStatsOldCacheAgeInSec),
					startEndDate => parent.intervalWorkTimeStatsBuilder.GetIntervalWorkTime(startEndDate.StartDate, startEndDate.EndDate));
			}

			public SimpleWorkTimeStats GetSimpleWorkTimeStatsOrCached(DateTime startDate, DateTime endDate)
			{
				return simple.GetValue(new StartEndDateTime(startDate, endDate));
			}

			public IntervalWorkTimeStats GetIntervalWorkTimeForWeekOrCached(DateTime startDate, DateTime endDate)
			{
				return weekInterval.GetValue(new StartEndDateTime(startDate, endDate));
			}

			public IntervalWorkTimeStats GetIntervalWorkTimeForMonthOrCached(DateTime startDate, DateTime endDate)
			{
				return monthInterval.GetValue(new StartEndDateTime(startDate, endDate));
			}

			private class CacheEntry<TKey, TValue>
				where TKey : IEquatable<TKey>
				where TValue : class
			{
				private TKey cachedKey;
				private TValue cachedValue;
				private readonly StopwatchLite sw;
				private readonly Func<TKey, TValue> factory;

				public CacheEntry(TimeSpan lifeSpan, Func<TKey, TValue> valueFactory)
				{
					sw = new StopwatchLite(lifeSpan, true);
					factory = valueFactory;
				}

				public TValue GetValue(TKey key)
				{
					if (EqualityComparer<TKey>.Default.Equals(key, cachedKey)
						&& cachedValue != default(TValue)
						&& !sw.IsIntervalElapsed())
					{
						return cachedValue;
					}
					cachedValue = factory(key);
					cachedKey = key;
					sw.Restart();
					return cachedValue;
				}
			}
		}
	}
}
