using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Jira.Logic;
using log4net;

namespace JiraSyncTool.Jira
{
	class JiraService : PeriodicManagerBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Dictionary<int, PeriodicManager> instancesDictionary = new Dictionary<int, PeriodicManager>();
		private readonly PeriodicManager instanceFromConfig;
		private PeriodicManager instanceFromApi = null;
		private volatile bool initialized = false;
		private readonly bool shouldGetConfigsFromDatabase = false;

		public JiraService()
		{
			IntervalInMinutes = 1;
			instanceFromConfig = new PeriodicManager();
			if (!string.IsNullOrEmpty(Properties.Settings.Default.JobControlConnectionString))
			{
				shouldGetConfigsFromDatabase = true;
			} else
			{
				log.Info("The connection string is empty, getting configs from database is disabled.");
			}

			initialized = true;
		}

		public override void Stop()
		{
			foreach (var instance in instancesDictionary.Values)
			{
				instance.Stop();
			}
			base.Stop();
		}

		protected override void ExecuteOnTimer(CancellationToken token)
		{
			while(!initialized) Thread.Sleep(10);
			try
			{
				if (shouldGetConfigsFromDatabase)
				{
					using (ISyncDataContext context = new JiraSyncDataContext())
					{
						var list = context.GetConfigs();
						if (list == null) return;
						foreach (var configElement in list)
						{
							if (configElement.CompanyId == instanceFromConfig.Config.CompanyId) continue;
							if (instancesDictionary.ContainsKey(configElement.CompanyId))
							{
								var periodicManager = instancesDictionary[configElement.CompanyId];
								if (!periodicManager.Config.Equals(configElement))
								{
									log.Info(
										$"Config for company (id: {configElement.CompanyId}) changed. Starting sync instance with the new config...");
									periodicManager.Stop();
									instancesDictionary[configElement.CompanyId] = new PeriodicManager(configElement);
								}
							}
							else
							{
								log.Info(
									$"New configuration got from the database. CompanyId: {configElement.CompanyId}. Starting new sync instance...");
								instancesDictionary[configElement.CompanyId] = new PeriodicManager(configElement);
							}
						}

						var stoppableInstances = instancesDictionary.Keys.ToList().Except<int>(list.Select(x => x.CompanyId));
						foreach (var stoppableInstanceCompanyId in stoppableInstances)
						{
							log.Info($"Stopping sync instance with companyId:{stoppableInstanceCompanyId}");
							instancesDictionary[stoppableInstanceCompanyId].Stop();
							instancesDictionary.Remove(stoppableInstanceCompanyId);
						}
					}
				}
			}
			catch (Exception e)
			{
				log.Warn("Database error.", e);
			}

			if (instanceFromConfig.Config.CompanyAuthCode != Guid.Empty && instanceFromApi == null)
			{
				log.Info("Creating configuration from companyAuthCode in config...");
				using (ISyncDataContext context = new JiraFromJcApiSyncDataContext(instanceFromConfig.Config.CompanyAuthCode))
				{
					try
					{
						instanceFromApi = new PeriodicManager(context.GetConfigs().First());
					}
					catch (Exception e)
					{
						log.Error("Couldn't initialize config from API.", e);
					}
				}
			}
		}
	}
}
