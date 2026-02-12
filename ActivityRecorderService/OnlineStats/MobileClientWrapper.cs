using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderService.OnlineStats
{
	public class MobileClientWrapper : IDisposable
	{
		public readonly MobileServiceReference.MobileJobCTRLServerClient Client;

		public MobileClientWrapper()
		{
			Client = new MobileServiceReference.MobileJobCTRLServerClient();
			//Client.ClientCredentials.UserName.UserName = ConfigManager.MobileUserId.ToString();
			//Client.ClientCredentials.UserName.Password = ConfigManager.MobileUserPassword;
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
