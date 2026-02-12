using System;

namespace JiraSyncTool.Jira.Utils
{
	public static class ProjHelper
	{
		public static DateTime? GetDate(this DateTime? dateTime)
		{
			if (dateTime == null) return null;
			return dateTime.Value.Date;
		}

		public static TimeSpan? GetDuration(int? durationInMinutes)
		{
			if (durationInMinutes == null) return null;
			return TimeSpan.FromMinutes(durationInMinutes.Value);
		}

		public static TimeSpan? GetDuration(double? durationInHours)
		{
			if (durationInHours == null) return null;
			return TimeSpan.FromHours(durationInHours.Value);
		}

		public static TimeSpan? GetDuration(TimeSpan? timespan)
		{
			if (timespan == null) return null;
			return TimeSpan.FromMinutes((long)timespan.Value.TotalMinutes);
		}

		public static int? GetMinutes(TimeSpan? duration)
		{
			if (duration == null) return null;
			return (int?)duration.Value.TotalMinutes;
		}

		public static TimeSpan GetWorkDuration(Decimal time)
		{
			return TimeSpan.FromMinutes(Convert.ToDouble(time) / 10.0);
		}
	}
}
