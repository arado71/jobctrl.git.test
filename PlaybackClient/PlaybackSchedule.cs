using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduling;
using log4net;

namespace PlaybackClient
{
	public partial class PlaybackSchedule
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public TimeZoneInfo TimeZone { get { return TimeZoneId == null ? TimeZoneInfo.Utc : TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId); } }

		private Schedule localSchedule;
		public Schedule LocalSchedule
		{
			set
			{
				localSchedule = value;
			}
			get
			{
				try
				{
					return localSchedule ?? new ScheduleData()
					{
						Type = (ScheduleType)ScheduleType,
						StartDate = FirstScheduleDate.FromUtcToLocal(TimeZone),
						EndDate = LastScheduleDate.FromUtcToLocal(TimeZone),
						FirstDayOfWeek = (DayOfWeek)FirstDayOfWeek.GetValueOrDefault(0),
						Interval = Interval.GetValueOrDefault(0),
						MonthlyEffectiveDay = MonthlyEffectiveDay.GetValueOrDefault(0),
						WeeklyEffectiveDays = (DaysOfWeek)WeeklyEffectiveDays.GetValueOrDefault(0),
					}.CreateSchedule();
				}
				catch (Exception ex)
				{
					log.Error("Unable to get LocalSchedule", ex);
					return Schedule.Never;
				}
			}
		}

		public bool TryGetNextUtcStartDate(DateTime fromUtcExclusive, out DateTime utcStartDate)
		{
			utcStartDate = LocalSchedule.GetOccurances()
				.Select(n => n.FromLocalToUtc(TimeZone))
				.SkipWhile(n => n <= fromUtcExclusive)
				.FirstOrDefault();
			return utcStartDate != DateTime.MinValue;
		}

		public override string ToString()
		{
			return "PlaybackSchedule userId: " + UserId + " datastart: " + StartDate + " dataend: " + EndDate + " first: " + FirstScheduleDate + " schedule: " + LocalSchedule;
		}
	}
}
