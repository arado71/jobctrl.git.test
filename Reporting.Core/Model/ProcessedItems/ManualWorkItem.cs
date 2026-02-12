using System;
using Reporter.Interfaces;

namespace Reporter.Model.ProcessedItems
{
    [Serializable]
	public class ManualWorkItem : WorkItem, IEquatable<ManualWorkItem>
	{
		public string Description { get; set; }

		public override ItemType Type { get { return ItemType.Manual; } }

		public ManualWorkItem()
		{

		}

		public ManualWorkItem(ManualWorkItem other)
			: base(other)
		{
			Description = other.Description;
		}

		public ManualWorkItem(IManualWorkItem workItem)
			: base(workItem)
		{
			Description = workItem.Description;
		}

		public override WorkItem Clone()
		{
			return new ManualWorkItem(this);
		}

		public override bool Equals(object other)
		{
			return Equals(other as ManualWorkItem);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() * 23 + Description.GetHashCode();
		}

		public bool Equals(ManualWorkItem other)
		{
			return base.Equals(other) && Description == other.Description;
		}
	}
}
