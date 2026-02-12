using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Mail
{
	public interface IMailCaptureService : IDisposable
	{
		void Initialize();
		MailCaptures GetMailCaptures();
	}
}
