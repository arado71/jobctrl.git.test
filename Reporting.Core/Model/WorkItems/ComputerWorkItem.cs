using System;
using System.Collections.Generic;
using Reporter.Interfaces;
using Reporter.Model.ProcessedItems;

namespace Reporter.Model.WorkItems
{
	public class ComputerWorkItem : WorkItem, IComputerWorkItem
	{
		public ComputerWorkItem()
		{
		}

		public ComputerWorkItem(IComputerWorkItem other)
			: base(other)
		{
			ComputerId = other.ComputerId;
			MouseActivity = other.MouseActivity;
			KeyboardActivity = other.KeyboardActivity;
		}

		public TimeSpan Duration()
		{
			return EndDate - StartDate;
		}

		public override void Resize(DateTime startDate, DateTime endDate)
		{
			var scale = (endDate - startDate).Ticks / (double)(EndDate - StartDate).Ticks;
			MouseActivity = (int)Math.Ceiling(MouseActivity * scale);
			KeyboardActivity = (int)Math.Ceiling(KeyboardActivity * scale);
			StartDate = startDate;
			EndDate = endDate;
		}

		public override ProcessedItems.WorkItem GetProcessedItem(DateTime startDate, DateTime endDate, Dictionary<string, string> values)
		{
			return new ProcessedItems.PcWorkItem(this, startDate, endDate) { Values = values };
		}

		public override WorkItem Clone()
		{
			return new ComputerWorkItem(this);
		}

		public int ComputerId { get; set; }
		public int MouseActivity { get; set; }
		public int KeyboardActivity { get; set; }
	}
}
