using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class ManualWorkItemTypeTests : DbTestsBase
	{
		private static ManualDataClassesDataContext GetContext()
		{
			return new ManualDataClassesDataContext();
		}

		[Fact]
		public void CanInitializeDbData()
		{
			//Arrange
			var mwtEnum = Enum.GetValues(typeof(ManualWorkItemTypeEnum)).Cast<ManualWorkItemTypeEnum>();
			var count = mwtEnum.Count();
			var isWorkIdReqCount = mwtEnum.Where(ManualWorkItemTypeHelper.IsWorkIdRequired).Count();

			//Act
			ManualWorkItemTypeHelper.InitializeDbData();

			//Assert
			using (var context = GetContext())
			{
				var res = context.ManualWorkItemTypes.ToDictionary(n => n.Id);
				Assert.Equal(count, res.Count);
				Assert.Equal(isWorkIdReqCount, res.Where(n => n.Value.IsWorkIdRequired).Count());
				foreach (var manualWorkItemTypeInDb in res)
				{
					manualWorkItemTypeInDb.Value.IsWorkIdRequired = ManualWorkItemTypeHelper.IsWorkIdRequired(manualWorkItemTypeInDb.Value.Id);
					manualWorkItemTypeInDb.Value.Name = manualWorkItemTypeInDb.Value.Id.Description();
				}
			}
		}

		[Fact]
		public void CanInitializeDbData2Times()
		{
			CanInitializeDbData();
			CanInitializeDbData();
		}

		[Fact]
		public void CanUpdateDbInInitializeDbData()
		{
			CanInitializeDbData();
			using (var context = GetContext())
			{
				var types = context.ManualWorkItemTypes.ToList();
				foreach (var type in types)
				{
					type.IsWorkIdRequired = !type.IsWorkIdRequired;
					type.Name += "345435";
				}
				context.SubmitChanges();
			}
			CanInitializeDbData();
		}

		[Fact]
		public void CannotInsertNullWorkIdWhenWorkIdIsRequired()
		{
			Assert.Throws<SqlException>(() => InsertManualWorkWhereWorkIdIsRequired(null));
		}

		[Fact]
		public void CanInsertNonNullWorkIdWhenWorkIdIsRequired()
		{
			InsertManualWorkWhereWorkIdIsRequired(23);
		}

		private static void InsertManualWorkWhereWorkIdIsRequired(int? workId)
		{
			//Arrange
			using (var context = GetContext())
			{
				context.ManualWorkItemTypes.InsertOnSubmit(new ManualWorkItemType()
				{
					Id = ManualWorkItemTypeEnum.AddWork,
					IsWorkIdRequired = true,
					Name = "",
				});
				context.SubmitChanges();
			}

			//Act
			using (var context = GetContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					WorkId = workId,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow,
				});
				context.SubmitChanges();
			}
		}

		[Fact]
		public void CannotUpdateIsWorkRequiredToTrueIfThereIsANullWorkId()
		{
			//Arrange
			using (var context = GetContext())
			{
				context.ManualWorkItemTypes.InsertOnSubmit(new ManualWorkItemType()
				{
					Id = ManualWorkItemTypeEnum.DeleteInterval,
					IsWorkIdRequired = false,
					Name = "",
				});
				context.SubmitChanges();

				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					WorkId = null,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow,
				});
				context.SubmitChanges();
			}

			//Act
			using (var context = GetContext())
			{
				var type = context.ManualWorkItemTypes.Single(n => n.Id == ManualWorkItemTypeEnum.DeleteInterval);
				type.IsWorkIdRequired = true;
				//Assert
				Assert.Throws<SqlException>(() => context.SubmitChanges());
			}
		}

		[Fact]
		public void CannotInsertNonNullWorkIdWhenWorkIdIsNotRequired()
		{
			Assert.Throws<SqlException>(() => InsertManualDeleteWhereWorkIdIsNotRequired(23));
		}

		[Fact]
		public void CanInsertNullWorkIdWhenWorkIdIsNotRequired()
		{
			InsertManualDeleteWhereWorkIdIsNotRequired(null);
		}

		private static void InsertManualDeleteWhereWorkIdIsNotRequired(int? workId)
		{
			//Arrange
			using (var context = GetContext())
			{
				context.ManualWorkItemTypes.InsertOnSubmit(new ManualWorkItemType()
				{
					Id = ManualWorkItemTypeEnum.DeleteInterval,
					IsWorkIdRequired = false,
					Name = "",
				});
				context.SubmitChanges();
			}

			//Act
			using (var context = GetContext())
			{
				context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
				{
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
					WorkId = workId,
					StartDate = DateTime.UtcNow,
					EndDate = DateTime.UtcNow,
				});
				context.SubmitChanges();
			}
		}
	}
}
