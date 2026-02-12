using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace LotusNotesMeetingCaptureServiceNamespace
{
	[DataContract(Name = "FinishedMeetingData", Namespace = "http://jobctrl.com/")]
	public partial class FinishedMeetingData
	{
		[DataMember]
		public List<FinishedMeetingEntry> FinishedMeetings { get; set; }
	}

	[DataContract(Name = "FinishedMeetingEntry", Namespace = "http://jobctrl.com/")]
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

		//TODO: adding properties: AccountEmail, Organizer, IsRecurring, WorkId

		public override string ToString()
		{
			string attendeesStr = Attendees == null ? "N/A" : String.Join(", ", Attendees.Select(a => a.ToString()).ToArray());
			return String.Format("[Title = {3}, Location = {4}, StartTime = {5}, EndTime = {6}, CreationTime = {1}, LastModificationTime = {2}, Attendees = {7},  Id = {0}]", Id, CreationTime, LastmodificationTime, Title, Location, StartTime, EndTime, attendeesStr);
		}

		public FinishedMeetingEntry Clone()
		{
			return new FinishedMeetingEntry()
			{
				Id = Id,
				CreationTime = CreationTime,
				LastmodificationTime = LastmodificationTime,
				Title = Title,
				Description = Description,
				Location = Location,
				StartTime = StartTime,
				EndTime = EndTime,
				Attendees = new List<MeetingAttendee>(Attendees),
			};
		}
	}

	[DataContract(Name = "MeetingAttendee", Namespace = "http://jobctrl.com/")]
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

		public override string ToString()
		{
			return String.Format("[Email={0}, Type={1}, ResponseStatus={2}]", Email, Type, ResponseStatus);
		}
	}

	[DataContract(Name = "MeetingAttendeeType", Namespace = "http://jobctrl.com/")]
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
