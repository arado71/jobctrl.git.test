using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tct.ActivityRecorderClient.Logging
{
	class CustomRollingDebugLogFileAppender : RollingFileAppender
	{
		private readonly Dictionary<LoggingData, ExceptionCounter> exceptionDictionary = new Dictionary<LoggingData, ExceptionCounter>();
#if DEBUG
		private const int tickCountAfterLoggingRestored = 10000;
#else
#if DEV
		private const int tickCountAfterLoggingRestored = 60000;
#else
		private const int tickCountAfterLoggingRestored = 600000;
#endif
#endif
		private static readonly FieldInfo loggingEventmDataFieldInfo = typeof(LoggingEvent).GetField(
			"m_data",
			BindingFlags.Instance | BindingFlags.NonPublic);
		private static volatile bool isPreviouslySuppressedLoggingsLogged = false;
		private static readonly object lockObject = new object();

		protected override void Append(LoggingEvent loggingEvent)
		{
			lock (lockObject)
			{
				if (!isPreviouslySuppressedLoggingsLogged)
				{
					try
					{
						isPreviouslySuppressedLoggingsLogged = true;
						var previouslySuppressedLoggingEvents_legacy = IsolatedStorageLogHelper.SuppressedLoggingEvents_legacy;
						if (previouslySuppressedLoggingEvents_legacy != null && previouslySuppressedLoggingEvents_legacy.Count != 0)
						{							base.Append(new LoggingEvent(
								MethodBase.GetCurrentMethod().DeclaringType,
								null,
								null,
								Level.Debug,
								"The following exceptions was suppressed in the previous run: ", null));
							foreach (var suppressedLoggingEvent in previouslySuppressedLoggingEvents_legacy)
							{
								base.Append(suppressedLoggingEvent);
							}
						}
						var previouslySuppressedLoggingEvents = IsolatedStorageLogHelper.SuppressedLoggingEvents;
						if (previouslySuppressedLoggingEvents != null)
						{
							base.Append(new LoggingEvent(
								MethodBase.GetCurrentMethod().DeclaringType,
								null,
								null,
								Level.Debug,
								"The following exceptions was suppressed in the previous run: ", null));
							foreach (var suppressedLoggingEvent in previouslySuppressedLoggingEvents)
							{
								if (suppressedLoggingEvent.Value.Count < 2) continue;
								var le = new LoggingEvent(
									MethodBase.GetCurrentMethod().DeclaringType,
									null,
									null,
									Level.Debug,
									$"{suppressedLoggingEvent.Value.Count} times, " +
									$"first: {suppressedLoggingEvent.Value.FirstOccurrence:yyyy-MM-dd HH:mm:ss.fff}, " +
									$"last: {suppressedLoggingEvent.Value.LastOccurrence:yyyy-MM-dd HH:mm:ss.fff}, " +
									$"thread: {suppressedLoggingEvent.Value.ThreadName}." +
									Environment.NewLine +
									suppressedLoggingEvent.Key.Message +
									Environment.NewLine +
									suppressedLoggingEvent.Key.ExceptionString
									, null
									);
								
								base.Append(le);
							}
						}
					}
					catch (Exception e)
					{
						base.Append(new LoggingEvent(
							MethodBase.GetCurrentMethod().DeclaringType,
							null,
							null,
							Level.Error,
							"The exceptions from the previous run couldn't be processed.", e));
					}
					finally
					{
						IsolatedStorageLogHelper.DeleteIfExists();
					}
				}

				if (loggingEvent.ExceptionObject == null)
				{
					base.Append(loggingEvent);
					return;
				}

				string message = loggingEvent.MessageObject.ToString();
				string exceptionString = loggingEvent.GetExceptionString();
				string threadName = loggingEvent.ThreadName;
				var loggingData = new LoggingData(message, exceptionString, threadName);
				if (exceptionDictionary.ContainsKey(loggingData))
				{
					var counter = exceptionDictionary[loggingData];
					if (Environment.TickCount - counter.Tick > tickCountAfterLoggingRestored)
					{
						if (counter.Count > 2)
						{
							SetMessageOnLoggingEvent(loggingEvent,
								string.Format($"The following exception was suppressed {counter.Count} times between " +
											  $"({counter.FirstOccurrence:yyyy-MM-dd HH:mm:ss.fff}" +
											  $" - {counter.LastOccurrence:yyyy-MM-dd HH:mm:ss.fff}): " +
											  Environment.NewLine +
											  loggingEvent.MessageObject
									));
							base.Append(loggingEvent);
							counter.Tick = Environment.TickCount;
							counter.Count = 0;
							counter.Suppressed = true;
							counter.FirstOccurrence = loggingEvent.TimeStamp;
							IsolatedStorageLogHelper.Save(exceptionDictionary);
							return;
						}
						counter.Tick = Environment.TickCount;
						counter.Count = 1;
						base.Append(loggingEvent);
						return;
					}
					if (counter.Count++ < 2 && !counter.Suppressed)
					{
						SetMessageOnLoggingEvent(loggingEvent,
							string.Format("The following exception is going to be suppressed: " + loggingEvent.MessageObject,
								counter.Count));
						counter.LastOccurrence = loggingEvent.TimeStamp;
						base.Append(loggingEvent);
					}
					else
					{
						counter.LastOccurrence = loggingEvent.TimeStamp;
						IsolatedStorageLogHelper.Save(exceptionDictionary);
					}
				}
				else
				{
					exceptionDictionary[loggingData] = new ExceptionCounter(loggingEvent);
					base.Append(loggingEvent);
				}
			}
		}

		private static void SetMessageOnLoggingEvent(LoggingEvent loggingEvent, string newMessage)
		{
			var loggingEventData = (LoggingEventData)loggingEventmDataFieldInfo.GetValue(loggingEvent);
			loggingEventData.Message = newMessage;
			loggingEventmDataFieldInfo.SetValue(loggingEvent, loggingEventData);
		}
	}

	[Serializable]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	class ExceptionCounter
	{
		public ExceptionCounter(LoggingEvent loggingEvent)
		{
			Count = 1;
			Tick = Environment.TickCount;
			Suppressed = false;
			ThreadName = loggingEvent.ThreadName;
			FirstOccurrence = loggingEvent.TimeStamp;
			LastOccurrence = loggingEvent.TimeStamp;
		}
		public string ThreadName { get; set; }

		public DateTime FirstOccurrence { get; set; }

		public DateTime LastOccurrence { get; set; }

		public uint Count { get; set; }

		public int Tick { get; set; }

		public bool Suppressed { get; set; }
	}

	[Serializable]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	class LoggingData
	{
		public string Message { get; }
		public string ThreadName { get; }
		public string ExceptionString { get; }

		public LoggingData(string m, string e, string t)
		{
			Message = m;
			ExceptionString = e;
			ThreadName = t;
		}

		public static bool operator ==(LoggingData ld1, LoggingData ld2)
		{
			if (ReferenceEquals(ld1, null) && ReferenceEquals(ld2, null)) return true;
			if (ReferenceEquals(ld1, null) || ReferenceEquals(ld2, null)) return false;
			if (ld1.Message != ld2.Message) return false;
			if (ld1.ThreadName != ld2.ThreadName) return false;
			if (ld1.ExceptionString != ld2.ExceptionString) return false;
			return true;
		}

		public static bool operator !=(LoggingData ld1, LoggingData ld2)
		{
			return !(ld1 == ld2);
		}

		public override bool Equals(object other)
		{
			var ld2 = (LoggingData) other;
			var ld1 = this;
			if (ld2 == null) return false;
			if (ld1.Message != ld2.Message) return false;
			if (ld1.ThreadName != ld2.ThreadName) return false;
			if (ld1.ExceptionString != ld2.ExceptionString) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}
}
