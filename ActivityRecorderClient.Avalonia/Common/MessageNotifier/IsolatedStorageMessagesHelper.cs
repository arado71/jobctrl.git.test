using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.MessageNotifier
{
	using Message = ActivityRecorderServiceReference.Message;
	public class IsolatedStorageMessagesHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string StoreDir = "Messages";
		private static string path { get { return StoreDir + "-" + ConfigManager.UserId; } }

		static IsolatedStorageMessagesHelper()
		{
			IsolatedStorageSerializationHelper.CreateDir(path);
		}
		public static void Save(Message data)
		{
			IsolatedStorageSerializationHelper.Save(Path.Combine(path, data.Id.ToString()), data);
		}
		public static void Delete(int id)
		{
			string fullPath = Path.Combine(path, id.ToString());
			IsolatedStorageSerializationHelper.Delete(fullPath);
		}
		public static bool DeleteIfExists(int id)
		{
			string fullPath = Path.Combine(path, id.ToString());
			if (!IsolatedStorageSerializationHelper.Exists(fullPath)) return false;
			IsolatedStorageSerializationHelper.Delete(fullPath);
			return true;
		}
		public static void GetItems(Action<Message> action)
		{
			foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(path, "*")))
			{
				Message loaded;
				if (IsolatedStorageSerializationHelper.Load(Path.Combine(path, fileName), out loaded))
					action(loaded);
			}
		}
		public static IEnumerable<Message> Items
		{
			get
			{
				foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(path, "*")))
				{
					Message loaded;
					if (IsolatedStorageSerializationHelper.Load(Path.Combine(path, fileName), out loaded))
						yield return loaded;
				}
			}
		}
	}
}
