using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobCTRL.Plugins;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public abstract class PluginConferenceDomCaptureBase : ICaptureExtension, IDisposable
	{
		private PluginDomCapture domCapture;
		protected abstract string ServiceName { get; }
		protected abstract string JsPartyName { get; }
		protected abstract string JsPartyEmail { get; }
		protected abstract string InCallUrl { get; }

		public abstract string Id { get; }

		protected PluginConferenceDomCaptureBase()
		{
			Initialize();
		}

		private void Initialize()
		{
			var settings = new List<DomSettings>();
			if (!string.IsNullOrEmpty(JsPartyName))
				settings.Add(new DomSettings
					{
						Key = PluginConference.KeyConferencePartyName,
						EvalString = JsPartyName,
						UrlPattern = InCallUrl
					}
				);
			
			if (!string.IsNullOrEmpty(JsPartyEmail))
				settings.Add(new DomSettings
					{
						Key = PluginConference.KeyConferencePartyEmail,
						EvalString = JsPartyEmail,
						UrlPattern = InCallUrl
					}
				);
			domCapture = new PluginDomCapture();
			domCapture.SetDomCaptures(settings);
		}


		public IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public void SetParameter(string name, string value)
		{
			domCapture.SetParameter(name, value);
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return PluginConference.KeyConferenceService;
			yield return PluginConference.KeyConferenceState;
			yield return PluginConference.KeyConferencePartyName;
			yield return PluginConference.KeyConferencePartyEmail;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			var res = domCapture.Capture(hWnd, processId, processName);
			if (res == null || !res.Any()) return null;
			return res.Concat(new[] { new KeyValuePair<string, string>(PluginConference.KeyConferenceService, ServiceName), new KeyValuePair<string, string>(PluginConference.KeyConferenceState, PluginConference.InCallStateName), });
		}

		public void Dispose()
		{
			domCapture?.Dispose();
		}
	}
}
