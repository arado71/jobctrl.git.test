using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter
{
	public class TimeSeriesElement
	{
		public double Value { get; private set; }
		public DateTime Timestamp { get; private set; }

		public TimeSeriesElement(DateTime timestamp, double value)
		{
			Value = value;
			Timestamp = timestamp;
		}
	}
}
