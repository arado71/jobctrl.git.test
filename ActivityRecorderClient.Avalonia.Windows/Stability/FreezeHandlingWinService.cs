using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Stability
{
	public class FreezeHandlingWinService : FreezeHandlingService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		private readonly ActivityRecorderForm form;

		public FreezeHandlingWinService(SynchronizationContext guiThread, ActivityRecorderForm form) : base(guiThread, form.CurrentWorkController)
		{
			this.form = form;
		}

		protected override void EmergencyRestart(int? workId)
		{
			DebugEx.EnsureGuiThread();
			var workIdParameter = workId != null ? " " + workId.Value : "";
			// TODO: mac
			//AppControlServiceHelper.UnregisterProcess();
			log.InfoFormat("Restarting application with parameters: EmergencyRestart{0}", workIdParameter);
			var startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location) { Arguments = "EmergencyRestart " + ConfigManager.CurrentProcessPid + workIdParameter, UseShellExecute = false };
			Process.Start(startInfo);
			Environment.Exit(0);
		}
	}
}
