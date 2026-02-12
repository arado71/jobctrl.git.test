using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderService.Website
{
	public class WebsiteClientWrapper : IDisposable
	{
		public WebsiteServiceReference.ClientAPISoapClient Client;

		public WebsiteClientWrapper()
		{
			Client = new WebsiteServiceReference.ClientAPISoapClient();
		}

		public void RefreshClient()
		{
			WcfClientDisposeHelper.Dispose(Client);
			Client = new WebsiteServiceReference.ClientAPISoapClient();
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
