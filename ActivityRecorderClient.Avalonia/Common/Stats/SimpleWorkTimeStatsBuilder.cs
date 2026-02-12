using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Stats
{
	/// <summary>
	/// Class for calculating SimpleWorkTimeStats from local client data and aggregated daily worktimes from the server.
	/// </summary>
	public class SimpleWorkTimeStatsBuilder
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static string FilePath { get { return "SimpleBuilderState-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<SimpleWorkTimeStats>> SimpleWorkTimeStatsCalculated; //this type is for backward compatibility atm. and assumed to be 'immutable'

		private readonly SimpleWorkTimeStats currentStats = new SimpleWorkTimeStats()
		{
			UserId = ConfigManager.UserId,
			FromDate = new DateTime(1900, 01, 01),
			ToDate = DateTime.UtcNow,
			Stats = new Dictionary<int, SimpleWorkTimeStat>(),
		};

		private LocalWorkStats localWorkStats = new LocalWorkStats(); //dailyWorkTimeStats and localWorkStats should contain only disjoint intervals
		private Dictionary<DateTime, DailyWorkTimeStats> dailyWorkTimeStats = new Dictionary<DateTime, DailyWorkTimeStats>();

		internal SimpleWorkTimeStatsBuilderState GetState() //public would require DeepClone()
		{
			return new SimpleWorkTimeStatsBuilderState(localWorkStats, dailyWorkTimeStats);
		}

		internal void SetState(SimpleWorkTimeStatsBuilderState state) //public would require DeepClone()
		{
			localWorkStats = state.LocalWorkStats;
			UpdateDailyStats(state.DailyWorkTimeStats);
		}

		public void LoadData()
		{
			SimpleWorkTimeStatsBuilderState data;
			if (IsolatedStorageSerializationHelper.Exists(FilePath)
				&& IsolatedStorageSerializationHelper.Load(FilePath, out data))
			{
				SetState(data);
			}
			else
			{
				SetState(SimpleWorkTimeStatsBuilderState.Empty);
			}
		}

		public bool SaveData()
		{
			return IsolatedStorageSerializationHelper.Save(FilePath, GetState());
		}

		public void AddWorkInterval(WorkInterval workInterval)
		{
			if (!IsValid(workInterval)) return;
			AddToStats(localWorkStats.WorkIntervalsByTypeByWorkId.GetOrAdd(workInterval.WorkType, _ => new Dictionary<int, List<Interval>>()), workInterval);
		}

		public void DeleteWorkInterval(IntervalWithType interval)
		{
			if (!IsValid(interval)) return;
			DeleteFromStats(localWorkStats.WorkIntervalsByTypeByWorkId.GetOrAdd(interval.WorkType, _ => new Dictionary<int, List<Interval>>()), interval);
		}

		public void UpdateDailyStats(Dictionary<DateTime, DailyWorkTimeStats> stats)
		{
			var sw = Stopwatch.StartNew();
			dailyWorkTimeStats = stats; //assume this is ours noone will modify it

			var latestServerDay = dailyWorkTimeStats.Keys.Aggregate(new DateTime?(), (last, curr) => last.HasValue && last.Value > curr ? last : curr); //max
			var oldCutOff = latestServerDay.HasValue ? latestServerDay.Value.AddDays(-7) : new DateTime?(); //we don't expect any local data before this day

			//make dailyWorkTimeStats and localWorkStats disjoint
			var localDays = GetLocalDays();
			foreach (var localDay in localDays)
			{
				DailyWorkTimeStats dailyServerStat;
				if (dailyWorkTimeStats.TryGetValue(localDay, out dailyServerStat)) //accept server data whatever it says
				{
					DeleteFromAllStats(new Interval()
					{
						StartDate = localDay,
						EndDate = localDay + dailyServerStat.PartialInterval.GetValueOrDefault(TimeSpan.FromDays(1)),
					}, true);
				}
				else if (oldCutOff.HasValue && localDay < oldCutOff) //we have old local-only data
				{
					log.Warn("Deleting local worktime for day " + localDay.ToInvariantShortDateString() + " without server data. (latest server data is for day " + latestServerDay.ToInvariantShortDateString() + ")");
					//maybe we tried to upload old data to the server which was dropped, it is very unlikely that the server cound not make it valid until now
					DeleteFromAllStats(new Interval() { StartDate = localDay, EndDate = localDay + TimeSpan.FromDays(1), }, true);
				}
			}

			//remove old local partials with 0 worktime (which were colleted on days without worktime but in online status)
			if (oldCutOff.HasValue)
			{
				var oldKeys = dailyWorkTimeStats
					.Where(n => n.Value.NetWorkTime == TimeSpan.Zero
								&& (n.Value.TotalWorkTimeByWorkId == null || n.Value.TotalWorkTimeByWorkId.Count == 0)
								&& n.Value.Version == 0
								&& n.Value.Day < oldCutOff.Value)
					.Select(n => n.Key)
					.ToList();
				oldKeys.ForEach(n => dailyWorkTimeStats.Remove(n));
			}

			//RecalculateStats because we delayed it until now
			RecalculateStats();
			log.DebugFormat("UpdateDailyStats finished in {0:0.000}ms", sw.Elapsed.TotalMilliseconds);
		}

		private HashSet<DateTime> GetLocalDays()
		{
			var result = new HashSet<DateTime>();
			foreach (var localIntervalsByWorkId in localWorkStats.WorkIntervalsByTypeByWorkId.Values)
			{
				foreach (var intervals in localIntervalsByWorkId.Values)
				{
					foreach (var interval in intervals)
					{
						for (DateTime day = interval.StartDate.Date; day <= interval.EndDate.Date; day = day.AddDays(1))
						{
							result.Add(day);
						}
					}
				}
			}
			return result;
		}

		public void ChangeWorkId(int oldValue, int newValue)
		{
			var sw = Stopwatch.StartNew();
			foreach (var localIntervalsByWorkId in localWorkStats.WorkIntervalsByTypeByWorkId.Values)
			{
				List<Interval> intervals;
				if (localIntervalsByWorkId.TryGetValue(oldValue, out intervals))
				{
					localIntervalsByWorkId.Remove(oldValue);
					localIntervalsByWorkId.GetOrAdd(newValue, _ => new List<Interval>()).AddRange(intervals);
				}
			}
			RecalculateStats();
			log.VerboseFormat("ChangeWorkId finished in {0:0.000}ms", sw.Elapsed.TotalMilliseconds);
		}

		private void DeleteFromAllStats(Interval del, bool suppressRecalculate = false)
		{
			foreach (var localIntervalsByWorkId in localWorkStats.WorkIntervalsByTypeByWorkId.Values)
			{
				DeleteFromStats(localIntervalsByWorkId, del, true);
			}
			if (!suppressRecalculate) RecalculateStats();
		}

		private void DeleteFromStats(Dictionary<int, List<Interval>> localIntervalByWorkId, Interval del, bool suppressRecalculate = false)
		{
			//we can only delete from localWorkStats which is not ideal but KISS
			var keysToRemove = new List<int>();
			foreach (var kvp in localIntervalByWorkId)
			{
				var intervals = kvp.Value;
				for (int i = 0; i < intervals.Count; i++)
				{
					var curr = intervals[i];

					if (curr.StartDate < del.EndDate && curr.EndDate > del.StartDate) //we have an intersection
					{
						if (curr.StartDate < del.StartDate && curr.EndDate > del.EndDate) //delete from middle
						{
							intervals.Insert(i++, new Interval() { StartDate = del.EndDate, EndDate = curr.EndDate });
							curr.EndDate = del.StartDate;
						}
						else
						{
							if (del.StartDate <= curr.StartDate && curr.StartDate <= del.EndDate)
							{
								curr.StartDate = del.EndDate;
							}
							if (del.StartDate <= curr.EndDate && curr.EndDate <= del.EndDate)
							{
								curr.EndDate = del.StartDate;
							}
							if (curr.EndDate <= curr.StartDate)
							{
								intervals.RemoveAt(i--);
							}
						}
					}
				}
				if (intervals.Count == 0) keysToRemove.Add(kvp.Key);
			}
			keysToRemove.ForEach(n => localIntervalByWorkId.Remove(n));
			if (!suppressRecalculate) RecalculateStats();
		}

		private void AddToStats(Dictionary<int, List<Interval>> localIntervalByWorkId, WorkInterval workItem)
		{
			var workId = workItem.WorkId;

			var intervals = localIntervalByWorkId.GetOrAdd(workId, _ => new List<Interval>());
			if (intervals.Count > 0 && intervals[intervals.Count - 1].EndDate == workItem.StartDate) //optimize for common case (aggregate intervals)
			{
				intervals[intervals.Count - 1].EndDate = workItem.EndDate;
			}
			else
			{
				intervals.Add(new Interval() { StartDate = workItem.StartDate, EndDate = workItem.EndDate });
			}

			var stat = currentStats.Stats.GetOrAdd(workId, wId => new SimpleWorkTimeStat() { WorkId = wId });
			stat.TotalWorkTime += workItem.EndDate - workItem.StartDate;

			OnSimpleWorkTimeStatsCalculated();
		}

		private void RecalculateStats()
		{
			var sw = Stopwatch.StartNew();
			currentStats.Stats.Clear();
			foreach (var dailyWorkTimeStat in dailyWorkTimeStats.Values)
			{
				if (dailyWorkTimeStat.TotalWorkTimeByWorkId == null) continue;
				foreach (var kvp in dailyWorkTimeStat.TotalWorkTimeByWorkId)
				{
					var workId = kvp.Key;
					var stat = currentStats.Stats.GetOrAdd(workId, wId => new SimpleWorkTimeStat() { WorkId = wId });
					stat.TotalWorkTime += kvp.Value;
				}
			}

			foreach (var localIntervalsByWorkId in localWorkStats.WorkIntervalsByTypeByWorkId.Values)
			{
				RecalculateStatsForLocalDict(localIntervalsByWorkId);
			}

			log.VerboseFormat("RecalculateStats finished in {0:0.000}ms", sw.Elapsed.TotalMilliseconds);
			OnSimpleWorkTimeStatsCalculated();
		}

		private void RecalculateStatsForLocalDict(Dictionary<int, List<Interval>> localIntervalByWorkId)
		{
			foreach (var kvp in localIntervalByWorkId)
			{
				var workId = kvp.Key;
				var stat = currentStats.Stats.GetOrAdd(workId, wId => new SimpleWorkTimeStat() { WorkId = wId });
				foreach (var interval in kvp.Value)
				{
					stat.TotalWorkTime += interval.EndDate - interval.StartDate;
				}
			}
		}

		private static bool IsValid(Interval interval)
		{
			if (interval == null || interval.EndDate < interval.StartDate)
			{
				log.ErrorAndFail("Invalid interval");
				return false;
			}
			return true;
		}

		private void OnSimpleWorkTimeStatsCalculated()
		{
			currentStats.ToDate = DateTime.UtcNow;
			var del = SimpleWorkTimeStatsCalculated;
			if (del != null) del(this, SingleValueEventArgs.Create(currentStats.DeepClone())); //don't leak the original data as it will be modified
		}
	}
}
