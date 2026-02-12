using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Tct.ActivityRecorderService.WebsiteServiceReference
{
	public partial class PendingMeeting
	{
		public string OrganizerName
		{
			get
			{
				string namePattern = CultureInfo.CurrentCulture.LCID == 1038 ? "{1} {0}" : "{0} {1}";	//TODO: Get name order for CultureInfo.
				return string.Format(namePattern, OrganizerFirstName, OrganizerLastName);
			}
		}
	}
}
