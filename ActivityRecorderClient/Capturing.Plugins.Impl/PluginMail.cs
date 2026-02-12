using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Mail;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginMail : PluginCompositionBase
	{
		public const int CaptureCachingDurationInSeconds = 5;
		public const string PluginId = "JobCTRL.Mail";

		public override string[] InnerPluginIds => new[] { PluginOutlook.PluginId, PluginLotusNotes.PluginId, PluginGmail.PluginId };
		public override string Id => PluginId;

		public PluginMail()
		{
		}

	}
}
