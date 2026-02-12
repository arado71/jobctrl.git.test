using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using log4net;

namespace AppControlService
{
	public partial class AppControlService : ServiceBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// windows service skeleton based on this article:
		// https://code.msdn.microsoft.com/windowsapps/CSWindowsService-9f2f568e

		private bool stopping;
		private readonly ManualResetEvent stoppedEvent;
		private ServiceHost serviceHost;
		private AppControlServiceService serviceInstance;

		public AppControlService()
		{
			InitializeComponent();

			this.stopping = false;
			this.stoppedEvent = new ManualResetEvent(false);
		}

		protected override void OnStart(string[] args)
		{
			// Log a service start message to the Application log. 
			this.eventLog1.WriteEntry("AppControlService in OnStart.");

			// Queue the main service function for execution in a worker thread. 
			ThreadPool.QueueUserWorkItem(ServiceWorkerThread); 
		}

		private void ServiceWorkerThread(object state)
		{
			StartServiceHost();
			// Periodically check if the service is stopping. 
			while (!this.stopping)
			{
				// Perform main service function here... 

				Thread.Sleep(5000);

				//serviceInstance.CheckAndHandleClosed();
			}

			StopServiceHost();
			// Signal the stopped event. 
			this.stoppedEvent.Set();
		}

		private void StartServiceHost()
		{
			try
			{
				serviceInstance = new AppControlServiceService();
				serviceHost = new ServiceHost(serviceInstance);
				serviceHost.Description.Endpoints.Clear(); //use programmatic endpoint only

				if (!IsDebug) //we don't expose mex for release builds and we might not have permissions if not elevated
				{
					var metad = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
					if (metad != null) serviceHost.Description.Behaviors.Remove(metad);
				}

				serviceHost.AddServiceEndpoint("AppControlService.IAppControlServiceService", new NetNamedPipeBinding(), "net.pipe://localhost/AppControlServiceService");
				serviceHost.Open();

				log.Info("AppControlServiceService started successfully.");
			}
			catch (AddressAlreadyInUseException ex)
			{
				log.Error("AppControlServiceService already listening on the given address.", ex);
				Environment.Exit(-1);
			}
			catch (Exception ex)
			{
				log.Error("Error occured while starting AppControlServiceService.", ex);
				Environment.Exit(-1);
			}
		}

		private void StopServiceHost()
		{
			try
			{
				if (serviceHost != null)
				{
					serviceHost.Close();
					serviceHost = null;
					log.Info("AppControlServiceService stopped.");
				}
			}
			catch (Exception ex)
			{
				log.Error("Error occured while closing OutlookMailCaptureHostForm.", ex);
			}
		}

		protected override void OnStop()
		{
			// Log a service stop message to the Application log. 
			this.eventLog1.WriteEntry("AppControlService in OnStop.");

			// Indicate that the service is stopping and wait for the finish  
			// of the main service function (ServiceWorkerThread). 
			this.stopping = true;
			this.stoppedEvent.WaitOne();
		}

		private static bool IsDebug
		{
			get
			{
#if DEBUG
				return true;
#else
				return false;
#endif
			}
		}
	}
}
