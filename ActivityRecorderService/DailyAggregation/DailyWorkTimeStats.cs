using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.DailyAggregation
{
	[DataContract(Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class DailyWorkTimeStats
	{
		public int UserId { get; set; }

		[DataMember]
		public DateTime Day { get; set; }

		[DataMember]
		public long Version { get; set; }

		[DataMember]
		public TimeSpan NetWorkTime { get; set; }

		[DataMember]
		public TimeSpan ComputerWorkTime { get; set; }

		[DataMember]
		public TimeSpan MobileWorkTime { get; set; }

		[DataMember]
		public TimeSpan ManuallyAddedWorkTime { get; set; }

		[DataMember]
		public TimeSpan HolidayTime { get; set; }

		[DataMember]
		public TimeSpan SickLeaveTime { get; set; }

		[DataMember]
		public Dictionary<int, TimeSpan> TotalWorkTimeByWorkId { get; set; }

		[DataMember(Order = 1, EmitDefaultValue = false)]
		public TimeSpan? PartialInterval { get; set; }
	}
}
