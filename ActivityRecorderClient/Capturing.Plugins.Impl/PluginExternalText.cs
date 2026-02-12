using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginExternalText : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "JobCTRL.ExternalText";
		private const string KeyExternalText = "ExternalText";
		private readonly IWindowExternalTextHelper externalTextHelper;

		public PluginExternalText(IWindowExternalTextHelper externalTextHelper)
		{
			this.externalTextHelper = externalTextHelper;
		}

		public string Id
		{
			get { return PluginId; }
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
			yield return KeyExternalText;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			yield return new KeyValuePair<string, string>(KeyExternalText, externalTextHelper.GetTextByWindow(hWnd));
		}
	}
}
