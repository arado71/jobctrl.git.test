using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Database;

namespace TelemetryConverter.Aggregators
{
	public interface ITableAggregator
	{
		string Category { get; }
		string Name { get; }
		IEnumerable<TableColumn> Columns { get; }
		IEnumerable<object[]> GetRows(SeriesDatabase database, Interval interval, TimeSpan range);
	}
}
