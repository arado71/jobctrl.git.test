using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService
{
	public class ConfigManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly Version Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
		public static readonly string DebugOrReleaseString = (System.Reflection.Assembly.GetExecutingAssembly()
			.GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
			.OfType<System.Reflection.AssemblyConfigurationAttribute>()
			.FirstOrDefault(n => !string.IsNullOrEmpty(n.Configuration)) ?? new System.Reflection.AssemblyConfigurationAttribute("Unknown")).Configuration + " build";
		public static readonly string ApplicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

		public static readonly int CompletionPortThreadsMin = 500;
		public static readonly int CompletionPortThreadsMax = 2000;
		public static readonly int WorkerThreadsMin = 500;
		public static readonly int WorkerThreadsMax = 4000;
		public static readonly int IdleAfterInSec = (int)TimeSpan.FromMinutes(2).TotalSeconds;
		public static readonly int TimedOutAfterInSec = (int)TimeSpan.FromSeconds(20).TotalSeconds;
		public static readonly int ActivityAverageIntervalInSec = (int)TimeSpan.FromMinutes(2).TotalSeconds;
		public static readonly int AggregateInterval = (int)TimeSpan.FromMinutes(5).TotalMilliseconds; //TotalWorkTimeStats is calculated after aggregation so we need to aggr often
		public static readonly int AggregateDailyTimesInterval = (int)TimeSpan.FromHours(0.5).TotalMilliseconds;
		public static readonly int IvrActivityImportInterval = -1;
		public static readonly int IvrActivityUpdateInterval = -1;
		public static readonly int StatsWorkItemUpdateInterval = (int)TimeSpan.FromSeconds(20).TotalMilliseconds;
		public static readonly int UsageStatsUpdateInterval = (int)TimeSpan.FromMinutes(60).TotalMilliseconds;
		public static readonly int UsageStatsShortSummaryDays = 60;
		public static readonly int MobileStatusUpdateInterval = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
		public static readonly int CalendarUpdateInterval = (int)TimeSpan.FromHours(1).TotalMilliseconds;
		public static readonly int MeetingSyncInterval = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
		public static readonly int OnlineDataUpdateInterval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
		public static readonly int OnlineAggrDataUpdateInterval = (int)TimeSpan.FromMinutes(15).TotalMilliseconds;
		public static readonly int OnlineOldDataUpdateInterval = (int)TimeSpan.FromMinutes(60).TotalMilliseconds;
		public static readonly int OnlineDetailedStatsCacheAgeInSec = (int)TimeSpan.FromSeconds(25).TotalSeconds;
		public static readonly int OnlineUserStatsOldCacheAgeInSec = (int)TimeSpan.FromMinutes(10).TotalSeconds;
		public static readonly int OnlineVirtualScreenScalePct = 33;
		public static readonly int VersionCacheInterval = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;
		public static readonly int NotificationCacheUpdateInterval = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;
		public static readonly bool IvrGetLocationInfo = true;
		public static readonly int IvrLocationInfoTimeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
		public static readonly int IvrLocationInfoRetryCount = 5;
		public static readonly int UserIdRefreshInterval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
		public static readonly int StorageRefreshInterval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
		public static readonly int WorkNameRefreshInterval = (int)TimeSpan.FromHours(1).TotalMilliseconds;
		public static readonly string EmailSmtpHost = "dont.send.emails";
		public static readonly int EmailSmtpPort = 25;
		public static readonly string EmailSmtpDomain = null;
		public static readonly bool EmailSsl = false;
		public static readonly string EmailFrom = "JobCTRL";
		public static readonly string EmailUserName = "";
		public static readonly string EmailPassword = "";
		public static readonly string EmailBcc;
		public static readonly string EmailCc;
		public static readonly int EmailTimeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
		public static readonly int EmailRetryType1Count = 5;
		public static readonly int EmailRetryType1InvervalInSec = (int)TimeSpan.FromMinutes(5).TotalSeconds;
		public static readonly int EmailRetryType2Count = 40;
		public static readonly int EmailRetryType2InvervalInSec = (int)TimeSpan.FromHours(2).TotalSeconds;
		public static readonly int EmailMaxSizeInMB = 25;
		public static readonly string ScreenShotsDir = "C:\\";
		public static readonly string VoiceRecordingsDir = "C:\\";
		public static readonly string MobileDataDir = "C:\\";
		public static readonly string TelemetryDataDir = "C:\\";
		public static readonly string ClientLogsDir = "C:\\";
		public static readonly string ClientLogsUrl = "http://localhost/logs";
		public static readonly string ClientLogsEmailTo = "";
		public static readonly string SilverlightDir = "Silverlight";
		public static readonly string EmailsToSendDir = "EmailsToSend";
		public static readonly string DeadLetterDir = "DeadLetter";
		public static readonly DateTime IgnoreErrorsCutOff = DateTime.MinValue;
		public static readonly string MetConnectionString = "#p:wstcpip #a:localhost:1999";
		public static readonly int AuthCacheMaxAgeInSec = 530;
		public static readonly int AuthCacheMaxAgeThresholdInSec = 30;
		public static readonly int AuthPasswordExpiryCacheMaxAgeInSec = 60;
		public static readonly int ClientLoginTicketMaxAgeInHour = 24;
		public static readonly int MaxWorkItemAgeInDays = 60;
		public static readonly int MaxWorkItemLength = 300000;
		public static readonly int DownloadChunkSizeBytes = 50 * 1024;
		public static readonly int CacheSizeProcessNameId = 1000;
		public static readonly int CacheSizeTitleId = 60000;
		public static readonly int CacheSizeUrlId = 10000;
		public static readonly int CacheSizeWorkName = 20000;
		public static readonly int CacheSizeWorkOrProjects = 100000;
		public static readonly int CacheSizeCollectorKeyId = 1000;
		public static readonly int CacheSizeCollectorValueId = 50000;
		public static readonly int ClientKickTimeoutInSec = (int)TimeSpan.FromMinutes(15).TotalSeconds;
		public static readonly string ImportUserName = null;
		public static readonly string ImportPassword = null;
		public static readonly string WebsiteSystemComponentTicket = "7fc72f60-52c0-4142-9257-4d34cffb4b7a";
		public static readonly bool ServerOnlyMessagesEnabled = true;
		public static readonly int MinutesBeforeWorktimeEndNotification = 31;		
		public static readonly int OcrLearningTimeoutInMinutes = 60;
		public static readonly int OcrLearningIterationsMaxCount = 100;
		public static readonly string OcrLearningIssueNotificationEmailAddress = "";
		public static readonly int OcrLearningManagerInterval = (int)TimeSpan.FromHours(1).TotalMilliseconds;
		public static readonly bool IsCollectedValuesEncrypted = false;
		public static readonly string GoogleClientId = "";
		public static readonly string GoogleClientSecret = "";
		public static readonly bool CreditRunOutCheckingEnabled = true;
		public static readonly bool TodoNotificationToAdminsEnabled = true;
		public static readonly bool GiveUpdateByClientAssemblyName = false;
		public static readonly int SpecificScheduleSecondNotificationBeforeMinutes = 30;
		public static readonly int SpecificScheduleLastNotificationBeforeMinutes = 2;
		public static readonly int SpecificScheduleLastNotificationBeforeMinutesBefore = 8;
		public static readonly int SpecificScheduleNotificationTimingSecs = -30;
		public static readonly string ReportServerAddress = null;

		private static readonly string[] emailErrorsToIgnore;
		public static string[] EmailErrorsToIgnore
		{
			get { return emailErrorsToIgnore.ToArray(); }
		}

		private static readonly string[] updatePathStrings;
		private static readonly NameValueCollection secureAppSettings = ConfigurationManager.GetSection("secureAppSettings") as NameValueCollection;

		public static string[] UpdatePathStrings
		{
			get { return updatePathStrings.ToArray(); }
		}

		static ConfigManager()
		{
			log.Info("Initializing " + ApplicationName + " " + DebugOrReleaseString + " " + " Ver.:" + Version);
			log.Info(string.Format("Machinename: {0}, OSVersion: {1}, Framework version: {2}, Bitness: {3}", Environment.MachineName, Environment.OSVersion, Environment.Version, IntPtr.Size == 4 ? "x86" : "x64"));
			log.Info("Current directory: " + System.IO.Directory.GetCurrentDirectory());

			WorkerThreadsMin = GetValueFromConfig("workerThreadsMin", WorkerThreadsMin);
			WorkerThreadsMax = GetValueFromConfig("workerThreadsMax", WorkerThreadsMax);
			CompletionPortThreadsMin = GetValueFromConfig("completionPortThreadsMin", CompletionPortThreadsMin);
			CompletionPortThreadsMax = GetValueFromConfig("completionPortThreadsMax", CompletionPortThreadsMax);
			IdleAfterInSec = GetValueFromConfig("IdleAfterInSec", IdleAfterInSec);
			TimedOutAfterInSec = GetValueFromConfig("TimedOutAfterInSec", TimedOutAfterInSec);
			ActivityAverageIntervalInSec = GetValueFromConfig("ActivityAverageIntervalInSec", ActivityAverageIntervalInSec);
			AggregateInterval = GetValueFromConfig("AggregateInterval", AggregateInterval);
			AggregateDailyTimesInterval = GetValueFromConfig("AggregateDailyTimesInterval", AggregateDailyTimesInterval);
			IvrActivityImportInterval = GetValueFromConfig("IvrActivityImportInterval", IvrActivityImportInterval);
			IvrActivityUpdateInterval = GetValueFromConfig("IvrActivityUpdateInterval", IvrActivityUpdateInterval);
			StatsWorkItemUpdateInterval = GetValueFromConfig("StatsWorkItemUpdateInterval", StatsWorkItemUpdateInterval);
			UsageStatsUpdateInterval = GetValueFromConfig("UsageStatsUpdateInterval", UsageStatsUpdateInterval);
			UsageStatsShortSummaryDays = GetValueFromConfig("UsageStatsShortSummaryDays", UsageStatsShortSummaryDays);
			MobileStatusUpdateInterval = GetValueFromConfig("MobileStatusUpdateInterval", MobileStatusUpdateInterval);
			CalendarUpdateInterval = GetValueFromConfig("CalendarUpdateInterval", CalendarUpdateInterval);
			MeetingSyncInterval = GetValueFromConfig("MeetingSyncInterval", MeetingSyncInterval);
			OnlineDataUpdateInterval = GetValueFromConfig("OnlineDataUpdateInterval", OnlineDataUpdateInterval);
			OnlineAggrDataUpdateInterval = GetValueFromConfig("OnlineAggrDataUpdateInterval", OnlineAggrDataUpdateInterval);
			OnlineOldDataUpdateInterval = GetValueFromConfig("OnlineOldDataUpdateInterval", OnlineOldDataUpdateInterval);
			OnlineDetailedStatsCacheAgeInSec = GetValueFromConfig("OnlineDetailedStatsCacheAgeInSec", OnlineDetailedStatsCacheAgeInSec);
			OnlineUserStatsOldCacheAgeInSec = GetValueFromConfig("OnlineUserStatsOldCacheAgeInSec", OnlineUserStatsOldCacheAgeInSec);
			OnlineVirtualScreenScalePct = GetValueFromConfig("OnlineVirtualScreenScalePct", OnlineVirtualScreenScalePct);
			VersionCacheInterval = GetValueFromConfig("VersionCacheInterval", VersionCacheInterval);
			NotificationCacheUpdateInterval = GetValueFromConfig("NotificationCacheUpdateInterval", NotificationCacheUpdateInterval);
			IvrGetLocationInfo = GetValueFromConfig("IvrGetLocationInfo", IvrGetLocationInfo);
			IvrLocationInfoTimeout = GetValueFromConfig("IvrLocationInfoTimeout", IvrLocationInfoTimeout);
			IvrLocationInfoRetryCount = GetValueFromConfig("IvrLocationInfoRetryCount", IvrLocationInfoRetryCount);
			UserIdRefreshInterval = GetValueFromConfig("UserIdRefreshInterval", UserIdRefreshInterval);
			StorageRefreshInterval = GetValueFromConfig("StorageRefreshInterval", StorageRefreshInterval);
			WorkNameRefreshInterval = GetValueFromConfig("WorkNameRefreshInterval", WorkNameRefreshInterval);
			EmailSmtpHost = GetValueFromConfig("EmailSmtpHost", EmailSmtpHost);
			EmailSmtpPort = GetValueFromConfig("EmailSmtpPort", EmailSmtpPort);
			EmailSmtpDomain = GetValueFromConfig("EmailSmtpDomain", EmailSmtpDomain);
			EmailSsl = GetValueFromConfig("EmailSsl", EmailSsl);
			EmailFrom = GetValueFromConfig("EmailFrom", EmailFrom);
			EmailUserName = GetValueFromConfig("EmailUserName", EmailUserName);
			EmailPassword = GetValueFromConfig("EmailPassword", EmailPassword, true);
			EmailBcc = GetValueFromConfig("EmailBcc", EmailBcc);
			EmailCc = GetValueFromConfig("EmailCc", EmailCc);
			EmailTimeout = GetValueFromConfig("EmailTimeout", EmailTimeout);
			EmailRetryType1Count = GetValueFromConfig("EmailRetryType1Count", EmailRetryType1Count);
			EmailRetryType1InvervalInSec = GetValueFromConfig("EmailRetryType1InvervalInSec", EmailRetryType1InvervalInSec);
			EmailRetryType2Count = GetValueFromConfig("EmailRetryType2Count", EmailRetryType2Count);
			EmailRetryType2InvervalInSec = GetValueFromConfig("EmailRetryType2InvervalInSec", EmailRetryType2InvervalInSec);
			EmailMaxSizeInMB = GetValueFromConfig("EmailMaxSizeInMB", EmailMaxSizeInMB);
			ScreenShotsDir = GetValueFromConfig("ScreenShotsDir", ScreenShotsDir);
			SilverlightDir = GetValueFromConfig("SilverlightDir", SilverlightDir);
			EmailsToSendDir = GetValueFromConfig("EmailsToSendDir", EmailsToSendDir);
			DeadLetterDir = GetValueFromConfig("DeadLetterDir", DeadLetterDir);
			VoiceRecordingsDir = GetValueFromConfig("VoiceRecordingsDir", VoiceRecordingsDir);
			IgnoreErrorsCutOff = GetValueFromConfig("IgnoreErrorsCutOff", IgnoreErrorsCutOff);
			ClientLogsDir = GetValueFromConfig("ClientLogsDir", ClientLogsDir);
			MobileDataDir = GetValueFromConfig("MobileDataDir", MobileDataDir);
			TelemetryDataDir = GetValueFromConfig("TelemetryDataDir", TelemetryDataDir);
			ClientLogsUrl = GetValueFromConfig("ClientLogsUrl", ClientLogsUrl);
			ClientLogsEmailTo = GetValueFromConfig("ClientLogsEmailTo", ClientLogsEmailTo);
			MetConnectionString = GetValueFromConfig("MetConnectionString", MetConnectionString);
			AuthCacheMaxAgeInSec = GetValueFromConfig("AuthCacheMaxAgeInSec", AuthCacheMaxAgeInSec);
			AuthCacheMaxAgeThresholdInSec = GetValueFromConfig("AuthCacheMaxAgeThresholdInSec", AuthCacheMaxAgeThresholdInSec);
			AuthPasswordExpiryCacheMaxAgeInSec = GetValueFromConfig("AuthPasswordExpiryCacheMaxAgeInSec", AuthPasswordExpiryCacheMaxAgeInSec);
			MaxWorkItemLength = GetValueFromConfig("MaxWorkItemLength", MaxWorkItemLength);
			MaxWorkItemAgeInDays = GetValueFromConfig("MaxWorkItemAgeInDays", MaxWorkItemAgeInDays);
			DownloadChunkSizeBytes = GetValueFromConfig("DownloadChunkSizeBytes", DownloadChunkSizeBytes);
			CacheSizeProcessNameId = GetValueFromConfig("CacheSizeProcessNameId", CacheSizeProcessNameId);
			CacheSizeTitleId = GetValueFromConfig("CacheSizeTitleId", CacheSizeTitleId);
			CacheSizeUrlId = GetValueFromConfig("CacheSizeUrlId", CacheSizeUrlId);
			CacheSizeWorkName = GetValueFromConfig("CacheSizeWorkName", CacheSizeWorkName);
			CacheSizeWorkOrProjects = GetValueFromConfig("CacheSizeWorkOrProjects", CacheSizeWorkOrProjects);
			CacheSizeCollectorKeyId = GetValueFromConfig("CacheSizeCollectorKeyId", CacheSizeCollectorKeyId);
			CacheSizeCollectorValueId = GetValueFromConfig("CacheSizeCollectorValueId", CacheSizeCollectorValueId);
			ClientKickTimeoutInSec = GetValueFromConfig("ClientKickTimeoutInSec", ClientKickTimeoutInSec);
			ImportUserName = GetValueFromConfig("ImportUserName", ImportUserName);
			ImportPassword = GetValueFromConfig("ImportPassword", ImportPassword, true);
			WebsiteSystemComponentTicket = GetValueFromConfig("WebsiteSystemComponentTicket", WebsiteSystemComponentTicket);
			ServerOnlyMessagesEnabled = GetValueFromConfig("ServerOnlyMessagesEnabled", ServerOnlyMessagesEnabled);
			MinutesBeforeWorktimeEndNotification = GetValueFromConfig("MinutesBeforeWorktimeEndNotification", MinutesBeforeWorktimeEndNotification);
			OcrLearningTimeoutInMinutes = GetValueFromConfig("OcrLearningTimeoutInMinutes", OcrLearningTimeoutInMinutes);
			OcrLearningIterationsMaxCount = GetValueFromConfig("OcrLearningIterationsMaxCount", OcrLearningIterationsMaxCount);
			OcrLearningIssueNotificationEmailAddress = GetValueFromConfig("OcrLearningIssueNotificationEmailAddress", OcrLearningIssueNotificationEmailAddress);
			OcrLearningManagerInterval  = GetValueFromConfig("OcrLearningManagerInterval", OcrLearningManagerInterval);
			IsCollectedValuesEncrypted = GetValueFromConfig("IsCollectedValuesEncrypted", IsCollectedValuesEncrypted);
			GoogleClientId = GetValueFromConfig("GoogleClientId", GoogleClientId);
			GoogleClientSecret = GetValueFromConfig("GoogleClientSecret", GoogleClientSecret);
			CreditRunOutCheckingEnabled = GetValueFromConfig("CreditRunOutCheckingEnabled", CreditRunOutCheckingEnabled);
			TodoNotificationToAdminsEnabled = GetValueFromConfig("TodoNotificationToAdminsEnabled", TodoNotificationToAdminsEnabled);
			GiveUpdateByClientAssemblyName = GetValueFromConfig("GiveUpdateByClientAssemblyName", GiveUpdateByClientAssemblyName);
			SpecificScheduleSecondNotificationBeforeMinutes = GetValueFromConfig("SpecificScheduleSecondNotificationBeforeMinutes", SpecificScheduleSecondNotificationBeforeMinutes);
			SpecificScheduleLastNotificationBeforeMinutes = GetValueFromConfig("SpecificScheduleLastNotificationBeforeMinutes", SpecificScheduleLastNotificationBeforeMinutes);
			SpecificScheduleLastNotificationBeforeMinutesBefore = GetValueFromConfig("SpecificScheduleLastNotificationBeforeMinutesBefore", SpecificScheduleLastNotificationBeforeMinutesBefore);
			SpecificScheduleNotificationTimingSecs = GetValueFromConfig("SpecificScheduleNotificationTimingSecs", SpecificScheduleNotificationTimingSecs);
			ReportServerAddress = GetValueFromConfig("ReportServerAddress", ReportServerAddress);

			if (MaxWorkItemLength > 600000) //this would cause malfunction
			{
				log.Error("MaxWorkItemLength cannot be greater than 600000");
				MaxWorkItemLength = 600000;
			}

			emailErrorsToIgnore = ConfigurationManager.AppSettings.AllKeys.Where(n => n.StartsWith("EmailErrorToIgnore"))
					.Select(n =>
					{
						var val = ConfigurationManager.AppSettings[n];
						log.Info("[" + n + "] = '" + val + "' (Found in config)");
						return val;
					})
					.ToArray();

			updatePathStrings = ConfigurationManager.AppSettings.AllKeys.Where(n => n.StartsWith("UpdatePath|"))
					.Select(n =>
					{
						var val = ConfigurationManager.AppSettings[n];
						log.Info("[" + n + "] = '" + val + "' (Found in config)");
						return n + "|" + val;
					})
					.ToArray();
		}

		/// <summary>
		/// Gets a value from the .config file
		/// </summary>
		/// <typeparam name="T">Type of the value</typeparam>
		/// <param name="appSettingsKey">Name of the value</param>
		/// <param name="defaultValue">Default value to use</param>
		/// <param name="sensitiveInformation">If parameter is sensitive information, it is masked in log files</param>
		/// <returns>The found value in the config or the default value</returns>
		internal static T GetValueFromConfig<T>(string appSettingsKey, T defaultValue, bool sensitiveInformation = false)
		{
			string configValue;
			try
			{
				configValue = secureAppSettings?[appSettingsKey];
				if (string.IsNullOrEmpty(configValue))
					configValue = ConfigurationManager.AppSettings[appSettingsKey];
			}
			catch (Exception ex)
			{
				log.Error("[" + appSettingsKey + "] = '" + defaultValue + "' (Unable to get value)", ex);
				return defaultValue;
			}
			if (string.IsNullOrEmpty(configValue))
			{
				log.Info("[" + appSettingsKey + "] = '" + defaultValue + "' (Not found in config)");
				return defaultValue;
			}
			try
			{
				T parsedValue = (T)Convert.ChangeType(configValue, typeof(T));
				log.Info("[" + appSettingsKey + "] = '" + (sensitiveInformation ? "********" : parsedValue.ToString()) + "' (Found in config)");
				return parsedValue;
			}
			catch (Exception ex)
			{
				log.Error("[" + appSettingsKey + "] = '" + defaultValue + "' (Unable to parse '" + configValue + "' as " + typeof(T).Name + ")", ex);
				return defaultValue;
			}
		}
	}
}
