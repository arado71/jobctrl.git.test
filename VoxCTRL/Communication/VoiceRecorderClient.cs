using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace VoxCTRL.ActivityRecorderServiceReference
{
	partial class VoiceRecorderClient
	{
		public TimeSpan OperationTimeout
		{
			get { return ((IContextChannel)base.Channel).OperationTimeout; }
			set { ((IContextChannel)base.Channel).OperationTimeout = value; }
		}
	}
}
