using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Model;
using Reporter.Model.Email;
using Reporter.Model.ProcessedItems;
using Reporter.Processing;

namespace Reporter.Reports
{
	public class EmailCommunicationReport : ICommunicationReport
	{
		private readonly IEmailDbContext context;
		private EmailAddressBook addressBook;

		public EmailCommunicationReport(IEmailDbContext context)
		{
			this.context = context;
		}

		public CommunicationReportResult GenerateReport(int[] userIds, DateTime startDate, DateTime endDate)
		{
			var queryResult = new QueryResult
			{
				CollectedItems = context.GetCollectedItems(userIds, startDate, endDate),
				ManualWorkItems = context.GetDeletions(userIds, startDate, endDate),
				WorkItems = context.GetWorkItems(userIds, startDate, endDate)
			};
			var unpivoted = ReportHelper.Transform(queryResult.CalculateNet()).ToLookup(x => Device.FromProcessedItem(x));
			if (addressBook == null) addressBook = new EmailAddressBook(context, userIds);
			var emails = EmailCalculator.GetEmails(unpivoted.Select(x => new KeyValuePair<Device, List<PcWorkItem>>(x.Key, x.OfType<PcWorkItem>().ToList())).ToDictionary(k => k.Key, v => v.Value));
			addressBook.GuessAddresses(emails);
			var report = new EmailCalculator(addressBook);

			report.Generate(emails);

			Debug.WriteLine("Generated report for " + report.Users.Count + " users with " + (report.Inbound.Select(n => n.Value).DefaultIfEmpty(TimeSpan.Zero).Sum(n => n.TotalHours) + report.Outbound.Select(n => n.Value).DefaultIfEmpty(TimeSpan.Zero).Sum(n => n.TotalHours)) + " hours of calls.");
			var c = -1;
			var userLookup = report.Users.ToDictionary(x => x,
				y => (IUser) new CommunicationReportUser() { Name = y.ToString(), UserId = y.Id ?? c-- });
			var orderedUsers = userLookup.Values.OrderBy(x => x.Name).ToList();
			var outbound = report.Outbound
				.Select(n => (ICommunicationAmount)new CommunicationAmount()
				{
					From = userLookup[n.Key.Item1],
					To = userLookup[n.Key.Item2],
					Duration = n.Value
				})
				.ToList();
			var inbound = report.Inbound
				.Select(n => (ICommunicationAmount)new CommunicationAmount()
				{
					From = userLookup[n.Key.Item1],
					To = userLookup[n.Key.Item2],
					Duration = n.Value
				})
				.ToList();

			return new CommunicationReportResult() { Users = orderedUsers, Outbound = outbound, Inbound = inbound };
		}
	}
}
