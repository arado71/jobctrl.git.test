using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using log4net;

namespace Tct.ActivityRecorderClient.Serialization
{
	public static class ProtectedDataSerializationHelper
	{
		public static bool Save<T>(string path, T itemToSave)
		{
			byte[] protectedData;
			return ProtectedDataHelper.Protect(itemToSave, out protectedData) && IsolatedStorageSerializationHelper.Save(path, protectedData);
		}

		public static bool Load<T>(string path, out T value)
		{
			byte[] protectedData;
			if (IsolatedStorageSerializationHelper.Load(path, out protectedData))
			{
				return ProtectedDataHelper.Unprotect(protectedData, out value);
			}
			else
			{
				value = default(T);
				return false;
			}
		}

		public static bool Exists(string path)
		{
			return IsolatedStorageSerializationHelper.Exists(path);
		}

		public static bool Delete(string path)
		{
			return IsolatedStorageSerializationHelper.Delete(path);
		}
	}

	public static class ProtectedDataHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static bool Protect<T>(T itemToProtect, out byte[] protectedData)
		{
			try
			{
				using (var memStream = new MemoryStream())
				{
					IFormatter formatter = new BinaryFormatter();
					formatter.Serialize(memStream, itemToProtect);
					protectedData = ProtectedData.Protect(memStream.ToArray(), null, DataProtectionScope.CurrentUser);
					return true;
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to protect item.", ex);
				protectedData = null;
				return false;
			}
		}

		public static bool Unprotect<T>(byte[] dataToUnprotect, out T unprotectedItem)
		{
			try
			{
				using (var memStream = new MemoryStream())
				{
					var unprotectedData = ProtectedData.Unprotect(dataToUnprotect, null, DataProtectionScope.CurrentUser);
					memStream.Write(unprotectedData, 0, unprotectedData.Length);
					memStream.Seek(0, SeekOrigin.Begin);
					IFormatter formatter = new BinaryFormatter();
					unprotectedItem = (T)formatter.Deserialize(memStream);
					return true;
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to unprotect data.", ex);
				unprotectedItem = default(T);
				return false;
			}
		}
	}
}
