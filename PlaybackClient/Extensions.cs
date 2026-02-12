using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace System
{
	public static class LinqExtensions
	{
		public static DbTransaction BeginTransaction(this DataContext context, IsolationLevel isoLevel, int timeout = 300, bool objectTrackingEnabled = false)
		{
			context.CommandTimeout = timeout;
			context.ObjectTrackingEnabled = objectTrackingEnabled;
			context.Connection.Open();
			context.Transaction = context.Connection.BeginTransaction(isoLevel);
			return context.Transaction;
		}
		public static void SetXactAbortOn(this DataContext context) => context.ExecuteCommand("SET XACT_ABORT ON");
	}

	public static class TimeExtensions
	{
		public static string ToDateWithHourMinuteString(this DateTime? dateTime)
		{
			return dateTime.HasValue ? dateTime.Value.ToDateWithHourMinuteString() : "";
		}

		public static string ToDateWithHourMinuteString(this DateTime dateTime)
		{
			return dateTime.ToString("yyyy. MM. dd. HH:mm");
		}

		public static string ToHourMinuteString(this DateTime? dateTime)
		{
			return dateTime.HasValue ? dateTime.Value.ToHourMinuteString() : "";
		}

		public static string ToHourMinuteString(this DateTime dateTime)
		{
			return dateTime.ToString("HH:mm");
		}

		public static string ToHourMinuteSecondString(this TimeSpan? timeSpan)
		{
			return timeSpan.HasValue ? timeSpan.Value.ToHourMinuteSecondString() : "";
		}

		public static string ToHourMinuteSecondString(this TimeSpan timeSpan)
		{
			return timeSpan < TimeSpan.Zero
					? "-" + ToHourMinuteSecondStringNonNeg(-timeSpan)
					: ToHourMinuteSecondStringNonNeg(timeSpan);
		}

		private static string ToHourMinuteSecondStringNonNeg(this TimeSpan timeSpan)
		{
			return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Days * 24 + timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}

		public static string ToHourMinuteString(this TimeSpan? timeSpan)
		{
			return timeSpan.HasValue ? timeSpan.Value.ToHourMinuteString() : "";
		}

		public static string ToHourMinuteString(this TimeSpan timeSpan)
		{
			return timeSpan < TimeSpan.Zero
					? "-" + ToHourMinuteStringNonNeg(-timeSpan)
					: ToHourMinuteStringNonNeg(timeSpan);
		}

		private static string ToHourMinuteStringNonNeg(this TimeSpan timeSpan)
		{
			return string.Format("{0:D2}:{1:D2}", timeSpan.Days * 24 + timeSpan.Hours, timeSpan.Minutes);
		}

		private static DateTime FromLocalToUtcImpl(this DateTime localDateTime, TimeZoneInfo timeZone)
		{
			Debug.Assert(localDateTime.Kind == DateTimeKind.Unspecified);
			return localDateTime - timeZone.GetUtcOffset(localDateTime);
		}

		public static DateTime FromLocalToUtc(this DateTime localDateTime, TimeZoneInfo timeZone)
		{
			var unspecifiedDateTime = localDateTime.Kind == DateTimeKind.Unspecified
				? localDateTime
				: new DateTime(localDateTime.Ticks, DateTimeKind.Unspecified);
			return FromLocalToUtcImpl(unspecifiedDateTime, timeZone);

			//this would throw if localDateTime kind is Local and timeZone is not Local
			//and would also throw on invalid date
			//try 
			//{
			//    return TimeZoneInfo.ConvertTime(localDateTime, timeZone, TimeZoneInfo.Utc);
			//}
			//catch (ArgumentException)
			//{
			//    return localDateTime - timeZone.GetUtcOffset(localDateTime);
			//}
		}

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

	public static class Log4NetExtensions
	{
		public static void ErrorAndFail(this ILog log, string message, Exception ex)
		{
			log.Error(message, ex);
			Debug.Fail(message + Environment.NewLine + ex);
		}

		public static void ErrorAndFail(this ILog log, string message)
		{
			ErrorAndFail(log, message, null);
		}
	}

	public static class EnumerationExtensions
	{
		public static string Description(this Enum enumeration)
		{
			string value = enumeration.ToString();
			Type type = enumeration.GetType();
			//Use reflection to try and get the description attribute for the enumeration
			DescriptionAttribute[] descAttribute =
				(DescriptionAttribute[])type.GetField(value).GetCustomAttributes(typeof(DescriptionAttribute), false);
			return descAttribute.Length > 0 ? descAttribute[0].Description : string.Empty;
		}
	}


	public static class ObjectExtensions
	{
		public static void DisposeObject(this object disposable)
		{
			using (disposable as IDisposable) { }
		}
	}
}
