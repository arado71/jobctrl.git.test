using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Caching.Works;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class WorkHierarchyCachePerfTests
	{
		//mulipy each with 100 to get better results, but we don't want to sloww down regular tests
		private const int works = 20000;
		private const int projs = 4000;
		private const int itFound = 20000;
		private const int itNotFound = 4000;

		private IEnumerable<Work> GetWorks()
		{
			for (int i = 1; i < works + 1; i++)
			{
				yield return new Work() { Id = i, Name = "Work " + i, ProjectId = i / 5 + 1 };
			}
		}

		private IEnumerable<Project> GetProjects()
		{
			for (int i = 1; i < projs + 1; i++)
			{
				yield return new Project() { Id = i, Name = "Projects " + i, ParentId = i / 10 + 1 };
			}
		}

		private WorkHierarchyBase GetReaderWriterLock()
		{
			return new WorkHierarchyCacheRwLock(GetWorks(), GetProjects(),
				i => new Work() { Id = i, Name = "Work " + i, ProjectId = i },
				i => new Project() { Id = i, Name = "Projects " + i, ParentId = (i % projs) + 1 });
		}

		private WorkHierarchyBase GetConcurrent()
		{
			return new WorkHierarchyCacheDict(GetWorks(), GetProjects(),
				i => new Work() { Id = i, Name = "Work " + i, ProjectId = i },
				i => new Project() { Id = i, Name = "Projects " + i, ParentId = (i % projs) + 1 });
		}

		private WorkHierarchyBase GetLru()
		{
			return new WorkHierarchyCacheLru(GetWorks(), GetProjects(),
				i => new Work() { Id = i, Name = "Work " + i, ProjectId = i },
				i => new Project() { Id = i, Name = "Projects " + i, ParentId = (i % projs) + 1 }
				, (int)(works * 1.2), (int)(projs * 1.2));
		}

		private WorkHierarchyBase GetLruSmall()
		{
			return new WorkHierarchyCacheLru(GetWorks(), GetProjects(),
				i => new Work() { Id = i, Name = "Work " + i, ProjectId = i },
				i => new Project() { Id = i, Name = "Projects " + i, ParentId = (i % projs) + 1 }
				, (int)(works * 0.1), (int)(projs * 0.1));
		}

		private int SingleThreaded(WorkHierarchyBase wh)
		{
			var st = Environment.TickCount;
			for (int i = 1; i < itFound; i++)
			{
				Work work;
				wh.TryGetWork(i, out work);
			}
			for (int i = works + 1; i < works + itNotFound; i++)
			{
				Work work;
				wh.TryGetWork(i, out work);
			}
			return Environment.TickCount - st;
		}

		private int TwoThreaded(WorkHierarchyBase wh)
		{
			var st = Environment.TickCount;
			var t1 = Task.Factory.StartNew(() =>
			{
				for (int i = 1; i < itFound; i++)
				{
					Work work;
					wh.TryGetWork(i, out work);
				}
			});
			var t2 = Task.Factory.StartNew(() =>
			{
				for (int i = works + 1; i < works + itNotFound; i++)
				{
					Work work;
					wh.TryGetWork(i, out work);
				}
			});
			Task.WaitAll(t1, t2);
			return Environment.TickCount - st;
		}

		private int ParallelThreads(WorkHierarchyBase wh)
		{
			var st = Environment.TickCount;
			Parallel.For(1, itFound, i =>
			{
				Work work;
				wh.TryGetWork(i % 5 == 0 ? i + works : i, out work);
			});
			return Environment.TickCount - st;
		}

		[Fact]
		public void DictWithReaderWriterLockSingleThread()
		{
			var wh = GetReaderWriterLock();
			Console.WriteLine(SingleThreaded(wh));
		}

		[Fact]
		public void ConcurrentDictSingleThread()
		{
			var wh = GetConcurrent();
			Console.WriteLine(SingleThreaded(wh));
		}

		[Fact]
		public void LruSingleThread()
		{
			var wh = GetLru();
			Console.WriteLine(SingleThreaded(wh));
		}

		[Fact]
		public void SmallLruSingleThread()
		{
			var wh = GetLruSmall();
			Console.WriteLine(SingleThreaded(wh));
		}

		[Fact]
		public void DictWithReaderWriterLockTwoThread()
		{
			var wh = GetReaderWriterLock();
			Console.WriteLine(TwoThreaded(wh));
		}

		[Fact]
		public void ConcurrentDictTwoThread()
		{
			var wh = GetConcurrent();
			Console.WriteLine(TwoThreaded(wh));
		}

		[Fact]
		public void LruTwoThread()
		{
			var wh = GetLru();
			Console.WriteLine(TwoThreaded(wh));
		}

		[Fact]
		public void SmallLruTwoThread()
		{
			var wh = GetLruSmall();
			Console.WriteLine(TwoThreaded(wh));
		}

		[Fact]
		public void DictWithReaderWriterLockParallel()
		{
			var wh = GetReaderWriterLock();
			Console.WriteLine(ParallelThreads(wh));
		}

		[Fact]
		public void ConcurrentDictParallel()
		{
			var wh = GetConcurrent();
			Console.WriteLine(ParallelThreads(wh));
		}

		[Fact]
		public void LruParallel()
		{
			var wh = GetLru();
			Console.WriteLine(ParallelThreads(wh));
		}

		[Fact]
		public void SmallLruParallel()
		{
			var wh = GetLruSmall();
			Console.WriteLine(ParallelThreads(wh));
		}
	}
}
