using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class DetailedUserStats
	{
		private void AppendIntervalStats(DetailedIntervalStats stat, Dictionary<int, List<Interval>> result)
		{
			foreach (var s in stat.TodaysWorkIntervalsByWorkId)
			{
				if (!result.ContainsKey(s.Key))
				{
					result.Add(s.Key, new List<Interval>());
				}

				result[s.Key].AddRange(s.Value.Intervals);
			}
		}

		public IEnumerable<Interval> GetNonManualIntervals()
		{
			return ComputerStatsByCompId.SelectMany(x => x.Value.TodaysWorkIntervalsByWorkId.SelectMany(y => y.Value.Intervals))
				.Union(MobileStatsByMobileId.SelectMany(x => x.Value.TodaysWorkIntervalsByWorkId.SelectMany(y => y.Value.Intervals)))
				.Union(this.IvrStats.TodaysWorkIntervalsByWorkId.SelectMany(x => x.Value.Intervals));
		}

		public IEnumerable<KeyValuePair<Interval, int>> GetManualIntervalsWithId()
		{
			return ManuallyAddedStats.TodaysWorkIntervalsByWorkId.SelectMany(workId => workId.Value.Intervals,
				(workId, interval) => new KeyValuePair<Interval, int>(interval, workId.Key));
		}

		public IEnumerable<Interval> GetManualIntervals()
		{
			return ManuallyAddedStats.TodaysWorkIntervalsByWorkId.SelectMany(x => x.Value.Intervals);
		}

		public Dictionary<int, List<Interval>> GetIntervals()
		{
			var result = new Dictionary<int, List<Interval>>();
			if (ComputerStatsByCompId != null)
			{
				foreach (var computer in ComputerStatsByCompId)
				{
					AppendIntervalStats(computer.Value, result);
				}
			}

			if (MobileStatsByMobileId != null)
			{
				foreach (var mobile in MobileStatsByMobileId)
				{
					AppendIntervalStats(mobile.Value, result);
				}
			}

			if (HolidayStats != null)
			{
				AppendIntervalStats(HolidayStats, result);
			}

			if (ManuallyAddedStats != null)
			{
				AppendIntervalStats(ManuallyAddedStats, result);
			}

			if (IvrStats != null)
			{
				AppendIntervalStats(IvrStats, result);
			}

			return result;
		}
	}
}
