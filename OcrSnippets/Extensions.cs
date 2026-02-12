using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using log4net;

namespace TcT.OcrSnippets
{
	public static class Extensions
	{
        //FormatString
	    public static string FS(this string input, params object[] pArray)
	    {
	        return String.Format(input, pArray);
	    }
		public static string ToDateWithHourMinuteSecondString(this DateTime dateTime)
		{
			return dateTime.ToString("yyyy. MM. dd. HH:mm:ss");
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
			return (timeSpan.Days * 24 + timeSpan.Hours).ToString("D2", System.Globalization.CultureInfo.InvariantCulture)
				+ ":" + timeSpan.Minutes.ToString("D2", System.Globalization.CultureInfo.InvariantCulture)
				+ ":" + timeSpan.Seconds.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
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

		public static int GetDays(this TimeSpan timeSpan)
		{
			return (int)(timeSpan.TotalDays >= 0 ? Math.Ceiling(timeSpan.TotalDays) : Math.Floor(timeSpan.TotalDays));
		}

		private static string ToHourMinuteStringNonNeg(this TimeSpan timeSpan)
		{
			return (timeSpan.Days * 24 + timeSpan.Hours).ToString(System.Globalization.CultureInfo.InvariantCulture)
				+ ":" + timeSpan.Minutes.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
		}

		public static string ToShortDateString(this DateTime? dateTime)
		{
			return dateTime.HasValue ? dateTime.Value.ToShortDateString() : "";
		}

		public static string ToTotalHoursString(this TimeSpan? timeSpan)
		{
			return timeSpan.HasValue ? timeSpan.Value.TotalHours.ToString("0.#") : "";
		}

		public static string ToInvariantShortDateString(this DateTime? dateTime)
		{
			return dateTime.HasValue ? dateTime.Value.ToInvariantShortDateString() : "";
		}

		public static string ToInvariantShortDateString(this DateTime dateTime)
		{
			return dateTime.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
		}

		public static DateTime FromUtcToLocal(this DateTime utcDateTime, TimeZoneInfo timeZone = null)
		{
			var utckindDateTime = utcDateTime.Kind == DateTimeKind.Utc
				? utcDateTime
				: new DateTime(utcDateTime.Ticks, DateTimeKind.Utc);
			return FromUtcToLocalImpl(utckindDateTime, timeZone ?? TimeZoneInfo.Local);
		}

		private static DateTime FromUtcToLocalImpl(DateTime utcDateTime, TimeZoneInfo timeZone)
		{
			Debug.Assert(utcDateTime.Kind == DateTimeKind.Utc);
			return new DateTime(utcDateTime.Ticks + timeZone.GetUtcOffset(utcDateTime).Ticks, DateTimeKind.Unspecified);
		}

		public static DateTime FromLocalToUtc(this DateTime localDateTime, TimeZoneInfo timeZone = null)
		{
			var unspecifiedDateTime = localDateTime.Kind == DateTimeKind.Unspecified
				? localDateTime
				: new DateTime(localDateTime.Ticks, DateTimeKind.Unspecified);
			return FromLocalToUtcImpl(unspecifiedDateTime, timeZone ?? TimeZoneInfo.Local);
		}

		private static DateTime FromLocalToUtcImpl(DateTime localDateTime, TimeZoneInfo timeZone)
		{
			Debug.Assert(localDateTime.Kind == DateTimeKind.Unspecified);
			return localDateTime - timeZone.GetUtcOffset(localDateTime);
		}

		public static string Ellipse(this string text, int length)
		{
			if (length < 4) throw new ArgumentOutOfRangeException("length");
			return text == null || text.Length <= length
				? text
				: text.Substring(0, length - 3) + "...";
		}

		public static string Truncate(this string text, int length)
		{
			return text == null || text.Length <= length
				? text
				: text.Substring(0, length);
		}

		public static bool IsNullOrWhiteSpace(this string value)
		{
			if (value != null)
			{
				for (int i = 0; i < value.Length; i++)
				{
					if (!char.IsWhiteSpace(value[i]))
					{
						return false;
					}
				}
			}
			return true;
		}

		public static void FatalAndFail(this ILog log, string message, Exception ex)
		{
			log.Fatal(message, ex);
			Debug.Fail(message + Environment.NewLine + ex);
		}

		public static void FatalAndFail(this ILog log, string message)
		{
			FatalAndFail(log, message, null);
		}

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

		public static bool IsVerboseEnabled(this ILog log)
		{
			return log.Logger.IsEnabledFor(log4net.Core.Level.Verbose);
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

		public static bool CollectionEqual<T>(this ICollection<T> first, ICollection<T> second, IEqualityComparer<T> comparer = null)
		{
			if (first == null || first.Count == 0) return (second == null || second.Count == 0);
			if (second == null || second.Count == 0) return false;
			if (first.Count != second.Count) return false;
			if (comparer == null) comparer = EqualityComparer<T>.Default;
			return first.SequenceEqual(second, comparer);
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
}
