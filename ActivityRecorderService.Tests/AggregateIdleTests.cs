using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class AggregateIdleTests : DbTestsBase
	{
		private static DateTime now = new DateTime(2011, 08, 03);
		private static IEnumerable<AggregateIdleInterval> UpdateAggregateWorkItemIntervals()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				context.UpdateHourlyAggregateWorkItems();
				//hax until AggregateIdleIntervals added to linq
				return context.ExecuteQuery<AggregateIdleInterval>("SELECT * FROM [dbo].[AggregateIdleIntervals]").ToList();
			}
		}

		private static void InsertWorkItems(params WorkItem[] workItems)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				foreach (var workItem in workItems)
				{
					context.WorkItems.InsertOnSubmit(workItem);
				}
				context.SubmitChanges();
			}
		}

		private static void AssertSameInterval(WorkItem workItem, AggregateIdleInterval aggrInterval)
		{
			Assert.Equal(workItem.StartDate, aggrInterval.StartDate);
			Assert.Equal(workItem.EndDate, aggrInterval.EndDate);
			Assert.Equal(workItem.UserId, aggrInterval.UserId);
			Assert.Equal(workItem.WorkId, aggrInterval.WorkId);
			Assert.Equal(workItem.GroupId, aggrInterval.GroupId);
			Assert.Equal(workItem.CompanyId, aggrInterval.CompanyId);
			Assert.Equal(workItem.ComputerId, aggrInterval.ComputerId);
			Assert.Equal(workItem.IsRemoteDesktop, aggrInterval.IsRemoteDesktop);
			Assert.Equal(workItem.IsVirtualMachine, aggrInterval.IsVirtualMachine);
			Assert.Equal(workItem.PhaseId, aggrInterval.PhaseId);
		}

		#region Can aggregate cases
		[Fact]
		public void AggregateWorkItemIntervalsTableIsPopulated()
		{
			//Arrange
			var wi = new WorkItem()
			{
				PhaseId = Guid.NewGuid(),
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			InsertWorkItems(wi);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());
			var aggr = aggrs.Single();
			AssertSameInterval(wi, aggr);
		}

		[Fact]
		public void AggregateTwoAdjacentIntervals()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());
			var aggr = aggrs.Single();
			Assert.Equal(wi1.StartDate, aggr.StartDate);
			Assert.Equal(wi2.EndDate, aggr.EndDate);
			Assert.Equal(wi1.UserId, aggr.UserId);
			Assert.Equal(wi1.WorkId, aggr.WorkId);
			Assert.Equal(wi1.GroupId, aggr.GroupId);
			Assert.Equal(wi1.CompanyId, aggr.CompanyId);
			Assert.Equal(wi1.ComputerId, aggr.ComputerId);
			Assert.Equal(wi1.IsRemoteDesktop, aggr.IsRemoteDesktop);
			Assert.Equal(wi1.IsVirtualMachine, aggr.IsVirtualMachine);
			Assert.Equal(wi1.PhaseId, aggr.PhaseId);
		}

		[Fact]
		public void AggregateTwoAdjacentIntervalsInTwoSteps()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi2);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi1);
			UpdateAggregateWorkItemIntervals();

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());
			var aggr = aggrs.Single();
			Assert.Equal(wi1.StartDate, aggr.StartDate);
			Assert.Equal(wi2.EndDate, aggr.EndDate);
			Assert.Equal(wi1.UserId, aggr.UserId);
			Assert.Equal(wi1.WorkId, aggr.WorkId);
			Assert.Equal(wi1.GroupId, aggr.GroupId);
			Assert.Equal(wi1.CompanyId, aggr.CompanyId);
			Assert.Equal(wi1.ComputerId, aggr.ComputerId);
			Assert.Equal(wi1.IsRemoteDesktop, aggr.IsRemoteDesktop);
			Assert.Equal(wi1.IsVirtualMachine, aggr.IsVirtualMachine);
			Assert.Equal(wi1.PhaseId, aggr.PhaseId);
		}

		[Fact]
		public void AggregateThreeAdjacentIntervals()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			var wi3 = new WorkItem()
			{
				StartDate = now.AddHours(4),
				EndDate = now.AddHours(8),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi3, wi2, wi1);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());
			var aggr = aggrs.Single();
			Assert.Equal(wi1.StartDate, aggr.StartDate);
			Assert.Equal(wi3.EndDate, aggr.EndDate);
			Assert.Equal(wi1.UserId, aggr.UserId);
			Assert.Equal(wi1.WorkId, aggr.WorkId);
			Assert.Equal(wi1.GroupId, aggr.GroupId);
			Assert.Equal(wi1.CompanyId, aggr.CompanyId);
			Assert.Equal(wi1.ComputerId, aggr.ComputerId);
			Assert.Equal(wi1.IsRemoteDesktop, aggr.IsRemoteDesktop);
			Assert.Equal(wi1.IsVirtualMachine, aggr.IsVirtualMachine);
			Assert.Equal(wi1.PhaseId, aggr.PhaseId);
		}

		[Fact]
		public void MergeOnlyAddsToOneInterval()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(1),
				EndDate = now.AddHours(2),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			var wi3 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(3),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi3);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi2, wi1);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());


			var aggr1 = aggrs.Where(n => n.StartDate == now).Single();
			var aggr2 = aggrs.Where(n => n.StartDate == now.AddHours(1)).Single();

			Assert.True((aggr1.EndDate == now.AddHours(3) && aggr2.EndDate == now.AddHours(2))
			|| (aggr1.EndDate == now.AddHours(2) && aggr2.EndDate == now.AddHours(3)));

			Assert.Equal(wi1.UserId, aggr1.UserId);
			Assert.Equal(wi1.WorkId, aggr1.WorkId);
			Assert.Equal(wi1.GroupId, aggr1.GroupId);
			Assert.Equal(wi1.CompanyId, aggr1.CompanyId);
			Assert.Equal(wi1.ComputerId, aggr1.ComputerId);
			Assert.Equal(wi1.IsRemoteDesktop, aggr1.IsRemoteDesktop);
			Assert.Equal(wi1.IsVirtualMachine, aggr1.IsVirtualMachine);
			Assert.Equal(wi1.PhaseId, aggr1.PhaseId);

			Assert.Equal(wi1.UserId, aggr2.UserId);
			Assert.Equal(wi1.WorkId, aggr2.WorkId);
			Assert.Equal(wi1.GroupId, aggr2.GroupId);
			Assert.Equal(wi1.CompanyId, aggr2.CompanyId);
			Assert.Equal(wi1.ComputerId, aggr2.ComputerId);
			Assert.Equal(wi1.IsRemoteDesktop, aggr2.IsRemoteDesktop);
			Assert.Equal(wi1.IsVirtualMachine, aggr2.IsVirtualMachine);
			Assert.Equal(wi1.PhaseId, aggr2.PhaseId);
		}

		[Fact]
		public void AggregateThreeAdjacentIntervalsInThreeSteps()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			var wi3 = new WorkItem()
			{
				StartDate = now.AddHours(4),
				EndDate = now.AddHours(8),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi3);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi2);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi1);
			UpdateAggregateWorkItemIntervals();

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());
			var aggr = aggrs.Single();
			Assert.Equal(wi1.StartDate, aggr.StartDate);
			Assert.Equal(wi3.EndDate, aggr.EndDate);
			Assert.Equal(wi1.UserId, aggr.UserId);
			Assert.Equal(wi1.WorkId, aggr.WorkId);
			Assert.Equal(wi1.GroupId, aggr.GroupId);
			Assert.Equal(wi1.CompanyId, aggr.CompanyId);
			Assert.Equal(wi1.ComputerId, aggr.ComputerId);
			Assert.Equal(wi1.IsRemoteDesktop, aggr.IsRemoteDesktop);
			Assert.Equal(wi1.IsVirtualMachine, aggr.IsVirtualMachine);
			Assert.Equal(wi1.PhaseId, aggr.PhaseId);
		}

		[Fact]
		public void AggregateSomeComplicatedIntervalsInSeveralSteps()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			var wi3 = new WorkItem()
			{
				StartDate = now.AddHours(4),
				EndDate = now.AddHours(8),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};

			var wi11 = new WorkItem()
			{
				StartDate = now.AddHours(1),
				EndDate = now.AddHours(2.5),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId + 1,
			};
			var wi12 = new WorkItem()
			{
				StartDate = now.AddHours(2.5),
				EndDate = now.AddHours(5),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId + 1,
			};

			var wi13 = new WorkItem()
			{
				StartDate = now.AddHours(6),
				EndDate = now.AddHours(7),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId + 1,
			};

			var wi14 = new WorkItem()
			{
				StartDate = now.AddHours(7),
				EndDate = now.AddHours(8),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId + 1,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId + 1,
			};

			var wi21 = new WorkItem()
			{
				StartDate = now.AddHours(5),
				EndDate = now.AddHours(6),
				UserId = wi1.UserId + 1,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};

			var wi31 = new WorkItem()
			{
				StartDate = now.AddHours(8),
				EndDate = now.AddHours(9),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
				IsRemoteDesktop = true,
			};

			var wi32 = new WorkItem()
			{
				StartDate = now.AddHours(9),
				EndDate = now.AddHours(10),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
				IsVirtualMachine = true,
			};

			//InsertWorkItems(wi3, wi21, wi14, wi2, wi13, wi1, wi12, wi11);
			InsertWorkItems(wi3);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi21);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi14);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi2);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi13);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi1);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi12, wi11, wi31, wi32);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(7, aggrs.Count());
			Assert.DoesNotThrow(() => aggrs
				.Where(n => n.StartDate == now)
				.Where(n => n.EndDate == now.AddHours(8))
				.Where(n => n.UserId == wi1.UserId && n.GroupId == wi1.GroupId && n.CompanyId == wi1.CompanyId && n.WorkId == wi1.WorkId && n.ComputerId == wi1.ComputerId)
				.Single());
			Assert.DoesNotThrow(() => aggrs
				.Where(n => n.StartDate == now.AddHours(1))
				.Where(n => n.EndDate == now.AddHours(5))
				.Where(n => n.UserId == wi1.UserId && n.GroupId == wi1.GroupId && n.CompanyId == wi1.CompanyId && n.WorkId == wi1.WorkId && n.ComputerId == wi1.ComputerId + 1)
				.Single());

			var aggr3 = aggrs.Where(n => n.StartDate == now.AddHours(6)).Single();
			AssertSameInterval(wi13, aggr3);

			var aggr4 = aggrs.Where(n => n.StartDate == now.AddHours(7)).Single();
			AssertSameInterval(wi14, aggr4);

			var aggr5 = aggrs.Where(n => n.StartDate == now.AddHours(5)).Single();
			AssertSameInterval(wi21, aggr5);

			var aggr6 = aggrs.Where(n => n.StartDate == now.AddHours(8)).Single();
			AssertSameInterval(wi31, aggr6);

			var aggr7 = aggrs.Where(n => n.StartDate == now.AddHours(9)).Single();
			AssertSameInterval(wi32, aggr7);
		}

		[Fact]
		public void AggregateXAdjacentIntervalsInYSteps()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			var wi3 = new WorkItem()
			{
				StartDate = now.AddHours(4),
				EndDate = now.AddHours(8),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			var wi4 = new WorkItem()
			{
				StartDate = now.AddHours(8),
				EndDate = now.AddHours(9),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			var wi5 = new WorkItem()
			{
				StartDate = now.AddHours(9),
				EndDate = now.AddHours(10),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			var wi6 = new WorkItem()
			{
				StartDate = now.AddHours(10),
				EndDate = now.AddHours(11),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi6);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi4);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi2);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi5, wi3, wi1);
			UpdateAggregateWorkItemIntervals();

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());
			var aggr = aggrs.Single();
			Assert.Equal(wi1.StartDate, aggr.StartDate);
			Assert.Equal(wi6.EndDate, aggr.EndDate);
			Assert.Equal(wi1.UserId, aggr.UserId);
			Assert.Equal(wi1.WorkId, aggr.WorkId);
			Assert.Equal(wi1.GroupId, aggr.GroupId);
			Assert.Equal(wi1.CompanyId, aggr.CompanyId);
			Assert.Equal(wi1.ComputerId, aggr.ComputerId);
			Assert.Equal(wi1.IsRemoteDesktop, aggr.IsRemoteDesktop);
			Assert.Equal(wi1.IsVirtualMachine, aggr.IsVirtualMachine);
			Assert.Equal(wi1.PhaseId, aggr.PhaseId);
		}

		[Fact]
		public void AggregateRandom()
		{
			//Arrange
			var range = 1000;
			var rnd = new Random(666);
			var arr = Enumerable.Range(0, range).OrderBy(n => rnd.Next()).ToArray();
			var arr2 = Enumerable.Range(0, range).OrderBy(n => rnd.Next()).ToArray();
			var arr3 = Enumerable.Range(0, range).OrderBy(n => rnd.Next()).ToArray();
			for (int i = 0; i < arr.Length; i++)
			{
				var wiZ1 = new WorkItem()
				{
					StartDate = now.AddMinutes(arr[i]),
					EndDate = now.AddMinutes(arr[i]),
					PhaseId = new Guid("8d252f3c-f1a0-4c29-8148-cd0392a66570"),
				};
				var wi1 = new WorkItem()
				{
					StartDate = now.AddMinutes(arr[i]),
					EndDate = now.AddMinutes(arr[i] + 1),
					PhaseId = new Guid("8d252f3c-f1a0-4c29-8148-cd0392a66571"),
				};
				var wi2 = new WorkItem()
				{
					StartDate = now.AddMinutes(arr2[i]),
					EndDate = now.AddMinutes(arr2[i] + 1),
					PhaseId = new Guid("8d252f3c-f1a0-4c29-8148-cd0392a66572"),
				};
				var wi3 = new WorkItem()
				{
					StartDate = now.AddMinutes(arr3[i]),
					EndDate = now.AddMinutes(arr3[i] + 1),
					PhaseId = new Guid("8d252f3c-f1a0-4c29-8148-cd0392a66573"),
				};
				var wiZ2 = new WorkItem()
				{
					StartDate = now.AddMinutes(arr3[i]),
					EndDate = now.AddMinutes(arr3[i]),
					PhaseId = new Guid("8d252f3c-f1a0-4c29-8148-cd0392a66574"),
				};
				InsertWorkItems(wiZ2, wiZ1);
				InsertWorkItems(wi2, wi1, wi3);
				if (arr[i] % 10 == 0) UpdateAggregateWorkItemIntervals();
			}

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(3, aggrs.Count());
			var aggr = aggrs.Where(n => n.PhaseId == new Guid("8d252f3c-f1a0-4c29-8148-cd0392a66571")).Single();
			Assert.Equal(now, aggr.StartDate);
			Assert.Equal(now.AddMinutes(range), aggr.EndDate);
			var aggr2 = aggrs.Where(n => n.PhaseId == new Guid("8d252f3c-f1a0-4c29-8148-cd0392a66572")).Single();
			Assert.Equal(now, aggr2.StartDate);
			Assert.Equal(now.AddMinutes(range), aggr2.EndDate);
			var aggr3 = aggrs.Where(n => n.PhaseId == new Guid("8d252f3c-f1a0-4c29-8148-cd0392a66573")).Single();
			Assert.Equal(now, aggr3.StartDate);
			Assert.Equal(now.AddMinutes(range), aggr3.EndDate);
		}

		[Fact]
		public void InsertWorkItemBetweenTwoInterval()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(1),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(3),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			var wi3 = new WorkItem()
			{
				StartDate = now.AddHours(1),
				EndDate = now.AddHours(2),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi2, wi1);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi3);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());


			var aggr1 = aggrs.Where(n => n.StartDate == now).Single();

			Assert.True(aggr1.EndDate == now.AddHours(3) && aggr1.StartDate == now);

			Assert.Equal(wi1.UserId, aggr1.UserId);
			Assert.Equal(wi1.WorkId, aggr1.WorkId);
			Assert.Equal(wi1.GroupId, aggr1.GroupId);
			Assert.Equal(wi1.CompanyId, aggr1.CompanyId);
			Assert.Equal(wi1.ComputerId, aggr1.ComputerId);
			Assert.Equal(wi1.IsRemoteDesktop, aggr1.IsRemoteDesktop);
			Assert.Equal(wi1.IsVirtualMachine, aggr1.IsVirtualMachine);
			Assert.Equal(wi1.PhaseId, aggr1.PhaseId);
		}

		[Fact]
		public void InsertWorkItemBeforeInterval()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(1),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(1),
				EndDate = now.AddHours(2),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi2);
			UpdateAggregateWorkItemIntervals();
			InsertWorkItems(wi1);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());


			var aggr1 = aggrs.Where(n => n.StartDate == now).Single();

			Assert.True(aggr1.EndDate == now.AddHours(2) && aggr1.StartDate == now);

			Assert.Equal(wi1.UserId, aggr1.UserId);
			Assert.Equal(wi1.WorkId, aggr1.WorkId);
			Assert.Equal(wi1.GroupId, aggr1.GroupId);
			Assert.Equal(wi1.CompanyId, aggr1.CompanyId);
			Assert.Equal(wi1.ComputerId, aggr1.ComputerId);
			Assert.Equal(wi1.IsRemoteDesktop, aggr1.IsRemoteDesktop);
			Assert.Equal(wi1.IsVirtualMachine, aggr1.IsVirtualMachine);
			Assert.Equal(wi1.PhaseId, aggr1.PhaseId);
		}

		#endregion

		#region Cannot aggregate cases
		[Fact]
		public void CannotAggregateWithActivities()
		{
			//Arrange
			var wi = new WorkItem()
			{
				PhaseId = Guid.NewGuid(),
				StartDate = now,
				EndDate = now.AddHours(2),
				MouseActivity = 5,
				KeyboardActivity = 7,
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			InsertWorkItems(wi);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(0, aggrs.Count());
		}

		[Fact]
		public void CannotAggregateWithKeyboardActivity()
		{
			//Arrange
			var wi = new WorkItem()
			{
				PhaseId = Guid.NewGuid(),
				StartDate = now,
				EndDate = now.AddHours(2),
				KeyboardActivity = 7,
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			InsertWorkItems(wi);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(0, aggrs.Count());
		}

		[Fact]
		public void CannotAggregateWithMouseActivity()
		{
			//Arrange
			var wi = new WorkItem()
			{
				PhaseId = Guid.NewGuid(),
				StartDate = now,
				EndDate = now.AddHours(2),
				MouseActivity = 5,
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			InsertWorkItems(wi);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(0, aggrs.Count());
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsButOneWithActivity()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				MouseActivity = 2,
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			AssertSameInterval(wi1, aggr1);
		}

		[Fact]
		public void CannotAggregateWithBitActivities()
		{
			//Arrange
			var wi = new WorkItem()
			{
				PhaseId = Guid.NewGuid(),
				StartDate = now,
				EndDate = now.AddHours(2),
				MouseActivity = -1,
				KeyboardActivity = -1,
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			InsertWorkItems(wi);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(0, aggrs.Count());
		}

		[Fact]
		public void CannotAggregateWithKeyboardBitActivity()
		{
			//Arrange
			var wi = new WorkItem()
			{
				PhaseId = Guid.NewGuid(),
				StartDate = now,
				EndDate = now.AddHours(2),
				KeyboardActivity = -1,
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			InsertWorkItems(wi);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(0, aggrs.Count());
		}

		[Fact]
		public void CannotAggregateWithMouseBitActivity()
		{
			//Arrange
			var wi = new WorkItem()
			{
				PhaseId = Guid.NewGuid(),
				StartDate = now,
				EndDate = now.AddHours(2),
				MouseActivity = -1,
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			InsertWorkItems(wi);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(0, aggrs.Count());
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsButOneWithBitActivity()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				MouseActivity = -1,
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(1, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			AssertSameInterval(wi1, aggr1);
		}

		[Fact]
		public void CannotAggregateTwoIntervalsWithIntersection()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(1),
				EndDate = now.AddHours(3),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		[Fact]
		public void CannotAggregateTwoIntervalsWithAGapInBetween()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2).AddSeconds(1),
				EndDate = now.AddHours(3),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsWithUserIdMismatch()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId + 1,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsWithWorkIdMismatch()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId + 1,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsWithGroupIdMismatch()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId + 1,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsWithCompanyIdMismatch()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId + 1,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsWithComputerIdMismatch()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId + 1,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsWithPhaseIdMismatch()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				PhaseId = Guid.NewGuid(),
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				PhaseId = Guid.NewGuid(),
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsWithIsRemoteDesktopMismatch()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
				IsRemoteDesktop = true,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		[Fact]
		public void CannotAggregateTwoAdjacentIntervalsWithIsVirtualMachineMismatch()
		{
			//Arrange
			var wi1 = new WorkItem()
			{
				StartDate = now,
				EndDate = now.AddHours(2),
				UserId = 10,
				WorkId = 2,
				GroupId = 3,
				CompanyId = 4,
				ComputerId = 5,
			};
			var wi2 = new WorkItem()
			{
				StartDate = now.AddHours(2),
				EndDate = now.AddHours(4),
				UserId = wi1.UserId,
				WorkId = wi1.WorkId,
				GroupId = wi1.GroupId,
				CompanyId = wi1.CompanyId,
				ComputerId = wi1.ComputerId,
				IsVirtualMachine = true,
			};
			InsertWorkItems(wi1, wi2);

			//Act
			var aggrs = UpdateAggregateWorkItemIntervals();

			//Assert
			Assert.Equal(2, aggrs.Count());
			var aggr1 = aggrs.Single(n => n.StartDate == wi1.StartDate);
			var aggr2 = aggrs.Single(n => n.StartDate == wi2.StartDate);
			AssertSameInterval(wi1, aggr1);
			AssertSameInterval(wi2, aggr2);
		}

		#endregion

	}

	internal class AggregateIdleInterval
	{
		public long Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid PhaseId { get; set; }
		public int UserId { get; set; }
		public int GroupId { get; set; }
		public int CompanyId { get; set; }
		public int WorkId { get; set; }
		public int ComputerId { get; set; }
		public bool IsRemoteDesktop { get; set; }
		public bool IsVirtualMachine { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}
