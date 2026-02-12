using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	partial class MeetingData
	{
		public override string ToString()
		{
			string lastSyncDate = LastSuccessfulSyncDate == null ? "N/A" : LastSuccessfulSyncDate.ToString();
			string calendarEmailAccounts = CalendarEmailAccounts == null ? "N/A" : String.Join(", ", CalendarEmailAccounts.ToArray());
			int pendingMeetingCount = PendingMeetings == null ? 0 : PendingMeetings.Count;
			return String.Format("LastSuccesfullSyncDate = {0}, CalendarEmailAccounts = [{1}], Number of PendingMeetings = {2}, Number of OngoingMeetings = {3}", lastSyncDate, calendarEmailAccounts, pendingMeetingCount, UpcomingMeetings?.Count ?? 0);
		}
	}

	partial class MeetingEntry
	{
		public string OrganizerName
		{
			get
			{
				string namePattern = CultureInfo.CurrentUICulture.LCID == 1038 ? "{1} {0}" : "{0} {1}";	//TODO: Get name order for CultureInfo.
				return string.Format(namePattern, OrganizerFirstName, OrganizerLastName);
			}
		}
	}

	partial class FinishedMeetingData
	{
		public override string ToString()
		{
			string startDate = QueryIntervalStartDate == null ? "N/A" : QueryIntervalStartDate.ToShortDateString();
			string startTime = QueryIntervalStartDate == null ? "N/A" : QueryIntervalStartDate.ToShortTimeString();
			string endDate = QueryIntervalEndDate == null ? "N/A" : QueryIntervalEndDate.ToShortDateString();
			string endTime = QueryIntervalEndDate == null ? "N/A" : QueryIntervalEndDate.ToShortTimeString();
			string queryInterval = String.Format("({0} {1} - {2})", startDate, startTime, (startDate == endDate ? "" : endDate + " ") + endTime);
			string calendarEmailAccounts = CalendarEmailAccounts == null ? "N/A" : String.Join(", ", CalendarEmailAccounts.ToArray());
			string finishedMeetingsStr = String.Join(", \r\n", FinishedMeetings.Select(f => " - " + f.ToString()).ToArray());
			return String.Format("Query interval = {0}, CalendarEmailAccounts = [{1}] \r\n{2}\r\n", queryInterval, calendarEmailAccounts, finishedMeetingsStr);
		}

		public DateTime QueryIntervalStartDate { get; set; } //This should be part of the DataContract

		public DateTime QueryIntervalEndDate
		{
			get { return LastQueryIntervalEndDate.Value; }
			set { LastQueryIntervalEndDate = value; }
		}

		public List<string> CalendarEmailAccounts { get; set; }	//This should be part of the DataContract
	}

	partial class FinishedMeetingEntry
	{
		public override string ToString()
		{
			string shortDateStart = StartTime == null ? "N/A" : StartTime.ToShortDateString();
			string shortTimeStart = StartTime == null ? "N/A" : StartTime.ToShortTimeString();
			string shortTimeEnd = EndTime == null ? "N/A" : EndTime.ToShortTimeString();
			string attendees = Attendees == null ? "N/A" : String.Join(", ", Attendees.Select(a => a.ToString()).ToArray());
			return String.Format("{0} {{{1}}} ({2} {3} - {4}) ({5}) [{6}]", Title, Status, shortDateStart, shortTimeStart, shortTimeEnd, CreationTime, attendees);
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

	partial class MeetingAttendee
	{
		public override string ToString()
		{
			return Type == MeetingAttendeeType.Organizer ? Email.ToUpper() : Email.ToLower();
		}
	}

}
