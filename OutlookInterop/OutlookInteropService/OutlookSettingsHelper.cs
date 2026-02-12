using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using log4net;
using Tct.ActivityRecorderClient;

namespace OutlookInteropService
{
	//Outlook object model - security warnings
	//http://support.microsoft.com/kb/926512
	//http://www.slipstick.com/developer/change-programmatic-access-options/
	//HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Office\14.0\Outlook\Security\ObjectModelGuard 2 (64bit Win 32 bit Outlook)
	//HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Office\14.0\Outlook\Security\ObjectModelGuard 2
	//or
	//HKEY_CURRENT_USER\Software\Policies\Microsoft\Office\14.0\Outlook\Security\AdminSecurityMode 3
	//HKEY_CURRENT_USER\Software\Policies\Microsoft\Office\14.0\Outlook\Security\PromptOOMAddressInformationAccess 2
	public static class OutlookSettingsHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static bool IsOutlookInstalled
		{
			get { return Type.GetTypeFromProgID("Outlook.Application", false) != null; }	//This key is not redirected.
		}

		//JC compiled for .NET 3.5 with X86 won't see registry keys of 64 bit Office
		public static bool IsObjectGuardDisabled
		{
			get
			{
				try
				{
					var keyName = GetRegistryDir(false) + @"\Security";
					var altKeyName = GetRegistryDir(true) + @"\Security";
					return (RegistryHelper.GetValueFromEitherView(RegistryHive.LocalMachine, keyName, altKeyName, "ObjectModelGuard") ?? "").ToString() == "2";
				}
				catch (Exception ex)
				{
					log.Error("Unable to get IsObjectGuardDisabled", ex);
					return false;
				}
			}
		}

		private static string GetRegistryDir(bool altDir)
		{
			var verSp = OutlookVersionStr.Split('.');
			var verDot = verSp[verSp.Length - 1] + ".0";

			return String.Format(@"SOFTWARE\{0}Microsoft\Office\{1}\Outlook", altDir ? @"Wow6432Node\" : "", verDot);
		}

		public static OutlookVersion OutlookVersion
		{
			get { return GetOutlookVersionFromString(OutlookVersionStr); }
		}

		public static string OutlookVersionStr
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

		public static Bitness? OutlookBitness
		{
			get
			{
				try
				{
					string bitnessStr = "";
					if (OutlookVersionStr.StartsWith("Outlook.Application."))
					{
						var ver = OutlookVersionStr.Substring(20);
						if (!ver.Contains('.')) ver += ".0";
						var value = RegistryHelper.GetValueFromEitherView(RegistryHive.LocalMachine, 
																			string.Format("SOFTWARE\\Microsoft\\Office\\{0}\\Outlook", ver), 
																			string.Format("SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{0}\\Outlook", ver), 
																			"Bitness");
						if (value != null)
							bitnessStr = value.ToString();
						else
							bitnessStr = "x86";
					}

					if (bitnessStr.Equals("x86", StringComparison.InvariantCultureIgnoreCase)) return Bitness.X86;
					if (bitnessStr.Equals("x64", StringComparison.InvariantCultureIgnoreCase)) return Bitness.X64;

					return null;
				}
				catch (Exception e)
				{
					log.Error("Error while retrieving Outlok bitness from registry.", e);
					return null;
				}
			}
		}

		public static OutlookVersion GetOutlookVersionFromString(string versionString)
		{
			switch (versionString)
			{
				case "Outlook.Application.8":
					return OutlookVersion.Outlook97;
				case "Outlook.Application.8.5": //this does not exists in registry (but we don't care nor support it)
					return OutlookVersion.Outlook98;
				case "Outlook.Application.9":
					return OutlookVersion.Outlook2000;
				case "Outlook.Application.10":
					return OutlookVersion.Outlook2002;
				case "Outlook.Application.11":
					return OutlookVersion.Outlook2003;
				case "Outlook.Application.12":
					return OutlookVersion.Outlook2007;
				case "Outlook.Application.14":
					return OutlookVersion.Outlook2010;
				case "Outlook.Application.15":
					return OutlookVersion.Outlook2013;
				case "Outlook.Application.16":
					return OutlookVersion.Outlook2016;
				default:
					return OutlookVersion.Unknown;
			}
		}

		private static bool IsRegistryKeyExists(RegistryKey regkey, string name)
		{
			try
			{
				using (RegistryKey subkey = regkey.OpenSubKey(name))
				{
					return subkey != null;
				}
			}
			catch (Exception ex)
			{
				log.Error(String.Format("Unable to check registry key existence. ({0})", name), ex);
				return false;
			}
		}
	}

	public enum Bitness
	{
		X86,
		X64,
	}

	public enum OutlookVersion
	{
		Unknown,
		Outlook97,
		Outlook98,
		Outlook2000,
		Outlook2002,
		Outlook2003,
		Outlook2007,
		Outlook2010,
		Outlook2013,
		Outlook2016,
	}
}
