using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Tct.ActivityRecorderClient.OutlookMeetingCaptureServiceReference
{
	partial class MeetingCaptureServiceClient
	{
		public TimeSpan OperationTimeout
		{
			get { return ((IContextChannel)base.Channel).OperationTimeout; }
			set { ((IContextChannel)base.Channel).OperationTimeout = value; }
		}
	}
}
