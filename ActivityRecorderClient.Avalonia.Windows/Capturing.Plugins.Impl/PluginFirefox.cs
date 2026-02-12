using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Firefox;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	//todo more robust socket port handling (or don't use sockets at all ?)
	//todo share proxy between all plugins?
	//todo handle more firefox windows/profiles (we can connect to only one)
	//todo handle ff uninstall if JC is uninstalled
	//todo fix disable/enable addon will mess up eventPassing so it won't work on existing tabs anymore
	public class PluginFirefox : PluginDomCaptureBase, IDisposable
	{
		public const string PluginId = "JobCTRL.FF";

		public PluginFirefox()
		{
			Id = PluginId;
			FirefoxInstallHelper.InstallAddonOneTimeIfApplicable();
		}

		private IFirefoxProxy c;
		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (domCaptureSettings.Count == 0) return null;
			if (!string.Equals(processName, "firefox.exe", StringComparison.OrdinalIgnoreCase)) return null;

			AppVersionLogger.LogAssemblyVersionFromProcId((uint) processId);
			if (c == null) c = new FirefoxProxy();
			return c.Capture(hWnd, domCaptureSettings);
		}

		public void Dispose()
		{
			if (c != null) c.Dispose();
		}
	}
}
