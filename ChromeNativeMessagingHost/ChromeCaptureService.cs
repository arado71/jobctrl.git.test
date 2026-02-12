using System;
using System.ServiceModel;
using log4net;

namespace NativeMessagingHost
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	class ChromeCaptureService : IChromeCaptureService
	{
		private readonly ExtensionCommunication communication;
		private readonly Action stopAction;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ChromeCaptureService(ExtensionCommunication communication, Action stopAction)
		{
			this.communication = communication;
			this.stopAction = stopAction;
		}

		public string SendCommand(string command)
		{
			try
			{
				return communication.Request(command);
			}
			catch (Exception e)
			{
				log.Error("SendCommand failed: {0}", e);
				throw;
			}
		}

		public void StopService()
		{
			stopAction();
		}
	}
}