using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Database;
using TelemetryConverter.Telemetry;

namespace TelemetryConverter.Aggregators
{
	class VersionDataAggregator : EventTableAggregator
	{
		public override string GetName()
		{
			return "JC Version";
		}

		//public override IEnumerable<TableColumn> Columns
		//{
		//	get
		//	{
		//		var r = base.Columns.ToList();
		//		r.Add(new TableColumn("UserId", typeof(string)));
		//		return r;
		//	}
		//}

		public override IEnumerable<object[]> GetRows(SeriesDatabase database, Interval interval, TimeSpan range)
		{
			var raw = database.GetEventTable<TelemetryEvent>().Data;
			var relevantData = raw.CreateFiltered(interval.StartDate, interval.EndDate, x => EventPredicate(x)).GroupBy(d => d.Parameter).ToList();
			var groupedData = relevantData.ToDictionary(k => k.Key,
				k => k.Value.GroupBy(t => t.UserId).Values.Count);
			foreach (var relevant in groupedData)
			{
				yield return new object[] { relevant.Key, relevant.Value };
			}
		}

		public override Predicate<TelemetryEvent> EventPredicate
		{
			get { return x => x.Name == "JcVersion"; }
		}
	}
}
