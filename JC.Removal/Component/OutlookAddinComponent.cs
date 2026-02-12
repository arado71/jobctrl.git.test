using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace JC.Removal.Component
{
	class OutlookAddinComponent: BaseComponent
	{
		private const string outlookAddinsRootKey = @"Software\Microsoft\Office\Outlook\Addins";
		private const string outlookAddinsRelativeKey = @"Outlook\Addins";
		private const string officeBaseRootKey = @"Software\Microsoft\Office";
		private const string outlookAddinName = "MailActivityTracker";
		private const string officeVersionPattern = @"\d+\.\d+";
		private const string userFriendlyName = "Outlook addin";

		public override string GetUserFriendlyName()
		{
			return userFriendlyName;
		}

		public override bool Remove(out string error)
		{
			error = null;
			try
			{
				using (var officeRootRegKey = Registry.CurrentUser.OpenSubKey(officeBaseRootKey))
					if (officeRootRegKey != null)
						foreach (var subKeyName in officeRootRegKey.GetSubKeyNames().Where(x => Regex.IsMatch(x, officeVersionPattern)))
							using (var addinsRegKey = officeRootRegKey.OpenSubKey(subKeyName))
								if (addinsRegKey != null)
									foreach (var keyName in addinsRegKey.GetSubKeyNames().Where(x => x.StartsWith(outlookAddinName)))
										addinsRegKey.DeleteSubKeyTree(keyName);

				using (var addInRegKey = Registry.CurrentUser.OpenSubKey(outlookAddinsRootKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
				{
					if (addInRegKey != null)
						foreach (var keyName in addInRegKey.GetSubKeyNames().Where(x => x.StartsWith(outlookAddinName)))
							addInRegKey.DeleteSubKeyTree(keyName);
					return true;
				}
			}
			catch (Exception ex)
			{
				error = ex.ToString();
				return false;
			}
		}

		public override bool IsInstalled()
		{
			using (var officeRootRegKey = Registry.CurrentUser.OpenSubKey(officeBaseRootKey))
			{
				if (officeRootRegKey != null)
					foreach (var subKeyName in officeRootRegKey.GetSubKeyNames().Where(x => Regex.IsMatch(x, officeVersionPattern)))
					{
						using (var addinsRegKey = officeRootRegKey.OpenSubKey(Path.Combine(subKeyName, outlookAddinsRelativeKey)))
							if(addinsRegKey != null && addinsRegKey.GetSubKeyNames().Any(x => x.StartsWith(outlookAddinName)))
						return true;
					}
			}
			using (var addInRegKey = Registry.CurrentUser.OpenSubKey(Path.Combine(outlookAddinsRootKey, outlookAddinsRelativeKey), RegistryKeyPermissionCheck.ReadWriteSubTree))
			{
				return addInRegKey != null && addInRegKey.GetSubKeyNames().Any(x => x.StartsWith(outlookAddinName));
			}
		}

		public override string[] GetProcessesNames()
		{
			return new[] { "outlook.exe" };
		}
	}
}
