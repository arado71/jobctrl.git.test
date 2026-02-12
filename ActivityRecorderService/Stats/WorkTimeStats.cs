using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class WorkTimeStats
	{
		[DataMember]
		public TimeSpan ComputerWorkTime { get; set; }
		[DataMember]
		public TimeSpan SumWorkTime { get; set; }
	}
}