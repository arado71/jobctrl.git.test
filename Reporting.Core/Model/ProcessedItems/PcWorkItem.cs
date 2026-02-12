using System;
using Reporter.Model.WorkItems;

namespace Reporter.Model.ProcessedItems
{
	[Serializable]
	public class PcWorkItem : WorkItem, IEquatable<PcWorkItem>
	{
		public double KeyboardActivity { get; set; }
		public double MouseActivity { get; set; }
		public int ComputerId { get; set; }

		public override ItemType Type { get { return ItemType.Pc; } }

		public PcWorkItem()
		{
		}

		public PcWorkItem(WorkItems.ComputerWorkItem item, DateTime startDate, DateTime endDate)
			: base(item, startDate, endDate)
		{
			var scale = (endDate - startDate).Ticks / (double)(item.EndDate - item.StartDate).Ticks;
			KeyboardActivity = item.KeyboardActivity * scale;
			MouseActivity = item.MouseActivity * scale;
			ComputerId = item.ComputerId;
			StartDate = startDate;
			EndDate = endDate;
		}

		public PcWorkItem(PcWorkItem other)
			: base(other)
		{
			ComputerId = other.ComputerId;
			KeyboardActivity = other.KeyboardActivity;
			MouseActivity = other.MouseActivity;
		}

		public override WorkItem Clone()
		{
			return new PcWorkItem(this);
		}

		public override bool Equals(object other)
		{
			return Equals(other as PcWorkItem);
		}

		public override int GetHashCode()
		{
			var hash = base.GetHashCode();
			hash = hash * 23 + ComputerId.GetHashCode();
			return hash;
		}

		public bool Equals(PcWorkItem other)
		{
			return base.Equals(other)
				   && ComputerId == other.ComputerId
				   && KeyboardActivity == other.KeyboardActivity
				   && MouseActivity == other.MouseActivity;
		}
	}
}
