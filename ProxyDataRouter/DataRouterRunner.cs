using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ProxyDataRouter
{
	public static class DataRouterRunner
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static Thread workerThread;
		private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		private static void Run(CancellationToken cancellationToken)
		{
			log.Info("Started");
			var passwordCache = new ConcurrentDictionary<int, string>();
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					using (var router = new CallRouter(passwordCache))
						while (!cancellationToken.IsCancellationRequested)
						{
							router.ProxyClient.Client.InitiateChannel();
						}
					log.Info("Stopped");
				}
				catch (Exception ex)
				{
					log.Error("Unexpected error", ex);
					Thread.Sleep(5000);
					log.Info("Restarted after error");
				}
			}
		}

		public static void Start(bool waitForStop = false)
		{
			workerThread = new Thread(() => Run(cancellationTokenSource.Token));
			workerThread.Start();
			if (waitForStop) workerThread.Join();
		}

		public static void Stop()
		{
			cancellationTokenSource.Cancel();
			workerThread.Join();
		}
	}
}
