using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class WorkTimeSprocTests : DbTestsBase
	{
		public WorkTimeSprocTests()
		{
			Tct.ActivityRecorderService.Properties.Settings.Default["_jobcontrolConnectionString"] = testDb.ConnectionString;
		}

		private static ManualDataClassesDataContext GetManualContext()
		{
			return new ManualDataClassesDataContext();
		}

		private static ActivityRecorderDataClassesDataContext GetActivityContext()
		{
			return new ActivityRecorderDataClassesDataContext();
		}

		private static IvrDataClassesDataContext GetIvrContext()
		{
			return new IvrDataClassesDataContext();
		}

		private static MobileDataClassesDataContext GetMobileContext()
		{
			return new MobileDataClassesDataContext();
		}

		private static List<GetTotalWorkTimeByWorkIdForUserResult> GetTotalWorkTime(int userId, DateTime? startDate, DateTime? endDate)
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
				return context.GetTotalWorkTimeByWorkIdForUser(userId, startDate, endDate).ToList();
			}
		}

		private static List<GetTotalWorkTimeByWorkIdForUserResult> GetTotalWorkTime(int userId, DateTime endDate)
		{
			return GetTotalWorkTime(userId, null, endDate);
		}

		private static void AssertTotal(GetTotalWorkTimeByWorkIdForUserResult total)
		{
			Assert.Equal(
				total.ComputerCorrectionTime
				+ total.ComputerWorkTime
				+ total.HolidayTime
				+ total.IvrCorrectionTime
				+ total.IvrWorkTime
				+ total.MobileCorrectionTime
				+ total.MobileWorkTime
				+ total.ManualWorkTime
				+ total.SickLeaveTime
				,
				total.TotalWorkTime);
		}

		private static void AssertSameWorkTime(GetTotalWorkTimeByWorkIdForUserResult first, GetTotalWorkTimeByWorkIdForUserResult second)
		{
			Assert.True(
				   first.ComputerCorrectionTime == second.ComputerCorrectionTime
				&& first.ComputerWorkTime == second.ComputerWorkTime
				&& first.HolidayTime == second.HolidayTime
				&& first.IvrCorrectionTime == second.IvrCorrectionTime
				&& first.IvrWorkTime == second.IvrWorkTime
				&& first.MobileCorrectionTime == second.MobileCorrectionTime
				&& first.MobileWorkTime == second.MobileWorkTime
				&& first.ManualWorkTime == second.ManualWorkTime
				&& first.SickLeaveTime == second.SickLeaveTime
				&& first.TotalWorkTime == second.TotalWorkTime
				&& first.WorkId == second.WorkId);
		}

		private static readonly DateTime now = new DateTime(2010, 12, 02, 10, 00, 00);

		#region Manual AddWork AddHoliday AddSickLeave tests

		private static void CalculateManualWorkItem(ManualWorkItemTypeEnum type)
		{
			//Arrange
			if (type != ManualWorkItemTypeEnum.AddWork
				&& type != ManualWorkItemTypeEnum.AddHoliday
				&& type != ManualWorkItemTypeEnum.AddSickLeave)
			{
				throw new ArgumentOutOfRangeException("type");
			}
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = type,
					UserId = 1,
					WorkId = 3,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}
			//Act
			var total = GetTotalWorkTime(1, now.AddDays(1)).Single();
			//Assert
			Assert.Equal(3, total.WorkId);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(type == ManualWorkItemTypeEnum.AddWork ? 120000 : 0, total.ManualWorkTime);
			Assert.Equal(type == ManualWorkItemTypeEnum.AddHoliday ? 120000 : 0, total.HolidayTime);
			Assert.Equal(type == ManualWorkItemTypeEnum.AddSickLeave ? 120000 : 0, total.SickLeaveTime);
			AssertTotal(total);

			var total2 = GetTotalWorkTime(1, null, null).Single();
			AssertSameWorkTime(total, total2);
			var total3 = GetTotalWorkTime(1, now, now.AddMinutes(2)).Single();
			AssertSameWorkTime(total, total3);
		}

		private static void CalculateManualWorkItemAndRetriveSmallerInterval(ManualWorkItemTypeEnum type)
		{
			//Arrange
			if (type != ManualWorkItemTypeEnum.AddWork
			&& type != ManualWorkItemTypeEnum.AddHoliday
			&& type != ManualWorkItemTypeEnum.AddSickLeave)
			{
				throw new ArgumentOutOfRangeException("type");
			}
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = type,
					UserId = 1,
					WorkId = 3,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}
			//Act
			var total = GetTotalWorkTime(1, now.AddMinutes(1)).Single();
			//Assert
			Assert.Equal(3, total.WorkId);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(type == ManualWorkItemTypeEnum.AddWork ? 60000 : 0, total.ManualWorkTime);
			Assert.Equal(type == ManualWorkItemTypeEnum.AddHoliday ? 60000 : 0, total.HolidayTime);
			Assert.Equal(type == ManualWorkItemTypeEnum.AddSickLeave ? 60000 : 0, total.SickLeaveTime);
			AssertTotal(total);

			var total2 = GetTotalWorkTime(1, now, now.AddMinutes(1)).Single();
			AssertSameWorkTime(total, total2);
			var total3 = GetTotalWorkTime(1, now.AddMinutes(1), now.AddMinutes(2)).Single();
			AssertSameWorkTime(total, total3);
			var total4 = GetTotalWorkTime(1, now.AddSeconds(30), now.AddSeconds(90)).Single();
			AssertSameWorkTime(total, total4);
		}

		private static void CalculateTwoManualWorkItemsAndRetriveSmallerInterval(ManualWorkItemTypeEnum type)
		{
			//Arrange
			if (type != ManualWorkItemTypeEnum.AddWork
			&& type != ManualWorkItemTypeEnum.AddHoliday
			&& type != ManualWorkItemTypeEnum.AddSickLeave)
			{
				throw new ArgumentOutOfRangeException("type");
			}
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = type,
					UserId = 1,
					WorkId = 3,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = type,
					UserId = 1,
					WorkId = 3,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(3),
				});
				context.SubmitChanges();
			}
			//Act
			var total = GetTotalWorkTime(1, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(3, total.WorkId);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(type == ManualWorkItemTypeEnum.AddWork ? 180000 : 0, total.ManualWorkTime);
			Assert.Equal(type == ManualWorkItemTypeEnum.AddHoliday ? 180000 : 0, total.HolidayTime);
			Assert.Equal(type == ManualWorkItemTypeEnum.AddSickLeave ? 180000 : 0, total.SickLeaveTime);
			AssertTotal(total);

			var total2 = GetTotalWorkTime(1, now.AddMinutes(1), now.AddMinutes(3)).Single();
			AssertSameWorkTime(total, total2);
			var total3 = GetTotalWorkTime(1, now.AddSeconds(50), now.AddSeconds(170)).Single();
			AssertSameWorkTime(total, total3);
		}

		[Fact]
		public void CalculateManualAddWork()
		{
			CalculateManualWorkItem(ManualWorkItemTypeEnum.AddWork);
		}

		[Fact]
		public void CalculateManualHoliday()
		{
			CalculateManualWorkItem(ManualWorkItemTypeEnum.AddHoliday);
		}

		[Fact]
		public void CalculateManualSickLeave()
		{
			CalculateManualWorkItem(ManualWorkItemTypeEnum.AddSickLeave);
		}

		[Fact]
		public void CalculateManualAddWorkAndRetriveSmallerInterval()
		{
			CalculateManualWorkItemAndRetriveSmallerInterval(ManualWorkItemTypeEnum.AddWork);
		}

		[Fact]
		public void CalculateManualHolidayAndRetriveSmallerInterval()
		{
			CalculateManualWorkItemAndRetriveSmallerInterval(ManualWorkItemTypeEnum.AddHoliday);
		}

		[Fact]
		public void CalculateManualSickLeaveAndRetriveSmallerInterval()
		{
			CalculateManualWorkItemAndRetriveSmallerInterval(ManualWorkItemTypeEnum.AddSickLeave);
		}

		[Fact]
		public void CalculateTwoManualAddWorksAndRetriveSmallerInterval()
		{
			CalculateTwoManualWorkItemsAndRetriveSmallerInterval(ManualWorkItemTypeEnum.AddWork);
		}

		[Fact]
		public void CalculateTwoManualHolidaysAndRetriveSmallerInterval()
		{
			CalculateTwoManualWorkItemsAndRetriveSmallerInterval(ManualWorkItemTypeEnum.AddHoliday);
		}

		[Fact]
		public void CalculateTwoManualSickLeavesAndRetriveSmallerInterval()
		{
			CalculateTwoManualWorkItemsAndRetriveSmallerInterval(ManualWorkItemTypeEnum.AddSickLeave);
		}
		#endregion

		#region Ivr tests
		[Fact]
		public void EmptyCorrectionHasNoEffect()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteIvrInterval,
					UserId = 5,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).SingleOrDefault();
			//Assert
			Assert.Null(total);
		}

		#endregion

		#region ComputerWork tests
		[Fact]
		public void CalculateWorkItem()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}
			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			var total2 = GetTotalWorkTime(5, now, now.AddMinutes(2)).Single();
			AssertSameWorkTime(total, total2);
			var total3 = GetTotalWorkTime(5, null, null).Single();
			AssertSameWorkTime(total, total3);
			var total4 = GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(3)).Single();
			AssertSameWorkTime(total, total4);
			var total5 = GetTotalWorkTime(5, now, null).Single();
			AssertSameWorkTime(total, total5);
		}

		[Fact]
		public void CalculateWorkItemAndRetriveSmaller()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}
			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(1)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(60000, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(1)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(1)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddSeconds(50), now.AddSeconds(110)).Single());
		}

		[Fact]
		public void CalculateTwoWorkItemsAndRetriveSmaller()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(3),
				});
				context.SubmitChanges();
			}
			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(180000, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), now.AddMinutes(4)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddSeconds(50), now.AddSeconds(170)).Single());
		}

		[Fact]
		public void EmptyCompCorrectionHasNoEffect()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
					UserId = 5,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).SingleOrDefault();
			//Assert
			Assert.Null(total);
		}

		[Fact]
		public void CalculateWorkItemWithCorrection()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.ComputerWorkTime);
			Assert.Equal(-60000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, null).Single());

			total = GetTotalWorkTime(5, now.AddSeconds(30), now.AddMinutes(2)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(90000, total.ComputerWorkTime);
			Assert.Equal(-60000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			total = GetTotalWorkTime(5, now, now.AddMinutes(1.5)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(90000, total.ComputerWorkTime);
			Assert.Equal(-30000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateWorkItemWithCorrectionDeleteInterval()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.ComputerWorkTime);
			Assert.Equal(-60000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateWorkItemWithCorrectionDeleteIvrInterval()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteIvrInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateWorkItemWithCorrectionToWrongUser()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
					UserId = 5555,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateWorkItemWithTwoCorrectionsOverlapping()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(20),
					EndDate = now.AddMinutes(2),
				});

				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(1).AddSeconds(40),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.ComputerWorkTime);
			Assert.Equal(-60000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, null).Single());

			total = GetTotalWorkTime(5, now.AddSeconds(30), now.AddMinutes(2)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(90000, total.ComputerWorkTime);
			Assert.Equal(-60000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			total = GetTotalWorkTime(5, now, now.AddMinutes(1.5)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(90000, total.ComputerWorkTime);
			Assert.Equal(-30000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateWorkItemWithTwoCorrections()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
					UserId = 5,
					StartDate = now.AddSeconds(30),
					EndDate = now.AddMinutes(1),
				});

				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(30),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.ComputerWorkTime);
			Assert.Equal(-60000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateTwoWorkItemsWithTwoCorrectionsOverlapping()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(3),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(30),
					EndDate = now.AddMinutes(2).AddSeconds(10),
				});

				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
					UserId = 5,
					StartDate = now.AddMinutes(2),
					EndDate = now.AddMinutes(2).AddSeconds(30),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(3)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(240000, total.ComputerWorkTime);
			Assert.Equal(-90000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(4)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(4)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, null).Single());

			total = GetTotalWorkTime(5, now.AddSeconds(30), now.AddMinutes(2)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(150000, total.ComputerWorkTime);
			Assert.Equal(-60000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			total = GetTotalWorkTime(5, now.AddMinutes(1.5), now.AddMinutes(3)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.ComputerWorkTime);
			Assert.Equal(-90000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateTwoWorkItemsWithTwoCorrectionsOverlappingDifferentWorksSmallerInterval()
		{
			//Arrange
			using (var context = GetActivityContext())
			{
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.WorkItems.InsertOnSubmit(new WorkItem()
				{
					UserId = 5,
					WorkId = 22,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(3),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(30),
					EndDate = now.AddMinutes(2).AddSeconds(10),
				});

				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(50),
					EndDate = now.AddMinutes(2).AddSeconds(30),
				});
				context.SubmitChanges();
			}

			//Act
			var totals = GetTotalWorkTime(5, now.AddMinutes(3).AddSeconds(-1)).ToList();
			var total = totals.Where(n => n.WorkId == 2).Single();
			var total2 = totals.Where(n => n.WorkId == 22).Single();
			//Assert
			Assert.Equal(2, totals.Count);

			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.ComputerWorkTime);
			Assert.Equal(-30000, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			Assert.Equal(22, total2.WorkId);
			Assert.Equal(119000, total2.ComputerWorkTime);
			Assert.Equal(-60000, total2.ComputerCorrectionTime);
			Assert.Equal(0, total2.IvrWorkTime);
			Assert.Equal(0, total2.IvrCorrectionTime);
			Assert.Equal(0, total2.ManualWorkTime);
			Assert.Equal(0, total2.HolidayTime);
			Assert.Equal(0, total2.SickLeaveTime);
			AssertTotal(total2);
		}
		#endregion

		#region MobileWork tests
		[Fact]
		public void CalculateMobileWorkItem()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
			}
			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.MobileWorkTime);
			Assert.Equal(0, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			var total2 = GetTotalWorkTime(5, now, now.AddMinutes(2)).Single();
			AssertSameWorkTime(total, total2);
			var total3 = GetTotalWorkTime(5, null, null).Single();
			AssertSameWorkTime(total, total3);
			var total4 = GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(3)).Single();
			AssertSameWorkTime(total, total4);
			var total5 = GetTotalWorkTime(5, now, null).Single();
			AssertSameWorkTime(total, total5);
		}

		[Fact]
		public void CalculateMobileWorkItemAndRetriveSmaller()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}
			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(1)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(60000, total.MobileWorkTime);
			Assert.Equal(0, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(1)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(1)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddSeconds(50), now.AddSeconds(110)).Single());
		}

		[Fact]
		public void CalculateTwoMobileWorkItemsAndRetriveSmaller()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(3),
				});
				context.SubmitChanges();
			}
			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(180000, total.MobileWorkTime);
			Assert.Equal(0, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(1), now.AddMinutes(4)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddSeconds(50), now.AddSeconds(170)).Single());
		}

		[Fact]
		public void EmptyMobileCorrectionHasNoEffect()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
					UserId = 5,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).SingleOrDefault();
			//Assert
			Assert.Null(total);
		}

		[Fact]
		public void CalculateMobileWorkItemWithCorrection()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.MobileWorkTime);
			Assert.Equal(-60000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, null).Single());

			total = GetTotalWorkTime(5, now.AddSeconds(30), now.AddMinutes(2)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(90000, total.MobileWorkTime);
			Assert.Equal(-60000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			total = GetTotalWorkTime(5, now, now.AddMinutes(1.5)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(90000, total.MobileWorkTime);
			Assert.Equal(-30000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateMobileWorkItemWithCorrectionDeleteInterval()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.MobileWorkTime);
			Assert.Equal(-60000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateMobileWorkItemWithCorrectionDeleteIvrInterval()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteIvrInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.MobileWorkTime);
			Assert.Equal(0, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateMobileWorkItemWithCorrectionToWrongUser()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
					UserId = 5555,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.MobileWorkTime);
			Assert.Equal(0, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateMobileWorkItemWithTwoCorrectionsOverlapping()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(20),
					EndDate = now.AddMinutes(2),
				});

				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(1).AddSeconds(40),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.MobileWorkTime);
			Assert.Equal(-60000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(2)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, null).Single());

			total = GetTotalWorkTime(5, now.AddSeconds(30), now.AddMinutes(2)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(90000, total.MobileWorkTime);
			Assert.Equal(-60000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			total = GetTotalWorkTime(5, now, now.AddMinutes(1.5)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(90000, total.MobileWorkTime);
			Assert.Equal(-30000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateMobileWorkItemWithTwoCorrections()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
					UserId = 5,
					StartDate = now.AddSeconds(30),
					EndDate = now.AddMinutes(1),
				});

				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(30),
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(2)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.MobileWorkTime);
			Assert.Equal(-60000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateTwoMobileWorkItemsWithTwoCorrectionsOverlapping()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(3),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(30),
					EndDate = now.AddMinutes(2).AddSeconds(10),
				});

				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
					UserId = 5,
					StartDate = now.AddMinutes(2),
					EndDate = now.AddMinutes(2).AddSeconds(30),
				});
				context.SubmitChanges();
			}

			//Act
			var total = GetTotalWorkTime(5, now.AddMinutes(3)).Single();
			//Assert
			Assert.Equal(2, total.WorkId);
			Assert.Equal(240000, total.MobileWorkTime);
			Assert.Equal(-90000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, now.AddMinutes(4)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(3)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), now.AddMinutes(4)).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now.AddMinutes(-1), null).Single());
			AssertSameWorkTime(total, GetTotalWorkTime(5, now, null).Single());

			total = GetTotalWorkTime(5, now.AddSeconds(30), now.AddMinutes(2)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(150000, total.MobileWorkTime);
			Assert.Equal(-60000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			total = GetTotalWorkTime(5, now.AddMinutes(1.5), now.AddMinutes(3)).Single();
			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.MobileWorkTime);
			Assert.Equal(-90000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);
		}

		[Fact]
		public void CalculateTwoMobileWorkItemsWithTwoCorrectionsOverlappingDifferentWorksSmallerInterval()
		{
			//Arrange
			using (var context = GetMobileContext())
			{
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 2,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.InsertMobileWorkItem(new MobileWorkItem()
				{
					UserId = 5,
					WorkId = 22,
					StartDate = now.AddMinutes(1),
					EndDate = now.AddMinutes(3),
				});
				context.SubmitChanges();
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetManualContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(30),
					EndDate = now.AddMinutes(2).AddSeconds(10),
				});

				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
					UserId = 5,
					StartDate = now.AddMinutes(1).AddSeconds(50),
					EndDate = now.AddMinutes(2).AddSeconds(30),
				});
				context.SubmitChanges();
			}

			//Act
			var totals = GetTotalWorkTime(5, now.AddMinutes(3).AddSeconds(-1)).ToList();
			var total = totals.Where(n => n.WorkId == 2).Single();
			var total2 = totals.Where(n => n.WorkId == 22).Single();
			//Assert
			Assert.Equal(2, totals.Count);

			Assert.Equal(2, total.WorkId);
			Assert.Equal(120000, total.MobileWorkTime);
			Assert.Equal(-30000, total.MobileCorrectionTime);
			Assert.Equal(0, total.ComputerWorkTime);
			Assert.Equal(0, total.ComputerCorrectionTime);
			Assert.Equal(0, total.IvrWorkTime);
			Assert.Equal(0, total.IvrCorrectionTime);
			Assert.Equal(0, total.ManualWorkTime);
			Assert.Equal(0, total.HolidayTime);
			Assert.Equal(0, total.SickLeaveTime);
			AssertTotal(total);

			Assert.Equal(22, total2.WorkId);
			Assert.Equal(119000, total2.MobileWorkTime);
			Assert.Equal(-60000, total2.MobileCorrectionTime);
			Assert.Equal(0, total2.ComputerWorkTime);
			Assert.Equal(0, total2.ComputerCorrectionTime);
			Assert.Equal(0, total2.IvrWorkTime);
			Assert.Equal(0, total2.IvrCorrectionTime);
			Assert.Equal(0, total2.ManualWorkTime);
			Assert.Equal(0, total2.HolidayTime);
			Assert.Equal(0, total2.SickLeaveTime);
			AssertTotal(total2);
		}
		#endregion
	}
}
