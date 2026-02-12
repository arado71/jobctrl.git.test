using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Reporter.Model.WorkItems;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Caching.Works;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;
using ManualWorkItem = Tct.ActivityRecorderService.ManualWorkItem;
using MobileWorkItem = Tct.ActivityRecorderService.MobileWorkItem;
using WorkItem = Tct.ActivityRecorderService.WorkItem;

namespace Tct.Tests.ActivityRecorderService
{
	public class ChartTests : DbTestsBase
	{
		private readonly TimeZoneInfo localTimeZone = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague;Central Europe Standard Time;Central Europe Daylight Time;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");
		private readonly UserStatInfo userStatInfo13;

		public ChartTests()
		{
			userStatInfo13 = new UserStatInfo() { Id = 13, Name = "Teszt user13", StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = localTimeZone };
			WorkHierarchyServiceForTests.CurrentService = WorkHierarchy.Empty;
		}

		[Fact]
		public void Chart3CompTest()
		{
			#region InitData
			var queryDate = new DateTime(2010, 09, 21);
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ExecuteCommand(TestDataCreator.WorkItemsForUser13Date20100921);
			}
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
			}
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 13,
					WorkId = 2,
					StartDate = queryDate.AddHours(9.5),
					EndDate = queryDate.AddHours(10),
				});
				context.SubmitChanges();
			}
			#endregion

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, queryDate, userStatInfo13);
			var intvalsByUser = StatsDbHelper.GetAggregateWorkItemIntervalsByUser(startEnd.StartDate, startEnd.EndDate);
			var workitemForUser = StatsDbHelper.GetWorkItemsForUser(13, startEnd.StartDate, startEnd.EndDate);
			var manualWorkItemsForUser = StatsDbHelper.GetManualWorkItemsByUser(startEnd.StartDate, startEnd.EndDate)[13];

			var works = new WorkHierarchy(new Dictionary<int, Work>() { { 1253, new Work() { Id = 1253, Name = "asdasd asd as dsa das das das dsadsadsasad sdas" } } }, new Dictionary<int, Project>());
			WorkHierarchyServiceForTests.CurrentService = works;

			var result = ChartHelper.CreateWorkIntervalsAndActivityChart(intvalsByUser.First().ToList(), workitemForUser, manualWorkItemsForUser.ToList(), startEnd.StartDate, startEnd.EndDate, userStatInfo13).ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp.png", result);
		}

		[Fact]
		public void Chart1CompTest()
		{
			#region InitData
			var queryDate = new DateTime(2010, 09, 07);
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ExecuteCommand(TestDataCreator.WorkItemsForUser13Date20100907);
			}
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
			}
			#endregion

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, queryDate, userStatInfo13);
			var intvalsByUser = StatsDbHelper.GetAggregateWorkItemIntervalsByUser(startEnd.StartDate, startEnd.EndDate);
			var workitemForUser = StatsDbHelper.GetWorkItemsForUser(13, startEnd.StartDate, startEnd.EndDate);
			var manualWorkItemsForUser = StatsDbHelper.GetManualWorkItemsByUser(startEnd.StartDate, startEnd.EndDate)[13];

			var result = ChartHelper.CreateWorkIntervalsAndActivityChart(intvalsByUser.First().ToList(), workitemForUser, manualWorkItemsForUser.ToList(), startEnd.StartDate, startEnd.EndDate, userStatInfo13).ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp2.png", result);
		}

		[Fact]
		public void Chart1CompTestWithDelete()
		{
			#region InitData
			var queryDate = new DateTime(2010, 09, 07);
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ExecuteCommand(TestDataCreator.WorkItemsForUser13Date20100907);
			}
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
			}

			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
					UserId = 13,
					StartDate = queryDate.AddHours(10.5),
					EndDate = queryDate.AddHours(11),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 13,
					StartDate = queryDate.AddHours(13),
					EndDate = queryDate.AddHours(13.5),
				});
				context.SubmitChanges();
			}
			#endregion

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, queryDate, userStatInfo13);
			var intvalsByUser = StatsDbHelper.GetAggregateWorkItemIntervalsByUser(startEnd.StartDate, startEnd.EndDate);
			var workitemForUser = StatsDbHelper.GetWorkItemsForUser(13, startEnd.StartDate, startEnd.EndDate);
			var manualWorkItemsForUser = StatsDbHelper.GetManualWorkItemsByUser(startEnd.StartDate, startEnd.EndDate)[13];

			var result = ChartHelper.CreateWorkIntervalsAndActivityChart(intvalsByUser.First().ToList(), workitemForUser, manualWorkItemsForUser.ToList(), startEnd.StartDate, startEnd.EndDate, userStatInfo13).ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp3.png", result);
		}

		[Fact]
		public void Chart1CompTestNewDate()
		{
			#region InitData
			var queryDate = new DateTime(2010, 09, 07);
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ExecuteCommand(TestDataCreator.WorkItemsForUser13Date20100907);
			}
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
			}
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 13,
					WorkId = 2,
					StartDate = queryDate.AddHours(19),
					EndDate = queryDate.AddHours(24),
				});
				context.SubmitChanges();
			}
			#endregion

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, queryDate, userStatInfo13);
			var intvalsByUser = StatsDbHelper.GetAggregateWorkItemIntervalsByUser(startEnd.StartDate, startEnd.EndDate);
			var workitemForUser = StatsDbHelper.GetWorkItemsForUser(13, startEnd.StartDate, startEnd.EndDate);
			var manualWorkItemsForUser = StatsDbHelper.GetManualWorkItemsByUser(startEnd.StartDate, startEnd.EndDate)[13];

			var resultChart = ChartHelper.CreateWorkIntervalsAndActivityChart(intvalsByUser.First().ToList(), workitemForUser, manualWorkItemsForUser.ToList(), startEnd.StartDate, startEnd.EndDate, userStatInfo13);
			var labels = resultChart.ChartAreas[1].AxisX.CustomLabels.AsEnumerable().Select(n => n.Text).ToArray();
			Assert.True(labels.Where(n => n.StartsWith(startEnd.StartDate.Date.ToString("d", new CultureInfo("hu-HU")))).Count() == 1);
			Assert.True(labels.Where(n => n.StartsWith(startEnd.EndDate.Date.ToString("d", new CultureInfo("hu-HU")))).Count() == 1);
			var result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp3.png", result);
		}

		[Fact]
		public void ChartPieTest()
		{
			var processedItems = new List<Reporter.Model.ProcessedItems.WorkItem>()
			{
				new Reporter.Model.ProcessedItems.WorkItem(new ComputerWorkItem()){
					Values = new Dictionary<string,string>()
					{
						{"ProcessName","devenv.exe"},
						{"Title","ActivityRecorder - Majkremszaft Visual Studio"},
					},
					Duration = TimeSpan.FromMinutes(10),
				},
				new Reporter.Model.ProcessedItems.WorkItem(new ComputerWorkItem()){
					Values = new Dictionary<string,string>()
					{
						{"ProcessName","totalcmd.exe"},
						{"Title","Total Commander - xye"},
					},
					Duration = TimeSpan.FromMinutes(5),
				},
				new Reporter.Model.ProcessedItems.WorkItem(new ComputerWorkItem()){
					Values = new Dictionary<string,string>()
					{
						{"ProcessName","firefox.exe"},
						{"Title","ASa asdsad asd as - Moz FF"},
					},
					Duration = TimeSpan.FromMinutes(2),
				},
				new Reporter.Model.ProcessedItems.WorkItem(new ComputerWorkItem()){
					Values = new Dictionary<string,string>()
					{
						{"ProcessName","1.exe"},
						{"Title","ASa asdsad asd as - Moz FF"},
					},
					Duration = TimeSpan.FromMinutes(1),
				},
				new Reporter.Model.ProcessedItems.WorkItem(new ComputerWorkItem()){
					Values = new Dictionary<string,string>()
					{
						{"ProcessName","2.exe"},
						{"Title","ASa asdsad asd as - Moz FF"},
					},
					Duration = TimeSpan.FromMinutes(1),
				},
				new Reporter.Model.ProcessedItems.WorkItem(new ComputerWorkItem()){
					Values = new Dictionary<string,string>()
					{
						{"ProcessName","3.exe"},
						{"Title","ASa asdsad asd as - Moz FF"},
					},
					Duration = TimeSpan.FromMinutes(1),
				},
				new Reporter.Model.ProcessedItems.WorkItem(new ComputerWorkItem()){
					Values = new Dictionary<string,string>()
					{
						{"ProcessName","4.exe"},
						{"Title","ASa asdsad asd as - Moz FF"},
					},
					Duration = TimeSpan.FromMinutes(1),
				},
				new Reporter.Model.ProcessedItems.WorkItem(new ComputerWorkItem()){
					Values = new Dictionary<string,string>()
					{
						{"ProcessName","5.exe"},
						{"Title","ASa asdsad asd as - Moz FF"},
					},
					Duration = TimeSpan.FromMinutes(1),
				},
				new Reporter.Model.ProcessedItems.WorkItem(new ComputerWorkItem()){
					Values = new Dictionary<string,string>()
					{
						{"ProcessName","6.exe"},
						{"Title","ASa asdsad asd as - Moz FF"},
					},
					Duration = TimeSpan.FromMinutes(1),
				},
			};
			var resultChart = ChartHelper.CreateProcessChart(processedItems, TimeSpan.FromHours(8));
			var result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp4.png", result);
		}

		[Fact]
		public void ChartSmallIntervalLocalTimeOk()
		{
			#region InitData
			var queryDate = new DateTime(2011, 07, 11);
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 13,
					WorkId = 2,
					StartDate = queryDate.AddHours(8).AddSeconds(5).AddTicks(435),
					EndDate = queryDate.AddHours(8).AddMinutes(10),
				});
				context.SubmitChanges();
			}
			#endregion

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, queryDate, userStatInfo13);
			var manualWorkItemsForUser = StatsDbHelper.GetManualWorkItemsByUser(startEnd.StartDate, startEnd.EndDate)[13];

			var resultChart = ChartHelper.CreateWorkIntervalsAndActivityChart(Enumerable.Empty<AggregateWorkItemInterval>().ToList(), Enumerable.Empty<WorkItem>().ToList(), manualWorkItemsForUser.ToList(), startEnd.StartDate, startEnd.EndDate, userStatInfo13);
			var labels = resultChart.ChartAreas[1].AxisX.CustomLabels.AsEnumerable().Select(n => n.Text).ToArray();
			Assert.True(labels.First() == manualWorkItemsForUser.First().StartDate.FromUtcToLocal(userStatInfo13.TimeZone).ToString("g", new CultureInfo(EmailStatsHelper.DefaultCulture)));
			Assert.True(labels.Count() > 1);
			var result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp5.png", result);
		}

		[Fact]
		public void ChartMeeting()
		{
			#region InitData
			var queryDate = new DateTime(2011, 07, 11);
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					SourceId = (byte)ManualWorkItemSourceEnum.MeetingAdd,
					UserId = 13,
					WorkId = 2,
					StartDate = queryDate.AddHours(8).AddSeconds(5).AddTicks(435),
					EndDate = queryDate.AddHours(8).AddMinutes(10),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 13,
					WorkId = 2,
					StartDate = queryDate.AddHours(8).AddMinutes(10),
					EndDate = queryDate.AddHours(8).AddMinutes(18).AddTicks(31),
				});
				context.SubmitChanges();
			}
			#endregion

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, queryDate, userStatInfo13);
			var manualWorkItemsForUser = StatsDbHelper.GetManualWorkItemsByUser(startEnd.StartDate, startEnd.EndDate)[13];
		
			var resultChart = ChartHelper.CreateWorkIntervalsAndActivityChart(Enumerable.Empty<AggregateWorkItemInterval>().ToList(), Enumerable.Empty<WorkItem>().ToList(), manualWorkItemsForUser.ToList(), startEnd.StartDate, startEnd.EndDate, userStatInfo13);
			var labels = resultChart.ChartAreas[1].AxisX.CustomLabels.AsEnumerable().Select(n => n.Text).ToArray();
			Assert.True(labels.First() == manualWorkItemsForUser.First().StartDate.FromUtcToLocal(userStatInfo13.TimeZone).ToString("g", new CultureInfo(EmailStatsHelper.DefaultCulture)));
			Assert.True(labels.Count() > 1);
			var result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp5.png", result);
		}

		[Fact]
		public void ChartVerySmallIntervalLocalTimeOk()
		{
			#region InitData
			var queryDate = new DateTime(2011, 07, 11);
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 13,
					WorkId = 2,
					StartDate = queryDate.AddHours(8).AddSeconds(5).AddTicks(435),
					EndDate = queryDate.AddHours(8).AddMinutes(1),
				});
				context.SubmitChanges();
			}
			#endregion

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, queryDate, userStatInfo13);
			var manualWorkItemsForUser = StatsDbHelper.GetManualWorkItemsByUser(startEnd.StartDate, startEnd.EndDate)[13];

			var resultChart = ChartHelper.CreateWorkIntervalsAndActivityChart(Enumerable.Empty<AggregateWorkItemInterval>().ToList(), Enumerable.Empty<WorkItem>().ToList(), manualWorkItemsForUser.ToList(), startEnd.StartDate, startEnd.EndDate, userStatInfo13);
			var labels = resultChart.ChartAreas[1].AxisX.CustomLabels.AsEnumerable().Select(n => n.Text).ToArray();
			Assert.True(labels.First() == manualWorkItemsForUser.First().StartDate.FromUtcToLocal(userStatInfo13.TimeZone).ToString("g", new CultureInfo(EmailStatsHelper.DefaultCulture)));
			Assert.True(labels.Count() > 1);
			var result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp5.png", result);
		}

		[Fact]
		public void ChartOneAndHalfHourIntervalHasAtLeastTwoLabels()
		{
			#region InitData
			var queryDate = new DateTime(2011, 07, 11);
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 13,
					WorkId = 2,
					StartDate = queryDate.AddHours(8).AddMinutes(10),
					EndDate = queryDate.AddHours(9).AddMinutes(40),
				});
				context.SubmitChanges();
			}
			#endregion

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, queryDate, userStatInfo13);
			var manualWorkItemsForUser = StatsDbHelper.GetManualWorkItemsByUser(startEnd.StartDate, startEnd.EndDate)[13];

			var resultChart = ChartHelper.CreateWorkIntervalsAndActivityChart(Enumerable.Empty<AggregateWorkItemInterval>().ToList(), Enumerable.Empty<WorkItem>().ToList(), manualWorkItemsForUser.ToList(), startEnd.StartDate, startEnd.EndDate, userStatInfo13);
			var labels = resultChart.ChartAreas[1].AxisX.CustomLabels.AsEnumerable().Select(n => n.Text).ToArray();
			Assert.True(labels.Count() > 1);
			var result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp5.png", result);
		}

		[Fact]
		public void Chart3CompAndMobileTest()
		{
			#region InitData
			var queryDate = new DateTime(2010, 09, 21);
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ExecuteCommand(TestDataCreator.WorkItemsForUser13Date20100921);
			}
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
			}
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 13,
					WorkId = 2,
					StartDate = queryDate.AddHours(9.5),
					EndDate = queryDate.AddHours(10),
				});
				context.SubmitChanges();
			}
			#endregion

			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, queryDate, userStatInfo13);
			var intvalsByUser = StatsDbHelper.GetAggregateWorkItemIntervalsByUser(startEnd.StartDate, startEnd.EndDate);
			var workitemForUser = StatsDbHelper.GetWorkItemsForUser(13, startEnd.StartDate, startEnd.EndDate);
			var manualWorkItemsForUser = StatsDbHelper.GetManualWorkItemsByUser(startEnd.StartDate, startEnd.EndDate)[13];

			var mobileWorkItems = new[] {
				new MobileWorkItem(){ Id = 1, UserId = 13, WorkId = 34, Imei = 2, StartDate = queryDate.AddHours(9.5), EndDate = queryDate.AddHours(10),}, 
				new MobileWorkItem(){ Id = 2, UserId = 13, WorkId = 36, Imei = 1, StartDate = queryDate.AddHours(10), EndDate = queryDate.AddHours(10.2),}, 
			};
			var works = new WorkHierarchy(new Dictionary<int, Work>() { { 1253, new Work() { Id = 1253, Name = "asdasd asd as dsa das das das dsadsadsasad sdas" } } }, new Dictionary<int, Project>());
			WorkHierarchyServiceForTests.CurrentService = works;

			var resultChart = ChartHelper.CreateWorkIntervalsAndActivityChart(intvalsByUser.First().ToList(), workitemForUser, manualWorkItemsForUser.ToList(), mobileWorkItems.ToList(), startEnd.StartDate, startEnd.EndDate, userStatInfo13);
			Assert.True(resultChart.ChartAreas[0].AxisX.CustomLabels.Select(n => n.Text).Contains(EmailStats.ChartAxisSmartphone));
			var result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp.png", result);
		}

		[Fact]
		public void ChartSimpleWorkWarnTest()
		{
			var now = new DateTime(2012, 02, 18);
			List<EmailStatsHelper.WorkWithProgress> workProgresses = new List<EmailStatsHelper.WorkWithProgress>()
			{
				new EmailStatsHelper.WorkWithProgress() { StartDate = now.AddDays(-1), EndDate = now.AddDays(-1), Now = now.AddDays(3), Name = "3 napos csuszas", TargetWorkTime = TimeSpan.FromHours(2), TotalWorkTime = TimeSpan.FromHours(4) },
				new EmailStatsHelper.WorkWithProgress() { StartDate = now.AddDays(-1), EndDate = now.AddDays(-1), Now = now.AddDays(2), Name = "2 napos csuszas", TargetWorkTime = TimeSpan.FromHours(2), TotalWorkTime = TimeSpan.FromHours(5) },
				new EmailStatsHelper.WorkWithProgress() { StartDate = now.AddDays(-1), EndDate = now.AddDays(-1), Now = now.AddDays(2.4), Name = "2.4 napos csuszas", TargetWorkTime = TimeSpan.FromHours(2), TotalWorkTime = TimeSpan.FromHours(2) },
				new EmailStatsHelper.WorkWithProgress() { StartDate = now.AddDays(-1), EndDate = now.AddDays(-1), Now = now.AddDays(2.5), Name = "2.5 napos csuszas", TargetWorkTime = TimeSpan.FromHours(2), TotalWorkTime = TimeSpan.FromHours(0) },
				new EmailStatsHelper.WorkWithProgress() { StartDate = now.AddDays(-1), EndDate = now.AddDays(-1), Now = now.AddDays(2.6), Name = "2.6 napos csuszas", TargetWorkTime = TimeSpan.FromHours(2), TotalWorkTime = TimeSpan.FromHours(2) },
				new EmailStatsHelper.WorkWithProgress() { StartDate = now.AddDays(-1), EndDate = now.AddDays(-1), Now = now.AddDays(4), Name = "4 napos csuszas", TargetWorkTime = TimeSpan.FromHours(2), TotalWorkTime = TimeSpan.FromHours(7) },
				new EmailStatsHelper.WorkWithProgress() { StartDate = now.AddDays(-1), EndDate = now.AddDays(-1), Now = now.AddDays(3.1), Name = "3.1 napos csuszas", TargetWorkTime = TimeSpan.FromHours(2), TotalWorkTime = TimeSpan.FromHours(1) },
				new EmailStatsHelper.WorkWithProgress() { StartDate = now.AddDays(-1), EndDate = now.AddDays(-1), Now = now.AddDays(7), Name = "7 napos csuszas", TargetWorkTime = TimeSpan.FromHours(2), TotalWorkTime = TimeSpan.FromHours(3) },
			};
			var resultChart = ChartHelper.CreateWorkWarnEndDateChart(workProgresses);
			var result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp4.png", result);

			resultChart = ChartHelper.CreateWorkWarnTargetTimeChart(workProgresses);
			result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp5.png", result);
		}

		[Fact]
		public void ChartWarnHeuristicTest()
		{
			var now = new DateTime(2012, 02, 18);
			List<EmailStatsHelper.WorkWithProgress> workProgresses = new List<EmailStatsHelper.WorkWithProgress>()
			{
				new EmailStatsHelper.WorkWithProgress() { 
					StartDate = now, 
					EndDate = now.AddDays(10), 
					Now = now.AddDays(9), 
					Name = "Problemas feladat 1", 
					TargetWorkTime = TimeSpan.FromHours(80), 
					TotalWorkTime = TimeSpan.FromHours(65), //64 should be done
					WorkingDaysCount = 10,
					WorkingDaysLeft = 2,
				},
				new EmailStatsHelper.WorkWithProgress() { 
					StartDate = now, 
					EndDate = now.AddDays(10), 
					Now = now.AddDays(9), 
					Name = "Problemas feladat 2", 
					TargetWorkTime = TimeSpan.FromHours(16), 
					TotalWorkTime = TimeSpan.FromHours(6),
					WorkingDaysCount = 10,
					WorkingDaysLeft = 2,
				},
			};
			var resultChart = ChartHelper.CreateWorkWarnHeuristic(workProgresses);
			var result = resultChart.ToPng();
			Assert.NotEmpty(result);
			//File.WriteAllBytes("c:\\temp6.png", result);
		}
	}
}
