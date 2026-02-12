using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.TodoLists
{
	class IsolatedStorageTodoListHelper
	{
		private const string StoreDir = "TodoList";
		private const string FormSizeFileName = "FormSize";
		private static string path { get { return StoreDir + "-" + ConfigManager.UserId; } }

		static IsolatedStorageTodoListHelper()
		{
			IsolatedStorageSerializationHelper.CreateDir(path);
		}

		public static void Save(Size s)
		{
			IsolatedStorageSerializationHelper.Save(Path.Combine(path, FormSizeFileName), s);
		}

		public static Size? Size
		{
			get
			{
				string itemPath = Path.Combine(path, FormSizeFileName);
				if (IsolatedStorageSerializationHelper.Exists(itemPath) && IsolatedStorageSerializationHelper.Load(itemPath, out Size value))
					return value;
				return null;
			}
		}
	}
}
