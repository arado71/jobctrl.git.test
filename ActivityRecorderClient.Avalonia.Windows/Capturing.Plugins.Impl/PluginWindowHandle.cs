using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginWindowHandle : ICaptureExtension
	{
		public const string PluginId = "JobCTRL.Handle";
		public const string KeyHandle = "Handle";

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
			yield return KeyHandle;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			return new Dictionary<string, string>(1)
			{
				{KeyHandle, hWnd.ToString()}
			};
		}
	}
}
