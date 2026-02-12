using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.OnlineStats;

namespace Tct.ActivityRecorderService
{
	/// <summary>
	/// Class for calculating gross intervals.
	/// One can group intervals by a key 1. to make it faster, 2. to add additional info for a group of intervals
	/// </summary>
	public class IntervalAggregator<TKey>
	{
		private Dictionary<TKey, List<StartEndDateTime>> intervalsDict = new Dictionary<TKey, List<StartEndDateTime>>();

		public void Add(TKey key, DateTime startDate, DateTime endDate)
		{
			if (endDate < startDate)
			{
				Debug.Fail("Invalid interval");
				return;
			}
			if (startDate == endDate) return;
			List<StartEndDateTime> intervalsForKey;
			if (!intervalsDict.TryGetValue(key, out intervalsForKey))
			{
				intervalsForKey = new List<StartEndDateTime>();
				intervalsDict.Add(key, intervalsForKey);
			}
			for (int i = 0; i < intervalsForKey.Count; i++) //probably we won't need to optimize this... (time will tell)
			{
				if (intervalsForKey[i].EndDate == startDate) //found a matching interval
				{
					intervalsForKey[i] = new StartEndDateTime(intervalsForKey[i].StartDate, endDate);
					MergeStartDateForInterval(intervalsForKey, i);
					return;
				}
			}
			//no matching intervals found
			intervalsForKey.Add(new StartEndDateTime(startDate, endDate));
			MergeStartDateForInterval(intervalsForKey, intervalsForKey.Count - 1);
		}

		public void Refresh(List<Tuple<TKey, StartEndDateTime>> aggrCorItems)
		{
			if (aggrCorItems == null || aggrCorItems.Count == 0) return;
			var end = aggrCorItems.Max(a => a.Item2.EndDate);
			var itemsAfter = intervalsDict.SelectMany(i => i.Value.Where(s => s.EndDate > end).Select(s => new Tuple<TKey, StartEndDateTime>(i.Key, s.StartDate < end ? new StartEndDateTime(end, s.EndDate) : s))).ToList();
			intervalsDict = new Dictionary<TKey, List<StartEndDateTime>>();
			foreach (var item in aggrCorItems.Concat(itemsAfter))
			{
				Add(item.Item1, item.Item2.StartDate, item.Item2.EndDate);
			}
		}

		//intervalsToMerge should be quite small so not sure if we want to sort it...
		private static void MergeStartDateForInterval(List<StartEndDateTime> intervalsForKey, int modifiedIdx)
		{
			if (intervalsForKey.Count < 2) return; //fast path
			var startDate = intervalsForKey[modifiedIdx].StartDate;
			var endDate = intervalsForKey[modifiedIdx].EndDate;
			for (int i = 0; i < intervalsForKey.Count; i++)
			{
				if (i == modifiedIdx) continue;
				if (intervalsForKey[i].StartDate == endDate) //found a matching interval
				{
					intervalsForKey[i] = new StartEndDateTime(startDate, intervalsForKey[i].EndDate);
					intervalsForKey.RemoveAt(modifiedIdx);
					return;
				}
			}
		}

		public IEnumerable<KeyValuePair<TKey, List<StartEndDateTime>>> GetIntervalsWithKeys()
		{
			foreach (var keyValue in intervalsDict)
			{
				yield return keyValue;
			}
		}

		/// <summary>
		/// In order to avoid memory leak we have to drop old intervals
		/// </summary>
		/// <param name="cutoffDate">Cutoff date</param>
		public void TruncateIntervalsBefore(DateTime cutoffDate)
		{
			List<TKey> keysToRemove = new List<TKey>();
			foreach (var keyValue in intervalsDict)
			{
				var intervalsForKey = keyValue.Value;
				for (int i = 0; i < intervalsForKey.Count; i++)
				{
					if (intervalsForKey[i].EndDate < cutoffDate) //remove
					{
						intervalsForKey.RemoveAt(i--);
					}
					else if (intervalsForKey[i].StartDate < cutoffDate) //trucate
					{
						intervalsForKey[i] = new StartEndDateTime(cutoffDate, intervalsForKey[i].EndDate);
					}
				}
				if (intervalsForKey.Count == 0) //all removed
				{
					keysToRemove.Add(keyValue.Key);
				}
			}
			foreach (var key in keysToRemove)
			{
				intervalsDict.Remove(key);
			}

			if (keysToRemove.Count > 0)
				intervalsDict = intervalsDict.ToDictionary(d => d.Key, d => d.Value); // recreate dictionary to reduce waste space
		}
	}
}
