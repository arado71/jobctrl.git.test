using System;
using Reporter.Interfaces;

namespace Reporter.Model.ProcessedItems
{
    [Serializable]
	public class MobileWorkItem : WorkItem, IEquatable<MobileWorkItem>
	{
		public long Imei { get; set; }
        public MobileWorkitemType MobileWorkitemType { get; set; }
        public override ItemType Type { get { return ItemType.Mobile; } }
        public long? CallId { get; set; }

        public MobileWorkItem()
		{
		}

		public MobileWorkItem(MobileWorkItem other)
			: base(other)
		{
			Imei = other.Imei;
            MobileWorkitemType = other.MobileWorkitemType;
            CallId = other.CallId;
		}

		public MobileWorkItem(IMobileWorkItem workItem)
			: base(workItem)
		{
			Imei = workItem.Imei;
		    MobileWorkitemType = workItem.MobileWorkitemType;
            CallId = workItem.CallId;
		}

		public override WorkItem Clone()
		{
			return new MobileWorkItem(this);
		}

		public override bool Equals(object other)
		{
			return Equals(other as MobileWorkItem);
		}

		public override int GetHashCode()
		{
            var hash = base.GetHashCode();
            hash = hash * 23 + Imei.GetHashCode();
            hash = hash * 23 + MobileWorkitemType.GetHashCode();
            hash = hash * 23 + CallId.GetValueOrDefault().GetHashCode();
            return hash;
		}

		public bool Equals(MobileWorkItem other)
		{
			return base.Equals(other) 
                    && Imei == other.Imei
                    && MobileWorkitemType == other.MobileWorkitemType
                    && CallId == other.CallId;
        }
	}
}
