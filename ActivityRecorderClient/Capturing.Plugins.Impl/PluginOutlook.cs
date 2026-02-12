using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Mail;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginOutlook : PluginMailBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string PluginId = "JobCTRL.Outlook";

		public PluginOutlook()
			: base(new MailCaptureCachedWinManager(new OutlookMailCaptureWinService()), log)
		{
		}

		public override string Id
		{
			get { return PluginId; }
		}
	}
}
