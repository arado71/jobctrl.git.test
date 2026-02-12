using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	public static class CalculatorHelper
	{
		public static DateTime GetLocalReportDate(DateTime utcWorkItemTime, TimeZoneInfo userTimeZoneInfo, TimeSpan startOfDayOffset)
		{
			var localTime = utcWorkItemTime.FromUtcToLocal(userTimeZoneInfo);
			return (localTime - startOfDayOffset).Date;
		}

		internal static StartEndDateTime GetUtcStartEndTimeForReportFromLocalDate(ReportType reportType, DateTime localReportDate, UserStatInfo userStatInfo)
		{
			var localStartEndTime = GetLocalStartEndTimeForReportFromLocalDate(reportType, localReportDate.Date, userStatInfo.StartOfDayOffset);
			var utcStartEndTime = new StartEndDateTime(
				localStartEndTime.StartDate.FromLocalToUtc(userStatInfo.TimeZone)
				, localStartEndTime.EndDate.FromLocalToUtc(userStatInfo.TimeZone));
			return utcStartEndTime;
		}

		internal static StartEndDateTime GetLocalInclusiveDaysForReportFromLocalDay(ReportType reportType, DateTime localReportDate)
		{
			var timeRange = GetLocalStartEndTimeForReportFromLocalDate(reportType, localReportDate, TimeSpan.Zero);
			return new StartEndDateTime(timeRange.StartDate, timeRange.EndDate.AddDays(-1));
		}

		private static StartEndDateTime GetLocalStartEndTimeForReportFromLocalDate(ReportType reportType, DateTime localReportDate, TimeSpan startOfDayOffset)
		{
			var date = localReportDate.Date;
			switch (reportType)
			{
				case ReportType.Daily:
					{
						var start = date + startOfDayOffset;
						var end = start.AddDays(1);
						return new StartEndDateTime(start, end);
					}
				case ReportType.Weekly:
					{
						int dayDiff = date.DayOfWeek - DayOfWeek.Monday;
						if (dayDiff < 0)
						{
							dayDiff += 7;
						}
						var start = date.Date.AddDays(-dayDiff) + startOfDayOffset;
						var end = start.AddDays(7);
						return new StartEndDateTime(start, end);
					}
				case ReportType.Monthly:
					{
						int dayDiff = date.Day - 1;
						var start = date.Date.AddDays(-dayDiff) + startOfDayOffset;
						var end = start.AddMonths(1);
						return new StartEndDateTime(start, end);
					}
				default:
					throw new ArgumentOutOfRangeException("reportType");
			}
		}

		private const float maxPct = 9.999f;
		public static float GetTargetEndDatePct(DateTime startDateParam, DateTime endDateParam, DateTime now)
		{
			var startDate = startDateParam.Date;
			var endDate = endDateParam.Date.AddDays(1); //(StartDate, EndDate) is a date range including EndDate
			if (endDate <= startDate) return maxPct; //don't divide by zero
			var result =
				(now < startDate)
					? 0f
					: (float)((now - startDate).TotalMilliseconds / (endDate - startDate).TotalMilliseconds);
			return Clamp(result, 0, maxPct);
		}

		public static float GetTargetWorkTimePct(TimeSpan targetTotalWorkTime, TimeSpan totalWorkTime)
		{
			if (targetTotalWorkTime <= TimeSpan.Zero) return maxPct; //don't divide by zero, also negative number is error
			var result = (float)(totalWorkTime.TotalMilliseconds / targetTotalWorkTime.TotalMilliseconds);
			return Clamp(result, 0, maxPct);
		}

		public static float Clamp(float value, float minValue, float maxValue)
		{
			return value < minValue
				? minValue
				: value > maxValue
					? maxValue
					: value;
		}
	}
}
