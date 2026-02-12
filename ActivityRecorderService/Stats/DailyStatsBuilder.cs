using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.OnlineStats;

namespace Tct.ActivityRecorderService.Stats
{
	//todo userInfo should be on per user basis (but this class is obsolate)
	//todo intervals across day change are not handled
	[System.Reflection.Obfuscation(Exclude = false, ApplyToMembers = true)]
	public class DailyStatsBuilder : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object thisLock = new object();
		private readonly Dictionary<long, DailyStats> stats = new Dictionary<long, DailyStats>();
		private readonly OnlineStatsManager onlineStatsManager; //hax for overriding online stats
		private readonly UserStatInfo userInfo = new UserStatInfo() { StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague;Central Europe Standard Time;Central Europe Daylight Time;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];") }; //ugly hax but this class is obsolate after all...

		public static readonly TimeSpan IdleAfter = TimeSpan.FromSeconds(ConfigManager.IdleAfterInSec);
		public static readonly TimeSpan TimedOutAfter = TimeSpan.FromSeconds(ConfigManager.TimedOutAfterInSec);
		public static readonly TimeSpan ActivityAverageInterval = TimeSpan.FromSeconds(ConfigManager.ActivityAverageIntervalInSec);

		public DailyStatsBuilder(OnlineStatsManager onlineStatsManager)
			: base(log)
		{
			try
			{
				log.Info("Initializing DailyStatsBuilder starting...");
				ManagerCallbackInterval = ConfigManager.StatsWorkItemUpdateInterval;
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					context.DeferredLoadingEnabled = false;
					var localReportDate = CalculatorHelper.GetLocalReportDate(DateTime.UtcNow, userInfo.TimeZone, userInfo.StartOfDayOffset);
					var startEndDate = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localReportDate, userInfo);
					DateTime from = startEndDate.StartDate;
					DateTime to = startEndDate.EndDate;
					var todaysItems = context.WorkItems.Where(n => n.StartDate >= from).Where(n => n.StartDate < to).ToList(); //this is not good but day overlap is not handled in this legacy class
					log.Info("Initializing DailyStatsBuilder executed query on WorkItems from " + from + " to " + to);
					foreach (var item in todaysItems)
					{
						AddWorkItem(item);
					}
				}
				UpdateManualWorkItems();
				this.onlineStatsManager = onlineStatsManager;
				log.Info("Initializing DailyStatsBuilder done");
			}
			catch (Exception ex)
			{
				log.Error("Unable to initialize DailyStatsBuilder", ex);
			}
		}

		private void UpdateManualWorkItems()
		{
			log.Info("Updating statistics of ManualWorkItems started");
			using (var context = new ManualDataClassesDataContext())
			{
				context.DeferredLoadingEnabled = false;
				var localReportDate = CalculatorHelper.GetLocalReportDate(DateTime.UtcNow, userInfo.TimeZone, userInfo.StartOfDayOffset);
				var startEndDate = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localReportDate, userInfo);
				DateTime from = startEndDate.StartDate;
				DateTime to = startEndDate.EndDate;
				var todaysItems = context.ManualWorkItems.Where(n => n.StartDate >= from).Where(n => n.StartDate < to).ToList(); //this is not good but day overlap is not handled in this legacy class
				log.Info("Found " + todaysItems.Count + " ManualWorkItem" + (todaysItems.Count == 1 ? "" : "s"));
				foreach (var item in todaysItems)
				{
					AddManualWorkItem(item);
				}
			}
			log.Info("Updating statistics of ManualWorkItems finished");
		}

		public void AddWorkItem(WorkItem item)
		{
			try
			{
				lock (thisLock)
				{
					var currStat = GetDailyStatsForDateTime(item.StartDate);
					currStat.AddWorkItem(item);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to add workItem: " + item.Id, ex);
			}
		}

		private void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			try
			{
				lock (thisLock)
				{
					//todo fix/think about if EndDate is on the next day
					//hax don't create entry if we have only ManualWorkItems because they are not handled in UserWorkStats atm. (i.e. NullRef will be thrown)
					var currStat = GetDailyStatsForDateTime(manualWorkItem.StartDate, false);
					if (currStat == null) return;
					currStat.AddManualWorkItem(manualWorkItem);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to add manualWorkItem: " + manualWorkItem.Id, ex);
			}
		}

		private DailyStats GetDailyStatsForDateTime(DateTime currentTime)
		{
			return GetDailyStatsForDateTime(currentTime, true);
		}

		private DailyStats GetDailyStatsForDateTime(DateTime currentTime, bool forceCreation)
		{
			var date = CalculatorHelper.GetLocalReportDate(currentTime, userInfo.TimeZone, userInfo.StartOfDayOffset);
			DailyStats result;
			if (!stats.TryGetValue(date.Ticks, out result))
			{
				if (!forceCreation) return null;
				result = new DailyStats() { Date = date };
				stats.Add(date.Ticks, result);
			}
			return result;
		}

		public DailyStats GetDailyStatsFiltered(StatsFilter filter)
		{
			lock (thisLock)
			{
				var currStats = GetDailyStatsForDateTime(DateTime.UtcNow);
				DailyStats statsToSend = null;
				if (currStats.SatisfiesFilter(filter))
				{
					statsToSend = currStats.GetFilteredCopy(filter);
				}
				//hax to correct offline status (I don't care about online status atm.)
				if (onlineStatsManager != null && statsToSend != null)
				{
					foreach (var userStat in statsToSend.Users)
					{
						try
						{
							var onStat = onlineStatsManager.GetBriefUserStats(userStat.UserId);
							if (onStat.Status == OnlineStatus.Offline)
							{
								userStat.Status = UserStatus.Offline;
							}
						}
						catch (Exception ex)
						{
							log.Error("Unable to get brief user stats for user " + userStat.UserId, ex);
						}
					}
				}
				return statsToSend;
			}
		}

		public WorkTimeStats GetTodaysWorkTimeStats(int userId, int groupId, int companyId)
		{
			lock (thisLock)
			{
				var currStats = GetDailyStatsForDateTime(DateTime.UtcNow);
				return currStats.GetWorkTimeStatsForUser(userId, groupId, companyId);
			}
		}

		private void RemoveOldStats()
		{
			//Remove entries older than 2 days to prevent memory leak
			var removeCutOff = DateTime.UtcNow.AddDays(-2).Date.Ticks;
			lock (thisLock)
			{
				var keysToRemove = new List<long>();
				foreach (var stat in stats)
				{
					if (stat.Key < removeCutOff)
					{
						keysToRemove.Add(stat.Key);
					}
				}
				foreach (var toRemove in keysToRemove)
				{
					stats.Remove(toRemove);
				}
			}
		}

		protected override void ManagerCallbackImpl()
		{
			UpdateManualWorkItems();
			RemoveOldStats();
		}
	}
}
