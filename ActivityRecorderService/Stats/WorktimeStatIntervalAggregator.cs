using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.WebsiteServiceReference;

namespace Tct.ActivityRecorderService.Stats
{
	public class WorktimeStatIntervalAggregator
	{
		private readonly Dictionary<int, List<IntervalsByComputer>> intervalStore = new Dictionary<int, List<IntervalsByComputer>>();

		public WorktimeStatIntervals Add(int userId, int computerId, WorktimeStatIntervals intervals, DateTime now)
		{
			lock (intervalStore)
			{
				var newItem = new IntervalsByComputer { ComputerId = computerId, Intervals = intervals, LastReceived = now };
				if (!intervalStore.TryGetValue(userId, out var items))
				{
					intervalStore.Add(userId, items = new List<IntervalsByComputer> { newItem });
					return intervals;
				}

				var found = items.FirstOrDefault(i => i.ComputerId == computerId);
				if (found != null)
				{
					found.Intervals = intervals;
					found.LastReceived = now;
				}
				else
				{
					items.Add(newItem);
				}

				var runouts = items.OrderByDescending(i => i.LastReceived).Skip(1).Where(i => i.LastReceived < now.AddDays(-1)).Select(i => i.ComputerId).ToList();
				items.RemoveAll(i => runouts.Contains(i.ComputerId));
				var next = items.Aggregate<IntervalsByComputer, WorktimeStatIntervals>(0, (current, item) => current | item.Intervals);
				return next;
			}
		}

		public WorktimeStatIntervals? Get(int userId, DateTime now)
		{
			lock (intervalStore)
			{
				if (!intervalStore.TryGetValue(userId, out var items)) return null;
				var runouts = items.OrderByDescending(i => i.LastReceived).Skip(1).Where(i => i.LastReceived < now.AddDays(-1)).Select(i => i.ComputerId).ToList();
				items.RemoveAll(i => runouts.Contains(i.ComputerId));

				return items.Aggregate<IntervalsByComputer, WorktimeStatIntervals>(0, (current, item) => current | item.Intervals);
			}
		}


		private class IntervalsByComputer
		{
			public int ComputerId { get; set; }
			public WorktimeStatIntervals Intervals { get; set; }
			public DateTime LastReceived { get; set; }
		}
	}
}
