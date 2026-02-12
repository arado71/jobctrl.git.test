using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PlaybackClient.MobileServiceReference
{
	partial class WorkItem
	{
		public Guid SessionIdTyped
		{
			get
			{
				Guid result;
				return Guid.TryParse(SessionId, out result) ? result : Guid.Empty;
			}
			set { SessionId = value.ToString(); }
		}

		public DateTime StartDateTyped
		{
			get { return DateTimeFromString(StartDate); }
			set { StartDate = DateTimeToString(value); }
		}

		public DateTime EndDateTyped
		{
			get { return DateTimeFromString(EndDate); }
			set { EndDate = DateTimeToString(value); }
		}

		public static string DateTimeToString(DateTime dateTime)
		{
			return dateTime.ToString("yyyy.MM.dd. HH:mm:ssZ"); //Z is needed to be treated as UTC at the server side
		}

		public static DateTime DateTimeFromString(string dateString)
		{
			//we want to get back the same value that is set [we ignore Kind] (by leaving Z at the end, Parse would return Local time that is why we specify AdjustToUniversal)
			DateTime result;
			return DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out result)
				? result
				: DateTime.MinValue;
		}

		public override string ToString()
		{
			return "mobileWorkItem workId: " + WorkId + " start: " + StartDateTyped + " end: " + EndDateTyped;
		}
	}
}
