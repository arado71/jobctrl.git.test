using System;

namespace Reporter.Reports
{
	public interface ICommunicationReport
	{
		CommunicationReportResult GenerateReport(int[] userIds, DateTime startDate, DateTime endDate);
	}
}