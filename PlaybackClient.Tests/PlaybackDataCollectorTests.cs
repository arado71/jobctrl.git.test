using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Xunit;

namespace PlaybackClient.Tests
{
	public class PlaybackDataCollectorTests : DbTestsBase
	{
		private readonly DateTime baseDate = new DateTime(2011, 05, 18, 12, 00, 00);

		[Fact]
		public void SimpleCollect()
		{
			//Arrange
			var now = baseDate;
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1)};

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
			}

			//Assert
			var collector = new PlaybackDataCollector();
			var collected = collector.GetDataForTest(1, now, now.AddHours(1));

			Assert.Equal(1, collected.WorkItems.Count);
			var workitem = collected.WorkItems[0];
			Assert.Equal(1, workitem.UserId);
			Assert.Equal(now, workitem.StartDate);
			Assert.Equal(now.AddHours(1), workitem.EndDate);
			Assert.Equal(0, workitem.DesktopCaptures.Count);
		}

		[Fact]
		public void SimpleCollectWithScrshot()
		{
			//Arrange
			var now = baseDate.AddHours(1);
			var scrShot = new ScreenShot() { CreateDate = now, Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), ReceiveDate = now, UserId = 1, Height = 10, Width = 10, X = 1, Y = 2, ScreenNumber = 3 };
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot } };

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
			}

			//Assert
			var collector = new PlaybackDataCollector();
			var collected = collector.GetDataForTest(1, now, now.AddHours(1));

			Assert.Equal(1, collected.WorkItems.Count);
			var workitem = collected.WorkItems[0];
			Assert.Equal(1, workitem.UserId);
			Assert.Equal(now, workitem.StartDate);
			Assert.Equal(now.AddHours(1), workitem.EndDate);
			Assert.Equal(1, workitem.DesktopCaptures.Count);
			Assert.Equal(1, workitem.DesktopCaptures[0].Screens.Count);
			var screen = workitem.DesktopCaptures[0].Screens[0];
			Assert.Equal(10, screen.Height);
			Assert.Equal(10, screen.Width);
			Assert.Equal(1, screen.X);
			Assert.Equal(2, screen.Y);
			Assert.Equal(3, screen.ScreenNumber);
			Assert.Equal($"C:\\0\\{workitem.UserId}\\{now:yyyy-MM-dd}\\{now:HH}\\{workitem.UserId}_{now:HH-mm-ss}_{scrShot.ScreenNumber}_{scrShot.Id}.{scrShot.Extension}", screen.ScreenShotPath);
		}

		[Fact]
		public void SimpleCollectWithTwoScrshot()
		{
			//Arrange
			var now = baseDate.AddHours(2);
			var scrShot = new ScreenShot() { CreateDate = now, Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), ReceiveDate = now, UserId = 1, Height = 10, Width = 10, X = 1, Y = 2, ScreenNumber = 3 };
			var scrShot2 = new ScreenShot() { CreateDate = now.AddMinutes(1), Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), ReceiveDate = now.AddMinutes(1), UserId = 1, Height = 20, Width = 30, X = 4, Y = 5, ScreenNumber = 6 };
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot, scrShot2 } };

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
			}

			//Assert
			var collector = new PlaybackDataCollector();
			var collected = collector.GetDataForTest(1, now, now.AddHours(1));

			Assert.Equal(1, collected.WorkItems.Count);
			var workitem = collected.WorkItems[0];
			Assert.Equal(1, workitem.UserId);
			Assert.Equal(now, workitem.StartDate);
			Assert.Equal(now.AddHours(1), workitem.EndDate);
			Assert.Equal(2, workitem.DesktopCaptures.Count);
			Assert.Equal(1, workitem.DesktopCaptures[0].Screens.Count);
			var screen = workitem.DesktopCaptures[0].Screens[0];
			Assert.Equal(10, screen.Height);
			Assert.Equal(10, screen.Width);
			Assert.Equal(1, screen.X);
			Assert.Equal(2, screen.Y);
			Assert.Equal(3, screen.ScreenNumber);
			Assert.Equal($"C:\\0\\{workitem.UserId}\\{scrShot.CreateDate:yyyy-MM-dd}\\{scrShot.CreateDate:HH}\\{workitem.UserId}_{scrShot.CreateDate:HH-mm-ss}_{scrShot.ScreenNumber}_{scrShot.Id}.{scrShot.Extension}", screen.ScreenShotPath);
			Assert.Equal(1, workitem.DesktopCaptures[1].Screens.Count);
			var screen2 = workitem.DesktopCaptures[1].Screens[0];
			Assert.Equal(20, screen2.Height);
			Assert.Equal(30, screen2.Width);
			Assert.Equal(4, screen2.X);
			Assert.Equal(5, screen2.Y);
			Assert.Equal(6, screen2.ScreenNumber);
			Assert.Equal($"C:\\0\\{workitem.UserId}\\{scrShot2.CreateDate:yyyy-MM-dd}\\{scrShot2.CreateDate:HH}\\{workitem.UserId}_{scrShot2.CreateDate:HH-mm-ss}_{scrShot2.ScreenNumber}_{scrShot2.Id}.{scrShot2.Extension}", screen2.ScreenShotPath);
		}

		[Fact]
		public void SimpleCollectWithOtherScrshot()
		{
			//Arrange
			var now = baseDate.AddHours(3);
			var scrShot = new ScreenShot() { CreateDate = now, Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), ReceiveDate = now, UserId = 1, Height = 10, Width = 10, X = 1, Y = 2, ScreenNumber = 3 };
			var scrShot2 = new ScreenShot() { CreateDate = now.AddHours(2), Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), ReceiveDate = now.AddMinutes(1), UserId = 1, Height = 20, Width = 30, X = 4, Y = 5, ScreenNumber = 6 };
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot } };
			var workItem2 = new WorkItem() { UserId = 1, StartDate = now.AddHours(2), EndDate = now.AddHours(3), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot2 } };

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.WorkItems.InsertOnSubmit(workItem2);
				context.SubmitChanges();
			}

			//Assert
			var collector = new PlaybackDataCollector();
			var collected = collector.GetDataForTest(1, now, now.AddHours(1));

			Assert.Equal(1, collected.WorkItems.Count);
			var workitem = collected.WorkItems[0];
			Assert.Equal(1, workitem.UserId);
			Assert.Equal(now, workitem.StartDate);
			Assert.Equal(now.AddHours(1), workitem.EndDate);
			Assert.Equal(1, workitem.DesktopCaptures.Count);
			Assert.Equal(1, workitem.DesktopCaptures[0].Screens.Count);
			var screen = workitem.DesktopCaptures[0].Screens[0];
			Assert.Equal(10, screen.Height);
			Assert.Equal(10, screen.Width);
			Assert.Equal(1, screen.X);
			Assert.Equal(2, screen.Y);
			Assert.Equal(3, screen.ScreenNumber);
			Assert.Equal($"C:\\0\\{workitem.UserId}\\{now:yyyy-MM-dd}\\{now:HH}\\{workitem.UserId}_{now:HH-mm-ss}_{scrShot.ScreenNumber}_{scrShot.Id}.{scrShot.Extension}", screen.ScreenShotPath);
		}

		[Fact]
		public void SimpleCollectWithScrshotsInCloseWorkitem()
		{
			//Arrange
			var now = baseDate.AddHours(4);
			var scrShot = new ScreenShot() { CreateDate = now, Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), ReceiveDate = now, UserId = 1, Height = 10, Width = 10, X = 1, Y = 2, ScreenNumber = 3 };
			var scrShot2 = new ScreenShot() { CreateDate = now.AddHours(1), Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), ReceiveDate = now.AddMinutes(1), UserId = 1, Height = 20, Width = 30, X = 4, Y = 5, ScreenNumber = 6 };
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot } };
			var workItem2 = new WorkItem() { UserId = 1, StartDate = now.AddHours(1), EndDate = now.AddHours(2), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot2 } };

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.WorkItems.InsertOnSubmit(workItem2);
				context.SubmitChanges();
			}

			//Assert
			var collector = new PlaybackDataCollector();
			var collected = collector.GetDataForTest(1, now, now.AddHours(1));

			Assert.Equal(1, collected.WorkItems.Count);
			var workitem = collected.WorkItems[0];
			Assert.Equal(1, workitem.UserId);
			Assert.Equal(now, workitem.StartDate);
			Assert.Equal(now.AddHours(1), workitem.EndDate);
			Assert.Equal(1, workitem.DesktopCaptures.Count);
			Assert.Equal(1, workitem.DesktopCaptures[0].Screens.Count);
			var screen = workitem.DesktopCaptures[0].Screens[0];
			Assert.Equal(10, screen.Height);
			Assert.Equal(10, screen.Width);
			Assert.Equal(1, screen.X);
			Assert.Equal(2, screen.Y);
			Assert.Equal(3, screen.ScreenNumber);
			Assert.Equal($"C:\\0\\{workitem.UserId}\\{now:yyyy-MM-dd}\\{now:HH}\\{workitem.UserId}_{now:HH-mm-ss}_{scrShot.ScreenNumber}_{scrShot.Id}.{scrShot.Extension}", screen.ScreenShotPath);
		}

	}
}
