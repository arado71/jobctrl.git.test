using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TelemetryConverter.Database;

namespace TelemetryConverter.Aggregators
{
	public class EventCount<T> : ISeriesAggregator
		where T : IEvent
	{
		public string Category { get; private set; }
		public string Name { get; private set; }

		public EventCount(string category, string name)
		{
			Name = name;
			Category = category;
		}

		public double GetResult(SeriesDatabase database, Interval range)
		{
			return database.GetEventTable<T>().Data.CountBetween(range.StartDate, range.EndDate);
		}
	}
}
