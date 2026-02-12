using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using log4net.Appender;
using Microsoft.Win32;

namespace Tct.ActivityRecorderClient.Capturing.Mail
{
	internal static class OutlookAddinInstallHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string outlookPluginFriendlyName = "Mail Activity Tracker";
		private const string outlookAddinKey = "MailActivityTracker{0}";
		private const string outlookPluginDescription = "JobCTRL Mail Activity Tracker Addin for Outlook";
		private const string outlookPluginSearchPattern = "(?:(?:MailActivityTracker)|(?:" + outlookPluginFriendlyName + "))(?<hash>[0-9A-Z]*)";
		private const string outlookAddinsRootKey = @"Software\Microsoft\Office\Outlook\Addins";
		private const string outlookAddSubjectSuffixKey = "AddSubjectSuffix";
		private const string outlookDisabledAddins = @"SOFTWARE\Microsoft\Office\{0}\Outlook\Resiliency\DisabledItems";
		private const string outlookDoNotDisableAddins = @"SOFTWARE\Microsoft\Office\{0}\Outlook\Resiliency\DoNotDisableAddinList";
		private const string vstolocalSuffix = "|vstolocal";
		private static readonly List<string> spareAddins = new List<string>();
		private static readonly Regex versionPartRegex = new Regex(@"\\v\d+[.]\d+[.]\d+[.]\d+");
		private static readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		private static readonly Assembly addinAssembly = Assembly.GetAssembly(typeof(OutlookAddinInstallHelper));
		private static readonly Version addinVersion = addinAssembly.GetName().Version;
#if DEBUG
		internal static readonly string AddinDir = Path.Combine(appDir, "OutlookAddin");
#else
		internal static readonly string AddinDir = Path.Combine(appDir, "v" + addinVersion, "OutlookAddin");
#endif
		public static string OutlookAddinLocHash { get; private set; }

		static OutlookAddinInstallHelper()
		{
			// TODO: need a more sophisticated method for per machine detection
			var isPerMachineInstalled = addinAssembly.Location != null && addinAssembly.Location.Contains("Program Files");
			if (isPerMachineInstalled)
				log.Info("Per machine installed");

			var directoryName = Path.GetDirectoryName(addinAssembly.CodeBase);
			if (directoryName == null) return;
#if DEBUG
			OutlookAddinLocHash = "";
		}
#else
			var location = versionPartRegex.Replace(directoryName.Replace("file:\\", "").Replace("\\OutlookAddin", ""), "");
			OutlookAddinLocHash = CreateConsistentHashCode(location).ToString("X");
			log.Debug("OutlookAddinLocHash: " + OutlookAddinLocHash + " Location: " + location);
		}

		/// <summary>
		/// Consistent hash value for strings on both x86 and x64 platforms
		/// based on http://stackoverflow.com/questions/8838053/can-object-gethashcode-produce-different-results-for-the-same-objects-strings
		/// </summary>
		private static int CreateConsistentHashCode(string obj)
		{
			if (obj == null)
				return 0;
			int hash = obj.Length;
			for (int i = 0; i != obj.Length; ++i)
				hash = (hash << 5) - hash + obj[i];
			return hash;
		}
#endif

		public static string GetAddinNode()
		{
			try
			{
				using (var outlookAddinsKey = Registry.CurrentUser.OpenSubKey(outlookAddinsRootKey))
				{
					if (outlookAddinsKey == null) return null;
					string nodeName = null;
					spareAddins.Clear();
					foreach (var addinName in outlookAddinsKey.GetSubKeyNames())
					{
						var matcher = Regex.Match(addinName, outlookPluginSearchPattern);
						if (!matcher.Success) continue;
						using (var addinKey = outlookAddinsKey.OpenSubKey(addinName))
						{
							if (addinKey == null) continue;
							var hash = matcher.Groups["hash"].Value;
							if (OutlookAddinLocHash == hash && string.IsNullOrEmpty(nodeName))
								nodeName = addinKey.Name;
							else if (string.IsNullOrEmpty(hash)) 
								spareAddins.Add(addinKey.Name);
						}
					}
					return nodeName;
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Cannot open registry", ex);
				return null;
			}
		}

		public static string InstallAddin(string path, string existingRegKey)
		{
#if DEBUG
			return outlookPluginFriendlyName;
#endif
			RegistryKey addinKey = null;
			try
			{
				if (existingRegKey != null)
				{
					existingRegKey = existingRegKey.Replace(@"HKEY_CURRENT_USER\", "");
					addinKey = Registry.CurrentUser.OpenSubKey(existingRegKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
				}
				if (addinKey == null)
				{
					addinKey = Registry.CurrentUser.CreateSubKey(GetAddinKey(), RegistryKeyPermissionCheck.ReadWriteSubTree);
				}
				if (addinKey == null)
				{
					throw new ArgumentException("registry key can't be created");
				}
				addinKey.SetValue("FriendlyName", outlookPluginFriendlyName);
				addinKey.SetValue("Description", outlookPluginDescription);
				addinKey.SetValue("LoadBehavior", 3, RegistryValueKind.DWord);
				if (OutlookVersion == null || !OutLookVersionDot.Contains('.') || !int.TryParse(OutLookVersionDot.Split('.')[0], out var majorVersion)) majorVersion = 0;
				var vstoPath = path != null ? (majorVersion >= 16 ? path : new Uri(path).AbsoluteUri) + vstolocalSuffix : "";
				addinKey.SetValue("Manifest", vstoPath);
				using (var doNotDisableAddins =
					Registry.CurrentUser.CreateSubKey(string.Format(outlookDoNotDisableAddins, OutLookVersionDot), RegistryKeyPermissionCheck.ReadWriteSubTree))
				{
					if (doNotDisableAddins != null)
					{
						doNotDisableAddins.SetValue(AddinName, 1L, RegistryValueKind.DWord);
					}
				}
				foreach (var addin in spareAddins)
				{
					UninstallPlugin(addin);
				}
				// also delete same addins with different key
				using (var outlookAddinsKey = Registry.CurrentUser.OpenSubKey(outlookAddinsRootKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
				{
					if (outlookAddinsKey == null) return addinKey.Name;
					foreach (var addinName in outlookAddinsKey.GetSubKeyNames().Where(n => n != AddinName))
					{
						using (var adi = outlookAddinsKey.OpenSubKey(addinName))
						{
							if (adi == null) continue;
							var manifest = adi.GetValue("Manifest") as string;
							if (vstoPath == manifest)
								outlookAddinsKey.DeleteSubKey(addinName);
						}
					}
				}
				return addinKey.Name;
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Cannot install addin", ex);
				return null;
			}
			finally
			{
				if (addinKey != null)
					addinKey.Close();
			}
		}

		public static bool CheckIfInstalledAndNotDisabled()
		{
			var addinNode = GetAddinNode();
			if (!IsInstalled(addinNode)) return false;
			return !ClearDisabled() && !CheckInactive(addinNode);
		}

		private static string GetAddinKey()
		{
			return outlookAddinsRootKey + @"\" + AddinName;
		}

		private static string AddinName { get { return string.Format(outlookAddinKey, OutlookAddinLocHash); } }

		private static bool ClearDisabled()
		{
			var addinKey = AddinName;
			var addinKeyUtf16 = new byte[addinKey.Length * 2];
			Encoding.Unicode.GetBytes(addinKey, 0, addinKey.Length, addinKeyUtf16, 0);
			var disabledAddins = Registry.CurrentUser.OpenSubKey(string.Format(outlookDisabledAddins, OutLookVersionDot), RegistryKeyPermissionCheck.ReadWriteSubTree);
			if (disabledAddins != null)
			{
				foreach (
					var disabledAddinKey in
						disabledAddins.GetValueNames()
							.Select(k => new { Key = k, Value = disabledAddins.GetValue(k) as byte[] })
							.Where(v => v.Value != null && IsBytesContainBytes(v.Value, addinKeyUtf16)))
				{
					disabledAddins.DeleteValue(disabledAddinKey.Key);
					return true;
				}
			}
			return false;
		}

		private static bool CheckInactive(string addinNode)
		{
			try
			{
				addinNode = addinNode.Replace(@"HKEY_CURRENT_USER\", "");
				using (var addinKey = Registry.CurrentUser.OpenSubKey(addinNode, RegistryKeyPermissionCheck.ReadSubTree))
				{
					if (addinKey == null) return false;
					var loadBehavValue = addinKey.GetValue("LoadBehavior") as int?;
					if (loadBehavValue.HasValue && loadBehavValue.Value != 3)
						return true;
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Cannot open registry", ex);
			}
			return false;
		}

		private static bool IsBytesContainBytes(byte[] source, byte[] pattern)
		{
			var len = pattern.Length;
			var limit = source.Length - len;
			for (var i = 0; i <= limit; i++)
			{
				var k = 0;
				for (; k < len; k++)
				{
					if (pattern[k] != source[i + k]) break;
				}
				if (k == len) return true;
			}
			return false;
		}

		private static void CopyFolder(string sourcePath, string targetPath, bool recursive = true)
		{
			var files = System.IO.Directory.GetFiles(sourcePath);

			foreach (var srcFile in files)
			{
				var fileName = System.IO.Path.GetFileName(srcFile);
				// ReSharper disable once AssignNullToNotNullAttribute
				var destFile = System.IO.Path.Combine(targetPath, fileName);
				File.Copy(srcFile, destFile, true);
			}

			if (!recursive) return;

			foreach (var dir in Directory.GetDirectories(sourcePath))
			{
				var dirName = Path.GetFileName(dir);
				var targPath = Path.Combine(targetPath, dirName);
				Directory.CreateDirectory(targPath);
				CopyFolder(dir, targPath, true);
			}
		}

		public static void UninstallPlugin(string existingRegKey)
		{
			try
			{
				if (existingRegKey != null)
				{
					existingRegKey = existingRegKey.Replace(@"HKEY_CURRENT_USER\", "");
					Registry.CurrentUser.DeleteSubKeyTree(existingRegKey);
				}
			}
			catch (Exception ex)
			{
				log.Error("Cannot uninstall plugin", ex);
			}
		}

		public static bool IsInstalled(string addinKey)
		{
			if (addinKey == null) return false;
			addinKey = addinKey.Replace(@"HKEY_CURRENT_USER\", "");
			using (var addInRegKey = Registry.CurrentUser.OpenSubKey(addinKey, RegistryKeyPermissionCheck.ReadSubTree))
			{
				return addInRegKey != null && addInRegKey.GetValueNames().Contains("Manifest");
			}
		}

		public static MailTrackingType AddSubjectSuffix
		{
			get
			{
				try
				{
					using (var addInKey = Registry.CurrentUser.OpenSubKey(GetAddinKey(), RegistryKeyPermissionCheck.ReadSubTree))
					{
						if (addInKey == null) return MailTrackingType.Disable;
						var value = addInKey.GetValue(outlookAddSubjectSuffixKey);
						return value != null ? (MailTrackingType)((int)value) : MailTrackingType.Disable;
					}
				}
				catch (Exception ex)
				{
					log.Error("Cannot get value", ex);
					return MailTrackingType.Disable;
				}
			}
			set
			{
				RegistryKey addInRegKey = null;
				try
				{		
					var addinKey = GetAddinKey();
					addInRegKey = Registry.CurrentUser.OpenSubKey(addinKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
					{
						if (addInRegKey == null)
						{
							if (value == MailTrackingType.Disable) return;
							addInRegKey = Registry.CurrentUser.CreateSubKey(addinKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
							if (addInRegKey == null)
							{
								log.Error("Cannot set value, node cannot be created");
								return;
							}
						}
						if (value == MailTrackingType.Disable && !IsInstalled(addinKey))
						{
							Registry.CurrentUser.DeleteSubKeyTree(addinKey);
							return;
						}
						addInRegKey.SetValue(outlookAddSubjectSuffixKey, value, RegistryValueKind.DWord);
					}
				}
				catch (Exception ex)
				{
					log.Error("Cannot set value", ex);
				}
				finally
				{
					if (addInRegKey != null)
						addInRegKey.Dispose();
				}
			}
		}

		private static string OutlookVersion
		{
			get
			{
				try
				{
					var value = Registry.GetValue("HKEY_CLASSES_ROOT\\Outlook.Application\\CurVer", null, null);	//This key is not redirected.
					return value != null ? value.ToString() : null;
				}
				catch (Exception e)
				{
					log.Error("Error while retrieving Outlook version from registry.", e);
					return null;
				}
			}
		}

		private static string OutLookVersionDot
		{
			get
			{
				var verSp = OutlookVersion.Split('.');
				return verSp[verSp.Length - 1] + ".0";
			}
		}

	}
}
