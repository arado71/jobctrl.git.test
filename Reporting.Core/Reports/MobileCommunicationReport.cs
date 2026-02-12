using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Model.Mobile;

namespace Reporter.Reports
{
	public class MobileCommunicationReport : ICommunicationReport
	{
		private readonly IMobileDbContext context;
		private MobileUserPhoneBook phoneBook;

		public MobileCommunicationReport(IMobileDbContext context)
		{
			this.context = context;
		}

		public CommunicationReportResult GenerateReport(int[] userIds, DateTime startDate, DateTime endDate)
		{
			if (phoneBook == null) phoneBook = new MobileUserPhoneBook(context);
			var report = new MobilePhoneCallsReportCalculator(phoneBook);

			var phoneCalls = context.GetMobilePhoneCalls(userIds, startDate, endDate);

			report.GenerateForCalls(phoneCalls);

			Debug.WriteLine("Generated report for " + report.ReportableUsers.Count + " users with " + report.ReportableCalls.Select(n => n.Value).DefaultIfEmpty(TimeSpan.Zero).Sum(n => n.TotalHours) + " hours of calls.");

			var userLookup = report.ReportableUsers
				.ToDictionary(n => n.Key, n => (IUser)new CommunicationReportUser()
				{
					Name = n.Value.ToString(),
					UserId = n.Value.UserId.Value
				});

			var orderedUsers = userLookup.Values.OrderBy(n => n.Name).ToList();
			var calls = report.ReportableCalls
				.Select(n => (ICommunicationAmount)new CommunicationAmount()
				{
					From = userLookup[n.Key.Item1],
					To = userLookup[n.Key.Item2],
					Duration = n.Value
				})
				.ToList();

			return new CommunicationReportResult() { Users = orderedUsers, Outbound = calls };
		}
	}
}
