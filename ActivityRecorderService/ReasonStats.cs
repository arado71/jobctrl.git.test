using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "ReasonStats", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ReasonStats
	{
		[DataMember]
		public Dictionary<int, int> ReasonCountByWorkId { get; set; }
	}
}
