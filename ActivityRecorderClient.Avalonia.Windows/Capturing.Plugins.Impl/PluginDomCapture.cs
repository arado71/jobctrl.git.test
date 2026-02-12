using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using System.Text.RegularExpressions;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
    public class PluginDomCapture : PluginCompositionBase
    {
	    public const string PluginId = "JobCTRL.DomCapture";

	    public override string[] InnerPluginIds => new[] { PluginChrome.PluginId, PluginFirefox.PluginId, PluginInternetExplorer.PluginId, PluginEdge.PluginId, PluginEdgeBlink.PluginId };
	    public override string Id => PluginId;

		public PluginDomCapture()
        {
        }

        public void SetDomCaptures(List<DomSettings> settings)
        {
	        var paramValue = JsonHelper.SerializeData(settings);
	        SetParameter(PluginDomCaptureBase.ParamDomCapture, paramValue);
        }

	    public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
        {
	        return ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableDomCapture) ? 
				new KeyValuePair<string, string>[0] : base.Capture(hWnd, processId, processName);
        }

    }
}
