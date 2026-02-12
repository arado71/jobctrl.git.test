using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.Persistence
{
	public static class PersistenceHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static bool Save<T>(IPathResolver<T> pathSelector, T itemToSave)
		{
			long _;
			return Save(pathSelector, itemToSave, out _);
		}

		public static bool Save<T>(IPathResolver<T> pathSelector, T itemToSave, out long length)
		{
			if (pathSelector == null) throw new ArgumentNullException("pathSelector");
			var dir = pathSelector.GetRootDir();
			var path = Path.Combine(dir, pathSelector.GetFilePath(itemToSave));
			try
			{
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to create dir and save item to path: " + path, ex);
				length = 0;
				return false;
			}
			return Save(path, itemToSave, out length);
		}

		public static bool Save<T>(string path, T itemToSave, out long length)
		{
			try
			{
				IFormatter formatter = new BinaryFormatter();
				using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough))
				{
					formatter.Serialize(stream, itemToSave);
					length = stream.Length;
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to save item to path: " + path, ex);
				length = 0;
				return false;
			}
			return true;
		}

		public static IEnumerable<T> LoadAllFromRootDir<T>(IPathResolver<T> pathSelector)
		{
			if (pathSelector == null) throw new ArgumentNullException("pathSelector");
			var dir = pathSelector.GetRootDir();
			var dirInfo = new DirectoryInfo(dir);
			if (!dirInfo.Exists) yield break;
			foreach (var file in dirInfo.GetFiles(pathSelector.GetSearchPattern()))
			{
				T obj;
				if (Load(file.FullName, out obj))
				{
					yield return obj;
				}
			}
		}

		public static bool Load<T>(string path, out T value)
		{
			var shouldDelete = false;
			try
			{
				T obj;
				IFormatter formatter = new BinaryFormatter();
				using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					if (stream.Length == 0)
					{
						log.Error("Deleting empty item from path: " + path);
						shouldDelete = true;
						value = default(T);
						return false;
					}
					else
					{
						obj = (T)formatter.Deserialize(stream);
					}
				}
				value = obj;
				return true;
			}
			catch (Exception ex)
			{
				log.Error("Unable to load item from path: " + path, ex);
				value = default(T);
				return false;
			}
			finally
			{
				if (shouldDelete)
				{
					try
					{
						File.Delete(path);
					}
					catch (Exception ex)
					{
						log.Error("Unable to delete empty item from path: " + path, ex);
					}
				}
			}
		}

		public static bool Delete<T>(IPathResolver<T> pathSelector, T itemToDelete)
		{
			if (pathSelector == null) throw new ArgumentNullException("pathSelector");
			var dir = pathSelector.GetRootDir();
			var path = Path.Combine(dir, pathSelector.GetFilePath(itemToDelete));
			return Delete(path);
		}

		public static bool Delete(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch (Exception ex)
			{
				log.Error("Unable to delete file from path: " + path, ex);
				return false;
			}
			return true;
		}
	}
}
