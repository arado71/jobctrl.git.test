using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Tct.ActivityRecorderClient.OutlookAddinMailCaptureServiceReference;

namespace Tct.ActivityRecorderClient.Communication
{
	public class OutlookAddinMailCaptureClientWrapper : IDisposable
	{
		public readonly AddinMailCaptureServiceClient Client;

#if DEBUG
		private const string suffix = "";
#else
		private static readonly string suffix = "_" + Process.GetCurrentProcess().SessionId + "_" + OutlookAddinInstallHelper.OutlookAddinLocHash;
#endif
		public OutlookAddinMailCaptureClientWrapper()
		{
			Client = new AddinMailCaptureServiceClient("NetNamedPipeBinding_IMailCaptureService1", "net.pipe://localhost/OutlookAddinMailCaptureService" + suffix);
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
