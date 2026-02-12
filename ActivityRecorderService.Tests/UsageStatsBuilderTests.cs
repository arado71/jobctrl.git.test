using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.UsageStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class UsageStatsBuilderTests
	{
		private readonly TimeZoneInfo localTimeZone = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague;Central Europe Standard Time;Central Europe Daylight Time;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");
		private readonly UserStatInfo userStatInfo13;
		private readonly DateTime now = new DateTime(2011, 07, 18, 09, 00, 00);
		private readonly DateTime localDate;

		public UsageStatsBuilderTests()
		{
			userStatInfo13 = new UserStatInfo() { Id = 13, Name = "Teszt user13", StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = localTimeZone };
			localDate = CalculatorHelper.GetLocalReportDate(now, userStatInfo13.TimeZone, userStatInfo13.StartOfDayOffset);
		}

		[Fact]
		public void CreateWithEmpty()
		{
			var builder = new UsageStatsBuilder(Enumerable.Empty<UsageStat>(), userStatInfo13);
			Assert.Empty(builder.GetUsageStats());
		}

		[Fact]
		public void AddOneInterval()
		{
			//Arrange
			var builder = new UsageStatsBuilder(Enumerable.Empty<UsageStat>(), userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddHours(2) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(1, result.Count);
			Assert.Equal(0, result[0].Id);
			Assert.Equal(TimeSpan.FromHours(2), result[0].ComputerWorkTime);
			Assert.Equal(localDate, result[0].LocalDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, result[0].StartDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, result[0].EndDate);
		}

		[Fact]
		public void AddOneIntervalWithDayChange()
		{
			//Arrange
			var builder = new UsageStatsBuilder(Enumerable.Empty<UsageStat>(), userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddDays(2) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(3, result.Count);
			Assert.Equal(0, result[0].Id);
			Assert.Equal(0, result[1].Id);
			Assert.Equal(0, result[2].Id);
			Assert.Equal(TimeSpan.FromHours(16), result[0].ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(24), result[1].ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(8), result[2].ComputerWorkTime);
			Assert.Equal(localDate, result[0].LocalDate);
			Assert.Equal(localDate.AddDays(1), result[1].LocalDate);
			Assert.Equal(localDate.AddDays(2), result[2].LocalDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, result[0].StartDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, result[0].EndDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).StartDate, result[1].StartDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).EndDate, result[1].EndDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(2), userStatInfo13).StartDate, result[2].StartDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(2), userStatInfo13).EndDate, result[2].EndDate);
		}

		[Fact]
		public void AddTwoIntervalsSameDay()
		{
			//Arrange
			var builder = new UsageStatsBuilder(Enumerable.Empty<UsageStat>(), userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddHours(1) });
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now.AddHours(1), EndDate = now.AddHours(2) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(1, result.Count);
			Assert.Equal(0, result[0].Id);
			Assert.Equal(TimeSpan.FromHours(2), result[0].ComputerWorkTime);
			Assert.Equal(localDate, result[0].LocalDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, result[0].StartDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, result[0].EndDate);
		}

		[Fact]
		public void AddTwoIntervalsSameDayReverse()
		{
			//Arrange
			var builder = new UsageStatsBuilder(Enumerable.Empty<UsageStat>(), userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now.AddHours(1), EndDate = now.AddHours(2) });
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddHours(1) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(1, result.Count);
			Assert.Equal(0, result[0].Id);
			Assert.Equal(TimeSpan.FromHours(2), result[0].ComputerWorkTime);
			Assert.Equal(localDate, result[0].LocalDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, result[0].StartDate);
			Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, result[0].EndDate);
		}

		[Fact]
		public void AddThreeIntervalsDifferentDays()
		{
			foreach (var intervals in new[] {
				new AggregateWorkItemInterval() { StartDate = now.AddDays(0), EndDate = now.AddDays(0).AddHours(1) },
				new AggregateWorkItemInterval() { StartDate = now.AddDays(1), EndDate = now.AddDays(1).AddHours(2) },
				new AggregateWorkItemInterval() { StartDate = now.AddDays(2), EndDate = now.AddDays(2).AddHours(3) },
			}.Permute())
			{
				//Arrange
				var builder = new UsageStatsBuilder(Enumerable.Empty<UsageStat>(), userStatInfo13);

				//Act
				foreach (var interval in intervals)
				{
					builder.AddAggregateWorkItemInterval(interval);
				}

				//Assert
				var result = builder.GetUsageStats().ToList();
				Assert.Equal(3, result.Count);
				Assert.Equal(0, result[0].Id);
				Assert.Equal(0, result[1].Id);
				Assert.Equal(0, result[2].Id);
				Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate).Single().ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(2), result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(3), result.Where(n => n.LocalDate == localDate.AddDays(2)).Single().ComputerWorkTime);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(0), userStatInfo13).StartDate, result.Where(n => n.LocalDate == localDate.AddDays(0)).Single().StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(0), userStatInfo13).EndDate, result.Where(n => n.LocalDate == localDate.AddDays(0)).Single().EndDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).StartDate, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).EndDate, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().EndDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(2), userStatInfo13).StartDate, result.Where(n => n.LocalDate == localDate.AddDays(2)).Single().StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(2), userStatInfo13).EndDate, result.Where(n => n.LocalDate == localDate.AddDays(2)).Single().EndDate);
			}
		}

		[Fact]
		public void AddIntervalIntoSmallGapWithEndDateOtherDay()
		{
			//Arrange
			var builder = new UsageStatsBuilder(new[] {  
				new UsageStat() { Id = 1, LocalDate = localDate.AddDays(-1), StartDate = localDate.AddDays(-3), EndDate = now.AddHours(-1)} ,
				new UsageStat() { Id = 2, LocalDate = localDate.AddDays(1), StartDate = now.AddHours(1), EndDate = localDate.AddDays(3).AddTicks(1)} ,
				}, userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddHours(2) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(3, result.Count);
			Assert.Equal(0, result.Where(n => n.LocalDate == localDate).Single().Id);
			Assert.Equal(1, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().Id);
			Assert.Equal(2, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().Id);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().ComputerWorkTime);
			Assert.Equal(now.AddHours(-1), result.Where(n => n.LocalDate == localDate).Single().StartDate);
			Assert.Equal(now.AddHours(1), result.Where(n => n.LocalDate == localDate).Single().EndDate);
		}

		[Fact]
		public void AddIntervalIntoSmallGapWithStartDateOtherDay()
		{
			//Arrange
			var builder = new UsageStatsBuilder(new[] {  
				new UsageStat() { Id = 1, LocalDate = localDate.AddDays(-1), StartDate = localDate.AddDays(-3), EndDate = now.AddHours(1)} ,
				new UsageStat() { Id = 2, LocalDate = localDate.AddDays(1), StartDate = now.AddHours(2), EndDate = localDate.AddDays(3).AddTicks(1)} ,
				}, userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddHours(2) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(3, result.Count);
			Assert.Equal(0, result.Where(n => n.LocalDate == localDate).Single().Id);
			Assert.Equal(1, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().Id);
			Assert.Equal(2, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().Id);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().ComputerWorkTime);
			Assert.Equal(now.AddHours(1), result.Where(n => n.LocalDate == localDate).Single().StartDate);
			Assert.Equal(now.AddHours(2), result.Where(n => n.LocalDate == localDate).Single().EndDate);
		}

		[Fact]
		public void AddIntervalIntoSmallGapWithBothEndOtherDay()
		{
			//Arrange
			var builder = new UsageStatsBuilder(new[] {  
				new UsageStat() { Id = 1, LocalDate = localDate.AddDays(-1), StartDate = localDate.AddDays(-3), EndDate = now.AddHours(1)} ,
				new UsageStat() { Id = 2, LocalDate = localDate.AddDays(1), StartDate = now.AddHours(2), EndDate = localDate.AddDays(3).AddTicks(1)} ,
				}, userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddHours(3) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(3, result.Count);
			Assert.Equal(0, result.Where(n => n.LocalDate == localDate).Single().Id);
			Assert.Equal(1, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().Id);
			Assert.Equal(2, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().Id);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().ComputerWorkTime);
			Assert.Equal(now.AddHours(1), result.Where(n => n.LocalDate == localDate).Single().StartDate);
			Assert.Equal(now.AddHours(2), result.Where(n => n.LocalDate == localDate).Single().EndDate);
		}

		[Fact]
		public void AddIntervalIntoSmallGapWithEndDateTrunc()
		{
			//Arrange
			var builder = new UsageStatsBuilder(new[] {  
				new UsageStat() { Id = 1, LocalDate = localDate.AddDays(-1), StartDate = localDate.AddDays(-3), EndDate = now.AddDays(-3).AddHours(1)} ,
				new UsageStat() { Id = 4, LocalDate = localDate, StartDate = now.AddHours(-1), EndDate = now.AddHours(1)} ,
				new UsageStat() { Id = 2, LocalDate = localDate.AddDays(1), StartDate = now.AddDays(3), EndDate = localDate.AddDays(3).AddTicks(1)} ,
				}, userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddHours(2) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(3, result.Count);
			Assert.Equal(4, result.Where(n => n.LocalDate == localDate).Single().Id);
			Assert.Equal(1, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().Id);
			Assert.Equal(2, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().Id);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().ComputerWorkTime);
			Assert.Equal(now.AddHours(-1), result.Where(n => n.LocalDate == localDate).Single().StartDate);
			Assert.Equal(now.AddHours(1), result.Where(n => n.LocalDate == localDate).Single().EndDate);
		}

		[Fact]
		public void AddIntervalIntoSmallGapWithStartDateTrunc()
		{
			//Arrange
			var builder = new UsageStatsBuilder(new[] {  
				new UsageStat() { Id = 1, LocalDate = localDate.AddDays(-1), StartDate = localDate.AddDays(-3), EndDate = now.AddDays(-3).AddHours(1)} ,
				new UsageStat() { Id = 4, LocalDate = localDate, StartDate = now.AddHours(1), EndDate = now.AddHours(2)} ,
				new UsageStat() { Id = 2, LocalDate = localDate.AddDays(1), StartDate = localDate.AddDays(3), EndDate = localDate.AddDays(3).AddTicks(1)} ,
				}, userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddHours(2) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(3, result.Count);
			Assert.Equal(4, result.Where(n => n.LocalDate == localDate).Single().Id);
			Assert.Equal(1, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().Id);
			Assert.Equal(2, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().Id);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().ComputerWorkTime);
			Assert.Equal(now.AddHours(1), result.Where(n => n.LocalDate == localDate).Single().StartDate);
			Assert.Equal(now.AddHours(2), result.Where(n => n.LocalDate == localDate).Single().EndDate);
		}

		[Fact]
		public void AddIntervalIntoSmallGapWithBothEndTrunc()
		{
			//Arrange
			var builder = new UsageStatsBuilder(new[] {  
				new UsageStat() { Id = 1, LocalDate = localDate.AddDays(-1), StartDate = localDate.AddDays(-3), EndDate = now.AddDays(-3).AddHours(1)} ,
				new UsageStat() { Id = 4, LocalDate = localDate, StartDate = now.AddHours(1), EndDate = now.AddHours(2)} ,
				new UsageStat() { Id = 2, LocalDate = localDate.AddDays(1), StartDate = localDate.AddDays(3), EndDate = localDate.AddDays(3).AddTicks(1)} ,
				}, userStatInfo13);

			//Act
			builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now, EndDate = now.AddHours(3) });

			//Assert
			var result = builder.GetUsageStats().ToList();
			Assert.Equal(3, result.Count);
			Assert.Equal(4, result.Where(n => n.LocalDate == localDate).Single().Id);
			Assert.Equal(1, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().Id);
			Assert.Equal(2, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().Id);
			Assert.Equal(TimeSpan.FromHours(1), result.Where(n => n.LocalDate == localDate).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.Where(n => n.LocalDate == localDate.AddDays(-1)).Single().ComputerWorkTime);
			Assert.Equal(TimeSpan.Zero, result.Where(n => n.LocalDate == localDate.AddDays(1)).Single().ComputerWorkTime);
			Assert.Equal(now.AddHours(1), result.Where(n => n.LocalDate == localDate).Single().StartDate);
			Assert.Equal(now.AddHours(2), result.Where(n => n.LocalDate == localDate).Single().EndDate);
		}

		[Fact]
		public void NoOverlapsOrGapsFor365Days()
		{
			//Arrange
			var builder = new UsageStatsBuilder(Enumerable.Empty<UsageStat>(), userStatInfo13);
			for (int i = 0; i < 365; i++)
			{
				builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = now.AddDays(i), EndDate = now.AddDays(i).AddHours(3) });
			}

			//Act
			var result = builder.GetUsageStats().ToDictionary(n => n.LocalDate);

			//Assert
			for (int i = 0; i < 364; i++)
			{
				Assert.Equal(result[now.Date.AddDays(i)].EndDate, result[now.Date.AddDays(i + 1)].StartDate);
			}
		}

		[Fact]
		public void GeneratedRandomDataWontCauseOverlaps()
		{
			//Arrange
			var rnd = new Random(0);
			int iter = 2000;

			var builder = new UsageStatsBuilder(Enumerable.Empty<UsageStat>(), userStatInfo13);

			while (iter-- >= 0)
			{
				builder = new UsageStatsBuilder(builder.GetUsageStats(), GetRandomUserInfo(TestDataCreator.TimeZones, rnd));
				var startDate = now.AddDays(iter * rnd.NextDouble());
				var endDate = startDate.AddMilliseconds(10).AddHours(25 * rnd.NextDouble());
				builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = startDate, EndDate = endDate });
			}

			//Act
			var usageStats = builder.GetUsageStats().OrderBy(n => n.StartDate).ToList();

			//Assert
			for (int i = 0; i < usageStats.Count - 1; i++)
			{
				Assert.True(usageStats[i].EndDate <= usageStats[i + 1].StartDate);
			}
		}

		//xunit.console.clr4 ActivityRecorderService.Tests.dll /trait "name=SpeedIsNotAnIssue"
		[Trait("name", "SpeedIsNotAnIssue")]
		[Fact]
		public void SpeedIsNotAnIssue()
		{
			//Arrange
			var rnd = new Random(0);
			int iter = 200000;

			var builder = new UsageStatsBuilder(Enumerable.Empty<UsageStat>(), userStatInfo13);

			//Act
			var start = Environment.TickCount;
			while (iter-- >= 0)
			{
				var startDate = now.AddDays(100 * 365.2425 * rnd.NextDouble());
				var endDate = startDate.AddMilliseconds(10).AddHours(25 * rnd.NextDouble());
				builder.AddAggregateWorkItemInterval(new AggregateWorkItemInterval() { StartDate = startDate, EndDate = endDate });
			}
			var usageStats = builder.GetUsageStats().ToList();

			//Assert
			Assert.True(Environment.TickCount - start < TimeSpan.FromMinutes(1).TotalMilliseconds);
		}

		private static UserStatInfo GetRandomUserInfo(TimeZoneInfo[] timeZones, Random rnd)
		{
			return new UserStatInfo()
			{
				Id = 13,
				StartOfDayOffset = TimeSpan.FromSeconds(rnd.Next(1801) - 900),
				TimeZone = timeZones[rnd.Next(timeZones.Length)],
			};
		}
	}
}
