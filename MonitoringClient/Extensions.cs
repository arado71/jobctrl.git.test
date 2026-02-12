using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonitoringClient
{
	public static class Extensions
	{
		private static DateTime FromUtcToLocalImpl(this DateTime utcDateTime, TimeZoneInfo timeZone)
		{
			Debug.Assert(utcDateTime.Kind == DateTimeKind.Utc);
			return new DateTime(utcDateTime.Ticks + timeZone.GetUtcOffset(utcDateTime).Ticks, DateTimeKind.Unspecified);
		}

		public static DateTime FromUtcToLocal(this DateTime utcDateTime, TimeZoneInfo timeZone)
		{
			var utckindDateTime = utcDateTime.Kind == DateTimeKind.Utc
				? utcDateTime
				: new DateTime(utcDateTime.Ticks, DateTimeKind.Utc);
			return FromUtcToLocalImpl(utckindDateTime, timeZone);

			//this would throw if utcDateTime kind is Local and timeZone is not Local
			//return TimeZoneInfo.ConvertTime(utcDateTime, TimeZoneInfo.Utc, timeZone);
		}

		public static DateTime? FromUtcToLocal(this DateTime? utcDateTime, TimeZoneInfo timeZone)
		{
			return utcDateTime.HasValue ? FromUtcToLocal(utcDateTime.Value, timeZone) : new DateTime?();
		}

		private static DateTimeOffset FromUtcToDateTimeOffsetImpl(DateTime utcDateTime, TimeZoneInfo timeZone)
		{
			Debug.Assert(utcDateTime.Kind == DateTimeKind.Utc);
			var offset = timeZone.GetUtcOffset(utcDateTime);
			return new DateTimeOffset(utcDateTime.Ticks + offset.Ticks, offset); //use Ticks so kind is not checked
		}

		public static DateTimeOffset FromUtcToDateTimeOffset(this DateTime utcDateTime, TimeZoneInfo timeZone)
		{
			var utckindDateTime = utcDateTime.Kind == DateTimeKind.Utc
				? utcDateTime
				: new DateTime(utcDateTime.Ticks, DateTimeKind.Utc);
			return FromUtcToDateTimeOffsetImpl(utckindDateTime, timeZone);
		}
	}
}
