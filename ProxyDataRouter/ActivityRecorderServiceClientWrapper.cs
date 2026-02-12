using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyDataRouter.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace ProxyDataRouter
{
	class ActivityRecorderServiceClientWrapper : IDisposable
	{
		public ActivityRecorderServiceReference.ActivityRecorderClient Client;

		public ActivityRecorderServiceClientWrapper()
		{
			Client = new ActivityRecorderClient("NetTcpBinding_IActivityRecorder");
		}

		public void RefreshClient()
		{
			WcfClientDisposeHelper.Dispose(Client);
			Client = new ActivityRecorderClient("NetTcpBinding_IActivityRecorder");
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
