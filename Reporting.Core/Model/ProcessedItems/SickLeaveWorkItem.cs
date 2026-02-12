using System;
using Reporter.Interfaces;

namespace Reporter.Model.ProcessedItems
{
    [Serializable]
	public class SickLeaveWorkItem : WorkItem, IEquatable<SickLeaveWorkItem>
	{
		public override ItemType Type { get { return ItemType.SickLeave; } }

		public SickLeaveWorkItem()
		{
		}

		public SickLeaveWorkItem(SickLeaveWorkItem other)
			: base(other)
		{
		}

		public SickLeaveWorkItem(ISickLeaveWorkItem workItem)
			: base(workItem)
		{
		}

		public override WorkItem Clone()
		{
			return new SickLeaveWorkItem(this);
		}

		public override bool Equals(object other)
		{
			return Equals(other as SickLeaveWorkItem);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() * 23;
		}

		public bool Equals(SickLeaveWorkItem other)
		{
			return base.Equals(other);
		}
	}
}
