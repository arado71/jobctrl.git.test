using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using PlaybackClient.ActivityRecorderServiceReference;
using Scheduling;
using Xunit;

namespace PlaybackClient.Tests
{
	public class PlaybackDataSchedulerTests : IDisposable
	{
		private static readonly DateTime now = new DateTime(2013, 09, 03, 10, 00, 00);
		private readonly Mock<PlaybackDataScheduler> schedulerMock;
		private readonly Mock<IPlaybackDataSender> senderMock;
		private readonly Mock<IPlaybackDataCollector> collectorMock;
		private readonly Mock<PlaybackDataConverterTest> converterMock;
		private readonly PlaybackDataScheduler scheduler;

		public PlaybackDataSchedulerTests()
		{
			senderMock = new Mock<IPlaybackDataSender>();
			collectorMock = new Mock<IPlaybackDataCollector>();
			converterMock = new Mock<PlaybackDataConverterTest>() { CallBase = true };
			senderMock.As<IDisposable>().Setup(n => n.Dispose());
			collectorMock.As<IDisposable>().Setup(n => n.Dispose());
			converterMock.As<IDisposable>().Setup(n => n.Dispose());
			schedulerMock = new Mock<PlaybackDataScheduler>(
				(Func<IPlaybackDataSender>)(() => senderMock.Object),
				(Func<IPlaybackDataCollector>)(() => collectorMock.Object),
				(Func<IPlaybackDataConverter>)(() => converterMock.Object)) { CallBase = true };
			scheduler = schedulerMock.Object;
		}

		[Fact]
		public void SimpleFutureSend()
		{
			var done = new DoneDetector(3);
			collectorMock
				.Setup(ctx => ctx.GetDataFor(1, now, now.AddHours(1)))
				.Returns(new PlaybackData()
				{
					UserId = 1,
					StartDate = now,
					EndDate = now.AddHours(1),
					WorkItems = new List<WorkItem>()
					{
						new WorkItem() { UserId = 1, StartDate = now.AddMinutes(1), EndDate = now.AddMinutes(2),},
					}
				})
				.Callback(() => done.Signal());
			senderMock
				.Setup(ctx => ctx.SendAsync(It.IsAny<List<PlaybackDataItem>>()))
				.Callback<List<PlaybackDataItem>>(data =>
				{
					Assert.NotNull(data);
					Assert.Equal(1, data.Count);
					Assert.NotNull(data[0].WorkItem);
					Assert.Equal(1, data[0].WorkItem.UserId);
					Assert.Equal(now.AddHours(2).AddMinutes(1), data[0].WorkItem.StartDate);
					Assert.Equal(now.AddHours(2).AddMinutes(2), data[0].WorkItem.EndDate);
					done.Signal();
				});
			converterMock
				.Setup(ctx => ctx.GetActualizedItemsTest(It.IsAny<PlaybackData>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Callback<PlaybackData, DateTime, DateTime>((data, start, send) =>
				{
					Assert.Equal(1, data.UserId);
					Assert.Equal(now, data.StartDate);
					Assert.Equal(now.AddHours(1), data.EndDate);
					Assert.Equal(1, data.WorkItems.Count);
					Assert.Equal(1, data.WorkItems[0].UserId);
					Assert.Equal(now.AddMinutes(1), data.WorkItems[0].StartDate); //it's not modified yet
					Assert.Equal(now.AddMinutes(2), data.WorkItems[0].EndDate);
					Assert.Equal(start, now.AddHours(2));
					Assert.Equal(send, now.AddHours(2));
					done.Signal();
				});
			DateTimeEx.UtcNow = () => now.AddHours(2);
			scheduler.Add(new PlaybackSchedule()
			{
				UserId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
				FirstScheduleDate = now.AddHours(2),
				TimeZoneId = TimeZoneInfo.Utc.Id,
				LocalSchedule = Schedule.CreateOneTime(now.AddHours(2))
			});
			Assert.True(done.Wait(TimeSpan.FromSeconds(5)));
		}

		[Fact]
		public void SimpleResumeSend()
		{
			var done = new DoneDetector(2);
			collectorMock
				.Setup(ctx => ctx.GetDataFor(4, now, now.AddHours(1)))
				.Returns(new PlaybackData()
				{
					UserId = 4,
					StartDate = now,
					EndDate = now.AddHours(1),
					WorkItems = new List<WorkItem>()
		            {
		                new WorkItem() { UserId = 4, StartDate = now.AddMinutes(1), EndDate = now.AddMinutes(2),},
		                new WorkItem() { UserId = 4, StartDate = now.AddMinutes(2), EndDate = now.AddMinutes(3),},
		            }
				})
				.Callback(() => done.Signal());
			senderMock
				.Setup(ctx => ctx.SendAsync(It.IsAny<List<PlaybackDataItem>>()))
				.Callback<List<PlaybackDataItem>>(data =>
				{
					Assert.Equal(1, data.Count); //old data is discarded
					Assert.Equal(4, data[0].WorkItem.UserId);
					Assert.Equal(now.AddHours(2).AddMinutes(2), data[0].WorkItem.StartDate);
					Assert.Equal(now.AddHours(2).AddMinutes(3), data[0].WorkItem.EndDate);
					done.Signal();
				});
			DateTimeEx.UtcNow = () => now.AddHours(2).AddMinutes(2);
			scheduler.Add(new PlaybackSchedule()
			{
				UserId = 4,
				StartDate = now,
				EndDate = now.AddHours(1),
				FirstScheduleDate = now.AddHours(2),
				TimeZoneId = TimeZoneInfo.Utc.Id,
				LocalSchedule = Schedule.CreateOneTime(now.AddHours(2))
			});
			Assert.True(done.Wait(TimeSpan.FromSeconds(5)));
		}

		[Fact]
		public void LaterFutureSend()
		{
			var done = new DoneDetector(2);
			collectorMock
				.Setup(ctx => ctx.GetDataFor(1, now, now.AddHours(1)))
				.Returns(new PlaybackData()
				{
					UserId = 1,
					StartDate = now,
					EndDate = now.AddHours(1),
					WorkItems = new List<WorkItem>()
		            {
		                new WorkItem() { UserId = 1, StartDate = now.AddMinutes(1), EndDate = now.AddMinutes(2),},
		            }
				})
				.Callback(() => done.Signal());
			senderMock
				.Setup(ctx => ctx.SendAsync(It.IsAny<List<PlaybackDataItem>>()))
				.Callback<List<PlaybackDataItem>>(data =>
				{
					Assert.Equal(1, data.Count);
					Assert.Equal(1, data[0].WorkItem.UserId);
					Assert.Equal(now.AddHours(2).AddMinutes(1), data[0].WorkItem.StartDate);
					Assert.Equal(now.AddHours(2).AddMinutes(2), data[0].WorkItem.EndDate);
					done.Signal();
				});
			DateTimeEx.UtcNow = () => now.AddHours(2).AddSeconds(-0.3);
			scheduler.Add(new PlaybackSchedule()
			{
				UserId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
				FirstScheduleDate = now.AddHours(2),
				TimeZoneId = TimeZoneInfo.Utc.Id,
				LocalSchedule = Schedule.CreateOneTime(now.AddHours(2))
			});
			Assert.False(done.Wait(TimeSpan.Zero));
			DateTimeEx.UtcNow = () => now.AddHours(2).AddMinutes(5);
			Assert.True(done.Wait(TimeSpan.FromSeconds(5)));
		}

		[Fact]
		public void LaterFutureSendTwo()
		{
			var done = new DoneDetector(2);
			collectorMock
				.Setup(ctx => ctx.GetDataFor(1, now, now.AddHours(1)))
				.Returns(new PlaybackData()
				{
					UserId = 1,
					StartDate = now,
					EndDate = now.AddHours(1),
					WorkItems = new List<WorkItem>()
		            {
		                new WorkItem() { UserId = 1, StartDate = now.AddMinutes(1), EndDate = now.AddMinutes(2), KeyboardActivity = 1, },
		                new WorkItem() { UserId = 1, StartDate = now.AddMinutes(2), EndDate = now.AddMinutes(3), KeyboardActivity = 2, },
		            }
				})
				.Callback(() => done.Signal());
			senderMock
				.Setup(ctx => ctx.SendAsync(It.IsAny<List<PlaybackDataItem>>()))
				.Callback<List<PlaybackDataItem>>(data =>
				{
					Assert.Equal(2, data.Count);
					Assert.Equal(1, data[0].WorkItem.UserId);
					Assert.Equal(now.AddHours(2).AddMinutes(1), data[0].WorkItem.StartDate);
					Assert.Equal(now.AddHours(2).AddMinutes(2), data[0].WorkItem.EndDate);
					Assert.Equal(1, data[0].WorkItem.KeyboardActivity);
					Assert.Equal(1, data[1].WorkItem.UserId);
					Assert.Equal(now.AddHours(2).AddMinutes(2), data[1].WorkItem.StartDate);
					Assert.Equal(now.AddHours(2).AddMinutes(3), data[1].WorkItem.EndDate);
					Assert.Equal(2, data[1].WorkItem.KeyboardActivity);
					done.Signal();
				});
			DateTimeEx.UtcNow = () => now.AddHours(2).AddSeconds(-0.3);
			scheduler.Add(new PlaybackSchedule()
			{
				UserId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
				FirstScheduleDate = now.AddHours(2),
				TimeZoneId = TimeZoneInfo.Utc.Id,
				LocalSchedule = Schedule.CreateOneTime(now.AddHours(2))
			});
			Assert.False(done.Wait(TimeSpan.Zero));
			DateTimeEx.UtcNow = () => now.AddHours(2).AddMinutes(5);
			Assert.True(done.Wait(TimeSpan.FromSeconds(5)));
		}

		[Fact]
		public void LaterFutureSendRetry()
		{
			var done1 = new DoneDetector(2);
			var done2 = new DoneDetector();
			var calls = 0;
			collectorMock
				.Setup(ctx => ctx.GetDataFor(1, now, now.AddHours(1)))
				.Returns(() =>
				{
					done1.Signal();
					calls++;
					if (calls == 1) throw new Exception();
					return new PlaybackData()
						{
							UserId = 1,
							StartDate = now,
							EndDate = now.AddHours(1),
							WorkItems = new List<WorkItem>()
				         				    {
				         				        new WorkItem() {UserId = 1, StartDate = now.AddMinutes(1), EndDate = now.AddMinutes(2),},
				         				    }
						};
				});
			senderMock
				.Setup(ctx => ctx.SendAsync(It.IsAny<List<PlaybackDataItem>>()))
				.Callback<List<PlaybackDataItem>>(data =>
				{
					Assert.Equal(1, data.Count);
					Assert.Equal(1, data[0].WorkItem.UserId);
					Assert.Equal(now.AddHours(2).AddMinutes(1), data[0].WorkItem.StartDate);
					Assert.Equal(now.AddHours(2).AddMinutes(2), data[0].WorkItem.EndDate);
					done2.Signal();
				});
			schedulerMock
				.Setup(ctx => ctx.GetScheduleRetries())
				.Returns(1);
			schedulerMock
				.Setup(ctx => ctx.GetScheduleBaseRetryIntervalInSec())
				.Returns(1);
			DateTimeEx.UtcNow = () => now.AddHours(2).AddSeconds(-0.3);
			scheduler.Add(new PlaybackSchedule()
			{
				UserId = 1,
				StartDate = now,
				EndDate = now.AddHours(1),
				FirstScheduleDate = now.AddHours(2),
				TimeZoneId = TimeZoneInfo.Utc.Id,
				LocalSchedule = Schedule.CreateOneTime(now.AddHours(2))
			});
			DateTimeEx.UtcNow = () => now.AddHours(2).AddMinutes(5);
			Assert.True(done1.Wait(TimeSpan.FromSeconds(5)));
			Assert.True(done2.Wait(TimeSpan.FromSeconds(5)));
		}

		public void Dispose()
		{
			scheduler.Dispose();
			senderMock.As<IDisposable>().Verify(n => n.Dispose());
			collectorMock.As<IDisposable>().Verify(n => n.Dispose());
			converterMock.As<IDisposable>().Verify(n => n.Dispose());
		}

		public class PlaybackDataConverterTest : PlaybackDataConverter
		{
			public virtual void GetActualizedItemsTest(PlaybackData data, DateTime utcNewStartDate, DateTime utcSendFromDate)
			{
				//dummy method for testing
			}

			public override List<PlaybackDataItem> GetActualizedItems(PlaybackData data, DateTime utcNewStartDate, DateTime utcSendFromDate)
			{
				GetActualizedItemsTest(data, utcNewStartDate, utcSendFromDate); //call dummy so we can test it
				return base.GetActualizedItems(data, utcNewStartDate, utcSendFromDate);
			}
		}
	}
}
