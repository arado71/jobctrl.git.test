using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using JiraSyncTool;
using log4net;

namespace JiraSyncTool
{
	partial class SyncService : ServiceBase
	{
		private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static PeriodicManagerBase periodicManager;

		protected override void OnStart(string[] args)
		{
			log.Info("Jira sync OnStart");
			StartService();
		}

		protected override void OnStop()
		{
			log.Info("Jira sync OnStop");
			StopService();
		}
		protected override void OnShutdown()
		{
			log.Info("Jira sync OnShutdown");
			StopService();
		}

		internal static void StartService()
		{	
			periodicManager = new Jira.JiraService();
		}

		internal static void StopService()
		{
			if (periodicManager != null)
				periodicManager.Stop();
		}
	}
}
