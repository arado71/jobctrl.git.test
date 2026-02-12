using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Database;
using TelemetryConverter.Grafana;
using TelemetryConverter.Telemetry;

namespace TelemetryConverter.Aggregators
{
	public class EventTableAggregator : ITableAggregator
	{
		public string Category
		{
			get { return "telemetry"; }
		}

		public virtual string GetName()
		{
			return "Events";
		}

		public string Name
		{
			get { return GetName(); }
		}

		public virtual Predicate<TelemetryEvent> EventPredicate
		{
			get { return b => true; }
		}

		public virtual IEnumerable<TableColumn> Columns
		{
			get
			{
				//yield return new TableColumn("UserId", typeof(string));
				//yield return new TableColumn("Time", typeof(DateTime), TableResult.SortType.Descending);
				//yield return new TableColumn("Name", typeof(string));
				yield return new TableColumn("Action", typeof(string));
				yield return new TableColumn("Count", typeof(int));
			}
		}

		public virtual IEnumerable<object[]> GetRows(SeriesDatabase database, Interval interval, TimeSpan range)
		{
			var raw = database.GetEventTable<TelemetryEvent>().Data;
			var relevantData = raw.CreateFiltered(interval.StartDate, interval.EndDate, x => EventPredicate(x)).GroupBy(d=>d.Parameter).ToList();

			foreach (var relevant in relevantData)
			{
				yield return new object[] { relevant.Key, relevant.Value.Count };
			}
		}
	}
}
