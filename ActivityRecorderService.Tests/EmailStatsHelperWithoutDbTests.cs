using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class EmailStatsHelperWithoutDbTests
	{
		private readonly TimeZoneInfo localTimeZone = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague;Central Europe Standard Time;Central Europe Daylight Time;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");
		private readonly UserStatInfo userStatInfo13;
		private readonly DateTime localDate;
		private readonly DateTime now = new DateTime(2011, 07, 28, 10, 00, 00);

		public EmailStatsHelperWithoutDbTests()
		{
			userStatInfo13 = new UserStatInfo() { Id = 13, Name = "Teszt user13", StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = localTimeZone };
			localDate = CalculatorHelper.GetLocalReportDate(now, userStatInfo13.TimeZone, userStatInfo13.StartOfDayOffset);
		}

		[Fact]
		public void ObfuscateWorkTime()
		{
			//Arrange
			const string obsHM = "**:**";
			const string obsHMS = "**:**:**";

			var tests = new[] {
				new { Input = (string)null, Expected = (string)null },
				new { Input = "", Expected = "" },
				new { Input = "23:23", Expected = obsHM },
				new { Input = "234:23", Expected = obsHM },
				new { Input = "234:23:34", Expected = obsHMS },
				new { Input = "345234:23:34", Expected = obsHMS },
				new { Input = "23:63", Expected = "23:63" },
				new { Input = "23:12:63", Expected = obsHM + ":63" },
				new { Input = "23:\r\n34", Expected = "23:\r\n34" },
				new { Input = "23:43\r\n34:42:43", Expected = obsHM + "\r\n" + obsHMS },
				new { Input = "60:60", Expected = "60:60" },
				new { Input = "0:60", Expected = "0:60" },
				new { Input = "0:59", Expected = "0:59" },
				new { Input = "00:59", Expected = obsHM },
				new { Input = TimeSpan.FromMinutes(59).ToHourMinuteString(), Expected = obsHM },
				new { Input = TimeSpan.FromMinutes(4353459).ToHourMinuteString(), Expected = obsHM },
				new { Input = TimeSpan.FromMinutes(59).ToHourMinuteSecondString(), Expected = obsHMS },
				new { Input = TimeSpan.FromMinutes(4353459).ToHourMinuteSecondString(), Expected = obsHMS },
				new { Input = ">23:23<", Expected = ">" + obsHM + "<" },
				new { Input = ">234:23<", Expected = ">" + obsHM + "<" },
				new { Input = ">234:23:34<", Expected = ">" + obsHMS + "<" },
				new { Input = ">345234:23:34<", Expected = ">" + obsHMS + "<" },
			};

			//Act

			//Assert
			foreach (var test in tests)
			{
				Assert.Equal(test.Expected, EmailStatsHelper.ObfuscateWorkTimes(test.Input));
			}
		}

		[Fact]
		public void GetLocalDaysForIntervalDay()
		{
			//Arrange
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13);

			//Act
			var result = EmailStatsHelper.GetLocalDaysForInterval(userStatInfo13, startEnd.StartDate, startEnd.EndDate);

			//Assert
			var expected = new[] { localDate };
			Assert.True(expected.SequenceEqual(result));
		}

		[Fact]
		public void GetLocalDaysForIntervalWeek()
		{
			//Arrange
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Weekly, localDate, userStatInfo13);

			//Act
			var result = EmailStatsHelper.GetLocalDaysForInterval(userStatInfo13, startEnd.StartDate, startEnd.EndDate);

			//Assert
			List<DateTime> expected = new List<DateTime>();
			for (int i = 0; i < 7; i++)
			{
				expected.Add(localDate.AddDays(-3 + i));
			}
			Assert.True(expected.SequenceEqual(result));
		}

		[Fact]
		public void GetLocalDaysForIntervalMonth()
		{
			//Arrange
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Monthly, localDate, userStatInfo13);

			//Act
			var result = EmailStatsHelper.GetLocalDaysForInterval(userStatInfo13, startEnd.StartDate, startEnd.EndDate);

			//Assert
			List<DateTime> expected = new List<DateTime>();
			for (int i = 0; i < 31; i++)
			{
				expected.Add(localDate.AddDays(-27 + i));
			}
			Assert.True(expected.SequenceEqual(result));
		}

		[Fact]
		public void GetAggregateEmailGroupsForOneRequestWithSortKeyCheck()
		{
			//Arrange
			var emails = new[] { 
				new EmailToSend() { UserId= 1, SortKey = "a" },
				new EmailToSend() { UserId= 2, SortKey = "c" },
				new EmailToSend() { UserId= 3, SortKey = "b" },
			};
			var req = new[]{
				new AggregateEmailRequest() { ReportId = 1, UserIds =new[] {1,2,3}.ToList(), EmailsTo =new[] {"q@q.q","w@w.w"}.Select(n => new EmailTarget{Address = n, CultureId = EmailStatsHelper.DefaultCulture}).ToList(), Frequency = ReportFrequency.Daily },
			};

			//Act
			var result = EmailStatsAggregateHelper.GetAggregateEmailGroups(emails, ReportType.Daily, req).ToList();

			//Assert
			Assert.Equal(1, result.Count);
			Assert.True(req[0].UserIds.OrderBy(n => n).SequenceEqual(result[0].EmailsToAggregate.Select(n => n.UserId).OrderBy(n => n)));
			Assert.True(emails.OrderBy(n => n.SortKey).Select(n => n.UserId).SequenceEqual(result[0].EmailsToAggregate.Select(n => n.UserId)));
			Assert.True(req[0].EmailsTo.Select(n => n.Address).OrderBy(n => n).SequenceEqual(result[0].EmailsTo.Select(n => n.Address).OrderBy(n => n)));
		}

		[Fact]
		public void GetAggregateEmailGroupsDifferentFreqency()
		{
			//Arrange
			var emails = new[] { 
				new EmailToSend() { UserId= 1, SortKey = "a" },
				new EmailToSend() { UserId= 2, SortKey = "c" },
				new EmailToSend() { UserId= 3, SortKey = "b" },
			};
			var req = new[]{
				new AggregateEmailRequest() { ReportId = 1, UserIds =new[] {1,2,3}.ToList(), EmailsTo =new[] {"q@q.q","w@w.w"}.Select(n => new EmailTarget{Address = n,CultureId = EmailStatsHelper.DefaultCulture}).ToList(), Frequency = ReportFrequency.Daily },
			};

			//Act
			var result = EmailStatsAggregateHelper.GetAggregateEmailGroups(emails, ReportType.Monthly, req).ToList();

			//Assert
			Assert.Equal(0, result.Count);
		}

		[Fact]
		public void GetAggregateEmailGroupsForTwoRequestsMerged()
		{
			//Arrange
			var emails = new[] { 
				new EmailToSend() { UserId= 1, SortKey = "a" },
				new EmailToSend() { UserId= 2, SortKey = "c" },
				new EmailToSend() { UserId= 3, SortKey = "b" },
			};
			var req = new[]{
				new AggregateEmailRequest() { ReportId = 1, UserIds =new[] {1,2,3}.ToList(), EmailsTo =new[] {"q@q.q"}.Select(n => new EmailTarget{Address = n, CultureId = EmailStatsHelper.DefaultCulture}).ToList(), Frequency = ReportFrequency.Daily },
				new AggregateEmailRequest() { ReportId = 2, UserIds =new[] {1,2,3}.ToList(), EmailsTo =new[] {"w@w.w"}.Select(n => new EmailTarget{Address = n, CultureId = EmailStatsHelper.DefaultCulture}).ToList(), Frequency = ReportFrequency.Daily },
			};

			//Act
			var result = EmailStatsAggregateHelper.GetAggregateEmailGroups(emails, ReportType.Daily, req).ToList();

			//Assert
			Assert.Equal(1, result.Count);
			Assert.True(req[0].UserIds.OrderBy(n => n).SequenceEqual(result[0].EmailsToAggregate.Select(n => n.UserId).OrderBy(n => n)));
			Assert.True(emails.OrderBy(n => n.SortKey).Select(n => n.UserId).SequenceEqual(result[0].EmailsToAggregate.Select(n => n.UserId)));
			Assert.True(req.Select(n => n.EmailsTo.Select(m => m.Address)).SelectMany(n => n).OrderBy(n => n).SequenceEqual(result[0].EmailsTo.Select(n => n.Address).OrderBy(n => n)));
		}

		[Fact]
		public void GetAggregateEmailGroupsForTwoRequestsMergedSubsetUserIds()
		{
			//Arrange
			var emails = new[] { 
				new EmailToSend() { UserId= 1, SortKey = "a" },
				new EmailToSend() { UserId= 2, SortKey = "c" },
				new EmailToSend() { UserId= 3, SortKey = "b" },
			};
			var req = new[]{
				new AggregateEmailRequest() { ReportId = 1, UserIds =new[] {1,2,5}.ToList(), EmailsTo =new[] {"q@q.q"}.Select(n => new EmailTarget{Address = n, CultureId = EmailStatsHelper.DefaultCulture}).ToList(), Frequency = ReportFrequency.Daily | ReportFrequency.Monthly },
				new AggregateEmailRequest() { ReportId = 2, UserIds =new[] {1,2,6,45}.ToList(), EmailsTo =new[] {"w@w.w"}.Select(n => new EmailTarget{Address = n, CultureId = EmailStatsHelper.DefaultCulture}).ToList(), Frequency = ReportFrequency.Daily | ReportFrequency.Weekly },
			};

			//Act
			var result = EmailStatsAggregateHelper.GetAggregateEmailGroups(emails, ReportType.Daily, req).ToList();

			//Assert
			Assert.Equal(1, result.Count);
			Assert.True(new[] { 1, 2 }.SequenceEqual(result[0].EmailsToAggregate.Select(n => n.UserId).OrderBy(n => n)));
			Assert.True(new[] { 1, 2 }.SequenceEqual(result[0].EmailsToAggregate.Select(n => n.UserId)));
			Assert.True(req.Select(n => n.EmailsTo.Select(m => m.Address)).SelectMany(n => n).OrderBy(n => n).SequenceEqual(result[0].EmailsTo.Select(n => n.Address).OrderBy(n => n)));
		}

		[Fact]
		public void GetAggregateEmailGroupsForTwoRequestsDifferentUserIds()
		{
			//Arrange
			var emails = new[] { 
				new EmailToSend() { UserId= 1, SortKey = "a" },
				new EmailToSend() { UserId= 2, SortKey = "c" },
				new EmailToSend() { UserId= 3, SortKey = "b" },
			};
			var req = new[]{
				new AggregateEmailRequest() { ReportId = 1, UserIds =new[] {1,2}.ToList(), EmailsTo =new[] {"q@q.q"}.Select(n => new EmailTarget{Address = n, CultureId = EmailStatsHelper.DefaultCulture}).ToList(), Frequency = ReportFrequency.Daily | ReportFrequency.Monthly },
				new AggregateEmailRequest() { ReportId = 2, UserIds =new[] {2,3}.ToList(), EmailsTo =new[] {"w@w.w"}.Select(n => new EmailTarget{Address = n, CultureId = EmailStatsHelper.DefaultCulture}).ToList(), Frequency = ReportFrequency.Daily | ReportFrequency.Weekly },
			};

			//Act
			var result = EmailStatsAggregateHelper.GetAggregateEmailGroups(emails, ReportType.Daily, req).ToList();

			//Assert
			Assert.Equal(2, result.Count);
			Assert.True(new[] { 1, 2 }.SequenceEqual(result.Where(n => n.EmailsTo[0].Address == "q@q.q").Single().EmailsToAggregate.Select(n => n.UserId)));
			Assert.True(new[] { 3, 2 }.SequenceEqual(result.Where(n => n.EmailsTo[0].Address == "w@w.w").Single().EmailsToAggregate.Select(n => n.UserId)));
		}
	}
}
