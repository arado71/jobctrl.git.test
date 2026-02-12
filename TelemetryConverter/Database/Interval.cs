using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter.Database
{
	public class Interval : IInterval
	{
		public DateTime StartDate { get; private set; }
		public DateTime EndDate { get; private set; }

		public Interval(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}

		public Interval Intersect(IInterval other)
		{
			if (other.EndDate <= StartDate || other.StartDate >= EndDate) return null;
			return new Interval(new DateTime(Math.Max(StartDate.Ticks, other.StartDate.Ticks)), new DateTime(Math.Min(EndDate.Ticks, other.EndDate.Ticks)));
		}

		public IEnumerable<Interval> Slice(TimeSpan sliceLength)
		{
			var currentEnd = EndDate;
			var currentStart = currentEnd - sliceLength;
			while (currentStart > StartDate)
			{
				yield return new Interval(currentStart, currentEnd);
				currentEnd = currentStart;
				currentStart = currentEnd - sliceLength;
			}
		}
	}

	public class Interval<T> : Interval
	{
		public T Value { get; private set; }

		public Interval(DateTime startDate, DateTime endDate, T value) : base(startDate, endDate)
		{
			Value = value;
		}

		public Interval(Interval interval, T value)
			: this(interval.StartDate, interval.EndDate, value)
		{
		}
	}
}
