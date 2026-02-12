using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	/// <summary>
	/// Thread-safe class for describing date ranges as free text
	/// </summary>
	/// <remarks>
	/// This class is not really portable in this way
	/// </remarks>
	public class DateRangeFormatter
	{
		#region Dictionaries
		private static readonly Dictionary<string, string> englishLongDict = new Dictionary<string, string>()
		{
			{"PartSeparator", ", "},
			{"NumberSeparator", " "},
			{DateRangeType.Millisecond+"Singular","millisecond"},
			{DateRangeType.Millisecond+"Plural","milliseconds"},
			{DateRangeType.Second+"Singular","second"},
			{DateRangeType.Second+"Plural","seconds"},
			{DateRangeType.Minute+"Singular","minute"},
			{DateRangeType.Minute+"Plural","minutes"},
			{DateRangeType.Hour+"Singular","hour"},
			{DateRangeType.Hour+"Plural","hours"},
			{DateRangeType.Day+"Singular","day"},
			{DateRangeType.Day+"Plural","days"},
			{DateRangeType.Week+"Singular","week"},
			{DateRangeType.Week+"Plural","weeks"},
			{DateRangeType.Month+"Singular","month"},
			{DateRangeType.Month+"Plural","months"},
			{DateRangeType.Year+"Singular","year"},
			{DateRangeType.Year+"Plural","years"},
		};

		private static readonly Dictionary<string, string> hungarianMixedDict = new Dictionary<string, string>()
		{
			{"PartSeparator", ", "},
			{"NumberSeparator", " "},
			{DateRangeType.Millisecond+"Singular","ms"},
			{DateRangeType.Millisecond+"Plural","ms"},
			{DateRangeType.Second+"Singular","mp"},
			{DateRangeType.Second+"Plural","mp"},
			{DateRangeType.Minute+"Singular","perc"},
			{DateRangeType.Minute+"Plural","perc"},
			{DateRangeType.Hour+"Singular","óra"},
			{DateRangeType.Hour+"Plural","óra"},
			{DateRangeType.Day+"Singular","nap"},
			{DateRangeType.Day+"Plural","nap"},
			{DateRangeType.Week+"Singular","hét"},
			{DateRangeType.Week+"Plural","hét"},
			{DateRangeType.Month+"Singular","hónap"},
			{DateRangeType.Month+"Plural","hónap"},
			{DateRangeType.Year+"Singular","év"},
			{DateRangeType.Year+"Plural","év"},
		};
		#endregion

		public static DateRangeFormatter EnglishLong = new DateRangeFormatter(englishLongDict);
		public static DateRangeFormatter HungarianMixed = new DateRangeFormatter(hungarianMixedDict);

		public static DateRangeFormatter Current
		{
			get
			{
				// TODO extend to multiple langs & move strings to resources
				if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.ToLower() == "hu-hu")
					return HungarianMixed;
				return EnglishLong;
			}
		}

		private readonly Dictionary<string, string> currentDict;

		private DateRangeFormatter(Dictionary<string, string> currDict)
		{
			currentDict = currDict;
		}

		public string GetApproxRangeString(double milliseconds)
		{
			return GetApproxRangeString(TimeSpan.FromMilliseconds(milliseconds));
		}

		public string GetApproxRangeString(DateTime startDate, DateTime endDate)
		{
			return GetApproxRangeString(endDate - startDate);
		}

		public string GetApproxRangeString(TimeSpan duration)
		{
			var ms = (long)(duration.TotalMilliseconds + 0.5);
			if (duration < TimeSpan.Zero) return null;

			//Ranges: (inclusive - exclusive)
			//0 ms - 750 ms - ms (round to nearest ms)
			//750ms - 90s - s  (round to nearest second)
			//90s - 90m - m    (round to nearest minute)
			//90m - 18h - h    (round to nearest hour)
			//18h - 14d - d    (round to nearest day)
			//14d - 10w - w    (round to nearest week)
			//10w - 12m - m    (round to nearest month) - needs date data in theory
			//12m -     - y, m (round to nearest month) - needs date data in theory

			if (ms < 750)
			{
				return FormatSingleDateRange(RoundToNearest(ms, DateRangeType.Millisecond), DateRangeType.Millisecond);
			}
			else if (ms < 90 * OneSecInMs) //90s
			{
				return FormatSingleDateRange(RoundToNearest(ms, DateRangeType.Second), DateRangeType.Second);
			}
			else if (ms < 90 * OneMinInMs) //90m
			{
				return FormatSingleDateRange(RoundToNearest(ms, DateRangeType.Minute), DateRangeType.Minute);
			}
			else if (ms < 18 * OneHourInMs) //18h
			{
				return FormatSingleDateRange(RoundToNearest(ms, DateRangeType.Hour), DateRangeType.Hour);
			}
			else if (ms < 14 * OneDayInMs) //14d
			{
				return FormatSingleDateRange(RoundToNearest(ms, DateRangeType.Day), DateRangeType.Day);
			}
			else if (ms < 10 * OneWeekInMs) //10w
			{
				return FormatSingleDateRange(RoundToNearest(ms, DateRangeType.Week), DateRangeType.Week);
			}
			else
			{
				var months = RoundToNearest(ms, DateRangeType.Month);
				return months < 12
					? FormatSingleDateRange(months, DateRangeType.Month)
					: FormatDoubleDateRange(months / 12, DateRangeType.Year, months % 12, DateRangeType.Month);
			}
		}

		private const double daysInYear = 365.2425;
		private const double avgDaysInMonth = daysInYear / 12;
		//this is not accurate and doesn't consider days in month which is not ideal but I can live with that (we won't have date information for timespans anyway)
		private static int RoundToNearestMonth(double milliseconds)
		{
			var days = milliseconds / OneDayInMs;
			return (int)Math.Round(days / avgDaysInMonth);
		}

		private const long OneSecInMs = 1000;
		private const long OneMinInMs = 60 * OneSecInMs;
		private const long OneHourInMs = 60 * OneMinInMs;
		private const long OneDayInMs = 24 * OneHourInMs;
		private const long OneWeekInMs = 7 * OneDayInMs;
		private static int RoundToNearest(long srcMs, DateRangeType rangeType)
		{
			switch (rangeType)
			{
				case DateRangeType.Millisecond:
					return (int)srcMs;
				case DateRangeType.Second:
					return (int)((srcMs + OneSecInMs / 2) / OneSecInMs);
				case DateRangeType.Minute:
					return (int)((srcMs + OneMinInMs / 2) / OneMinInMs);
				case DateRangeType.Hour:
					return (int)((srcMs + OneHourInMs / 2) / OneHourInMs);
				case DateRangeType.Day:
					return (int)((srcMs + OneDayInMs / 2) / OneDayInMs);
				case DateRangeType.Week:
					return (int)((srcMs + OneWeekInMs / 2) / OneWeekInMs);
				case DateRangeType.Month:
					return RoundToNearestMonth(srcMs);
				case DateRangeType.Year:
					return (RoundToNearestMonth(srcMs) + 6) / 12;
				default:
					throw new ArgumentOutOfRangeException("rangeType");
			}
		}

		private string FormatSingleDateRange(int number, DateRangeType rangeType)
		{
			return number + currentDict["NumberSeparator"] + (number == 1 ? currentDict[rangeType + "Singular"] : currentDict[rangeType + "Plural"]);
		}

		private string FormatDoubleDateRange(int number1, DateRangeType rangeType1, int number2, DateRangeType rangeType2)
		{
			return
				number1 == 0
				? FormatSingleDateRange(number2, rangeType2)
				: number2 == 0
					? FormatSingleDateRange(number1, rangeType1)
					: FormatSingleDateRange(number1, rangeType1) + currentDict["PartSeparator"] + FormatSingleDateRange(number2, rangeType2);
		}

		private enum DateRangeType
		{
			Millisecond = 0,
			Second,
			Minute,
			Hour,
			Day,
			Week,
			Month,
			Year,
		}
	}
}
