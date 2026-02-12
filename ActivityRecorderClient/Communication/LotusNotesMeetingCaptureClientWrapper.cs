using System;
using Tct.ActivityRecorderClient.LotusNotesMeetingCaptureServiceReference;
using log4net;

namespace Tct.ActivityRecorderClient.Communication
{
	public class LotusNotesMeetingCaptureClientWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public readonly MeetingCaptureServiceClient Client;

        public LotusNotesMeetingCaptureClientWrapper()
		{
            Client = new MeetingCaptureServiceClient("NetNamedPipeBinding_IMeetingCaptureService1", "net.pipe://localhost/LotusNotesMeetingCaptureService_" + ConfigManager.CurrentProcessPid);
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
