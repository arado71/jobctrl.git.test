using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public interface IWorkTimeQuery
	{
		TimeSpan GetStartOfDayOffset(int userId);
		ClientWorkTimeHistory GetStats(DateTime startTime, DateTime endTime, int userId);
		void Modify(WorkTimeModifications modifications, int userId);
		WorkNames GetWorkNames(int userId, List<int> workIds);
		event EventHandler WorkTimeUpdatesSent;
	}
}
