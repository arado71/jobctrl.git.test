using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TelemetryConverter.Database;

namespace TelemetryConverter
{
	public class LogEvent : IEvent
	{
		public DateTime Timestamp { get; set; }
		public string Source { get; set; }
		public string Message { get; set; }
		public string Severity { get; set; }

		public LogEvent(DateTime timestamp, string source, string message, string severity)
		{
			Timestamp = timestamp;
			Source = source;
			Message = message;
			Severity = severity;
		}
	}
}
