using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService
{
	public static class CalendarHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static bool IsWorkDay(int calendarId, DateTime date)
		{
			using (var context = new IvrDataClassesDataContext())
			{
				return context.IsWorkDay(calendarId, date).Value;
			}
		}

		public static bool IsWorkDayOld(int calendarId, DateTime date)
		{
			using (var context = new IvrDataClassesDataContext())
			{
				var calendar = context.Calendars.Single(n => n.Id == calendarId);
				var exception = calendar.CalendarExceptions.SingleOrDefault(n => n.Date == date.Date);
				if (exception != null)
				{
					return exception.IsWorkDay;
				}
				//check in inherited calendars
				var baseCalendarId = calendar.InheritedFrom;
				while (baseCalendarId.HasValue)
				{
					var baseCalendar = context.Calendars.Single(n => n.Id == baseCalendarId.Value);
					var baseException = baseCalendar.CalendarExceptions.SingleOrDefault(n => n.Date == date.Date);
					if (baseException != null)
					{
						return baseException.IsWorkDay;
					}

					baseCalendarId = baseCalendar.InheritedFrom;
				}
				//no exceptions found
				return IsWorkDayNoExceptions(calendar, date);
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

		public static List<DateTime> GetWorkDaysNoThrow(int calendarId, DateTime startDateInclusive, DateTime endDateInclusive)
		{
			try
			{
				return GetWorkDays(calendarId, startDateInclusive, endDateInclusive);
			}
			catch (Exception ex)
			{
				log.Error("Unable to get work days for calendar " + calendarId + " between " + startDateInclusive + " and " + endDateInclusive, ex);
				return null;
			}
		}

		public static List<DateTime> GetFallbackWorkDays(DateTime startDateInclusive, DateTime endDateInclusive)
		{
			var fallbackResult = new List<DateTime>();
			for (var day = startDateInclusive.Date; day <= endDateInclusive.Date; day = day.AddDays(1))
			{
				if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday) continue;
				fallbackResult.Add(day);
			}
			return fallbackResult;
		}

		public static List<DateTime> GetWorkDays(int calendarId, DateTime startDateInclusive, DateTime endDateInclusive)
		{
			if (startDateInclusive.Date > endDateInclusive.Date) throw new ArgumentException();
			using (var context = new IvrDataClassesDataContext())
			{
				var result = new List<DateTime>();
				Calendar mainCalendar = null;
				int? calId = calendarId;
				var exceptions = new Dictionary<DateTime, bool>();

				while (calId.HasValue)
				{
					var calendar = context.Calendars.Single(n => n.Id == calId.Value);
					if (mainCalendar == null) mainCalendar = calendar;

					var dbExceptions = calendar.CalendarExceptions
						.Where(n => n.Date >= startDateInclusive.Date && n.Date <= endDateInclusive.Date)
						.Select(n => new { n.Date, n.IsWorkDay })
						.ToList();

					foreach (var dbException in dbExceptions)
					{
						if (!exceptions.ContainsKey(dbException.Date)) //we can only override if not exists
						{
							exceptions.Add(dbException.Date, dbException.IsWorkDay);
						}
					}

					calId = calendar.InheritedFrom;
				}

				var curr = startDateInclusive.Date;
				while (curr <= endDateInclusive.Date)
				{
					bool isWorkDay;
					if (!exceptions.TryGetValue(curr, out isWorkDay))
					{
						isWorkDay = IsWorkDayNoExceptions(mainCalendar, curr);
					}
					if (isWorkDay)
					{
						result.Add(curr);
					}

					curr = curr.AddDays(1);
				}

				return result;
			}
		}

		public static int CountDaysBetween(List<DateTime> workingDaysSorted, DateTime startDateInclusive, DateTime endDateInclusive)
		{
			if (startDateInclusive.Date != startDateInclusive || endDateInclusive.Date != endDateInclusive || startDateInclusive > endDateInclusive) throw new ArgumentException();

			var startIdx = workingDaysSorted.BinarySearch(startDateInclusive); //index of first work day
			if (startIdx < 0) startIdx = ~startIdx;

			var endIdx = workingDaysSorted.BinarySearch(endDateInclusive); //index after the last work day
			endIdx = (endIdx < 0) ? ~endIdx : endIdx + 1;

			return endIdx - startIdx;
		}

	}
}
