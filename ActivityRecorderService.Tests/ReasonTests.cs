using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class ReasonTests
	{
		[Fact]
		public void BuildBigButPlausibleMenuStressTest()
		{
			//old method
			//5 10 (111111) - 5 mins (100x)
			//4 10  (11111) - 3 secs
			var numLevels = 5;
			var numItemsPerLevel = 10;
			var id = 0;
			var root = new UserReason() { ReasonItemParentId = null, ReasonItemId = ++id };
			var currentLevel = new[] { root };
			var reasons = new List<UserReason>() { root };
			for (int i = 0; i < numLevels; i++)
			{
				currentLevel = currentLevel.SelectMany(n => Enumerable.Range(0, numItemsPerLevel).Select(m => new UserReason() { ReasonItemParentId = n.ReasonItemId, ReasonItemId = ++id })).ToArray();
				reasons.AddRange(currentLevel);
			}
			Console.WriteLine(reasons.Count);
			var sw = Stopwatch.StartNew();
			var result = JobControlDataClassesDataContext.BuildReasonNodeTree(reasons);
			Console.WriteLine(sw.Elapsed.TotalMilliseconds + "ms");
			//check if the correct tree is built
			var queue = new Queue<CloseReasonNode>();
			result.ForEach(n => queue.Enqueue(n));
			int j = 0;
			while (queue.Count > 0)
			{
				var curr = queue.Dequeue();
				Assert.Equal(++j, curr.NodeId);
				if (curr.Children != null)
				{
					curr.Children.ForEach(n => queue.Enqueue(n));
				}
			}
		}

		[Fact]
		public void NoInfiniteLoop()
		{
			var reasons = new List<UserReason>()
			               	{
								new UserReason() { ReasonItemParentId = null, ReasonItemId = 1 },
			               		new UserReason() { ReasonItemParentId = 3, ReasonItemId = 2 },
			               		new UserReason() { ReasonItemParentId = 2, ReasonItemId = 3 },
			               	};
			JobControlDataClassesDataContext.BuildReasonNodeTree(reasons);
		}

		[Fact]
		public void NoInfiniteLoopBadData()
		{
			var reasons = new List<UserReason>()
			               	{
								new UserReason() { ReasonItemParentId = null, ReasonItemId = 1 },
			               		new UserReason() { ReasonItemParentId = 1, ReasonItemId = 2 },
			               		new UserReason() { ReasonItemParentId = 3, ReasonItemId = 2 },
			               		new UserReason() { ReasonItemParentId = 2, ReasonItemId = 3 },
			               	};
			JobControlDataClassesDataContext.BuildReasonNodeTree(reasons);
		}

		[Fact]
		public void SimpleTree()
		{
			var reasons = new List<UserReason>()
			               	{
			               		new UserReason() {ReasonItemParentId = null, ReasonItemId = 1},
			               		new UserReason() {ReasonItemParentId = null, ReasonItemId = 2},
			               		new UserReason() {ReasonItemParentId = 1, ReasonItemId = 3},
			               		new UserReason() {ReasonItemParentId = 2, ReasonItemId = 4},
			               		new UserReason() {ReasonItemParentId = 2, ReasonItemId = 5},
			               		new UserReason() {ReasonItemParentId = 5, ReasonItemId = 6},
			               	};
			var result = JobControlDataClassesDataContext.BuildReasonNodeTree(reasons);
			Assert.Equal(1, result[0].NodeId);
			Assert.Equal(2, result[1].NodeId);
			Assert.Equal(3, result[0].Children[0].NodeId);
			Assert.Equal(4, result[1].Children[0].NodeId);
			Assert.Equal(5, result[1].Children[1].NodeId);
			Assert.Equal(6, result[1].Children[1].Children[0].NodeId);
		}
	}
}
