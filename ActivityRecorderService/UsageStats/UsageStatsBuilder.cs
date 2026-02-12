using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.OnlineStats;

namespace Tct.ActivityRecorderService.UsageStats
{
	/// <summary>
	/// Class for calculating daily usage stats for a given user (for e-comm)
	/// </summary>
	public class UsageStatsBuilder
	{
		//we don't allow overlaps between UsageStat intervals because that would complicate things and we would charge twice for a given time
		//we allow gaps however because we don't want to charge for a local day twice (LocalDate is unique)

		private readonly Dictionary<DateTime, UsageStat> usageStatsByLocalDate = new Dictionary<DateTime, UsageStat>();
		private readonly List<UsageStat> usageStats = new List<UsageStat>();
		private static readonly UsageStatDateComparer dateComparer = new UsageStatDateComparer();
		private readonly UserStatInfo userStatInfo;

		public UsageStatsBuilder(IEnumerable<UsageStat> previousUsages, UserStatInfo userStatInfo)
		{
			this.userStatInfo = userStatInfo;
			foreach (var previousUsage in previousUsages)
			{
				usageStats.Add(previousUsage);
				usageStatsByLocalDate.Add(previousUsage.LocalDate, previousUsage); //LocalDate is unique !
			}
			usageStats.Sort(dateComparer);
		}

		public IEnumerable<UsageStat> GetUsageStats()
		{
			foreach (var usageStat in usageStats)
			{
				yield return usageStat;
			}
		}

		public void AddAggregateWorkItemIntervals(IEnumerable<IComputerWorkItem> items)
		{
			foreach (var item in items)
			{
				AddAggregateWorkItemInterval(item);
			}
		}

		public void AddAggregateWorkItemInterval(IComputerWorkItem item)
		{
			AddInterval(new StartEndDateTime(item.StartDate, item.EndDate), (usageStat, time) => usageStat.ComputerWorkTime += time);
		}

		public void AddMobileWorkItems(IEnumerable<IMobileWorkItem> items)
		{
			foreach (var item in items)
			{
				AddMobileWorkItem(item);
			}
		}

		public void AddMobileWorkItem(IMobileWorkItem item)
		{
			var isBeacon = item.Imei == userStatInfo.Id;
			AddInterval(new StartEndDateTime(item.StartDate, item.EndDate), (usageStat, time) =>
			{
				usageStat.MobileWorkTime += time;
				usageStat.UsedBeaconClient |= isBeacon;
				usageStat.UsedMobile |= !isBeacon;
			});
		}

		public void AddManualWorkItems(IEnumerable<IManualWorkItem> items)
		{
			foreach (var item in items)
			{
				AddManualWorkItem(item);
			}
		}

		public void AddManualWorkItem(IManualWorkItem item)
		{
			if (item.ManualWorkItemTypeId != ManualWorkItemTypeEnum.AddWork) return;
			AddInterval(new StartEndDateTime(item.StartDate, item.EndDate), (usageStat, time) => usageStat.ManuallyAddedWorkTime += time);
		}

		internal void AddVoiceRecordings(IEnumerable<Voice.VoiceRecording> voxIntervals)
		{
			foreach (var item in voxIntervals)
			{
				AddInterval(new StartEndDateTime(item.StartDate, item.StartDate.AddMilliseconds(item.Duration)), (usageStat, time) => usageStat.UsedVoxCtrl = true);
			}
		}

		private readonly UsageStat dummySearch = new UsageStat(); //reduce GC pressure
		private void AddInterval(StartEndDateTime item, Action<UsageStat, TimeSpan> updateAction)
		{
			if (item.EndDate < item.StartDate) return;
			//we need the find the first UsageStat 'bucket' which can hold the StartDate of this item
			//we will create a dummy entry so we can search for the right bucket
			dummySearch.StartDate = item.StartDate;
			var idx = usageStats.BinarySearch(dummySearch, dateComparer);
			UsageStat bucketToHoldStartDate;
			if (idx >= 0) //found a bucket with same StartDate
			{
				bucketToHoldStartDate = usageStats[idx];
			}
			else //find or create a bucket that can hold the StartDate of the item
			{
				int newIdx = ~idx;

				if (newIdx > 0 && usageStats[newIdx - 1].EndDate > item.StartDate) //found a bucket
				{
					Debug.Assert(usageStats[newIdx - 1].StartDate < item.StartDate);
					bucketToHoldStartDate = usageStats[newIdx - 1];
				}
				else //try to create a bucket because there is no bucket which can hold our StartDate
				{
					//insert new bucket which can hold the StartDate of the item
					//we can only insert if we have no bucket for this LocalDate otherwise we have to drop some part of the worktime
					var localDate = CalculatorHelper.GetLocalReportDate(item.StartDate, userStatInfo.TimeZone, userStatInfo.StartOfDayOffset);
					if (!usageStatsByLocalDate.ContainsKey(localDate)) //if we have no stats for this day (we can create a bucket that can hold our StartDate)
					{
						var startEndDate = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo);
						//we can have no overlaps so calculate min/max dates
						var minStartDate = newIdx > 0 ? usageStats[newIdx - 1].EndDate : DateTime.MinValue;
						var maxEndDate = newIdx < usageStats.Count ? usageStats[newIdx].StartDate : DateTime.MaxValue;
						startEndDate = new StartEndDateTime(
							minStartDate > startEndDate.StartDate ? minStartDate : startEndDate.StartDate,
							maxEndDate < startEndDate.EndDate ? maxEndDate : startEndDate.EndDate);
						Debug.Assert(startEndDate.Duration() > TimeSpan.Zero);
						bucketToHoldStartDate = new UsageStat()
						{
							UserId = userStatInfo.Id,
							LocalDate = localDate,
							StartDate = startEndDate.StartDate,
							EndDate = startEndDate.EndDate
						};
						usageStats.Insert(newIdx, bucketToHoldStartDate);
						usageStatsByLocalDate.Add(localDate, bucketToHoldStartDate);
					}
					else //if we have stats for this day (and StartDate is not in any existing interval)
					{
						//caclulate the first interval that can hold some (early) part of our item
						var nextDayStartDate = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo).StartDate;
						var nextBucketStartDate = newIdx < usageStats.Count ? usageStats[newIdx].StartDate : DateTime.MaxValue;
						var earliestStartDate = nextBucketStartDate < nextDayStartDate ? nextBucketStartDate : nextDayStartDate;
						if (earliestStartDate >= item.EndDate) return; //cannot store this item in any bucket
						AddInterval(new StartEndDateTime(earliestStartDate, item.EndDate), updateAction); //todo avoid SO ex
						return;
					}
				}
			}

			Debug.Assert(bucketToHoldStartDate != null);
			Debug.Assert(bucketToHoldStartDate.StartDate <= item.StartDate);
			Debug.Assert(bucketToHoldStartDate.EndDate > item.StartDate);
			//we have a bucket that will contain the StartDate of the item
			if (bucketToHoldStartDate.EndDate >= item.EndDate) //whole item will fit into the bucket
			{
				updateAction(bucketToHoldStartDate, item.Duration());
			}
			else
			{
				updateAction(bucketToHoldStartDate, bucketToHoldStartDate.EndDate - item.StartDate);
				//add the remaining part
				AddInterval(new StartEndDateTime(bucketToHoldStartDate.EndDate, item.EndDate), updateAction);
			}
		}

		private class UsageStatDateComparer : IComparer<UsageStat>
		{
			public int Compare(UsageStat x, UsageStat y)
			{
				Debug.Assert(x != null);
				Debug.Assert(y != null);
				if (x == null)
				{
					return y == null ? 0 : -1;
				}
				if (y == null) return 1;

				return DateTime.Compare(x.StartDate, y.StartDate);
			}
		}
	}
}
