using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Mail;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginLotusNotes : PluginMailBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string PluginId = "JobCTRL.LotusNotes";

		public PluginLotusNotes()
			: base(new MailCaptureCachedWinManager(new LotusNotesMailCaptureWinService()), log)
		{
		}

		public override string Id
		{
			get { return PluginId; }
		}
	}
}
