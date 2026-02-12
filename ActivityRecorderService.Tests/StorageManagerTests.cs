using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Storage;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class StorageManagerTests : IDisposable
	{
		private static readonly string screenShotRootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp", "testScreenshots");
		private readonly Random rnd = new Random();
		private readonly List<byte[]> dummyFileContents = new List<byte[]>();
		private DateTime day1 = new DateTime(2019,6,1);
		private DateTime day2 = new DateTime(2020,2,1);
		private DateTime day3 = new DateTime(2020, 5, 1);

		public StorageManagerTests()
		{
			dummyFileContents.Clear();
			for (var i = 0; i < 1000; i++)
			{
				var size = rnd.Next(1000, 5000);
				var file = new byte[size];
				rnd.NextBytes(file);
				dummyFileContents.Add(file);
			}
		}

		public void Dispose()
		{
			Directory.Delete(screenShotRootDir, true);
		}

		[Fact]
		public void CompactPathResolverReadWrite()
		{
			var pathResolver = new CompactScreenShotPathResolver(screenShotRootDir);
			var storageManager = new StorageManager(pathResolver);
			var id = 0;
			var screenShots = new List<ScreenShot>();
			foreach (var fileContent in dummyFileContents)
			{
				var now = day1 + TimeSpan.FromMinutes(id);
				var screenShot = new ScreenShot() { Data = new Binary(fileContent), Id = id, CreateDate = now, ReceiveDate = now, Height = 100, Width = 200, Extension = "jpg"};
				var workItem = new WorkItem()
				{
					Id = id,
					UserId = 1, 
					CompanyId = -1,
				};
				screenShot.WorkItem = workItem;
				storageManager.TrySaveScreenShotsAsync(workItem).Wait();
				screenShot.Data = null;
				screenShots.Add(screenShot);
				id++;
			}

			string dir = null;
			id = 0;
			foreach (var fileContent in dummyFileContents)
			{
				pathResolver.GetPath(screenShots[id], false, out dir, out var fileName, out var offset, out var length);
				using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
				{
					stream.Position = offset;
					var buffer = new byte[length];
					stream.Read(buffer, 0, length);
					Assert.Equal(fileContent, buffer);
				}

				id++;
			}

		}

		[Fact]
		public void CompactPathResolverReadWriteParallel()
		{
			var pathResolver = new CompactScreenShotPathResolver(screenShotRootDir);
			var storageManager = new StorageManager(pathResolver);
			var screenShots = new List<ScreenShot>();
			var ev = new AutoResetEvent(false);
			var t = new Thread(() =>
			{
				var id = 0;
				foreach (var fileContent in dummyFileContents)
				{
					var now = day2 + TimeSpan.FromMinutes(id);
					var screenShot = new ScreenShot() { Data = new Binary(fileContent), Id = id, CreateDate = now, ReceiveDate = now, Height = 100, Width = 200, Extension = "jpg" };
					var workItem = new WorkItem()
					{
						Id = id,
						UserId = 1,
						CompanyId = -1,
					};
					screenShot.WorkItem = workItem;
					storageManager.TrySaveScreenShotsAsync(workItem).Wait();
					screenShot.Data = null;
					screenShots.Add(screenShot);
					Console.WriteLine($"Write: {id}");
					id++;
					ev.Set();
				}
			});
			t.IsBackground = false;
			t.Start();

			string dir = null;
			try
			{
				var i = 0;
				ev.WaitOne();
				foreach (var fileContent in dummyFileContents)
				{
					while (i >= screenShots.Count || screenShots[i] == null) ev.WaitOne();
					pathResolver.GetPath(screenShots[i], false, out dir, out var fileName, out var offset, out var length);
					using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
					{
						//ev.WaitOne();
						stream.Position = offset;
						var buffer = new byte[length];
						stream.Read(buffer, 0, length);
						Assert.Equal(fileContent, buffer);
					}
					Console.WriteLine($"Read: {i}");
					i++;
				}
			}
			finally
			{
				t.Join();
			}
		}

		[Fact]
		public void CompactPathResolveWriteLotParallel()
		{
			var pathResolver = new CompactScreenShotPathResolver(screenShotRootDir);
			var storageManager = new StorageManager(pathResolver);
			var screenShots = new List<ScreenShot>();
			var ev = new AutoResetEvent(false);
			var processed = 0;
			var id = 0;
			var count = 100;
			var files = dummyFileContents.Take(count).ToList();
			var threads = new Thread[files.Count];
			foreach (var fileContent in files)
			{
				var now = day3 + TimeSpan.FromMinutes(id);
				var screenShot = new ScreenShot() { Data = new Binary(fileContent), Id = id, CreateDate = now, ReceiveDate = now, Height = 100, Width = 200, Extension = "jpg" };
				var workItem = new WorkItem()
				{
					Id = id,
					UserId = 1,
					CompanyId = -1,
				};
				screenShot.WorkItem = workItem;
				threads[id] = new Thread(() =>
				{
					Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} workitem: {workItem.Id}");
					var maxTimeout = 200;
					while (true)
					{
						var task = storageManager.TrySaveScreenShotsAsync(workItem);
						task.Wait();
						if (task.Result) break;
						Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} workitem: {workItem.Id} retrying...");
						Thread.Sleep(rnd.Next(100, maxTimeout));
						if (maxTimeout < 20000) maxTimeout *= 2;
					}

					Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} end.");
					processed++;
					ev.Set();
				}) { IsBackground = false };
				threads[id].Start();
				//screenShot.Data = null;
				screenShots.Add(screenShot);
				Console.WriteLine($"Write: {id}");
				id++;
			}

			while (processed < files.Count) ev.WaitOne(1000);
			foreach (var thread in threads)
			{
				thread.Join();
			}
			var i = 0;
			var pathSet = new HashSet<string>();
			foreach (var fileContent in files)
			{
				while (i >= screenShots.Count) ev.WaitOne();
				pathResolver.GetPath(screenShots[i], false, out var dir, out var fileName, out var offset, out var length);
				var path = Path.Combine(dir, fileName);
				pathSet.Add(path);
				using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
				{
					stream.Position = offset;
					var buffer = new byte[length];
					stream.Read(buffer, 0, length);
					Assert.Equal(fileContent, buffer);
				}
				Console.WriteLine($"Read: {i}");
				i++;
			}
			Assert.Equal(files.Sum(c => c.Length), pathSet.Select(p => new FileInfo(p).Length).Sum());
		}

		[Fact(Skip = "only for performance tests")]
		public void ScreenShotPathResolverPerfTest()
		{
			var resolvers = new List<IScreenShotPathResolver>()
			{
				new ModuloIdScreenShotPathResolver(screenShotRootDir),
				new CompactScreenShotPathResolver(screenShotRootDir),
			};

			foreach (var resolver in resolvers)
			{
				var storageManager = new StorageManager(resolver);
				var sw = Stopwatch.StartNew();
				for (var id = 0; id < 50000; id++)
				{
					var now = DateTime.Today + TimeSpan.FromMinutes(id);
					var screenShot = new ScreenShot() { Data = new Binary(dummyFileContents[id % dummyFileContents.Count]), Id = id, CreateDate = now, ReceiveDate = now, Height = 100, Width = 200, Extension = "jpg" };
					var workItem = new WorkItem()
					{
						Id = id,
						UserId = 1,
						CompanyId = -1,
					};
					screenShot.WorkItem = workItem;
					storageManager.TrySaveScreenShotsAsync(workItem).Wait();
					id++;
				}

				var elapsedMilliseconds = sw.ElapsedMilliseconds;
				Console.WriteLine($@"ScreenShot performance test with resolver {resolver.GetType().Name} finished in {elapsedMilliseconds} ms");
			}
		}
	}
}
