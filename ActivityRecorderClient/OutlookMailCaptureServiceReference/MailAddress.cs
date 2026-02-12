using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference
{
	public partial class MailAddress
	{
		public override string ToString()
		{
			return Name + " <" + Email + ">";
		}
	}
}
