using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Moq;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.WorktimeHistory;
using Xunit;
using Xunit.Sdk;

namespace Tct.Tests.ActivityRecorderClient
{
	public class WorkTimeModificationTests : DisabledThreadAsserts
	{
		public WorkTimeModificationTests()
		{
			DateTimeEx.OverrideNow(() => day2.EndDate);
		}

		private static readonly WorkDataWithParentNames testWorkData1 = new WorkDataWithParentNames()
		{
			ParentId = null,
			ParentNames = new List<string> { "Abcd" },
			WorkData = new WorkData { Name = "Foo", Id = 100, Priority = 500, Description = "Lorem ipsum" }
		};

		private static readonly WorkDataWithParentNames testWorkData2 = new WorkDataWithParentNames()
		{
			ParentId = null,
			ParentNames = new List<string> { "Abcd" },
			WorkData = new WorkData { Name = "Bar", Id = 101, Priority = 600, Description = "Dolor sit amet" }
		};

		private static readonly WorkDataWithParentNames testWorkData3 = new WorkDataWithParentNames()
		{
			ParentId = null,
			ParentNames = new List<string> { "Efgh" },
			WorkData = new WorkData { Name = "Baz", Id = 102, Priority = 300 }
		};

		/*
		 *                day 1      |     day2
		 *   | interval1 | interval2 | interval3 |
		 *   |             interval4             |
		 *         | interval5 | interval6 |
		 */

		private static readonly TimeSpan dayOffset = new TimeSpan(3, 0, 0);
		private static readonly Interval day1 = new Interval(new DateTime(2000, 01, 01, 3, 0, 0), new DateTime(2000, 01, 02, 3, 0, 0)).FromLocalToUtc();
		private static readonly Interval day2 = new Interval(new DateTime(2000, 01, 02, 3, 0, 0), new DateTime(2000, 01, 03, 3, 0, 0)).FromLocalToUtc();

		private static readonly Interval interval1 = new Interval(new DateTime(2000, 01, 02, 1, 0, 0), new DateTime(2000, 01, 02, 2, 0, 0)).FromLocalToUtc();
		private static readonly Interval interval2 = new Interval(new DateTime(2000, 01, 02, 2, 0, 0), new DateTime(2000, 01, 02, 3, 0, 0)).FromLocalToUtc();
		private static readonly Interval interval3 = new Interval(new DateTime(2000, 01, 02, 3, 0, 0), new DateTime(2000, 01, 02, 4, 0, 0)).FromLocalToUtc();
		private static readonly Interval interval4 = new Interval(new DateTime(2000, 01, 02, 1, 0, 0), new DateTime(2000, 01, 02, 4, 0, 0)).FromLocalToUtc();
		private static readonly Interval interval5 = new Interval(new DateTime(2000, 01, 02, 1, 30, 0), new DateTime(2000, 01, 02, 2, 30, 0)).FromLocalToUtc();
		private static readonly Interval interval6 = new Interval(new DateTime(2000, 01, 02, 2, 30, 0), new DateTime(2000, 01, 02, 3, 30, 0)).FromLocalToUtc();

		/*         day 1          | 
		 *   |computer1|  manual  |
		 */

		[Fact]
		public void GetStats()
		{
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval2.StartDate,
				EndDate = interval2.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsMeeting = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()))
				.Returns(() => new ClientWorkTimeHistory()
				{
					ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval()
						{
							ComputerId = 1234,
							StartDate = interval1.StartDate,
							EndDate = interval1.EndDate,
							WorkId = 10
						},
					},
					ManualIntervals = new List<ManualInterval>()
					{
						manualInterval,
					},
					ModificationAgeLimit = new TimeSpan(3, 0, 0, 0),
					TotalTimeInMs = (long)(interval1.Duration + interval2.Duration).TotalMilliseconds,
				});
			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.GetStats(day1);
			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.NotNull(result.Result);
			Assert.Equal(interval1.Duration + interval2.Duration, result.Result.WorkTime);
			Assert.Equal(interval1.Duration, result.Result.GetWorkLength(10));
			Assert.Equal(interval2.Duration, result.Result.GetWorkLength(15));
			Assert.Equal(TimeSpan.FromTicks(0), result.Result.GetWorkLength(5));
			Assert.True(result.Result.WorksByDevice.ContainsKey(DeviceType.Computer));
			Assert.True(result.Result.WorksByDevice.ContainsKey(DeviceType.Meeting));
			Assert.True(result.Result.WorksByDevice[DeviceType.Computer].ContainsKey(1234));
			Assert.True(result.Result.WorksByDevice[DeviceType.Meeting].ContainsKey(0));
			Assert.False(result.Result.WorksByDevice.ContainsKey(DeviceType.Manual));
			Assert.Equal(interval1.Duration, result.Result.GetDeviceLength(DeviceType.Computer));
			Assert.Equal(interval2.Duration, result.Result.GetDeviceLength(DeviceType.Meeting));
			Assert.Equal(TimeSpan.FromTicks(0), result.Result.GetDeviceLength(DeviceType.Manual));
			Assert.Equal(interval1.StartDate, result.Result.Bounds.StartDate);
			Assert.Equal(interval2.EndDate, result.Result.Bounds.EndDate);
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
		}

		/*
		*                day 1      |     day2
		*   | added | 
		*
		*   |AddWork|
		*/

		[Fact]
		public void AddWork()
		{
			var comment = "Very important meeting";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var res = service.CreateWork(testWorkData1, interval1, comment);

			Assert.NotNull(res);
			Assert.Null(res.Exception);
			Assert.True(res.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(mods =>
				mods.ManualIntervalModifications.Count == 1
				&& mods.ManualIntervalModifications[0].OriginalItem == null
				&& mods.ManualIntervalModifications[0].NewItem != null
				&& mods.ManualIntervalModifications[0].NewItem.StartDate == interval1.StartDate
				&& mods.ManualIntervalModifications[0].NewItem.EndDate == interval1.EndDate
				&& mods.ManualIntervalModifications[0].NewItem.WorkId == testWorkData1.WorkData.Id
				&& mods.ManualIntervalModifications[0].NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork
				&& mods.ManualIntervalModifications[0].NewItem.Comment.Equals(comment, StringComparison.InvariantCulture)
				), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
		}

		[Fact]
		public void AddTooLongWork()
		{
			var comment = "Very important meeting";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var res = service.CreateWork(testWorkData1, new Interval(interval1.StartDate, interval1.StartDate.AddHours(24)), comment, true);

			Assert.NotNull(res);
			Assert.NotNull(res.Exception);
			Assert.IsType<ValidationException>(res.Exception);
			Assert.Equal(Severity.Error, ((ValidationException)res.Exception).Severity);
			Assert.False(res.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.AtMostOnce());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.AtMostOnce());
		}

		[Fact]
		public void AddFutureWork()
		{
			var comment = "Very important meeting";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			//workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory());
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var res = service.CreateWork(testWorkData1, new Interval(day2.EndDate.AddMinutes(-15), day2.EndDate.AddMinutes(-5)), comment, true);

			Assert.NotNull(res);
			Assert.NotNull(res.Exception);
			Assert.IsType<ValidationException>(res.Exception);
			Assert.Equal(Severity.Error, ((ValidationException)res.Exception).Severity);
			Assert.False(res.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.AtMostOnce());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Never());
		}

		/*
		*                day 1      |     day2
		*                   | comp1 | 
		*			|         added         |                
		*
		*           |AddWork|       |AddWork|
		*/

		[Fact]
		public void AddOverlapping()
		{
			var comment = "Very important meetings";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 10},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.CreateWork(testWorkData1, interval4, comment);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Warn, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
		}

		[Fact]
		public void AddOverlappingForced()
		{
			var comment = "Very important meetings";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset).Verifiable();
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 10},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var res = service.CreateWork(testWorkData1, interval4, comment, true);

			Assert.NotNull(res);
			Assert.Null(res.Exception);
			Assert.True(res.Result);
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(4));
			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(mods =>
				mods.ManualIntervalModifications.Count == 2
				&& mods.ManualIntervalModifications.All(y => y.OriginalItem == null)
				&& mods.ManualIntervalModifications.All(y => y.NewItem != null)
				&& mods.ManualIntervalModifications.Any(y => y.NewItem.StartDate == interval1.StartDate && y.NewItem.EndDate == interval1.EndDate)
				&& mods.ManualIntervalModifications.Any(y => y.NewItem.StartDate == interval3.StartDate && y.NewItem.EndDate == interval3.EndDate)
				&& mods.ManualIntervalModifications.All(y => y.NewItem.WorkId == testWorkData1.WorkData.Id)
				&& mods.ManualIntervalModifications.All(y => y.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork)
				&& mods.ManualIntervalModifications.All(y => y.NewItem.Comment.Equals(comment, StringComparison.InvariantCulture))
				), It.IsAny<int>()), Times.Once());
		}

		[Fact]
		public void AddTooMuchOverlapping()
		{
			var comment = "Very important meetings";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = Enumerable.Range(0, 10).Select(x => new ComputerInterval()
				{
					ComputerId = 1,
					StartDate = interval2.StartDate.AddMinutes(2 * x),
					EndDate = interval2.StartDate.AddMinutes(2 * x + 1),
					WorkId = 10
				}).ToList(),
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.CreateWork(testWorkData1, interval4, comment);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Error, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
		}

		[Fact]
		public void AddTooOldForced()
		{
			var comment = "Very important meetings";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.CreateWork(testWorkData1, new Interval(day2.EndDate.AddHours(-72), day2.EndDate.AddHours(-71)), comment, true);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Error, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.AtMostOnce());
		}

		[Fact]
		public void AddTooMuchOverlappingForced()
		{
			var comment = "Very important meetings";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = Enumerable.Range(0, 10).Select(x => new ComputerInterval()
				{
					ComputerId = 1,
					StartDate = interval2.StartDate.AddMinutes(2 * x),
					EndDate = interval2.StartDate.AddMinutes(2 * x + 1),
					WorkId = 10
				}).ToList(),
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.CreateWork(testWorkData1, interval4, comment, true);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Error, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
		}

		/*
		*                day 1      |     day2
		*           |         comp1         | 
		*		       	   |  added |                
		*/

		[Fact]
		public void AddFullOverlapping()
		{
			var comment = "Very important meetings";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval4.StartDate, EndDate = interval4.EndDate, WorkId = 10},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.CreateWork(testWorkData1, interval2, comment);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Error, ((ValidationException)result.Exception).Severity);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once());
		}

		[Fact]
		public void AddFullOverlappingForced()
		{
			var comment = "Very important meetings";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval4.StartDate, EndDate = interval4.EndDate, WorkId = 10},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.CreateWork(testWorkData1, interval2, comment, true);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Error, ((ValidationException)result.Exception).Severity);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once());
		}

		[Fact]
		public void AddNoThrow()
		{
			var exception = new ArgumentException("Lorem ipsum");
			var comment = "Very important meeting";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()))
				.Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });
			workTimeHistoryMock.Setup(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>())).Throws(exception);

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			GeneralResult<bool> res = null;
			Assert.DoesNotThrow(() => { res = service.CreateWork(testWorkData1, interval1, comment); });

			Assert.NotNull(res);
			Assert.NotNull(res.Exception);
			Assert.Equal(exception, res.Exception);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
		}

		/*         day 1           |   day 2
		 *   | comp 1   |          |   comp 2 |
		 *   |             deletion           |
		 *   
		 *   |         DeleteInterval         |
		 */

		[Fact]
		public void DeleteInterval()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 10},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 2, StartDate = interval3.StartDate, EndDate = interval3.EndDate, WorkId = 20},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.DeleteInterval(interval4, comment);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(4));
			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(mods =>
				mods.ManualIntervalModifications.Count == 1
				&& mods.ManualIntervalModifications[0].OriginalItem == null
				&& mods.ManualIntervalModifications[0].NewItem != null
				&& mods.ManualIntervalModifications[0].NewItem.StartDate == interval4.StartDate
				&& mods.ManualIntervalModifications[0].NewItem.EndDate == interval4.EndDate
				&& mods.ManualIntervalModifications[0].NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteInterval
				&& mods.ManualIntervalModifications[0].NewItem.Comment.Equals(comment, StringComparison.InvariantCulture)
				), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
			//workNameProviderMock.Verify(x => x.GetNames(It.Is<IEnumerable<int>>(y => y.Count() == 2 && y.Contains(10) && y.Contains(20))), Times.Once());
		}

		/*         day 1           |   day 2
		 *   | comp 1   |          |   comp 2 |
		 *                    | ManDel  |
		 *   |             deletion           |
		 *   
		 *   |         DeleteInterval         |
		 */

		[Fact]
		public void DeleteIntervalWithExistingDelete()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 1234,
				StartDate = interval6.StartDate,
				EndDate = interval6.EndDate,
				ManualWorkItemType = ManualWorkItemTypeEnum.DeleteInterval,
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>() { manualInterval },
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 10},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>() { manualInterval },
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 2, StartDate = interval3.StartDate, EndDate = interval3.EndDate, WorkId = 20},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.DeleteInterval(interval4, comment);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(4));
			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(mods =>
				mods.ManualIntervalModifications.Count == 2
				&& mods.ManualIntervalModifications.Any(z =>
					z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.StartDate == interval4.StartDate
					&& z.NewItem.EndDate == interval6.StartDate
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteInterval)
				&& mods.ManualIntervalModifications.Any(z =>
					z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.StartDate == interval6.EndDate
					&& z.NewItem.EndDate == interval4.EndDate
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteInterval)
				), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
			//workNameProviderMock.Verify(x => x.GetNames(It.Is<IEnumerable<int>>(y => y.Count() == 2 && y.Contains(10) && y.Contains(20))), Times.Once());
		}

		[Fact]
		public void DeleteIntervalNoThrowOffset()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Throws(new ArgumentNullException("Foo"));

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.DeleteInterval(interval4, comment);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
		}

		/*         day 1           |   day 2
		 *   | comp 1 |   manual   |   comp 2 |
		 *   |             deletion           |
		 *   
		 *   |         DeleteInterval         |
		 *             ( -manual )
		 */

		[Fact]
		public void DeleteIntervalWithManual()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval2.StartDate,
				EndDate = interval2.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 10},
					},
				ManualIntervals = new List<ManualInterval>()
					{
						manualInterval
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 2, StartDate = interval3.StartDate, EndDate = interval3.EndDate, WorkId = 20},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.DeleteInterval(interval4, comment);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Warn, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
		}

		[Fact]
		public void DeleteIntervalWithManualForced()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval2.StartDate,
				EndDate = interval2.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 10},
					},
				ManualIntervals = new List<ManualInterval>()
					{
						manualInterval
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 2, StartDate = interval3.StartDate, EndDate = interval3.EndDate, WorkId = 20},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.DeleteInterval(interval4, comment, true);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(4));
			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(mods =>
				mods.ManualIntervalModifications.Count == 2
				&& mods.ManualIntervalModifications.Any(y =>
					y.OriginalItem == null && y.NewItem != null
					&& y.NewItem.StartDate == interval4.StartDate
					&& y.NewItem.EndDate == interval4.EndDate
					&& y.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteInterval
					&& y.NewItem.Comment.Equals(comment, StringComparison.InvariantCulture))
				&& mods.ManualIntervalModifications.Any(y =>
					y.OriginalItem == manualInterval
					&& y.NewItem == null)), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
		}

		/*         day 1           |   day 2
		 *   | manual |       | manual2  |
		 *   |             deletion           |
		 *   
		 *   |         DeleteInterval         |
		 *    (-manual)        (-manual2)
		 */

		[Fact]
		public void DeleteIntervalWithManualOverhanging()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval1.StartDate,
				EndDate = interval1.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};
			var manualInterval2 = new ManualInterval()
			{
				Id = 11,
				StartDate = interval6.StartDate,
				EndDate = interval6.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>()
					{
						manualInterval,
						manualInterval2
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.DeleteInterval(interval4, comment);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Warn, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
		}

		[Fact]
		public void DeleteIntervalWithManualOverhangingForced()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval1.StartDate,
				EndDate = interval1.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};
			var manualInterval2 = new ManualInterval()
			{
				Id = 11,
				StartDate = interval6.StartDate,
				EndDate = interval6.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>()
					{
						manualInterval,
						manualInterval2
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.DeleteInterval(interval4, comment, true);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(4));
			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(mods =>
				mods.ManualIntervalModifications.Count == 3
				&& mods.ManualIntervalModifications.Any(y =>
					y.OriginalItem == null && y.NewItem != null
					&& y.NewItem.StartDate == interval4.StartDate
					&& y.NewItem.EndDate == interval4.EndDate
					&& y.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteInterval
					&& y.NewItem.Comment.Equals(comment, StringComparison.InvariantCulture))
				&& mods.ManualIntervalModifications.Any(y =>
					y.OriginalItem == manualInterval
					&& y.NewItem == null)
				&& mods.ManualIntervalModifications.Any(y =>
					y.OriginalItem == manualInterval2
					&& y.NewItem == null
				)), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
		}

		/*         day 1          | 
		 *   |computer1|  manual  |
		 *       | mobile 1 |
		 *   Delete computer1
		 * 
		 *   |DelComp  |
		 */

		[Fact]
		public void DeleteComputerWork()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>(MockBehavior.Loose);
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval3.StartDate,
				EndDate = interval3.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval() { ComputerId = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 10},
					},
				MobileIntervals = new List<MobileInterval>()
					{
						new MobileInterval() { Imei = 1, StartDate = interval5.StartDate, EndDate = interval5.EndDate, WorkId = 30}
					},
				ManualIntervals = new List<ManualInterval>()
					{
						manualInterval,
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Throws(new ArgumentException("Foo"));

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var statRes = service.GetStats(day1);
			var selectedInterval = statRes.Result.WorksByDevice[DeviceType.Computer][1].First();
			var result = service.DeleteWork(selectedInterval, comment);

			Assert.NotNull(statRes);
			Assert.Null(statRes.Exception);
			Assert.NotNull(statRes.Result);
			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(mods =>
				mods.ManualIntervalModifications.Count == 1
				&& mods.ManualIntervalModifications.Any(y =>
					y.OriginalItem == null && y.NewItem != null
					&& y.NewItem.StartDate == interval1.StartDate
					&& y.NewItem.EndDate == interval1.EndDate
					&& y.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval
					&& y.NewItem.Comment.Equals(comment, StringComparison.InvariantCulture)
				)), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
		}

		/*         day 1          | 
		 *   |computer1|  manual  |
		 *      | computer 2 |
		 *   Delete computer1
		 * 
		 *   |DelComp  |
		 */

		[Fact(Skip = "Not implemented yet")]
		public void DeleteOverlappingComputerWork()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>(MockBehavior.Strict);
			//workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval3.StartDate,
				EndDate = interval3.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};

			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval() { ComputerId = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 10},
						new ComputerInterval() { ComputerId = 1, StartDate = interval5.StartDate, EndDate = interval5.EndDate, WorkId = 20},
					},
				ManualIntervals = new List<ManualInterval>()
					{
						manualInterval,
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var statRes = service.GetStats(day1);
			Assert.NotNull(statRes);
			Assert.Null(statRes.Exception);
			Assert.NotNull(statRes.Result);
			var selectedInterval = statRes.Result.WorksByDevice[DeviceType.Computer][1].First();
			var result = service.DeleteWork(selectedInterval, comment);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.False(result.Result);
			workTimeHistoryMock.VerifyAll();
		}

		[Fact(Skip = "Not implemented yet")]
		public void DeleteOverlappingComputerWorkForced()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>(MockBehavior.Strict);
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval3.StartDate,
				EndDate = interval3.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval() { ComputerId = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 10},
						new ComputerInterval() { ComputerId = 1, StartDate = interval5.StartDate, EndDate = interval5.EndDate, WorkId = 20},
					},
				ManualIntervals = new List<ManualInterval>()
					{
						manualInterval,
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.Modify(It.Is<WorkTimeModifications>(mods =>
				mods.ManualIntervalModifications.Count == 1
				&& mods.ManualIntervalModifications.Any(y =>
					y.OriginalItem == null && y.NewItem != null
					&& y.NewItem.StartDate == interval1.StartDate
					&& y.NewItem.EndDate == interval1.EndDate
					&& y.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval
					&& y.NewItem.Comment.Equals(comment, StringComparison.InvariantCulture)
				)), It.IsAny<int>())).Verifiable();
			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var statRes = service.GetStats(day1);
			Assert.NotNull(statRes);
			Assert.Null(statRes.Exception);
			Assert.NotNull(statRes.Result);
			var selectedInterval = statRes.Result.WorksByDevice[DeviceType.Computer][1].First();
			var result = service.DeleteWork(selectedInterval, comment);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);
			workTimeHistoryMock.VerifyAll();
		}

		/*         day 1          | 
		 *   |mobile 1|  manual  |
		 *       | computer 1 |
		 *   Delete mobile 1
		 * 
		 *   |DelMob   |
		 */

		[Fact]
		public void DeleteMobileWork()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>(MockBehavior.Loose);
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval3.StartDate,
				EndDate = interval3.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				MeetingId = 100,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval() { ComputerId = 1, StartDate = interval5.StartDate, EndDate = interval5.EndDate, WorkId = 10},
					},
				MobileIntervals = new List<MobileInterval>()
					{
						new MobileInterval() { Imei = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 30}
					},
				ManualIntervals = new List<ManualInterval>()
					{
						manualInterval,
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var statRes = service.GetStats(day1);
			var selectedInterval = statRes.Result.WorksByDevice[DeviceType.Mobile][1].First();
			var result = service.DeleteWork(selectedInterval, comment);

			Assert.NotNull(statRes);
			Assert.Null(statRes.Exception);
			Assert.NotNull(statRes.Result);
			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(mods =>
				mods.ManualIntervalModifications.Count == 1
				&& mods.ManualIntervalModifications.Any(y =>
					y.OriginalItem == null && y.NewItem != null
					&& y.NewItem.StartDate == interval1.StartDate
					&& y.NewItem.EndDate == interval1.EndDate
					&& y.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteMobileInterval
					&& y.NewItem.Comment.Equals(comment, StringComparison.InvariantCulture)
				)), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
		}

		/*         day 1          | 
		 *             |  manual  |
		 *   Delete manual
		 * 
		 *   |DelIvr   |
		 */

		[Fact]
		public void DeleteManualWork()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval2.StartDate,
				EndDate = interval2.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()))
				.Returns(() => new ClientWorkTimeHistory()
				{
					ManualIntervals = new List<ManualInterval>()
					{
						manualInterval,
					},
					ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
				});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var statRes = service.GetStats(day1);
			Assert.NotNull(statRes);
			Assert.Null(statRes.Exception);
			Assert.NotNull(statRes.Result);
			var selectedInterval = statRes.Result.WorksByDevice[DeviceType.Manual][0].First();
			var result = service.DeleteWork(selectedInterval, comment);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Warn, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once(), "Stats not requested");
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once(), "Unnecessary GetStats call");
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never(), "No modification required but called anyway");
		}

		[Fact]
		public void DeleteManualWorkForced()
		{
			var comment = "Oops";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				Id = 10,
				StartDate = interval2.StartDate,
				EndDate = interval2.EndDate,
				WorkId = 15,
				ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()))
				.Returns(() => new ClientWorkTimeHistory()
				{
					ManualIntervals = new List<ManualInterval>()
					{
						manualInterval,
					},
					ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
				});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var statRes = service.GetStats(day1);
			var selectedInterval = statRes.Result.WorksByDevice[DeviceType.Manual][0].First();
			var result = service.DeleteWork(selectedInterval, comment, true);

			Assert.NotNull(statRes);
			Assert.Null(statRes.Exception);
			Assert.NotNull(statRes.Result);
			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2), "Stats not requested or refreshed");
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2), "Unnecessary GetStat calls");
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once(), "Too many Modification calls");
			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(y =>
				y.ManualIntervalModifications.Count == 1
				&& y.ManualIntervalModifications[0].OriginalItem == manualInterval
				&& y.ManualIntervalModifications[0].NewItem == null
				), It.IsAny<int>()), Times.Once(), "Invalid Modification parameters");
		}

		/*
		*                day 1      |     day2
		*       |  comp1  | 
		*			 |   mod   |                
		*
		*            |-Cmp|
		*            | +  |
		*/

		[Fact]
		public void ModifyInterval()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 10},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.ModifyInterval(interval5, testWorkData1, comment);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(y =>
				y.ManualIntervalModifications.Count == 2
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval
					&& z.NewItem.StartDate == interval5.StartDate
					&& z.NewItem.EndDate == interval1.EndDate)
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork
					&& z.NewItem.StartDate == interval5.StartDate
					&& z.NewItem.EndDate == interval1.EndDate
					&& z.NewItem.WorkId == 100)
				&& y.Comment.Equals(comment)
				), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
		}

		/*
		*                day 1      |     day2
		*       |  comp1  |  comp1  |
		*			 |   mod   |                
		*
		*            |-Cmp|-Cmp|
		*            | +  | +  |
		*/

		// It is also correct to merge intervals, however due to readability this check has been omitted
		[Fact]
		public void ModifyTwoIntervals()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>()
					{
						new ComputerInterval(){ComputerId = 1, StartDate = interval1.StartDate, EndDate = interval1.EndDate, WorkId = 10},
						new ComputerInterval(){ComputerId = 1, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 20},
					},
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.ModifyInterval(interval5, testWorkData1, comment);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(y =>
				y.ManualIntervalModifications.Count == 4
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval
					&& z.NewItem.StartDate == interval5.StartDate
					&& z.NewItem.EndDate == interval1.EndDate)
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval
					&& z.NewItem.StartDate == interval2.StartDate
					&& z.NewItem.EndDate == interval5.EndDate)
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork
					&& z.NewItem.StartDate == interval5.StartDate
					&& z.NewItem.EndDate == interval1.EndDate
					&& z.NewItem.WorkId == 100)
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork
					&& z.NewItem.StartDate == interval2.StartDate
					&& z.NewItem.EndDate == interval5.EndDate
					&& z.NewItem.WorkId == 100)
				&& y.Comment.Equals(comment)
				), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
		}

		/*
		*                day 1      |     day2
		*       |   meet1   | 
		*			  |   mod   |                
		*
		*       |meet1|
		*             |+meet|
		*/

		[Fact]
		public void ModifyManualInterval()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				IsMeeting = true,
				WorkId = 10,
				StartDate = interval1.StartDate,
				EndDate = interval1.EndDate,
				MeetingId = 1,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>() { manualInterval },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.ModifyInterval(interval5, testWorkData1, comment);

			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Warn, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once());
		}

		[Fact]
		public void ModifyManualIntervalForced()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				IsMeeting = true,
				WorkId = 10,
				StartDate = interval1.StartDate,
				EndDate = interval1.EndDate,
				MeetingId = 1,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>() { manualInterval },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.ModifyInterval(interval5, testWorkData1, comment, true);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(y =>
				y.Comment.Equals(comment)
				&& y.ManualIntervalModifications.Count == 2
				&& y.ManualIntervalModifications.Any(z => z.OriginalItem == manualInterval)
				&& y.ManualIntervalModifications.Any(
					z => z.NewItem.StartDate == interval1.StartDate
					&& z.NewItem.EndDate == interval5.StartDate
					&& z.NewItem.WorkId == 10
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork)
				&& y.ManualIntervalModifications.Any(
					z => z.NewItem.StartDate == interval5.StartDate
					&& z.NewItem.EndDate == interval1.EndDate
					&& z.NewItem.WorkId == 100
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork)
				), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
		}

		/*
		*                day 1      |     day2
		*       |          meet1            | 
		*			    |    mod    |                
		*
		*       | meet1 |           | +meet |
		*               |   +meet   |
		*/

		[Fact]
		public void ModifyManualPartialIntervalForced()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval()
			{
				IsMeeting = true,
				WorkId = 10,
				StartDate = interval4.StartDate,
				EndDate = interval4.EndDate,
				MeetingId = 1,
				IsEditable = true
			};
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>() { manualInterval },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var result = service.ModifyInterval(interval2, testWorkData1, comment, true);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(y =>
				y.Comment.Equals(comment)
				&& y.ManualIntervalModifications.Count == 3
				&& y.ManualIntervalModifications.Any(z => z.OriginalItem == manualInterval)
				&& y.ManualIntervalModifications.Any(
					z => z.NewItem.StartDate == interval1.StartDate
					&& z.NewItem.EndDate == interval1.EndDate
					&& z.NewItem.WorkId == 10
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork)
				&& y.ManualIntervalModifications.Any(
					z => z.NewItem.StartDate == interval2.StartDate
					&& z.NewItem.EndDate == interval2.EndDate
					&& z.NewItem.WorkId == 100
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork)
				&& y.ManualIntervalModifications.Any(
					z => z.NewItem.StartDate == interval3.StartDate
					&& z.NewItem.EndDate == interval3.EndDate
					&& z.NewItem.WorkId == 10
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork)
				), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(3));
		}

		/*
		*                day 1      |     day2
		*			    | comp #100 |                
		*
		*                      | comp #100 |
		*               
		*               |-comp |    | +Man |
		*/

		[Fact]
		public void ModifyWorkExtendInterval()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>() { new ComputerInterval() { ComputerId = 1000, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 100 } },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var stats = service.GetStats(day1);
			var workInterval = stats.Result.Works[0];
			var result = service.ModifyWork(workInterval, testWorkData1, new[] { interval6 }, comment);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(y =>
				y.Comment.Equals(comment)
				&& y.ManualIntervalModifications.Count == 2
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.StartDate == interval2.StartDate
					&& z.NewItem.EndDate == interval6.StartDate
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval)
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.StartDate == interval2.EndDate
					&& z.NewItem.EndDate == interval6.EndDate
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork
					&& z.NewItem.WorkId == 100
				)), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(3));
		}

		[Fact]
		public void ModifyWorkExtendIntervalSameWork()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>() { new ComputerInterval() { ComputerId = 1000, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 10 } },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory());

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var stats = service.GetStats(day1);
			var workInterval = stats.Result.Works[0];
			var result = service.ModifyWork(workInterval, null, new[] { interval6 }, comment);

			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(y =>
				y.Comment.Equals(comment)
				&& y.ManualIntervalModifications.Count == 2
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.StartDate == interval2.StartDate
					&& z.NewItem.EndDate == interval6.StartDate
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval)
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.StartDate == interval2.EndDate
					&& z.NewItem.EndDate == interval6.EndDate
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork
					&& z.NewItem.WorkId == 10
				)), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(3));
		}

		/*
		*                day 1      |     day2
		*	 		    | comp #10  |                
		*
		*                      | comp #100 |
		*               
		*               |   -comp   |
		*                      |   +man    |
		*/

		[Fact]
		public void ModifyWorkIdAndExtendInterval()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ComputerIntervals = new List<ComputerInterval>() { new ComputerInterval() { ComputerId = 1000, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 10 } },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory());

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var stats = service.GetStats(day1);
			var workInterval = stats.Result.Works[0];
			var result = service.ModifyWork(workInterval, testWorkData1, new[] { interval6 }, comment);

			Assert.NotNull(stats);
			Assert.Null(stats.Exception);
			Assert.NotNull(stats.Result);
			Assert.Equal(1, stats.Result.Works.Length);
			Assert.NotNull(workInterval);
			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(y =>
				y.Comment.Equals(comment)
				&& y.ManualIntervalModifications.Count == 2
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.StartDate == interval2.StartDate
					&& z.NewItem.EndDate == interval2.EndDate
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.DeleteComputerInterval)
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == null
					&& z.NewItem != null
					&& z.NewItem.StartDate == interval6.StartDate
					&& z.NewItem.EndDate == interval6.EndDate
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork
					&& z.NewItem.WorkId == 100
				)), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(3));
		}

		/*
		*                day 1      |     day2
		*			    | man  #100 |                
		*
		*                      | man  #100 |
		*/

		[Fact]
		public void ModifyWorkManualInterval()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval() { Id = 1234, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 10, IsEditable = true };
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>() { manualInterval },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var stats = service.GetStats(day1);
			var workInterval = stats.Result.Works[0];
			var result = service.ModifyWork(workInterval, testWorkData1, new[] { interval6 }, comment);

			Assert.NotNull(stats);
			Assert.Null(stats.Exception);
			Assert.NotNull(stats.Result);
			Assert.Equal(1, stats.Result.Works.Length);
			Assert.NotNull(workInterval);
			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Warn, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.AtMostOnce());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Between(1, 2, Range.Inclusive));
		}

		[Fact]
		public void ModifyWorkManualIntervalForced()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval() { Id = 1234, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 10, IsEditable = true };
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>() { manualInterval },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var stats = service.GetStats(day1);
			var workInterval = stats.Result.Works[0];
			var result = service.ModifyWork(workInterval, testWorkData1, new[] { interval6 }, comment, true);

			Assert.NotNull(stats);
			Assert.Null(stats.Exception);
			Assert.NotNull(stats.Result);
			Assert.Equal(1, stats.Result.Works.Length);
			Assert.NotNull(workInterval);
			Assert.Equal(manualInterval, workInterval.OriginalInterval);
			Assert.NotNull(result);
			Assert.Null(result.Exception);
			Assert.True(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.Is<WorkTimeModifications>(y =>
				y.Comment.Equals(comment)
				&& y.ManualIntervalModifications.Count == 1
				&& y.ManualIntervalModifications.Any(
					z => z.OriginalItem == manualInterval
					&& z.NewItem != null
					&& z.NewItem.StartDate == interval6.StartDate
					&& z.NewItem.EndDate == interval6.EndDate
					&& z.NewItem.WorkId == 100
					&& z.NewItem.ManualWorkItemType == ManualWorkItemTypeEnum.AddWork)
				), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Exactly(2));
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(3));
		}

		[Fact]
		public void ModifyWorkManualNotEditableInterval()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval() { Id = 1234, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 10, IsEditable = false };
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>() { manualInterval },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var stats = service.GetStats(day1);
			var workInterval = stats.Result.Works[0];
			var result = service.ModifyWork(workInterval, testWorkData1, new[] { interval6 }, comment);

			Assert.NotNull(stats);
			Assert.Null(stats.Exception);
			Assert.NotNull(stats.Result);
			Assert.Equal(1, stats.Result.Works.Length);
			Assert.NotNull(workInterval);
			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Error, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.AtMostOnce());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Between(1, 2, Range.Inclusive));
		}

		[Fact]
		public void ModifyWorkManualNotEditableIntervalForced()
		{
			var comment = "Correction";
			var workTimeHistoryMock = new Mock<IWorkTimeQuery>();
			workTimeHistoryMock.Setup(x => x.GetStartOfDayOffset(It.IsAny<int>())).Returns(() => dayOffset);
			var manualInterval = new ManualInterval() { Id = 1234, StartDate = interval2.StartDate, EndDate = interval2.EndDate, WorkId = 10, IsEditable = false };
			workTimeHistoryMock.Setup(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory()
			{
				ManualIntervals = new List<ManualInterval>() { manualInterval },
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0)
			});
			workTimeHistoryMock.Setup(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>())).Returns(() => new ClientWorkTimeHistory() { ModificationAgeLimit = new TimeSpan(3, 0, 0, 0) });

			var service = new WorkTimeDummyService(workTimeHistoryMock.Object);
			var stats = service.GetStats(day1);
			var workInterval = stats.Result.Works[0];
			var result = service.ModifyWork(workInterval, testWorkData1, new[] { interval6 }, comment, true);

			Assert.NotNull(stats);
			Assert.Null(stats.Exception);
			Assert.NotNull(stats.Result);
			Assert.Equal(1, stats.Result.Works.Length);
			Assert.NotNull(workInterval);
			Assert.NotNull(result);
			Assert.NotNull(result.Exception);
			Assert.IsType<ValidationException>(result.Exception);
			Assert.Equal(Severity.Error, ((ValidationException)result.Exception).Severity);
			Assert.False(result.Result);

			workTimeHistoryMock.Verify(x => x.Modify(It.IsAny<WorkTimeModifications>(), It.IsAny<int>()), Times.Never());
			workTimeHistoryMock.Verify(x => x.GetStats(day1.StartDate, day1.EndDate, It.IsAny<int>()), Times.Once());
			workTimeHistoryMock.Verify(x => x.GetStats(day2.StartDate, day2.EndDate, It.IsAny<int>()), Times.AtMostOnce());
			workTimeHistoryMock.Verify(x => x.GetStats(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Between(1, 2, Range.Inclusive));
		}
	}
}
