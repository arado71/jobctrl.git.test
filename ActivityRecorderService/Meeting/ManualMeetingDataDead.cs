using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "ManualMeetingDataDead", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ManualMeetingDataDead
	{
		[DataMember]
		public int UserId { get; set; }

		[DataMember]
		public ManualMeetingData ManualMeetingData { get; set; }

		public override string ToString()
		{
			return "uid: " + UserId + " " + ManualMeetingData;
		}
	}
}
