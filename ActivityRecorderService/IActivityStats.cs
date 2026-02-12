using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderService.Stats;

namespace Tct.ActivityRecorderService
{
	[ServiceContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public interface IActivityStats
	{
#if LEGACY
		[OperationContract]
		DailyStats GetDailyStats();

		[OperationContract]
		DailyStats GetDailyStatsByUserId(int userId);

		[OperationContract]
		DailyStats GetDailyStatsByUserIds(List<int> userIds);

		[OperationContract]
		DailyStats GetDailyStatsByGroupId(int groupId);

		[OperationContract]
		DailyStats GetDailyStatsByCompanyId(int companyId);
#endif
		[OperationContract]
		void SendDailyEmails(DateTime date, List<int> userIds);

		[OperationContract]
		void SendWeeklyEmails(DateTime date, List<int> userIds);

		[OperationContract]
		void SendMonthlyEmails(DateTime date, List<int> userIds);

		[OperationContract]
		void SendDailyEmailsTo(DateTime date, List<int> userIds, List<string> toAddresses);

		[OperationContract]
		void SendWeeklyEmailsTo(DateTime date, List<int> userIds, List<string> toAddresses);

		[OperationContract]
		void SendMonthlyEmailsTo(DateTime date, List<int> userIds, List<string> toAddresses);

		[OperationContract]
		void SendProjectEmails(DateTime utcStartDate, DateTime utcEndDate, int reportUserId, bool isInternal, List<int> projectRootIds, List<string> toAddresses);
	}
}
