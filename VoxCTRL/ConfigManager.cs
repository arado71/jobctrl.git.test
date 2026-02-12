using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using VoxCTRL.Communication;
using log4net;
using VoxCTRL.ActivityRecorderServiceReference;
using System.Diagnostics;
using VoxCTRL.VersionReporting;

namespace VoxCTRL
{
	public class ConfigManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly string ApplicationName = "VoxCTRL";
		public static Version Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
		public static readonly string DebugOrReleaseString = (System.Reflection.Assembly.GetExecutingAssembly()
			.GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
			.OfType<System.Reflection.AssemblyConfigurationAttribute>()
			.FirstOrDefault(n => !string.IsNullOrEmpty(n.Configuration)) ?? new System.Reflection.AssemblyConfigurationAttribute("Unknown")).Configuration + " build";
		public static int UserId;
		public static string UserName;
		public static string UserPassword;
		public static readonly string ValidCertificate;
		public static bool IsRoamingStorageScopeNeeded { get; private set; }
		public static ComputerIdGenerator CompIdGenerator;

		static ConfigManager()
		{
			log.Info("Initializing " + ApplicationName + " " + DebugOrReleaseString + " " + " Ver.:" + Version);
			log.Info(string.Format("Machinename: {0}, OSVersion: {1}, Framework version: {2}", Environment.MachineName, Environment.OSVersion, Environment.Version));
			log.Info("TickCount: " + Environment.TickCount + " (" + TimeSpan.FromMilliseconds(Environment.TickCount).ToHourMinuteSecondString() + ") Now: " + DateTime.Now + " UtcNow: " + DateTime.UtcNow);
			ValidCertificate = GetValueFromConfig("ValidCertificate", ValidCertificate);
			CompIdGenerator = new ComputerIdGenerator();
			log.InfoFormat("ComputerId: {0}", CompIdGenerator.ComputerId);
			IsRoamingStorageScopeNeeded = GetValueFromConfig("IsRoamingStorageScopeNeeded", false);
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

		private static DateTime? userPasswordExpirationDate;
		public static DateTime? UserPasswordExpirationDate
		{
			get { return userPasswordExpirationDate; }
			set
			{
				if (value == userPasswordExpirationDate) return;
				userPasswordExpirationDate = value;
				log.Info("UserPasswordExpirationDate is set to " + value);
			}
		}

		private static bool? isNameMandatory;
		public static bool? IsNameMandatory
		{
			get { return isNameMandatory; }
			set
			{
				if (value == isNameMandatory) return;
				isNameMandatory = value;
				log.Info("IsNameMandatory is set to " + value);
			}
		}

		private static int? quality;
		public static int? Quality
		{
			get { return quality; }
			set
			{
				if (value == quality) return;
				quality = value;
				log.Info("Quality is set to " + value);
			}
		}

		private static bool isManualStartStopEnabled = true;
		public static bool IsManualStartStopEnabled
		{
			get { return isManualStartStopEnabled; }
			set
			{
				if (value == isManualStartStopEnabled) return;
				isManualStartStopEnabled = value;
				log.Info("IsManualStartStopEnabled is set to " + value);
			}
		}

		private static void SetAuthData(AuthData authData)
		{
			if (authData == null) return;
			ConfigManager.UserName = authData.Name;
		}

		public static void RefreshPasswordTo(LoginData loginData)
		{
			if (loginData == null) return;
			Debug.Assert(loginData.UserId == ConfigManager.UserId);
			SetLoginData(loginData, true);
		}

		private static bool SetLoginData(LoginData loginData, bool isPasswordChange)
		{
			if (loginData == null)
			{
				log.Info("Login " + (isPasswordChange ? "password change " : "") + "cancelled");
				return false;
			}
			if (!isPasswordChange) ConfigManager.UserId = loginData.UserId;
			ConfigManager.UserPassword = loginData.UserPassword;
			ConfigManager.UserPasswordExpirationDate = loginData.UserPasswordExpirationDate;
			SetAuthData(loginData.AuthData);
			if (loginData.RememberMe)
			{
				LoginData.SaveToDisk(loginData);
			}
			else
			{
				LoginData.DeleteFromDisk();
			}
			log.Info("Login " + (isPasswordChange ? "password change " : "") + "for user: " + ConfigManager.UserId);
			return true;
		}
	}
}
