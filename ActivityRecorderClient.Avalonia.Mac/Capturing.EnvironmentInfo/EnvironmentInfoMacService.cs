using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.EnvironmentInfo
{
	public class EnvironmentInfoMacService : EnvironmentInfoService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public override bool IsNet4Available => true;

		public override bool IsNet45Available => true;

		public override int HighestNetVersionAvailable => 8;

		protected override VirtualMachine GetCurrentVirtualMachine()
		{
			//todo
			return VirtualMachine.None;
		}

		protected override bool GetIsRemoteDesktop()
		{
			//todo
			return false;
		}

		protected override int GetComputerId()
		{
			var idData = GetSerialNumber();
			if (idData == null) //fallback to MAC address
			{
				log.Warn("Unable to get serial number");
				var allIfs = NetworkInterface.GetAllNetworkInterfaces();
				var en0 = allIfs.Where(n => n.Id == "en0").FirstOrDefault();
				if (en0 == null)
					return -1;
				idData = en0.GetPhysicalAddress().ToString();
			}
			using (SHA1CryptoServiceProvider sha1Prov = new SHA1CryptoServiceProvider())
			{
				byte[] compBytes = Encoding.ASCII.GetBytes(idData);
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

		private static string GetSerialNumber()
		{
			//https://developer.apple.com/library/mac/#technotes/tn/tn1103.html
			//http://www.jaharmi.com/2008/03/15/to_get_mac_serial_numbers_scripts_is_ioreg_or_system_profiler_faster
			//ioreg -l | grep IOPlatformSerialNumber
			//system_profiler SPHardwareDataType | awk '/Serial Number/ { print $NF; }'
			using (var p = new Process())
			{
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.FileName = "system_profiler";
				p.StartInfo.Arguments = "SPHardwareDataType";
				p.Start();
				var output = p.StandardOutput.ReadToEnd();
				p.WaitForExit();
				return output.Split(new [] {'\n'}, StringSplitOptions.RemoveEmptyEntries)
					.Where(n => n.Contains("Serial Number") && n.Contains(": "))
					.Select(n => n.Split(new [] {": "}, StringSplitOptions.RemoveEmptyEntries).Last())
					.FirstOrDefault();
			}
		}

		// TODO: mac
		public override RemoteDesktop GetCurrentRemoteDesktop()
		{
			return RemoteDesktop.None;
		}
	}
}

