using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Mail;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginMsTeams : PluginConferenceDomCaptureBase
	{
		public const string PluginId = "JobCTRL.MsTeams";

		protected override string ServiceName => "MsTeams";
		protected override string JsPartyName => "document.querySelector(\"div.call-state-layer > div.state-description-text\").innerText";
		protected override string JsPartyEmail => "document.querySelector(\"div.wrapper > profile-picture > img\").getAttribute(\"upn\")";
		protected override string InCallUrl => "https://teams[.]microsoft[.]com/_#/calling.*";

		public override string Id => PluginId;
	}
}
