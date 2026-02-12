using System;
using System.Collections.Generic;
using Reporter.Interfaces;
using Reporter.Model.ProcessedItems;

namespace Reporter.Model.WorkItems
{
	public class SickLeaveWorkItem : WorkItem, ISickLeaveWorkItem
	{
		public SickLeaveWorkItem()
		{

		}

		public SickLeaveWorkItem(ISickLeaveWorkItem other)
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
			return new SickLeaveWorkItem(this);
		}

		public override ProcessedItems.WorkItem GetProcessedItem(DateTime startDate, DateTime endDate, Dictionary<string, string> values)
		{
			return new ProcessedItems.SickLeaveWorkItem(this) { StartDate = startDate, EndDate = endDate, Duration = endDate - startDate, Values = values };
		}
	}
}
