using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TelemetryConverter.DataSources;

namespace TelemetryConverter.ServerLog
{
	public class ServerLogDataSource : ConverterDataSource<LogEvent, string[]>
	{
		public ServerLogDataSource(string logHeadFilename) : base(new TextAppendDatasource(logHeadFilename))
		{
		}

		protected override IEnumerable<LogEvent> Convert(string[] input)
		{
			var result = new List<LogEvent>();
			foreach (var line in input)
			{
				var m = Regex.Match(line, @"(?<date>\d+\-\d+\-\d+\s+\d+\:\d+\:\d+,\d+)\s+\[(?<thread>.*?)\]\s+(?<severity>\w+)\s+(?<source>.*?)\s+\-\s+(?<message>.*)");
				if (!m.Success)
				{
					if (result.Count > 0)
					{
						result[result.Count - 1].Message += "\n" + line;
					}
					else
					{
						Debug.Fail("Unexpected data");
					}
				}
				else
				{
					DateTime date;
					if (!DateTime.TryParseExact(m.Groups["date"].Value, "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
					{
						Debug.Fail("Unexpected date format");
					}

					var severity = m.Groups["severity"].Value;
					var message = m.Groups["message"].Value;
					var source = m.Groups["source"].Value;
					result.Add(new LogEvent(date, source, message, severity));
				}
			}

			return new LogEvents(result.ToArray());
		}
	}
}
