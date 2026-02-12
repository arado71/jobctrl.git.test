using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.MailFilterService
{
	public class WebsiteClientWrapper : IDisposable
	{
		public WebsiteApi.APISoapClient Client;

		public WebsiteClientWrapper()
		{
			Client = new WebsiteApi.APISoapClient();
		}

		public void RefreshClient()
		{
			WcfClientDisposeHelper.Dispose(Client);
			Client = new WebsiteApi.APISoapClient();
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
