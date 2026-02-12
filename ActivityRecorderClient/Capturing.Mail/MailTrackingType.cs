using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Mail
{
	public enum MailTrackingType
	{
		Disable = 0,
		BodyAndSubject = 1,
		BodyOnly = 2
	}

	[Flags]
	public enum MailTrackingSettings
	{
		None = 0,
		ReadId = 1,
		WriteIdToMail = 2,
		ShowPopupWindow = 4,
		All = ~0
	}
}
