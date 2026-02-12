using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.OnlineStats
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class SimpleWorkTimeStat
	{
		[DataMember]
		public int WorkId { get; set; }

		[DataMember]
		public TimeSpan TotalWorkTime { get; set; }
	}
}
