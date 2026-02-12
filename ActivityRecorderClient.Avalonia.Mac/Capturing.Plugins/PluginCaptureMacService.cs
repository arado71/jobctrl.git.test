using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	//dummy quick and dirty implementation
	public class PluginCaptureMacService : PluginCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly Dictionary<string, Func<PluginStartInfoDetails, ICaptureExtension>> knownPlugins = new Dictionary<string, Func<PluginStartInfoDetails, ICaptureExtension>>();
        protected override IEnumerable<PluginStartInfo> GetInternalCaptureExtensionSettings() => [];

		protected override ICaptureExtensionAdapter GetCaptureExtensionWithId(PluginStartInfo pluginStartInfo)
		{
			Func<PluginStartInfoDetails, ICaptureExtension> captureExtensionFactory;
			if (knownPlugins.TryGetValue(pluginStartInfo.PluginId, out captureExtensionFactory))
			{
				return new CaptureExtensionMacAdapter(captureExtensionFactory, pluginStartInfo);
			}
			log.Warn("Unable to find plugin " + pluginStartInfo);
			return null; //todo proper loading / searching for external plugins
		}

		static PluginCaptureMacService()
		{
			AddPlugin((p) => new PluginInternalWorkState());
		}

		private static void AddPlugin(Func<PluginStartInfoDetails, ICaptureExtension> captureExtensionFactory)
		{
			try
			{
				ICaptureExtension capExt;
				using ((capExt = captureExtensionFactory(null)) as IDisposable)
				{
					if (capExt == null || capExt.Id == null) throw new Exception("Null extension or Id");
					knownPlugins.Add(capExt.Id, captureExtensionFactory);
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to load plugin", ex);
			}
		}


	}
}