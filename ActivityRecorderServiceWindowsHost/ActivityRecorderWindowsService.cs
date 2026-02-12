using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Maintenance;
using Tct.ActivityRecorderService.Proxy;

namespace ActivityRecorderServiceWindowsHost
{
	public partial class ActivityRecorderWindowsService : ServiceBase
	{
		private const string ComponentName = "PcServer";
		private ServiceHost serviceHost;

		public ActivityRecorderWindowsService()
		{
			InitializeComponent();
			ServiceName = ActivityRecorderWindowsServiceInstaller.ServiceName;
		}

		protected override void OnStart(string[] args)
		{
			OnStop();

			bool isProxyMode;
			try
			{
				var configValue = ConfigurationManager.AppSettings["ProxyMode"];
				isProxyMode = configValue != null && bool.Parse(configValue);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An exception occured: {0}", ex.Message);
				isProxyMode = false;
			}
			serviceHost = new ServiceHost(isProxyMode ? typeof(ProxyService) : typeof(ActivityRecorderService));
			serviceHost.Open();
			GlobalActivityEventDbHelper.Add(GlobalActivityEventType.ComponentStarted, ComponentName);
		}

		protected override void OnStop()
		{
			if (serviceHost == null) return;
			serviceHost.Close();
			serviceHost = null;
			GlobalActivityEventDbHelper.Add(GlobalActivityEventType.ComponentStopped, ComponentName);
		}
	}
}
