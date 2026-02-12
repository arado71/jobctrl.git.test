using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using log4net;

namespace Tct.ActivityRecorderService
{
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

		public static string ToInvariantShortDateString(this DateTime? dateTime)
		{
			return dateTime.HasValue ? dateTime.Value.ToInvariantShortDateString() : "";
		}

		public static string ToInvariantShortDateString(this DateTime dateTime)
		{
			return dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		}

		public static string ToInvariantString(this DateTime? dateTime)
		{
			return dateTime.HasValue ? dateTime.Value.ToInvariantString() : "";
		}

		public static string ToInvariantString(this DateTime dateTime)
		{
			return dateTime.ToString("G", CultureInfo.InvariantCulture);
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

	public static class CultureInfoExtensions
	{
		public static string GetCultureSpecificName(this CultureInfo cultureInfo, string firstName, string lastName, string nameAddition = null, bool isPrefix = false)
		{
			if (cultureInfo.ToString().ToLower() == "hu-hu" || cultureInfo.ToString().ToLower() == "ko-kr" || cultureInfo.ToString().ToLower() == "ja-jp")
				return isPrefix
						? (nameAddition + " " + (lastName + " " + firstName).Trim()).Trim()
						: ((lastName + " " + firstName).Trim() + " " + nameAddition).Trim();

			return isPrefix
						? (nameAddition + " " + (firstName + " " + lastName).Trim()).Trim()
						: ((firstName + " " + lastName).Trim() + " " + nameAddition).Trim();
		}
	}

	public static class DictionaryExtensions
	{
		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
		{
			if (dict == null) throw new ArgumentNullException("dict");
			TValue value;
			return dict.TryGetValue(key, out value)
				? value
				: default(TValue);
		}

		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
		{
			if (dict == null) throw new ArgumentNullException("dict");
			if (valueFactory == null) throw new ArgumentNullException("valueFactory");
			TValue value;
			if (!dict.TryGetValue(key, out value))
			{
				value = valueFactory(key);
				dict.Add(key, value);
			}
			return value;
		}

		public static Dictionary<TKey, TValue> ToDictionaryAllowDuplicates<TKey, TValue, T>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (keySelector == null) throw new ArgumentNullException("keySelector");
			if (keySelector == null) throw new ArgumentNullException("valueSelector");
			var result = new Dictionary<TKey, TValue>();
			foreach (var item in source)
			{
				result[keySelector(item)] = valueSelector(item);
			}
			return result;
		}
		public static void RemoveAll<T>(this ICollection<T> source, Func<T, bool> condition)
		{
			source.Where<T>(condition).ToList().ForEach(e => source.Remove(e));
		}
	}

	public static class LinqExtensions
	{
		public static long ToLong(this Binary binary)
		{
			if (binary == null) throw new ArgumentNullException();
			if (binary.Length != 8) throw new ArgumentOutOfRangeException();

			byte[] bytes = binary.ToArray();
			if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return BitConverter.ToInt64(bytes, 0);
		}

		public static Binary ToBinary(this long value)
		{
			var bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return new Binary(bytes);
		}

		public static DbTransaction BeginTransaction(this DataContext context, IsolationLevel isoLevel, int timeout = 300, bool objectTrackingEnabled = false)
		{
			context.CommandTimeout = timeout;
			context.ObjectTrackingEnabled = objectTrackingEnabled;
			context.Connection.Open();
			context.Transaction = context.Connection.BeginTransaction(isoLevel);
			return context.Transaction;
		}

		public static void SetXactAbortOn(this DataContext context) => context.ExecuteCommand("SET XACT_ABORT ON");
		public static void SetXactAbortOn(this DbConnection connection) => connection.Execute("SET XACT_ABORT ON");
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

		public static void WarnAndFail(this ILog log, string message, Exception ex)
		{
			log.Warn(message, ex);
			Debug.Fail(message + Environment.NewLine + ex);
		}

		public static void WarnAndFail(this ILog log, string message)
		{
			WarnAndFail(log, message, null);
		}

		public static void InfoAndFail(this ILog log, string message, Exception ex)
		{
			log.Info(message, ex);
			Debug.Fail(message + Environment.NewLine + ex);
		}

		public static void InfoAndFail(this ILog log, string message)
		{
			InfoAndFail(log, message, null);
		}

		public static void DebugAndFail(this ILog log, string message, Exception ex)
		{
			log.Debug(message, ex);
			Debug.Fail(message + Environment.NewLine + ex);
		}

		public static void DebugAndFail(this ILog log, string message)
		{
			DebugAndFail(log, message, null);
		}

		public static void Verbose(this ILog log, string message)
		{
			Verbose(log, message, null);
		}

		private static readonly Type callerStackBoundaryType = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
		public static void Verbose(this ILog log, string message, Exception ex)
		{
			if (log.Logger.IsEnabledFor(log4net.Core.Level.Verbose))
			{
				log.Logger.Log(callerStackBoundaryType, log4net.Core.Level.Verbose, message, ex);
			}
		}

		public static void VerboseFormat(this ILog log, string format, object arg0)
		{
			if (log.Logger.IsEnabledFor(log4net.Core.Level.Verbose))
			{
				log.Logger.Log(callerStackBoundaryType, log4net.Core.Level.Verbose,
					new log4net.Util.SystemStringFormat(System.Globalization.CultureInfo.InvariantCulture, format, new[] { arg0 }), null);
			}
		}

		public static void VerboseFormat(this ILog log, string format, object arg0, object arg1)
		{
			if (log.Logger.IsEnabledFor(log4net.Core.Level.Verbose))
			{
				log.Logger.Log(callerStackBoundaryType, log4net.Core.Level.Verbose,
					new log4net.Util.SystemStringFormat(System.Globalization.CultureInfo.InvariantCulture, format, new[] { arg0, arg1 }), null);
			}
		}

		public static void VerboseFormat(this ILog log, string format, object arg0, object arg1, object arg2)
		{
			if (log.Logger.IsEnabledFor(log4net.Core.Level.Verbose))
			{
				log.Logger.Log(callerStackBoundaryType, log4net.Core.Level.Verbose,
					new log4net.Util.SystemStringFormat(System.Globalization.CultureInfo.InvariantCulture, format, new[] { arg0, arg1, arg2 }), null);
			}
		}

		public static void VerboseFormat(this ILog log, string format, params object[] args)
		{
			if (log.Logger.IsEnabledFor(log4net.Core.Level.Verbose))
			{
				log.Logger.Log(callerStackBoundaryType, log4net.Core.Level.Verbose,
					new log4net.Util.SystemStringFormat(System.Globalization.CultureInfo.InvariantCulture, format, args), null);
			}
		}
	}

	public static class StringExtensions
	{
		public static string ToInvariantString(this int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToInvariantString(this int? value)
		{
			return value == null ? "" : value.Value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToInvariantString(this long value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToInvariantString(this double value)
		{
			return value.ToString("0.000", CultureInfo.InvariantCulture);
		}

		public static string ToTotalMillisecondsString(this TimeSpan value)
		{
			return value.TotalMilliseconds.ToString("0.000", CultureInfo.InvariantCulture);
		}

		public static string ToTotalMillisecondsString(this Stopwatch sw)
		{
			return sw.Elapsed.TotalMilliseconds.ToString("0.000", CultureInfo.InvariantCulture);
		}
	}

	public static class WcfExtensions
	{
		//http://www.w3.org/TR/REC-xml/#charsets
		//http://blogs.technet.com/b/stefan_gossner/archive/2009/08/26/how-to-deal-with-invalid-characters-in-soap-responses-from-asp-net-web-services.aspx
		// #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]	/* any Unicode character, excluding the surrogate blocks, FFFE, and FFFF. */
		private static readonly Regex invalidRegex = new Regex("[\u0000-\u0008\u000B\u000C\u000E-\u001F\uD800-\uDFFF]");

		public static string ReplaceInvalidXmlChars(this string text, string replacement)
		{
			return text == null ? null : invalidRegex.Replace(text, replacement);
		}

		public static bool HasInvalidXmlChars(this string text)
		{
			return invalidRegex.IsMatch(text);
		}
	}
}
