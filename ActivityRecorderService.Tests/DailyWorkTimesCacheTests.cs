using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.DailyAggregation;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class DailyWorkTimesCacheTests : DbTestsBase
	{
		private static readonly DateTime now = new DateTime(2014, 10, 16, 12, 00, 00);
		private const int userId = 13;
		private const int workId = 1;

		private readonly DailyWorkTimesCache cache = new DailyWorkTimesCache();

		private static void CalculateAggregateDailyWorkTimes()
		{
			DailyWorkTimesHelper.Aggregate();
		}

		private static void AssertWorkTimes(DailyWorkTimeStats aggr, DateTime day
			, int netWorkTime
			, int computerWorkTime = 0
			, int mobileWorkTime = 0
			, int manualWorkTime = 0
			, int holidayTime = 0
			, int sickLeaveTime = 0
			)
		{
			Assert.Equal(userId, aggr.UserId);
			Assert.Equal(day, aggr.Day);
			Assert.Equal(TimeSpan.FromMinutes(netWorkTime), aggr.NetWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(computerWorkTime), aggr.ComputerWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(mobileWorkTime), aggr.MobileWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(manualWorkTime), aggr.ManuallyAddedWorkTime);
			Assert.Equal(TimeSpan.FromMinutes(holidayTime), aggr.HolidayTime);
			Assert.Equal(TimeSpan.FromMinutes(sickLeaveTime), aggr.SickLeaveTime);
		}

		[Fact]
		public long CacheOneEntryManual()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = workId,
					UserId = userId,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			CalculateAggregateDailyWorkTimes();
			var res = cache.GetDailyWorkTimeStats(userId, 0);

			//Assert
			Assert.Equal(1, res.Count);
			AssertWorkTimes(res[0], now.Date, 2, manualWorkTime: 2);
			Assert.Equal(1, res[0].TotalWorkTimeByWorkId.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res[0].TotalWorkTimeByWorkId[workId]);
			return res.Max(n => n.Version);
		}

		[Fact]
		public void CacheOneEntryEmpty()
		{
			//Arrange

			//Act
			var res = cache.GetDailyWorkTimeStats(userId, 0);

			//Assert
			Assert.Null(res);
		}

		[Fact]
		public void CacheOneEntryEmptyInvalidOnly()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = workId,
					UserId = userId,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var res = cache.GetDailyWorkTimeStats(userId, 0);

			//Assert
			Assert.Null(res);
		}

		[Fact]
		public void EmptyDbResultIsAlsoCached()
		{
			//Arrange
			CacheOneEntryEmpty();
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = workId,
					UserId = userId,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}
			CalculateAggregateDailyWorkTimes();

			//Act
			var res = cache.GetDailyWorkTimeStats(userId, 0);

			//Assert
			Assert.Null(res);
		}

		[Fact]
		public void EmptyDbResultIsAlsoCachedButCanBeInvalidated()
		{
			//Arrange
			EmptyDbResultIsAlsoCached();

			//Act
			cache.UpdateLastValidVersionCache();
			var res = cache.GetDailyWorkTimeStats(userId, 0);

			//Assert
			Assert.Equal(1, res.Count);
			AssertWorkTimes(res[0], now.Date, 2, manualWorkTime: 2);
			Assert.Equal(1, res[0].TotalWorkTimeByWorkId.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res[0].TotalWorkTimeByWorkId[workId]);
		}

		[Fact]
		public void CacheOneEntrySecondCallCached()
		{
			//Arrange
			var ver = CacheOneEntryManual();
			using (var context = new ManualDataClassesDataContext())
			{
				var man = context.ManualWorkItems.Single();
				man.EndDate = man.EndDate.AddMinutes(1);
				context.SubmitChanges();
			}

			//Act
			var res = cache.GetDailyWorkTimeStats(userId, ver);

			//Assert
			Assert.Null(res);
		}

		[Fact]
		public void CacheOneEntrySecondCallCachedForInvalid()
		{
			//Arrange
			var ver = CacheOneEntryManual();
			using (var context = new ManualDataClassesDataContext())
			{
				var man = context.ManualWorkItems.Single();
				man.EndDate = man.EndDate.AddMinutes(1);
				context.SubmitChanges();
			}

			//Act
			cache.UpdateLastValidVersionCache(); //it won't do anything since the row in not valid
			var res = cache.GetDailyWorkTimeStats(userId, ver);

			//Assert
			Assert.Null(res);
		}

		[Fact]
		public void CacheOneEntrySecondCallNotCachedIfInvalidated()
		{
			//Arrange
			var ver = CacheOneEntryManual();
			using (var context = new ManualDataClassesDataContext())
			{
				var man = context.ManualWorkItems.Single();
				man.EndDate = man.EndDate.AddMinutes(1);
				context.SubmitChanges();
			}

			//Act
			CalculateAggregateDailyWorkTimes();
			cache.UpdateLastValidVersionCache();
			var res = cache.GetDailyWorkTimeStats(userId, ver);

			//Assert
			Assert.Equal(1, res.Count);
			Assert.Equal(now.Date, res[0].Day);
			AssertWorkTimes(res[0], now.Date, 3, manualWorkTime: 3);
			Assert.Equal(1, res[0].TotalWorkTimeByWorkId.Count);
			Assert.Equal(TimeSpan.FromMinutes(3), res[0].TotalWorkTimeByWorkId[workId]);
		}

		[Fact]
		public long CacheOneEntryComputer()
		{
			//Arrange
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					WorkId = workId,
					UserId = userId,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
			}
			CalculateAggregateDailyWorkTimes();

			//Act
			var res = cache.GetDailyWorkTimeStats(userId, 0);

			//Assert
			Assert.Equal(1, res.Count);
			AssertWorkTimes(res[0], now.Date, 2, computerWorkTime: 2);
			Assert.Equal(1, res[0].TotalWorkTimeByWorkId.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res[0].TotalWorkTimeByWorkId[workId]);
			return res.Max(n => n.Version);
		}

		[Fact]
		public long CacheOneEntryMobile()
		{
			//Arrange
			using (var context = new MobileDataClassesDataContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					WorkId = workId,
					UserId = userId,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
			}
			CalculateAggregateDailyWorkTimes();

			//Act
			var res = cache.GetDailyWorkTimeStats(userId, 0);

			//Assert
			Assert.Equal(1, res.Count);
			AssertWorkTimes(res[0], now.Date, 2, mobileWorkTime: 2);
			Assert.Equal(1, res[0].TotalWorkTimeByWorkId.Count);
			Assert.Equal(TimeSpan.FromMinutes(2), res[0].TotalWorkTimeByWorkId[workId]);
			return res.Max(n => n.Version);
		}

	}
}
