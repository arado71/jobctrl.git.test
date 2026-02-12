using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Mail
{
	/// <summary>
	/// Thread-safe wrapper class for reducing load on outlook (don't get captures every 300ms) and avoid blocking while communicating with external process.
	/// </summary>
	public class MailCaptureCachedWinManager : PeriodicManager, IMailCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IMailCaptureService mailCaptureService;
		private MailCaptures lastCapture;

		public MailCaptureCachedWinManager(IMailCaptureService mailCaptureService)
			: base(log, false)
		{
			this.mailCaptureService = mailCaptureService;
		}

		protected override void ManagerCallbackImpl()
		{
			var mailCaptures = mailCaptureService.GetMailCaptures();
			Interlocked.Exchange(ref lastCapture, mailCaptures);
		}

		protected override int ManagerCallbackInterval
		{
			get { return 1000; }
		}

		public override void Start(int first = 0)
		{
			mailCaptureService.Initialize();
			base.Start(first);
		}

		public override void Stop()
		{
			base.Stop();
			mailCaptureService.Dispose();
		}

		public void Dispose()
		{
			Stop();
		}

		public void Initialize()
		{
			Start();
		}

		public MailCaptures GetMailCaptures()
		{
			return Interlocked.CompareExchange(ref lastCapture, null, null);
		}
	}
}
