using System;
using System.Collections.Generic;
using Reporter.Interfaces;
using Reporter.Model.ProcessedItems;

namespace Reporter.Model.WorkItems
{
	public class CalendarMeetingWorkItem : WorkItem, ICalendarMeetingWorkItem
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string Participants { get; set; }
        public string Location { get; set; }
        public string OrganizerEmail { get; set; }

        public CalendarMeetingWorkItem()
		{

		}

		public CalendarMeetingWorkItem(ICalendarMeetingWorkItem other)
			: base(other)
		{
			Title = other.Title;
			Description = other.Description;
			Participants = other.Participants;
            Location = other.Location;
            OrganizerEmail = other.OrganizerEmail;
        }

		public override void Resize(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}

		public override WorkItem Clone()
		{
			return new CalendarMeetingWorkItem(this);
		}

		public override ProcessedItems.WorkItem GetProcessedItem(DateTime startDate, DateTime endDate, Dictionary<string, string> values)
		{
			return new ProcessedItems.CalendarMeetingWorkItem(this) { StartDate = startDate, EndDate = endDate, Duration = endDate - startDate, Values = values };
		}
	}
}
