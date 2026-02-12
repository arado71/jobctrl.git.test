using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Extra
{
	public class WorkTimeStatsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int workTimeStatsUpdateInterval = 30 * 1000; // 30 secs /**/5 GetSimpleWorkTimeStats GetClientWorkTimeStats 2616 bytes/call inside, in variables; but 69 packets 33526 bytes/call outside, in Ethernet packets
		private static string FilePath { get { return "TotalWorkTimeStats-" + ConfigManager.UserId; } }
		private long todaysWorkTime = long.MinValue;

		public static string NotAvailableWorkTimeStatsString { get { return ConfigManager.LocalSettingsForUser.DisplayThisWeeksStats ? NotAvailableWorkTimeStatsStringWithWeek : NotAvailableWorkTimeStatsStringWithoutWeek; } }

		public TimeSpan? TodaysWorkTime { get { var ticks = Interlocked.Read(ref todaysWorkTime); return ticks == long.MinValue ? new TimeSpan?() : new TimeSpan(ticks); } }
		private ClientWorkTimeStats clientWorkTimeStats;
		public ClientWorkTimeStats ClientWorkTimeStats
		{
			get { return Interlocked.CompareExchange(ref clientWorkTimeStats, null, null); }
			private set { Interlocked.Exchange(ref clientWorkTimeStats, value); }
		}

		public string WorkTimeStatsStringTooltip { get; private set; } //quick and dirty... and thread-safe (not volatile)
		public string WorkTimeStatsString { get { return ConfigManager.LocalSettingsForUser.DisplayThisWeeksStats ? WorkTimeStatsStringWithWeek : WorkTimeStatsStringWithoutWeek; } }
		private string WorkTimeStatsStringWithWeek { get; set; }
		private string WorkTimeStatsStringWithoutWeek { get; set; }
#if LegacySimple
		public event EventHandler<SingleValueEventArgs<SimpleWorkTimeStats>> SimpleWorkTimeStatsReceived;
#endif
		public event EventHandler PasswordError;
		public event EventHandler ActiveOnlyError;
		public event EventHandler PasswordExpiredError;

		public WorkTimeStatsManager()
			: base(log)
		{
			WorkTimeStatsStringTooltip = FormatTooltip(null);
			WorkTimeStatsStringWithoutWeek = FormatWorkTimeStats(null, false);
			WorkTimeStatsStringWithWeek = FormatWorkTimeStats(null, true);
		}

		public void LoadData()
		{
#if LegacySimple
			SimpleWorkTimeStats value;
			if (IsolatedStorageSerializationHelper.Exists(FilePath)
				&& IsolatedStorageSerializationHelper.Load(FilePath, out value))
			{
				OnSimpleWorkTimeStatsReceived(value);
				log.Info("Loaded Total WorkTime Stats from disk");
			}
#else
			if (IsolatedStorageSerializationHelper.Exists(FilePath))
			{
				IsolatedStorageSerializationHelper.Delete(FilePath);
				log.Info("Deleted Total WorkTime Stats on disk");
			}
#endif
		}

		protected override void ManagerCallbackImpl()
		{
			ClientWorkTimeStats stats = null;
			try
			{
				int userId = ConfigManager.UserId;
				stats = ActivityRecorderClientWrapper.Execute(n => n.GetClientWorkTimeStats(userId));
				//todo these should be moved somewhere else
				if (ConfigManager.IsAuthDataRequired) //ugly hax
				{
					var authData = ActivityRecorderClientWrapper.Execute(n => n.Authenticate(AuthenticationHelper.GetClientInfo()));
					ConfigManager.SetAndSaveAuthDataIfApplicable(authData);
				}
#if LegacySimple
				//Use GetSimpleWorkTimeStats frequently until we calculate stats on the client side 
				//hax don't create new manager & tcp connection for GetSimpleWorkTimeStats
				var totalWorkTimeStats = ActivityRecorderClientWrapper.Execute(n => n.GetSimpleWorkTimeStats(userId, DateTime.UtcNow));
				if (totalWorkTimeStats == null) //probably the service was restarted and still calculating
				{
					log.Debug("No Total work time stats received");
				}
				else
				{
					log.Debug("Total work time received until " + totalWorkTimeStats.ToDate);
					OnSimpleWorkTimeStatsReceived(totalWorkTimeStats);
					IsolatedStorageSerializationHelper.Save(FilePath, totalWorkTimeStats);
				}
#endif
			}
			catch (Exception ex)
			{
				if (AuthenticationHelper.IsInvalidUserOrPasswordException(ex))
				{
					log.Info("Invalid password");
					OnPasswordError();
				}
				else if (AuthenticationHelper.IsActiveUserOnlyException(ex))
				{
					log.Info("User is not active");
					OnActiveOnlyError();
				}
				else if(AuthenticationHelper.IsPasswordExpiredException(ex))
				{
					log.Info("Password expired");
					OnPasswordExpiredError();
				}
				{
					WcfExceptionLogger.LogWcfError("communicate with the server", log, ex);
				}
			}
			ClientWorkTimeStats = stats;
			WorkTimeStatsStringTooltip = FormatTooltip(stats);
			WorkTimeStatsStringWithoutWeek = FormatWorkTimeStats(stats, false);
			WorkTimeStatsStringWithWeek = FormatWorkTimeStats(stats, true);
			if (stats != null && stats.TodaysWorkTime != null)
			{
				Interlocked.Exchange(ref todaysWorkTime, stats.TodaysWorkTime.NetWorkTime.Ticks);
			}
			else
			{
				Interlocked.Exchange(ref todaysWorkTime, long.MinValue);
			}
		}

		private static readonly string NotAvailableWorkTimeStatsStringWithWeek = Labels.TodaysWorkTime + ": --:--" + Environment.NewLine + Labels.ThisWeeksWorkTime + ": --:-- (\u0394: --:--, \u01a9:  --:--)" + Environment.NewLine + Labels.ThisMonthsWorkTime + ": --:-- (\u0394: --:--, \u01a9:  ---:--)  ";
		private static readonly string NotAvailableWorkTimeStatsStringWithoutWeek = Labels.TodaysWorkTime + ": --:--" + Environment.NewLine + Labels.ThisMonthsWorkTime + ": --:-- (\u0394: --:--, \u01a9:  ---:--)  ";
		private static string FormatWorkTimeStats(ClientWorkTimeStats stats, bool includeWeek)
		{
			if (stats == null || stats.TodaysWorkTime == null || stats.ThisWeeksWorkTime == null || stats.ThisMonthsWorkTime == null)
			{
				return includeWeek ? NotAvailableWorkTimeStatsStringWithWeek : NotAvailableWorkTimeStatsStringWithoutWeek;
			}
			return Labels.TodaysWorkTime + ": " + stats.TodaysWorkTime.NetWorkTime.ToHourMinuteString()
				+ (includeWeek
					? Environment.NewLine + Labels.ThisWeeksWorkTime + ": " + stats.ThisWeeksWorkTime.NetWorkTime.ToHourMinuteString() + " (\u0394: " + (stats.ThisWeeksTargetUntilTodayNetWorkTime - stats.ThisWeeksWorkTime.NetWorkTime).ToHourMinuteString() + ", \u01a9: " + stats.ThisWeeksTargetNetWorkTime.ToHourMinuteString() + ")"
					: "")
				+ Environment.NewLine + Labels.ThisMonthsWorkTime + ": " + stats.ThisMonthsWorkTime.NetWorkTime.ToHourMinuteString() + " (\u0394: " + (stats.ThisMonthsTargetUntilTodayNetWorkTime - stats.ThisMonthsWorkTime.NetWorkTime).ToHourMinuteString() + ", \u01a9: " + stats.ThisMonthsTargetNetWorkTime.ToHourMinuteString() + ")";
		}

		private static readonly string helpString = "\u0394: " + Labels.WorkTimeStats_DeltaTime + ", \u01a9: " + Labels.WorkTimeStats_SummaTime;
		private static string FormatTooltip(ClientWorkTimeStats stats)
		{
			if (stats == null || stats.TodaysWorkTime == null || stats.TodaysWorkTime.NetWorkTime == TimeSpan.Zero) return helpString;
			return ""
				+ (stats.TodaysWorkTime.ComputerWorkTime == TimeSpan.Zero ? "" : Labels.TodaysComputerWorkTime + ": " + stats.TodaysWorkTime.ComputerWorkTime.ToHourMinuteString() + " ")
				+ (stats.TodaysWorkTime.MobileWorkTime == TimeSpan.Zero ? "" : Labels.TodaysMobileWorkTime + ": " + stats.TodaysWorkTime.MobileWorkTime.ToHourMinuteString() + " ")
				+ (stats.TodaysWorkTime.ManuallyAddedWorkTime == TimeSpan.Zero ? "" : Labels.TodaysManuallyAddedWorkTime + ": " + stats.TodaysWorkTime.ManuallyAddedWorkTime.ToHourMinuteString() + " ")
				+ helpString;
		}

		protected override int ManagerCallbackInterval
		{
			get { return workTimeStatsUpdateInterval; }
		}

#if LegacySimple
		private void OnSimpleWorkTimeStatsReceived(SimpleWorkTimeStats totalStats)
		{
			EventHandler<SingleValueEventArgs<SimpleWorkTimeStats>> received = SimpleWorkTimeStatsReceived;
			if (received != null) received(this, SingleValueEventArgs.Create(totalStats));
		}
#endif

		private void OnPasswordError()
		{
			EventHandler error = PasswordError;
			if (error != null) error(this, EventArgs.Empty);
		}

		private void OnActiveOnlyError()
		{
			EventHandler error = ActiveOnlyError;
			if (error != null) error(this, EventArgs.Empty);
		}

		private void OnPasswordExpiredError()
		{
			EventHandler error = PasswordExpiredError;
			if (error != null) error(this, EventArgs.Empty);
		}
	}
}
