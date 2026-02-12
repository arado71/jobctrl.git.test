using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference;
using log4net;

namespace Tct.ActivityRecorderClient.Communication
{
	public class OutlookMailCaptureClientWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public readonly OutlookMailCaptureServiceClient Client;

		public OutlookMailCaptureClientWrapper()
		{
			Client = new OutlookMailCaptureServiceClient("NetNamedPipeBinding_IOutlookMailCaptureService", "net.pipe://localhost/OutlookMailCaptureService_" + ConfigManager.CurrentProcessPid);
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
