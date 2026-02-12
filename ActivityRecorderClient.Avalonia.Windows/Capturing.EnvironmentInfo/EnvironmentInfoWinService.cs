using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using Microsoft.Win32;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;

namespace Tct.ActivityRecorderClient.Capturing.EnvironmentInfo
{
	public class EnvironmentInfoWinService : EnvironmentInfoService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public EnvironmentInfoWinService()
		{
			var verAndCap = GetOSVersionAndCaption();
			if (verAndCap.Value != null)
			{
				OSVersion = new Version(verAndCap.Value);
				OSFullName = verAndCap.Key;
			}
		}

		private static KeyValuePair<string, string> GetOSVersionAndCaption()
		{
			KeyValuePair<string, string> kvpOSSpecs = new KeyValuePair<string, string>();
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
			try
			{

				foreach (var os in searcher.Get())
				{
					var version = os["Version"].ToString();
					var productName = os["Caption"].ToString();
					kvpOSSpecs = new KeyValuePair<string, string>(productName, version);
				}
			}
			catch (Exception e)
			{
				log.Warn("GetOSversion failed", e);
			}

			return kvpOSSpecs;
		}

		protected override VirtualMachine GetCurrentVirtualMachine()
		{
			using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem"))
			{
				searcher.Options.Timeout = TimeSpan.FromMinutes(1);
				using (var moCollection = searcher.Get())
				{
					foreach (ManagementObject manObj in moCollection)
					{
						using (manObj)
						{
							var manufacturer = manObj["Manufacturer"];
							if (manufacturer != null && manufacturer.ToString().IndexOf("VMware", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								return VirtualMachine.VmWare;
							}
							if (manufacturer != null && manufacturer.ToString().IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								return VirtualMachine.Microsoft;
							}
							if (manufacturer != null && manufacturer.ToString().IndexOf("Parallels", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								return VirtualMachine.Parallels;
							}

							var model = manObj["Model"];
							if (model != null && model.ToString().IndexOf("VirtualBox", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								return VirtualMachine.VirtualBox;
							}
						}
					}
				}
			}

			return VirtualMachine.None;
		}

		protected override bool GetIsRemoteDesktop()
		{
			return GetCurrentRemoteDesktop() != RemoteDesktop.None;
		}

		//UAC will change compId this is not ideal...
		//without UAC
		//CPU0BFEBFBFF000206A7ST9500325ASG ATA Device5VENSAAV
		//with UAC
		//CPU0BFEBFBFF000206A7ST9500325ASG ATA Device20202020202020202020202056354e4541535641
		protected override int GetComputerId()
		{
			var computerParams = new StringBuilder();

			using (var searcher = new ManagementObjectSearcher("SELECT DeviceID,ProcessorId FROM Win32_Processor"))
			{
				searcher.Options.Timeout = TimeSpan.FromMinutes(1);
				using (var moCollection = searcher.Get())
				{
					foreach (ManagementObject manObj in moCollection)
					{
						using (manObj)
						{
							AppendProperty(computerParams, manObj, "DeviceID");
							AppendProperty(computerParams, manObj, "ProcessorId", "No ProcessorId");
						}
					}
				}
			}

			uint index = 0xffffffff;
			using (var searcher = new ManagementObjectSearcher("SELECT Model, Index FROM Win32_DiskDrive where MediaType = 'Fixed hard disk media'"))
			{
				searcher.Options.Timeout = TimeSpan.FromMinutes(1);
				using (var moCollection = searcher.Get())
				{
					foreach (ManagementObject manObj in moCollection)
					{
						using (manObj)
						{
							if (AppendProperty(computerParams, manObj, "Model"))
							{
								index = (uint)manObj["Index"];
								break;
							}
						}
					}
				}
			}

			var wmiSelect = "SELECT SerialNumber FROM Win32_PhysicalMedia";
			if (index != 0xffffffff)
				wmiSelect += " where Tag = '\\\\\\\\.\\\\PHYSICALDRIVE" + index + "'";
			using (var searcher = new ManagementObjectSearcher(wmiSelect))
			{
				searcher.Options.Timeout = TimeSpan.FromMinutes(1);
				using (var moCollection = searcher.Get())
				{
					foreach (ManagementObject manObj in moCollection)
					{
						using (manObj)
						{
							AppendProperty(computerParams, manObj, "SerialNumber", "No Serial.");
							break;
						}
					}
				}
			}

			using (SHA1CryptoServiceProvider sha1Prov = new SHA1CryptoServiceProvider())
			{
				byte[] compBytes = Encoding.ASCII.GetBytes(computerParams.ToString());
				sha1Prov.ComputeHash(compBytes);
				Debug.Assert(sha1Prov.HashSize == 160);
				Debug.Assert(sha1Prov.Hash.Length == 20);

				int hashCode = 0;
				for (int i = 0; i < sha1Prov.Hash.Length; i += 4)
				{
					hashCode ^= (sha1Prov.Hash[i] << 24)
								+ (sha1Prov.Hash[i + 1] << 16)
								+ (sha1Prov.Hash[i + 2] << 8)
								+ (sha1Prov.Hash[i + 3]);
				}
				return hashCode == -1 ? 1 : hashCode;
			}
		}

		private static bool AppendProperty(StringBuilder sb, ManagementObject manObj, string propertyName, string alternateValue = null)
		{
			var value = manObj[propertyName];
			var valueToUse = value != null ? value.ToString().Trim() : alternateValue;
			if (valueToUse != null)
			{
				sb.Append(valueToUse);
				log.Debug("WMI poperty: " + propertyName + " - " + valueToUse);
				return true;
			}
			return false;
		}

		//http://msdn.microsoft.com/en-us/library/hh925568.aspx
		public override bool IsNet4Available
		{
			get
			{
				try
				{
					var value = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\", "Install", null); //This is a redirected key but there are values in both view.
					return value != null && (int)value == 1;
				}
				catch (Exception ex)
				{
					log.Error("Error while retrieving if .Net4 is installed.", ex);
					return false;
				}
			}
		}

		public override bool IsNet45Available
		{
			get
			{
				try
				{
					return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\", "Release", null) != null; //This is a redirected key but there are values in both view.
				}
				catch (Exception ex)
				{
					log.Error("Error while retrieving if .Net45 is installed.", ex);
					return false;
				}
			}
		}

		public override int HighestNetVersionAvailable
		{
			get
			{
				try
				{
					return (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\", "Release", null); //This is a redirected key but there are values in both view.
				}
				catch (Exception ex)
				{
					log.Error("Error while retrieving highest installed .NET version.", ex);
					return -1;
				}
			}
		}

		private RemoteDesktop prevRemoteDesktop;

		public override RemoteDesktop GetCurrentRemoteDesktop()
		{
			var ret = RemoteDesktop.None;
			if (System.Windows.Forms.SystemInformation.TerminalServerSession) ret = RemoteDesktop.RDP;
			else if (isAnydeskSessionCachedFunc.GetOrCalculateValue()) ret = RemoteDesktop.AnyDesk;
			else
			{
				var currentSessionId = Process.GetCurrentProcess().SessionId;
				if (Process.GetProcessesByName("TeamViewer_Desktop").Where(p => p.SessionId == currentSessionId).Any()) ret = RemoteDesktop.TeamViewer;
			}

			if (prevRemoteDesktop == ret) return ret;
			log.Debug("RemoteDesktop changed: " + ret);
			prevRemoteDesktop = ret;
			return ret;
		}

		private readonly CachedFunc<bool> isAnydeskSessionCachedFunc = new CachedFunc<bool>(IsAnydeskSession, TimeSpan.FromMinutes(1));
		private	static readonly Regex anyDeskMainClassNameRegex = new Regex("^ad_(msi_)?win#[0-9]+$");
		private	static readonly Regex anyDeskConnWinClassNameRegex = new Regex("^(accept_panel|layout_window_t)#[0-9]+$"); // maybe: (auth_win)|(accept_panel)
		private	static readonly Regex anyDeskDiscBtnClassNameRegex = new Regex("^basic_button#[0-9]+$");

		private static bool IsAnydeskSession()
		{
			var found = false;
			WinApi.EnumWindows((h, l) =>
			{
				var className = WindowTextHelper.GetClassName(h);
				if (!anyDeskMainClassNameRegex.IsMatch(className)) return true;
				if (!EnumChildWindowsHelper.GetChildWindowInfo(h, c => anyDeskConnWinClassNameRegex.IsMatch(c.ClassName)).SelectMany(p => EnumChildWindowsHelper.GetChildWindowInfo(p.Handle, c => anyDeskDiscBtnClassNameRegex.IsMatch(c.ClassName) && !string.IsNullOrEmpty(c.Caption))).Any()) return true;
				found = true;
				return false;
			}, 0);

			return found;
		}
	}
}
