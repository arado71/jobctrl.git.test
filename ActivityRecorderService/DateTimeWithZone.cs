using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService
{
	//idea from: http://stackoverflow.com/questions/246498/creating-a-datetime-in-a-specific-time-zone-in-c-fx-35#246529
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public struct DateTimeWithZone
	{
		private readonly DateTime utcDateTime;
		private readonly TimeZoneInfo timeZone;

		public static DateTimeWithZone Now
		{
			get { return new DateTimeWithZone(DateTime.UtcNow, TimeZoneInfo.Local); }
		}

		//assume we've got utc time no matter of the Kind
		public DateTimeWithZone(DateTime utcDateTime, TimeZoneInfo timeZone)
		{
			//Local time could be converted with utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone); (using local timeZone)
			Debug.Assert(utcDateTime.Kind != DateTimeKind.Local);
			this.utcDateTime =
				utcDateTime.Kind == DateTimeKind.Utc
				? utcDateTime
				: new DateTime(utcDateTime.Ticks, DateTimeKind.Utc);
			this.timeZone = timeZone;
		}

		public DateTime UniversalTime { get { return utcDateTime; } }

		public TimeZoneInfo TimeZone { get { return timeZone; } }

		public DateTime LocalTime
		{
			get
			{
				return TimeZoneInfo.ConvertTime(utcDateTime, timeZone);
			}
		}
	}
}
