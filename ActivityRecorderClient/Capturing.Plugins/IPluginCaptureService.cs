using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public interface IPluginCaptureService : IDisposable
	{
		void SetCaptureExtensions(List<DesktopWindow> windowsInfo, Func<DesktopWindow, bool> shouldCaptureWindow, Dictionary<string, string> globalVariables );
		void LoadCaptureExtensionsFromWorkDetectorRulesAsync(WorkDetectorRule[] rules);
		void LoadCaptureExtensionsFromCollectorRulesAsync(CollectorRule[] rules);
		void RegisterCaptureExtensionSettings(PluginStartInfo settings);
		void UnregisterCaptureExtensionSettings(PluginStartInfo settings);
	}
}
