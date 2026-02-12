using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu
{
	public class WorkDataEventArgs : EventArgs
	{
		public WorkData WorkData { get; private set; }

		public bool OwnTask { get; private set; }

		public WorkDataEventArgs(WorkData workData, bool ownTask = true)
		{
			if (workData == null) throw new ArgumentNullException("workData");
			WorkData = workData;
			OwnTask = ownTask;
		}
	}
}
