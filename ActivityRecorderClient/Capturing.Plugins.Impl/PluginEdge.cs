using System;
using System.Collections.Generic;
using System.Linq;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Edge;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	//???//todo share proxy between all plugins?
	//???//todo handle more chrome windows/profiles (we can connect to only one)
	//???//todo handle Chrome extension uninstall if JC is uninstalled
	//???//todo fix disable/enable addon will mess up eventPassing so it won't work on existing tabs anymore
	public class PluginEdge : PluginDomCaptureBase
	{
		public const string PluginId = "JobCTRL.Edge";

        public PluginEdge()
		{
			EdgeInstallHelper.InstallExtensionOneTimeIfApplicable();
			Id = PluginId;
		}

		private EdgeProxy c;
		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (domCaptureSettings.Count == 0) return null;
            if (!string.Equals(processName, "microsoftedge.exe", StringComparison.OrdinalIgnoreCase)) return null;

			AppVersionLogger.LogAssemblyVersionEdge(hWnd);
			if (c == null) c = new EdgeProxy();
			var capt = c.Capture(hWnd, domCaptureSettings);
            
		    return capt;
		}
	}
}
