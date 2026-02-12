using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.OnlineStats
{
	/// <summary>
	/// Yet an other builder for stats (I know this is not ideal, but no idea how to make it simpler and fast enough)
	/// Note: This should perform faster than other builders that are calculating more data
	/// </summary>
	public static class SimpleWorkTimeStatsBuilder
	{
		public static SimpleWorkTimeStats GetSimpleWorkTime(int userId, DateTime startDate, DateTime endDate,
			IEnumerable<IComputerWorkItem> computerWorkItems,
			IEnumerable<IManualWorkItem> manualWorkItems,
			IEnumerable<IMobileWorkItem> mobileWorkItems
			)
		{
			var result = new SimpleWorkTimeStats
			{
				UserId = userId,
				FromDate = startDate,
				ToDate = endDate,
				Stats = new Dictionary<int, SimpleWorkTimeStat>(),
			};
			//prefilter
			manualWorkItems = manualWorkItems
				.Where(n => startDate < n.EndDate)
				.Where(n => n.StartDate < endDate);
			computerWorkItems = computerWorkItems
				.Where(n => startDate < n.EndDate)
				.Where(n => n.StartDate < endDate);
			mobileWorkItems = mobileWorkItems
				.Where(n => startDate < n.EndDate)
				.Where(n => n.StartDate < endDate);

			var inverseQueryInterval = new IntervalConcatenator();
			inverseQueryInterval.Add(DateTime.MinValue, startDate);
			inverseQueryInterval.Add(endDate, DateTime.MaxValue);

			//we don't handle deletion by WorkId atm.
			var comCorrIntervals = new IntervalConcatenator();
			var ivrCorrIntervals = new IntervalConcatenator();
			var mobCorrIntervals = new IntervalConcatenator();
			foreach (var manualWorkItem in manualWorkItems)
			{
				switch (manualWorkItem.ManualWorkItemTypeId)
				{
					case ManualWorkItemTypeEnum.AddWork:
						{
							var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
							if (!startEndDate.HasValue) continue;
							IncreaseDictValue(result.Stats, manualWorkItem.WorkId ?? -1, startEndDate.Value.Duration());
						}
						break;
					case ManualWorkItemTypeEnum.DeleteInterval:
						comCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
						ivrCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
						mobCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
						break;
					case ManualWorkItemTypeEnum.DeleteIvrInterval:
						ivrCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
						break;
					case ManualWorkItemTypeEnum.DeleteComputerInterval:
						comCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
						break;
					case ManualWorkItemTypeEnum.AddHoliday:
						{
							var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
							if (!startEndDate.HasValue) continue;
							IncreaseDictValue(result.Stats, manualWorkItem.WorkId ?? -1, startEndDate.Value.Duration());
						}
						break;
					case ManualWorkItemTypeEnum.AddSickLeave:
						{
							var startEndDate = GetIntersectStartEndDateTime(manualWorkItem.StartDate, manualWorkItem.EndDate, startDate, endDate);
							if (!startEndDate.HasValue) continue;
							IncreaseDictValue(result.Stats, manualWorkItem.WorkId ?? -1, startEndDate.Value.Duration());
						}
						break;
					case ManualWorkItemTypeEnum.DeleteMobileInterval:
						mobCorrIntervals.Add(manualWorkItem.StartDate, manualWorkItem.EndDate);
						break;
					default:
						Debug.Fail("Invalid ManualWorkItemTypeId");
						//logerror?
						break;
				}
			}

			//we don't care about dates outside of the query interval
			comCorrIntervals.Subtract(inverseQueryInterval);
			ivrCorrIntervals.Subtract(inverseQueryInterval);
			mobCorrIntervals.Subtract(inverseQueryInterval);

			var workIntervals = new IntervalConcatenator(); //reduce memory pressure (reuse obj)
			foreach (var mobileWorkItem in mobileWorkItems)
			{
				var startEndDate = GetIntersectStartEndDateTime(mobileWorkItem.StartDate, mobileWorkItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				workIntervals.Clear();
				workIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);
				workIntervals.Subtract(mobCorrIntervals);
				var correctedDuration = workIntervals.Duration();
				IncreaseDictValue(result.Stats, mobileWorkItem.WorkId, correctedDuration);
			}

			foreach (var workItem in computerWorkItems)
			{
				var startEndDate = GetIntersectStartEndDateTime(workItem.StartDate, workItem.EndDate, startDate, endDate);
				if (!startEndDate.HasValue) continue;
				workIntervals.Clear();
				workIntervals.Add(startEndDate.Value.StartDate, startEndDate.Value.EndDate);
				workIntervals.Subtract(comCorrIntervals);
				var correctedDuration = workIntervals.Duration();
				IncreaseDictValue(result.Stats, workItem.WorkId, correctedDuration);
			}

			return result;
		}

		private static void IncreaseDictValue(IDictionary<int, SimpleWorkTimeStat> dict, int workId, TimeSpan value)
		{
			if (value == TimeSpan.Zero) return; //don't create Zero length worktimes
			SimpleWorkTimeStat stat;
			if (dict.TryGetValue(workId, out stat))
			{
				stat.TotalWorkTime += value;
			}
			else
			{
				dict.Add(workId, new SimpleWorkTimeStat() { WorkId = workId, TotalWorkTime = value });
			}
		}

		private static StartEndDateTime? GetIntersectStartEndDateTime(DateTime firstStart, DateTime firstEnd, DateTime secondStart, DateTime secondEnd)
		{
			if (firstEnd < firstStart || secondEnd < secondStart) return null; //invalid intervals
			var result = new StartEndDateTime(
				firstStart < secondStart ? secondStart : firstStart, //MAX(firstStart, secondStart)
				secondEnd < firstEnd ? secondEnd : firstEnd);        //MIN(secondEnd, firstEnd)
			return result.EndDate < result.StartDate ? new StartEndDateTime?() : result;
		}
	}
}
