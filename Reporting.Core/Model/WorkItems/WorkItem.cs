using System;
using System.Collections.Generic;
using Reporter.Interfaces;
using Reporter.Model.ProcessedItems;

namespace Reporter.Model.WorkItems
{
	public abstract class WorkItem : IWorkItem
	{
		public int WorkId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int UserId { get; set; }

		protected WorkItem()
		{
		}

		protected WorkItem(IWorkItem other)
		{
			WorkId = other.WorkId;
			StartDate = other.StartDate;
			EndDate = other.EndDate;
			UserId = other.UserId;
		}

		public abstract void Resize(DateTime startDate, DateTime endDate);
		public abstract WorkItem Clone();
		public abstract ProcessedItems.WorkItem GetProcessedItem(DateTime startDate, DateTime endDate, Dictionary<string, string> values);
	}
}
