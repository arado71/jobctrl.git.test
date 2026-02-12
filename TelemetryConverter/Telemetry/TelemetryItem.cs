using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TelemetryConverter.Database;

namespace TelemetryConverter.Telemetry
{
	public class TelemetryItem : IEnumerable<TelemetryEvent>, IInterval
	{
		public int UserId { get; set; }
		public int ComputerId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public TelemetryEvent[] Events { get; set; }
		IEnumerator<TelemetryEvent> IEnumerable<TelemetryEvent>.GetEnumerator()
		{
			return Events.ToList().GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return Events.GetEnumerator();
		}
	}
}
