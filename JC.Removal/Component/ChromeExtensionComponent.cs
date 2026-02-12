using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;

namespace JC.Removal.Component
{
	class ChromeExtensionComponent: BaseComponent
	{
		private const string nativeHostRegKey = @"Software\Google\Chrome\NativeMessagingHosts\com.tct.jobctrl";
		private const string currentExtensionId = "obmlbfkihnobgbokahopnbaehffncfoe";
		private static readonly KeyValuePair<string, string>[] oldExtensionIds = { new KeyValuePair<string, string>("cdiboboglhphpeooagpmpkfbokmffeme", "7498"), new KeyValuePair<string, string>("mpjhadgoffhidbcmjjcofofkfgbpnfbd", "7499") };
		private const string extensionForceRegKey = @"Software\Policies\Google\Chrome\ExtensionInstallForcelist\";
		private const string extensionRegKey = @"Software\Google\Chrome\Extensions\";
		private const string preferencesPath = "Google\\Chrome\\User Data\\Default\\Preferences";
		private const string currentExtensionIdNum = "7500";
		private const string userFriendlyName = "Chrome extension";

		public override string GetUserFriendlyName()
		{
			return userFriendlyName;
		}

		public override bool Remove(out string error)
		{
			try
			{
				var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), preferencesPath);
				var prefs = File.ReadAllText(path);
				var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(prefs),
					new XmlDictionaryReaderQuotas());
				var root = XDocument.Load(jsonReader).Root;
				if (root == null)
				{
					error = "Chrome preferences root not found.";
					return false;
				}
				var extensionsElement = root?.Element("extensions");
				var settingsElement = extensionsElement?.Element("settings");
				var jcExtensionElement = settingsElement?.Element(currentExtensionId);
				jcExtensionElement?.Remove();
				var ms = new MemoryStream();
				var jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8);
				root.WriteTo(jsonWriter);
				jsonWriter.Flush();
				jsonWriter.Close();
				var nJson = Encoding.UTF8.GetString(ms.ToArray());
				nJson = nJson.Replace("\":\"\"", "\":null").Replace("\\/", "/");
				File.WriteAllText(path, nJson);
				if (IsElevatedProcess())
				{
					DeleteRegistryValue(Registry.LocalMachine, extensionForceRegKey, currentExtensionIdNum);
				}
				DeleteRegistryKey(Registry.CurrentUser, extensionRegKey + currentExtensionId);
				DeleteRegistryKey(Registry.CurrentUser, nativeHostRegKey);
				error = null;
				return true;
			}
			catch (Exception ex)
			{
				error = ex.ToString();
				return false;
			}
		}

		private static void DeleteRegistryValue(RegistryKey rootKey, string key, string name)
		{
			using (var regkey = rootKey.OpenSubKey(key, true))
			{
				regkey?.DeleteValue(name, false);
			}
		}

		private static void DeleteRegistryKey(RegistryKey rootKey, string key)
		{
			using (var regkey = rootKey.OpenSubKey(key, false))
			{
				if (regkey == null) return;
			}
			rootKey.DeleteSubKeyTree(key);
		}

		private static bool IsElevatedProcess()
		{
			if (!(Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6)) return true;
			return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
		}

		public override bool IsInstalled()
		{
			if (IsElevatedProcess())
				using (var regKey = Registry.LocalMachine.OpenSubKey(extensionForceRegKey))
					if (regKey?.GetValue(currentExtensionIdNum) != null) return true;
			using (var regKey = Registry.CurrentUser.OpenSubKey(extensionRegKey + currentExtensionId))
				if (regKey != null)
					return true;
			using (var regKey = Registry.CurrentUser.OpenSubKey(nativeHostRegKey))
				if (regKey != null)
					return true;
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), preferencesPath);
			var prefs = File.ReadAllText(path);
			var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(prefs),
				new XmlDictionaryReaderQuotas());
			var root = XDocument.Load(jsonReader).Root;
			if (root == null)
			{
				return false;
			}
			var extensionsElement = root?.Element("extensions");
			var settingsElement = extensionsElement?.Element("settings");
			var jcExtensionElement = settingsElement?.Element(currentExtensionId);
			if (jcExtensionElement != null) return true;
			return false;
		}

		public override string[] GetProcessesNames()
		{
			return new[] { "chrome.exe", "JC.Chrome.exe" };
		}
	}
}
