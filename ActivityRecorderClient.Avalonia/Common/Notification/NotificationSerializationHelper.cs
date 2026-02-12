using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Notification
{
	public class NotificationSerializationHelper
	{
		public static readonly string StoreDir = "Notifications";

		static NotificationSerializationHelper()
		{
			IsolatedStorageSerializationHelper.CreateDir(StoreDir);
		}

		public static PendingNotification CreatePending(NotificationData data)
		{
			var result = new PendingNotification(data);
			IsolatedStorageSerializationHelper.Save(GetPath(result), result);
			return result;
		}

		public static void DeletePending(PendingNotification data)
		{
			IsolatedStorageSerializationHelper.Delete(GetPath(data));
		}

		public static void SavePending(PendingNotification data) //this is dangerous and not water-tight
		{
			IsolatedStorageSerializationHelper.Save(GetPath(data), data);
		}

		public static List<PendingNotification> LoadPendings()
		{
			var res = new List<PendingNotification>();
			foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(StoreDir, "u" + ConfigManager.UserId + "_*")))
			{
				PendingNotification loaded;
				if (IsolatedStorageSerializationHelper.Load(Path.Combine(StoreDir, fileName), out loaded))
				{
					res.Add(loaded);
				}
			}
			return res;
		}

		private static string GetPath(PendingNotification data)
		{
			return Path.Combine(StoreDir, "u" + data.Result.UserId + "_" + data.Id.ToString());
		}
	}
}
