using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Database;

namespace TelemetryConverter.Aggregators
{
	public class ServerErrorCount : ISeriesAggregator
	{
		public string Category { get { return "server"; } }
		public string Name { get { return "Error count"; } }
		public double GetResult(SeriesDatabase database, Interval range)
		{
			var raw = database.GetEventTable<LogEvent>();
			return raw.Data.CreateFiltered(range.StartDate, range.EndDate, x => string.Equals(x.Severity, "error", StringComparison.InvariantCultureIgnoreCase)).Count;
		}
	}
}
