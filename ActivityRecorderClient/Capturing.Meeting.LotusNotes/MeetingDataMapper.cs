using System.Collections.Generic;
using System.Linq;

namespace Tct.ActivityRecorderClient.Capturing.Meeting.LotusNotes
{
	public class MeetingDataMapper
	{
		public static List<ActivityRecorderServiceReference.FinishedMeetingEntry> To(List<LotusNotesMeetingCaptureServiceReference.FinishedMeetingEntry> finishedMeetings)
		{
			if (finishedMeetings == null) return new List<ActivityRecorderServiceReference.FinishedMeetingEntry>();
			return finishedMeetings.Select(m => To(m)).ToList();

		}

		private static ActivityRecorderServiceReference.FinishedMeetingEntry To(LotusNotesMeetingCaptureServiceReference.FinishedMeetingEntry finishedMeeting)
		{
			return new ActivityRecorderServiceReference.FinishedMeetingEntry()
			{
				Id = finishedMeeting.Id,
				CreationTime = finishedMeeting.CreationTime,
				LastmodificationTime = finishedMeeting.LastmodificationTime,
				Title = finishedMeeting.Title,
				Description = finishedMeeting.Description,
				Location = finishedMeeting.Location,
				StartTime = finishedMeeting.StartTime,
				EndTime = finishedMeeting.EndTime,
				Attendees = To(finishedMeeting.Attendees)
			};

		}

		public static List<ActivityRecorderServiceReference.MeetingAttendee> To(List<LotusNotesMeetingCaptureServiceReference.MeetingAttendee> attendees)
		{
			if (attendees == null) return new List<ActivityRecorderServiceReference.MeetingAttendee>();
			return attendees.Select(a => To(a)).ToList();
		}

		private static ActivityRecorderServiceReference.MeetingAttendee To(LotusNotesMeetingCaptureServiceReference.MeetingAttendee attendee)
		{
			return new ActivityRecorderServiceReference.MeetingAttendee()
			{
				Email = attendee.Email,
				Type = To(attendee.Type),
				ResponseStatus = To(attendee.ResponseStatus)
			};
		}

		private static ActivityRecorderServiceReference.MeetingAttendeeType To(LotusNotesMeetingCaptureServiceReference.MeetingAttendeeType meetingAttendeeType)
		{
			switch (meetingAttendeeType)
			{
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeType.Organizer:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Organizer;
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeType.Required:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Required;
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeType.Optional:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Optional;
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeType.Resource:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Resource;
				default:
					throw new DataMappingException("MeetingAttendeeType mapping error!");
			}
		}

		private static ActivityRecorderServiceReference.MeetingAttendeeResponseStatus To(LotusNotesMeetingCaptureServiceReference.MeetingAttendeeResponseStatus meetingAttendeeResponseStatus)
		{
			switch (meetingAttendeeResponseStatus)
			{
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseNone:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseNone;
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseOrganized:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseOrganized;
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseTentative:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseTentative;
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseAccepted:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseAccepted;
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseDeclined:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseDeclined;
				case LotusNotesMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseNotResponded:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseNotResponded;
				default:
					throw new DataMappingException("MeetingAttendeeResponseStatus mapping error!");
			}
		}
	}
}
