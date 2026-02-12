using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using Microsoft.Win32;
using VoxCTRL.Update;

namespace VoxCTRL
{
	public static class AutoStartHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string appName = "VoxCTRL";
		private const string SoftwareMicrosoftWindowsCurrentversionRun = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

		public static void Register(UpdateManager updateService)
		{
			Register(appName, updateService.GetAppPath());
		}

		private static void Register(string appName, string shortcutPath)
		{
#if (DEBUG || DEV)
			if ("1" == "" + 1) return; //don't modify registry for debug/dev builds
#endif
			try
			{
				using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(SoftwareMicrosoftWindowsCurrentversionRun, true))	//This key not redirected under HKEY_CURRENT_USER
				{
					shortcutPath = "\"" + shortcutPath + "\""; //avoid autostart problem when an accent is in path
					var regVal = regkey.GetValue(appName);
					if (regVal != null)
					{
						if (regVal.ToString() == shortcutPath) return;
						regkey.DeleteValue(appName, false); //delete old key if exists
					}
					regkey.SetValue(appName, shortcutPath);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to write autostart info into registry for " + shortcutPath, ex);
			}
		}
	}
}
