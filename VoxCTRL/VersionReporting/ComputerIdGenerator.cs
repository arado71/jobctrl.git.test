using System;
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using log4net;

namespace VoxCTRL.VersionReporting
{
	public class ComputerIdGenerator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ComputerIdGenerator()
		{
			ComputerId = GetComputerIdOrMinusOneNoThrow(false);
			if (ComputerId != -1) return;
			int tries = 40;
			while (ComputerId == -1 && --tries > 0)
			{
				if (tries % 5 == 0) log.Info("Still calculating computer id");
				System.Threading.Thread.Sleep(2000);
				ComputerId = GetComputerIdOrMinusOneNoThrow(tries == 1); //log error on last
			}
		}

		public int ComputerId { get; }

		private int GetComputerIdOrMinusOneNoThrow(bool logError)
		{
			try
			{
				return GetComputerId();
			}
			catch (Exception ex)
			{
				if (logError) log.Error("Unable to get computerId", ex);
				return -1;
			}
		}

		//UAC will change compId this is not ideal...
		//without UAC
		//CPU0BFEBFBFF000206A7ST9500325ASG ATA Device5VENSAAV
		//with UAC
		//CPU0BFEBFBFF000206A7ST9500325ASG ATA Device20202020202020202020202056354e4541535641
		private int GetComputerId()
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
	}
}
