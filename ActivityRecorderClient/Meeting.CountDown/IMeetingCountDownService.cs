using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Controller;

namespace Tct.ActivityRecorderClient.Meeting.CountDown
{
	public interface IMeetingCountDownService : IMutualWorkTypeService
	{
		event EventHandler<SingleValueEventArgs<ManualWorkItem>> ManualWorkItemCreated;
		bool IsPermanent { get; }
		bool ResumeWorkOnClose { get; }
		ManualWorkItem CurrentWorkItem { get; }
		void StartWork(WorkData workData, bool isPermanent, bool isForced);
		void CheckUnfinishedTimedTask();
	}
}
