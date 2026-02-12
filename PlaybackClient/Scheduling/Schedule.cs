using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Scheduling
{
	public class Schedule
	{
		public static readonly Schedule Never = new Schedule(Enumerable.Empty<DateTime>(), "Never occurs");

		private Schedule(IEnumerable<DateTime> dateSequence, string description)
		{
			MaxDate = new DateTime(3000, 1, 1); //don't generate infinite sequences
			this.dateSequence = dateSequence;
			this.description = description;
		}

		public DateTime MaxDate { get; set; }
		private readonly IEnumerable<DateTime> dateSequence; //possible infinite sequence of dates
		private readonly string description;

		public IEnumerable<DateTime> GetOccurances()
		{
			return GetOccurancesBetween(DateTime.MinValue, MaxDate);
		}

		public IEnumerable<DateTime> GetOccurancesAfter(DateTime from)
		{
			return GetOccurancesBetween(from, MaxDate);
		}

		public IEnumerable<DateTime> GetOccurancesBetween(DateTime fromExclusive, DateTime toExclusive)
		{
			return dateSequence
				.SkipWhile(n => n <= fromExclusive)
				.TakeWhile(n => n < toExclusive);
		}

		public static Schedule CreateOneTime(DateTime startDate)
		{
			var dateSeq = Generate(startDate, _ => null);
			return new Schedule(dateSeq, "Occurs once at " + startDate);
		}

		public static Schedule CreateForEvenInterval(DateTime startDate, DateTime? endDate, TimeSpan interval)
		{
			if (interval <= TimeSpan.Zero) throw new ArgumentException("interval");
			if (endDate.HasValue && endDate.Value < startDate) throw new ArgumentException("endDate");
			var dateSeq = Generate(startDate, date =>
			{
				var result = date + interval;
				if (endDate.HasValue && endDate < result)
				{
					return null;
				}
				return result;
			});
			return new Schedule(dateSeq, "Occurs after every " + interval + " interval. " + GetIntervalDescription(startDate, endDate));
		}

		public static Schedule CreateDaily(DateTime startDate, DateTime? endDate, int repeatEveryNDays)
		{
			if (repeatEveryNDays <= 0) throw new ArgumentException("repeatEveryNDays");
			if (endDate.HasValue && endDate.Value < startDate) throw new ArgumentException("endDate");
			var dateSeq = Generate(startDate, date =>
			{
				var result = date.AddDays(repeatEveryNDays);
				if (endDate.HasValue && endDate < result)
				{
					return null;
				}
				return result;
			});
			return new Schedule(dateSeq, "Occurs every " + (repeatEveryNDays == 1 ? "day" : repeatEveryNDays + " days") + " at " + startDate.TimeOfDay + ". " + GetIntervalDescription(startDate, endDate));
		}

		public static Schedule CreateWeekly(DateTime startDate, DateTime? endDate, int repeatEveryNWeeks)
		{
			return CreateWeekly(startDate, endDate, repeatEveryNWeeks, DaysOfWeek.None, DayOfWeek.Monday);
		}

		public static Schedule CreateWeekly(DateTime startDate, DateTime? endDate, int repeatEveryNWeeks, DaysOfWeek specificDays, DayOfWeek firstDayOfWeek)
		{
			if (repeatEveryNWeeks <= 0) throw new ArgumentException("repeatEveryNWeeks");
			if (endDate.HasValue && endDate.Value < startDate) throw new ArgumentException("endDate");
			if (specificDays < DaysOfWeek.None || specificDays > DaysOfWeek.EveryDay) throw new ArgumentException("specificDays");
			if (specificDays == DaysOfWeek.None)
			{
				var dateSeq = Generate(startDate, date =>
				{
					var result = date.AddDays(repeatEveryNWeeks * 7);
					if (endDate.HasValue && endDate < result)
					{
						return null;
					}
					return result;
				});
				return new Schedule(dateSeq, "Occurs every " + (repeatEveryNWeeks == 1 ? "week" : repeatEveryNWeeks + " weeks") + " at " + startDate.TimeOfDay + ". " + GetIntervalDescription(startDate, endDate));
			}
			else
			{
				//new GregorianCalendar().GetWeekOfYear(startDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
				var dateSeq = CreateWeeklySpecificDaysDateSequence(startDate, endDate, repeatEveryNWeeks, specificDays, firstDayOfWeek);
				return new Schedule(dateSeq, "Occurs every " + (repeatEveryNWeeks == 1 ? "week" : repeatEveryNWeeks + " weeks") + " on " + specificDays + " at " + startDate.TimeOfDay + ". " + GetIntervalDescription(startDate, endDate));
			}
		}

		private static IEnumerable<DateTime> CreateWeeklySpecificDaysDateSequence(DateTime startDate, DateTime? endDate, int repeatEveryNWeeks, DaysOfWeek specificDays, DayOfWeek firstDayOfWeek)
		{
			if (specificDays <= DaysOfWeek.None || specificDays > DaysOfWeek.EveryDay) throw new ArgumentException("specificDays");
			int dayDiff = startDate.DayOfWeek - firstDayOfWeek;
			if (dayDiff < 0)
			{
				dayDiff += 7;
			}
			var startOfWeek = startDate.AddDays(-dayDiff);
			for (var result = startDate; ; result = result.AddDays(1))
			{
				int weeksFromStartDate = (result - startOfWeek).Days / 7;

				if (endDate.HasValue && endDate < result)
				{
					yield break;
				}
				if (weeksFromStartDate % repeatEveryNWeeks != 0) continue;
				switch (result.DayOfWeek)
				{
					case DayOfWeek.Sunday:
						if ((specificDays & DaysOfWeek.Sunday) != 0) yield return result;
						break;
					case DayOfWeek.Monday:
						if ((specificDays & DaysOfWeek.Monday) != 0) yield return result;
						break;
					case DayOfWeek.Tuesday:
						if ((specificDays & DaysOfWeek.Tuesday) != 0) yield return result;
						break;
					case DayOfWeek.Wednesday:
						if ((specificDays & DaysOfWeek.Wednesday) != 0) yield return result;
						break;
					case DayOfWeek.Thursday:
						if ((specificDays & DaysOfWeek.Thursday) != 0) yield return result;
						break;
					case DayOfWeek.Friday:
						if ((specificDays & DaysOfWeek.Friday) != 0) yield return result;
						break;
					case DayOfWeek.Saturday:
						if ((specificDays & DaysOfWeek.Saturday) != 0) yield return result;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public static Schedule CreateMonthly(DateTime startDate, DateTime? endDate, int repeatEveryNMonth)
		{
			return CreateMonthly(startDate, endDate, repeatEveryNMonth, startDate.Day);
		}

		public static Schedule CreateMonthly(DateTime startDate, DateTime? endDate, int repeatEveryNMonth, int day)
		{
			if (repeatEveryNMonth <= 0) throw new ArgumentException("repeatEveryNMonth");
			if (endDate.HasValue && endDate.Value < startDate) throw new ArgumentException("endDate");
			if (day < 1 || day > 31) throw new ArgumentException("day");
			var dateSeq = CreateMonthlySpecificDayDateSequence(startDate, endDate, repeatEveryNMonth, day);
			return new Schedule(dateSeq, "Occurs every " + (repeatEveryNMonth == 1 ? "month" : repeatEveryNMonth + " months") + " on day " + day + (day > 28 ? " (or last day of the month)" : "") + " at " + startDate.TimeOfDay + ". " + GetIntervalDescription(startDate, endDate));
		}

		private static IEnumerable<DateTime> CreateMonthlySpecificDayDateSequence(DateTime startDate, DateTime? endDate, int repeatEveryNMonth, int day)
		{
			if (repeatEveryNMonth <= 0) throw new ArgumentException("repeatEveryNMonth");
			if (endDate.HasValue && endDate.Value < startDate) throw new ArgumentException("endDate");
			if (day < 1 || day > 31) throw new ArgumentException("day");
			//quite ugly code :(
			//we have to find the first date where the day part equals to the parameter, so we can use AddMonth to generate the sequence
			//this is needed because ((Jan 31 + 1 month) + 1 month) = Marc 28 (or 29) and not Marc 31
			var firstMonthWithEnoughDays = startDate;
			var monthModifier = firstMonthWithEnoughDays.Day > day ? 1 : 0;
			while (firstMonthWithEnoughDays.Day != day)
			{
				var oldMonth = firstMonthWithEnoughDays.Month;
				firstMonthWithEnoughDays = firstMonthWithEnoughDays.AddDays(1);
				if (oldMonth != firstMonthWithEnoughDays.Month) monthModifier--;
			}

			while (true)
			{
				var result = firstMonthWithEnoughDays.AddMonths(monthModifier);
				monthModifier += repeatEveryNMonth;
				if (endDate.HasValue && endDate.Value < result)
				{
					yield break;
				}
				yield return result;
			}
		}

		//todo Penultimate day of the month?
		//todo Second Monday of the month?
		//public static Schedule CreateMonthly(DateTime startDate, DateTime? endDate, int repeatEveryNMonth, RelativeRecurrence relativeRecurrence, DayOfWeek dayOfWeek)
		//{
		//    if (repeatEveryNMonth <= 0) throw new ArgumentException("repeatEveryNMonth");
		//    if (relativeRecurrence == RelativeRecurrence.None) throw new ArgumentException("relativeRecurrence");
		//    if (endDate.HasValue && endDate.Value < startDate) throw new ArgumentException("endDate");
		//    return null;
		//}

		public override string ToString()
		{
			return description;
		}

		//todo flags ?
		//public enum RelativeRecurrence
		//{
		//    None = 0,
		//    First,
		//    Second,
		//    Third,
		//    Fourth,
		//    Antepenultimate,
		//    Penultimate,
		//    Last
		//}

		private static IEnumerable<T> Generate<T>(T initial, Func<T, T?> next)
			where T : struct
		{
			T? val = initial;
			while (val.HasValue)
			{
				yield return val.Value;
				val = next(val.Value);
			}
		}

		private static string GetIntervalDescription(DateTime startDate, DateTime? endDate)
		{
			return endDate.HasValue
				? "Schedule will be used between " + startDate + " and " + endDate.Value + "."
				: "Schedule will be used starting on " + startDate + ".";
		}
	}
}
