using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using Microsoft.Win32;

namespace Tct.ActivityRecorderClient.Update
{
	public class AutoStartHelper: IAutoStartHelper
	{
		private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string jcAppName = "JobCTRL";
		private const string SoftwareMicrosoftWindowsCurrentversionAppPaths = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths";
		private const string SoftwareMicrosoftWindowsCurrentversionRun = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

		public void Register(IUpdateService updateService)
		{
			Register(jcAppName, updateService.GetAppPath());
		}

		private void Register(string appName, string shortcutPath)
		{
#if (DEBUG || DEV)
			if ("1" == "" + 1) return; //don't modify registry for debug/dev builds
#endif
			var shortcutPathQuoted = "\"" + shortcutPath + "\""; //avoid autostart problem when an accent is in path
			try
			{
				using (var lmRegkey = Registry.LocalMachine.OpenSubKey(SoftwareMicrosoftWindowsCurrentversionRun))   
				{
					var lmRegVal = lmRegkey?.GetValue(appName);
					if (lmRegVal != null && lmRegVal.ToString() == shortcutPath)
					{
						using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(SoftwareMicrosoftWindowsCurrentversionRun, true)) //This key not redirected under HKEY_CURRENT_USER
						{
							regkey?.DeleteValue(appName, false);
						}
						log.Debug("HKLM Run item found, HKCU item deleted");
						return;
					}
				}
				RegistryKey appPathKey = null;
				try
				{
					appPathKey = Registry.CurrentUser.OpenSubKey(SoftwareMicrosoftWindowsCurrentversionAppPaths, true);
					if (appPathKey == null)
						appPathKey = Registry.CurrentUser.CreateSubKey(SoftwareMicrosoftWindowsCurrentversionAppPaths);
					using (var jcappKey = appPathKey.CreateSubKey(Path.GetFileName(shortcutPath), RegistryKeyPermissionCheck.ReadWriteSubTree))
					{
						jcappKey.SetValue(null, shortcutPath);
						jcappKey.SetValue("Path", Path.GetDirectoryName(shortcutPath));
					}
				}
				finally
				{
					appPathKey?.Dispose();
				}
				using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(SoftwareMicrosoftWindowsCurrentversionRun, true))	//This key not redirected under HKEY_CURRENT_USER
				{
					var regVal = regkey.GetValue(appName);
					if (regVal != null)
					{
						if (regVal.ToString() == shortcutPathQuoted) return;
						regkey.DeleteValue(appName, false); //delete old key if exists
					}
					regkey.SetValue(appName, shortcutPathQuoted);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to write autostart info into registry for " + shortcutPath, ex);
			}
		}
	}
}
