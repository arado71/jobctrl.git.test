using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService
{
	public class WorktimeSchedulesLookup
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private int userId;
		private CalendarLookup calendarLookup;
		private readonly object calculateLock = new object();

		private volatile WorktimeSchedulesLookupInterval cachedInterval;

		public WorktimeSchedulesLookup(int userId, CalendarLookup calendarLookup)
		{
			this.userId = userId;
			this.calendarLookup = calendarLookup;
		}

		public List<DateTime> GetWorkDays(DateTime startDateInclusive, DateTime endDateInclusive)
		{
			var interval = GetLookupInterval(startDateInclusive, endDateInclusive);
			if (interval == null) return null;
			return interval.GetWorkDays(startDateInclusive, endDateInclusive);
		}

		public TimeSpan GetTargetWorkTime(DateTime startDateInclusive, DateTime endDateInclusive, TimeSpan defaultWorkTime, List<TargetWorkTimeInterval> targetWorkTimeIntervals = null)
		{
			var timeScheduleLookupInterval = GetLookupInterval(startDateInclusive, endDateInclusive);
			return timeScheduleLookupInterval != null && timeScheduleLookupInterval.HasSchedule ? timeScheduleLookupInterval.GetTargetWorkTime(startDateInclusive, endDateInclusive, defaultWorkTime, targetWorkTimeIntervals) : calendarLookup.GetTargetWorkTime(startDateInclusive, endDateInclusive, defaultWorkTime, targetWorkTimeIntervals);
		}

		public WorkTimeScheduleSpecific GetWorkTimeScheduleSpecific(DateTime day)
		{
			var timeScheduleLookupInterval = GetLookupInterval(day, day);
			return timeScheduleLookupInterval != null &&
			       timeScheduleLookupInterval.SpecificDateByDay.TryGetValue(day, out var value)
				? value
				: null;
		}

		private WorktimeSchedulesLookupInterval GetLookupInterval(DateTime startDateInclusive, DateTime endDateInclusive)
		{
			Debug.Assert(startDateInclusive == startDateInclusive.Date);
			Debug.Assert(endDateInclusive == endDateInclusive.Date);
			var currentCachedInterval = cachedInterval;
			if (currentCachedInterval != null && currentCachedInterval.StartDate < startDateInclusive && currentCachedInterval.EndDate > endDateInclusive)
			{
				return currentCachedInterval;
			}

			lock (calculateLock)
			{
				if (cachedInterval != null && cachedInterval.StartDate < startDateInclusive && cachedInterval.EndDate > endDateInclusive)
				{
					return cachedInterval;
				}

				if (cachedInterval == null)
				{
					var result = CalculateInterval(startDateInclusive, endDateInclusive);
					Interlocked.Exchange(ref cachedInterval, result);
				}
				else
				{
					if (cachedInterval.StartDate > startDateInclusive)
					{
						var leftResult = CalculateInterval(startDateInclusive, cachedInterval.StartDate.AddDays(-1));
						if (leftResult == null)
						{
							return null;
						}

						leftResult.MergeContinuous(cachedInterval);
						leftResult.HasSchedule |= cachedInterval.HasSchedule;
						foreach (var pair in cachedInterval.SpecificDateByDay)
						{
							leftResult.SpecificDateByDay.Add(pair.Key, pair.Value);
						}
						Interlocked.Exchange(ref cachedInterval, leftResult);
					}

					if (cachedInterval.EndDate < endDateInclusive)
					{
						var rightResult = CalculateInterval(cachedInterval.EndDate.AddDays(1), endDateInclusive);
						if (rightResult == null)
						{
							return null;
						}

						rightResult.MergeContinuous(cachedInterval);
						rightResult.HasSchedule |= cachedInterval.HasSchedule;
						foreach (var pair in cachedInterval.SpecificDateByDay)
						{
							rightResult.SpecificDateByDay.Add(pair.Key, pair.Value);
						}
						Interlocked.Exchange(ref cachedInterval, rightResult);
					}
				}

				return cachedInterval;
			}
		}

		private DateTime Max(DateTime v1, DateTime v2)
		{
			return v1 < v2 ? v2 : v1;
		}

		private DateTime Min(DateTime v1, DateTime v2)
		{
			return v1 > v2 ? v2 : v1;
		}

		private WorktimeSchedulesLookupInterval CalculateInterval(DateTime startDateInclusive, DateTime endDateInclusive)
		{
			try
			{
				var res = new WorktimeSchedulesLookupInterval() { StartDate = startDateInclusive, EndDate = endDateInclusive, };
				using (var context = new JobControlDataClassesDataContext())
				{
					var tempEnumerable = context.GetWorktimeSchedulesForUser(userId, startDateInclusive, endDateInclusive, out TimeZoneInfo timeZone, out int endOfDayInMinutes); 
					var offset = timeZone.BaseUtcOffset - TimeSpan.FromMinutes(endOfDayInMinutes);
					var items = tempEnumerable.Select(i => new {i.Day, StartDate = i.StartDate != default(DateTime) ? i.StartDate + offset : i.StartDate, EndDate = i.EndDate != default(DateTime) ? i.EndDate + offset : i.EndDate, i.IsWorkDay, i.SpecificDailyOvertimeLimit}).ToList();
					var interval = calendarLookup.GetInterval(startDateInclusive, endDateInclusive);
					var calIdx = interval.SearchFirstWorkday(startDateInclusive);
					var calEndIdx = interval.SearchNextWorkday(endDateInclusive);
					var sumTargetWorkTime = TimeSpan.Zero;
					var countDaysWithDefaultTargetWorkTime = 0;
					for (var day = startDateInclusive; day <= endDateInclusive; day += TimeSpan.FromDays(1))
					{
						var isHolidayInSchedule = !items.Where(i => i.Day == day).All(i => i.IsWorkDay) && items.Any(i => i.Day == day);
						var workTime = TimeSpan.FromMilliseconds(items.Where(i => /*i.Day == day &&*/ i.IsWorkDay && (i.StartDate.Date == day || i.EndDate.Date == day)).Sum(i => (Min(i.EndDate, day.AddDays(1)) - Max(i.StartDate, day)).TotalMilliseconds));
						if (isHolidayInSchedule || workTime > TimeSpan.Zero) res.HasSchedule = true;
						var isWorkInCalendar = calIdx < calEndIdx && day == interval.SortedWorkDays[calIdx];
						if (isWorkInCalendar)
						{
							if (!isHolidayInSchedule && workTime == TimeSpan.Zero)
							{
								sumTargetWorkTime += interval.SumCustomTargetWorkTimes[calIdx + 1] - interval.SumCustomTargetWorkTimes[calIdx];
								countDaysWithDefaultTargetWorkTime += interval.DefaultTargetWorkTimeDayCounts[calIdx + 1] - interval.DefaultTargetWorkTimeDayCounts[calIdx];
							}
							calIdx++;
						}
						if ((isWorkInCalendar || workTime > TimeSpan.Zero) && !isHolidayInSchedule)
						{
							sumTargetWorkTime += workTime;
							res.SortedWorkDays.Add(day);
							res.SumCustomTargetWorkTimes.Add(sumTargetWorkTime);
							res.DefaultTargetWorkTimeDayCounts.Add(countDaysWithDefaultTargetWorkTime);
						}
						var found = items.FirstOrDefault(i => i.Day == day && i.SpecificDailyOvertimeLimit != null);
						if (found != null)
						{
							res.SpecificDateByDay.Add(day, new WorkTimeScheduleSpecific
							{
								DailyTargetWorkTime = workTime,
								DailyOvertimeLimit = found.SpecificDailyOvertimeLimit.Value,
							});
						}
					}
				}

				//log.DebugFormat("Calculated time schedule interval {0}-{1} ({2})", startDateInclusive, endDateInclusive, res.HasSchedule);
				return res;
			}
			catch (Exception ex)
			{
				Debug.Assert(true, "unexpected error " + ex.Message);
				log.Error("Unable to query time schedule " + userId + " between " + startDateInclusive + " and " + endDateInclusive, ex);
				return null;
			}

		}

		private class WorktimeSchedulesLookupInterval : CalendarLookup.CalendarLookupInterval
		{
			public bool HasSchedule { get; set; }

			public Dictionary<DateTime, WorkTimeScheduleSpecific> SpecificDateByDay { get; } =
				new Dictionary<DateTime, WorkTimeScheduleSpecific>();
		}

		public class WorkTimeScheduleSpecific
		{
			public TimeSpan DailyTargetWorkTime { get; set; }
			public TimeSpan DailyOvertimeLimit { get; set; }

		}
	}
}
