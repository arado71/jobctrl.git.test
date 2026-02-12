using System;
using System.Collections.Generic;
using Reporter.Interfaces;
using Reporter.Model.ProcessedItems;

namespace Reporter.Model.WorkItems
{
	public class ManualWorkItem : WorkItem, IManualWorkItem
	{
		public string Description { get; set; }

		public ManualWorkItem()
		{

		}

		public ManualWorkItem(IManualWorkItem other)
			: base(other)
		{
			Description = other.Description;
		}

		public override void Resize(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}

		public override WorkItem Clone()
		{
			return new ManualWorkItem(this);
		}

		public override ProcessedItems.WorkItem GetProcessedItem(DateTime startDate, DateTime endDate, Dictionary<string, string> values)
		{
			return new ProcessedItems.ManualWorkItem(this) { StartDate = startDate, EndDate = endDate, Duration = endDate - startDate, Values = values };
		}
	}
}
