using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public abstract class PluginDomCaptureBase : ICaptureExtension
	{
		public const string ParamDomCapture = "DomCapture";
		public string Id { get; protected set; }
		
		protected readonly List<DomSettings> domCaptureSettings = new List<DomSettings>();
		public virtual IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public virtual void SetParameter(string name, string value)
		{
			if (string.Equals(name, ParamDomCapture, StringComparison.OrdinalIgnoreCase))
			{
				domCaptureSettings.Clear();
				List<DomSettings> settings;
				JsonHelper.DeserializeData(value, out settings);
				SetDomCaptures(settings);
			}
		}

		public virtual void SetDomCaptures(List<DomSettings> settings)
		{
			foreach (var setting in settings)
			{
				if (setting.CheckValidAndInitialize())
				{
					domCaptureSettings.Add(setting);
				}
			}
		}

		public virtual IEnumerable<string> GetCapturableKeys()
		{
            foreach (var setting in domCaptureSettings)
            {
                yield return setting.Key;
            }
		}

		public abstract IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName);

    }
}
