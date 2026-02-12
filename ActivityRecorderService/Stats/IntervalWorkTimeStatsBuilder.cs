using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.OnlineStats;

namespace Tct.ActivityRecorderService.Stats
{
	/// <summary>
	/// Class for calculating work times from different intervals. (Using AggregateWorkItemIntervals for computer worktime)
	/// </summary>
	public class IntervalWorkTimeStatsBuilder
	{
		private readonly List<IComputerWorkItem> workItems = new List<IComputerWorkItem>();
		private readonly List<IManualWorkItem> manualWorkItems = new List<IManualWorkItem>();
		private readonly List<IMobileWorkItem> mobileWorkItems = new List<IMobileWorkItem>();

		public IEnumerable<IComputerWorkItem> AggregateWorkItemIntervals { get { return workItems; } }
		public IEnumerable<IManualWorkItem> ManualWorkItems { get { return manualWorkItems; } }
		public IEnumerable<IMobileWorkItem> MobileWorkItems { get { return mobileWorkItems; } }

		private void AddAggregateWorkItemInterval(IComputerWorkItem item)
		{
			workItems.Add(item);
		}

		public void RefreshAggregateWorkItemIntervals(IEnumerable<IComputerWorkItem> items)
		{
			workItems.Clear();
			workItems.Capacity = 0;
			workItems.AddRange(items);
		}

		public void RefreshManualWorkItems(IEnumerable<IManualWorkItem> items)
		{
			manualWorkItems.Clear();
			manualWorkItems.Capacity = 0;
			foreach (var item in items)
			{
				manualWorkItems.Add(item);
			}
		}

		public void RefreshMobileWorkItems(IEnumerable<IMobileWorkItem> items)
		{
			mobileWorkItems.Clear();
			mobileWorkItems.Capacity = 0;
			foreach (var item in items)
			{
				mobileWorkItems.Add(item);
			}
		}

		public IntervalWorkTimeStats GetIntervalWorkTime(DateTime startDate, DateTime endDate)
		{
			var result = new IntervalWorkTimeStats()
			{
				StartDate = startDate,
				EndDate = endDate,
				ManualWorkItems = manualWorkItems
					.Where(n => startDate < n.EndDate)
					.Where(n => n.StartDate < endDate)
					.ToList(),
				AggregateWorkItemIntervals = workItems
					.Where(n => startDate < n.EndDate)
					.Where(n => n.StartDate < endDate)
					.ToList(),
				MobileWorkItems = mobileWorkItems
					.Where(n => startDate < n.EndDate)
					.Where(n => n.StartDate < endDate)
					.ToList(),
				ComputerWorkTimeById = new Dictionary<int, TimeSpan>(),
				IvrWorkTimeById = new Dictionary<int, TimeSpan>(),
				MobileWorkTimeById = new Dictionary<int, TimeSpan>(),
				WorkIntervalsById = new Dictionary<int, List<IntervalConcatenator.Interval>>(),
			};
			var inverseQueryInterval = new IntervalConcatenator();
			inverseQueryInterval.Add(DateTime.MinValue, startDate);
			inverseQueryInterval.Add(endDate, DateTime.MaxValue);

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
				AddRangeDictValue(result.WorkIntervalsById, mobileWorkItem.WorkId, workIntervals.GetIntervals());
			}

			foreach (var workItem in result.AggregateWorkItemIntervals)
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
				AddRangeDictValue(result.WorkIntervalsById, workItem.WorkId, workIntervals.GetIntervals());
			}

			var sumIntervals = comIntervals.Clone();
			sumIntervals.Merge(mobIntervals);

			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddWork))
			{
				var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				result.ManuallyAddedWorkTime += startEndDate.Value.Duration();
				sumIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);
				AddRangeDictValue(result.WorkIntervalsById, manualWorkItem.WorkId ?? -1, new List<IntervalConcatenator.Interval> { new IntervalConcatenator.Interval(startEndDate.Value.StartDate, startEndDate.Value.EndDate) });
			}

			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddHoliday))
			{
				var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				result.HolidayTime += startEndDate.Value.Duration();
				AddRangeDictValue(result.WorkIntervalsById, manualWorkItem.WorkId ?? -1, new List<IntervalConcatenator.Interval> { new IntervalConcatenator.Interval(startEndDate.Value.StartDate, startEndDate.Value.EndDate) });
			}

			foreach (var manualWorkItem in result.ManualWorkItems.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddSickLeave))
			{
				var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				result.SickLeaveTime += startEndDate.Value.Duration();
				AddRangeDictValue(result.WorkIntervalsById, manualWorkItem.WorkId ?? -1, new List<IntervalConcatenator.Interval> { new IntervalConcatenator.Interval(startEndDate.Value.StartDate, startEndDate.Value.EndDate) });
			}

			sumIntervals.Subtract(inverseQueryInterval);
			comIntervals.Subtract(inverseQueryInterval);
			mobIntervals.Subtract(inverseQueryInterval);

			result.NetComputerWorkTime = comIntervals.Duration();
			result.NetMobileWorkTime = mobIntervals.Duration();

			result.SumWorkTime = sumIntervals.Duration() + result.HolidayTime + result.SickLeaveTime;

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

		private static void AddRangeDictValue(IDictionary<int, List<IntervalConcatenator.Interval>> dict, int key, List<IntervalConcatenator.Interval> values)
		{
			List<IntervalConcatenator.Interval> prevValue;
			if (dict.TryGetValue(key, out prevValue))
			{
				dict[key].AddRange(values);
			}
			else
			{
				dict.Add(key, values);
			}
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
