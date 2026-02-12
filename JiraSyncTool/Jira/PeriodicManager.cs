using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JiraSyncTool.Jira.Model.Jira;
using JiraSyncTool.Jira.Logic;
using JiraSyncTool.Jira.Utils;

namespace JiraSyncTool.Jira
{
	internal class PeriodicManager : PeriodicManagerBase
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(PeriodicManager));
		private int worklogSyncCounter;
		private int worklogSyncInterval;
		private JcApiAdapter jcApiAdapter;
		private int jcFullSyncCounter;
		private int jcFullSyncInterval;
		private JiraConfig config;
		private volatile bool initialized = false;
		private volatile bool shouldRun = false;

		public JiraConfig Config
		{
			get { return config; }
		}

		public PeriodicManager()
		{
			config = new JiraConfigFileConfigManager().Config;
			worklogSyncInterval = config.JiraWorklogSyncInterval;
			worklogSyncCounter = worklogSyncInterval;
			jcFullSyncInterval = config.JcFullSyncInterval;
			jcFullSyncCounter = jcFullSyncInterval;
			jcApiAdapter = new JcApiAdapter(config.CompanyAuthCode);
			if (config.CompanyAuthCode != Guid.Empty
			    && !string.IsNullOrEmpty(config.JiraAuthServer)
			    && !string.IsNullOrEmpty(config.JiraBaseUrl)
			    && !string.IsNullOrEmpty(config.JiraOAuthClientId)
			    && !string.IsNullOrEmpty(config.JiraSharedSecret)
			    && !string.IsNullOrEmpty(config.WorklogComment)) shouldRun = true;
			initialized = true;
		}

		public PeriodicManager(JiraConfig config)
		{
			this.config = config;
			worklogSyncInterval = config.JiraWorklogSyncInterval;
			worklogSyncCounter = worklogSyncInterval;
			jcFullSyncInterval = config.JcFullSyncInterval;
			jcFullSyncCounter = jcFullSyncInterval;
			jcApiAdapter = new JcApiAdapter(config.CompanyAuthCode);
			shouldRun = true;
			initialized = true;
		}

		protected override void ExecuteOnTimer(CancellationToken token)
		{
			while (!initialized) Thread.Sleep(10);
			if (!shouldRun) return;
			if (++jcFullSyncCounter >= jcFullSyncInterval)
			{
				jcApiAdapter = new JcApiAdapter(config.CompanyAuthCode);
				jcFullSyncCounter = 0;
			}
			var jiraApiAdapter = new JiraApiAdapter(config);
			ApplicationSync sync = new ApplicationSync(jiraApiAdapter, jcApiAdapter, config.TargetRootProjectId);
			try
			{
				sync.SyncToJc(token);
				if (worklogSyncInterval != -1 && ++worklogSyncCounter >= worklogSyncInterval)
				{
					var interval = new Model.Interval()
					{
						StartDate = DateTime.Now.AddDays(-21).Date,
						EndDate = DateTime.Now.Date
					};
					try
					{
						sync.SyncToJira(token, interval);
						worklogSyncCounter = 0;
					}
					catch (Exception e)
					{
						log.Error("Unexpected exception in SyncToJira. Trying again in the next cycle.", e);
						jcFullSyncInterval = config.JcFullSyncInterval;
						jcFullSyncCounter = jcFullSyncInterval;
					}

				}
			}
			catch (OperationCanceledException)
			{
				log.Info("Synchronization canceled");
			}
			catch (Exception e)
			{
				log.Error("Unexpected exception in SyncToJc. Trying again in the next cycle.", e);
				jcFullSyncInterval = config.JcFullSyncInterval;
				jcFullSyncCounter = jcFullSyncInterval;
			}

			log.Info("Synchronization finished");
		}
	}
}
