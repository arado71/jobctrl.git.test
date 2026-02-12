using System;
using System.Collections.Generic;
using System.ServiceModel;
using OutlookInteropService;

namespace LotusNotesMeetingCaptureServiceNamespace
{
	[ServiceContract]
	public interface IMeetingCaptureService
	{
		[OperationContract]
		void Initialize(string password, string mailServer, string mailFile);

		[OperationContract]
		string GetVersionInfo();

		[OperationContract]
		List<FinishedMeetingEntry> CaptureMeetings(IList<string> calendarAccountEmails, DateTime startDate, DateTime endDate);

		[OperationContract]
		MailCaptures GetMailCaptures();

		[OperationContract]
		void StopService();
	}
}
