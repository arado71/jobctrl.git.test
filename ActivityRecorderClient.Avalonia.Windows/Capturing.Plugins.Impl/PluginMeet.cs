using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginMeet : PluginConferenceDomCaptureBase
	{
		public const string PluginId = "JobCTRL.Meet";

		protected override string ServiceName => "Meet";
		protected override string JsPartyName => "(function () { var tag = document.querySelector(\"div:nth-child(2) > div > div.YvoLGe > div.cSO8I.N4cbF > div.G3llDe.Dxboad > div\"); if (tag != null) return tag.innerText; else return 'n/a'; })()";
		protected override string JsPartyEmail => null;
		protected override string InCallUrl => "https://meet[.]google[.]com/[a-z-]+";

		public override string Id => PluginId;
	}
}
