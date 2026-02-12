using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu
{
	public static class MenuBuilderHelper
	{
		public static float? GetPercentage(WorkData workData, SimpleWorkTimeStat totalStat)
		{
			if (workData == null) return null;

			float? endDatePct = GetTargetEndDatePct(workData);
			float? totalTimePct = GetTargetWorkTimePct(workData, totalStat);

			if (!endDatePct.HasValue) return totalTimePct;
			if (!totalTimePct.HasValue) return endDatePct;
			return Math.Max(endDatePct.Value, totalTimePct.Value);
		}

		private const float maxPct = 9.999f;
		public static float? GetTargetEndDatePct(WorkData workData)
		{
			if (!HasTargetEndDate(workData)) return null;
			var now = DateTime.Now; //StartDate and EndDate are local times
			var startDate = workData.StartDate.Value.Date;
			var endDate = workData.EndDate.Value.Date.AddDays(1); //(StartDate, EndDate) is a date range including EndDate
			if (endDate <= startDate) return maxPct; //don't divide by zero
			var result =
				(now < startDate)
					? 0f
					: (float)((now - startDate).TotalMilliseconds / (endDate - startDate).TotalMilliseconds);
			return Clamp(result, 0, maxPct);
		}

		public static float? GetTargetWorkTimePct(WorkData workData, SimpleWorkTimeStats totalStats)
		{
			if (workData == null) return null;
			if (!workData.Id.HasValue) return null;
			return GetTargetWorkTimePct(workData, GetWorkStatForId(totalStats, workData.Id.Value));
		}

		private static float? GetTargetWorkTimePct(WorkData workData, SimpleWorkTimeStat totalStat)
		{
			if (workData == null) return null;
			Debug.Assert(totalStat == null || !workData.Id.HasValue || workData.Id.Value == totalStat.WorkId);
			if (!workData.TargetTotalWorkTime.HasValue || totalStat == null || !workData.Id.HasValue || workData.Id.Value != totalStat.WorkId) return null;
			if (workData.TargetTotalWorkTime.Value <= TimeSpan.Zero) return maxPct; //don't divide by zero, also negative number is error
			var result = (float)(totalStat.TotalWorkTime.TotalMilliseconds / workData.TargetTotalWorkTime.Value.TotalMilliseconds);
			return Clamp(result, 0, maxPct);
		}

		private static float Clamp(float value, float minValue, float maxValue)
		{
			return value < minValue
				? minValue
				: value > maxValue
					? maxValue
					: value;
		}

		public static bool HasTargetEndDate(WorkData workData)
		{
			if (workData == null) return false;
			if (!workData.StartDate.HasValue || !workData.EndDate.HasValue) return false;
			return true;
		}

		public static bool HasTargetTotalWorkTime(WorkData workData)
		{
			if (workData == null) return false;
			if (!workData.TargetTotalWorkTime.HasValue) return false;
			return true;
		}

		public static bool HasPriority(WorkData workData)
		{
			if (workData == null) return false;
			if (!workData.Priority.HasValue) return false;
			if (workData.Priority.Value < 0) return false;
			return true;
		}

		public static SimpleWorkTimeStat GetWorkStatForId(SimpleWorkTimeStats totalStats, int workId)
		{
			if (totalStats == null) return null;
			SimpleWorkTimeStat result;
			return totalStats.Stats.TryGetValue(workId, out result) ? result : new SimpleWorkTimeStat() { WorkId = workId };
		}

		private static int MenuTopItemsCount
		{
			get { return Math.Max(1, ConfigManager.LocalSettingsForUser.MenuTopItemsCount); }
		}

		public static IEnumerable<WorkDataWithParentNames> GetTopPriorityWorkData(ClientMenu menu)
		{
			var flatWorkData = MenuHelper.FlattenDistinctWorkDataThatHasId(menu);
			var topPriData = flatWorkData
				.Where(n => MenuBuilderHelper.HasPriority(n.WorkData) && n.WorkData.IsVisibleInMenu)
				.OrderByDescending(n => n.WorkData.Priority.Value)
				.Take(MenuTopItemsCount);
			return topPriData;
		}

		public static IEnumerable<WorkDataWithParentNames> GetTopTargetEndDateWorkData(ClientMenu menu)
		{
			var flatWorkData = MenuHelper.FlattenDistinctWorkDataThatHasId(menu);
			var topEndData = flatWorkData
				.Where(n => MenuBuilderHelper.HasTargetEndDate(n.WorkData) && n.WorkData.IsVisibleInMenu)
				.OrderByDescending(n => MenuBuilderHelper.GetTargetEndDatePct(n.WorkData) ?? -1)
				.Take(MenuTopItemsCount);
			return topEndData;
		}

		public static IEnumerable<WorkDataWithParentNames> GetTopTargetWorkTimeWorkData(ClientMenu menu, SimpleWorkTimeStats totalStats)
		{
			var flatWorkData = MenuHelper.FlattenDistinctWorkDataThatHasId(menu);
			var topWorkTimeData = flatWorkData
				.Where(n => MenuBuilderHelper.HasTargetTotalWorkTime(n.WorkData) && n.WorkData.IsVisibleInMenu)
				.OrderByDescending(n => MenuBuilderHelper.GetTargetWorkTimePct(n.WorkData, totalStats) ?? -1)
				.Take(MenuTopItemsCount);
			return topWorkTimeData;
		}
	}
}
