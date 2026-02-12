using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	/// <summary>
	/// Creates Minutely Aggregated Keyboard and Mouse Activities
	/// </summary>
	public class AggregateActivityBuilder
	{
		private readonly Dictionary<DateTime, AggregatedActivity> aggregatedActivities = new Dictionary<DateTime, AggregatedActivity>();
		public StartEndDateTime? AggregatedInterval { get; private set; }

		public void AddWorkItem(WorkItem workItem)
		{
			AddWorkItems(new[] { workItem });
		}

		public void AddWorkItems(IEnumerable<WorkItem> workItems)
		{
			if (workItems == null) return;
			var minStartMin = DateTime.MaxValue;
			var maxStartMin = DateTime.MinValue;
			var count = 0;
			foreach (var workItem in workItems)
			{
				count++;
				var startMin = new DateTime(workItem.StartDate.Year, workItem.StartDate.Month, workItem.StartDate.Day, workItem.StartDate.Hour, workItem.StartDate.Minute, 00);
				var endMin = new DateTime(workItem.EndDate.Year, workItem.EndDate.Month, workItem.EndDate.Day, workItem.EndDate.Hour, workItem.EndDate.Minute, 00);
				minStartMin = startMin < minStartMin ? startMin : minStartMin;
				maxStartMin = startMin > maxStartMin ? startMin : maxStartMin;
				int remainingKeyboardActivity = workItem.KeyboardActivity;
				int remainingMouseActivity = workItem.MouseActivity;
				var currStartDate = workItem.StartDate;
				while (startMin <= endMin)
				{
					AggregatedActivity aggr;
					if (!aggregatedActivities.TryGetValue(startMin, out aggr)) //get the current AggregatedActivity for this minute
					{
						aggr = new AggregatedActivity() { StartDate = startMin, EndDate = startMin.AddMinutes(1) };
						aggregatedActivities.Add(startMin, aggr);
					}
					if (startMin == endMin) //last interval
					{
						aggr.KeyboardActivity += remainingKeyboardActivity;
						aggr.MouseActivity += remainingMouseActivity;
						break;
					}
					var currEndDate = startMin.AddMinutes(1);
					var duration = currEndDate - currStartDate;
					var wholeDuration = workItem.EndDate - workItem.StartDate;
					var currKeyboardActivity = (int)(workItem.KeyboardActivity * duration.TotalMilliseconds / wholeDuration.TotalMilliseconds);
					var currMouseActivity = (int)(workItem.MouseActivity * duration.TotalMilliseconds / wholeDuration.TotalMilliseconds);
					aggr.KeyboardActivity += currKeyboardActivity;
					aggr.MouseActivity += currMouseActivity;
					remainingKeyboardActivity -= currKeyboardActivity;
					remainingMouseActivity -= currMouseActivity;

					startMin = startMin.AddMinutes(1);
					currStartDate = startMin;
				}
			}
			if (count == 0) return;
			var maxEndMin = maxStartMin.AddMinutes(1);
			if (!AggregatedInterval.HasValue)
			{
				AggregatedInterval = new StartEndDateTime(minStartMin, maxEndMin);
			}
			else
			{
				AggregatedInterval = new StartEndDateTime(
					AggregatedInterval.Value.StartDate < minStartMin ? AggregatedInterval.Value.StartDate : minStartMin,
					AggregatedInterval.Value.EndDate > maxEndMin ? AggregatedInterval.Value.EndDate : maxEndMin);
			}
		}

		public List<AggregatedActivity> GetMinutelyAggregatedActivity(DateTime startDate, DateTime endDate)
		{
			var result = new List<AggregatedActivity>();
			if (!AggregatedInterval.HasValue) return result;
			var startMin = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, 00);
			for (var currDate = startMin; currDate < endDate; currDate = currDate.AddMinutes(1))
			{
				AggregatedActivity aggr;
				if (!aggregatedActivities.TryGetValue(currDate, out aggr))
				{
					aggr = new AggregatedActivity() { StartDate = currDate, EndDate = currDate.AddMinutes(1) };
				}
				result.Add(aggr);
			}
			return result;
		}

		public void ClearDataBefore(DateTime startDate)
		{
			var keysToRemove = aggregatedActivities.Keys.Where(n => n < startDate).ToList();
			foreach (var time in keysToRemove)
			{
				aggregatedActivities.Remove(time);
			}
		}
	}
}
