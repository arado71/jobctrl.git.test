using System;
using System.Collections.Generic;
using Reporter.Interfaces;
using Reporter.Model.ProcessedItems;

namespace Reporter.Model.WorkItems
{
	public class HolidayWorkItem : WorkItem, IHolidayWorkItem
	{
		public HolidayWorkItem()
		{

		}

		public HolidayWorkItem(IHolidayWorkItem other)
			: base(other)
		{
		}

		public override void Resize(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}

		public override WorkItem Clone()
		{
			return new HolidayWorkItem(this);
		}

		public override ProcessedItems.WorkItem GetProcessedItem(DateTime startDate, DateTime endDate, Dictionary<string, string> values)
		{
			return new ProcessedItems.HolidayWorkItem(this) { StartDate = startDate, EndDate = endDate, Duration = endDate - startDate, Values = values };
		}
	}
}
