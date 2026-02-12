using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Database;
using TelemetryConverter.Grafana;

namespace TelemetryConverter.Aggregators
{
	public class ErrorTableAggregator : ITableAggregator
	{
		public string Category
		{
			get { return "server"; }
		}
		public string Name
		{
			get { return "Error list"; }
		}

		public IEnumerable<TableColumn> Columns
		{
			get
			{
				yield return new TableColumn("Time", typeof(DateTime), TableResult.SortType.Descending);
				yield return new TableColumn("Source", typeof(string));
				yield return new TableColumn("Message", typeof(string));
			}
		}

		public IEnumerable<object[]> GetRows(SeriesDatabase database, Interval interval, TimeSpan range)
		{
			var raw = database.GetEventTable<LogEvent>().Data;
			var relevantData = raw.CreateFiltered(interval.StartDate, interval.EndDate,
				x => string.Equals(x.Severity, "error", StringComparison.InvariantCultureIgnoreCase)).ToList();
			foreach (var relevant in relevantData)
			{
				yield return new object[] { relevant.Timestamp, relevant.Source.Replace("Tct.ActivityRecorderService.", ""), relevant.Message };
			}
		}
	}
}
