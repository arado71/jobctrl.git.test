using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Database;
using TelemetryConverter.Telemetry;

namespace TelemetryConverter.Aggregators
{
	public class TelemetryUploadCount : ISeriesAggregator
	{
		public string Category { get { return "telemetry"; } }
		public string Name { get { return "Upload count"; } }
		public double GetResult(SeriesDatabase database, Interval range)
		{
			var rawData = database.GetIntervalTable<TelemetryItem>().Data;
			var flattened = rawData.Flatten((items, interval) => new Interval<int>(interval.StartDate, interval.EndDate, items.Count()));
			var ranged = flattened.EnumerateContains(range).ToArray();
			if (ranged.Length == 0) return 0.0;
			var max = ranged.Max(x => x.Value);
			return max;
		}
	}
}
