using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace OutlookMeetingCaptureService
{
	[ServiceContract]
	public interface IMeetingCaptureService
	{
		[OperationContract]
		string GetVersionInfo(bool useRedemption);

		[OperationContract]
		List<FinishedMeetingEntry> CaptureMeetings(IList<string> calendarAccountEmails, DateTime startDate, DateTime endDate, int manualWorkItemEditAgeLimit, bool needNonMeetingAppointments, bool needUpdatesDeletes, string folderInclusionPattern, string folderExclusionPattern, bool needTentativeMeetings, bool useRedemption, int delayedDeleteIntervalInMins);

		[OperationContract]
		List<MeetingAttendee> DisplaySelectNamesDialog(IntPtr parentWindowHandle);

		[OperationContract]
		void StopService();
	}
}
