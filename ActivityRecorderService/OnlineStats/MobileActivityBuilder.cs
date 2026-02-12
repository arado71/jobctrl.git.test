using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.OnlineStats
{
	public class MobileActivityBuilder
	{
		private static readonly TimeSpan maxInverval = TimeSpan.FromMinutes(30);
		private readonly Dictionary<DateTime, int> mobileActivities = new Dictionary<DateTime, int>();

		public bool HasActivity { get { return mobileActivities.Count != 0; } }

		public List<int> GetMinutelyAggregatedActivityReversed(DateTime startDate, DateTime endDate)
		{
			if (!HasActivity) return null;
			var result = new List<int>();
			var startMin = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, 00);
			var endMin = new DateTime(endDate.Year, endDate.Month, endDate.Day, endDate.Hour, endDate.Minute, 00);
			//for (var currDate = startMin; currDate < endDate; currDate = currDate.AddMinutes(1))
			for (var currDate = endMin; currDate >= startMin; currDate = currDate.AddMinutes(-1))
			{
				result.Add(mobileActivities.GetValueOrDefault(currDate));
			}
			return result;
		}

		public void RefreshMobileActivityInfo(IEnumerable<MobileActivityInfo> activities)
		{
			mobileActivities.Clear();
			foreach (var act in activities.OrderByDescending(n => n.EndDate).TakeWhile(n => n.EndDate > DateTime.UtcNow - maxInverval))
			{
				var remainingActicity = act.Activity;
				var wholeDuration = act.EndDate - act.StartDate;
				var startMin = new DateTime(act.StartDate.Year, act.StartDate.Month, act.StartDate.Day, act.StartDate.Hour, act.StartDate.Minute, 0);
				var endMin = new DateTime(act.EndDate.Year, act.EndDate.Month, act.EndDate.Day, act.EndDate.Hour, act.EndDate.Minute, 0);
				var currStartDate = act.StartDate;

				while (startMin <= endMin)
				{
					int originalActivity;
					if (!mobileActivities.TryGetValue(startMin, out originalActivity)) //get the current AggregatedActivity for this minute
					{
						originalActivity = 0;
					}
					if (startMin == endMin) //last interval
					{
						mobileActivities[startMin] = originalActivity + remainingActicity;
						break;
					}

					var currEndDate = startMin.AddMinutes(1);
					if (currEndDate > act.EndDate) currEndDate = act.EndDate;
					var duration = currEndDate - currStartDate;
					var currentActivity = (int)(act.Activity * duration.TotalMilliseconds / wholeDuration.TotalMilliseconds);
					mobileActivities[startMin] = originalActivity + currentActivity;
					remainingActicity -= currentActivity;

					startMin = startMin.AddMinutes(1);
					currStartDate = startMin;
				}
			}
		}

	}
}
