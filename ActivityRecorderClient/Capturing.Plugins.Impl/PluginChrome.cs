using System;
using System.Collections.Generic;
using System.Linq;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	//???//todo share proxy between all plugins?
	//???//todo handle more chrome windows/profiles (we can connect to only one)
	//???//todo handle Chrome extension uninstall if JC is uninstalled
	//???//todo fix disable/enable addon will mess up eventPassing so it won't work on existing tabs anymore
	public class PluginChrome : PluginDomCaptureBase
	{
		public const string PluginId = "JobCTRL.Chrome";

		public PluginChrome()
		{
			ChromeInstallHelper.InstallExtensionOneTimeIfApplicable();
			Id = PluginId;
		}

		private ChromeProxy c;
		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (domCaptureSettings.Count == 0) return null;
			if (!string.Equals(processName, "chrome.exe", StringComparison.OrdinalIgnoreCase)) return null;

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId);
			if (c == null) c = new ChromeProxy();
			var capt = c.Capture(hWnd, processId, domCaptureSettings);
            
		    return capt;
		}
	}
}
