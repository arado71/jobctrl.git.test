using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public class WorkTimeDummyService : WorkTimeService
	{
		public WorkTimeDummyService(IWorkTimeQuery workTimeHistory) : base(workTimeHistory)
		{
		}

		public override void ShowModification(DateTime? localDay = null)
		{
			Debug.Fail("ShowModification not implemented");
		}

		public override void ShowModifyWork(DeviceWorkInterval workInterval)
		{
			Debug.Fail("ShowModifyWork not implemented");
		}

		public override void ShowModifyInterval(Interval localInterval)
		{
			Debug.Fail("ShowModifyInterval not implemented");
		}
	}
}
