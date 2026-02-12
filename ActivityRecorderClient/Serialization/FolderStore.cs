using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Serialization
{
	public class FolderStore<T> where T : class, IUploadItem
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly string dir;

		public FolderStore(string folderName)
		{
			dir = folderName;
			IsolatedStorageSerializationHelper.CreateDir(dir);
		}

		public bool Save(T itemToSave)
		{
			if (itemToSave == null) return true;
			string path = GetPath(itemToSave);
			return IsolatedStorageSerializationHelper.Save(path, itemToSave);
		}

		public bool Delete(T itemToDelete)
		{
			if (itemToDelete == null) return true;
			string path = GetPath(itemToDelete);
			return IsolatedStorageSerializationHelper.Delete(path);
		}

		public bool DeleteAll()
		{
			var res = true;
			try
			{
				foreach (var path in GetPersistedPaths())
				{
					res &= IsolatedStorageSerializationHelper.Delete(path);
				}
			}
			catch (Exception ex) when (ex is DirectoryNotFoundException || ex is IsolatedStorageException)
			{
				log.Warn("Can't delete isolated storage items", ex);
			}
			return res;
		}

		public List<string> GetPersistedPaths()
		{
			var result = new List<string>();
			foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(dir, "u" + ConfigManager.UserId + "_*")))
			{
				string filePath = Path.Combine(dir, fileName);
				result.Add(filePath);
			}
			result.Sort();
			return result;
		}

		public T Load(string path)
		{
			T itemLoaded;
			if (IsolatedStorageSerializationHelper.Load(path, out itemLoaded) && itemLoaded.UserId == ConfigManager.UserId)
			{
				return itemLoaded;
			}
			return null;
		}

		private string GetPath(T item)
		{
			return Path.Combine(dir, "u" + item.UserId + "_" + item.StartDate.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + item.Id);
		}
	}
}
