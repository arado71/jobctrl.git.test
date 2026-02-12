using System;

namespace Tct.ActivityRecorderClient.Telemetry
{
	public class Event : ITelemetryEvent
	{
		public string EventName { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
