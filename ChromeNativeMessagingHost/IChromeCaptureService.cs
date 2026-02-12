using System.ServiceModel;

namespace NativeMessagingHost
{
	[ServiceContract]
	interface IChromeCaptureService
	{
		[OperationContract]
		string SendCommand(string command);

		[OperationContract]
		void StopService();
	}
}