using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Tct.ActivityRecorderClient.Controller;
using log4net;

namespace Tct.ActivityRecorderClient.InterProcess
{
	class InterProcessWinManager : IInterProcessManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#if DEBUG
		private const string IpcEndPointName = "JcIpc-dbg";
#else
#if DEV
		private const string IpcEndPointName = "JcIpc-dev";
#else
		private const string IpcEndPointName = "JcIpc";
#endif
#endif
		private readonly SynchronizationContext context;
		private readonly CurrentWorkController currentWorkController;
		private readonly Thread serverThread;
		private readonly MenuCoordinator menuCoordinator;
		private readonly IWindowExternalTextHelper externalTextHelper;
		private volatile bool isRunning;
		private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
		private InterProcessService service;

		public InterProcessWinManager(SynchronizationContext context, CurrentWorkController currentWorkController, MenuCoordinator menuCoordinator, IWindowExternalTextHelper externalTextHelper)
		{
			this.context = context;
			this.menuCoordinator = menuCoordinator;
			this.currentWorkController = currentWorkController;
			this.externalTextHelper = externalTextHelper;
			serverThread = new Thread(RequestHandler);
			serverThread.IsBackground = true;
		}

		private void RequestHandler()
		{
			isRunning = true;
			service = new InterProcessService(context, currentWorkController, menuCoordinator, externalTextHelper);
			var endpointUri = new Uri("net.pipe://localhost/" + IpcEndPointName);
			try
			{
				using (var serviceHost = new ServiceHost(service, endpointUri))
				{
					serviceHost.Open();
					log.Debug("Started InterProcess service thread");
					while (isRunning)
						stopEvent.WaitOne(60000);
				}
				log.Debug("InterProcess service thread exited");
			}
			catch (Exception ex)
			{
				log.Error("Error while creating server", ex);
				isRunning = false;
			}
		}

		public void Start()
		{
			log.Info("Starting InterProcess service thread");
			serverThread.Start();
		}

		public void Stop()
		{
			isRunning = false;
			stopEvent.Set();
			serverThread.Join();
			log.Info("Stopped InterProcess service thread");
		}

		public void UpdateMenu(ActivityRecorderServiceReference.ClientMenu clientMenu)
		{
			// invoked on gui thread
			service.UpdateMenu(clientMenu);
		}
	}
}
