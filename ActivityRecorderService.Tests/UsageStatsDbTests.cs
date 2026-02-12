using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.UsageStats;
using Tct.ActivityRecorderService.Voice;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class UsageStatsDbTests : DbTestsBase
	{
		private readonly TimeZoneInfo localTimeZone = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague;Central Europe Standard Time;Central Europe Daylight Time;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");
		private readonly UserStatInfo userStatInfo13;
		private readonly DateTime now = new DateTime(2011, 07, 19, 14, 00, 00);
		private readonly DateTime localDate;
		private readonly ILookup<int, AggregateWorkItemInterval> emptyComp = new AggregateWorkItemInterval[0].ToLookup(n => 0);
		private readonly ILookup<int, MobileWorkItem> emptyMobile = new MobileWorkItem[0].ToLookup(n => 0);
		private readonly ILookup<int, ManualWorkItem> emptyManual = new ManualWorkItem[0].ToLookup(n => 0);
		private readonly ILookup<int, VoiceRecording> emptyVox = new VoiceRecording[0].ToLookup(n => 0);

		public UsageStatsDbTests()
		{
			Tct.ActivityRecorderService.Properties.Settings.Default["_jobcontrolConnectionString"] = testDb.ConnectionString;
			userStatInfo13 = new UserStatInfo() { Id = 13, Name = "Teszt user13", StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = localTimeZone };
			localDate = CalculatorHelper.GetLocalReportDate(now, userStatInfo13.TimeZone, userStatInfo13.StartOfDayOffset);
		}

		#region Basic Db Tests
		[Fact]
		public void Linq2SqlUsageStatsTransactionRollback()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					UserId = 1,
					ComputerWorkTime = TimeSpan.FromMilliseconds(1),
					LocalDate = now.Date,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.SubmitChanges();
			}

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					UserId = 1,
					ComputerWorkTime = TimeSpan.FromMilliseconds(1),
					LocalDate = now.Date.AddDays(1),
					StartDate = now.Date.AddDays(1),
					EndDate = now.Date.AddDays(2),
					CreateDate = now,
					UpdateDate = now,
				});
				context.UsageStats.Where(n => n.UserId == 1).Single().UpdateDate = now.AddHours(1);

				var task = Task.Factory.StartNew(() =>
										{
											using (var contextBg = new AggregateDataClassesDataContext())
											{
												contextBg.UsageStats.Where(n => n.UserId == 1).Single().UpdateDate = now.AddHours(2);
												contextBg.SubmitChanges();
											}
										});
				task.Wait();

				Assert.Throws<ChangeConflictException>(() => context.SubmitChanges());
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Equal(now.AddHours(2), context.UsageStats.Where(n => n.UserId == 1).Single().UpdateDate);
			}
		}

		[Fact]
		public void IdSeedStartsAboveZero()
		{
			//Arrange
			var inserted = new UsageStat()
				{
					UserId = 1,
					ComputerWorkTime = TimeSpan.FromMilliseconds(1),
					LocalDate = now.Date,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				};

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UsageStats.InsertOnSubmit(inserted);
				context.SubmitChanges();
			}

			//Assert
			Assert.True(inserted.Id > 0);
		}

		[Fact]
		public void CannotInsertLocalDateWithTimePart()
		{
			//Arrange
			var inserted = new UsageStat()
			{
				UserId = 1,
				ComputerWorkTime = TimeSpan.FromMilliseconds(1),
				LocalDate = now.Date.AddMilliseconds(3),
				StartDate = now.Date,
				EndDate = now.Date.AddDays(1),
				CreateDate = now,
				UpdateDate = now,
			};

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UsageStats.InsertOnSubmit(inserted);
				//Assert
				Assert.Throws<SqlException>(() => context.SubmitChanges());
			}
		}

		[Fact]
		public void UserIdLocalDateIsUnique()
		{
			//Arrange
			var inserted1 = new UsageStat()
			{
				UserId = 1,
				ComputerWorkTime = TimeSpan.FromMilliseconds(1),
				LocalDate = now.Date,
				StartDate = now.Date,
				EndDate = now.Date.AddDays(1),
				CreateDate = now,
				UpdateDate = now,
			};
			var inserted2 = new UsageStat()
			{
				UserId = 2,
				ComputerWorkTime = TimeSpan.FromMilliseconds(1),
				LocalDate = now.Date,
				StartDate = now.Date,
				EndDate = now.Date.AddDays(1),
				CreateDate = now,
				UpdateDate = now,
			};
			var inserted3 = new UsageStat()
			{
				UserId = 1,
				ComputerWorkTime = TimeSpan.FromMilliseconds(1),
				LocalDate = now.Date.AddDays(1),
				StartDate = now.Date,
				EndDate = now.Date.AddDays(1),
				CreateDate = now,
				UpdateDate = now,
			};
			var inserted4 = new UsageStat()
			{
				UserId = 1,
				ComputerWorkTime = TimeSpan.FromMilliseconds(1),
				LocalDate = now.Date,
				StartDate = now.Date.AddDays(1),
				EndDate = now.Date.AddDays(2),
				CreateDate = now,
				UpdateDate = now,
			};

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UsageStats.InsertOnSubmit(inserted1);
				context.UsageStats.InsertOnSubmit(inserted2);//same LocalDate different UserId
				context.UsageStats.InsertOnSubmit(inserted3);//same UserId different LocalDate
				//Assert
				Assert.DoesNotThrow(() => context.SubmitChanges());

				context.UsageStats.InsertOnSubmit(inserted4);
				Assert.Throws<SqlException>(() => context.SubmitChanges());//same LocalDate same UserId
			}
		}

		[Fact]
		public void CannotInsertWhereEndDateIsLessThanStartDate()
		{
			//Arrange
			var inserted = new UsageStat()
			{
				UserId = 1,
				ComputerWorkTime = TimeSpan.FromMilliseconds(1),
				LocalDate = now.Date,
				StartDate = now.Date.AddDays(1),
				EndDate = now.Date,
				CreateDate = now,
				UpdateDate = now,
			};

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UsageStats.InsertOnSubmit(inserted);
				//Assert
				Assert.Throws<SqlException>(() => context.SubmitChanges());
			}
		}

		[Fact]
		public void InsertSimpleStat()
		{
			//Arrange
			var inserted = new UsageStat()
			{
				UserId = 1,
				ComputerWorkTime = TimeSpan.FromMilliseconds(1),
				LocalDate = now.Date,
				StartDate = now.Date,
				EndDate = now.Date.AddDays(1),
				CreateDate = now,
				UpdateDate = now,
			};

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UsageStats.InsertOnSubmit(inserted);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var dbData = context.UsageStats.Where(n => n.UserId == 1).Single();
				TestBase.AssertValueTypeOrStringPropertiesAreTheSame(inserted, dbData);
			}
		}

		[Fact]
		public void InsertSimpleStatAllTimes()
		{
			//Arrange
			var inserted = new UsageStat()
			{
				UserId = 1,
				ComputerWorkTime = TimeSpan.FromMilliseconds(1),
				MobileWorkTime = TimeSpan.FromMilliseconds(3),
				ManuallyAddedWorkTime = TimeSpan.FromMilliseconds(4),
				LocalDate = now.Date,
				StartDate = now.Date,
				EndDate = now.Date.AddDays(1),
				CreateDate = now,
				UpdateDate = now,
			};

			//Act
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UsageStats.InsertOnSubmit(inserted);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var dbData = context.UsageStats.Where(n => n.UserId == 1).Single();
				TestBase.AssertValueTypeOrStringPropertiesAreTheSame(inserted, dbData);
			}
		}
		#endregion

		#region Retrieval Db Tests
		private void UsersAreRetrievedArrange()
		{
			ManualWorkItemTypeHelper.InitializeDbData();
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 1,
					CreateDate = DateTime.Today
				});
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 2,
					CreateDate = DateTime.Today
				});
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 3,
					CreateDate = DateTime.Today
				});
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 4,
					CreateDate = DateTime.Today
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 1,
					WorkId = 1,
					StartDate = now,
					EndDate = now.AddHours(1),
				});
				context.SubmitChanges();
			}
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
			}
			using (var context = new ManualDataClassesDataContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = 2,
					WorkId = 3,
					StartDate = now.AddHours(1),
					EndDate = now.AddHours(2),
				});
				context.SubmitChanges();
			}
			using (var context = new MobileDataClassesDataContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 4,
					WorkId = 7,
					StartDate = now.AddHours(3),
					EndDate = now.AddHours(4),
				});
				context.SubmitChanges();
			}
		}

		[Fact]
		public void UsersAreRetrievedWholeDuration()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var users = StatsDbHelper.GetUsersForUsageStats(now, now.AddHours(4));
			users.Sort();

			//Assert
			Assert.True(new[] { 1, 2, 4 }.SequenceEqual(users));
		}

		[Fact]
		public void UsersAreRetrievedOneSmallerDuration()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var users = StatsDbHelper.GetUsersForUsageStats(now, now.AddHours(3).AddMinutes(-1));
			users.Sort();

			//Assert
			Assert.True(new[] { 1, 2 }.SequenceEqual(users));
		}


		[Fact]
		public void UsersAreRetrievedTwoSmallerDuration()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var users = StatsDbHelper.GetUsersForUsageStats(now, now.AddHours(2));
			users.Sort();

			//Assert
			Assert.True(new[] { 1, 2 }.SequenceEqual(users));
		}

		[Fact]
		public void UsersAreRetrievedThreeSmallerDuration()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var users = StatsDbHelper.GetUsersForUsageStats(now, now.AddHours(1).AddMinutes(-1));
			users.Sort();

			//Assert
			Assert.True(new[] { 1 }.SequenceEqual(users));
		}

		[Fact]
		public void UsersAreNotRetrievedForEmptyInterval()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var users = StatsDbHelper.GetUsersForUsageStats(now.AddHours(-1), now.AddMinutes(-1));
			users.Sort();

			//Assert
			Assert.Empty(users);
		}

		[Fact]
		public void IntervalsAreRetrievedComputer()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var intervals = StatsDbHelper.GetAggregateWorkItemIntervalsForUserCovered(1, now, now.AddHours(4));

			//Assert
			Assert.Equal(1, intervals.Count);
		}


		[Fact]
		public void IntervalsAreRetrievedManual()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var intervals = StatsDbHelper.GetManualWorkItemsForUserCovered(2, now, now.AddHours(4));

			//Assert
			Assert.Equal(1, intervals.Count);
		}

		//cannot test mobile :(

		[Fact]
		public void IntervalsAreRetrievedComputerSmaller()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var intervals = StatsDbHelper.GetAggregateWorkItemIntervalsForUserCovered(1, now.AddHours(0.2), now.AddHours(0.8));

			//Assert
			Assert.Equal(1, intervals.Count);
		}


		[Fact]
		public void IntervalsAreRetrievedManualSmaller()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var intervals = StatsDbHelper.GetManualWorkItemsForUserCovered(2, now.AddHours(1.2), now.AddHours(1.8));

			//Assert
			Assert.Equal(1, intervals.Count);
		}

		[Fact]
		public void IntervalsAreNotRetrievedComputer()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var intervals = StatsDbHelper.GetAggregateWorkItemIntervalsForUserCovered(1, now.AddHours(-1), now);

			//Assert
			Assert.Equal(0, intervals.Count);
		}


		[Fact]
		public void IntervalsAreNotRetrievedManual()
		{
			//Arrange
			UsersAreRetrievedArrange();

			//Act
			var intervals = StatsDbHelper.GetManualWorkItemsForUserCovered(2, now, now.AddHours(1));

			//Assert
			Assert.Equal(0, intervals.Count);
		}

		#endregion

		#region UsageStatsHelper GenerateUsageStats Computer Tests
		[Fact]
		public void InsertNewUsage()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, emptyMobile, emptyManual, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
			}
		}

		[Fact]
		public void UpdateUsage()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var aggr2 = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(2),
					EndDate = now.AddHours(3),},
			}.Concat(aggr[userStatInfo13.Id]).ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, emptyMobile, emptyManual, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				context.CommitUsageStatsToEcomm();
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr2, emptyMobile, emptyManual, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(3), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
			}
		}

		[Fact]
		public void InsertAndUpdateUsages()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var aggr2 = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(2),
					EndDate = now.AddHours(3),},
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddDays(1).AddHours(2),
					EndDate = now.AddDays(1).AddHours(3),},
			}.Concat(aggr[userStatInfo13.Id]).ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, emptyMobile, emptyManual, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr2, emptyMobile, emptyManual, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Where(n => n.LocalDate == localDate).Single();
				Assert.Equal(TimeSpan.FromHours(3), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				var usageStat2 = context.UsageStats.Where(n => n.LocalDate == localDate.AddDays(1)).Single();
				Assert.Equal(TimeSpan.FromHours(1), usageStat2.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat2.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat2.UserId);
				Assert.Equal(localDate.AddDays(1), usageStat2.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).StartDate, usageStat2.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).EndDate, usageStat2.EndDate);
			}
		}
		#endregion

		#region UsageStatsHelper GenerateUsageStats Manual Tests
		[Fact]
		public void InsertNewUsageManual()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new ManualWorkItem() { 
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(emptyComp, emptyMobile, aggr, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
			}
		}

		[Fact]
		public void UpdateUsageManual()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new ManualWorkItem() { 
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var aggr2 = new[] { 
				new ManualWorkItem() { 
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(2),
					EndDate = now.AddHours(3),},
			}.Concat(aggr[userStatInfo13.Id]).ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(emptyComp, emptyMobile, aggr, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				context.CommitUsageStatsToEcomm();
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(emptyComp, emptyMobile, aggr2, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(3), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
			}
		}

		[Fact]
		public void InsertAndUpdateUsagesManual()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new ManualWorkItem() { 
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var aggr2 = new[] { 
				new ManualWorkItem() { 
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(2),
					EndDate = now.AddHours(3),},
				new ManualWorkItem() { 
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now.AddDays(1).AddHours(2),
					EndDate = now.AddDays(1).AddHours(3),},
			}.Concat(aggr[userStatInfo13.Id]).ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(emptyComp, emptyMobile, aggr, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(emptyComp, emptyMobile, aggr2, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Where(n => n.LocalDate == localDate).Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(3), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				var usageStat2 = context.UsageStats.Where(n => n.LocalDate == localDate.AddDays(1)).Single();
				Assert.Equal(TimeSpan.Zero, usageStat2.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(1), usageStat2.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat2.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat2.UserId);
				Assert.Equal(localDate.AddDays(1), usageStat2.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).StartDate, usageStat2.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).EndDate, usageStat2.EndDate);
			}
		}

		[Fact]
		public void WontInsertNewUsageManualHoliday()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new ManualWorkItem() { 
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(emptyComp, emptyMobile, aggr, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
		}
		#endregion

		#region UsageStatsHelper GenerateUsageStats Combined Tests
		[Fact]
		public void InsertNewUsageCombined()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var mob = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(3),},
			}.ToLookup(n => n.UserId);
			var man = new[] { 
				new ManualWorkItem() {
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(4),},
			}.ToLookup(n => n.UserId);

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, mob, man, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(3), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
			}
		}

		[Fact]
		public void UpdateUsageCombined()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var mob = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(3),},
			}.ToLookup(n => n.UserId);
			var man = new[] { 
				new ManualWorkItem() {
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(4),},
			}.ToLookup(n => n.UserId);
			var aggr2 = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(2),
					EndDate = now.AddHours(3),},
			}.Concat(aggr[userStatInfo13.Id]).ToLookup(n => n.UserId);
			var mob2 = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(3),
					EndDate = now.AddHours(4),
					Imei = userStatInfo13.Id,
				},
			}.Concat(mob[userStatInfo13.Id]).ToLookup(n => n.UserId);
			var man2 = new[] { 
				new ManualWorkItem() { 
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(4),
					EndDate = now.AddHours(5),},
			}.Concat(man[userStatInfo13.Id]).ToLookup(n => n.UserId);
			var vox2 = new [] {new VoiceRecording()
			{
				UserId = userStatInfo13.Id,
				Name = "xyz",
				StartDate = now.AddHours(2).AddMinutes(5),
				Duration = (int)TimeSpan.FromMinutes(10).TotalMilliseconds,
			}}.ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, mob, man, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(3), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				Assert.Equal(false, usageStat.UsedVoxCtrl);
				Assert.Equal(true, usageStat.UsedMobile);
				Assert.Equal(false, usageStat.UsedBeaconClient);
				context.CommitUsageStatsToEcomm();
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr2, mob2, man2, vox2, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(3), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(5), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				Assert.Equal(true, usageStat.UsedVoxCtrl);
				Assert.Equal(true, usageStat.UsedMobile);
				Assert.Equal(true, usageStat.UsedBeaconClient);
			}
		}

		[Fact]
		public void UpdateUsageVoxCtrl()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var mob = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(3),},
			}.ToLookup(n => n.UserId);
			var man = new[] { 
				new ManualWorkItem() {
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(4),},
			}.ToLookup(n => n.UserId);
			var vox2 = new[] {new VoiceRecording()
			{
				UserId = userStatInfo13.Id,
				Name = "xyz",
				StartDate = now.AddHours(2).AddMinutes(5),
				Duration = (int)TimeSpan.FromMinutes(10).TotalMilliseconds,
			}}.ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, mob, man, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(3), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				Assert.Equal(false, usageStat.UsedVoxCtrl);
				context.CommitUsageStatsToEcomm();
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, mob, man, vox2, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(3), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				Assert.Equal(true, usageStat.UsedVoxCtrl);
			}
		}

		[Fact]
		public void UpdateUsageMobileUsed()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var mob = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(3),},
			}.ToLookup(n => n.UserId);
			var man = new[] { 
				new ManualWorkItem() {
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(4),},
			}.ToLookup(n => n.UserId);
			var mob2 = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(3),
					EndDate = now.AddHours(4),
				}
			}.ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, emptyMobile, man, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				Assert.Equal(false, usageStat.UsedMobile);
				context.CommitUsageStatsToEcomm();
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, mob2, man, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(1), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				Assert.Equal(true, usageStat.UsedMobile);
			}
		}

		[Fact]
		public void UpdateUsageUsedBeacon()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var man = new[] { 
				new ManualWorkItem() {
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(4),},
			}.ToLookup(n => n.UserId);
			var mob2 = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(3),
					EndDate = now.AddHours(4),
					Imei = userStatInfo13.Id,
				}
			}.ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, emptyMobile, man, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				Assert.Equal(false, usageStat.UsedBeaconClient);
				context.CommitUsageStatsToEcomm();
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, mob2, man, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(1), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				Assert.Equal(true, usageStat.UsedBeaconClient);
			}
		}

		[Fact]
		public void InsertAndUpdateUsagesCombined()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var mob = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(3),},
			}.ToLookup(n => n.UserId);
			var man = new[] { 
				new ManualWorkItem() {
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(4),},
			}.ToLookup(n => n.UserId);
			var aggr2 = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(2),
					EndDate = now.AddHours(3),},
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddDays(1).AddHours(2),
					EndDate = now.AddDays(1).AddHours(3),},
			}.Concat(aggr[userStatInfo13.Id]).ToLookup(n => n.UserId);
			var mob2 = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(3),
					EndDate = now.AddHours(4),},
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddDays(3).AddHours(3),
					EndDate = now.AddDays(3).AddHours(4),},
			}.Concat(mob[userStatInfo13.Id]).ToLookup(n => n.UserId);
			var man2 = new[] { 
				new ManualWorkItem() { 
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(4),
					EndDate = now.AddHours(5),},
				new ManualWorkItem() { 
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now.AddDays(4).AddHours(4),
					EndDate = now.AddDays(4).AddHours(5),},
			}.Concat(man[userStatInfo13.Id]).ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, mob, man, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(3), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr2, mob2, man2, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Where(n => n.LocalDate == localDate).Single();
				Assert.Equal(TimeSpan.FromHours(3), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(5), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				var usageStat2 = context.UsageStats.Where(n => n.LocalDate == localDate.AddDays(1)).Single();
				Assert.Equal(TimeSpan.FromHours(1), usageStat2.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat2.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat2.UserId);
				Assert.Equal(localDate.AddDays(1), usageStat2.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).StartDate, usageStat2.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).EndDate, usageStat2.EndDate);
				usageStat2 = context.UsageStats.Where(n => n.LocalDate == localDate.AddDays(3)).Single();
				Assert.Equal(TimeSpan.Zero, usageStat2.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(1), usageStat2.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat2.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat2.UserId);
				Assert.Equal(localDate.AddDays(3), usageStat2.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(3), userStatInfo13).StartDate, usageStat2.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(3), userStatInfo13).EndDate, usageStat2.EndDate);
				usageStat2 = context.UsageStats.Where(n => n.LocalDate == localDate.AddDays(4)).Single();
				Assert.Equal(TimeSpan.Zero, usageStat2.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(1), usageStat2.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat2.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat2.UserId);
				Assert.Equal(localDate.AddDays(4), usageStat2.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(4), userStatInfo13).StartDate, usageStat2.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(4), userStatInfo13).EndDate, usageStat2.EndDate);
			}
		}

		[Fact]
		public void InsertAndUpdateUsagesCombinedNoShorten()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
			}
			var usi = new[] { userStatInfo13 }.ToLookup(n => n.Id);
			var aggr = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(2),},
			}.ToLookup(n => n.UserId);
			var mob = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(3),},
			}.ToLookup(n => n.UserId);
			var man = new[] { 
				new ManualWorkItem() {
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now,
					EndDate = now.AddHours(4),},
			}.ToLookup(n => n.UserId);
			var aggr2 = new[] { 
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(2),
					EndDate = now.AddHours(3),},
				new AggregateWorkItemInterval() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddDays(1).AddHours(2),
					EndDate = now.AddDays(1).AddHours(3),},
			}.Concat(aggr[userStatInfo13.Id]).ToLookup(n => n.UserId);
			var mob2 = new[] { 
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(3),
					EndDate = now.AddHours(4),},
				new MobileWorkItem() { 
					UserId = userStatInfo13.Id, 
					StartDate = now.AddDays(3).AddHours(3),
					EndDate = now.AddDays(3).AddHours(4),},
			}.Concat(mob[userStatInfo13.Id]).ToLookup(n => n.UserId);
			var man2 = new[] { 
				new ManualWorkItem() { 
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now.AddHours(4),
					EndDate = now.AddHours(5),},
				new ManualWorkItem() { 
 					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					UserId = userStatInfo13.Id, 
					StartDate = now.AddDays(4).AddHours(4),
					EndDate = now.AddDays(4).AddHours(5),},
			}.Concat(man[userStatInfo13.Id]).ToLookup(n => n.UserId);
			UsageStatsHelper.GenerateUsageStatsImpl(aggr, mob, man, emptyVox, usi);
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(3), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
			}

			//Act
			UsageStatsHelper.GenerateUsageStatsImpl(aggr2, mob2, man2, emptyVox, usi);

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Where(n => n.LocalDate == localDate).Single();
				Assert.Equal(TimeSpan.FromHours(3), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(4), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(5), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).StartDate, usageStat.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo13).EndDate, usageStat.EndDate);
				var usageStat2 = context.UsageStats.Where(n => n.LocalDate == localDate.AddDays(1)).Single();
				Assert.Equal(TimeSpan.FromHours(1), usageStat2.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat2.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat2.UserId);
				Assert.Equal(localDate.AddDays(1), usageStat2.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).StartDate, usageStat2.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(1), userStatInfo13).EndDate, usageStat2.EndDate);
				usageStat2 = context.UsageStats.Where(n => n.LocalDate == localDate.AddDays(3)).Single();
				Assert.Equal(TimeSpan.Zero, usageStat2.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(1), usageStat2.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat2.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat2.UserId);
				Assert.Equal(localDate.AddDays(3), usageStat2.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(3), userStatInfo13).StartDate, usageStat2.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(3), userStatInfo13).EndDate, usageStat2.EndDate);
				usageStat2 = context.UsageStats.Where(n => n.LocalDate == localDate.AddDays(4)).Single();
				Assert.Equal(TimeSpan.Zero, usageStat2.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat2.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(1), usageStat2.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat2.IsAcked);
				Assert.Equal(userStatInfo13.Id, usageStat2.UserId);
				Assert.Equal(localDate.AddDays(4), usageStat2.LocalDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(4), userStatInfo13).StartDate, usageStat2.StartDate);
				Assert.Equal(CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate.AddDays(4), userStatInfo13).EndDate, usageStat2.EndDate);
			}
		}
		#endregion

		#region UsageStatsHelper CommitUsageStatsToEcomm Tests
		[Fact]
		public void CommitGoodStat()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
					{
						IsAcked = false,
						LocalDate = localDate,
						ComputerWorkTime = TimeSpan.FromHours(2),
						UserId = 13,
						StartDate = now.Date,
						EndDate = now.Date.AddDays(1),
						CreateDate = now,
						UpdateDate = now,
					});
				context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void CommitGoodStatMobile()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					MobileWorkTime = TimeSpan.FromHours(2),
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromHours(2), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void CommitGoodStatManual()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ManuallyAddedWorkTime = TimeSpan.FromHours(2),
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void CommitGoodStatCombinedAdded()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromMinutes(2),
					MobileWorkTime = TimeSpan.FromMinutes(2),
					ManuallyAddedWorkTime = TimeSpan.FromMinutes(2),
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromMinutes(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromMinutes(2), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromMinutes(2), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void CommitTwoGoodStats()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromHours(2),
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromHours(2),
					UserId = 14,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				}); context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Where(n => n.UserId == 13).Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
				usageStat = context.UsageStats.Where(n => n.UserId == 14).Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(14, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void CommitTwoGoodStatsButWontCommitShort()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromHours(2),
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromMinutes(1),
					UserId = 15,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				}); context.SubmitChanges();
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromHours(2),
					UserId = 14,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				}); context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Where(n => n.UserId == 13).Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
				usageStat = context.UsageStats.Where(n => n.UserId == 14).Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(14, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
				usageStat = context.UsageStats.Where(n => n.UserId == 15).Single();
				Assert.Equal(TimeSpan.FromMinutes(1), usageStat.ComputerWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(15, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void CommitTwoGoodStatsButWontCommitWhereClientSetWorkedUsersOnDayFails()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromHours(2),
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromHours(2),
					UserId = -1,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				}); context.SubmitChanges();
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromHours(2),
					UserId = 14,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				}); context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Where(n => n.UserId == 13).Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
				usageStat = context.UsageStats.Where(n => n.UserId == 14).Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(true, usageStat.IsAcked);
				Assert.Equal(14, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
				usageStat = context.UsageStats.Where(n => n.UserId == -1).Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(-1, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void WontCommitShortStat()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromMinutes(4),
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromMinutes(4), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void WontCommitShortStatIvr()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void WontCommitShortStatMobile()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					MobileWorkTime = TimeSpan.FromMinutes(4),
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.FromMinutes(4), usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void WontCommitShortStatManual()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ManuallyAddedWorkTime = TimeSpan.FromMinutes(4),
					UserId = 13,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.Zero, usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.FromMinutes(4), usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(13, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}

		[Fact]
		public void WontCommitBadStatWhereClientSetWorkedUsersOnDayFails()
		{
			//Arrange
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.Empty(context.UsageStats);
				context.UsageStats.InsertOnSubmit(new UsageStat()
				{
					IsAcked = false,
					LocalDate = localDate,
					ComputerWorkTime = TimeSpan.FromHours(2),
					UserId = -1,
					StartDate = now.Date,
					EndDate = now.Date.AddDays(1),
					CreateDate = now,
					UpdateDate = now,
				});
				context.SubmitChanges();
			}

			//Act
			UsageStatsHelper.CommitUsageStatsToEcomm();

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				var usageStat = context.UsageStats.Single();
				Assert.Equal(TimeSpan.FromHours(2), usageStat.ComputerWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.MobileWorkTime);
				Assert.Equal(TimeSpan.Zero, usageStat.ManuallyAddedWorkTime);
				Assert.Equal(false, usageStat.IsAcked);
				Assert.Equal(-1, usageStat.UserId);
				Assert.Equal(localDate, usageStat.LocalDate);
				Assert.Equal(now.Date, usageStat.StartDate);
				Assert.Equal(now.Date.AddDays(1), usageStat.EndDate);
			}
		}
		#endregion
	}
}
