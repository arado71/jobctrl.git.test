using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using log4net;

namespace VoxCTRL.Serialization
{

	public static class IsolatedStorageSerializationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		//http://blogs.msdn.com/shawnfa/archive/2006/01/20/514411.aspx
		//IsClickOnce = AppDomain.CurrentDomain.ActivationContext != null
		//System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed is the same but slower
		private static IsolatedStorageFile GetStore()
		{
			//url/path is used which is not ideal but we don't care atm because this should be a ClickOnce program
			return ConfigManager.IsRoamingStorageScopeNeeded
				? IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Roaming, null, null)
				: IsolatedStorageFile.GetUserStoreForAssembly();
		}

		public static void CreateDir(string dir)
		{
			using (IsolatedStorageFile isoStore = GetStore())
			{
				isoStore.CreateDirectory(dir);
			}
		}

		public static string[] GetFileNames(string searchPattern)
		{
			using (IsolatedStorageFile isoStore = GetStore())
			{
				try
				{
					return isoStore.GetFileNames(searchPattern);
				}
				catch (DirectoryNotFoundException ex)
				{
					throw ex;
				}
				catch (IOException ex)
				{
					var path = GetIsolatedStoragePath(isoStore);
					string message = String.Format("Probably corrupted files or folders (searchPattern: {0}) are in the IsolatedStorage: {1}", searchPattern, path);
					throw new Exception(message, ex);
				}
				
			}
		}

		public static string GetIsolatedStoragePath(IsolatedStorageFile isoStore = null)
		{
			if (isoStore != null)
			{
				return isoStore.GetType().GetField("m_RootDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(isoStore).ToString();
			}
			using (isoStore = GetStore())
			{
				return isoStore.GetType().GetField("m_RootDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(isoStore).ToString();
			}
		}

		public static void RemoveIsolatedStorage()
		{
			using (IsolatedStorageFile isoStore = GetStore())
			{
				isoStore.Remove();
			}
		}

		public static bool Save<T>(string path, T itemToSave)
		{
			return IsolatedStoragePersistenceManagerGeneric<T>.Save(path, itemToSave);
		}

		public static bool Load<T>(string path, out T value)
		{
			return IsolatedStoragePersistenceManagerGeneric<T>.Load(path, out value);
		}

		public static bool Exists(string path)
		{
			using (IsolatedStorageFile isoStore = GetStore())
			{
				var files = isoStore.GetFileNames(path);
				return (files != null && files.Length == 1);
			}
		}
		
		public static DateTime? GetCreationTime(string path)
		{
			using (IsolatedStorageFile isoStore = GetStore())
			{
				var files = isoStore.GetFileNames(path);
				if (files != null && files.Length == 1)
				{
					return isoStore.GetCreationTime(path).DateTime;
				}

				return null;
			}
		}

		public static bool Delete(string path)
		{
			try
			{
				using (IsolatedStorageFile isoStore = GetStore())
				{
					if (Exists(path))
						isoStore.DeleteFile(path);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to delete item from path: " + path, ex);
				return false;
			}
			return true;
		}

		public static bool DeleteDirectory(string dir)
		{
			try
			{
				using (IsolatedStorageFile isoStore = GetStore())
				isoStore.DeleteDirectory(dir);
			}
			catch (Exception ex)
			{
				log.Error("Unable to delete directory: " + dir, ex);
				return false;
			}
			return true;
		}
		private static class IsolatedStoragePersistenceManagerGeneric<T>
		{

			public static bool Save(string path, T itemToSave)
			{
				try
				{
					using (IsolatedStorageFile isoStore = GetStore())
					{
						IFormatter formatter = new BinaryFormatter();

						Stream stream;
						stream = new IsolatedStorageFileStream(path, FileMode.Create, FileAccess.Write, isoStore);
						//    stream = new FileStream(path, FileMode.Create, FileAccess.Write);

						using (stream)
						{
							using (var ms = new MemoryStream())
							{
								formatter.Serialize(ms, itemToSave);	//Serializing big/complex data right into isf stream was slow. 
								ms.WriteTo(stream);
							}
						}
					}
				}
				catch (Exception ex)
				{
					log.Error("Unable to save item to path: " + path, ex);
					return false;
				}
				return true;
			}

			public static bool Load(string path, out T value)
			{
				try
				{
					T obj;

					using (IsolatedStorageFile isoStore = GetStore())
					{
						IFormatter formatter = new BinaryFormatter();
						formatter.Binder = LegacyBinder.Instance;

						//Opening a stream
						Stream stream;
						stream = new IsolatedStorageFileStream(path, FileMode.Open, FileAccess.Read, isoStore);
						//    stream = new FileStream(path, FileMode.Open, FileAccess.Read);

						using (stream)
						{
							if (stream.Length == 0)
							{
								log.Error("Deleting empty item from path: " + path);
								try
								{
									stream.Close();
									isoStore.DeleteFile(path);
								}
								catch (Exception ex)
								{
									log.Error("Unable to delete empty item from path: " + path, ex);
								}
								value = default(T);
								return false;
							}
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
			}
		}

		private class LegacyBinder : SerializationBinder
		{
			public static readonly LegacyBinder Instance = new LegacyBinder();

			private LegacyBinder()
			{
			}

			public override Type BindToType(string assemblyName, string typeName)
			{
				var newTypeName = typeName;
				switch (typeName)
				{
					case "Tct.ActivityRecorderClient.WorkDetector+WorkDetectorSettings":
						newTypeName = "Tct.ActivityRecorderClient.Capturing.Core.WorkDetector+WorkDetectorSettings";
						break;
				}
				return Type.GetType(newTypeName + ", " + assemblyName); //no strong name so assemName is ok (Juval Lowy): http://msdn.microsoft.com/en-us/magazine/cc163902.aspx
			}
		}

		#region Migration

		public static bool SaveAllDataForMigration()
		{
			try
			{
				var migrationPath = Path.Combine(Path.GetTempPath(), GetMigrationDirectoryName());
				if (Directory.Exists(migrationPath))
				{
					log.Warn("Deleting a migration directory that already exists. (" + migrationPath + ")");
					Directory.Delete(migrationPath, true);
				}

				using (var isoStore = GetStore())
				{
					var queue = new Queue<string>();
					queue.Enqueue("");
					while (queue.Count > 0)
					{
						var dir = queue.Dequeue();
						Directory.CreateDirectory(Path.Combine(migrationPath, dir));
						var searchPattern = String.IsNullOrEmpty(dir) ? "*" : dir + "\\*";
						foreach (string fileName in isoStore.GetFileNames(searchPattern))
							CopyFromIsolatedStroageNoThrow(fileName, Path.Combine(migrationPath, fileName), isoStore);
						foreach (string directoryName in isoStore.GetDirectoryNames(searchPattern))
							queue.Enqueue(directoryName);
					}
				}
				log.InfoFormat("Content of isolated storage has been saved to temp dir for migration. ({0})", migrationPath);
			}
			catch (Exception ex)
			{
				log.Error("Unable to save all data for migration. ", ex);
				return false;
			}
			return true;
		}

		public static bool TryMigrateData()
		{
			try
			{
				var migrationPath = Path.Combine(Path.GetTempPath(), GetMigrationDirectoryName());
				if (!Directory.Exists(migrationPath)) return false;

				using (IsolatedStorageFile isoStore = GetStore())
				{
					var queue = new Queue<string>();
					queue.Enqueue(migrationPath);
					while (queue.Count > 0)
					{
						var dir = queue.Dequeue();
						var relDir = dir.Substring(migrationPath.Length);
						if (!String.IsNullOrEmpty(relDir)) isoStore.CreateDirectory(relDir);
						foreach (string fileName in Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly))
							CopyToIsolatedStorageNoThrow(fileName, fileName.Substring(migrationPath.Length), isoStore);
						foreach (string directoryName in Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly))
							queue.Enqueue(directoryName);
					}
				}
				log.InfoFormat("Data has been migrated from temp dir to isolated storage. ({0})", migrationPath);
				Directory.Delete(migrationPath, true);
			}
			catch (Exception ex)
			{
				log.Error("Unable to migrate data from temp dir. ", ex);
				return false;
			}
			return true;
		}

		private static void CopyFromIsolatedStroageNoThrow(string sourceFileName, string destinationFileName, IsolatedStorageFile isoStore)
		{
			try
			{
				//Isolated storage doesn't allow us to copy files from it. We can't get path for it without hax either. So we need to read and write contents of files.
				using (var source = new IsolatedStorageFileStream(sourceFileName, FileMode.Open, FileAccess.Read, isoStore))
				using (var destination = new FileStream(destinationFileName, FileMode.Create, FileAccess.Write))
				{
					Copy(source, destination);
				}
			}
			catch (Exception ex)
			{
				log.Error("Failed to copy from isolated storage. " + sourceFileName + " -> " + destinationFileName, ex);
			}
		}

		private static void CopyToIsolatedStorageNoThrow(string sourceFileName, string destinationFileName, IsolatedStorageFile isoStore)
		{
			try
			{
				var fileNames = isoStore.GetFileNames(destinationFileName);
				if (fileNames != null && fileNames.Length == 1) //Multiple match???
				{
					log.Warn("File already exists in isolated storage. So we don't overwrite it. (" + destinationFileName + ")");
					return;
				}

				//Isolated storage doesn't allow us to copy files into it. We can't get path for it without hax either. So we need to read and write contents of files.
				using (var source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read))
				using (var destination = new IsolatedStorageFileStream(destinationFileName, FileMode.Create, FileAccess.Write, isoStore))
				{
					Copy(source, destination);
				}
			}
			catch (Exception ex)
			{
				log.Error("Failed to copy to isolated storage. " + sourceFileName + " -> " + destinationFileName, ex);
			}
		}

		private static void Copy(Stream source, Stream destination)
		{
			const int chunkSize = 4096;
			var bytes = new byte[chunkSize];
			int byteCount;
			while ((byteCount = source.Read(bytes, 0, chunkSize)) > 0)
			{
				destination.Write(bytes, 0, byteCount);
			}
		}

		private const string migrationDirPrefix = "jc_migration_";
		private static string GetMigrationDirectoryName()
		{
			return migrationDirPrefix + GetCurrentUserName();
		}

		private static string GetCurrentUserName()
		{
			try
			{
				var identity = WindowsIdentity.GetCurrent();
				return identity != null ? identity.Name.Replace('\\', '_') : "";
			}
			catch (Exception ex)
			{
				log.Error("Get name of current user failed.", ex);
			}
			return "";
		}

		#endregion
	}
}
