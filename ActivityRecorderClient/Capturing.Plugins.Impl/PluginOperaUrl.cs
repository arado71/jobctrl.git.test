using System;
using System.Collections.Generic;
using System.Linq;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginOperaUrl : PluginChromiumUrl
	{
		private const Browser BROWSER = Browser.Opera;
		public static string PluginId => "Internal." + BROWSER + "Url";
		public override string Id => PluginId;
		public override Browser Browser => BROWSER;
	}
}
