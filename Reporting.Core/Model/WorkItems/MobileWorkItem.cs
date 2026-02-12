using System;
using System.Collections.Generic;
using Reporter.Interfaces;
using Reporter.Model.ProcessedItems;

namespace Reporter.Model.WorkItems
{
	public class MobileWorkItem : WorkItem, IMobileWorkItem
	{
		public long Imei { get; set; }
        public MobileWorkitemType MobileWorkitemType { get; set; }
        public long? CallId { get; set; }

        public MobileWorkItem()
		{

		}

		public MobileWorkItem(IMobileWorkItem other)
			: base(other)
		{
			Imei = other.Imei;
		    MobileWorkitemType = other.MobileWorkitemType;
            CallId = other.CallId;
		}

		public override void Resize(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}

		public override WorkItem Clone()
		{
			return new MobileWorkItem(this);
		}

		public override ProcessedItems.WorkItem GetProcessedItem(DateTime startDate, DateTime endDate, Dictionary<string, string> values)
		{
			return new ProcessedItems.MobileWorkItem(this) { StartDate = startDate, EndDate = endDate, Duration = endDate - startDate, Values = values };
		}
	}
}
