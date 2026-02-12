using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient.Capturing.Extra
{
	public class WorkTimeStatsFromWebsiteManager : IDisposable
	{
		private const int WorkTimeStatsRefreshTimeInMinutes = 15;
		private const int WorkTimeStatsRefreshOnErrorTimeInMinutes = 5;
		private const int WorkTimeStatsRefreshTimeInMs = WorkTimeStatsRefreshTimeInMinutes * 60 * 1000;
		private const int WorkTimeStatsRefreshTimeAfterIntervalChangeInMs = 30 * 1000;
		private const int WorkTimeStatsRefreshOnErrorInMs = WorkTimeStatsRefreshOnErrorTimeInMinutes * 60 * 1000;

		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private WorkTimeStats knownWorkTimeStats;
		private int? lastQueryTick = null;
		private readonly object lockObject = new object();
		private WorkTimeCounter workTimeCounter;
		private WorkItemManager workItemManager;
		private IWorkTimeQuery workTimeHistory;
		private WorktimeStatIntervals lastWorktimeStatIntervals;

		public bool HasExactLocalWorkTime
		{
			get
			{
				var queryTick = lastQueryTick;
				return knownWorkTimeStats != null &&
					queryTick.HasValue && 
					Environment.TickCount - queryTick < WorkTimeStatsRefreshTimeInMs &&
					(ConfigManager.LocalSettingsForUser.DisplayWorktimeStats == lastWorktimeStatIntervals || Environment.TickCount - queryTick < WorkTimeStatsRefreshTimeAfterIntervalChangeInMs);
			}
		}

		public WorkTimeStatsFromWebsiteManager(WorkTimeCounter counter, WorkItemManager workItemManager, IWorkTimeQuery workTimeHistory)
		{
			workTimeCounter = counter;
			this.workItemManager = workItemManager;
			this.workTimeHistory = workTimeHistory;
			workItemManager.ItemSentSuccessfully += WorkItemManagerOnItemSentSuccessfully;
			workTimeHistory.WorkTimeUpdatesSent += WorkTimeHistoryOnWorkTimeUpdatesSent;
		}

		private void WorkTimeHistoryOnWorkTimeUpdatesSent(object sender, EventArgs eventArgs)
		{
			lastQueryTick = null;
		}

		private void WorkItemManagerOnItemSentSuccessfully(object sender, SingleValueEventArgs<IUploadItem> args)
		{
			if (args.Value is ManualWorkItem || args.Value is ActivityRecorderClient.Meeting.ManualMeetingItem)
			{
				lastQueryTick = null; 
			}
		}

		internal void GetWorkTimeStatsFromServer(Action<WorkTimeStats> onSuccessCallback, Action<TimeSpan> onErrorCallback)
		{
			if (onSuccessCallback == null) throw new ArgumentNullException(nameof(onSuccessCallback));
			if (onErrorCallback == null) throw new ArgumentNullException(nameof(onErrorCallback));
			var queryTick = lastQueryTick;
			if (queryTick.HasValue && Environment.TickCount - queryTick < WorkTimeStatsRefreshTimeInMs && (ConfigManager.LocalSettingsForUser.DisplayWorktimeStats == lastWorktimeStatIntervals || Environment.TickCount - queryTick < WorkTimeStatsRefreshTimeAfterIntervalChangeInMs))
			{
				var stats = GetLocalWorkTimeStatsIfExact();
				if (stats == null)
				{
					log.Debug("Worktime stats is null.");
					onErrorCallback(workTimeCounter.TodaysWorkTime);
					return;
				}
				onSuccessCallback(stats);
				return;
			}
			lock (lockObject)
			{
				try
				{
					lastWorktimeStatIntervals = ConfigManager.LocalSettingsForUser.DisplayWorktimeStats;
					var stats = ActivityRecorderClientWrapper.Execute(x =>
						x.GetWorkTimeStatsForUser(ConfigManager.UserId, ConfigManager.EnvironmentInfo.ComputerId, lastWorktimeStatIntervals));
					knownWorkTimeStats = stats;
					lastQueryTick = Environment.TickCount;
					var localWorkTimeFromTime = knownWorkTimeStats.LastComputerWorkitemEndTime.HasValue
						? workTimeCounter.GetLocalWorkTimeFromTime(knownWorkTimeStats.LastComputerWorkitemEndTime.Value)
						: workTimeCounter.TodaysWorkTime;
					var localWorkTimeFromTimeInMs = Convert.ToInt64(localWorkTimeFromTime.TotalMilliseconds);
					WorkTimeStats currStats = CreateCopyWithOffset(stats, localWorkTimeFromTimeInMs);
					log.Debug("Got worktimes from server: " +
					          $"Today's worktime from server: {TimeSpan.FromMilliseconds(stats.TodaysWorkTimeInMs).ToHourMinuteSecondString()} " +
					          $"Last ComputerWorkkitemEndTime: {stats.LastComputerWorkitemEndTime} " +
					          $"Added time: {TimeSpan.FromMilliseconds(localWorkTimeFromTimeInMs).ToHourMinuteSecondString()} " +
					          $"Total: {TimeSpan.FromMilliseconds(currStats.TodaysWorkTimeInMs).ToHourMinuteSecondString()}");
					onSuccessCallback(currStats);
				}
				//If we get exception here then we can try to get next time
				//TODO: what kind of exceptions should we catch here?
				catch (FaultException ex)
				{
					// If the call reached the website then the problem can only be the query limit
					// If the call didn't then the 5 min retry is not so expensive
					log.Warn("Couldn't get the work times from server.", ex);
					lastQueryTick = Environment.TickCount - WorkTimeStatsRefreshTimeInMs + WorkTimeStatsRefreshOnErrorInMs;
					onErrorCallback(workTimeCounter.TodaysWorkTime);
				}
				catch (TimeoutException ex)
				{
					// This can be a communication problem or sg else. We expect that the server got the message so we wait.
					log.Warn("Timeout occured in GetWorkTimeStatsFromServer.", ex);
					lastQueryTick = Environment.TickCount - WorkTimeStatsRefreshTimeInMs + WorkTimeStatsRefreshOnErrorInMs;
					onErrorCallback(workTimeCounter.TodaysWorkTime);
				}
				catch (CommunicationException ex)
				{
					// We expect there is a communication failure, so the server didn't consumed the message, so we don't have to wait.
					log.Warn("Communication exception in GetWorkTimeStatsFromServer.", ex);
					onErrorCallback(workTimeCounter.TodaysWorkTime);
				}
				catch (Exception ex)
				{
					// This is unexpected so we log the error and wait
					lastQueryTick = Environment.TickCount;
					log.Error("Couldn't get worktimestats from server.", ex);
					onErrorCallback(workTimeCounter.TodaysWorkTime);
				}
			}
		}

		internal WorkTimeStats GetLocalWorkTimeStatsIfExact(bool suppressLogging = false)
		{
			lock (lockObject)
			{
				var currStats = knownWorkTimeStats;
				if (currStats == null) return null;
				var localWorkTimeFromTime = knownWorkTimeStats.LastComputerWorkitemEndTime.HasValue 
					? workTimeCounter.GetLocalWorkTimeFromTime(knownWorkTimeStats.LastComputerWorkitemEndTime.Value)
					: workTimeCounter.TodaysWorkTime;
				var localWorkTimeFromTimeInMs = Convert.ToInt64(localWorkTimeFromTime.TotalMilliseconds);
				if (localWorkTimeFromTimeInMs != 0 && !suppressLogging)
					log.Debug($"Adjusting local worktime stats by {localWorkTimeFromTime.ToHourMinuteSecondString()}. " +
					          $"Total: {TimeSpan.FromMilliseconds(currStats.TodaysWorkTimeInMs + localWorkTimeFromTimeInMs).ToHourMinuteSecondString()}");
				WorkTimeStats stats = CreateCopyWithOffset(currStats, localWorkTimeFromTimeInMs);
				return stats;
			}
		}

		private static WorkTimeStats CreateCopyWithOffset(WorkTimeStats stats, long localWorkTimeFromTimeInMs)
		{
			return new WorkTimeStats
			{

				TodaysWorkTimeInMs = stats.TodaysWorkTimeInMs + localWorkTimeFromTimeInMs,
				ThisWeeksWorkTimeInMs = stats.ThisWeeksWorkTimeInMs + localWorkTimeFromTimeInMs,
				ThisMonthsWorkTimeInMs = stats.ThisMonthsWorkTimeInMs + localWorkTimeFromTimeInMs,
				ThisQuarterWorkTimeInMs = stats.ThisQuarterWorkTimeInMs + localWorkTimeFromTimeInMs,
				ThisYearWorkTimeInMs = stats.ThisYearWorkTimeInMs + localWorkTimeFromTimeInMs,
				TodaysTargetNetWorkTimeInMs = stats.TodaysTargetNetWorkTimeInMs,
				ThisWeeksTargetNetWorkTimeInMs = stats.ThisWeeksTargetNetWorkTimeInMs,
				ThisWeeksTargetUntilTodayNetWorkTimeInMs = stats.ThisWeeksTargetUntilTodayNetWorkTimeInMs,
				ThisMonthsTargetUntilTodayNetWorkTimeInMs = stats.ThisMonthsTargetUntilTodayNetWorkTimeInMs,
				ThisMonthsTargetNetWorkTimeInMs = stats.ThisMonthsTargetNetWorkTimeInMs,
				ThisQuarterTargetNetWorkTimeInMs = stats.ThisQuarterTargetNetWorkTimeInMs,
				ThisQuarterTargetUntilTodayNetWorkTimeInMs = stats.ThisQuarterTargetUntilTodayNetWorkTimeInMs,
				ThisYearTargetNetWorkTimeInMs = stats.ThisYearTargetNetWorkTimeInMs,
				ThisYearTargetUntilTodayNetWorkTimeInMs = stats.ThisYearTargetUntilTodayNetWorkTimeInMs
			};
		}

		public void Dispose()
		{
			workItemManager.ItemSentSuccessfully -= WorkItemManagerOnItemSentSuccessfully;
			workTimeHistory.WorkTimeUpdatesSent -= WorkTimeHistoryOnWorkTimeUpdatesSent;
		}
	}
}
