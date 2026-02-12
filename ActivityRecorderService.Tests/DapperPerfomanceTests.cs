using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	//Dapper ver 1.38
	public class DapperPerfomanceTests : DbTestsBase, IDisposable
	{
		private const int it = 10;
		private const int userId = 1;
		private const int workId = 2;
		private static readonly DateTime now = new DateTime(2014, 11, 07, 16, 00, 00);
		private static readonly TimeSpan maxAggrWorkItemLength = TimeSpan.FromDays(7);

		private const string dropSprocSql = @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAggregateWorkItemIntervalsForUserTest]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAggregateWorkItemIntervalsForUserTest]
";
		private const string createSprocSql = @"
CREATE PROCEDURE [dbo].[GetAggregateWorkItemIntervalsForUserTest]
	(
	@userId int,
	@startDate datetime,
	@endDate datetime
	)
AS
	SET NOCOUNT ON

	SELECT [UserId], [WorkId], [StartDate], [EndDate], [ComputerId]
	  FROM [dbo].[AggregateWorkItemIntervals]
	 WHERE [UserId] = @UserId
		   AND @startDate < [EndDate]
		   AND DATEADD(day,-7, @startDate) <= [StartDate] --no index on EndDate
		   AND [StartDate] < @endDate

	RETURN 0	
";

		public DapperPerfomanceTests()
		{
			using (var conn = new SqlConnection(Tct.ActivityRecorderService.Properties.Settings.Default.recorderConnectionString))
			{
				conn.Execute(dropSprocSql);
				conn.Execute(createSprocSql);
			}
		}

		public void Dispose()
		{
			using (var conn = new SqlConnection(Tct.ActivityRecorderService.Properties.Settings.Default.recorderConnectionString))
			{
				conn.Execute(dropSprocSql);
			}
		}

		private static void InsertAggrWorkItem(DateTime startDate, DateTime endDate, bool aggr = true)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = userId,
					WorkId = workId,
					StartDate = startDate,
					EndDate = endDate,
				});
				context.SubmitChanges();
			}
			if (!aggr) return;
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
			}
		}

		private static List<AggregateWorkItemIntervalCovered> GetAggrWorkItemsLinq(DateTime startDate, DateTime endDate)
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				context.ObjectTrackingEnabled = false;

				var result = context.AggregateWorkItemIntervals
					.Where(n => startDate < n.EndDate)
					.Where(n => startDate - maxAggrWorkItemLength <= n.StartDate) //no index on EndDate
					.Where(n => n.StartDate < endDate)
					.Where(n => n.UserId == userId)
					.Select(n => new AggregateWorkItemIntervalCovered()
					{
						UserId = n.UserId,
						WorkId = n.WorkId,
						StartDate = n.StartDate,
						EndDate = n.EndDate,
						ComputerId = n.ComputerId,
						PhaseId = n.PhaseId
					})
					.ToList();

				return result;
			}

		}

		private static List<AggregateWorkItemIntervalCovered> GetAggrWorkItemsLinqSproc(DateTime startDate, DateTime endDate)
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				var result = context.ExecuteQuery<AggregateWorkItemIntervalCovered>("exec [dbo].[GetAggregateWorkItemIntervalsForUserTest] @userId={0}, @startDate={1}, @endDate={2}", userId, startDate, endDate)
					.ToList();

				return result;
			}

		}

		private static List<AggregateWorkItemIntervalCovered> GetAggrWorkItemsDapper(DateTime startDate, DateTime endDate)
		{
			using (var conn = new SqlConnection(Tct.ActivityRecorderService.Properties.Settings.Default.recorderConnectionString))
			{
				return conn.Query<AggregateWorkItemIntervalCovered>("SELECT [UserId], [WorkId], [StartDate], [EndDate], [ComputerId] FROM [dbo].[AggregateWorkItemIntervals] WHERE"
															+ " [UserId] = @UserId"
															+ " AND @StartDate < [EndDate]"
															+ " AND @MinStartDate <= [StartDate]" //no index on EndDate
															+ " AND [StartDate] < @EndDate",
															new { UserId = userId, MinStartDate = startDate - maxAggrWorkItemLength, StartDate = startDate, EndDate = endDate }
					).ToList();
			}
		}

		private static List<AggregateWorkItemIntervalCovered> GetAggrWorkItemsDapperSproc(DateTime startDate, DateTime endDate)
		{
			using (var conn = new SqlConnection(Tct.ActivityRecorderService.Properties.Settings.Default.recorderConnectionString))
			{
				return conn.Query<AggregateWorkItemIntervalCovered>("exec [dbo].[GetAggregateWorkItemIntervalsForUserTest] @userId=@UserId, @startDate=@StartDate, @endDate=@EndDate",
															new { UserId = userId, MinStartDate = startDate - maxAggrWorkItemLength, StartDate = startDate, EndDate = endDate }
					).ToList();
			}
		}

		private static void WritePerformanceResults(int it, DateTime startDate, DateTime endDate)
		{
			GetAggrWorkItemsLinq(startDate, endDate);
			GetAggrWorkItemsLinqSproc(startDate, endDate);
			GetAggrWorkItemsDapper(startDate, endDate);
			GetAggrWorkItemsDapperSproc(startDate, endDate);

			var sw = Stopwatch.StartNew();
			for (int i = 0; i < it; i++)
			{
				GetAggrWorkItemsLinq(startDate, endDate);
			}
			Console.WriteLine("LINQ: " + sw.Elapsed);

			sw = Stopwatch.StartNew();
			for (int i = 0; i < it; i++)
			{
				GetAggrWorkItemsLinqSproc(startDate, endDate);
			}
			Console.WriteLine("LINQ Sproc: " + sw.Elapsed);

			sw = Stopwatch.StartNew();
			for (int i = 0; i < it; i++)
			{
				GetAggrWorkItemsDapper(startDate, endDate);
			}
			Console.WriteLine("Dapper: " + sw.Elapsed);

			sw = Stopwatch.StartNew();
			for (int i = 0; i < it; i++)
			{
				GetAggrWorkItemsDapperSproc(startDate, endDate);
			}
			Console.WriteLine("Dapper Sproc: " + sw.Elapsed);
		}

		//it 1000
		//LINQ: 00:00:16.8238269
		//LINQ Sproc: 00:00:15.0927818
		//Dapper: 00:00:03.9036812
		//Dapper Sproc: 00:00:03.8491688
		[Fact]
		public void EmptyGet()
		{
			//Arrange
			var startDate = now.Date;
			var endDate = now.Date.AddDays(1);

			//Act
			var linq = GetAggrWorkItemsLinq(startDate, endDate);
			var dapper = GetAggrWorkItemsDapper(startDate, endDate);

			//Assert
			Assert.Empty(linq);
			Assert.Empty(dapper);

			//Performance
			WritePerformanceResults(it, startDate, endDate);
		}

		//it 1000
		//LINQ: 00:00:16.3124178
		//LINQ Sproc: 00:00:14.0271430
		//Dapper: 00:00:03.8392963
		//Dapper Sproc: 00:00:03.8554823
		[Fact]
		public void GetOneRecord()
		{
			//Arrange
			InsertAggrWorkItem(now, now.AddMinutes(2));
			var startDate = now.Date;
			var endDate = now.Date.AddDays(1);

			//Act
			var linq = GetAggrWorkItemsLinq(startDate, endDate);
			var dapper = GetAggrWorkItemsDapper(startDate, endDate);

			//Assert
			Assert.Equal(1, linq.Count);
			Assert.Equal(1, dapper.Count);
			TestBase.AssertValueTypeOrStringPropertiesAreTheSame(linq[0], dapper[0]);

			//Performance
			WritePerformanceResults(it, startDate, endDate);
		}

		//it 1000
		//LINQ: 00:00:17.1878699
		//LINQ Sproc: 00:00:15.2101927
		//Dapper: 00:00:04.0732659
		//Dapper Sproc: 00:00:04.1074101
		[Fact]
		public void GetOneHundredRecords()
		{
			//Arrange
			for (int i = 0; i < 100; i++)
			{
				InsertAggrWorkItem(now.AddSeconds(2 * i), now.AddSeconds(2 * i + 1), i == 99);
			}
			var startDate = now.Date;
			var endDate = now.Date.AddDays(1);

			//Act
			var linq = GetAggrWorkItemsLinq(startDate, endDate);
			var dapper = GetAggrWorkItemsDapper(startDate, endDate);

			//Assert
			Assert.Equal(100, linq.Count);
			Assert.Equal(100, dapper.Count);
			for (int i = 0; i < 100; i++)
			{
				TestBase.AssertValueTypeOrStringPropertiesAreTheSame(linq[i], dapper[i]);
			}

			//Performance
			WritePerformanceResults(it, startDate, endDate);
		}
	}
}
