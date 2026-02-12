using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using JC.Removal.Utils;
using Microsoft.Win32;

namespace JC.Removal.Component
{
	class FirefoxExtensionComponent: BaseComponent
	{
		private const string extensionPath = @"SOFTWARE\Mozilla\Firefox\Extensions";
		private const string addonId = "ffinterop@jobctrl.com";
		private const string nativeHostRegKeyPath = @"Software\Mozilla\NativeMessagingHosts\com.tct.jobctrl";
		private const string extensionJsonFileName = "extensions.json";
		private static readonly string ffDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.Combine("Mozilla", "Firefox"));
		private static readonly string ffProfileFile = Path.Combine(ffDataPath, "profiles.ini");
		private static string userFriendlyName = "Firefox extension";
		
		public override string GetUserFriendlyName()
		{
			return userFriendlyName;
		}

		public override bool Remove(out string error)
		{
			try
			{
				error = null;
				using(var regkey = Registry.CurrentUser.OpenSubKey(extensionPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
					regkey?.DeleteValue(addonId, false);
				Registry.CurrentUser.DeleteSubKey(nativeHostRegKeyPath, false);

				var profilesIni = new IniFile(ffProfileFile);
				string profileName = null;
				string section = null;
				for (int i = 0; i < 10; i++)
				{
					var nsec = "Profile" + i;
					if (!profilesIni.KeyExists("Name", nsec)) break;
					section = nsec;
					profileName = profilesIni.Read("Name", section);
					if (profilesIni.Read("Default", section).Equals("1")) break;
				}
				if (string.IsNullOrEmpty(profileName))
				{
					error = "Default profile not found. Maybe Firefox is not installed.";
					return true;
				}
				var isRelative = profilesIni.Read("IsRelative", section).Equals("1");
				var extJsonPath = Path.Combine(isRelative ? ffDataPath : "", Path.Combine(profilesIni.Read("Path", section), extensionJsonFileName));
				var extJson = File.ReadAllText(extJsonPath);
				var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(extJson),
					new XmlDictionaryReaderQuotas());
				var root = XDocument.Load(jsonReader).Root;
				var addons = root?.XPathSelectElement("addons");
				var jcAddon = addons?.XPathSelectElement("item[id = \"" + addonId + "\"]");
				if (jcAddon != null)
				{
					var pathToExt = jcAddon.Element("path")?.Value;
					if(pathToExt != null)
						File.Delete(pathToExt);
					jcAddon.Remove();
					var ms = new MemoryStream();
					var jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8);
					root.WriteTo(jsonWriter);
					jsonWriter.Flush();
					jsonWriter.Close();
					var nJson = Encoding.UTF8.GetString(ms.ToArray());
					nJson = nJson.Replace("\":\"\"", "\":null").Replace("\\/", "/");
					File.WriteAllText(extJsonPath, nJson);
				}
				return true;
			}
			catch (Exception ex)
			{
				error = ex.ToString();
				return false;
			}
		}

		public override bool IsInstalled()
		{
			using (var regkey = Registry.CurrentUser.OpenSubKey(extensionPath))
				if (regkey?.GetValue(addonId) != null)
					return true;
			using (var nativeHostRegKey = Registry.CurrentUser.OpenSubKey(nativeHostRegKeyPath))
				if (nativeHostRegKey != null)
					return true;
			return false;
		}

		public override string[] GetProcessesNames()
		{
			return new[] { "firefox.exe", "JC.FF.exe" };
		}
	}
}
