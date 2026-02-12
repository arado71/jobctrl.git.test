using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using PlaybackClient;
using log4net;

namespace System
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

		public static readonly int StorageRefreshInterval = 60000;
		public static readonly string ScreenShotsDir = "C:\\";
		public static readonly int ParallelSenders = 10;
		public static readonly int SendRetries = 10;
		public static readonly int SendBaseRetryIntervalInSec = 10;
		public static readonly int ScheduleRetries = 10;
		public static readonly int ScheduleBaseRetryIntervalInSec = 60;
		public static readonly int MobileWorkItemGranularityInSec = 120;
		public static readonly string ImportUserName = "13";
		public static readonly string ImportPassword = "1";
		public static readonly int UserMultiplierLimit = 0;
		public static readonly int UserTargetCount = 0;
		public static readonly int UserIdStartIndex = 1;
		public static readonly int UserIdMaxCount = 0;
		public static readonly bool SendScreenshots = true;

		static ConfigManager()
		{
			log.Info("Initializing " + ApplicationName + " " + DebugOrReleaseString + " " + " Ver.:" + Version);
			log.Info(string.Format("Machinename: {0}, OSVersion: {1}, Framework version: {2}", Environment.MachineName, Environment.OSVersion, Environment.Version));

			StorageRefreshInterval = GetValueFromConfig("StorageRefreshInterval", StorageRefreshInterval);
			ScreenShotsDir = GetValueFromConfig("ScreenShotsDir", ScreenShotsDir);
			ParallelSenders = GetValueFromConfig("ParallelSenders", ParallelSenders);
			SendRetries = GetValueFromConfig("SendRetries", SendRetries);
			SendBaseRetryIntervalInSec = GetValueFromConfig("SendBaseRetryIntervalInSec", SendBaseRetryIntervalInSec);
			ScheduleRetries = GetValueFromConfig("ScheduleRetries", ScheduleRetries);
			ScheduleBaseRetryIntervalInSec = GetValueFromConfig("ScheduleBaseRetryIntervalInSec", ScheduleBaseRetryIntervalInSec);
			MobileWorkItemGranularityInSec = GetValueFromConfig("MobileWorkItemGranularityInSec", MobileWorkItemGranularityInSec);
			ImportUserName = GetValueFromConfig("ImportUserName", ImportUserName);
			ImportPassword = GetValueFromConfig("ImportPassword", AuthenticationHelper.GetHashedHexString(ImportPassword));
			UserMultiplierLimit = GetValueFromConfig("UserMultiplierLimit", UserMultiplierLimit);
			UserTargetCount = GetValueFromConfig("UserTargetCount", UserTargetCount);
			UserIdStartIndex = GetValueFromConfig("UserIdStartIndex", UserIdStartIndex);
			UserIdMaxCount = GetValueFromConfig("UserIdMaxCount", UserIdMaxCount);
			if (UserTargetCount >= 0 && UserMultiplierLimit == 0) UserMultiplierLimit = 100;
			SendScreenshots = GetValueFromConfig("SendScreenshots", SendScreenshots);
		}

		/// <summary>
		/// Gets a value from the .config file
		/// </summary>
		/// <typeparam name="T">Type of the value</typeparam>
		/// <param name="appSettingsKey">Name of the value</param>
		/// <param name="defaultValue">Default value to use</param>
		/// <returns>The found value in the config or the default value</returns>
		internal static T GetValueFromConfig<T>(string appSettingsKey, T defaultValue)
		{
			string configValue;
			try
			{
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
				log.Info("[" + appSettingsKey + "] = '" + parsedValue + "' (Found in config)");
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
