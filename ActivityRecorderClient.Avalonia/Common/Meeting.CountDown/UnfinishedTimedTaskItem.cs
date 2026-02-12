using System;
using System.Runtime.Serialization;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.Meeting.CountDown
{
	[DataContract]
	public partial class UnfinishedTimedTaskItem
	{
		[DataMember]
		public WorkData WorkData { get; private set; }
		[DataMember]
		public ManualWorkItem ManualWorkItem { private set; get; }
		[DataMember]
		public long StartTicks { get; private set; }
		[DataMember]
		public WorkDataWithParentNames WorkDataWithParentNames { get; private set; }
		[DataMember]
		public bool ResumeWorkOnClose { get; private set; }
		[DataMember]
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
