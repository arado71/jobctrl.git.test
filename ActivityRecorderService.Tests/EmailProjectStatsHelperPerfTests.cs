using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class EmailProjectStatsHelperPerfTests
	{
		[Fact]
		public void GetEmptyProjectCostTreeIsFastEnough()
		{
			var dict = Enumerable.Range(1, 100000)
				.Select(n => new Project() { Id = n, ParentId = n == 1 ? new int?() : n - 1, Name = "Proj" + n })
				.ToLookup(n => n.ParentId);
			var startTicks = Environment.TickCount;
			var tree = EmailProjectStatsHelper.GetEmptyProjectCostTree(dict);
			Assert.True(Environment.TickCount - startTicks < 10000);
		}

		[Fact]
		public void GetEmptyProjectCostTreeIsFastEnough2()
		{
			var dict = Enumerable.Range(1, 100000)
				.Select(n => new Project() { Id = n, ParentId = n % 2 == 1 ? new int?() : 1, Name = "Proj" + n })
				.ToLookup(n => n.ParentId);
			var startTicks = Environment.TickCount;
			var tree = EmailProjectStatsHelper.GetEmptyProjectCostTree(dict);
			Assert.True(Environment.TickCount - startTicks < 10000);
		}

		[Fact]
		public void GetEmptyProjectCostTreeSimple()
		{
			var dict = new[] {
				new Project() { Id = 1, ParentId = null , Name = "1"},
				new Project() { Id = 2, ParentId = 1 , Name = "2"},
				new Project() { Id = 3, ParentId = 2 , Name = "3"},
				new Project() { Id = 4, ParentId = null , Name = "4"},
				new Project() { Id = 5, ParentId = 4 , Name = "5"},
				new Project() { Id = 6, ParentId = 4 , Name = "6"},
				new Project() { Id = 7, ParentId = null , Name = "7"},
			}.ToLookup(n => n.ParentId);
			var tree = EmailProjectStatsHelper.GetEmptyProjectCostTree(dict);
			Assert.Equal(7, tree.Dict.Count);
			for (int i = 1; i < 8; i++)
			{
				Assert.Equal(i, tree.Dict[i].ProjectId);
				Assert.Equal(i.ToString(), tree.Dict[i].ProjectName);
				Assert.Equal(0, tree.Dict[i].UserWorkCosts.Count);
			}
			Assert.Equal(1, tree.Dict[1].Childrens.Count);
			Assert.Equal(2, tree.Dict[1].Childrens[0].ProjectId);
			Assert.Equal(1, tree.Dict[1].Childrens[0].Childrens.Count);
			Assert.Equal(3, tree.Dict[1].Childrens[0].Childrens[0].ProjectId);
			Assert.Equal(0, tree.Dict[1].Childrens[0].Childrens[0].Childrens.Count);

			Assert.Same(tree.Dict[2], tree.Dict[1].Childrens[0]);
			Assert.Same(tree.Dict[3], tree.Dict[1].Childrens[0].Childrens[0]);

			Assert.Equal(2, tree.Dict[4].Childrens.Count);
			Assert.True(tree.Dict[4].Childrens.Select(n => n.ProjectId).OrderBy(n => n).SequenceEqual(new[] { 5, 6 }));

			Assert.Equal(0, tree.Dict[5].Childrens.Count);
			Assert.Equal(0, tree.Dict[6].Childrens.Count);
			Assert.Equal(0, tree.Dict[7].Childrens.Count);

			Assert.Equal(3, tree.Tree.Count);
			Assert.Same(tree.Dict[1], tree.Tree.Where(n => n.ProjectId == 1).Single());
			Assert.Same(tree.Dict[2], tree.Tree.Where(n => n.ProjectId == 1).Single().Childrens.Where(n => n.ProjectId == 2).Single());
			Assert.Same(tree.Dict[3], tree.Tree.Where(n => n.ProjectId == 1).Single().Childrens.Where(n => n.ProjectId == 2).Single().Childrens.Where(n => n.ProjectId == 3).Single());
			Assert.Same(tree.Dict[4], tree.Tree.Where(n => n.ProjectId == 4).Single());
			Assert.Same(tree.Dict[5], tree.Tree.Where(n => n.ProjectId == 4).Single().Childrens.Where(n => n.ProjectId == 5).Single());
			Assert.Same(tree.Dict[6], tree.Tree.Where(n => n.ProjectId == 4).Single().Childrens.Where(n => n.ProjectId == 6).Single());
			Assert.Same(tree.Dict[7], tree.Tree.Where(n => n.ProjectId == 7).Single());
		}

		[Fact]
		public void GetEmptyProjectCostTreeCycle()
		{
			var dict = new[] {
				new Project() { Id = 1, ParentId = null , Name = "1"},
				new Project() { Id = 2, ParentId = 3 , Name = "2"},
				new Project() { Id = 3, ParentId = 2 , Name = "3"},
			}.ToLookup(n => n.ParentId);
			var tree = EmailProjectStatsHelper.GetEmptyProjectCostTree(dict);

			Assert.Equal(1, tree.Dict.Count);
			Assert.Equal(1, tree.Dict[1].ProjectId);

			Assert.Equal(1, tree.Tree.Count);
			Assert.Equal(1, tree.Tree[0].ProjectId);
		}
	}
}
