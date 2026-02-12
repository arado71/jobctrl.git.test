using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public class WcfWorkTimeQuery : IWorkTimeQuery
	{
		public TimeSpan GetStartOfDayOffset(int userId)
		{
			DebugEx.EnsureBgThread();
			return ActivityRecorderClientWrapper.Execute(n => n.GetStartOfDayOffset(userId));
		}

		public ClientWorkTimeHistory GetStats(DateTime startTime, DateTime endTime, int userId)
		{
			DebugEx.EnsureBgThread();
			return ActivityRecorderClientWrapper.Execute(n => n.GetWorkTimeHistory(userId, startTime, endTime));
		}

		public void Modify(WorkTimeModifications modifications, int userId)
		{
			DebugEx.EnsureBgThread();
			ActivityRecorderClientWrapper.Execute(n => n.ModifyWorkTime(userId, modifications));
			WorkTimeUpdatesSent?.Invoke(this, EventArgs.Empty);
		}


		public WorkNames GetWorkNames(int userId, List<int> workIds)
		{
			DebugEx.EnsureBgThread();
			return ActivityRecorderClientWrapper.Execute(n => n.GetWorkNames(userId, workIds));
		}

		public event EventHandler WorkTimeUpdatesSent;
	}
}
