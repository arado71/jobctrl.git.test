using System;
using System.Collections.Generic;
using Reporter.Interfaces;
using Reporter.Model.ProcessedItems;

namespace Reporter.Model.WorkItems
{
	public class AdhocMeetingWorkItem : WorkItem, IAdhocMeetingWorkItem
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string Participants { get; set; }

		public AdhocMeetingWorkItem()
		{

		}

		public AdhocMeetingWorkItem(IAdhocMeetingWorkItem other)
			: base(other)
		{
			Title = other.Title;
			Description = other.Description;
			Participants = other.Participants;
		}

		public override void Resize(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}

		public override WorkItem Clone()
		{
			return new AdhocMeetingWorkItem(this);
		}

		public override ProcessedItems.WorkItem GetProcessedItem(DateTime startDate, DateTime endDate, Dictionary<string, string> values)
		{
			return new ProcessedItems.AdhocMeetingWorkItem(this) { StartDate = startDate, EndDate = endDate, Duration = endDate - startDate, Values = values };
		}
	}
}
