using System;
using Reporter.Interfaces;

namespace Reporter.Model.ProcessedItems
{
    [Serializable]
	public class CalendarMeetingWorkItem : WorkItem, IEquatable<CalendarMeetingWorkItem>
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string Participants { get; set; }
        public string Location { get; set; }
        public string OrganizerEmail { get; set; }

        public override ItemType Type { get { return ItemType.CalendarMeeting; } }

		public CalendarMeetingWorkItem()
		{

		}

		public CalendarMeetingWorkItem(CalendarMeetingWorkItem other)
			: base(other)
		{
			Title = other.Title;
			Description = other.Description;
			Participants = other.Participants;
            Location = other.Location;
            OrganizerEmail = other.OrganizerEmail;
        }

		public CalendarMeetingWorkItem(ICalendarMeetingWorkItem workItem)
			: base(workItem)
		{
			Title = workItem.Title;
			Description = workItem.Description;
			Participants = workItem.Participants;
            Location = workItem.Location;
            OrganizerEmail = workItem.OrganizerEmail;
        }

		public override WorkItem Clone()
		{
			return new CalendarMeetingWorkItem(this);
		}

		public override bool Equals(object other)
		{
			return Equals(other as CalendarMeetingWorkItem);
		}

		public override int GetHashCode()
		{
			var hash = base.GetHashCode();
			hash = hash * 23 + Title.GetHashCode();
			hash = hash * 23 + Description.GetHashCode();
			hash = hash * 23 + Participants.GetHashCode();
            hash = hash * 23 + Location.GetHashCode();
            hash = hash * 23 + OrganizerEmail.GetHashCode();
            return hash;
		}

		public bool Equals(CalendarMeetingWorkItem other)
		{
			return base.Equals(other)
				   && Title == other.Title
				   && Description == other.Description
				   && Participants == other.Participants
                   && Location == other.Location
                   && OrganizerEmail == other.OrganizerEmail;
		}
	}
}
