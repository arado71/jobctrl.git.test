using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderService.OnlineStats;
using Tct.ActivityRecorderService.Stats;

namespace Tct.ActivityRecorderService.DailyAggregation
{
	public static class DailyWorkTimesHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Aggregate(DateTime? untilDay = null)
		{
			var until = (untilDay ?? DateTime.UtcNow).Date;
			using (var context = new AggregateDataClassesDataContext())
			{
				var sw = Stopwatch.StartNew();
				context.ObjectTrackingEnabled = false;

				var aggregateDailyWorkTimes = context
					.AggregateDailyWorkTimes
					.Where(n => !n.IsValid)
					.Where(n => n.Day < until)
					.Select(n => new { UserId = n.UserId, Day = n.Day })
					.ToList();
				log.Debug("Found " + aggregateDailyWorkTimes.Count + " invalid aggregated daily worktimes until " + until + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");

				foreach (var aggregateDailyWorkTime in aggregateDailyWorkTimes)
				{
					Aggregate(aggregateDailyWorkTime.UserId, aggregateDailyWorkTime.Day);
				}

				log.Debug("Updated DailyAggregateWorkTimeTables for all users in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			}
		}

		internal static void Aggregate(int userId, DateTime day, Action statsCalculated = null)
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				var sw = Stopwatch.StartNew();
				context.ObjectTrackingEnabled = false;
				//get current aggregated worktimes
				var aggrEntry = context.AggregateDailyWorkTimes.Where(n => n.UserId == userId && n.Day == day).Single();

				//calculate daily work times from db data
				var dailyStats = GetDailyWorkTimeStatsFromDb(userId, day);

				if (statsCalculated != null) statsCalculated(); //for easier testing

				//write new calculated aggregated worktimes back to db
				context.UpdateDailyAggregateWorkTimeTables(userId, day,
					(int)dailyStats.NetWorkTime.TotalMilliseconds, (int)dailyStats.ComputerWorkTime.TotalMilliseconds,
					(int)dailyStats.MobileWorkTime.TotalMilliseconds,
					(int)dailyStats.ManuallyAddedWorkTime.TotalMilliseconds, (int)dailyStats.HolidayTime.TotalMilliseconds,
					(int)dailyStats.SickLeaveTime.TotalMilliseconds, aggrEntry.Version,
					dailyStats.TotalWorkTimeByWorkId.Select(n => new KeyValuePair<int, int>(n.Key, (int)n.Value.TotalMilliseconds)));

				log.Debug("Updated DailyAggregateWorkTimeTables for user " + userId.ToInvariantString() + " day " + day.ToShortDateString() +
						  " in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public static DailyWorkTimeStats GetDailyWorkTimeStatsFromDb(int userId, DateTime day, TimeSpan? interval = null)
		{
			if (interval.GetValueOrDefault() > TimeSpan.FromDays(1)) throw new ArgumentOutOfRangeException("interval");
			var startDate = day.Date;
			var endDate = day.Date.Add(interval ?? TimeSpan.FromDays(1));

			//get intervals db data
			var stats = StatsDbHelper.GetAggregateWorkItemIntervalsForUserCovered(userId, startDate, endDate);
			var manualStats = StatsDbHelper.GetManualWorkItemsForUserCovered(userId, startDate, endDate);
			var mobileStats = StatsDbHelper.GetMobileWorkItemsForUserCovered(userId, startDate, endDate);

			//add db data to calculator
			var statsBuilder = new IntervalWorkTimeStatsBuilder();
			statsBuilder.RefreshManualWorkItems(manualStats);
			statsBuilder.RefreshAggregateWorkItemIntervals(stats);
			statsBuilder.RefreshMobileWorkItems(mobileStats);

			//calculate worktimes from db data
			var workTimes = statsBuilder.GetIntervalWorkTime(startDate, endDate);

			//calculate worktimes by workid from db data
			//this is not ideal that we use two seperate builders
			var simpleStats = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(userId, startDate, endDate, stats, manualStats, mobileStats);

			return new DailyWorkTimeStats()
			{
				UserId = userId,
				Day = startDate,
				PartialInterval = interval,
				Version = 0,
				NetWorkTime = workTimes.SumWorkTime,
				ComputerWorkTime = workTimes.ComputerWorkTime,
				MobileWorkTime = workTimes.MobileWorkTime,
				ManuallyAddedWorkTime = workTimes.ManuallyAddedWorkTime,
				HolidayTime = workTimes.HolidayTime,
				SickLeaveTime = workTimes.SickLeaveTime,
				TotalWorkTimeByWorkId = (simpleStats.Stats ?? new Dictionary<int, SimpleWorkTimeStat>()).ToDictionary(n => n.Value.WorkId, n => n.Value.TotalWorkTime),
			};
		}
	}
}
