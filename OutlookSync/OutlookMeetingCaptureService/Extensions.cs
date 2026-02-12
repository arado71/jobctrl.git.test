using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace System
{
	public static class Extensions
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
	}
}
