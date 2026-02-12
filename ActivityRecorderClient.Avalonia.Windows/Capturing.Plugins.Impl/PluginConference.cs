using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginConference : PluginCompositionBase
	{
		public const string PluginId = "JobCTRL.Conference";
		public const string KeyConferenceService = "ConferenceService";
		public const string KeyConferenceState = "ConferenceState";
		public const string KeyConferenceTime = "ConferenceTime";
		public const string KeyConferencePartyName = "ConferencePartyName";
		public const string KeyConferencePartyEmail = "ConferencePartyEmail";
		public const string InCallStateName = "InCall";

		public override string[] InnerPluginIds => new[] { PluginMsTeams.PluginId, PluginMeet.PluginId };
		public override string Id => PluginId;
	}
}
