using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.OutlookMeetingCaptureServiceReference
{
	partial class FinishedMeetingEntry
	{
		public override string ToString()
		{
			string shortDateStart = StartTime == null ? "N/A" : StartTime.ToShortDateString();
			string shortTimeStart = StartTime == null ? "N/A" : StartTime.ToShortTimeString();
			string shortTimeEnd = EndTime == null ? "N/A" : EndTime.ToShortTimeString();
			string attendees = Attendees == null ? "N/A" : String.Join(", ", Attendees.Select(a => a.ToString()).ToArray());
			return String.Format("{0} ({1} {2} - {3}) ({4}) [{5}]", Title, shortDateStart, shortTimeStart, shortTimeEnd, CreationTime, attendees);
		}
	}
}
