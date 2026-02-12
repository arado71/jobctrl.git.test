using System;
using System.ServiceModel;

namespace Tct.ActivityRecorderClient.ChromeCaptureServiceReference
{
	partial class ChromeCaptureServiceClient
	{
		public TimeSpan OperationTimeout
		{
			get { return ((IContextChannel)base.Channel).OperationTimeout; }
			set { ((IContextChannel)base.Channel).OperationTimeout = value; }
		}
	}
}
