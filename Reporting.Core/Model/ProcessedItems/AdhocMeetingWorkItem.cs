using System;
using Reporter.Interfaces;

namespace Reporter.Model.ProcessedItems
{
    [Serializable]
	public class AdhocMeetingWorkItem : WorkItem, IEquatable<AdhocMeetingWorkItem>
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string Participants { get; set; }

		public override ItemType Type { get { return ItemType.AdhocMeeting; } }

		public AdhocMeetingWorkItem()
		{

		}

		public AdhocMeetingWorkItem(AdhocMeetingWorkItem other)
			: base(other)
		{
			Title = other.Title;
			Description = other.Description;
			Participants = other.Participants;
		}

		public AdhocMeetingWorkItem(IAdhocMeetingWorkItem workItem)
			: base(workItem)
		{
			Title = workItem.Title;
			Description = workItem.Description;
			Participants = workItem.Participants;
		}

		public override WorkItem Clone()
		{
			return new AdhocMeetingWorkItem(this);
		}

		public override bool Equals(object other)
		{
			return Equals(other as AdhocMeetingWorkItem);
		}

		public override int GetHashCode()
		{
			var hash = base.GetHashCode();
			hash = hash * 23 + Title.GetHashCode();
			hash = hash * 23 + Description.GetHashCode();
			hash = hash * 23 + Participants.GetHashCode();
			return hash;
		}

		public bool Equals(AdhocMeetingWorkItem other)
		{
			return base.Equals(other)
				   && Title == other.Title
				   && Description == other.Description
				   && Participants == other.Participants;
		}
	}
}
