using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Meeting
{
	public static class IsolatedStoragePostponedMeetingsHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string StoreDir = "PostponedMeetings";
		private static string path { get { return StoreDir + "-" + ConfigManager.UserId; } }

		static IsolatedStoragePostponedMeetingsHelper()
		{
			IsolatedStorageSerializationHelper.CreateDir(path);
		}

		public static void Save(PostponedMeetingItem data)
		{
			IsolatedStorageSerializationHelper.Save(Path.Combine(path, data.Id.ToString()), data);
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

		public static void DeleteAll()
		{
			foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(path, "*")))
			{
				IsolatedStorageSerializationHelper.Delete(Path.Combine(path, fileName));
			}
		}

		public static IEnumerable<PostponedMeetingItem> Items
		{
			get
			{
				foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(path, "*")))
				{
					PostponedMeetingItem loaded;
					if (IsolatedStorageSerializationHelper.Load(Path.Combine(path, fileName), out loaded))
						yield return loaded;
				}
			}
		}
	}
}
