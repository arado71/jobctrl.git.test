using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.WebsiteServiceReference;
using System.Globalization;

namespace Tct.ActivityRecorderService.Meeting
{
	public static class MeetingDataMapper
	{
		private static string nonPrintableChars = "\x0\x1\x2\x3\x4\x5\x6\x7\x8\xb\xc\xe\xf\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f\x7f\x80\x81\x82\x83\x84\x85\x86\x87\x88\x89\x8a\x8b\x8c\x8d\x8e\x8f\x90\x91\x92\x93\x94\x95\x96\x97\x98\x99\x9a\x9b\x9c\x9d\x9e\x9f";

		public static CalendarMeeting[] To(List<FinishedMeetingEntry> finsihedMeetings)
		{
			if (finsihedMeetings == null) return new CalendarMeeting[0];
			return finsihedMeetings.Select(To).ToArray();
		}

		private static string SkipNonPrintable(string input)
		{
			if (input == null) return null;
			var sb = new StringBuilder(input.Length);
			foreach (var chr in input)
			{
				if (!nonPrintableChars.Contains(chr)) sb.Append(chr);
			}
			return sb.ToString();
		}

		private static CalendarMeeting To(FinishedMeetingEntry finishedMeeting)
		{
			return new CalendarMeeting()
			{
				Id = finishedMeeting.Id,
				CreationTime = finishedMeeting.CreationTime,
				LastModificationTime = finishedMeeting.LastmodificationTime,
				Title = SkipNonPrintable(finishedMeeting.Title),
				Description = SkipNonPrintable(finishedMeeting.Description),
				Location = SkipNonPrintable(finishedMeeting.Location),
				StartTime = finishedMeeting.StartTime,
				EndTime = finishedMeeting.EndTime,
				OldStartTime = finishedMeeting.OldStartTime,
				Status = To(finishedMeeting.Status),
				Attendees = To(finishedMeeting.Attendees),
				IsInFuture = finishedMeeting.IsInFuture
			};
		}

		private static CalendarMeetingStatus To(MeetingCrudStatus? status)
		{
			switch (status)
			{
				case MeetingCrudStatus.Created:
				case null:
					return CalendarMeetingStatus.Created;
				case MeetingCrudStatus.Updated:
					return CalendarMeetingStatus.Updated;
				case MeetingCrudStatus.Deleted:
					return CalendarMeetingStatus.Deleted;
				default:
					throw new DataMappingException("CalendarMeetingStatus mapping error");
			}
		}

		private static WebsiteServiceReference.MeetingAttendee[] To(List<MeetingAttendee> attendees)
		{
			if (attendees == null) return new WebsiteServiceReference.MeetingAttendee[0];
			return attendees.Select(To).ToArray();
		}

		private static WebsiteServiceReference.MeetingAttendee To(MeetingAttendee attendee)
		{
			return new WebsiteServiceReference.MeetingAttendee()
			{
				Email = attendee.Email,
				Type = To(attendee.Type),
				ResponseStatus = To(attendee.ResponseStatus)
			};
		}

		private static WebsiteServiceReference.MeetingAttendeeType To(MeetingAttendeeType meetingAttendeeType)
		{
			switch (meetingAttendeeType)
			{
				case MeetingAttendeeType.Organizer:
					return WebsiteServiceReference.MeetingAttendeeType.Organizer;
				case MeetingAttendeeType.Required:
					return WebsiteServiceReference.MeetingAttendeeType.Required;
				case MeetingAttendeeType.Optional:
					return WebsiteServiceReference.MeetingAttendeeType.Optional;
				case MeetingAttendeeType.Resource:
					return WebsiteServiceReference.MeetingAttendeeType.Resource;
				default:
					throw new DataMappingException("MeetingAttendeeType mapping error!");
			}
		}

		private static WebsiteServiceReference.MeetingAttendeeResponseStatus To(MeetingAttendeeResponseStatus meetingAttendeeResponseStatus)
		{
			switch (meetingAttendeeResponseStatus)
			{
				case MeetingAttendeeResponseStatus.ResponseNone:
					return WebsiteServiceReference.MeetingAttendeeResponseStatus.ResponseNone;
				case MeetingAttendeeResponseStatus.ResponseOrganized:
					return WebsiteServiceReference.MeetingAttendeeResponseStatus.ResponseOrganized;
				case MeetingAttendeeResponseStatus.ResponseTentative:
					return WebsiteServiceReference.MeetingAttendeeResponseStatus.ResponseTentative;
				case MeetingAttendeeResponseStatus.ResponseAccepted:
					return WebsiteServiceReference.MeetingAttendeeResponseStatus.ResponseAccepted;
				case MeetingAttendeeResponseStatus.ResponseDeclined:
					return WebsiteServiceReference.MeetingAttendeeResponseStatus.ResponseDeclined;
				case MeetingAttendeeResponseStatus.ResponseNotResponded:
					return WebsiteServiceReference.MeetingAttendeeResponseStatus.ResponseNotResponded;
				default:
					throw new DataMappingException("MeetingAttendeeResponseStatus mapping error!");
			}
		}

		public static MeetingData From(ManageMeetingsResponse response)
		{
			return new MeetingData()
			{
				CalendarEmailAccounts = response.CalendarEmailAccounts != null ? new List<string>(response.CalendarEmailAccounts) : new List<string>(),
				LastSuccessfulSyncDate = response.LastSuccessfulSyncDate,
				PendingMeetings = From(response.MeetingstoApprove),
				UpcomingMeetings = From(response.UpcomingMeetings)
			};
		}

		public static List<MeetingEntry> From(PendingMeeting[] pendingMeetings)
		{
			if (pendingMeetings == null) return new List<MeetingEntry>();
			return pendingMeetings.Select(From).ToList();
		}

		private static MeetingEntry From(PendingMeeting pendingMeting)
		{
			return new MeetingEntry()
			{
				Id = pendingMeting.Id,
				Title = pendingMeting.Title,
				StartDate = pendingMeting.StartDate,
				EndDate = pendingMeting.EndDate,
				OrganizerId = pendingMeting.OrganizerId,
#if RELEASE
				OrganizerName = pendingMeting.OrganizerName,
#endif
				OrganizerEmail = pendingMeting.OrganizerEmail,
				OrganizerFirstName = pendingMeting.OrganizerFirstName,
				OrganizerLastName = pendingMeting.OrganizerLastName
			};
		}

		public static ManualMeeting To(ManualMeetingData meeting)
		{
			return new ManualMeeting
			{
				TaskId = meeting.WorkId,
				Title = meeting.Title.ReplaceInvalidXmlChars(" "),
				Description = meeting.Description.ReplaceInvalidXmlChars(" "),
				Location = meeting.Location.ReplaceInvalidXmlChars(" "),
				StartTime = meeting.StartTime,
				EndTime = meeting.EndTime,
				AttendeeEmails = meeting.AttendeeEmails.ToArray(),
			};
		}

		public static ManualMeeting[] To(IEnumerable<ManualMeetingData> meetings)
		{
			return meetings.Select(To).ToArray();
		}

		public static AddManualMeetingsResult From(AddManualMeetingsRet ret)
		{
			switch (ret)
			{
				case AddManualMeetingsRet.OK:
					return AddManualMeetingsResult.OK;
				case AddManualMeetingsRet.UnknownError:
					return AddManualMeetingsResult.UnknownError;
				case AddManualMeetingsRet.AuthCodeNotValid:
					return AddManualMeetingsResult.AuthCodeNotValid;
				case AddManualMeetingsRet.InvalidAttendeeEmail:
				case AddManualMeetingsRet.InvalidTaskId:
				case AddManualMeetingsRet.InvalidTimeValues:
				// TODO: map this values to separate error values
				case AddManualMeetingsRet.AddManualMeetingError:
					return AddManualMeetingsResult.AddManualMeetingError;
				default:
					return AddManualMeetingsResult.UnknownError;
			}
		}
	}
}
