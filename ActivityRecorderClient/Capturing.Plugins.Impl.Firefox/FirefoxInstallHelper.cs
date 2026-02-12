using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Firefox
{
	public static class FirefoxInstallHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static int isInstalled;
		private const string addonId = "ffinterop@jobctrl.com";
		private const string addonFileExt = "_signed.xpi";
		private static readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if DEBUG
		internal static readonly string xpiDirName = Path.Combine(appDir, "FirefoxInterop");
#else
		internal static readonly string xpiDirName = Path.Combine(appDir, "v" + ConfigManager.Version, "FirefoxInterop");
#endif
		private static readonly string ffDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.Combine("Mozilla", "Firefox"));
		private static readonly string ffProfileFile = Path.Combine(ffDataPath, "profiles.ini");
		private const string extensionJsonFileName = "extensions.json";
		private const string jcFfJsonSnippet = "{{\"id\":\"{0}\",\"syncGUID\":\"{{5c412c7e-503a-483c-8f90-06d0746c2c2d}}\",\"version\":\"1.11.0\",\"type\":\"extension\",\"loader\":null,\"updateURL\":null,\"installOrigins\":null,\"manifestVersion\":2,\"optionsURL\":null,\"optionsType\":null,\"optionsBrowserStyle\":true,\"aboutURL\":null,\"defaultLocale\":{{\"name\":\"JobCTRLExtension\",\"description\":\"ExtensiontosupportJobCTRLautomaticrulestoworkwithFirefox.\",\"creator\":null,\"developers\":null,\"translators\":null,\"contributors\":null}},\"visible\":true,\"active\":true,\"userDisabled\":false,\"appDisabled\":false,\"embedderDisabled\":false,\"installDate\":{1},\"updateDate\":{1},\"applyBackgroundUpdates\":1,\"path\":\"{2}\",\"skinnable\":false,\"sourceURI\":\"{3}\",\"releaseNotesURI\":null,\"softDisabled\":false,\"foreignInstall\":false,\"strictCompatibility\":true,\"locales\":[],\"targetApplications\":[{{\"id\":\"toolkit@mozilla.org\",\"minVersion\":\"57.0\",\"maxVersion\":null}}],\"targetPlatforms\":[],\"signedState\":2,\"signedDate\":1556300000000,\"seen\":true,\"dependencies\":[],\"incognito\":\"spanning\",\"userPermissions\":{{\"permissions\":[\"nativeMessaging\",\"tabs\"],\"origins\":[\"*://*/*\",\"*://mail.google.com/*\"]}},\"optionalPermissions\":{{\"permissions\":[],\"origins\":[]}},\"icons\":{{\"128\":\"icon.png\"}},\"iconURL\":null,\"blocklistState\":0,\"blocklistURL\":null,\"startupData\":{{}},\"hidden\":false,\"installTelemetryInfo\":{{\"source\":\"other\",\"method\":\"other\"}},\"recommendationState\":null,\"rootURI\":\"jar:{4}!/\",\"location\":\"app-profile\"}}";
		private const string nativeHostManifestFileName = "com.tct.jobctrl.json";
		private const string nativeHostRegKey = @"Software\Mozilla\NativeMessagingHosts\com.tct.jobctrl";

		private const string extensionForceRegKey = @"Software\Policies\Mozilla\Firefox\";
		private const string extensionSettingsRegKey = "ExtensionSettings";
		private const string extensionSettingsRegJson = "{{\r\n  \"ffinterop@jobctrl.com\": {{\r\n    \"installation_mode\": \"force_installed\",\r\n    \"install_url\": \"{0}\"\r\n  }}\r\n}}\r\n";
		private const string pureExtensionFileName = addonId + ".xpi";

		public static void InstallAddonOneTimeIfApplicable()
		{
			if (Interlocked.CompareExchange(ref isInstalled, 1, 0) != 0) return;
			ThreadPool.QueueUserWorkItem(_ => InstallAddonNoThrow());
		}
		static FirefoxInstallHelper()
		{
			try
			{
				var path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe", "", null);
				if (path != null)
				{
					var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(path.ToString()).FileVersion;
					log.Debug("Firefox version: " + version);

				}
			}
			catch (Exception ex)
			{
				log.Warn("Accessing firefox version failed", ex);
			}
		}

		private static void RegisterAddonWith(string path)
		{
			var key = @"SOFTWARE\Mozilla\Firefox\Extensions";
			using (var regkey = Registry.CurrentUser.CreateSubKey(key))	//This key is not redirected under HKEY_CURRENT_USER
			{
				var regVal = regkey.GetValue(addonId);
				if (regVal != null)
				{
					if (regVal.ToString() != path)
						regkey.DeleteValue(addonId, false); //delete old key if exists
				}
				regkey.SetValue(addonId, path);
			}

			if (ProcessElevationHelper.IsElevated())
			{
				SetOrUpdateRegistryValue(path, Registry.LocalMachine);
			} else
			{
				InstallAddonWith(path);
			}
		}

		private static void SetOrUpdateRegistryValue(string extensionFilePath, RegistryKey rootKey)
		{
			using (var subKey = rootKey.OpenSubKey(extensionForceRegKey, true))
			{
				if (subKey == null)
				{
					using (var regkey = rootKey.CreateSubKey(extensionForceRegKey))
					{
						regkey.SetValue(extensionSettingsRegKey, string.Format(extensionSettingsRegJson, new Uri(extensionFilePath).AbsoluteUri));
					}
				}
				else
				{
					var subKeyValue = subKey.GetValue(extensionSettingsRegKey);
					if (subKeyValue == null)
					{
						subKey.SetValue(extensionSettingsRegKey, string.Format(extensionSettingsRegJson, new Uri(extensionFilePath).AbsoluteUri).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
						return;
					}
					dynamic des = JsonConvert.DeserializeObject(string.Join("", (string[])subKeyValue));
					if (des[addonId] != null)
					{
						des[addonId].installation_mode = "force_installed";
						des[addonId].install_url = new Uri(extensionFilePath).AbsoluteUri;
						subKey.SetValue(extensionSettingsRegKey, JsonConvert.SerializeObject(des, Newtonsoft.Json.Formatting.Indented).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
					}
					else
					{
						des[addonId] = new
						{
							installation_mode = "force_installed",
							install_url = new Uri(extensionFilePath).AbsoluteUri
						};
						subKey.SetValue(extensionSettingsRegKey, JsonConvert.SerializeObject(des, Newtonsoft.Json.Formatting.Indented).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
					}
				}
			}
		}

		private static void RegisterNativeHost(string path)
		{
			using (var regkey = Registry.CurrentUser.CreateSubKey(nativeHostRegKey))
			{
				var regVal = regkey.GetValue("");
				if (regVal != null)
				{
					if (regVal.ToString() == path)
						return;
					regkey.DeleteValue("", false); //delete old key if exists
				}
				regkey.SetValue("", path);
			}
		}

		private static void InstallAddonNoThrow()
		{
#if DEBUG
			return;
#endif
			try
			{
				var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var dir = Path.Combine(currDir, xpiDirName);

				var addonFileName = addonId + addonFileExt;
				var path = Path.Combine(dir, addonFileName);

				RegisterNativeHost(Path.Combine(dir, nativeHostManifestFileName));
				RegisterAddonWith(path);
				ActivateAddonWith(path);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error during addon install", ex);
			}
		}

		private static void InstallAddonWith(string path)
		{
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
				log.Warn("default profile not found");
				return;
			}
			var isRelative = profilesIni.Read("IsRelative", section).Equals("1");
			var profileDirectoryPath = Path.Combine(isRelative ? ffDataPath : "", profilesIni.Read("Path", section));
			var extJsonPath = Path.Combine(isRelative ? ffDataPath : "", Path.Combine(profilesIni.Read("Path", section), extensionJsonFileName));
			var extJson = File.ReadAllText(extJsonPath);
			var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(extJson),
				new XmlDictionaryReaderQuotas());
			var root = XDocument.Load(jsonReader).Root;
			var addons = root.XPathSelectElement("addons");

			var jcAddon = addons.XPathSelectElement("item[id = \"" + addonId + "\"]");
			if (jcAddon == null)
			{
				File.Copy(path, Path.Combine(profileDirectoryPath, "extensions", pureExtensionFileName));
				Console.WriteLine("xpi file copied.");
				addons.Add(string.Format(jcFfJsonSnippet, addonId,
						(long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds,
						Path.Combine(profileDirectoryPath, "extensions", pureExtensionFileName),
						new Uri(path).AbsoluteUri,
						new Uri(Path.Combine(profileDirectoryPath, "extensions", pureExtensionFileName)).AbsoluteUri));
				root.Save(extJsonPath, SaveOptions.DisableFormatting);
				Console.WriteLine("Enabled extension added to FF profile");
				var prefsJsFile = Path.Combine(profileDirectoryPath, "prefs.js");
				var lines = File.ReadAllLines(prefsJsFile);
				var newLines = new List<string>();
				foreach (var line in lines)
				{
					if (line.StartsWith("user_pref(\"extensions.webextensions.uuids\","))
					{
						newLines.Add(line.Substring(0, line.Length - 4) + $",\\\"ffinterop@jobctrl.com\\\":\\\"{Guid.NewGuid()}\\\"}}\");");
					}
					else
					{
						newLines.Add(line);
					}
				}
				File.WriteAllLines(prefsJsFile, newLines);
				Console.WriteLine("prefs.js updated");
				return;
			}
		}

		private static void ActivateAddonWith(string path)
		{
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
				log.Warn("default profile not found");
				return;
			}
			var isRelative = profilesIni.Read("IsRelative", section).Equals("1");
			var extJsonPath = Path.Combine(isRelative ? ffDataPath : "", Path.Combine(profilesIni.Read("Path", section), extensionJsonFileName));
			var extJson = File.ReadAllText(extJsonPath);
			var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(extJson),
				new XmlDictionaryReaderQuotas());
			var root = XDocument.Load(jsonReader).Root;
			var addons = root.XPathSelectElement("addons");
			var jcAddon = addons.XPathSelectElement("item[id = \"" + addonId + "\"]");
			if (jcAddon == null)
			{
				log.Warn("Couldn't find extension in FF profile");
				return;
			}
			var isActive = jcAddon.Element("active");
			var isUserDisabled = jcAddon.Element("userDisabled");
			var isAppDisabled = jcAddon.Element("appDisabled");
			if (isActive != null && isActive.Value.ToLower() == "true" && isUserDisabled != null &&
			    isUserDisabled.Value.ToLower() == "false" && isAppDisabled != null && isAppDisabled.Value.ToLower() == "false")
				return;
			jcAddon.SetElementValue("active", true);
			jcAddon.SetElementValue("userDisabled", false);
			jcAddon.SetElementValue("appDisabled", false);
			jcAddon.SetElementValue("seen", true);
			var itemTypenull = root.XPathSelectElements(".//*[@type = \"null\"]");
			foreach (var item in itemTypenull)
			{
				item.SetAttributeValue("type", "string");
			}
			var ms = new MemoryStream();
			var jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8);
			root.WriteTo(jsonWriter);
			jsonWriter.Flush();
			jsonWriter.Close();
			var nJson = Encoding.UTF8.GetString(ms.ToArray());
			nJson = nJson.Replace("\":\"\"", "\":null").Replace("\\/", "/");
			File.WriteAllText(extJsonPath, nJson);
			log.Debug("Disabled extension changed to enabled");
		}

		private static void CopyStream(Stream input, Stream output)
		{
			if (input == null) throw new ArgumentNullException("input");
			if (output == null) throw new ArgumentNullException("output");

			var buffer = new byte[8192];
			int bytesRead;
			while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, bytesRead);
			}
		}
	}
}
