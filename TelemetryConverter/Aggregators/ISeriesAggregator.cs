using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Database;

namespace TelemetryConverter.Aggregators
{
	public interface ISeriesAggregator
	{
		string Category { get; }
		string Name { get; }
		double GetResult(SeriesDatabase database, Interval range);
	}
}
