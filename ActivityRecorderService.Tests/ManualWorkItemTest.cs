using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class ManualWorkItemTest : DbTestsBase
	{
		private static ManualDataClassesDataContext GetContext()
		{
			return new ManualDataClassesDataContext();
		}

		[Fact]
		public void CannotInsertDataWhereEndDateIsLessThanStartDate()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			//Act
			using (var context = GetContext())
			{
				var now = DateTime.UtcNow;
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 0,
					StartDate = now,
					EndDate = now.AddMinutes(-2),
				});
				//Assert
				Assert.Throws<SqlException>(() => context.SubmitChanges());
			}
		}

		[Fact]
		public void CanInsertDataWhereEndDateIsNotLessThanStartDate()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();

			//Act
			using (var context = GetContext())
			{
				var now = DateTime.UtcNow;
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 0,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 0,
					StartDate = now,
					EndDate = now,
				});
				//Assert
				Assert.DoesNotThrow(() => context.SubmitChanges());
			}
		}


		[Fact]
		public void ServerSourcesAreInitiated()
		{
			using (var context = GetContext())
			{
				Assert.False(context.ManualWorkItemSources.Any());
			}

			ManualWorkItemTypeHelper.InitializeDbData();

			using (var context = GetContext())
			{
				context.ManualWorkItemSources.Single(n => n.SourceId == (byte)ManualWorkItemSourceEnum.ServerAdhocMeeting);
				context.ManualWorkItemSources.Single(n => n.SourceId == (byte)ManualWorkItemSourceEnum.Server);
			}
		}

		[Fact]
		public void CanInsertWithOutSource()
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();
			var now = new DateTime(2013, 10, 14, 18, 00, 00);

			//Act
			using (var context = GetContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 1,
					StartDate = now,
					EndDate = now.AddMinutes(2),
				});
				context.SubmitChanges();
			}

			//Assert
			using (var context = GetContext())
			{
				var res = context.ManualWorkItems.Single();
				Assert.Equal(null, res.SourceId);
				Assert.Equal(ManualWorkItemTypeEnum.AddWork, res.ManualWorkItemTypeId);
				Assert.Equal(1, res.WorkId);
				Assert.Equal(now, res.StartDate);
				Assert.Equal(now.AddMinutes(2), res.EndDate);
			}
		}

		[Fact]
		public void CanInsertWithServerSource()
		{
			CanInsertWithSource(ManualWorkItemSourceEnum.Server);
		}

		[Fact]
		public void CanInsertWithServerAdhocMeetingSource()
		{
			CanInsertWithSource(ManualWorkItemSourceEnum.ServerAdhocMeeting);
		}

		public void CanInsertWithSource(ManualWorkItemSourceEnum source)
		{
			//Arrange
			ManualWorkItemTypeHelper.InitializeDbData();
			var now = new DateTime(2013, 10, 14, 18, 00, 00);

			//Act
			using (var context = GetContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = 1,
					StartDate = now,
					EndDate = now.AddMinutes(2),
					SourceId = (byte)source,
				});
				context.SubmitChanges();
			}

			//Assert
			using (var context = GetContext())
			{
				var res = context.ManualWorkItems.Single();
				Assert.Equal((byte)source, res.SourceId);
				Assert.Equal(ManualWorkItemTypeEnum.AddWork, res.ManualWorkItemTypeId);
				Assert.Equal(1, res.WorkId);
				Assert.Equal(now, res.StartDate);
				Assert.Equal(now.AddMinutes(2), res.EndDate);
			}
		}
	}
}
