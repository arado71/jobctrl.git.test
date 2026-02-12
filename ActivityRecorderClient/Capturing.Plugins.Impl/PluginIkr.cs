using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginIkr : ICaptureExtension
	{
		private readonly PluginMdiClient mdiClient = new PluginMdiClient();

		public string Id
		{
			get { return "JobCTRL.IKR"; }
		}

		public PluginIkr()
		{
			mdiClient = new PluginMdiClient();
			mdiClient.SetParameter("ProcessName", "ikr.exe");
		}

		public IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public void SetParameter(string name, string value)
		{
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			return mdiClient.GetCapturableKeys();
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			return mdiClient.Capture(hWnd, processId, processName);
		}
	}
}
