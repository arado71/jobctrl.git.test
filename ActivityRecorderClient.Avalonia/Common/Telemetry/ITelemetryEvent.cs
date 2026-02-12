using System;

namespace Tct.ActivityRecorderClient.Telemetry
{
	public interface ITelemetryEvent
	{
		string EventName { get; set; }
		DateTime Timestamp { get; set; }
	}
}
