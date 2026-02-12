using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class TotalWorkTimeStats
	{
		[DataMember]
		public int UserId { get; set; }

		[DataMember]
		public DateTime FromDate { get; set; }

		[DataMember]
		public DateTime ToDate { get; set; }

		[DataMember]
		public Dictionary<int, TotalWorkTimeStat> Stats { get; set; }
	}
}
