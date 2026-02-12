using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using VoxCTRL.ActivityRecorderServiceReference;
using log4net;

namespace VoxCTRL
{
	[Serializable]
	public class LoginData
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public int UserId;
		public string UserPassword;
		public AuthData AuthData;
		public bool RememberMe;
		public DateTime? UserPasswordExpirationDate { get; set; }

		public static void DeleteFromDisk()
		{
			var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigManager.ApplicationName);
			var path = Path.Combine(dir, "DefaultUserId");
			if (File.Exists(path))
			{
				File.Delete(path);
				log.DebugFormat("LoginData deleted");
			}
		}

		public static void SaveToDisk(LoginData itemToSave)
		{
			var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigManager.ApplicationName);
			Directory.CreateDirectory(dir);
			var path = Path.Combine(dir, "DefaultUserId");
			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			using (var memStream = new MemoryStream())
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(memStream, itemToSave);
				memStream.Flush();
				var protectedData = ProtectedData.Protect(memStream.ToArray(), null, DataProtectionScope.CurrentUser);
				stream.Write(protectedData, 0, protectedData.Length);
			}
		}

		public static LoginData LoadFromDisk()
		{
			try
			{
				var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigManager.ApplicationName);
				var path = Path.Combine(dir, "DefaultUserId");
				if (!File.Exists(path)) return null;
				using (var memStream = new MemoryStream())
				{
					var fileData = File.ReadAllBytes(path);
					var unprotectedData = ProtectedData.Unprotect(fileData, null, DataProtectionScope.CurrentUser);
					memStream.Write(unprotectedData, 0, unprotectedData.Length);
					memStream.Seek(0, SeekOrigin.Begin);
					IFormatter formatter = new BinaryFormatter();
					return (LoginData)formatter.Deserialize(memStream);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to load data from disk", ex);
				return null;
			}
		}
	}
}
