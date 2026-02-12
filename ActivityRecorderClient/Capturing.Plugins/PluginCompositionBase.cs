using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobCTRL.Plugins;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public abstract class PluginCompositionBase: ICaptureExtension, IDisposable
	{
		private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();
		private static IPluginCaptureService pluginCaptureService;
		private ICaptureExtensionAdapter[] plugins;

		public abstract string[] InnerPluginIds { get; }

		public abstract string Id { get; }

		public IEnumerable<string> GetParameterNames()
		{
			return parameters.Keys;
		}

		public void SetParameter(string name, string value)
		{
			parameters[name] = value;
			if (plugins == null) return;
			foreach (var plugin in plugins)
			{
				plugin.SetParameter(name, value);
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield break;
		}

		public virtual IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (plugins == null)
			{
				var service = pluginCaptureService as PluginCaptureService;
				plugins = service?.GetCaptureExtensionsFromSettingsNoThrow(InnerPluginIds.Select(e => new PluginStartInfo { PluginId = e, Parameters = parameters.Select(p => new ActivityRecorderServiceReference.ExtensionRuleParameter { Name = p.Key, Value = p.Value }).ToList() }).ToList());
			}
			return plugins?.Select(plugin => plugin.Capture(hWnd, processId, processName)).FirstOrDefault(res => res != null);
		}

		public static void SetPluginCaptureService(IPluginCaptureService pluginCaptureSvc)
		{
			pluginCaptureService = pluginCaptureSvc;
		}

		public void Dispose()
		{
			if (plugins == null) return;
			foreach (var plugin in plugins)
			{
				plugin.Dispose();
			}
		}
	}
}
