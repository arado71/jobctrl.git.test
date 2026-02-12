using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Database;
using TelemetryConverter.Telemetry;

namespace TelemetryConverter.Aggregators
{
	public class GenericSeriesAggregator<T> : ISeriesAggregator
		where T : IEvent
	{
		public string Category
		{
			get {  return CustomCategory; }
		}

		public string CustomCategory;

		public string Name
		{
			get { return CustomName; }
		}
		public string CustomName;

		public Predicate<T> EventPredicate;
		public double GetResult(Database.SeriesDatabase database, Database.Interval range)
		{
			var isTel =  typeof(T) == typeof(TelemetryEvent);
			var additionalParameters = CustomName.Split('|');
			//var x =
			//	database.GetEventTable<T>()
			//		.Data.OfType<TelemetryEvent>()
			//		.ToArray()
			//		.Where(d => d.Name == "Feature")
			//		.Select(d => d.Parameter);
			return database.GetEventTable<T>().Data.ToList().Where(d => d.Timestamp >= range.StartDate && d.Timestamp <= range.EndDate && (isTel ? (d as TelemetryEvent).Name == additionalParameters[0] : true) && (isTel ? (d as TelemetryEvent).Parameter == additionalParameters[1] : true)).LongCount();
		}
	}
}
