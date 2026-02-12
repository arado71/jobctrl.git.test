using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "ManualMeetingData", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ManualMeetingData
	{
		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public string Location { get; set; }

		[DataMember]
		public DateTime StartTime { get; set; }

		[DataMember]
		public DateTime EndTime { get; set; }

		[DataMember]
		public List<string> AttendeeEmails { get; set; }

		[DataMember]
		public int WorkId { get; set; }

		[DataMember]
		public bool OnGoing { get; set; }

		[DataMember]
		public DateTime? OriginalStartTime { get; set; }

		[DataMember]
		public int IncludedIdleMinutes { get; set; }

		public override string ToString()
		{
			return "wid: " + WorkId + " start: " + StartTime + " end: " + EndTime + " ostart: " + OriginalStartTime + (OnGoing ? " ongoing" : "");
		}
	}
}
