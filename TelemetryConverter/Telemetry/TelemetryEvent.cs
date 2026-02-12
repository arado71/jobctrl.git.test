using System;
using TelemetryConverter.Database;

namespace TelemetryConverter.Telemetry
{
	[Serializable]
	public class TelemetryEvent : IEvent
	{
		public int UserId { get; private set; }
		public int ComputerId { get; private set; }
		public DateTime Timestamp { get; private set; }
		public string Name { get; private set; }
		public string Parameter { get; private set; }

		public TelemetryEvent(int userId, int computerId, DateTime timestamp, string name, string parameter)
		{
			UserId = userId;
			ComputerId = computerId;
			Timestamp = timestamp;
			Name = name;
			Parameter = parameter;
		}

		public override string ToString()
		{
			return "[" + UserId + "/" + ComputerId + "] " + Timestamp + " " + Name + " - " + Parameter;
		}
	}
}
