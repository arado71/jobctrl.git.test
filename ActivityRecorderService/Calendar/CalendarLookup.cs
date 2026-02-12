using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService
{
	public class CalendarLookup
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public int Id { get; private set; }

		private readonly object calculateLock = new object();

		private volatile CalendarLookupInterval cachedInterval;

		public CalendarLookup(int calendarId)
		{
			Id = calendarId;
		}

		public CalendarLookupInterval GetInterval(DateTime startDateInclusive, DateTime endDateInclusive)
		{
			return GetLookupInterval(startDateInclusive, endDateInclusive);
		}

		public List<DateTime> GetWorkDays(DateTime startDateInclusive, DateTime endDateInclusive)
		{
			var interval = GetLookupInterval(startDateInclusive, endDateInclusive);
			if (interval == null) return null;
			return interval.GetWorkDays(startDateInclusive, endDateInclusive);
		}

		public TimeSpan GetTargetWorkTime(DateTime startDateInclusive, DateTime endDateInclusive, TimeSpan defaultWorkTime, List<TargetWorkTimeInterval> targetWorkTimeIntervals = null)
		{
			var interval = GetLookupInterval(startDateInclusive, endDateInclusive);
			if (interval == null)
			{
				log.WarnFormat("Unable to query calendar {0} for {1}-{2}, calculating GetTargetWorkTime using fallback method", Id, startDateInclusive, endDateInclusive);
				return TimeSpan.FromTicks(CalendarHelper.GetFallbackWorkDays(startDateInclusive, endDateInclusive).Count * defaultWorkTime.Ticks);
			}

			return interval.GetTargetWorkTime(startDateInclusive, endDateInclusive, defaultWorkTime, targetWorkTimeIntervals);
		}

		public int CountWorkDays(DateTime startDateInclusive, DateTime endDateInclusive)
		{
			var interval = GetLookupInterval(startDateInclusive, endDateInclusive);
			if (interval == null)
			{
				log.WarnFormat("Unable to query calendar {0} for {1}-{2}, calculating CountWorkDays using fallback method", Id, startDateInclusive, endDateInclusive);
				return CalendarHelper.GetFallbackWorkDays(startDateInclusive, endDateInclusive).Count;
			}

			return interval.CountDays(startDateInclusive, endDateInclusive);
		}

		private CalendarLookupInterval GetLookupInterval(DateTime startDateInclusive, DateTime endDateInclusive)
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
						Interlocked.Exchange(ref cachedInterval, rightResult);
					}
				}

				return cachedInterval;
			}
		}

		private static bool IsWorkDayNoExceptions(Calendar calendar, DateTime date)
		{
			switch (date.DayOfWeek)
			{
				case DayOfWeek.Sunday:
					return calendar.IsSundayWorkDay;
				case DayOfWeek.Monday:
					return calendar.IsMondayWorkDay;
				case DayOfWeek.Tuesday:
					return calendar.IsTuesdayWorkDay;
				case DayOfWeek.Wednesday:
					return calendar.IsWednesdayWorkDay;
				case DayOfWeek.Thursday:
					return calendar.IsThursdayWorkDay;
				case DayOfWeek.Friday:
					return calendar.IsFridayWorkDay;
				case DayOfWeek.Saturday:
					return calendar.IsSaturdayWorkDay;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void AddToTargetWorkTimeLookup(Calendar calendar, Dictionary<DayOfWeek, TimeSpan> lookup)
		{
			if (calendar.TargetWorkTimeInMinutesMonday != null && !lookup.ContainsKey(DayOfWeek.Monday))
			{
				lookup.Add(DayOfWeek.Monday, TimeSpan.FromMinutes(calendar.TargetWorkTimeInMinutesMonday.Value));
			}

			if (calendar.TargetWorkTimeInMinutesTuesday != null && !lookup.ContainsKey(DayOfWeek.Tuesday))
			{
				lookup.Add(DayOfWeek.Tuesday, TimeSpan.FromMinutes(calendar.TargetWorkTimeInMinutesTuesday.Value));
			}

			if (calendar.TargetWorkTimeInMinutesWednesday != null && !lookup.ContainsKey(DayOfWeek.Wednesday))
			{
				lookup.Add(DayOfWeek.Wednesday, TimeSpan.FromMinutes(calendar.TargetWorkTimeInMinutesWednesday.Value));
			}

			if (calendar.TargetWorkTimeInMinutesThursday != null && !lookup.ContainsKey(DayOfWeek.Thursday))
			{
				lookup.Add(DayOfWeek.Thursday, TimeSpan.FromMinutes(calendar.TargetWorkTimeInMinutesThursday.Value));
			}

			if (calendar.TargetWorkTimeInMinutesFriday != null && !lookup.ContainsKey(DayOfWeek.Friday))
			{
				lookup.Add(DayOfWeek.Friday, TimeSpan.FromMinutes(calendar.TargetWorkTimeInMinutesFriday.Value));
			}

			if (calendar.TargetWorkTimeInMinutesSaturday != null && !lookup.ContainsKey(DayOfWeek.Saturday))
			{
				lookup.Add(DayOfWeek.Saturday, TimeSpan.FromMinutes(calendar.TargetWorkTimeInMinutesSaturday.Value));
			}

			if (calendar.TargetWorkTimeInMinutesSunday != null && !lookup.ContainsKey(DayOfWeek.Sunday))
			{
				lookup.Add(DayOfWeek.Sunday, TimeSpan.FromMinutes(calendar.TargetWorkTimeInMinutesSunday.Value));
			}
		}

		private CalendarLookupInterval CalculateInterval(DateTime startDateInclusive, DateTime endDateInclusive)
		{
			try
			{
				var targetWorkTimeLookup = new Dictionary<DayOfWeek, TimeSpan>();

				var res = new CalendarLookupInterval()
				{
					StartDate = startDateInclusive,
					EndDate = endDateInclusive,
				};

				if (startDateInclusive.Date > endDateInclusive.Date) throw new ArgumentException();
				using (var context = new IvrDataClassesDataContext())
				{
					Calendar mainCalendar = null;
					int? calId = Id;
					var exceptions = new Dictionary<DateTime, CalendarDay>();

					while (calId.HasValue)
					{
						var calendar = context.Calendars.Single(n => n.Id == calId.Value);
						if (mainCalendar == null)
						{
							mainCalendar = calendar;
						}

						AddToTargetWorkTimeLookup(calendar, targetWorkTimeLookup);
						var dbExceptions = calendar.CalendarExceptions
							.Where(n => n.Date >= startDateInclusive.Date && n.Date <= endDateInclusive.Date)
							.Select(n => new { n.Date, n.IsWorkDay, n.TargetWorkTimeInMinutes, n.CoreTimeEnd })
							.ToList();

						foreach (var dbException in dbExceptions)
						{
							if (!exceptions.ContainsKey(dbException.Date))
							{
								exceptions.Add(dbException.Date, new CalendarDay
								{
									Date = dbException.Date,
									IsWorkDay = dbException.IsWorkDay,
									TargetWorkTime =
										dbException.TargetWorkTimeInMinutes != null
											? (TimeSpan?)TimeSpan.FromMinutes(dbException.TargetWorkTimeInMinutes.Value)
											: null,
								});
							}
							else
							{
								var exception = exceptions[dbException.Date];
								exception.TargetWorkTime = exception.TargetWorkTime ??
														   (dbException.TargetWorkTimeInMinutes != null
															   ? (TimeSpan?)TimeSpan.FromMinutes(dbException.TargetWorkTimeInMinutes.Value)
															   : null);
							}
						}

						calId = calendar.InheritedFrom;
					}

					var curr = startDateInclusive.Date;
					var sumTargetWorkTime = TimeSpan.Zero;
					var countDaysWithDefaultTargetWorkTime = 0;
					while (curr <= endDateInclusive.Date)
					{
						bool isWorkDay;
						var targetWorkTime = (TimeSpan?)null;

						TimeSpan workTime;
						if (targetWorkTimeLookup.TryGetValue(curr.DayOfWeek, out workTime))
						{
							targetWorkTime = workTime;
						}

						CalendarDay calendarDay;
						if (!exceptions.TryGetValue(curr, out calendarDay))
						{
							isWorkDay = IsWorkDayNoExceptions(mainCalendar, curr);
						}
						else
						{
							isWorkDay = calendarDay.IsWorkDay;
							targetWorkTime = calendarDay.TargetWorkTime ?? targetWorkTime;
						}

						if (isWorkDay)
						{
							res.SortedWorkDays.Add(curr);

							if (targetWorkTime != null)
							{
								sumTargetWorkTime += targetWorkTime.Value;
							}
							else
							{
								countDaysWithDefaultTargetWorkTime++;
							}

							res.SumCustomTargetWorkTimes.Add(sumTargetWorkTime);
							res.DefaultTargetWorkTimeDayCounts.Add(countDaysWithDefaultTargetWorkTime);
						}

						curr = curr.AddDays(1);
					}
				}

				log.DebugFormat("Calculated interval {0}-{1}", startDateInclusive, endDateInclusive);
				return res;
			}
			catch (Exception ex)
			{
				log.Error("Unable to query calendar " + Id + " between " + startDateInclusive + " and " + endDateInclusive, ex);
				return null;
			}
		}

		private class CalendarDay
		{
			public DateTime Date { get; set; }
			public bool IsWorkDay { get; set; }
			public TimeSpan? TargetWorkTime { get; set; }
		}

		public class CalendarLookupInterval
		{
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
			public List<DateTime> SortedWorkDays { get; private set; }
			public List<TimeSpan> SumCustomTargetWorkTimes { get; private set; }
			public List<int> DefaultTargetWorkTimeDayCounts { get; private set; }

			public CalendarLookupInterval()
			{
				SortedWorkDays = new List<DateTime>();
				SumCustomTargetWorkTimes = new List<TimeSpan> { TimeSpan.Zero };
				DefaultTargetWorkTimeDayCounts = new List<int> { 0 };
			}

			public void MergeContinuous(CalendarLookupInterval otherInterval)
			{
				Debug.Assert(StartDate == otherInterval.EndDate.AddDays(1) || EndDate.AddDays(1) == otherInterval.StartDate, "Interval not continuous");
				if (otherInterval.StartDate < StartDate)
				{
					MergeBefore(otherInterval);
				}
				else
				{
					MergeAfter(otherInterval);
				}
			}

			public List<DateTime> GetWorkDays(DateTime startDateInclusive, DateTime endDateInclusive)
			{
				var firstWorkIdx = SearchFirstWorkday(startDateInclusive);
				var afterLastIdx = SearchNextWorkday(endDateInclusive);
				return SortedWorkDays.GetRange(firstWorkIdx, afterLastIdx - firstWorkIdx);
			}

			public int CountDays(DateTime startDateInclusive, DateTime endDateInclusive)
			{
				return SearchNextWorkday(endDateInclusive) - SearchFirstWorkday(startDateInclusive);
			}

			public TimeSpan GetTargetWorkTime(DateTime startDateInclusive, DateTime endDateInclusive, TimeSpan defaultWorkTime, List<TargetWorkTimeInterval> targetWorkTimeIntervals)
			{
				if (targetWorkTimeIntervals == null || endDateInclusive < targetWorkTimeIntervals.First().StartDate) return GetTargetWorkTime(startDateInclusive, endDateInclusive, defaultWorkTime);
				var start = startDateInclusive;
				var workTime = targetWorkTimeIntervals.Where(t => t.StartDate < start).LastOrDefault()?.TargetWorkTime ?? defaultWorkTime;
				var result = TimeSpan.Zero;
				foreach (var targetWorkTimeInterval in targetWorkTimeIntervals.Where(t => t.StartDate >= start).ToList())
				{
					var end = targetWorkTimeInterval.StartDate.AddDays(-1) < endDateInclusive ? targetWorkTimeInterval.StartDate.AddDays(-1) : endDateInclusive;
					result += GetTargetWorkTime(start, end, workTime);
					start = end.AddDays(1);
					if (start > endDateInclusive) break;
					workTime = targetWorkTimeInterval.TargetWorkTime;
				}
				if (start <= endDateInclusive)
					result += GetTargetWorkTime(start, endDateInclusive, workTime);
				return result;
			}

			public TimeSpan GetTargetWorkTime(DateTime startDateInclusive, DateTime endDateInclusive, TimeSpan defaultWorkTime)
			{
				var startIdx = SearchFirstWorkday(startDateInclusive);
				var endIdx = SearchNextWorkday(endDateInclusive);
				var defaultWorktimeWorkDays = DefaultTargetWorkTimeDayCounts[endIdx] -
									  DefaultTargetWorkTimeDayCounts[startIdx];
				var nonDefaultWorktime = SumCustomTargetWorkTimes[endIdx] -
										 SumCustomTargetWorkTimes[startIdx];
				// log.DebugFormat("Between {0}-{1} there is {2} standard days and {3} custom worktime", startDateInclusive, endDateInclusive, defaultWorktimeWorkDays, nonDefaultWorktime);
				return new TimeSpan(defaultWorktimeWorkDays * defaultWorkTime.Ticks) + nonDefaultWorktime;
			}

			private void MergeAfter(CalendarLookupInterval otherInterval)
			{
				Debug.Assert(EndDate.AddDays(1) == otherInterval.StartDate, "Intervals not continous");
				EndDate = otherInterval.EndDate;
				if (otherInterval.SortedWorkDays.Count == 0) return;

				var lastWorkTime = SumCustomTargetWorkTimes[SumCustomTargetWorkTimes.Count - 1];
				var lastWorkDays = DefaultTargetWorkTimeDayCounts[DefaultTargetWorkTimeDayCounts.Count - 1];

				SortedWorkDays.AddRange(otherInterval.SortedWorkDays);
				SumCustomTargetWorkTimes.AddRange(
					otherInterval.SumCustomTargetWorkTimes.Skip(1).Select(x => x + lastWorkTime));
				DefaultTargetWorkTimeDayCounts.AddRange(
					otherInterval.DefaultTargetWorkTimeDayCounts.Skip(1).Select(x => x + lastWorkDays));
			}

			private void MergeBefore(CalendarLookupInterval otherInterval)
			{
				Debug.Assert(otherInterval.EndDate.AddDays(1) == StartDate, "Intervals not continous");
				StartDate = otherInterval.StartDate;
				if (otherInterval.SortedWorkDays.Count == 0) return;
				
				var lastWorkTime =
					otherInterval.SumCustomTargetWorkTimes[otherInterval.SumCustomTargetWorkTimes.Count - 1];
				var originalAggregates = SumCustomTargetWorkTimes;
				var lastWorkDays =
					otherInterval.DefaultTargetWorkTimeDayCounts[otherInterval.DefaultTargetWorkTimeDayCounts.Count - 1];
				var originalDefaultWorktimeDayCount = DefaultTargetWorkTimeDayCounts;

				SortedWorkDays.InsertRange(0, otherInterval.SortedWorkDays);
				SumCustomTargetWorkTimes = new List<TimeSpan>(otherInterval.SumCustomTargetWorkTimes);
				SumCustomTargetWorkTimes.AddRange(originalAggregates.Skip(1).Select(x => x + lastWorkTime));
				DefaultTargetWorkTimeDayCounts = new List<int>(otherInterval.DefaultTargetWorkTimeDayCounts);
				DefaultTargetWorkTimeDayCounts.AddRange(originalDefaultWorktimeDayCount.Skip(1).Select(x => x + lastWorkDays));
			}

			public int SearchFirstWorkday(DateTime date)
			{
				var idx = SortedWorkDays.BinarySearch(date);
				return idx < 0 ? ~idx : idx;
			}

			public int SearchNextWorkday(DateTime date)
			{
				var idx = SortedWorkDays.BinarySearch(date);
				return (idx < 0) ? ~idx : idx + 1;
			}
		}
	}
}
