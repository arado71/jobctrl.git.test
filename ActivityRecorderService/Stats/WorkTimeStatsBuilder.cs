using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	//todo support mobile intervals or make this class obsolete
	public class WorkTimeStatsBuilder
	{
		private readonly IntervalConcatenator computerIntervals = new IntervalConcatenator();
		private readonly Dictionary<int, ManualWorkItem> manualWorkItems = new Dictionary<int, ManualWorkItem>();
		private TimeSpan ComputerWorkTime { get; set; }

		public void AddWorkItem(WorkItem item)
		{
			computerIntervals.Add(item.StartDate, item.EndDate);
			ComputerWorkTime += item.EndDate - item.StartDate;
		}

		public void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			manualWorkItems[manualWorkItem.Id] = manualWorkItem;
		}

		//The duration of ManualWorkItems can change so we only get a snapshot
		public WorkTimeStats GetWorkTime()
		{
			TimeSpan sumWorkTime;

			if (manualWorkItems.Count != 0)
			{
				IntervalConcatenator comIntervals = computerIntervals.Clone();
				foreach (var manualWorkItem in manualWorkItems.Values.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteComputerInterval))
				{
					comIntervals.Remove(manualWorkItem.StartDate, manualWorkItem.EndDate);
				}

				IntervalConcatenator sumIntervals = comIntervals;
				comIntervals = null; //don't use after this line as they mean something else now

				foreach (var manualWorkItem in manualWorkItems.Values.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteInterval))
				{
					sumIntervals.Remove(manualWorkItem.StartDate, manualWorkItem.EndDate);
				}
				foreach (var manualWorkItem in manualWorkItems.Values.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddWork))
				{
					sumIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
				}

				sumWorkTime = sumIntervals.Duration();

				foreach (var manualWorkItem in manualWorkItems.Values
					.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddHoliday
							 || n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddSickLeave))
				{
					sumWorkTime += (manualWorkItem.EndDate - manualWorkItem.StartDate);
				}
			}
			else
			{
				sumWorkTime = computerIntervals.Duration();
			}

			var result = new WorkTimeStats()
			{
				ComputerWorkTime = ComputerWorkTime,
				SumWorkTime = sumWorkTime,
			};
			return result;
		}
	}
}