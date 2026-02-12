using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginEdgeBlink : PluginDomCaptureBase
	{
		public const string PluginId = "JobCTRL.EdgeBlink";

		public PluginEdgeBlink()
		{
			EdgeBlinkInstallHelper.InstallExtensionOneTimeIfApplicable();
			Id = PluginId;
		}

		private EdgeBlinkProxy c;
		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (domCaptureSettings.Count == 0) return null;
			if (!string.Equals(processName, "msedge.exe", StringComparison.OrdinalIgnoreCase)) return null;

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId);
			if (c == null) c = new EdgeBlinkProxy();
			var capt = c.Capture(hWnd, processId, domCaptureSettings);

			return capt;
		}
	}
}
