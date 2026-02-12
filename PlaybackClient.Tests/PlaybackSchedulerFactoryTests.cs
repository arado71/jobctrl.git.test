using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using PlaybackClient.ActivityRecorderServiceReference;
using Xunit;

namespace PlaybackClient.Tests
{
	public class PlaybackSchedulerFactoryTests
	{
		private static readonly DateTime now = new DateTime(2013, 09, 03, 10, 00, 00);
		private readonly Mock<IPlaybackDataSender> senderMock;
		private readonly Mock<IPlaybackDataCollector> collectorMock;

		public PlaybackSchedulerFactoryTests()
		{
			senderMock = new Mock<IPlaybackDataSender>();
			collectorMock = new Mock<IPlaybackDataCollector>();
			PlaybackSchedulerFactory.SenderFactory = (Func<IPlaybackDataSender>)(() => senderMock.Object);
			PlaybackSchedulerFactory.CollectorFactory = (Func<IPlaybackDataCollector>)(() => collectorMock.Object);
		}

		[Fact]
		public void OneTimeImportWontChangeTimeOfDay()
		{
			var start = now;
			var end = now.AddDays(1);
			var curr = end.AddDays(3).AddHours(6);
			var done = new DoneDetector(2);
			Action validation = null;
			collectorMock
				.Setup(ctx => ctx.GetDataFor(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns<int, DateTime, DateTime>((userId, startDate, endDate) => new PlaybackData()
				{
					UserId = userId,
					StartDate = startDate,
					EndDate = endDate,
					WorkItems = new List<WorkItem>()
					{
						new WorkItem() { UserId = userId, StartDate = startDate, EndDate = startDate.AddTicks((endDate-startDate).Ticks/2), KeyboardActivity = 1},
						new WorkItem() { UserId = userId, StartDate = startDate.AddTicks((endDate-startDate).Ticks/2), EndDate = endDate, KeyboardActivity = 2},
					}
				})
				.Callback(() => done.Signal());
			senderMock
				.Setup(ctx => ctx.SendAsync(It.IsAny<List<PlaybackDataItem>>()))
				.Callback<List<PlaybackDataItem>>(data =>
				{
					validation = () => //Assert doesn't work here...
					{
						Assert.Equal(2, data.Count);
						Assert.Equal(13, data[0].WorkItem.UserId);
						Assert.Equal(start.TimeOfDay, data[0].WorkItem.StartDate.TimeOfDay);
						Assert.Equal(end.TimeOfDay, data[1].WorkItem.EndDate.TimeOfDay);
					};
					done.Signal();
				});

			DateTimeEx.Now = () => curr;
			bool d;
			using (PlaybackSchedulerFactory.OneTimeImport(13, start, end))
			{
				d = done.Wait(TimeSpan.FromSeconds(5));
			}
			Assert.NotNull(validation);
			validation();
			Assert.True(d);
		}

		[Fact]
		public void OneTimeImportCanMapUserId()
		{
			var start = now;
			var end = now.AddDays(1);
			var curr = end.AddDays(3).AddHours(6);
			var done = new DoneDetector(2);
			Action validation = null;
			collectorMock
				.Setup(ctx => ctx.GetDataFor(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns<int, DateTime, DateTime>((userId, startDate, endDate) => new PlaybackData()
				{
					UserId = userId,
					StartDate = startDate,
					EndDate = endDate,
					WorkItems = new List<WorkItem>()
					{
						new WorkItem() { UserId = userId, StartDate = startDate, EndDate = startDate.AddTicks((endDate-startDate).Ticks/2), KeyboardActivity = 1},
						new WorkItem() { UserId = userId, StartDate = startDate.AddTicks((endDate-startDate).Ticks/2), EndDate = endDate, KeyboardActivity = 2},
					},
					ManualWorkItems = new List<ManualWorkItem>()
					{
						new ManualWorkItem() { UserId= userId, StartDate = startDate, EndDate = endDate },
					},
				})
				.Callback(() => done.Signal());
			senderMock
				.Setup(ctx => ctx.SendAsync(It.IsAny<List<PlaybackDataItem>>()))
				.Callback<List<PlaybackDataItem>>(data =>
				{
					validation = () => //Assert doesn't work here...
					{
						Assert.Equal(3, data.Count);
						Assert.Equal(14, data[0].WorkItem.UserId);
						Assert.Equal(14, data[1].WorkItem.UserId);
						Assert.Equal(14, data[2].ManualWorkItem.UserId);
						Assert.Equal(start.TimeOfDay, data[0].WorkItem.StartDate.TimeOfDay);
						Assert.Equal(end.TimeOfDay, data[1].WorkItem.EndDate.TimeOfDay);
						Assert.Equal(start.TimeOfDay, data[2].ManualWorkItem.StartDate.TimeOfDay);
						Assert.Equal(end.TimeOfDay, data[2].ManualWorkItem.EndDate.TimeOfDay);
					};
					done.Signal();
				});

			DateTimeEx.Now = () => curr;
			bool d;
			using (PlaybackSchedulerFactory.OneTimeImport(13, start, end, 14))
			{
				d = done.Wait(TimeSpan.FromSeconds(5));
			}
			Assert.NotNull(validation);
			validation();
			Assert.True(d);
		}

		[Fact]
		public void OneTimeImportCanMapUserAndWorkId()
		{
			var start = now;
			var end = now.AddDays(1);
			var curr = end.AddDays(3).AddHours(6);
			var done = new DoneDetector(2);
			Action validation = null;
			collectorMock
				.Setup(ctx => ctx.GetDataFor(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns<int, DateTime, DateTime>((userId, startDate, endDate) => new PlaybackData()
				{
					UserId = userId,
					StartDate = startDate,
					EndDate = endDate,
					WorkItems = new List<WorkItem>()
					{
						new WorkItem() { UserId = userId, StartDate = startDate, EndDate = startDate.AddTicks((endDate-startDate).Ticks/2), KeyboardActivity = 1, WorkId = 1},
						new WorkItem() { UserId = userId, StartDate = startDate.AddTicks((endDate-startDate).Ticks/2), EndDate = endDate, KeyboardActivity = 2, WorkId = 1},
					},
					ManualWorkItems = new List<ManualWorkItem>()
					{
						new ManualWorkItem() { UserId= userId, StartDate = startDate, EndDate = endDate, WorkId = 1 },
					},
					MobileWorkItems = new List<MobileWorkItem>()
					{
						new MobileWorkItem() { UserId= userId, StartDate = startDate, EndDate = endDate, WorkId = 1 },
					},
				})
				.Callback(() => done.Signal());
			senderMock
				.Setup(ctx => ctx.SendAsync(It.IsAny<List<PlaybackDataItem>>()))
				.Callback<List<PlaybackDataItem>>(data =>
				{
					validation = () => //Assert doesn't work here...
					{
						Assert.Equal(14, data[0].WorkItem.UserId);
						Assert.Equal(2, data[0].WorkItem.WorkId);
						Assert.Equal(14, data[1].WorkItem.UserId);
						Assert.Equal(2, data[1].WorkItem.WorkId);
						Assert.Equal(14, data[2].ManualWorkItem.UserId);
						Assert.Equal(2, data[2].ManualWorkItem.WorkId);
						Assert.Equal(14, data[3].MobileRequest.UserId);
						Assert.Equal(2, data[3].MobileRequest.WorkItems[0].WorkId);
						for (int i = 4; i < data.Count; i++)
						{
							Assert.Equal(14, data[i].MobileRequest.UserId);
							Assert.Equal(2, data[i].MobileRequest.WorkItems[0].WorkId);
						}
						Assert.Equal(start.TimeOfDay, data[0].WorkItem.StartDate.TimeOfDay);
						Assert.Equal(end.TimeOfDay, data[1].WorkItem.EndDate.TimeOfDay);
						Assert.Equal(start.TimeOfDay, data[2].ManualWorkItem.StartDate.TimeOfDay);
						Assert.Equal(end.TimeOfDay, data[2].ManualWorkItem.EndDate.TimeOfDay);
					};
					done.Signal();
				});

			DateTimeEx.Now = () => curr;
			bool d;
			using (PlaybackSchedulerFactory.OneTimeImport(13, start, end, 14, n => n + 1))
			{
				d = done.Wait(TimeSpan.FromSeconds(5));
			}
			Assert.NotNull(validation);
			validation();
			Assert.True(d);
		}
	}
}
