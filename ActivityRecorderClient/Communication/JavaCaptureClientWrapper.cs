using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.JavaCaptureServiceReference;

namespace Tct.ActivityRecorderClient.Communication
{
	class JavaCaptureClientWrapper: IDisposable
	{
		public readonly JavaCaptureServiceClient Client;

		public JavaCaptureClientWrapper()
		{
			Client = new JavaCaptureServiceClient("NetNamedPipeBinding_IJavaCaptureService", "net.pipe://localhost/JavaCaptureService_" + ConfigManager.CurrentProcessPid);
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
