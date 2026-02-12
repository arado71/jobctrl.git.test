using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Controller;

namespace Tct.ActivityRecorderClient.Stability
{
	class NetworkStabilityWinManager: NetworkStabilityManager
	{
		public NetworkStabilityWinManager(CurrentWorkController currentWorkController) : base(currentWorkController) { }

		protected override void EmergencyRestart(int? workId)
		{
			//DebugEx.EnsureGuiThread();
			AppControlServiceHelper.UnregisterProcess();
			var workIdParameter = workId != null ? " " + workId.Value : "";
			log.InfoFormat("Restarting application with parameters: EmergencyRestart{0}", workIdParameter);
			var startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location) { Arguments = "EmergencyRestart " + ConfigManager.CurrentProcessPid + workIdParameter, UseShellExecute = false };
			Process.Start(startInfo);
			((Platform.PlatformWinFactory)Platform.Factory).MainForm.HideIcon();
			Environment.Exit(0);
		}
	}
}
