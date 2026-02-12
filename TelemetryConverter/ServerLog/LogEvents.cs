using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter
{
	public class LogEvents : IEnumerable<LogEvent>
	{
		public LogEvent[] Events { get; set; }

		public LogEvents(LogEvent[] events)
		{
			this.Events = events;
		}

		public IEnumerator<LogEvent> GetEnumerator()
		{
			return Events.ToList().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Events.GetEnumerator();
		}
	}
}
