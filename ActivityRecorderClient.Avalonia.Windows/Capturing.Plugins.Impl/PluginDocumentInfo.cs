using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginDocumentInfo : PluginOfficeBase
	{
		private List<PluginOfficeBase> plugins = new List<PluginOfficeBase>();

		public PluginDocumentInfo()
		{
			Id = "JobCTRL.DocumentInfo";

			plugins.Add(new PluginWord());
			plugins.Add(new PluginExcel());
			plugins.Add(new PluginPowerPoint());
			// TODO: mac
			//plugins.Add(new PluginAcrobat());
			//plugins.Add(new PluginAutoCad());
		}

		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			foreach (var plugin in plugins)
			{
				var res = plugin.Capture(hWnd, processId, processName);
				if (res != null) return res;
			}

			return null;
		}
	}
}
