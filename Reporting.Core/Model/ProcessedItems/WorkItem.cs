using System;
using System.Collections.Generic;
using System.Linq;
using Reporter.Interfaces;

namespace Reporter.Model.ProcessedItems
{
	[Serializable]
	public class WorkItem : IInterval, IEquatable<WorkItem>
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public TimeSpan Duration { get; set; }
		public int UserId { get; set; }
		public int WorkId { get; set; }
		public Dictionary<string, string> Values { get; set; }

		public virtual ItemType Type { get { return ItemType.Unknown; } }

		protected WorkItem()
		{
		}

		public WorkItem(IWorkItem baseItem)
		{
			StartDate = baseItem.StartDate;
			EndDate = baseItem.EndDate;
			Duration = baseItem.EndDate - StartDate;
			UserId = baseItem.UserId;
			WorkId = baseItem.WorkId;
			Values = new Dictionary<string, string>();
		}

		public WorkItem(IWorkItem baseItem, DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
			Duration = endDate - startDate;
			UserId = baseItem.UserId;
			WorkId = baseItem.WorkId;
			Values = new Dictionary<string, string>();
		}

		public WorkItem(WorkItem other)
		{
			StartDate = other.StartDate;
			EndDate = other.EndDate;
			Duration = other.Duration;
			UserId = other.UserId;
			WorkId = other.WorkId;
			Values = new Dictionary<string, string>(other.Values);
		}

		public virtual WorkItem Clone()
		{
			return new WorkItem(this);
		}

		public override bool Equals(object other)
		{
			return other.GetType() == GetType() && Equals(other as WorkItem);
		}

		public override int GetHashCode()
		{
			var hash = StartDate.GetHashCode();
			hash = hash * 23 + EndDate.GetHashCode();
			hash = hash * 23 + UserId.GetHashCode();
			hash = hash * 23 + WorkId.GetHashCode();
			hash = hash * 23 + Values.GetHashCode();
			return hash;
		}

		public bool Equals(WorkItem other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(other, this)) return true;
			return StartDate == other.StartDate
				   && EndDate == other.EndDate
				   && UserId == other.UserId
				   && WorkId == other.WorkId
				   && Values.Count == other.Values.Count
				   && !Values.Except(other.Values).Any();
		}
	}
}
