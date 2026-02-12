using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.DataSources;

namespace TelemetryConverter.Telemetry
{
	public class TelemetryEventDataSource : ConverterDataSource<TelemetryEvent, TelemetryItem>
	{
		public TelemetryEventDataSource(IDataSource<TelemetryItem> baseDataSource) : base(baseDataSource)
		{
		}

		protected override IEnumerable<TelemetryEvent> Convert(TelemetryItem input)
		{
			return input.Events;
		}
	}
}
