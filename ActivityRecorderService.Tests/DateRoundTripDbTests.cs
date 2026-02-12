using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Tct.ActivityRecorderService;

namespace Tct.Tests.ActivityRecorderService
{
	public class DateRoundTripDbTests : DbTestsBase
	{
		private readonly DateTime now = new DateTime(2011, 05, 18, 12, 00, 00);

		[Fact]
		public void WorkItemsDateUpdatedInLinqSameContext()
		{
			//Arrange
			var workItem = new WorkItem() { StartDate = now.AddTicks(-1), EndDate = now.AddHours(1) };
			WorkItem inserted;

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
			}

			//Assert
			Assert.Equal("2011-05-18 11:59:59", workItem.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
			Assert.Equal(workItem.StartDate.ToString("yyyy-MM-dd HH:mm:ss"), inserted.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
		}

		[Fact]
		public void ScreenShotsDateUpdatedInLinqSameContext()
		{
			//Arrange
			var scrShot = new ScreenShot() { CreateDate = now.AddTicks(-1), Extension = "", Data = new System.Data.Linq.Binary(new byte[0]) };
			var workItem = new WorkItem() { StartDate = now.AddTicks(-1), EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot } };
			WorkItem inserted;

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var loadOpt = new System.Data.Linq.DataLoadOptions();
				loadOpt.LoadWith<WorkItem>(n => n.ScreenShots);
				context.LoadOptions = loadOpt;

				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
			}

			//Assert
			Assert.Equal("2011-05-18 12:00:00", workItem.ScreenShots[0].CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
			Assert.Equal(workItem.ScreenShots[0].CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), inserted.ScreenShots[0].CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
		}

		[Fact]
		public void WorkItemsDateUpdatedInLinqDifferentContext()
		{
			//Arrange
			var workItem = new WorkItem() { StartDate = now.AddTicks(-1), EndDate = now.AddHours(1) };
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
				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
			}
			Assert.NotEqual(workItem.StartDate.ToString("yyyy-MM-dd HH:mm:ss"), inserted.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
			Assert.Equal("2011-05-18 11:59:59", workItem.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
			Assert.Equal("2011-05-18 12:00:00", inserted.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
		}

		[Fact]
		public void ScreenShotsDateUpdatedInLinqDifferentContext()
		{
			//Arrange
			var scrShot = new ScreenShot() { CreateDate = now.AddTicks(-1), Extension = "", Data = new System.Data.Linq.Binary(new byte[0]) };
			var workItem = new WorkItem() { StartDate = now.AddTicks(-1), EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() { scrShot } };
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
			Assert.Equal("2011-05-18 12:00:00", workItem.ScreenShots[0].CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
			Assert.Equal(workItem.ScreenShots[0].CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), inserted.ScreenShots[0].CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
		}

		[Fact]
		public void GetSqlRoundTripDateTimeIsTheSameAsLinqInserting()
		{
			//Arrange
			var workItem = new WorkItem() { StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() };
			//for (DateTime i = now; i < now.AddMilliseconds(10); i = i.AddTicks(1L)) //1:40mins
			for (DateTime i = now.AddMilliseconds(9); i < now.AddMilliseconds(10); i = i.AddTicks(1L))
			{
				workItem.ScreenShots.Add(new ScreenShot() { CreateDate = i, Extension = i.ToString("ss.fffffff"), Data = new System.Data.Linq.Binary(new byte[0]) });
			}
			WorkItem inserted;

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
			}
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var loadOpt = new System.Data.Linq.DataLoadOptions();
				loadOpt.LoadWith<WorkItem>(n => n.ScreenShotsInt);
				context.LoadOptions = loadOpt;

				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
			}

			//Assert
			for (int i = 0; i < workItem.ScreenShots.Count; i++)
			{
				Assert.True(workItem.ScreenShots[i].CreateDate.ToSqlRoundTripDateTime() == inserted.ScreenShots[i].CreateDate, i + " " + workItem.ScreenShots[i].CreateDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff") + " " + workItem.ScreenShots[i].CreateDate.ToSqlRoundTripDateTime().ToString("yyyy-MM-dd HH:mm:ss.ffffff") + " " + inserted.ScreenShots[i].CreateDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
			}
		}

		[Fact]
		public void InsertSqlRoundTripDateTimes()
		{
			//Arrange
			var workItem = new WorkItem() { StartDate = now, EndDate = now.AddHours(1), ScreenShots = new System.Data.Linq.EntitySet<ScreenShot>() };
			//for (DateTime i = now; i < now.AddMilliseconds(10); i = i.AddTicks(1L)) //1:40mins
			for (DateTime i = now.AddMilliseconds(9); i < now.AddMilliseconds(10); i = i.AddTicks(1L))
			{
				workItem.ScreenShots.Add(new ScreenShot() { CreateDate = i.ToSqlRoundTripDateTime(), Extension = i.ToString("ss.fffffff"), Data = new System.Data.Linq.Binary(new byte[0]) });
			}
			WorkItem inserted;

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.WorkItems.InsertOnSubmit(workItem);
				context.SubmitChanges();
			}
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var loadOpt = new System.Data.Linq.DataLoadOptions();
				loadOpt.LoadWith<WorkItem>(n => n.ScreenShotsInt);
				context.LoadOptions = loadOpt;

				inserted = context.WorkItems.Single(n => n.Id == workItem.Id);
			}

			//Assert
			for (int i = 0; i < workItem.ScreenShots.Count; i++)
			{
				Assert.True(workItem.ScreenShots[i].CreateDate == inserted.ScreenShots[i].CreateDate, i + " " + workItem.ScreenShots[i].CreateDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff") + " " + workItem.ScreenShots[i].CreateDate.ToSqlRoundTripDateTime().ToString("yyyy-MM-dd HH:mm:ss.ffffff") + " " + inserted.ScreenShots[i].CreateDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
			}
		}

	}
}
