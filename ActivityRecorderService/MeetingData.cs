using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "MeetingData", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class MeetingData
	{
		[DataMember]
		public List<MeetingEntry> PendingMeetings { get; set; }

		[DataMember(Order = 1)]
		public List<string> CalendarEmailAccounts { get; set; }

		[DataMember(Order = 1)]
		public DateTime? LastSuccessfulSyncDate { get; set; }

		[DataMember(Order = 2)]
		public List<MeetingEntry> UpcomingMeetings { get; set; }
	}

	[DataContract(Name = "MeetingEntry", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class MeetingEntry
	{
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public DateTime StartDate { get; set; }

		[DataMember]
		public DateTime EndDate { get; set; }

		[DataMember]
		public int? OrganizerId { get; set; }
#if RELEASE		//Client need to refresh its service reference from a service run in DEBUG 
		[DataMember]
		public string OrganizerName { get; set; }
#endif
		[DataMember(Order = 1)]
		public string OrganizerEmail { get; set; }

		[DataMember(Order = 1)]
		public string OrganizerFirstName { get; set; }

		[DataMember(Order = 1)]
		public string OrganizerLastName { get; set; }
	}
}
