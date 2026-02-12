using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ProxyDataRouter.ProxyServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace ProxyDataRouter
{
	class ProxyClientWrapper : IDisposable
	{
		public ProxyServiceReference.ProxyServiceClient Client;
		private InstanceContext context;

		public ProxyClientWrapper(InstanceContext context)
		{
			this.context = context;
			Client = new ProxyServiceClient(context, "NetTcpBinding_IProxyService");
		}

		public void RefreshClient()
		{
			WcfClientDisposeHelper.Dispose(Client);
			Client = new ProxyServiceClient(context, "NetTcpBinding_IProxyService");
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
