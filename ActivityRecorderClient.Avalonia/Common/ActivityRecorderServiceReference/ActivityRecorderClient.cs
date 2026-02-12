using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	partial class ActivityRecorderClient
	{
		public string EndpointName { get; set; }

		public TimeSpan OperationTimeout
		{
			get { return ((IContextChannel)base.Channel).OperationTimeout; }
			set { ((IContextChannel)base.Channel).OperationTimeout = value; }
		}

		public ConfigManager.ProxySettings ProxySetting { get; set; }
	}
}
