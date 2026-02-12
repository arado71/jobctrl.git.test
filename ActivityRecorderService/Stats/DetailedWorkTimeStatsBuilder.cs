using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	/// <summary>
	/// Class for calculating work times from different intervals. (Using WorkItems for computer worktime)
	/// </summary>
	public class DetailedWorkTimeStatsBuilder
	{
		private readonly List<WorkItem> workItems = new List<WorkItem>();
		private readonly Dictionary<int, ManualWorkItem> manualWorkItems = new Dictionary<int, ManualWorkItem>();
		private readonly Dictionary<long, MobileWorkItem> mobileWorkItems = new Dictionary<long, MobileWorkItem>();

		public void AddWorkItem(WorkItem item)
		{
			workItems.Add(item);
		}

		public void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			manualWorkItems[manualWorkItem.Id] = manualWorkItem;
		}

		public void AddMobileWorkItem(MobileWorkItem mobileWorkItem)
		{
			mobileWorkItems[mobileWorkItem.Id] = mobileWorkItem;
		}

		public void AddWorkItems(IEnumerable<WorkItem> items)
		{
			foreach (var item in items)
			{
				AddWorkItem(item);
			}
		}

		public void AddManualWorkItems(IEnumerable<ManualWorkItem> items)
		{
			foreach (var item in items)
			{
				AddManualWorkItem(item);
			}
		}

		public void AddMobileWorkItems(IEnumerable<MobileWorkItem> items)
		{
			foreach (var item in items)
			{
				AddMobileWorkItem(item);
			}
		}

		public DetailedWorkTimeStats GetDetailedWorkTime(DateTime startDate, DateTime endDate)
		{
			var result = new DetailedWorkTimeStats()
			{
				StartDate = startDate,
				EndDate = endDate,
				ManualWorkItems = manualWorkItems.Values
					.Where(n => startDate < n.EndDate)
					.Where(n => n.StartDate < endDate)
					.ToList(),
				WorkItems = workItems
					.Where(n => startDate < n.EndDate)
					.Where(n => n.StartDate < endDate)
					.ToList(),
				MobileWorkItems = mobileWorkItems.Values
					.Where(n => startDate < n.EndDate)
					.Where(n => n.StartDate < endDate)
					.ToList(),
				ComputerWorkTimeById = new Dictionary<int, TimeSpan>(),
				IvrWorkTimeById = new Dictionary<int, TimeSpan>(),
				MobileWorkTimeById = new Dictionary<int, TimeSpan>(),
				AllWorkTimeById = new Dictionary<int, TimeSpan>(),
			};
			var inverseQueryInterval = new IntervalConcatenator();
			inverseQueryInterval.Add(DateTime.MinValue, startDate);
			inverseQueryInterval.Add(endDate, DateTime.MaxValue);

			var ivrIntervals = new IntervalConcatenator();
			var comIntervals = new IntervalConcatenator();
			var mobIntervals = new IntervalConcatenator();

			//we don't handle deletion by WorkId atm.
			var comCorrIntervals = new IntervalConcatenator();
			var ivrCorrIntervals = new IntervalConcatenator();
			var mobCorrIntervals = new IntervalConcatenator();
			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteComputerInterval))
			{
				comCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
			}
			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteIvrInterval))
			{
				ivrCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
			}
			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteMobileInterval))
			{
				mobCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
			}
			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteInterval))
			{
				comCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
				ivrCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
				mobCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
			}
			//we don't care about dates outside of the query interval
			comCorrIntervals.Subtract(inverseQueryInterval);
			ivrCorrIntervals.Subtract(inverseQueryInterval);
			mobCorrIntervals.Subtract(inverseQueryInterval);

			foreach (var mobileWorkItem in result.MobileWorkItems)
			{
				var startEndDate = GetIntersectStartEndDateTime(mobileWorkItem.StartDate, mobileWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				var duration = startEndDate.Value.Duration();
				var workIntervals = new IntervalConcatenator();
				workIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);
				workIntervals.Subtract(mobCorrIntervals);
				var correctedDuration = workIntervals.Duration();
				result.MobileWorkTimeWithoutCorrection += duration;
				result.MobileWorkTime += correctedDuration;
				mobIntervals.Merge(workIntervals);
				IncreaseDictValue(result.MobileWorkTimeById, mobileWorkItem.WorkId, correctedDuration);
				IncreaseDictValue(result.AllWorkTimeById, mobileWorkItem.WorkId, correctedDuration);
			}

			foreach (var workItem in result.WorkItems)
			{
				var startEndDate = GetIntersectStartEndDateTime(workItem.StartDate, workItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				var duration = startEndDate.Value.Duration();
				var workIntervals = new IntervalConcatenator();
				workIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);
				workIntervals.Subtract(comCorrIntervals);
				var correctedDuration = workIntervals.Duration();
				result.ComputerWorkTimeWithoutCorrection += duration;
				result.ComputerWorkTime += correctedDuration;
				comIntervals.Merge(workIntervals);
				IncreaseDictValue(result.ComputerWorkTimeById, workItem.WorkId, correctedDuration);
				IncreaseDictValue(result.AllWorkTimeById, workItem.WorkId, correctedDuration);

				if (workItem.IsRemoteDesktop)
				{
					result.RemoteDesktopComputerWorkTime += correctedDuration;
				}
				if (workItem.IsVirtualMachine)
				{
					result.VirtualMachineComputerWorkTime += correctedDuration;
				}
			}

			var sumIntervals = comIntervals.Clone();
			sumIntervals.Merge(ivrIntervals);
			sumIntervals.Merge(mobIntervals);

			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddWork))
			{
				var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				result.ManuallyAddedWorkTime += startEndDate.Value.Duration();
				sumIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);
				IncreaseDictValue(result.AllWorkTimeById, manualWorkItem.WorkId ?? -1, startEndDate.Value.Duration());
			}

			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddHoliday))
			{
				var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				result.HolidayTime += startEndDate.Value.Duration();
				IncreaseDictValue(result.AllWorkTimeById, manualWorkItem.WorkId ?? -1, startEndDate.Value.Duration());
			}

			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddSickLeave))
			{
				var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				result.SickLeaveTime += startEndDate.Value.Duration();
				IncreaseDictValue(result.AllWorkTimeById, manualWorkItem.WorkId ?? -1, startEndDate.Value.Duration());
			}

			sumIntervals.Subtract(inverseQueryInterval);
			comIntervals.Subtract(inverseQueryInterval);
			ivrIntervals.Subtract(inverseQueryInterval);
			mobIntervals.Subtract(inverseQueryInterval);

			result.NetComputerWorkTime = comIntervals.Duration();
			result.NetMobileWorkTime = mobIntervals.Duration();

			result.SumWorkTime = sumIntervals.Duration() + result.HolidayTime + result.SickLeaveTime;

			var realWorkStartEnd = sumIntervals.GetBoundaries(); //don't calculate holidays and sickleaves into this
			if (realWorkStartEnd.HasValue)
			{
				result.WorkStartDate = realWorkStartEnd.Value.StartDate;
				result.WorkEndDate = realWorkStartEnd.Value.EndDate;
			}

			return result;
		}

		private static StartEndDateTime? GetIntersectStartEndDateTime(DateTime firstStart, DateTime firstEnd, DateTime secondStart, DateTime secondEnd)
		{
			if (firstEnd < firstStart || secondEnd < secondStart) return null; //invalid intervals
			var result = new StartEndDateTime(
				firstStart < secondStart ? secondStart : firstStart, //MAX(firstStart, secondStart)
				secondEnd < firstEnd ? secondEnd : firstEnd);        //MIN(secondEnd, firstEnd)
			return result.EndDate < result.StartDate ? new StartEndDateTime?() : result;
		}

		private static void IncreaseDictValue(IDictionary<int, TimeSpan> dict, int key, TimeSpan value)
		{
			TimeSpan prevValue;
			if (dict.TryGetValue(key, out prevValue))
			{
				dict[key] = prevValue + value;
			}
			else
			{
				dict.Add(key, value);
			}
		}
	}
}
