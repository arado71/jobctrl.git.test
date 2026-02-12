using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.VoiceRecorderControllerServiceReference;

namespace Tct.ActivityRecorderClient.Communication
{
	public class VoiceRecorderControllerClientWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
		public readonly VoiceRecorderControllerServiceClient Client;

		public VoiceRecorderControllerClientWrapper()
		{
			Client = new VoiceRecorderControllerServiceClient();
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}}
}
