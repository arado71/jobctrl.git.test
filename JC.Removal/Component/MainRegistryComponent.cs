using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace JC.Removal.Component
{
	class MainRegistryComponent : BaseComponent
	{
		private const string productsRegistryPath = @"Software\Microsoft\Installer\Products";
		private const string upgradeCodesRegistryPath = @"Software\Microsoft\Installer\UpgradeCodes";
		private const string jobCtrlRegistryPath = @"Software\JobCTRL";
		private const string autoRunRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
		private readonly string productName;
		private const string userFriendlyName = "Registry keys";

		public MainRegistryComponent(string productName)
		{
			this.productName = productName;
		}

		public override string GetUserFriendlyName()
		{
			return userFriendlyName;
		}

		public override bool Remove(out string error)
		{
			try
			{
				using (var autoRunRegKey = Registry.CurrentUser.OpenSubKey(autoRunRegistryPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
				{
					if (autoRunRegKey != null && autoRunRegKey.GetValueNames().Any(x => x == productName))
					{
						autoRunRegKey.DeleteValue(productName);
					}
				}

				bool shouldDeleteJobCtrlRegistryKey = false;
				using (var jcRegKey = Registry.CurrentUser.OpenSubKey(jobCtrlRegistryPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
				{
					if (jcRegKey != null)
					{
						if(jcRegKey.GetSubKeyNames().Any(x => x == productName))
							jcRegKey.DeleteSubKey(productName);
						shouldDeleteJobCtrlRegistryKey = jcRegKey.GetSubKeyNames().Length == 0;
					}
				}
				if(shouldDeleteJobCtrlRegistryKey) Registry.CurrentUser.DeleteSubKey(jobCtrlRegistryPath);
				using (var regKey = Registry.CurrentUser.OpenSubKey(productsRegistryPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
				{
					if (regKey == null)
					{
						error = "Unable to find products registry path.";
						return false;
					}
					foreach (var productKeyName in regKey.GetSubKeyNames())
						using (var productKey = regKey.OpenSubKey(productKeyName, RegistryKeyPermissionCheck.ReadSubTree))
						{
							if (productKey == null)
							{
								error = "Unexpected error during iteration.";
								return false;
							}
							var productNameValue = productKey.GetValue("ProductName");
							if (productNameValue is string productNameStringValue &&
								string.Equals(productNameStringValue, productName, StringComparison.OrdinalIgnoreCase))
							{
								using (var upgradeCodesRegKey = Registry.CurrentUser.OpenSubKey(upgradeCodesRegistryPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
								{
									if (upgradeCodesRegKey == null)
									{
										error = "Unable to find UpgradeCodes node.";
										return false;
									}

									string deletableKeyName = null;
									foreach (var keyName in upgradeCodesRegKey.GetSubKeyNames())
									{
										using (var upgradeCodeRegKey = upgradeCodesRegKey.OpenSubKey(keyName))
										{
											if (upgradeCodeRegKey.GetValueNames().Contains(productKeyName))
											{
												deletableKeyName = keyName;
												break;
											}
										}
									}

									if (deletableKeyName != null)
									{
										upgradeCodesRegKey.DeleteSubKey(deletableKeyName);
										regKey.DeleteSubKeyTree(productKeyName);
										error = null;
										return true;
									}
								}
								error = null;
								return true;
							}
						}

					error = $"Couldn't find product: {productName}";
					return false;
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
			using (var regKey = Registry.CurrentUser.OpenSubKey(productsRegistryPath, RegistryKeyPermissionCheck.ReadSubTree))
			{
				if (regKey == null) return false;
				foreach (var subKeyName in regKey.GetSubKeyNames())
				{
					using (var subKey = regKey.OpenSubKey(subKeyName, RegistryKeyPermissionCheck.ReadSubTree))
					{
						if (subKey == null) return false;
						var productNameValue = subKey.GetValue("ProductName");
						if (productNameValue is string productNameStringValue &&
							string.Equals(productNameStringValue, productName, StringComparison.OrdinalIgnoreCase))
							return true;
					}
				}
			}
			return false;
		}

		public override string[] GetProcessesNames()
		{
			return new string[0];
		}
	}
}
