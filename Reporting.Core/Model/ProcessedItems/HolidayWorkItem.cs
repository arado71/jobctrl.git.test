using System;
using Reporter.Interfaces;

namespace Reporter.Model.ProcessedItems
{
    [Serializable]
	public class HolidayWorkItem : WorkItem, IEquatable<HolidayWorkItem>
	{
		public override ItemType Type { get { return ItemType.Holiday; } }

		public HolidayWorkItem()
		{
		}

		public HolidayWorkItem(HolidayWorkItem other)
			: base(other)
		{
		}

		public HolidayWorkItem(IHolidayWorkItem workItem)
			: base(workItem)
		{
		}

		public override WorkItem Clone()
		{
			return new HolidayWorkItem(this);
		}

		public override bool Equals(object other)
		{
			return Equals(other as HolidayWorkItem);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() * 23;
		}

		public bool Equals(HolidayWorkItem other)
		{
			return base.Equals(other);
		}
	}
}
