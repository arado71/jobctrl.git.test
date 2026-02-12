using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Meeting.Outlook
{
	public class MeetingDataMapper
	{
		public static List<ActivityRecorderServiceReference.FinishedMeetingEntry> To(List<OutlookMeetingCaptureServiceReference.FinishedMeetingEntry> finishedMeetings)
		{
			if (finishedMeetings == null) return new List<ActivityRecorderServiceReference.FinishedMeetingEntry>();
			return finishedMeetings.Select(To).ToList();

		}

		private static readonly Regex taskIdPattern = new Regex(@"\[\s*[a-z]+\s*#\s*(?<id>[0-9]+)\s*\]", RegexOptions.IgnoreCase);
		private static ActivityRecorderServiceReference.FinishedMeetingEntry To(OutlookMeetingCaptureServiceReference.FinishedMeetingEntry finishedMeeting)
		{
			var desc = finishedMeeting.Description;
			if (!ConfigManager.IsMeetingDescriptionSynchronized && !string.IsNullOrEmpty(desc))
			{
				var matcher = taskIdPattern.Match(desc);
				desc = matcher.Success ? matcher.Value : null;
			}
			return new ActivityRecorderServiceReference.FinishedMeetingEntry()
			{
				Id = finishedMeeting.Id,
				CreationTime = finishedMeeting.CreationTime,
				LastmodificationTime = finishedMeeting.LastmodificationTime,
				Title = finishedMeeting.Title,
				Description = desc,
				Location = finishedMeeting.Location,
				StartTime = finishedMeeting.StartTime,
				EndTime = finishedMeeting.EndTime,
				OldStartTime = finishedMeeting.OldStartTime,
				Status = To(finishedMeeting.Status),
				Attendees = To(finishedMeeting.Attendees),
				IsInFuture = finishedMeeting.IsInFuture
			};

		}

		private static MeetingCrudStatus? To(OutlookMeetingCaptureServiceReference.MeetingCrudStatus? status)
		{
			switch (status)
			{
				case OutlookMeetingCaptureServiceReference.MeetingCrudStatus.Created:
					return MeetingCrudStatus.Created;
				case OutlookMeetingCaptureServiceReference.MeetingCrudStatus.Updated:
					return MeetingCrudStatus.Updated;
				case OutlookMeetingCaptureServiceReference.MeetingCrudStatus.Deleted:
					return MeetingCrudStatus.Deleted;
				case null:
					return null;
				default:
					throw new DataMappingException("MeetingCrudStatus mapping error!");
			}
		}

		public static List<ActivityRecorderServiceReference.MeetingAttendee> To(List<OutlookMeetingCaptureServiceReference.MeetingAttendee> attendees)
		{
			if (attendees == null) return new List<ActivityRecorderServiceReference.MeetingAttendee>();
			return attendees.Select(To).ToList();
		}

		private static ActivityRecorderServiceReference.MeetingAttendee To(OutlookMeetingCaptureServiceReference.MeetingAttendee attendee)
		{
			return new ActivityRecorderServiceReference.MeetingAttendee()
			{
				Email = attendee.Email,
				Type = To(attendee.Type),
				ResponseStatus = To(attendee.ResponseStatus)
			};
		}

		private static ActivityRecorderServiceReference.MeetingAttendeeType To(OutlookMeetingCaptureServiceReference.MeetingAttendeeType meetingAttendeeType)
		{
			switch (meetingAttendeeType)
			{
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeType.Organizer:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Organizer;
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeType.Required:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Required;
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeType.Optional:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Optional;
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeType.Resource:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Resource;
				default:
					throw new DataMappingException("MeetingAttendeeType mapping error!");
			}
		}

		private static ActivityRecorderServiceReference.MeetingAttendeeResponseStatus To(OutlookMeetingCaptureServiceReference.MeetingAttendeeResponseStatus meetingAttendeeResponseStatus)
		{
			switch (meetingAttendeeResponseStatus)
			{
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseNone:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseNone;
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseOrganized:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseOrganized;
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseTentative:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseTentative;
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseAccepted:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseAccepted;
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseDeclined:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseDeclined;
				case OutlookMeetingCaptureServiceReference.MeetingAttendeeResponseStatus.ResponseNotResponded:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseNotResponded;
				default:
					throw new DataMappingException("MeetingAttendeeResponseStatus mapping error!");
			}
		}

	}
}
