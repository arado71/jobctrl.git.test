using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using log4net.Core;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Logging
{
	class IsolatedStorageLogHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string StoreDir = "LogCounter";
		private const string LoggingEventsFilename = "LogCounterDictionary";

		static IsolatedStorageLogHelper()
		{
			IsolatedStorageSerializationHelper.CreateDir(StoreDir);
		}
		public static void Save(List<LoggingEvent> suppressedLoggingEvents)
		{
			IsolatedStorageSerializationHelper.Save(Path.Combine(StoreDir, LoggingEventsFilename), suppressedLoggingEvents);
		}
		public static bool DeleteIfExists()
		{
			string fullPath = Path.Combine(StoreDir, LoggingEventsFilename);
			if (!IsolatedStorageSerializationHelper.Exists(fullPath)) return false;
			IsolatedStorageSerializationHelper.Delete(fullPath);
			return true;
		}

		public static List<LoggingEvent> SuppressedLoggingEvents_legacy
		{
			get
			{
				List<LoggingEvent> loaded;
				if (IsolatedStorageSerializationHelper.Load(Path.Combine(StoreDir, LoggingEventsFilename), out loaded))
				{
					return loaded;
				}
				return null;
			}
		}

		internal static void Save(Dictionary<LoggingData, ExceptionCounter> exceptionDictionary)
		{
			IsolatedStorageSerializationHelper.Save(Path.Combine(StoreDir, LoggingEventsFilename), exceptionDictionary);
		}

		public static Dictionary<LoggingData, ExceptionCounter> SuppressedLoggingEvents
		{
			get
			{
				Dictionary<LoggingData, ExceptionCounter> loaded;
				if (IsolatedStorageSerializationHelper.Load(Path.Combine(StoreDir, LoggingEventsFilename), out loaded))
				{
					return loaded;
				}
				return null;
			}
		}
	}
}
