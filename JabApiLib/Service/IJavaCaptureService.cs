using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace Tct.Java.Service
{
	[ServiceContract]
	public interface IJavaCaptureService
	{
		[OperationContract]
		[FaultContract(typeof(FailReason))]
		KeyValuePair<string, string> Capture(JavaCaptureSettings captureSettings);

		[OperationContract]
		void StopService();

		void InitializePlugin(SynchronizationContext context);
	}
}
