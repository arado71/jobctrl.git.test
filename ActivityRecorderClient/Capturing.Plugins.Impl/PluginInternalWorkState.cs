using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Extra;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Meeting;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public sealed class PluginInternalWorkState : ICaptureExtension, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string PluginId = "Internal.Work";
		public const string ParamDailyWorkingMinutes = "DailyWorkingMinutes";
		public const string ParamWeeklyWorkingMinutes = "WeeklyWorkingMinutes";
		public const string ParamMonthlyWorkingMinutes = "MonthlyWorkingMinutes";
		public const string ParamCommand = "Command";
		public const string KeyIsWorking = "State";
		public const string KeyIsIPAddresses = "IPAddresses";
		public const string KeyIsDailyWorkingHoursReached = "IsDailyWorkingHoursReached";
		public const string KeyIsMonthlyWorkingHoursReached = "IsMonthlyWorkingHoursReached";
		public const string KeyIsWeeklyWorkingHoursReached = "IsWeeklyWorkingHoursReached";
		public const string KeyInactivityMins = "InactivityMins";
		public const string KeyOngoingMeeting = "OngoingMeeting";
		public const string KeyStartupTime = "StartupTime";
		public const string KeyIsDesktopLocked = "IsDesktopLocked";
		public const string ValueUnknown = "unknown";
		public const string ValueEmpty = "nonwork";

		private MutualWorkTypeCoordinator mutualCoordinator;
		private bool isInitialized;
		private volatile string lastState;
		private int dailyWorkingMinutes;
		private int weeklyWorkingMinutes;
		private int monthlyWorkingMinutes;
		private string commandText;
		private WorkTimeStatsFromWebsiteManager workTimeStatsManager;
		private IdleDetector idleDetector;
		private MeetingNotifier meetingNotifier;
		private int isBackgroundQueryRunning;
		private Func<bool> isDesktopLockedAccessor;
		private static bool isShutdownStarted;

		public string Id { get { return PluginId; } }

		private bool TryInitialize()
		{
			var winPlatform = Platform.Factory as Platform.PlatformWinFactory;
			if (winPlatform?.MainForm?.CurrentWorkController?.MutualWorkTypeCoordinator == null) return false;
			if (winPlatform.MainForm.IdleDetector == null) return false;
			mutualCoordinator = winPlatform.MainForm.CurrentWorkController.MutualWorkTypeCoordinator;
			idleDetector = winPlatform.MainForm.IdleDetector;
			meetingNotifier = winPlatform.MainForm.MeetingNotifier;
			isDesktopLockedAccessor = () => winPlatform.MainForm.IsDesktopLocked;
			lock(idleDetector) if (idleDetector.ProcessIdleReset == null) idleDetector.ProcessIdleReset = ProcessIdleReset; // inject action to idleDetector. Multiple instances of plugin don't matter, because of single idleDetector
			workTimeStatsManager = winPlatform.MainForm.WorkTimeStatsFromWebsiteManager;
			mutualCoordinator.StateTransitionCompleted += HandleStateChanged;
			winPlatform.MainForm.GuiContext.Post(_ =>
			{
				lastState = mutualCoordinator.StateString;
				if (string.IsNullOrEmpty(lastState)) lastState = ValueEmpty;
				log.DebugFormat("State set to {0}", lastState);
			}, null);
			log.Debug("Internal.WorkState plugin initialized successfully");
			return true;
		}

		private void HandleStateChanged(object sender, EventArgs e)
		{
			DebugEx.EnsureGuiThread();
			lastState = mutualCoordinator.StateString;
			if (string.IsNullOrEmpty(lastState)) lastState = ValueEmpty;
			log.DebugFormat("State changed to {0}", lastState);
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamDailyWorkingMinutes;
			yield return ParamMonthlyWorkingMinutes;
			yield return ParamWeeklyWorkingMinutes;
			yield return ParamCommand;
		}

		public void SetParameter(string name, string value)
		{
			if (name == ParamDailyWorkingMinutes)
			{
				if (int.TryParse(value, out dailyWorkingMinutes))
					log.DebugFormat("[ParamDailyWorkingMinutes] = " + dailyWorkingMinutes);
			}
			else if (name == ParamWeeklyWorkingMinutes)
			{
				if (int.TryParse(value, out weeklyWorkingMinutes))
					log.DebugFormat("[ParamWeeklyWorkingMinutes] = " + weeklyWorkingMinutes);
			}
			else if (name == ParamMonthlyWorkingMinutes)
			{
				if (int.TryParse(value, out monthlyWorkingMinutes))
					log.DebugFormat("[ParamMonthlyWorkingMinutes] = " + monthlyWorkingMinutes);
			}
			else if (name == ParamCommand)
			{
				commandText = value;
				log.Debug("[ParamCommand] = " + commandText);
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyIsWorking;
			yield return KeyIsIPAddresses;
			yield return KeyIsDailyWorkingHoursReached;
			yield return KeyIsMonthlyWorkingHoursReached;
			yield return KeyIsWeeklyWorkingHoursReached;
			yield return KeyInactivityMins;
			yield return KeyOngoingMeeting;
			yield return KeyStartupTime;
			yield return KeyIsDesktopLocked;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!isInitialized)
			{
				if (!TryInitialize())
				{
					yield return new KeyValuePair<string, string>(KeyIsWorking, ValueUnknown);
					yield return new KeyValuePair<string, string>(KeyIsIPAddresses, ValueUnknown);
					yield break;
				}

				isInitialized = true;
			}

			var currentStatus = lastState;
			var dailymins = 0;
			var weeklymins = 0;
			var monthlymins = 0;
			if (dailyWorkingMinutes > 0 || weeklyWorkingMinutes > 0 || monthlyWorkingMinutes > 0)
			{
				var clientWorkTimeStats = workTimeStatsManager.GetLocalWorkTimeStatsIfExact(true);
				if (clientWorkTimeStats != null)
				{
					dailymins = (int) clientWorkTimeStats.TodaysWorkTimeInMs / 60000;
					weeklymins = (int) clientWorkTimeStats.ThisWeeksWorkTimeInMs / 60000;
					monthlymins = (int) clientWorkTimeStats.ThisMonthsWorkTimeInMs / 60000;
				}

				if (clientWorkTimeStats == null || !workTimeStatsManager.HasExactLocalWorkTime)
				{
					if (Interlocked.Exchange(ref isBackgroundQueryRunning, 1) == 0)
						ThreadPool.QueueUserWorkItem(_ => workTimeStatsManager.GetWorkTimeStatsFromServer(__ => isBackgroundQueryRunning = 0, __ => isBackgroundQueryRunning = 0));
				}
			}
			if (!string.IsNullOrEmpty(currentStatus)) yield return new KeyValuePair<string, string>(KeyIsWorking, currentStatus);
			yield return new KeyValuePair<string, string>(KeyIsIPAddresses, string.Join("|", IpDetector.Instance.NetworkAdapterIPAddressesAndRDPClientAddress.ToList()));
			if (dailyWorkingMinutes > 0) yield return new KeyValuePair<string, string>(KeyIsDailyWorkingHoursReached, dailymins >= dailyWorkingMinutes ? "1" : "0");
			if (weeklyWorkingMinutes > 0) yield return new KeyValuePair<string, string>(KeyIsWeeklyWorkingHoursReached, weeklymins >= weeklyWorkingMinutes ? "1" : "0");
			if (monthlyWorkingMinutes > 0) yield return new KeyValuePair<string, string>(KeyIsMonthlyWorkingHoursReached, monthlymins >= monthlyWorkingMinutes ? "1" : "0");
			var idleMins = (int)(idleDetector.IdleDuringWorkTime + idleDetector.IdleAfterWorkTime).TotalMinutes;
			yield return new KeyValuePair<string, string>(KeyInactivityMins, idleMins.ToString());
			yield return new KeyValuePair<string, string>(KeyStartupTime, ConfigManager.StartupTime.ToString("yyyy/MM/dd HH:mm:ss"));
			yield return new KeyValuePair<string, string>(KeyOngoingMeeting, (meetingNotifier.UpcomingMeetings?.Count(m => m.StartDate <= DateTime.UtcNow && DateTime.UtcNow <= m.EndDate) ?? 0).ToString());
			yield return new KeyValuePair<string, string>(KeyIsDesktopLocked, isDesktopLockedAccessor == null ? null : isDesktopLockedAccessor() ? "1" : "0");
		}

		public void Dispose()
		{
			if (mutualCoordinator != null) mutualCoordinator.StateTransitionCompleted -= HandleStateChanged;
		}

		public void ProcessIdleReset(Rules.IWorkChangingRule matchingRule)
		{
			var idleMins = matchingRule?.ExtensionRules?.Where(e => e.Key.Id == PluginId && e.Key.Key == KeyInactivityMins).Select(e => e.Value).FirstOrDefault();
			if (idleMins != null) idleDetector.ResetIdleWorkTime();
			var command = matchingRule?.OriginalRule?.ExtensionRuleParametersById != null && matchingRule.OriginalRule.ExtensionRuleParametersById.TryGetValue(PluginId, out var values) ? values.Where(v => v.Name == ParamCommand).Select(v => v.Value).FirstOrDefault() : null;
			switch (command)
			{
				case "ShutdownComputer":
					if (isShutdownStarted) return;
					isShutdownStarted = true;
					log.Info("Shutdown initiated...");
#if !DEBUG
					WinApi.Shutdown();
#endif
					break;
			}
		}
	}
}
