using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiraSyncTool.Properties;

namespace JiraSyncTool.Jira.Utils
{
	class JiraConfigFileConfigManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private JiraConfig config = new JiraConfig();

		public JiraConfig Config
		{
			get { return config; }
		}

		public JiraConfigFileConfigManager()
		{
			config.WorklogComment = GetValueFromSetting("WorklogComment", config.WorklogComment);
			config.JiraAuthServer = GetValueFromSetting("JiraAuthServer", config.JiraAuthServer);
			config.JiraBaseUrl = GetValueFromSetting("JiraBaseUrl", config.JiraBaseUrl);
			config.JiraAppKey = GetValueFromSetting("JiraAppKey", config.JiraAppKey);
			config.JiraOAuthClientId = GetValueFromSetting("JiraOAuthClientId", config.JiraOAuthClientId);
			config.JiraSharedSecret = GetValueFromSetting("JiraSharedSecret", config.JiraSharedSecret);
			config.JiraWorklogSyncInterval = GetValueFromSetting("JiraWorklogSyncInterval", config.JiraWorklogSyncInterval);
			config.JcFullSyncInterval = GetValueFromSetting("JcFullSyncInterval", config.JcFullSyncInterval);
			config.TargetRootProjectId = GetValueFromSetting("TargetRootProjectId", config.TargetRootProjectId);
			config.CompanyAuthCode = GetValueFromSetting("CompanyAuthCode", config.CompanyAuthCode);
			config.IsServerJira = GetValueFromSetting("IsServerJira", config.IsServerJira);
		}

		private T GetValueFromSetting<T>(string appSettingsKey, T defaultValue)
		{
			object configValue;
			try
			{
				configValue = Settings.Default[appSettingsKey];
			}
			catch (Exception ex)
			{
				log.Error("[" + appSettingsKey + "] = '" + defaultValue + "' (Unable to get value)", ex);
				return defaultValue;
			}
			if (configValue == null)
			{
				log.Info("[" + appSettingsKey + "] = '" + defaultValue + "' (Not found in config)");
				return defaultValue;
			}
			try
			{
				T parsedValue = (T)Convert.ChangeType(configValue, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
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
