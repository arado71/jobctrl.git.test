using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.Meeting
{
	[DataContract(Name = "FinishedMeetingEntryDead", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class FinishedMeetingEntryDead
	{
		[DataMember]
		public int UserId { get; set; }

		[DataMember]
		public int ComputerId { get; set; }

		[DataMember]
		public FinishedMeetingEntry FinishedMeetingEntry { get; set; }

		public override string ToString()
		{
			return "uid: " + UserId + " " + FinishedMeetingEntry;
		}
	}
}
