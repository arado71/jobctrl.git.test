using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.OnlineStats
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ClientWorkTimeStats
	{
		[DataMember]
		public BriefNetWorkTimeStats TodaysWorkTime { get; set; }
		[DataMember]
		public BriefNetWorkTimeStats ThisWeeksWorkTime { get; set; }
		[DataMember]
		public BriefNetWorkTimeStats ThisMonthsWorkTime { get; set; }

		[DataMember(Order = 1)]
		public TimeSpan TodaysTargetNetWorkTime { get; set; }
		[DataMember(Order = 1)]
		public TimeSpan ThisWeeksTargetNetWorkTime { get; set; }
		[DataMember(Order = 1)]
		public TimeSpan ThisMonthsTargetNetWorkTime { get; set; }

		[DataMember(Order = 1)]
		public TimeSpan ThisWeeksTargetUntilTodayNetWorkTime { get; set; } //including today
		[DataMember(Order = 1)]
		public TimeSpan ThisMonthsTargetUntilTodayNetWorkTime { get; set; } //including today
	}
}
