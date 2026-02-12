using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Stats
{
	/// <summary>
	/// Class for fetching the latest version of daily aggregated stats from the server.
	/// </summary>
	public class DailyWorkTimeStatsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int callbackInterval = (int)TimeSpan.FromMinutes(35).TotalMilliseconds; //30 mins is the refresh interval on the server
		private static string FilePath { get { return "DailyWorkTimeStats-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<DailyWorkTimeStatsData>> DailyWorkTimeStatsChanged;

		private DailyWorkTimeStatsData dailyStats = new DailyWorkTimeStatsData() { DailyWorkTimes = new Dictionary<DateTime, DailyWorkTimeStats>() };

		public DailyWorkTimeStatsManager()
			: base(log)
		{
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				int userId = ConfigManager.UserId;
				var stats = ActivityRecorderClientWrapper.Execute(n => n.GetDailyWorkTimeStats(userId, dailyStats.CurrentVersion));
				if (stats != null) UpdateDailyStats(stats);
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get daily worktime stats", log, ex);
			}
		}

		private void UpdateDailyStats(List<DailyWorkTimeStats> stats)
		{
			if (stats == null || stats.Count == 0) return;
			foreach (var dailyWorkTimeStat in stats)
			{
				dailyStats.DailyWorkTimes[dailyWorkTimeStat.Day] = dailyWorkTimeStat;
				dailyStats.CurrentVersion = Math.Max(dailyStats.CurrentVersion, dailyWorkTimeStat.Version);
			}
			IsolatedStorageSerializationHelper.Save(FilePath, dailyStats);
			OnDailyStatsChanged(dailyStats);
		}

		protected override int ManagerCallbackInterval
		{
			get { return callbackInterval; }
		}

		public void LoadData()
		{
			log.Info("Loading DailyWorkTimeStatsData from disk");
			DailyWorkTimeStatsData data;
			if (IsolatedStorageSerializationHelper.Exists(FilePath)
				&& IsolatedStorageSerializationHelper.Load(FilePath, out data))
			{
				dailyStats = data;
			}
			OnDailyStatsChanged(dailyStats); //always raise so we know the initial state
		}

		private void OnDailyStatsChanged(DailyWorkTimeStatsData stats)
		{
			Debug.Assert(stats != null);
			var del = DailyWorkTimeStatsChanged;
			if (del != null) del(this, SingleValueEventArgs.Create(stats.DeepClone())); //don't leak the original data as it will be modified
		}
	}
}
