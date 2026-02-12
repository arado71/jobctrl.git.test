using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class IpDetector
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static IpDetector instance = null;
		public IEnumerable<string> NetworkAdapterIPAddresses { get; private set; }

		public IEnumerable<string> NetworkAdapterIPAddressesAndRDPClientAddress
		{
			get
			{
				return RDPClientAddress == null ? NetworkAdapterIPAddresses : NetworkAdapterIPAddresses.Concat(new List<string> { RDPClientAddress });
			}
		}

		public string RDPClientAddress { get; private set; }
		public static IpDetector Instance
		{
			get { return instance ?? (instance = new IpDetector()); }
		}
		private IpDetector()
		{
			AddressChangedCallback(this, null);
			NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
		}
		private void AddressChangedCallback(object sender, EventArgs e)
		{
			try
			{
				NetworkAdapterIPAddresses = GetIpsForNetworkAdapters()
					.Select(s => s.ToString())
					.OrderBy(t => t)
					.ToList();
				RDPClientAddress = GetRdpClientAddress();
			}
			catch (Exception ex)
			{
				log.Error("Error obtaining IP addresses");
			}
		}

		internal void HandleSessionSwitch()
		{
			RDPClientAddress = GetRdpClientAddress();
		}

		private IEnumerable<IPAddress> GetIpsForNetworkAdapters()
		{
			var nics = from inf in NetworkInterface.GetAllNetworkInterfaces()
					   where inf.OperationalStatus == OperationalStatus.Up
					   from unicast in inf.GetIPProperties().UnicastAddresses
					   where unicast.Address.AddressFamily == AddressFamily.InterNetwork
					   select unicast.Address;
			foreach (var a in nics.Where(e => e.Address != null && !IPAddress.IsLoopback(e)))
				yield return a;
		}

		private string GetRdpClientAddress()
		{
			if (!System.Windows.Forms.SystemInformation.TerminalServerSession)
				return null;
			try
			{
				IntPtr pServer = IntPtr.Zero;
				string sIPAddress = string.Empty;

				WinApi.WTS_CLIENT_ADDRESS oClientAddres = new WinApi.WTS_CLIENT_ADDRESS();

				IntPtr pSessionInfo = IntPtr.Zero;

				int iCount = 0;
				int iReturnValue = WinApi.WTSEnumerateSessions
					(pServer, 0, 1, ref pSessionInfo, ref iCount);
				int iDataSize = Marshal.SizeOf(typeof(WinApi.WTS_SESSION_INFO));

				int iCurrent = (int)pSessionInfo;
				try
				{
					if (iReturnValue != 0)
					{
						//Go to all sessions
						for (int i = 0; i < iCount; i++)
						{
							WinApi.WTS_SESSION_INFO oSessionInfo = (WinApi.WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)iCurrent,
								typeof(WinApi.WTS_SESSION_INFO));
							iCurrent += iDataSize;
							if (oSessionInfo.oState != WinApi.WTS_CONNECTSTATE_CLASS.WTSActive) continue;
							if (oSessionInfo.sWinsWorkstationName == "Console") continue;
							uint iReturned = 0;

							//Get the IP address of the Terminal Services User
							IntPtr pAddress = IntPtr.Zero;
							if (WinApi.WTSQuerySessionInformation(pServer, oSessionInfo.iSessionID,
									WinApi.WTS_INFO_CLASS.WTSClientAddress, out pAddress, out iReturned) == true)
							{
								oClientAddres = (WinApi.WTS_CLIENT_ADDRESS)Marshal.PtrToStructure(pAddress, oClientAddres.GetType());
								sIPAddress = oClientAddres.bAddress[2] + "." + oClientAddres.bAddress[3] + "."
											 + oClientAddres.bAddress[4] + "." + oClientAddres.bAddress[5];
							}
							return sIPAddress;
						}
					}
				}
				// We don't handle exception here, just to be sure
				finally
				{
					WinApi.WTSFreeMemory(pSessionInfo);
				}

				return null;
			}
			catch (Exception ex)
			{
				log.Warn("Unexpected exception in getting the rdp client's ip address.", ex);
				return null;
			}
		}
	}
}
