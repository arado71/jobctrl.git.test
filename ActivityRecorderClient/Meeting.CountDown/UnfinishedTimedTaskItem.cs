using System;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.Meeting.CountDown
{
	[Serializable]
	public class UnfinishedTimedTaskItem
	{
		public WorkData WorkData { get; private set; }
		public ManualWorkItem ManualWorkItem { private set; get; }
		public long StartTicks { get; private set; }
		public WorkDataWithParentNames WorkDataWithParentNames { get; private set; }
		public bool ResumeWorkOnClose { get; private set; }
		public bool IsPermanent { get; private set; }
		public UnfinishedTimedTaskItem(WorkData workdata, ManualWorkItem manualworkitem, long startticks, WorkDataWithParentNames wdwpn, bool resume, bool ispermanent)
		{
			WorkData = workdata;
			ManualWorkItem = manualworkitem;
			StartTicks = startticks;
			WorkDataWithParentNames = wdwpn;
			ResumeWorkOnClose = resume;
			IsPermanent = ispermanent;
		}
	}
}
