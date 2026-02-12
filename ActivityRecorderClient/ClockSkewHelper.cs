using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace Tct.ActivityRecorderClient
{
	public static class ClockSkewHelper
	{

		public static bool IsClockSkewException(Exception ex, out ClockSkewData data)
		{
			if (ex is MessageSecurityException
					&& ex.StackTrace != null
					&& ex.StackTrace.Contains("System.ServiceModel.Security.SecurityTimestamp.ValidateFreshness("))
			{
				DateTime? clientTime, serverTime;
				if (GetDateTimesFromString(ex.Message, out serverTime, out clientTime))
				{
					data = new ClockSkewData() { ClientTime = clientTime.Value, ServerTime = serverTime.Value };
					return true;
				}
			}
			data = null;
			return false;
		}

		public static bool GetDateTimesFromString(string source, out DateTime? firstDate, out DateTime? secondDate)
		{
			firstDate = null;
			secondDate = null;
			if (source == null) return false;
			var matches = Regex.Matches(source, "\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}[.]\\d{3}Z");
			if (matches.Count == 2)
			{
				try
				{
					firstDate = DateTime.ParseExact(matches[0].ToString(), "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture);
					secondDate = DateTime.ParseExact(matches[1].ToString(), "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture);
					return true;
				}
				catch { }
			}
			return false;
		}
	}

	public class ClockSkewData
	{
		public DateTime ClientTime { get; set; }
		public DateTime ServerTime { get; set; }
	}
}
