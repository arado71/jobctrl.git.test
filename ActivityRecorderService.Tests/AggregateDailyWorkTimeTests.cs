using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.DailyAggregation;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class AggregateDailyWorkTimeTests : DbTestsBase
	{
		private static readonly DateTime now = new DateTime(2014, 08, 12, 16, 00, 00);
		private const int userId = 13;
		private const int workId = 1;

		private static int GetAggregateDailyWorkTimesCount()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				return context.AggregateDailyWorkTimes.Count();
			}
		}

		private static void CalculateAggregateDailyWorkTimes()
		{
			DailyWorkTimesHelper.Aggregate();
		}

		private static List<AggregateDailyWorkTime> GetAggregateDailyWorkTimes()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				context.ObjectTrackingEnabled = false;
				return context.AggregateDailyWorkTimes.OrderBy(n => n.UserId).ThenBy(n => n.Day).ToList();
			}
		}

		private static List<AggregateDailyWorkTimesByWorkId> GetAggregateDailyWorkTimesByWorkIds()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				context.ObjectTrackingEnabled = false;
				return context.AggregateDailyWorkTimesByWorkIds.OrderBy(n => n.UserId).ThenBy(n => n.Day).ToList();
			}
		}

		private static void AssertWorkTimeByWorkIds(AggregateDailyWorkTimesByWorkId aggr, DateTime day, int totalWorkTime)
		{
			Assert.Equal(userId, aggr.UserId);
			Assert.Equal(workId, aggr.WorkId);
			Assert.Equal(day, aggr.Day);
			Assert.Equal(totalWorkTime, aggr.TotalWorkTime);
		}

		private static void AssertWorkTimes(AggregateDailyWorkTime aggr, bool isValid
			, int netWorkTime
			, int computerWorkTime = 0
			, int mobileWorkTime = 0
			, int ivrWorkTime = 0
			, int manualWorkTime = 0
			, int holidayTime = 0
			, int sickLeaveTime = 0
			)
		{
			Assert.Equal(userId, aggr.UserId);
			Assert.Equal(isValid, aggr.IsValid);
			Assert.Equal(netWorkTime, aggr.NetWorkTime);
			Assert.Equal(computerWorkTime, aggr.ComputerWorkTime);
			Assert.Equal(mobileWorkTime, aggr.MobileWorkTime);
			Assert.Equal(ivrWorkTime, aggr.IvrWorkTime);
			Assert.Equal(manualWorkTime, aggr.ManualWorkTime);
			Assert.Equal(holidayTime, aggr.HolidayTime);
			Assert.Equal(sickLeaveTime, aggr.SickLeaveTime);
		}

		#region ManualAddWork

		[Fact]
		public void ManualInsert()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();
			Assert.Equal(0, GetAggregateDailyWorkTimesCount());

			//Act
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

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(1, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			AssertWorkTimes(aggrs[0], false, 0);
		}

		[Fact]
		public void ManualInsertAndCalc()
		{
			//Arrange
			ManualInsert();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(1, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			var wt = (int)TimeSpan.FromMinutes(2).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt, manualWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void ManualInsertAndCalcLosingRaceWontUpdate()
		{
			//Arrange
			ManualInsert();

			//Act
			DailyWorkTimesHelper.Aggregate(userId, now.Date, () =>
			{
				AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), false, 0);
				using (var context = new ManualDataClassesDataContext()) //invalidate agreate daily table and increase vesion before stats are written back
				{
					var man = context.ManualWorkItems.Single();
					man.StartDate = man.StartDate.AddHours(-1);
					context.SubmitChanges();
				}
				AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), false, 0);
			});

			//Assert
			var wt = (int)TimeSpan.FromMinutes(2).TotalMilliseconds;
			AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), false, wt, manualWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void ManualInsertAndCalcLosingRaceWontUpdateAndCalcAgain()
		{
			//Arrange
			ManualInsertAndCalcLosingRaceWontUpdate();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var wt = (int)TimeSpan.FromMinutes(62).TotalMilliseconds;
			AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), true, wt, manualWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void ManualUpdate()
		{
			//Arrange
			ManualInsert();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new ManualDataClassesDataContext())
			{
				var man = context.ManualWorkItems.Single();
				man.StartDate = man.StartDate.AddHours(-1);
				context.SubmitChanges();
			}

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			Assert.Equal(false, aggr.IsValid);
		}

		[Fact]
		public void ManualUpdateAndCalc()
		{
			//Arrange
			ManualUpdate();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			var wt = (int)TimeSpan.FromMinutes(62).TotalMilliseconds;
			AssertWorkTimes(aggr, true, wt, manualWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void ManualDelete()
		{
			//Arrange
			ManualInsert();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new ManualDataClassesDataContext())
			{
				var man = context.ManualWorkItems.Single();
				context.ManualWorkItems.DeleteOnSubmit(man);
				context.SubmitChanges();
			}

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			Assert.Equal(userId, aggr.UserId);
			Assert.Equal(false, aggr.IsValid);
		}

		[Fact]
		public void ManualDeleteAndCalc()
		{
			//Arrange
			ManualDelete();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			AssertWorkTimes(aggr, true, 0);

			Assert.False(GetAggregateDailyWorkTimesByWorkIds().Any());
		}

		[Fact]
		public void AllFieldsUpdatedInAggregateDailyWorkTimes()
		{
			//Arrange
			ManualDelete();
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggrDb = context.AggregateDailyWorkTimes.Single();
				aggrDb.ComputerWorkTime = 1;
				aggrDb.HolidayTime = 1;
				aggrDb.IsValid = false;
				aggrDb.IvrWorkTime = 1;
				aggrDb.ManualWorkTime = 1;
				aggrDb.MobileWorkTime = 1;
				aggrDb.NetWorkTime = 1;
				aggrDb.SickLeaveTime = 1;
				context.SubmitChanges();
			}

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			AssertWorkTimes(aggr, true, 0);

			Assert.False(GetAggregateDailyWorkTimesByWorkIds().Any());
		}

		[Fact]
		public void ManualCannotUpdateUserId()
		{
			//Arrange
			ManualInsert();

			//Act
			using (var context = new ManualDataClassesDataContext())
			{
				var man = context.ManualWorkItems.Single();
				man.UserId = man.UserId + 1;
				//Assert
				Assert.Throws<SqlException>(() => context.SubmitChanges());
			}
		}

		[Fact]
		public void ManualInsertAcrossDays()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();
			Assert.Equal(0, GetAggregateDailyWorkTimesCount());

			//Act
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 1,
					UserId = userId,
					StartDate = now,
					EndDate = now.AddDays(1),
				});
				context.SubmitChanges();
			}

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);

			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(userId, aggrs[0].UserId);
			Assert.Equal(false, aggrs[0].IsValid);

			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			Assert.Equal(userId, aggrs[1].UserId);
			Assert.Equal(false, aggrs[1].IsValid);
		}

		[Fact]
		public void ManualInsertAcrossDaysAndCalc()
		{
			//Aggrange
			ManualInsertAcrossDays();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			var wt1 = (int)(now.Date.AddDays(1) - now).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt1, manualWorkTime: wt1);
			var wt2 = (int)(now.AddDays(1) - now.Date.AddDays(1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[1], true, wt2, manualWorkTime: wt2);

			var waggrs = GetAggregateDailyWorkTimesByWorkIds().ToList();
			Assert.Equal(2, waggrs.Count);
			AssertWorkTimeByWorkIds(waggrs[0], now.Date, wt1);
			AssertWorkTimeByWorkIds(waggrs[1], now.Date.AddDays(1), wt2);
		}

		[Fact]
		public void ManualUpdateAcrossDays()
		{
			//Arrange
			ManualInsertAcrossDays();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new ManualDataClassesDataContext())
			{
				var man = context.ManualWorkItems.Single();
				man.StartDate = man.StartDate.AddHours(-1);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date);
				Assert.Equal(now.Date, aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);

				aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date.AddDays(1));
				Assert.Equal(now.Date.AddDays(1), aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);
			}
		}

		[Fact]
		public void ManualUpdateAcrossDaysAndCalc()
		{
			//Arrange
			ManualUpdateAcrossDays();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			var wt1 = (int)(now.Date.AddDays(1) - now.AddHours(-1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt1, manualWorkTime: wt1);
			var wt2 = (int)(now.AddDays(1) - now.Date.AddDays(1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[1], true, wt2, manualWorkTime: wt2);

			var waggrs = GetAggregateDailyWorkTimesByWorkIds().ToList();
			Assert.Equal(2, waggrs.Count);
			AssertWorkTimeByWorkIds(waggrs[0], now.Date, wt1);
			AssertWorkTimeByWorkIds(waggrs[1], now.Date.AddDays(1), wt2);
		}

		[Fact]
		public void ManualUpdateAcrossDaysWithInsert()
		{
			//Arrange
			ManualInsert();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new ManualDataClassesDataContext())
			{
				var man = context.ManualWorkItems.Single();
				man.EndDate = man.EndDate.AddDays(1);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date);
				Assert.Equal(now.Date, aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);

				aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date.AddDays(1));
				Assert.Equal(now.Date.AddDays(1), aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);
			}
		}

		[Fact]
		public void ManualUpdateAcrossDaysWithInsertAndCalc()
		{
			//Arrange
			ManualUpdateAcrossDaysWithInsert();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			var wt1 = (int)(now.Date.AddDays(1) - now).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt1, manualWorkTime: wt1);
			var wt2 = (int)(now.AddMinutes(2).AddDays(1) - now.Date.AddDays(1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[1], true, wt2, manualWorkTime: wt2);

			var waggrs = GetAggregateDailyWorkTimesByWorkIds().ToList();
			Assert.Equal(2, waggrs.Count);
			AssertWorkTimeByWorkIds(waggrs[0], now.Date, wt1);
			AssertWorkTimeByWorkIds(waggrs[1], now.Date.AddDays(1), wt2);
		}

		[Fact]
		public void ManualDeleteAcrossDays()
		{
			//Arrange
			ManualInsertAcrossDays();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new ManualDataClassesDataContext())
			{
				var man = context.ManualWorkItems.Single();
				context.ManualWorkItems.DeleteOnSubmit(man);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date);
				Assert.Equal(now.Date, aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);

				aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date.AddDays(1));
				Assert.Equal(now.Date.AddDays(1), aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);
			}
		}

		[Fact]
		public void ManualDeleteAcrossDaysAndCalc()
		{
			//Arrange
			ManualDeleteAcrossDays();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			AssertWorkTimes(aggrs[0], true, 0);
			AssertWorkTimes(aggrs[1], true, 0);

			Assert.False(GetAggregateDailyWorkTimesByWorkIds().Any());
		}

		#endregion

		#region MobileWork

		[Fact]
		public void MobileInsert()
		{
			//Arrange
			Assert.Equal(0, GetAggregateDailyWorkTimesCount());

			//Act
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

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(1, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			AssertWorkTimes(aggrs[0], false, 0);
		}

		[Fact]
		public void MobileInsertAndCalc()
		{
			//Arrange
			MobileInsert();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(1, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			var wt = (int)TimeSpan.FromMinutes(2).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt, mobileWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void MobileInsertAndCalcLosingRaceWontUpdate()
		{
			//Arrange
			MobileInsert();

			//Act
			DailyWorkTimesHelper.Aggregate(userId, now.Date, () =>
			{
				AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), false, 0);
				using (var context = new MobileDataClassesDataContext()) //invalidate agreate daily table and increase vesion before stats are written back
				{
					var man = context.GetMobileWorkItems().Single();
					man.StartDate = man.StartDate.AddHours(-1);
					context.UpdateMobileWorkItem(man);
				}
				AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), false, 0);
			});

			//Assert
			var wt = (int)TimeSpan.FromMinutes(2).TotalMilliseconds;
			AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), false, wt, mobileWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void MobileInsertAndCalcLosingRaceWontUpdateAndCalcAgain()
		{
			//Arrange
			MobileInsertAndCalcLosingRaceWontUpdate();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var wt = (int)TimeSpan.FromMinutes(62).TotalMilliseconds;
			AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), true, wt, mobileWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void MobileUpdate()
		{
			//Arrange
			MobileInsert();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new MobileDataClassesDataContext())
			{
				var man = context.GetMobileWorkItems().Single();
				man.StartDate = man.StartDate.AddHours(-1);
				context.UpdateMobileWorkItem(man);
			}

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			Assert.Equal(false, aggr.IsValid);
		}

		[Fact]
		public void MobileUpdateAndCalc()
		{
			//Arrange
			MobileUpdate();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			var wt = (int)TimeSpan.FromMinutes(62).TotalMilliseconds;
			AssertWorkTimes(aggr, true, wt, mobileWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void MobileDelete()
		{
			//Arrange
			MobileInsert();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new MobileDataClassesDataContext())
			{
				context.DeleteAllMobileWorkItems();
			}

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			Assert.Equal(userId, aggr.UserId);
			Assert.Equal(false, aggr.IsValid);
		}

		[Fact]
		public void MobileDeleteAndCalc()
		{
			//Arrange
			MobileDelete();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			AssertWorkTimes(aggr, true, 0);

			Assert.False(GetAggregateDailyWorkTimesByWorkIds().Any());
		}

		[Fact]
		public void MobileCannotUpdateUserId()
		{
			//Arrange
			MobileInsert();

			//Act
			using (var context = new MobileDataClassesDataContext())
			{
				var man = context.GetMobileWorkItems().Single();
				man.UserId = man.UserId + 1;
				//Assert
				Assert.Throws<SqlException>(() => context.UpdateMobileWorkItem(man));
			}
		}

		[Fact]
		public void MobileInsertAcrossDays()
		{
			//Arrange
			Assert.Equal(0, GetAggregateDailyWorkTimesCount());

			//Act
			using (var context = new MobileDataClassesDataContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					WorkId = 1,
					UserId = userId,
					StartDate = now,
					EndDate = now.AddDays(1),
				});
			}

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);

			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(userId, aggrs[0].UserId);
			Assert.Equal(false, aggrs[0].IsValid);

			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			Assert.Equal(userId, aggrs[1].UserId);
			Assert.Equal(false, aggrs[1].IsValid);
		}

		[Fact]
		public void MobileInsertAcrossDaysAndCalc()
		{
			//Aggrange
			MobileInsertAcrossDays();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			var wt1 = (int)(now.Date.AddDays(1) - now).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt1, mobileWorkTime: wt1);
			var wt2 = (int)(now.AddDays(1) - now.Date.AddDays(1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[1], true, wt2, mobileWorkTime: wt2);

			var waggrs = GetAggregateDailyWorkTimesByWorkIds().ToList();
			Assert.Equal(2, waggrs.Count);
			AssertWorkTimeByWorkIds(waggrs[0], now.Date, wt1);
			AssertWorkTimeByWorkIds(waggrs[1], now.Date.AddDays(1), wt2);
		}

		[Fact]
		public void MobileUpdateAcrossDays()
		{
			//Arrange
			MobileInsertAcrossDays();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new MobileDataClassesDataContext())
			{
				var man = context.GetMobileWorkItems().Single();
				man.StartDate = man.StartDate.AddHours(-1);
				context.UpdateMobileWorkItem(man);
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date);
				Assert.Equal(now.Date, aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);

				aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date.AddDays(1));
				Assert.Equal(now.Date.AddDays(1), aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);
			}
		}

		[Fact]
		public void MobileUpdateAcrossDaysAndCalc()
		{
			//Arrange
			MobileUpdateAcrossDays();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			var wt1 = (int)(now.Date.AddDays(1) - now.AddHours(-1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt1, mobileWorkTime: wt1);
			var wt2 = (int)(now.AddDays(1) - now.Date.AddDays(1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[1], true, wt2, mobileWorkTime: wt2);

			var waggrs = GetAggregateDailyWorkTimesByWorkIds().ToList();
			Assert.Equal(2, waggrs.Count);
			AssertWorkTimeByWorkIds(waggrs[0], now.Date, wt1);
			AssertWorkTimeByWorkIds(waggrs[1], now.Date.AddDays(1), wt2);
		}

		[Fact]
		public void MobileUpdateAcrossDaysWithInsert()
		{
			//Arrange
			MobileInsert();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new MobileDataClassesDataContext())
			{
				var man = context.GetMobileWorkItems().Single();
				man.EndDate = man.EndDate.AddDays(1);
				context.UpdateMobileWorkItem(man);
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date);
				Assert.Equal(now.Date, aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);

				aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date.AddDays(1));
				Assert.Equal(now.Date.AddDays(1), aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);
			}
		}

		[Fact]
		public void MobileUpdateAcrossDaysWithInsertAndCalc()
		{
			//Arrange
			MobileUpdateAcrossDaysWithInsert();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			var wt1 = (int)(now.Date.AddDays(1) - now).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt1, mobileWorkTime: wt1);
			var wt2 = (int)(now.AddMinutes(2).AddDays(1) - now.Date.AddDays(1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[1], true, wt2, mobileWorkTime: wt2);

			var waggrs = GetAggregateDailyWorkTimesByWorkIds().ToList();
			Assert.Equal(2, waggrs.Count);
			AssertWorkTimeByWorkIds(waggrs[0], now.Date, wt1);
			AssertWorkTimeByWorkIds(waggrs[1], now.Date.AddDays(1), wt2);
		}

		[Fact]
		public void MobileDeleteAcrossDays()
		{
			//Arrange
			MobileInsertAcrossDays();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new MobileDataClassesDataContext())
			{
				context.DeleteAllMobileWorkItems();
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date);
				Assert.Equal(now.Date, aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);

				aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date.AddDays(1));
				Assert.Equal(now.Date.AddDays(1), aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);
			}
		}

		[Fact]
		public void MobileDeleteAcrossDaysAndCalc()
		{
			//Arrange
			MobileDeleteAcrossDays();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			AssertWorkTimes(aggrs[0], true, 0);
			AssertWorkTimes(aggrs[1], true, 0);

			Assert.False(GetAggregateDailyWorkTimesByWorkIds().Any());
		}

		#endregion

		#region AggregateIntervals

		private static void InsertWorkItems(params WorkItem[] workItems)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				foreach (var workItem in workItems)
				{
					context.WorkItems.InsertOnSubmit(workItem);
				}
				context.SubmitChanges();
			}
		}

		[Fact]
		public void AggregateIntervalInsert()
		{
			//Arrange
			Assert.Equal(0, GetAggregateDailyWorkTimesCount());

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				InsertWorkItems(new WorkItem()
				{
					WorkId = workId,
					UserId = userId,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.UpdateHourlyAggregateWorkItems();
			}

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(1, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			AssertWorkTimes(aggrs[0], false, 0);
		}

		[Fact]
		public void AggregateIntervalInsertAndCalc()
		{
			//Arrange
			AggregateIntervalInsert();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(1, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			var wt = (int)TimeSpan.FromMinutes(2).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt, computerWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void AggregateIntervalInsertAndCalcLosingRaceWontUpdate()
		{
			//Arrange
			AggregateIntervalInsert();

			//Act
			DailyWorkTimesHelper.Aggregate(userId, now.Date, () =>
			{
				AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), false, 0);
				using (var context = new AggregateDataClassesDataContext()) //invalidate agreate daily table and increase vesion before stats are written back
				{
					var man = context.AggregateWorkItemIntervals.Single();
					man.StartDate = man.StartDate.AddHours(-1);
					context.SubmitChanges();
				}
				AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), false, 0);
			});

			//Assert
			var wt = (int)TimeSpan.FromMinutes(2).TotalMilliseconds;
			AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), false, wt, computerWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void AggregateIntervalInsertAndCalcLosingRaceWontUpdateAndCalcAgain()
		{
			//Arrange
			AggregateIntervalInsertAndCalcLosingRaceWontUpdate();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var wt = (int)TimeSpan.FromMinutes(62).TotalMilliseconds;
			AssertWorkTimes(GetAggregateDailyWorkTimes().Single(n => n.Day == now.Date), true, wt, computerWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void AggregateIntervalUpdate()
		{
			//Arrange
			AggregateIntervalInsert();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				var man = context.AggregateWorkItemIntervals.Single();
				man.StartDate = man.StartDate.AddHours(-1);
				context.SubmitChanges();
			}

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			Assert.Equal(false, aggr.IsValid);
		}

		[Fact]
		public void AggregateIntervalUpdateAndCalc()
		{
			//Arrange
			AggregateIntervalUpdate();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			var wt = (int)TimeSpan.FromMinutes(62).TotalMilliseconds;
			AssertWorkTimes(aggr, true, wt, computerWorkTime: wt);

			AssertWorkTimeByWorkIds(GetAggregateDailyWorkTimesByWorkIds().Single(), now.Date, wt);
		}

		[Fact]
		public void AggregateIntervalDelete()
		{
			//Arrange
			AggregateIntervalInsert();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				var man = context.AggregateWorkItemIntervals.Single();
				context.AggregateWorkItemIntervals.DeleteOnSubmit(man);
				context.SubmitChanges();
			}

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			Assert.Equal(userId, aggr.UserId);
			Assert.Equal(false, aggr.IsValid);
		}

		[Fact]
		public void AggregateIntervalDeleteAndCalc()
		{
			//Arrange
			AggregateIntervalDelete();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggr = GetAggregateDailyWorkTimes().Single();
			Assert.Equal(now.Date, aggr.Day);
			AssertWorkTimes(aggr, true, 0);

			Assert.False(GetAggregateDailyWorkTimesByWorkIds().Any());
		}

		[Fact]
		public void AggregateIntervalCannotUpdateUserId()
		{
			//Arrange
			AggregateIntervalInsert();

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				var man = context.AggregateWorkItemIntervals.Single();
				man.UserId = man.UserId + 1;
				//Assert
				Assert.Throws<SqlException>(() => context.SubmitChanges());
			}
		}

		[Fact]
		public void AggregateIntervalInsertAcrossDays()
		{
			//Arrange
			Assert.Equal(0, GetAggregateDailyWorkTimesCount());

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				InsertWorkItems(new WorkItem()
				{
					WorkId = 1,
					UserId = userId,
					StartDate = now,
					EndDate = now.AddDays(1),
				});
				context.UpdateHourlyAggregateWorkItems();
			}

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);

			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(userId, aggrs[0].UserId);
			Assert.Equal(false, aggrs[0].IsValid);

			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			Assert.Equal(userId, aggrs[1].UserId);
			Assert.Equal(false, aggrs[1].IsValid);
		}

		[Fact]
		public void AggregateIntervalInsertAcrossDaysAndCalc()
		{
			//Aggrange
			AggregateIntervalInsertAcrossDays();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			var wt1 = (int)(now.Date.AddDays(1) - now).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt1, computerWorkTime: wt1);
			var wt2 = (int)(now.AddDays(1) - now.Date.AddDays(1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[1], true, wt2, computerWorkTime: wt2);

			var waggrs = GetAggregateDailyWorkTimesByWorkIds().ToList();
			Assert.Equal(2, waggrs.Count);
			AssertWorkTimeByWorkIds(waggrs[0], now.Date, wt1);
			AssertWorkTimeByWorkIds(waggrs[1], now.Date.AddDays(1), wt2);
		}

		[Fact]
		public void AggregateIntervalUpdateAcrossDays()
		{
			//Arrange
			AggregateIntervalInsertAcrossDays();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				var man = context.AggregateWorkItemIntervals.Single();
				man.StartDate = man.StartDate.AddHours(-1);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date);
				Assert.Equal(now.Date, aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);

				aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date.AddDays(1));
				Assert.Equal(now.Date.AddDays(1), aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);
			}
		}

		[Fact]
		public void AggregateIntervalUpdateAcrossDaysAndCalc()
		{
			//Arrange
			AggregateIntervalUpdateAcrossDays();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			var wt1 = (int)(now.Date.AddDays(1) - now.AddHours(-1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt1, computerWorkTime: wt1);
			var wt2 = (int)(now.AddDays(1) - now.Date.AddDays(1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[1], true, wt2, computerWorkTime: wt2);

			var waggrs = GetAggregateDailyWorkTimesByWorkIds().ToList();
			Assert.Equal(2, waggrs.Count);
			AssertWorkTimeByWorkIds(waggrs[0], now.Date, wt1);
			AssertWorkTimeByWorkIds(waggrs[1], now.Date.AddDays(1), wt2);
		}

		[Fact]
		public void AggregateIntervalUpdateAcrossDaysWithInsert()
		{
			//Arrange
			AggregateIntervalInsert();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				var man = context.AggregateWorkItemIntervals.Single();
				man.EndDate = man.EndDate.AddDays(1);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date);
				Assert.Equal(now.Date, aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);

				aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date.AddDays(1));
				Assert.Equal(now.Date.AddDays(1), aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);
			}
		}

		[Fact]
		public void AggregateIntervalUpdateAcrossDaysWithInsertAndCalc()
		{
			//Arrange
			AggregateIntervalUpdateAcrossDaysWithInsert();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			var wt1 = (int)(now.Date.AddDays(1) - now).TotalMilliseconds;
			AssertWorkTimes(aggrs[0], true, wt1, computerWorkTime: wt1);
			var wt2 = (int)(now.AddMinutes(2).AddDays(1) - now.Date.AddDays(1)).TotalMilliseconds;
			AssertWorkTimes(aggrs[1], true, wt2, computerWorkTime: wt2);

			var waggrs = GetAggregateDailyWorkTimesByWorkIds().ToList();
			Assert.Equal(2, waggrs.Count);
			AssertWorkTimeByWorkIds(waggrs[0], now.Date, wt1);
			AssertWorkTimeByWorkIds(waggrs[1], now.Date.AddDays(1), wt2);
		}

		[Fact]
		public void AggregateIntervalDeleteAcrossDays()
		{
			//Arrange
			AggregateIntervalInsertAcrossDays();
			CalculateAggregateDailyWorkTimes();

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				var man = context.AggregateWorkItemIntervals.Single();
				context.AggregateWorkItemIntervals.DeleteOnSubmit(man);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date);
				Assert.Equal(now.Date, aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);

				aggr = context.AggregateDailyWorkTimes.Single(n => n.Day == now.Date.AddDays(1));
				Assert.Equal(now.Date.AddDays(1), aggr.Day);
				Assert.Equal(userId, aggr.UserId);
				Assert.Equal(false, aggr.IsValid);
			}
		}

		[Fact]
		public void AggregateIntervalDeleteAcrossDaysAndCalc()
		{
			//Arrange
			AggregateIntervalDeleteAcrossDays();

			//Act
			CalculateAggregateDailyWorkTimes();

			//Assert
			var aggrs = GetAggregateDailyWorkTimes();
			Assert.Equal(2, aggrs.Count);
			Assert.Equal(now.Date, aggrs[0].Day);
			Assert.Equal(now.Date.AddDays(1), aggrs[1].Day);
			AssertWorkTimes(aggrs[0], true, 0);
			AssertWorkTimes(aggrs[1], true, 0);

			Assert.False(GetAggregateDailyWorkTimesByWorkIds().Any());
		}

		[Fact]
		public void AggregateIntervalAggrRealAndCalc()
		{
			//Arrange/Act/Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				InsertWorkItems(new WorkItem()
				{
					WorkId = workId,
					UserId = userId,
					StartDate = now.Date.AddMinutes(-2),
					EndDate = now.Date.AddMinutes(-1),
				});
				context.UpdateHourlyAggregateWorkItems(); //insert
				AssertWorkTimes(GetAggregateDailyWorkTimes().Single(), false, 0);
				CalculateAggregateDailyWorkTimes(); //make it valid
				AssertWorkTimes(GetAggregateDailyWorkTimes().Single(), true, 60000, computerWorkTime: 60000);

				InsertWorkItems(new WorkItem()
				{
					WorkId = workId,
					UserId = userId,
					StartDate = now.Date.AddMinutes(-1),
					EndDate = now.Date.AddMinutes(3),
				});
				context.UpdateHourlyAggregateWorkItems(); //update
				AssertWorkTimes(GetAggregateDailyWorkTimes().Where(n => n.Day == now.Date.AddDays(-1)).Single(), false, 60000, computerWorkTime: 60000);
				AssertWorkTimes(GetAggregateDailyWorkTimes().Where(n => n.Day == now.Date).Single(), false, 0);
				CalculateAggregateDailyWorkTimes(); //make it valid
				AssertWorkTimes(GetAggregateDailyWorkTimes().Where(n => n.Day == now.Date.AddDays(-1)).Single(), true, 120000, computerWorkTime: 120000);
				AssertWorkTimes(GetAggregateDailyWorkTimes().Where(n => n.Day == now.Date).Single(), true, 180000, computerWorkTime: 180000);

				InsertWorkItems(new WorkItem()
				{
					WorkId = workId,
					UserId = userId,
					StartDate = now.Date.AddMinutes(-4),
					EndDate = now.Date.AddMinutes(-2),
				});
				context.UpdateHourlyAggregateWorkItems(); //insert update delete
				AssertWorkTimes(GetAggregateDailyWorkTimes().Where(n => n.Day == now.Date.AddDays(-1)).Single(), false, 120000, computerWorkTime: 120000);
				AssertWorkTimes(GetAggregateDailyWorkTimes().Where(n => n.Day == now.Date).Single(), false, 180000, computerWorkTime: 180000);
				CalculateAggregateDailyWorkTimes(); //make it valid
				AssertWorkTimes(GetAggregateDailyWorkTimes().Where(n => n.Day == now.Date.AddDays(-1)).Single(), true, 240000, computerWorkTime: 240000);
				AssertWorkTimes(GetAggregateDailyWorkTimes().Where(n => n.Day == now.Date).Single(), true, 180000, computerWorkTime: 180000);
			}
		}
		#endregion

		#region UpdateDailyAggregateWorkTimeTables connection tests
		[Fact]
		public void UpdateDailyAggregateWorkTimeTablesFirstCall()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateDailyAggregateWorkTimeTables(userId, now.Date, 1, 1, 1, 1, 1, 1, (1L).ToBinary(),
					Enumerable.Empty<KeyValuePair<int, int>>());
			}
		}

		[Fact]
		public void UpdateDailyAggregateWorkTimeTablesSecondCall()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggrEntry = context.AggregateDailyWorkTimes.FirstOrDefault();
				context.UpdateDailyAggregateWorkTimeTables(userId, now.Date, 1, 1, 1, 1, 1, 1, (1L).ToBinary(),
					Enumerable.Empty<KeyValuePair<int, int>>());
			}
		}

		[Fact]
		public void UpdateDailyAggregateWorkTimeTablesAfterSecondCallContextWorks()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				var aggrEntry = context.AggregateDailyWorkTimes.FirstOrDefault();
				context.UpdateDailyAggregateWorkTimeTables(userId, now.Date, 1, 1, 1, 1, 1, 1, (1L).ToBinary(),
					Enumerable.Empty<KeyValuePair<int, int>>());
				var aggrEntry2 = context.AggregateDailyWorkTimes.FirstOrDefault();
			}
		}

		#endregion


		[Fact]
		public void ConcurrencyUpdateDailyAggregateWorkTimeTables()
		{
			const int timeoutInSec = 10;
			const int numThreads = 10;
			using (var context = new AggregateDataClassesDataContext())
			{
				//Arrange
				var connStr = context.Connection.ConnectionString;
				context.ExecuteCommand(initLoop);

				//Act/Assert
				var cancelToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSec)); //this doesn't work (as expected by me at least)... so we set cmdtimeouts and don't set tokens
				var tasks = new Task[numThreads + 1];
				tasks[0] = Task.Factory.StartNew(async () =>
				{
					using (var conn = new SqlConnection(connStr))
					using (var cmd = new SqlCommand(GetSqlWithRetryOnDeadlock(checkLoop), conn))
					{
						cmd.CommandTimeout = timeoutInSec + 10;
						conn.Open();
						await cmd.ExecuteNonQueryAsync();
					}
				}).Unwrap();
				for (int i = 1; i < tasks.Length; i++)
				{
					tasks[i] = Task.Factory.StartNew(async () =>
					{
						using (var conn = new SqlConnection(connStr))
						using (var cmd = new SqlCommand(GetSqlWithRetryOnDeadlock(updateLoop), conn))
						{
							cmd.CommandTimeout = timeoutInSec;
							conn.Open();
							await cmd.ExecuteNonQueryAsync();
						}
					}).Unwrap();
				}
				for (int i = 0; i < tasks.Length; i++)
				{
					int idx = Task.WaitAny(tasks);
					var task = tasks[idx];

					//sometimes the status of the first task is RanToCompletion
					Assert.True(idx == 0 || task.IsFaulted, "Task " + idx + " status is " + task.Status);
					Assert.True(idx == 0 || (task.Exception != null
						&& (task.Exception).InnerExceptions[0] is SqlException
						&& ((SqlException)task.Exception.InnerExceptions[0]).Number == -2), "Task exception was " + task.Exception); //-2146232060 unchecked((int)0x80131904
					Assert.False(task.Exception != null && task.Exception.ToString().Contains("Invalid sum"), "Invalid sum");
					Assert.True(cancelToken.IsCancellationRequested, "cancellation was not requested");
				}
			}
		}

		private static string GetSqlWithRetryOnDeadlock(string sql)
		{
			return string.Format(@"declare @isDeadlock bit = 1
WHILE (@isDeadlock = 1)
BEGIN
	SET @isDeadlock = 0
	BEGIN TRY
		{0}
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() = 1205)
			SET @isDeadlock = 1
	END CATCH
END
", sql);
		}

		private const string defaults = @"declare @userId int = 13
declare @day date = '2014-08-15'
";

		private const string initLoop = defaults + @"
SET NOCOUNT ON
declare @i WorkTimesById;

 WITH
  L0   AS(SELECT 1 AS c UNION ALL SELECT 1),
  L1   AS(SELECT 1 AS c FROM L0 AS A CROSS JOIN L0 AS B),
  L2   AS(SELECT 1 AS c FROM L1 AS A CROSS JOIN L1 AS B),
  L3   AS(SELECT 1 AS c FROM L2 AS A CROSS JOIN L2 AS B),
  L4   AS(SELECT 1 AS c FROM L3 AS A CROSS JOIN L3 AS B),
  L5   AS(SELECT 1 AS c FROM L4 AS A CROSS JOIN L4 AS B),
  Nums AS(SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS n FROM L5)

INSERT INTO @i
SELECT WorkId, SUM(WorkTime)
FROM
(
	SELECT
		ABS(CHECKSUM(NEWID())) % 10000 AS WorkId,
		1 AS WorkTime
	FROM Nums
	WHERE n <= 10000
	) as a
GROUP BY WorkId

--SELECT * FROM @i
--DELETE FROM [dbo].[AggregateDailyWorkTimes]
--DELETE FROM AggregateDailyWorkTimesByWorkId

declare @ver table (Version BINARY(8))

INSERT INTO [dbo].[AggregateDailyWorkTimes]
           ([UserId]
           ,[Day]
           ,[NetWorkTime]
           ,[ComputerWorkTime]
           ,[IvrWorkTime]
           ,[MobileWorkTime]
           ,[ManualWorkTime]
           ,[HolidayTime]
           ,[SickLeaveTime]
           ,[IsValid])
	output inserted.Version INTO @ver
    VALUES
           (@userId
           ,@day
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,0
           ,1)

declare @currVer rowversion
exec dbo.UpdateDailyAggregateWorkTimeTables @userId,@day,0,0,0,0,0,0,0,@currVer, @i
";

		private const string updateLoop = defaults + @"
declare @currVer rowversion
declare @i WorkTimesById
declare @ver table (Version BINARY(8))
WHILE 1=1
BEGIN
	SET  @currVer = (SELECT Version FROM [dbo].[AggregateDailyWorkTimes] WHERE userid = @userId AND day = @day)

	DELETE FROM @i
	 ;WITH
	  L0   AS(SELECT 1 AS c UNION ALL SELECT 1),
	  L1   AS(SELECT 1 AS c FROM L0 AS A CROSS JOIN L0 AS B),
	  L2   AS(SELECT 1 AS c FROM L1 AS A CROSS JOIN L1 AS B),
	  L3   AS(SELECT 1 AS c FROM L2 AS A CROSS JOIN L2 AS B),
	  L4   AS(SELECT 1 AS c FROM L3 AS A CROSS JOIN L3 AS B),
	  L5   AS(SELECT 1 AS c FROM L4 AS A CROSS JOIN L4 AS B),
	  Nums AS(SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS n FROM L5)

	INSERT INTO @i
	SELECT WorkId, SUM(WorkTime)
	FROM
	(
		SELECT
			ABS(CHECKSUM(NEWID())) % 10000 AS WorkId,
			1 AS WorkTime
		FROM Nums
		WHERE n <= 10000
		) as a
	GROUP BY WorkId

	exec dbo.UpdateDailyAggregateWorkTimeTables @userId,@day,0,0,0,0,0,0,0,@currVer, @i

END";
		private const string checkLoop = defaults + @"
declare @sum int
WHILE 1=1
BEGIN
	SET @sum = (SELECT SUM(TotalWorkTime) FROM AggregateDailyWorkTimesByWorkId WHERE userid = @userId AND day = @day)
	IF @sum <> 10000
	BEGIN
		RAISERROR('Invalid sum',16,1)
		BREAK
	END
END";
	}
}
/*
 * GENERATE AggregateDailyWorkTimes Data
DELETE FROM [dbo].[AggregateDailyWorkTimes]
declare @untilDate date = GETDATE()
declare @userCount int = 1000, @dayCount int = 400

declare @currDay date = (SELECT DATEADD(day, -@dayCount, @untilDate))
declare @currUser int = 1

WHILE @currDay < @untilDate
BEGIN

	WHILE @currUser <= @userCount
	BEGIN
		INSERT INTO [dbo].[AggregateDailyWorkTimes] ([UserId], [Day], [IsValid]) VALUES (@currUser, @currDay, 1)
		SET @currUser = @currUser + 1
	END

	SET @currDay = DATEADD(day,1,@currDay)
	SET @currUser = 1
END

--00:01:30 for 400.000 rows (1000 user 400 days)
--00:14:47 for 4.000.000 rows (10000 user 400 days)
--SELECT * FROM [dbo].[AggregateDailyWorkTimes]
*/