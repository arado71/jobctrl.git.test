using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ActivityRecorderServiceWindowsHost
{
	[RunInstaller(true)]
	public class ActivityRecorderWindowsServiceInstaller : Installer
	{
		public static readonly string ServiceDisplayName = "JobCTRL Application Server";
		public static readonly string ServiceName = "JobCTRL Server";

		private readonly ServiceProcessInstaller serviceProcessInstaller;

		public ActivityRecorderWindowsServiceInstaller()
		{
			serviceProcessInstaller = new ServiceProcessInstaller();
			var serviceInstaller = new ServiceInstaller();

			//# Service Information

			serviceInstaller.DisplayName = ServiceDisplayName;
			serviceInstaller.StartType = ServiceStartMode.Automatic;

			//# This must be identical to the WindowsService.ServiceBase name

			//# set in the constructor of WindowsService.cs

			serviceInstaller.ServiceName = ServiceName;

			this.Installers.Add(serviceProcessInstaller);
			this.Installers.Add(serviceInstaller);
		}

		protected override void OnBeforeInstall(System.Collections.IDictionary savedState)
		{
			base.OnBeforeInstall(savedState);

			//# Service Account Information
			serviceProcessInstaller.Account = GetAccount();
			serviceProcessInstaller.Username = GetParam("username");
			serviceProcessInstaller.Password = GetParam("password");
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
			var controller = new ServiceController(ServiceName);
			try
			{
				controller.Start();
			}
			catch (Exception ex)
			{
				string source = ServiceDisplayName + " Installer";
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
