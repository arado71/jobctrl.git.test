using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Meeting
{
	public interface IMeetingCaptureService : IDisposable
	{
		void Initialize();
		List<FinishedMeetingEntry> CaptureMeetings(IList<string> calendarAccountEmails, DateTime startDate, DateTime endDate);
		string GetVersionInfo();
		string[] ProcessNames { get; }
	}
}
