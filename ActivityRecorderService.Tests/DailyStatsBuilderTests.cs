using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Stats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class DailyStatsBuilderTests : DbTestsBase //very incomplete
	{
		private DailyStatsBuilder stats;

		[Fact]
		public void PrintAggregateDataClassesDataContextConnectionString()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				Console.WriteLine(context.Connection.ConnectionString);
			}
		}

		[Fact]
		public void PrintAggregateDataClassesDataContextConnectionStringAfterUse()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				context.ObjectTrackingEnabled = false;
				var aggrEntry = context.AggregateDailyWorkTimes.FirstOrDefault();
				Console.WriteLine(context.Connection.ConnectionString);
			}
		}

		[Fact]
		public void NoNullRefWhenOnlyManualWorkItems()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				var now = DateTime.UtcNow;
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 0,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				//if we are before 3 o'clock then we need an other ManualWorkItem
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 0,
					StartDate = now.AddDays(1),
					EndDate = now.AddDays(1).AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			stats = new DailyStatsBuilder(null);

			//Assert
			Assert.DoesNotThrow(() => stats.GetDailyStatsFiltered(new StatsFilter(null, null, null)));
		}

		[Fact]
		public void NoNullRefWhenOnlyManualWorkItemsButTwoUsers()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();
			var now = DateTime.UtcNow;
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 0,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				//if we are before 3 o'clock then we need an other ManualWorkItem
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 0,
					StartDate = now.AddDays(1),
					EndDate = now.AddDays(1).AddMinutes(2),
				});
				context.SubmitChanges();
			}

			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				//if we are before 3 o'clock then we need an other WorkItem
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 2,
					StartDate = now.AddDays(1),
					EndDate = now.AddDays(1).AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			stats = new DailyStatsBuilder(null);

			//Assert
			Assert.DoesNotThrow(() => stats.GetDailyStatsFiltered(new StatsFilter(null, null, null)));
		}
	}
}
