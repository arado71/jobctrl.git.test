using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace ProxyDataRouterService
{
	[RunInstaller(true)]
	public partial class ProxyDataRouterServiceInstaller : System.Configuration.Install.Installer
	{
		public ProxyDataRouterServiceInstaller()
		{
			InitializeComponent();
		}

		protected override void OnBeforeInstall(System.Collections.IDictionary savedState)
		{
			base.OnBeforeInstall(savedState);

			//# Service Account Information
			proxyDataRouterServiceProcessInstaller.Account = GetAccount();
			proxyDataRouterServiceProcessInstaller.Username = GetParam("username");
			proxyDataRouterServiceProcessInstaller.Password = GetParam("password");
		}

		private ServiceAccount GetAccount()
		{
			ServiceAccount result;
			var account = GetParam("account");
			return Enum.TryParse(account, out result)
				? result
				: ServiceAccount.LocalService;
		}

		private string GetParam(string settingName)
		{
			return Context == null || Context.Parameters == null || !Context.Parameters.ContainsKey(settingName)
				? null
				: Context.Parameters[settingName];
		}

		protected override void OnAfterInstall(System.Collections.IDictionary savedState)
		{
			base.OnAfterInstall(savedState);
			//start the service after install
			var controller = new ServiceController(serviceInstaller.ServiceName);
			try
			{
				controller.Start();
			}
			catch (Exception ex)
			{
				string source = serviceInstaller.DisplayName + " Installer";
				const string logName = "Application";
				if (!EventLog.SourceExists(source))
				{
					EventLog.CreateEventSource(source, logName);
				}

				using (var eLog = new EventLog())
				{
					eLog.Source = source;
					eLog.WriteEntry("The service could not be started. Please start the service manually. Error: " + ex.Message, EventLogEntryType.Error);
				}
			}
			finally
			{
				controller.Dispose();
			}
		}


	}
}
