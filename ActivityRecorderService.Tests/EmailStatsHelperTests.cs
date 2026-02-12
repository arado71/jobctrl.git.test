using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Caching.Works;
using Tct.ActivityRecorderService.Collector;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class EmailStatsHelperTests : DbTestsBase //very incomplete
	{
		private readonly DateTime now = new DateTime(2010, 12, 06, 12, 00, 00);
		private readonly TimeZoneInfo localTimeZone = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague;Central Europe Standard Time;Central Europe Daylight Time;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");
		private readonly UserStatInfo userStatInfo;
		private readonly UserStatInfo userStatInfo2;
		private readonly Dictionary<int, UserStatInfo> userStatInfoDict;

		public EmailStatsHelperTests()
		{
			MobileDataClassesDataContext.DebugMobileWorkItems = new MobileWorkItem[0]; //we have to reset this every time so the order of the tests won't cause any trouble
			userStatInfo = new UserStatInfo() { Id = 1, Name = "Teszt user1", FirstName = "user1", LastName = "Teszt", StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = localTimeZone, CalendarId = 1, };
			userStatInfo2 = new UserStatInfo() { Id = 2, Name = "Teszt2", FirstName = "user2", LastName = "Teszt", StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = localTimeZone, CalendarId = 1, };
			userStatInfoDict = new Dictionary<int, UserStatInfo>() { { 1, userStatInfo }, { 2, userStatInfo2 } };
			WorkHierarchyServiceForTests.Reset();
		}

		[Fact]
		public void MonthlyReportWithOneDeletedIntervalWontThrow()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 1,
					StartDate = now,
					EndDate = now.AddHours(2),
				});
				context.SubmitChanges();
			}

			//Act
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Monthly, now.Date, userStatInfo);
			var emails = EmailStatsHelper.GetEmailsToSend(startEnd.StartDate, startEnd.EndDate, ReportType.Monthly, userStatInfoDict);

			//Assert
			Assert.Equal(1, emails.Count);
			Assert.Equal(0, emails[0].WorkTimes.Count);
			Assert.Equal(TimeSpan.Zero, emails[0].FullWorkTime.SumWorkTime);
			Assert.True(emails[0].Subject.EndsWith("(Össz.: 00:00:00)"));
		}

		[Fact]
		public void DailyReportWithOneDeletedIntervalWontThrow()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 1,
					StartDate = now,
					EndDate = now.AddHours(2),
				});
				context.SubmitChanges();
			}

			//Act
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, now.Date, userStatInfo);
			var emails = EmailStatsHelper.GetEmailsToSend(startEnd.StartDate, startEnd.EndDate, ReportType.Daily, userStatInfoDict);

			//Assert
			Assert.Equal(1, emails.Count);
			Assert.Equal(0, emails[0].WorkTimes.Count);
			Assert.Equal(TimeSpan.Zero, emails[0].FullWorkTime.SumWorkTime);
			Assert.True(emails[0].Subject.EndsWith("(Össz.: 00:00:00)"));
		}

		[Fact]
		public void MonthlyReportOnDSTStart()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 1,
					WorkId = 2,
					StartDate = new DateTime(2010, 03, 28, 01, 30, 00).FromLocalToUtc(localTimeZone),
					EndDate = new DateTime(2010, 03, 28, 03, 30, 00).FromLocalToUtc(localTimeZone),
				});
				context.SubmitChanges();
			}

			//Act
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Monthly, new DateTime(2010, 03, 28), userStatInfo);
			var emails = EmailStatsHelper.GetEmailsToSend(startEnd.StartDate, startEnd.EndDate, ReportType.Monthly, userStatInfoDict);

			//Assert
			Assert.Equal(TimeSpan.FromHours(1).Ticks, emails[0].WorkTimes.Values.Sum(n => n.SumWorkTime.Ticks));
			Assert.Equal(TimeSpan.FromHours(1), emails[0].FullWorkTime.SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(30), emails[0].WorkTimes[new DateTime(2010, 03, 27)].SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(30), emails[0].WorkTimes[new DateTime(2010, 03, 28)].SumWorkTime);
		}

		[Fact]
		public void MonthlyReportOnDSTEnd()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 1,
					WorkId = 2,
					StartDate = new DateTime(2010, 10, 31, 01, 30, 00).FromLocalToUtc(localTimeZone),
					EndDate = new DateTime(2010, 10, 31, 03, 30, 00).FromLocalToUtc(localTimeZone),
				});
				context.SubmitChanges();
			}

			//Act
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Monthly, new DateTime(2010, 10, 31), userStatInfo);
			var emails = EmailStatsHelper.GetEmailsToSend(startEnd.StartDate, startEnd.EndDate, ReportType.Monthly, userStatInfoDict);

			//Assert
			Assert.Equal(TimeSpan.FromHours(3).Ticks, emails[0].WorkTimes.Values.Sum(n => n.SumWorkTime.Ticks));
			Assert.Equal(TimeSpan.FromHours(3), emails[0].FullWorkTime.SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(150), emails[0].WorkTimes[new DateTime(2010, 10, 30)].SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(30), emails[0].WorkTimes[new DateTime(2010, 10, 31)].SumWorkTime);
		}

		[Fact]
		public void ManualIntervalAccrossDayChangeIsTruncated()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 1,
					WorkId = 2,
					StartDate = new DateTime(2011, 06, 01, 02, 30, 00).FromLocalToUtc(localTimeZone),
					EndDate = new DateTime(2011, 06, 01, 03, 40, 00).FromLocalToUtc(localTimeZone),
				});
				context.SubmitChanges();
			}
			WorkHierarchyServiceForTests.CurrentService =
				new WorkHierarchy(new JobControlDataClassesDataContext().GetWorks().ToDictionary(n => n.Id),
					new JobControlDataClassesDataContext().GetProjects().ToDictionary(n => n.Id));

			//Act
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Monthly, new DateTime(2011, 06, 01), userStatInfo);
			var emails = EmailStatsHelper.GetEmailsToSend(startEnd.StartDate, startEnd.EndDate, ReportType.Monthly, userStatInfoDict);

			var startEnd2 = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Monthly, new DateTime(2011, 05, 01), userStatInfo);
			var emails2 = EmailStatsHelper.GetEmailsToSend(startEnd2.StartDate, startEnd2.EndDate, ReportType.Monthly, userStatInfoDict);

			//Assert
			Assert.Equal(TimeSpan.FromMinutes(40).Ticks, emails[0].WorkTimes.Values.Sum(n => n.SumWorkTime.Ticks));
			Assert.Equal(TimeSpan.FromMinutes(40), emails[0].FullWorkTime.SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(40), emails[0].WorkTimes[new DateTime(2011, 06, 01)].SumWorkTime);
			Assert.True(Regex.IsMatch(emails[0].Body, @"sok\)\s+00:40:00"));

			Assert.Equal(TimeSpan.FromMinutes(30).Ticks, emails2[0].WorkTimes.Values.Sum(n => n.SumWorkTime.Ticks));
			Assert.Equal(TimeSpan.FromMinutes(30), emails2[0].FullWorkTime.SumWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(30), emails2[0].WorkTimes[new DateTime(2011, 05, 31)].SumWorkTime);
			Assert.True(Regex.IsMatch(emails2[0].Body, @"sok\)\s+00:30:00"));
		}

		private void InitComplexData()
		{
			#region InitData
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = new IvrDataClassesDataContext())
			{
				context.Calendars.InsertOnSubmit(new Tct.ActivityRecorderService.Calendar() { IsMondayWorkDay = true, IsTuesdayWorkDay = true, IsWednesdayWorkDay = true, IsThursdayWorkDay = true, IsFridayWorkDay = true, IsSaturdayWorkDay = true, IsSundayWorkDay = true });
				context.SubmitChanges();
			}

			MobileDataClassesDataContext.DebugMobileWorkItems = new[] {
				new MobileWorkItem () { Id = 1, UserId = 1, WorkId = 23, Imei= 3, StartDate = now.Date.AddDays(6).AddHours(10), EndDate = now.Date.AddDays(6).AddHours(14) }
			};

			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 1,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddHours(2),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 2,
					WorkId = 3,
					StartDate = now.AddDays(1),
					EndDate = now.AddDays(1).AddHours(1),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
					UserId = 2,
					WorkId = 4,
					StartDate = now.AddDays(2),
					EndDate = now.AddDays(2).AddHours(8),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
					UserId = 2,
					WorkId = 4,
					StartDate = now.AddDays(3),
					EndDate = now.AddDays(3).AddHours(8),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 1,
					WorkId = 5,
					StartDate = now.AddDays(-1),
					EndDate = now.AddDays(-1).AddHours(6),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 2,
					WorkId = 5,
					StartDate = now.AddDays(-2),
					EndDate = now.AddDays(-2).AddHours(1),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 1,
					WorkId = 3,
					StartDate = now.AddDays(6),
					EndDate = now.AddDays(6).AddHours(1),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 2,
					WorkId = 4,
					StartDate = now.AddDays(6),
					EndDate = now.AddDays(6).AddHours(1),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
					UserId = 2,
					WorkId = 2,
					StartDate = now.AddDays(6).AddHours(2),
					EndDate = now.AddDays(6).AddHours(3),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 2,
					StartDate = now.AddDays(6).AddHours(3),
					EndDate = now.AddDays(6).AddHours(4),
				});
				context.SubmitChanges();
			}

			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 2,
					CreateDate = now.AddDays(1).AddHours(1),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 2,
					CreateDate = now.AddDays(1).AddHours(1).AddMinutes(1),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA bela" }, { "Url", "about:blank" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 2,
					CreateDate = now.AddDays(1).AddHours(1).AddMinutes(2),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA bela" }, { "Url", "https://mail.google.com/mail/?shva=1&ert=e4#label/TcT/13282327b68fb8e3" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 2,
					CreateDate = now.AddDays(1).AddHours(1).AddMinutes(3),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA belas" }, { "Url", "https://mail.google.com/mail/?shva=1&ert=e4#label/TcT/13282327b68fb8e3" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 2,
					CreateDate = now.AddDays(1).AddHours(1).AddMinutes(4),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "valami.exe" }, { "Title", "" }, { "Url", "http://msdn.microsoft.com/en-us/library/ms186981.aspx" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 2,
					CreateDate = now.AddDays(1).AddHours(1).AddMinutes(5),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "valami.exe" }, { "Title", "Valami" }, { "Url", "http://msdn.microsoft.com/en-us/library/ms143432.aspx" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 2,
					CreateDate = now.AddDays(1).AddHours(1).AddMinutes(6),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "devenv.exe" }, { "Title", "ActivityRecorder" }, { "Url", "" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 2,
					CreateDate = now.AddDays(1).AddHours(1).AddMinutes(57),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "hax.exe" }, { "Title", "Pwned" }, { "Url", "" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 2,
					CreateDate = now.AddDays(1).AddHours(2),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA" }, { "Url", "" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 1,
					CreateDate = now.AddDays(2).AddHours(1),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 1,
					CreateDate = now.AddDays(2).AddHours(1).AddMinutes(1),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA bela" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 1,
					CreateDate = now.AddDays(2).AddHours(1).AddMinutes(2),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA bela" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 1,
					CreateDate = now.AddDays(2).AddHours(1).AddMinutes(3),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA belas" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 1,
					CreateDate = now.AddDays(2).AddHours(1).AddMinutes(4),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "valami.exe" }, { "Title", "" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 1,
					CreateDate = now.AddDays(2).AddHours(1).AddMinutes(5),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "valami.exe" }, { "Title", "Valami" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 1,
					CreateDate = now.AddDays(2).AddHours(1).AddMinutes(6),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "devenv.exe" }, { "Title", "ActivityRecorder - Majkremszaft Visual Studio" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 1,
					CreateDate = now.AddDays(2).AddHours(1).AddMinutes(7),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "hax.exe" }, { "Title", "Pwned" } },
				});
				CollectedItemDbHelper.Insert(new CollectedItem()
				{
					UserId = 1,
					CreateDate = now.AddDays(2).AddHours(1).AddMinutes(8),
					CapturedValues = new Dictionary<string, string> { { "ProcessName", "bela.exe" }, { "Title", "BELA" } },
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 2,
					WorkId = 5,
					StartDate = now.AddDays(1).AddHours(1),
					EndDate = now.AddDays(1).AddHours(2),
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 2,
					WorkId = 5,
					StartDate = now.AddDays(5).AddHours(1),
					EndDate = now.AddDays(5).AddHours(2),
					IsVirtualMachine = true,
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 2,
					WorkId = 5,
					StartDate = now.AddDays(6).AddHours(1),
					EndDate = now.AddDays(6).AddHours(2),
					IsRemoteDesktop = true,
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 2,
					WorkId = 2,
					StartDate = now.AddDays(6).AddHours(2.5),
					EndDate = now.AddDays(6).AddHours(3.5),
					ComputerId = 34,
					IsRemoteDesktop = true,
					IsVirtualMachine = true,
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 2,
					WorkId = 5,
					StartDate = now.AddDays(6).AddHours(2),
					EndDate = now.AddDays(6).AddHours(3),
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 2,
					WorkId = 5,
					StartDate = now.AddDays(6).AddHours(4),
					EndDate = now.AddDays(6).AddHours(5),
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 1,
					WorkId = 5,
					StartDate = now.AddDays(2).AddHours(1).AddMinutes(1),
					EndDate = now.AddDays(2).AddHours(2),
				});
				var wi = new WorkItem()
				{
					UserId = 1,
					WorkId = 5,
					StartDate = now.AddDays(2).AddHours(1),
					EndDate = now.AddDays(2).AddHours(2).AddMinutes(-1),
					ActiveWindows = new List<ActiveWindow>(),
				};
				for (int i = 0; i < 100; i++)
				{
					wi.ActiveWindows.Add(new ActiveWindow() { ProcessName = "bela.exe", Title = "BELA", CreateDate = now.AddDays(2).AddHours(1).AddMinutes(8) });
				}
				context.WorkItems.InsertOnSubmit(wi);
				context.SubmitChanges();
			}

			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
			}

			#endregion
		}

		[Fact]
		public void MonthlyAndDailyAggregateReportNotEmpty()
		{
			//Arrange
			InitComplexData();

			//Act
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Monthly, now.Date, userStatInfo);
			var emails = EmailStatsHelper.GetEmailsToSend(startEnd.StartDate, startEnd.EndDate, ReportType.Monthly, userStatInfoDict);
			var aggr = EmailStatsAggregateHelper.GetAggregateEmailToSend(startEnd.StartDate, startEnd.EndDate, ReportType.Monthly, emails.GroupBy(n => "dummy").First(), EmailStatsHelper.DefaultCulture);

			var dailyStartEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, now.Date.AddDays(6), userStatInfo);
			var dailyMail = EmailStatsHelper.GetEmailsToSend(dailyStartEnd.StartDate, dailyStartEnd.EndDate, ReportType.Daily, userStatInfoDict);
			var dailyAggr = EmailStatsAggregateHelper.GetAggregateEmailToSend(dailyStartEnd.StartDate, dailyStartEnd.EndDate, ReportType.Daily, dailyMail.GroupBy(n => "dummy").First(), EmailStatsHelper.DefaultCulture);

			//Assert
			Assert.NotNull(aggr);
			Assert.True(!string.IsNullOrEmpty(aggr.BodyHtml));
			Assert.NotNull(dailyAggr);
			Assert.True(emails[0].Body.Contains("devenv.exe"));
			Assert.True(emails[0].Body.Contains("Egyéb"));
			Assert.True(!emails[1].Body.Contains("Egyéb"));

			//Send it out for real
			////SendMail("ztorok@tct.hu", "TestEmail @ " + DateTime.Now.ToString(new System.Globalization.CultureInfo("hu-HU")) + " " + aggr.Subject, aggr.Body, aggr.BodyHtml, aggr.HtmlResources);
			//SendMail("ztorok@tct.hu", "TestEmail @ " + DateTime.Now.ToString(new System.Globalization.CultureInfo("hu-HU")) + " " + aggr.Subject, aggr.Body, aggr.BodyHtml, aggr.HtmlResources);
			//SendMail("ztorok@tct.hu", "TestEmail @ " + DateTime.Now.ToString(new System.Globalization.CultureInfo("hu-HU")) + " " + dailyAggr.Subject, dailyAggr.Body, dailyAggr.BodyHtml, dailyAggr.HtmlResources);
		}

		[Fact]
		public void ProjTest()
		{
			InitComplexData();

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Monthly, now.Date, userStatInfo);
			var cultures = new List<string> { "hu-hu", "en-us" };
			var projectLookup = StatsDbHelper.GetProjectsById().Values.ToLookup(n => n.ParentId);
			var a = EmailProjectStatsHelper.GetEmailToSend(startEnd.StartDate, startEnd.EndDate, 1, true, new List<int> { 1 }, userStatInfoDict, cultures, projectLookup);
			var b = EmailProjectStatsHelper.GetEmailToSend(startEnd.StartDate, startEnd.EndDate, 1, true, new List<int> { 2, 4 }, userStatInfoDict, cultures, projectLookup);
			var email = a.Values.First();
			//SendMail("ztorok@tct.hu", "TestEmail @ " + DateTime.Now.ToString(new System.Globalization.CultureInfo("hu-HU")) + " " + email.Subject, email.Body, email.BodyHtml);
		}

		[Fact]
		public void HolidayTest()
		{
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = new IvrDataClassesDataContext())
			{
				context.Calendars.InsertOnSubmit(new Tct.ActivityRecorderService.Calendar() { IsMondayWorkDay = true, IsTuesdayWorkDay = true, IsWednesdayWorkDay = true, IsThursdayWorkDay = true, IsFridayWorkDay = true, IsSaturdayWorkDay = true, IsSundayWorkDay = true });
				context.SubmitChanges();
			}

			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
					UserId = 1,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddHours(2),
				});
				context.SubmitChanges();
			}

			var dailyStartEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, now.Date, userStatInfo);
			var dailyMail = EmailStatsHelper.GetEmailsToSend(dailyStartEnd.StartDate, dailyStartEnd.EndDate, ReportType.Daily, userStatInfoDict)[0];

			//SendMail("ztorok@tct.hu", "TestEmail @ " + DateTime.Now.ToString(new CultureInfo("hu-HU")) + " " + dailyMail.Subject, dailyMail.Body, dailyMail.BodyHtml, dailyMail.HtmlResources);
			Assert.Equal(0, dailyMail.HtmlResources.Count);
		}


		[Fact]
		public void AddWorkTest()
		{
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = new IvrDataClassesDataContext())
			{
				context.Calendars.InsertOnSubmit(new Tct.ActivityRecorderService.Calendar() { IsMondayWorkDay = true, IsTuesdayWorkDay = true, IsWednesdayWorkDay = true, IsThursdayWorkDay = true, IsFridayWorkDay = true, IsSaturdayWorkDay = true, IsSundayWorkDay = true });
				context.SubmitChanges();
			}

			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 1,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddHours(2),
				});
				context.SubmitChanges();
			}

			var dailyStartEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, now.Date, userStatInfo);
			var dailyMail = EmailStatsHelper.GetEmailsToSend(dailyStartEnd.StartDate, dailyStartEnd.EndDate, ReportType.Daily, userStatInfoDict)[0];

			//SendMail("ztorok@tct.hu", "TestEmail @ " + DateTime.Now.ToString(new CultureInfo("hu-HU")) + " " + dailyMail.Subject, dailyMail.Body, dailyMail.BodyHtml, dailyMail.HtmlResources);
			Assert.Equal(1, dailyMail.HtmlResources.Count);
		}

		public static readonly string EmailSmtpHost = "smtp.gmail.com";
		public static readonly int EmailSmtpPort = 587;
		public static readonly bool EmailSsl = true;
		public static readonly string EmailFrom = "jobctrl@jobctrl.com";
		public static readonly string EmailUserName = "jobctrl@jobctrl.com";
		public static readonly string EmailPassword = "jobctrl789";
		private static void SendMail(string to, string subject, string plainBody, string htmlBody, List<EmailResource> htmlResources)
		{
			using (var message = new MailMessage())
			{
				message.From = new MailAddress(EmailFrom);
				message.Subject = subject;
				message.To.Add(new MailAddress(to));
				var plainView = AlternateView.CreateAlternateViewFromString(plainBody, Encoding.UTF8, "text/plain");
				var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
				if (htmlResources != null)
				{
					foreach (var resource in htmlResources)
					{
						var linkedResource = new LinkedResource(new MemoryStream(resource.Data, false), resource.MediaType)
						{
							ContentId = resource.ContentId
						};
						htmlView.LinkedResources.Add(linkedResource);
					}
				}
				message.AlternateViews.Add(plainView);
				message.AlternateViews.Add(htmlView);
				using (var emailClient = new SmtpClient(EmailSmtpHost, EmailSmtpPort))
				{
					emailClient.UseDefaultCredentials = false;
					emailClient.Credentials = new NetworkCredential(EmailUserName, EmailPassword);
					emailClient.EnableSsl = EmailSsl;
					emailClient.Send(message);
				}
			}
		}

		private static void SendMail(string to, string subject, string plainBody, string htmlBody)
		{
			SendMail(to, subject, plainBody, htmlBody, null);
		}
	}
}
