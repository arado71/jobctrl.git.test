using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Meeting
{
	[System.SerializableAttribute()]
	public class AddMeetingHistory
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static string FilePath { get { return "AddMeetingHistory-" + ConfigManager.UserId; } }

		private static readonly AddMeetingHistory _instance;

		public static List<string> RecentEmails { get { return _instance.RecentEmailList; } }
		public static List<string> RecentSubjects { get { return _instance.RecentSubjectList; } }

		public List<string> RecentEmailList { get; private set; }
		public List<string> RecentSubjectList { get; private set; }

		static AddMeetingHistory()
		{
			try
			{
				if (IsolatedStorageSerializationHelper.Exists(FilePath) && IsolatedStorageSerializationHelper.Load(FilePath, out _instance)) return;
			}
			catch (Exception ex) when (ex is IOException || ex is IsolatedStorageException)
			{
				log.Warn("Can't access isolated storage", ex);
			}
			_instance = new AddMeetingHistory() {RecentEmailList = new List<string>(), RecentSubjectList = new List<string>()};
		}

		public static void Save()
		{
			IsolatedStorageSerializationHelper.Save(FilePath, _instance);
		}
	}
}
