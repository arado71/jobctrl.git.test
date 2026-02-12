using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MonitoringClient.ActivityMonitoringServiceReference
{
	public partial class DetailedUserStats
	{
		private Dictionary<string, Dictionary<int, WorkWithIntervals>> intervalStatsDict;
		public Dictionary<string, Dictionary<int, WorkWithIntervals>> TodaysIntervalStatsDict
		{
			get
			{
				if (intervalStatsDict != null) return intervalStatsDict;
				intervalStatsDict = new Dictionary<string, Dictionary<int, WorkWithIntervals>>();
				if (ComputerStatsByCompId != null)
				{
					foreach (var detailedComputerStats in ComputerStatsByCompId.Values)
					{
						if (HasTodaysStats(detailedComputerStats))
						{
							intervalStatsDict.Add("Comp " + detailedComputerStats.ComputerId, detailedComputerStats.TodaysWorkIntervalsByWorkId);
						}
					}
				}
				if (MobileStatsByMobileId != null)
				{
					foreach (var detailedMobileStats in MobileStatsByMobileId.Values)
					{
						if (HasTodaysStats(detailedMobileStats))
						{
							intervalStatsDict.Add("Mobile " + detailedMobileStats.MobileId, detailedMobileStats.TodaysWorkIntervalsByWorkId);
						}
					}
				}
				if (HasTodaysStats(ManuallyAddedStats))
				{
					intervalStatsDict.Add("Manual", ManuallyAddedStats.TodaysWorkIntervalsByWorkId);
				}
				if (HasTodaysStats(IvrStats))
				{
					intervalStatsDict.Add("Ivr", IvrStats.TodaysWorkIntervalsByWorkId);
				}
				if (HasTodaysStats(HolidayStats))
				{
					intervalStatsDict.Add("Holidays", HolidayStats.TodaysWorkIntervalsByWorkId);
				}
				if (HasTodaysStats(SickLeaveStats))
				{
					intervalStatsDict.Add("Sick Leaves", SickLeaveStats.TodaysWorkIntervalsByWorkId);
				}
				return intervalStatsDict;
			}
		}

		private string allActiveWindowsString;
		public string AllActiveWindowsString
		{
			get
			{
				if (allActiveWindowsString != null) return allActiveWindowsString;
				if (ComputerStatsByCompId != null && ComputerStatsByCompId.Count != 0)
				{
					allActiveWindowsString = string.Join(Environment.NewLine, ComputerStatsByCompId.Values
						.Where(n => n.RecentComputerActivity != null
							&& n.RecentComputerActivity.LastActiveWindow != null)
						.Select(n => "[" + n.RecentComputerActivity.LastActiveWindow.ProcessName + "]" + Environment.NewLine
							+ n.RecentComputerActivity.LastActiveWindow.Title));
				}
				return string.IsNullOrEmpty(allActiveWindowsString) ? "Waiting for data..." : allActiveWindowsString;
			}
		}

		private List<Point> aggregateKeyboardActivity;
		public List<Point> AggregateKeyboardActivity
		{
			get
			{
				if (aggregateKeyboardActivity != null) return aggregateKeyboardActivity;
				if (ComputerStatsByCompId != null
					&& ComputerStatsByCompId.Count != 0)
				{
					var recents = ComputerStatsByCompId.Values
						.Where(n => n.RecentComputerActivity != null)
						.Where(n => n.RecentComputerActivity.RecentKeyboardActivityPerMinute != null)
						.Select(n => new List<int>(n.RecentComputerActivity.RecentKeyboardActivityPerMinute));
					//handle more computers sum activity
					var aggr = recents.Aggregate((tot, n) => tot.Zip(n, (i1, i2) => i1 + i2).ToList());

					aggregateKeyboardActivity = aggr
						.Select((n, idx) => new Point(-idx, n))
						.ToList();
				}
				return aggregateKeyboardActivity;
			}
		}

		private List<Point> aggregateMouseActivity;
		public List<Point> AggregateMouseActivity
		{
			get
			{
				if (aggregateMouseActivity != null) return aggregateMouseActivity;
				if (ComputerStatsByCompId != null
					&& ComputerStatsByCompId.Count != 0)
				{
					var recents = ComputerStatsByCompId.Values
						.Where(n => n.RecentComputerActivity != null)
						.Where(n => n.RecentComputerActivity.RecentMouseActivityPerMinute != null)
						.Select(n => new List<int>(n.RecentComputerActivity.RecentMouseActivityPerMinute));
					//handle more computers sum activity
					var aggr = recents.Aggregate((tot, n) => tot.Zip(n, (i1, i2) => i1 + i2).ToList());

					aggregateMouseActivity = aggr
						.Select((n, idx) => new Point(-idx, n / 50))
						.ToList();
				}
				return aggregateMouseActivity;
			}
		}

		private bool showActivity;
		public bool ShowActivity
		{
			get
			{
				return this.showActivity;
			}
			set
			{
				if ((this.showActivity.Equals(value) != true))
				{
					this.showActivity = value;
					this.RaisePropertyChanged("ShowActivity");
				}
			}
		}

		public static bool HasTodaysStats(DetailedIntervalStats stats)
		{
			return stats != null && stats.TodaysWorkIntervalsByWorkId != null && stats.TodaysWorkIntervalsByWorkId.Count != 0;
		}
	}

	public class IntervalsData : ObservableCollection<IntervalData>
	{ }

	public class IntervalData
	{
		public double Time { get; set; }
		public string WorkName { get; set; }
		public string WorkType { get; set; }
	}
}
