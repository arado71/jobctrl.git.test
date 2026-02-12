using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.AppControlServiceReference;

namespace Tct.ActivityRecorderClient.Communication
{
	public class AppControlServiceClientWrapper : IDisposable
	{
		public readonly AppControlServiceServiceClient Client;

		public AppControlServiceClientWrapper()
		{
			Client = new AppControlServiceServiceClient("NetNamedPipeBinding_IAppControlServiceService", "net.pipe://localhost/AppControlServiceService");
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
