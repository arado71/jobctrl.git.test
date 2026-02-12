using System;
using System.Collections.Generic;

namespace Tct.ActivityRecorderService.MeetingSync
{
	public interface IGoogleCalendarSource
	{
		List<FinishedMeetingEntry> GetEvents(int userId, ref string syncToken, DateTime eventsAfter, bool needTentativeMeetings);
	}
}