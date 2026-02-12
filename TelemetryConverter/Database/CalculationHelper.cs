using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;

namespace TelemetryConverter.Database
{
	public static class CalculationHelper
	{
		public static TimeSpan GetAge(this IEvent @event)
		{
			if (@event is IInterval)
			{
				var interval = (IInterval) @event;
				return DateTime.UtcNow - interval.EndDate;
			}

			return DateTime.UtcNow - @event.Timestamp;
		}

		public static bool Contains<T>(this IInterval interval, DateTime time) where T : IInterval
		{
			return interval.StartDate <= time && interval.EndDate < time;
		}

		public static int GetIntervalCount<T>(this DateOrderedList<T> list) where T : IInterval
		{
			return GetConcatenator(list).GetIntervals().Count;
		}

		public static TimeSpan GetDuration<T>(this DateOrderedList<T> list) where T : IInterval
		{
			return GetConcatenator(list).Duration();
		}

		public static IntervalConcatenator GetConcatenator<T>(DateOrderedList<T> list) where T : IInterval 
		{
			var concatenator = new IntervalConcatenator();
			foreach (var element in list)
			{
				concatenator.Add(element.StartDate, element.EndDate);
			}
			return concatenator;
		}

	}
}
