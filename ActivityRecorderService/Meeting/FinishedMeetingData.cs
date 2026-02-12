using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Tct.ActivityRecorderService.WebsiteServiceReference;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "FinishedMeetingData", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class FinishedMeetingData
	{
		[DataMember]
		public List<FinishedMeetingEntry> FinishedMeetings { get; set; }

		[DataMember]
		public DateTime? LastQueryIntervalEndDate { get; set; }
	}

	[DataContract(Name = "FinishedMeetingEntry", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public partial class FinishedMeetingEntry
	{
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public DateTime CreationTime { get; set; }

		[DataMember]
		public DateTime LastmodificationTime { get; set; }

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
		public List<MeetingAttendee> Attendees { get; set; }

		[DataMember]
		public DateTime? OldStartTime { get; set; }

		[DataMember]
		public MeetingCrudStatus? Status { get; set; }

		[DataMember]
		public bool IsInFuture { get; set; }

		public override string ToString()
		{
			return "start: " + StartTime + " end: " + EndTime + " id: " + Id;
		}
	}

	[DataContract(Name = "MeetingAttendee", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public partial class MeetingAttendee
	{
		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public MeetingAttendeeType Type { get; set; }

		[DataMember]
		public MeetingAttendeeResponseStatus ResponseStatus { get; set; }

		//[DataMember]
		//public MeetingAttendeeTrackingStatus TrackingStatus { get; set; }
	}

	[DataContract(Name = "MeetingAttendeeType", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum MeetingAttendeeType
	{
		[EnumMember]
		Organizer = 0,
		[EnumMember]
		Required,
		[EnumMember]
		Optional,
		[EnumMember]
		Resource
	}

	[DataContract(Name = "MeetingAttendeeResponseStatus", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum MeetingAttendeeResponseStatus
	{
		[EnumMember]
		ResponseNone = 0,
		[EnumMember]
		ResponseOrganized,
		[EnumMember]
		ResponseTentative,
		[EnumMember]
		ResponseAccepted,
		[EnumMember]
		ResponseDeclined,
		[EnumMember]
		ResponseNotResponded,
	}

	[DataContract(Name = "MeetingCrudStatus", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum MeetingCrudStatus
	{
		[EnumMember]
		Created,
		[EnumMember]
		Updated,
		[EnumMember]
		Deleted,
	}

	//[DataContract(Name = "MeetingAttendeeTrackingStatus", Namespace = "http://jobctrl.com/")]
	//public enum MeetingAttendeeTrackingStatus
	//{
	//    [EnumMember]
	//    TrackingNone = 0,
	//    [EnumMember]
	//    TrackingDelivered,
	//    [EnumMember]
	//    TrackingNotDelivered,
	//    [EnumMember]
	//    TrackingNotRead,
	//    [EnumMember]
	//    TrackingRecallFailure,
	//    [EnumMember]
	//    TrackingRecallSuccess,
	//    [EnumMember]
	//    TrackingRead,
	//    [EnumMember]
	//    TrackingReplied
	//}
}
