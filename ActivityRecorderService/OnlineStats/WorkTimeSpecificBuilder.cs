using log4net;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.Messaging;

namespace Tct.ActivityRecorderService.OnlineStats
{
	internal class WorkTimeSpecificBuilder
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly CachedDictionary<int, Tuple<DateTime, TimeSpan>> specificTargetWorkTimeLimitsByUserId = new CachedDictionary<int, Tuple<DateTime, TimeSpan>>(TimeSpan.FromDays(1), true);

		private readonly TimeSpan cacheInterval = TimeSpan.FromMinutes(3);
		private readonly OnlineUserStatsBuilder onlineUserStatsBuilder;
		private readonly Func<WorktimeSchedulesLookup> worktimeSchedulesLookupFactory;
		private WorktimeSchedulesLookup worktimeSchedulesLookup;
		private WorktimeSchedulesLookup.WorkTimeScheduleSpecific scheduleSpecific;
		private DateTime lastCached;
		private TimeSpan todayWorkTime;
		private DateTime firstNotificationDate, secondNotificationDate, lastNotificationDate;


		public WorkTimeSpecificBuilder(OnlineUserStatsBuilder onlineUserStatsBuilder, Func<WorktimeSchedulesLookup> worktimeSchedulesLookupFactory)
		{
			this.onlineUserStatsBuilder = onlineUserStatsBuilder;
			this.worktimeSchedulesLookupFactory = worktimeSchedulesLookupFactory;
		}

		public void AddWorkItem(WorkItem workItem)
		{
			var localReportDate = CalculatorHelper.GetLocalReportDate(workItem.StartDate, onlineUserStatsBuilder.UserInfo.TimeZone, onlineUserStatsBuilder.UserInfo.StartOfDayOffset);
			if (DateTime.UtcNow - lastCached > cacheInterval) //slow path
			{
				worktimeSchedulesLookup = worktimeSchedulesLookupFactory();
				scheduleSpecific = worktimeSchedulesLookup.GetWorkTimeScheduleSpecific(localReportDate);
				lastCached = DateTime.UtcNow;
				if (scheduleSpecific == null)
				{
					lock (specificTargetWorkTimeLimitsByUserId)
						specificTargetWorkTimeLimitsByUserId.Remove(onlineUserStatsBuilder.UserId);
					return;
				}
				var stats = onlineUserStatsBuilder.GetBriefUserStats();
				todayWorkTime = stats.TodaysWorkTime.NetWorkTime;
				var shutdownTimeLimit = scheduleSpecific.DailyTargetWorkTime + scheduleSpecific.DailyOvertimeLimit - TimeSpan.FromMinutes(ConfigManager.SpecificScheduleLastNotificationBeforeMinutesBefore);
				lock (specificTargetWorkTimeLimitsByUserId)
					specificTargetWorkTimeLimitsByUserId.Set(onlineUserStatsBuilder.UserId, Tuple.Create(localReportDate, shutdownTimeLimit > TimeSpan.FromMinutes(1) ? shutdownTimeLimit : TimeSpan.FromMinutes(1)));
			}

			if (scheduleSpecific == null) return;

			todayWorkTime += workItem.EndDate - workItem.StartDate; //fast path

			if (todayWorkTime > scheduleSpecific.DailyTargetWorkTime + scheduleSpecific.DailyOvertimeLimit - TimeSpan.FromMinutes(ConfigManager.SpecificScheduleLastNotificationBeforeMinutes + ConfigManager.SpecificScheduleLastNotificationBeforeMinutesBefore) + TimeSpan.FromSeconds(ConfigManager.SpecificScheduleNotificationTimingSecs) && lastNotificationDate != localReportDate)
			{
				lastNotificationDate = localReportDate;
				secondNotificationDate = localReportDate;
				firstNotificationDate = localReportDate;
				SetCulture(onlineUserStatsBuilder.UserInfo);
				var minutes = (int)(((scheduleSpecific.DailyTargetWorkTime + scheduleSpecific.DailyOvertimeLimit - todayWorkTime).TotalSeconds
				                     - ConfigManager.SpecificScheduleLastNotificationBeforeMinutesBefore * 60
				                     + ConfigManager.SpecificScheduleNotificationTimingSecs + 30) / 60);
				var content = string.Format(EmailStats.EmailStats.SpecificScheduleLastNotificationMessage, minutes > 0 ? minutes : 0);
				MessageService.Instance.InsertMessage(onlineUserStatsBuilder.UserId, onlineUserStatsBuilder.UserId, content, "SpecificScheduleNotification");
			}
			else if (todayWorkTime > scheduleSpecific.DailyTargetWorkTime + scheduleSpecific.DailyOvertimeLimit - TimeSpan.FromMinutes(ConfigManager.SpecificScheduleSecondNotificationBeforeMinutes) + TimeSpan.FromSeconds(ConfigManager.SpecificScheduleNotificationTimingSecs) && secondNotificationDate != localReportDate)
			{
				secondNotificationDate = localReportDate;
				firstNotificationDate = localReportDate;
				SetCulture(onlineUserStatsBuilder.UserInfo);
				var minutes = (int)(((scheduleSpecific.DailyTargetWorkTime + scheduleSpecific.DailyOvertimeLimit - todayWorkTime).TotalSeconds 
				                     + ConfigManager.SpecificScheduleNotificationTimingSecs + 30) / 60);
				var content = string.Format(EmailStats.EmailStats.SpecificScheduleSecondNotificationMessage, minutes > 0 ? minutes : 0);
				MessageService.Instance.InsertMessage(onlineUserStatsBuilder.UserId, onlineUserStatsBuilder.UserId, content, "SpecificScheduleNotification");
			}
			else if (todayWorkTime > scheduleSpecific.DailyTargetWorkTime && firstNotificationDate != localReportDate)
			{
				firstNotificationDate = localReportDate;
				SetCulture(onlineUserStatsBuilder.UserInfo);
				var content = EmailStats.EmailStats.SpecificScheduleFirstNotificationMessage;
				MessageService.Instance.InsertMessage(onlineUserStatsBuilder.UserId, onlineUserStatsBuilder.UserId, content, "SpecificScheduleNotification");
			}

		}

		private static void SetCulture(UserStatInfo userStatInfo)
		{
			var culture = CultureInfo.GetCultureInfo(string.IsNullOrEmpty(userStatInfo.CultureId)
				? EmailStatsHelper.DefaultCulture
				: userStatInfo.CultureId);
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
		}

		public static bool PatchRules(int userId, string version, ref string oldVersion, out string newVersion, out List<WorkDetectorRule> patchedRules)
		{
			Tuple<DateTime, TimeSpan> found;
			lock (specificTargetWorkTimeLimitsByUserId)
				if (!specificTargetWorkTimeLimitsByUserId.TryGetValue(userId, out found))
				{
					newVersion = oldVersion;
					patchedRules = null;
					return false;
				}

			var versionSuffix = found.Item1.ToString("MMdd");
			if (version + versionSuffix == oldVersion)
			{
				newVersion = oldVersion;
				patchedRules = null;
				return true;
			}

			var tempRule = new WorkDetectorRule
			{
				RuleType = WorkDetectorRuleType.TempStopWork,
				RelatedId = -1,
				Name = "__ShutdownRuleAuto",
				IsEnabled = true,
				IsRegex = true,
				TitleRule = ".*",
				UrlRule = ".*",
				ProcessRule = ".*",
				ServerId = 1,
				ExtensionRuleParametersById = new Dictionary<string, List<ExtensionRuleParameter>>
				{
					{ "Internal.Work", new List<ExtensionRuleParameter>
					{
						new ExtensionRuleParameter { Name = "Command", Value = "ShutdownComputer" },
						new ExtensionRuleParameter { Name = "DailyWorkingMinutes", Value = ((int)found.Item2.TotalMinutes).ToString() }
					}}
				},
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>
				{
					{ "Internal.Work", new Dictionary<string, string> {{ "IsDailyWorkingHoursReached", "1" } }}
				},

			};

			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var rules = context.GetWorkDetectorRules(userId);
				if (rules == null)
				{
					newVersion = versionSuffix;
					patchedRules = new List<WorkDetectorRule> { tempRule };
					return true;
				}

				newVersion = rules.Version + versionSuffix;
				patchedRules = rules.Value.Concat(new[] { tempRule }).ToList();
				return true;
			}
		}
	}
}
