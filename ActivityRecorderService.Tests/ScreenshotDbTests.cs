using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class ScreenshotDbTests : DbTestsBase
	{
		private readonly DateTime now = new DateTime(2011, 05, 18, 12, 00, 00);

		[Fact]
		public void InsertSingleScreenshot()
		{
			//Arrange
			var scrShot = new ScreenShot() { UserId = 1, CreateDate = now, Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), X = 1, Y = 2, Width = 3, Height = 4, ScreenNumber = 5};
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot } };
			WorkItem inserted;

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var loadOpt = new System.Data.Linq.DataLoadOptions();
				loadOpt.LoadWith<WorkItem>(n => n.ScreenShotsInt);
				context.LoadOptions = loadOpt;

				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
			}
			Assert.Equal(1, inserted.ScreenShots.Count);
			Validate(scrShot, inserted.ScreenShots[0]);
		}

		[Fact]
		public void InsertNoScreenshot()
		{
			//Arrange
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1) };
			WorkItem inserted;

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var loadOpt = new System.Data.Linq.DataLoadOptions();
				loadOpt.LoadWith<WorkItem>(n => n.ScreenShotsInt);
				context.LoadOptions = loadOpt;

				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
			}
			Assert.Equal(0, inserted.ScreenShots.Count);
		}
		[Fact]
		public void InsertTwoScreenshot()
		{
			//Arrange
			var scrShot = new ScreenShot() { UserId = 1, CreateDate = now, Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), X = 1, Y = 2, Width = 3, Height = 4, ScreenNumber = 5 };
			var scrShot2 = new ScreenShot() { UserId = 1, CreateDate = now.AddMinutes(1), Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), X = 6, Y = 7, Width = 8, Height = 9, ScreenNumber = 10 };
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot, scrShot2 } };
			WorkItem inserted;

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var loadOpt = new System.Data.Linq.DataLoadOptions();
				loadOpt.LoadWith<WorkItem>(n => n.ScreenShotsInt);
				context.LoadOptions = loadOpt;

				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
			}
			Assert.Equal(2, inserted.ScreenShots.Count);
			Validate(scrShot, inserted.ScreenShots[0]);
			Validate(scrShot2, inserted.ScreenShots[1]);
		}

		[Fact]
		public void InsertOtherIndependentScreenshot()
		{
			//Arrange
			var scrShot = new ScreenShot() { UserId = 1, CreateDate = now, Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), X = 1, Y = 2, Width = 3, Height = 4, ScreenNumber = 5 };
			var scrShot2 = new ScreenShot() { UserId = 1, CreateDate = now.AddHours(2), Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), X = 6, Y = 7, Width = 8, Height = 9, ScreenNumber = 10 };
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot } };
			var workItem2 = new WorkItem() { UserId = 1, StartDate = now.AddHours(2), EndDate = now.AddHours(3), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot2 } };
			WorkItem inserted, inserted2;

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.WorkItems.InsertOnSubmit(workItem2);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var loadOpt = new System.Data.Linq.DataLoadOptions();
				loadOpt.LoadWith<WorkItem>(n => n.ScreenShotsInt);
				context.LoadOptions = loadOpt;

				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
				inserted2 = context.WorkItems.Single(n => n.Id == workItem2.Id);
			}
			Assert.Equal(1, inserted.ScreenShots.Count);
			Validate(scrShot, inserted.ScreenShots[0]);
			Assert.Equal(1, inserted2.ScreenShots.Count);
			Validate(scrShot2, inserted2.ScreenShots[0]);
		}

		[Fact]
		public void InsertIndependentScreenshotsInCloseWorkitems()
		{
			//Arrange
			var scrShot = new ScreenShot() { UserId = 1, CreateDate = now, Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), X = 1, Y = 2, Width = 3, Height = 4, ScreenNumber = 5 };
			var scrShot2 = new ScreenShot() { UserId = 1, CreateDate = now.AddHours(1), Extension = "jpg", Data = new System.Data.Linq.Binary(new byte[0]), X = 6, Y = 7, Width = 8, Height = 9, ScreenNumber = 10 };
			var workItem = new WorkItem() { UserId = 1, StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot } };
			var workItem2 = new WorkItem() { UserId = 1, StartDate = now.AddHours(1), EndDate = now.AddHours(2), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot2 } };
			WorkItem inserted, inserted2;

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.WorkItems.InsertOnSubmit(workItem2);
				context.SubmitChanges();
			}

			//Assert
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var loadOpt = new System.Data.Linq.DataLoadOptions();
				loadOpt.LoadWith<WorkItem>(n => n.ScreenShotsInt);
				context.LoadOptions = loadOpt;

				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
				inserted2 = context.WorkItems.Single(n => n.Id == workItem2.Id);
			}
			Assert.Equal(1, inserted.ScreenShots.Count);
			Validate(scrShot, inserted.ScreenShots[0]);
			Assert.Equal(1, inserted2.ScreenShots.Count);
			Validate(scrShot2, inserted2.ScreenShots[0]);
		}

		private static void Validate(ScreenShot expected, ScreenShot actual)
		{
			Assert.Equal(expected.CreateDate, actual.CreateDate);
			Assert.Equal(expected.UserId, actual.UserId);
			Assert.Equal(expected.Extension, actual.Extension);
			Assert.Equal(expected.X, actual.X);
			Assert.Equal(expected.Y, actual.Y);
			Assert.Equal(expected.Width, actual.Width);
			Assert.Equal(expected.Height, actual.Height);
			Assert.Equal(expected.ScreenNumber, actual.ScreenNumber);
		}
	}
}
