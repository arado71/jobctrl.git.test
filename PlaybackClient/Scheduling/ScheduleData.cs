using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Scheduling
{
	[Serializable]
	public class ScheduleData //todo implement INotifyPropertyChanged so we can use it on the server side in linq?
	{
		[DataMember(Order = 1)]
		public ScheduleType Type { get; set; }
		[DataMember(Order = 2)]
		public DateTime StartDate { get; set; }
		[DataMember(Order = 3)]
		public DateTime? EndDate { get; set; }
		[DataMember(Order = 4)]
		public int Interval { get; set; }
		[DataMember(Order = 5)]
		public DaysOfWeek WeeklyEffectiveDays { get; set; }
		[DataMember(Order = 6)]
		public DayOfWeek FirstDayOfWeek { get; set; } //firstDayOfWeek in weekly schedule
		[DataMember(Order = 7)]
		public int MonthlyEffectiveDay { get; set; } //day number in monthly schedule

		public string Description { get { return this.ToString(); } }

		public Schedule CreateSchedule()
		{
			switch (Type)
			{
				case ScheduleType.OneTime:
					return Schedule.CreateOneTime(StartDate);
				case ScheduleType.EvenInterval:
					return Schedule.CreateForEvenInterval(StartDate, EndDate, TimeSpan.FromMilliseconds(Interval));
				case ScheduleType.Daily:
					return Schedule.CreateDaily(StartDate, EndDate, Interval);
				case ScheduleType.Weekly:
					return Schedule.CreateWeekly(StartDate, EndDate, Interval, WeeklyEffectiveDays, FirstDayOfWeek);
				case ScheduleType.Monthly:
					return Schedule.CreateMonthly(StartDate, EndDate, Interval, MonthlyEffectiveDay);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override string ToString()
		{
			try
			{
				return CreateSchedule().ToString();
			}
			catch
			{
				return "Invalid ScheduleData";
			}
		}
	}
}
