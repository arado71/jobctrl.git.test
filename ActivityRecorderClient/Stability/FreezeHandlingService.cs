using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ClientErrorReporting;
using Tct.ActivityRecorderClient.Controller;

namespace Tct.ActivityRecorderClient.Stability
{
	public abstract class FreezeHandlingService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly SynchronizationContext context;
		private readonly CurrentWorkController currentWorkController;
		private HandlingStatus status = HandlingStatus.NoReport;

		// States: NoReport -> SendingReport -> ReportSent -> RestartPending
		private enum HandlingStatus
		{
			NoReport,
			SendingReport,
			ReportSent,
			RestartPending,
		}

		protected FreezeHandlingService(SynchronizationContext guiThread, CurrentWorkController currentWorkController)
		{
			context = guiThread;
			this.currentWorkController = currentWorkController;
		}

		public void HandleFreeze(Thread frozenThread, string stack)
		{
			DebugEx.EnsureGuiThread();
			switch (status)
			{
				case HandlingStatus.NoReport:
					status = HandlingStatus.SendingReport;
					log.Debug("Sending error report for freeze");
					ThreadPool.QueueUserWorkItem(__ =>
					{
						var sendResult = SendReport(frozenThread.Name, stack);
						context.Post(___ =>
						{
							log.Debug("Freeze error report " + (sendResult ? " sent successfully" : " sending failed"));
							status = HandlingStatus.ReportSent;
							HandleFreeze(frozenThread, stack);
						}, null);
					});
					break;
				case HandlingStatus.SendingReport:
					log.Debug("Thread " + frozenThread.Name + "freeze handling in progress...");
					break;
				case HandlingStatus.ReportSent:
					if (!currentWorkController.IsWorking && currentWorkController.MutualWorkTypeCoordinator.IsWorking)
					{
						log.Debug("Skipping thread " + frozenThread.Name + " freeze handling because of non-standard working");
						return;
					}

					log.Debug("Restarting application");
					status = HandlingStatus.RestartPending;
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

					break;
				case HandlingStatus.RestartPending:
					log.Debug("Thread " + frozenThread.Name + "freeze restart in progress...");
					break;
				default:
					Debug.Fail("Unkown FreezeHandlingService state");
					break;
			}
		}

		private bool SendReport(string threadName, string stackTrace)
		{
			DebugEx.EnsureBgThread();
			var reporter = Platform.Factory.GetErrorReporter();
			var errorDescription = "Thread " + threadName + " frozen";
			if (!string.IsNullOrEmpty(stackTrace))
			{
				errorDescription += " with stack:" + Environment.NewLine + stackTrace;
			}

			return reporter.ReportClientError(errorDescription, false);
		}

		protected abstract void EmergencyRestart(int? workId);
	}
}
