using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.View
{
	public static class FormatHelper
	{
		public static string GetDays(TimeSpan timeSpan)
		{
			var days = timeSpan.GetDays();
			return string.Format("{0} {1}", days, GetForm(days, Labels.Day, Labels.DayPlural));
		}

		private static string GetForm(int qty, string singular, string plural, params object[] args)
		{
			return string.Format(qty != 1 ? plural : singular, args);
		}

		public static string GetRemainingTime(TimeSpan time)
		{
			if (time.Ticks > 0)
			{
				if (time.TotalHours < 24)
				{
					var hours = (int)Math.Floor(time.TotalHours);
					return string.Format(Labels.Left, string.Format("{0} {1}", hours, GetForm(hours, Labels.HourSingular, Labels.HourPlural)));
				}
				else
				{
					var days = time.GetDays();
					return string.Format(Labels.Left, string.Format("{0} {1}", days, GetForm(days, Labels.Day, Labels.DayPlural)));
				}
			}
			else
			{
				var days = time.GetDays();
				return GetForm(-days, Labels.OverdueSingular, Labels.OverduePlural, -days);
			}
		}

		public static string GetDesc(NavigationBase navigation)
		{
			if (navigation == null) return string.Empty;
			return (string.Join(WorkDataWithParentNames.DefaultSeparator, navigation.Path.ToArray()) + WorkDataWithParentNames.DefaultSeparator + navigation.Name) + Environment.NewLine
				+ (navigation.Priority.HasValue ? " " + Labels.WorkData_Priority + ": " + navigation.Priority : string.Empty)
				+ (navigation.StartDate.HasValue ? " " + Labels.WorkData_StartDate + ": " + navigation.StartDate.Value.ToShortDateString() : string.Empty)
				+ (navigation.EndDate.HasValue ? " " + Labels.WorkData_EndDate + ": " + navigation.EndDate.Value.ToShortDateString() : string.Empty) + Environment.NewLine
				+ (" " + Labels.WorkData_WorkedHours + ": " + navigation.UsedTime.ToHourMinuteString())
				+ (navigation.TotalTime.HasValue ? " " + Labels.WorkData_TargetHours + ": " + navigation.TotalTime.Value.ToHourMinuteString() : string.Empty);
		}
	}
}
