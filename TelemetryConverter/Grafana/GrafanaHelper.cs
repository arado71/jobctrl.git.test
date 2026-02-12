using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TelemetryConverter.Grafana
{
	public static class GrafanaHelper
	{
		public static TimeSpan ConvertTimespan(string representation)
		{
			var m = Regex.Match(representation, "([0-9]+)(.*)");
			if (!m.Success)
			{
				throw new NotImplementedException();
			}

			var num = int.Parse(m.Groups[1].Value);
			var postfix = m.Groups[2].Value;
			switch (postfix)
			{
				case "ms":
					return new TimeSpan(0, 0, 0, 0, num);
				case "s":
					return new TimeSpan(0, 0, num);
				case "m":
					return new TimeSpan(0, num, 0);
				case "h":
					return new TimeSpan(num, 0, 0);
				case "d":
					return new TimeSpan(num, 0, 0, 0);
				case "w":
					return new TimeSpan(num * 7, 0, 0, 0);
				case "M":
					return new TimeSpan(30, 0, 0, 0);
				case "y":
					return new TimeSpan(365, 0, 0);
				default:
					throw new NotImplementedException();
			}
		}
	}
}
