using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Meeting
{
	public class PostponedMeetingsManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string StoreDir = "PostponedMeetings";
		private static string path { get { return StoreDir + "-" + ConfigManager.UserId; } }

		static PostponedMeetingsManager()
		{
			IsolatedStorageSerializationHelper.CreateDir(path);
		}
		public static bool Save(PostponedMeetingItem data)
		{
			return IsolatedStorageSerializationHelper.Save(Path.Combine(path, data.Id.ToString()), data);
		}
		public static void Delete(Guid guid)
		{
			string fullPath = Path.Combine(path, guid.ToString());
			IsolatedStorageSerializationHelper.Delete(fullPath);
		}
		public static bool DeleteIfExists(Guid guid)
		{
			string fullPath = Path.Combine(path, guid.ToString());
			if (!IsolatedStorageSerializationHelper.Exists(fullPath)) return false;
			IsolatedStorageSerializationHelper.Delete(fullPath);
			return true;
		}
		public static void GetItems(Action<PostponedMeetingItem> action)
		{
			foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(path, "*")))
			{
				PostponedMeetingItem loaded;
				if (IsolatedStorageSerializationHelper.Load(Path.Combine(path, fileName), out loaded))
					action(loaded);
			}
		}
		public static IEnumerable<ManualMeetingItem> Items
		{
			get
			{
				foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(path, "*")))
				{
					ManualMeetingItem loaded;
					if (IsolatedStorageSerializationHelper.Load(Path.Combine(path, fileName), out loaded))
						yield return loaded;
				}
			}
		}
	}
}
