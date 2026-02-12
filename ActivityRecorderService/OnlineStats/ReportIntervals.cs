using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;

namespace Tct.ActivityRecorderService.OnlineStats
{
	public struct ReportIntervals
	{
		public readonly DateTime UtcNow;
		public readonly DateTime LocalReportDate;
		public readonly StartEndDateTime Today;
		public readonly StartEndDateTime ThisWeek;
		public readonly StartEndDateTime ThisMonth;
		public readonly StartEndDateTime ThisWeekLocalDay;
		public readonly StartEndDateTime ThisMonthLocalDay;

		public ReportIntervals(DateTime utcNow, UserStatInfo userStatInfo)
		{
			UtcNow = utcNow;
			LocalReportDate = CalculatorHelper.GetLocalReportDate(utcNow, userStatInfo.TimeZone, userStatInfo.StartOfDayOffset);
			Today = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, LocalReportDate, userStatInfo);
			ThisWeek = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Weekly, LocalReportDate, userStatInfo);
			ThisMonth = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Monthly, LocalReportDate, userStatInfo);
			ThisWeekLocalDay = CalculatorHelper.GetLocalInclusiveDaysForReportFromLocalDay(ReportType.Weekly, LocalReportDate);
			ThisMonthLocalDay = CalculatorHelper.GetLocalInclusiveDaysForReportFromLocalDay(ReportType.Monthly, LocalReportDate);
		}
	}
}
