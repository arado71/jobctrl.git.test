using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Controller;

namespace Tct.ActivityRecorderClient.Stability
{
	public abstract class NetworkStabilityManager : PeriodicManager
	{
		protected static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly CurrentWorkController currentWorkController;

		public static bool CheckConnectivity()
		{
			try
			{
				using (var wc = new WebClient())
				{
					wc.DownloadString("http://google.com");
					log.Debug("HTTP Get request OK to google.com");
					return true;
				}
			}
			catch (Exception e)
			{
				log.Debug("HTTP Get request FAILED to google.com");
				log.Debug(e);
				return false;
			}
		}

		public NetworkStabilityManager(CurrentWorkController currentWorkController)
			: base(log)
		{
			this.currentWorkController = currentWorkController;
		}

		private int numOfOfflineChecks = 0;

		protected override void ManagerCallbackImpl()
		{
			if (currentWorkController.IsOnline)
			{
				numOfOfflineChecks = 0;
				return;
			}
			numOfOfflineChecks++;
			if (numOfOfflineChecks >= 5)
			{
				if (CheckConnectivity())
				{
					numOfOfflineChecks = 0;
					return;
				}
				var connStat = new CheckConnectionExternally().Ready();
				if (!connStat)
				{
					numOfOfflineChecks = 0;
					return;
				}
				if (currentWorkController.IsWorking)
				{
					var hasLastUserSelectedWork = currentWorkController != null
												  && currentWorkController.LastUserSelectedOrPermWork != null
												  && currentWorkController.LastUserSelectedOrPermWork.Id != null;
					var workId = hasLastUserSelectedWork ? currentWorkController.LastUserSelectedOrPermWork.Id : null;
					EmergencyRestart(workId);
				}
				else
				{
					EmergencyRestart(null);
				}
			}
		}

		internal class CheckConnectionExternally
		{
			private readonly Process shadowProcess;
			private volatile bool ready;
			private readonly ManualResetEvent mreStopped = new ManualResetEvent(false);
			public bool Ready()
			{
				mreStopped.WaitOne(10000);
				return ready;
			}

			public CheckConnectionExternally()
			{
				log.Info("Shadow Process started");
				var startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location)
				{
					Arguments = "CheckConnectivity",
					UseShellExecute = false
				};
				shadowProcess = Process.Start(startInfo);
				if (shadowProcess != null)
				{
					shadowProcess.EnableRaisingEvents = true;
					shadowProcess.Exited += P_Exited;
				}
			}
			private void P_Exited(object sender, EventArgs e)
			{
				log.Info("Process Exited w/ code " + shadowProcess.ExitCode);
				ready = shadowProcess.ExitCode == 0;
				mreStopped.Set();
			}
		}
		protected override int ManagerCallbackInterval
		{
			get { return 30 * 1000; }
		}

		protected abstract void EmergencyRestart(int? workId);
	}
}
