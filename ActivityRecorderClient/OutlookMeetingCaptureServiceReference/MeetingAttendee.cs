using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.OutlookMeetingCaptureServiceReference
{
	partial class MeetingAttendee
	{
		public override string ToString()
		{
			return Type == MeetingAttendeeType.Organizer ? Email.ToUpper() : Email.ToLower();
		}
	}
}
