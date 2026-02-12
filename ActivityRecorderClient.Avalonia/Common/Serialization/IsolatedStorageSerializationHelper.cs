using log4net;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Tct.ActivityRecorderClient.Serialization
{

	public static class IsolatedStorageSerializationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly byte[] HmacKey = Encoding.UTF8.GetBytes("JobCTRL");
		private static readonly int HmacSize = 32;

		//http://blogs.msdn.com/shawnfa/archive/2006/01/20/514411.aspx
		//IsClickOnce = AppDomain.CurrentDomain.ActivationContext != null
		//System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed is the same but slower
		private static IsolatedStorageFile GetStore()
		{
			if (ConfigManager.IsAppLevelStorageNeeded)
			{
				return IsolatedStorageFile.GetUserStoreForApplication();
			}
			else
			{
				//url/path is used which is not ideal but we don't care atm because this should be a ClickOnce program
				return ConfigManager.IsRoamingStorageScopeNeeded
					? IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Roaming, null, null)
					: IsolatedStorageFile.GetUserStoreForAssembly();
			}
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
				catch (DirectoryNotFoundException)
				{
					throw;
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

		public static bool Save<T>(string path, T itemToSave, Type[]? knownTypes = null)
		{
			return IsolatedStoragePersistenceManagerGeneric<T>.Save(path, itemToSave, knownTypes);
		}

		public static bool Load<T>(string path, out T value, Type[]? knownTypes = null)
		{
			return IsolatedStoragePersistenceManagerGeneric<T>.Load(path, out value, knownTypes);
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
					try
					{
						return isoStore.GetCreationTime(path).DateTime;
					}
					catch (ArgumentOutOfRangeException)
					{
						log.Warn($"The file has invalid creation time ({path})");
					}
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
				{
					isoStore.DeleteDirectory(dir);
				}
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

			public static bool Save(string path, T itemToSave, Type[]? knownTypes = null)
			{
				try
				{
					using (IsolatedStorageFile isoStore = GetStore())
					{
						Stream stream;
						try
						{
							stream = new IsolatedStorageFileStream(path, FileMode.Create, FileAccess.Write, isoStore);
						}
						catch (DirectoryNotFoundException)
						{
							log.Warn($"The directory of this path not found, recreating... ({path})");
							CreateDir(Path.GetDirectoryName(path));
							stream = new IsolatedStorageFileStream(path, FileMode.Create, FileAccess.Write, isoStore);
						}

						using (stream)
						{
							using (var ms = new MemoryStream())
							{
								var serializer = new DataContractSerializer(typeof(T), knownTypes);
								serializer.WriteObject(ms, itemToSave);

								ms.Position = 0;
								byte[] mac;
								using (var hmac = new HMACSHA256(HmacKey))
								{
									mac = hmac.ComputeHash(ms);
								}

								stream.Write(mac, 0, mac.Length);
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

			public static bool Load(string path, out T value, Type[]? knownTypes = null)
			{
				try
				{
					T obj;

					using (IsolatedStorageFile isoStore = GetStore())
					{
						var stream = new IsolatedStorageFileStream(path, FileMode.Open, FileAccess.Read, isoStore);
						using (stream)
						{
							if (stream.Length <= HmacSize)
							{
								log.Error("Deleting empty/invalid item from path: " + path);
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
							byte[] storedMac = new byte[HmacSize];
							stream.Read(storedMac, 0, HmacSize);
							byte[] computedMac;
							using (var hmac = new HMACSHA256(HmacKey))
							{
								computedMac = hmac.ComputeHash(stream);
							}
							if (!storedMac.SequenceEqual(computedMac))
							{
								log.Error("MAC mismatch - file tampered on path: " + path);
								value = default;
								return false;
							}

							stream.Position = HmacSize;
							var serializer = new DataContractSerializer(typeof(T), knownTypes);
							obj = (T)serializer.ReadObject(stream);
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
	}
}
