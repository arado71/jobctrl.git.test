using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class WorkItemEventArgs : EventArgs
	{
		public WorkItem WorkItem { get; private set; }

		public WorkItemEventArgs(WorkItem workItem)
		{
			if (workItem == null) throw new ArgumentNullException("workItem");
			WorkItem = workItem;
		}
	}
}
