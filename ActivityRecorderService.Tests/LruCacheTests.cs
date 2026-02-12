using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Caching;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class LruCacheTests
	{
		[Fact]
		public void Create()
		{
			Assert.DoesNotThrow(() => new LruCache<string, int>(30));
		}

		[Fact]
		public void DropOverCap()
		{
			var cache = new LruCache<string, int>(30);
			for (int i = 0; i < 40; i++)
			{
				cache.TryAdd(i.ToString(), i);
			}
			int value;
			Assert.True(cache.Count < 34);
			Assert.False(cache.TryGetValue("5", out value));
			Assert.True(cache.TryGetValue("15", out value));
			Assert.Equal(15, value);
		}

		[Fact]
		public void NoDropUnderCap()
		{
			var cache = new LruCache<string, int>(30);
			for (int i = 0; i < 20; i++)
			{
				cache.TryAdd(i.ToString(), i);
			}
			int value;
			Assert.True(cache.Count == 20);
			Assert.True(cache.TryGetValue("0", out value));
			Assert.Equal(0, value);
			Assert.True(cache.TryGetValue("19", out value));
			Assert.Equal(19, value);
		}

		[Fact]
		public void Remove()
		{
			var cache = new LruCache<string, int>(30);
			for (int i = 0; i < 20; i++)
			{
				cache.TryAdd(i.ToString(), i);
			}
			int value;
			Assert.Equal(20, cache.Count);
			Assert.True(cache.TryRemove("4"));
			Assert.Equal(19, cache.Count);
			Assert.False(cache.TryGetValue("4", out value));
			Assert.True(cache.TryGetValue("15", out value));
			Assert.Equal(15, value);
		}

		[Fact]
		public void DropOverCapButRecentWontBeDropped()
		{
			int value;
			var cache = new LruCache<string, int>(30);
			for (int i = 0; i < 40; i++)
			{
				if (i % 10 == 0) cache.TryGetValue("4", out value);
				cache.TryAdd(i.ToString(), i);
			}
			Assert.True(cache.Count < 34);
			Assert.False(cache.TryGetValue("5", out value));
			Assert.True(cache.TryGetValue("15", out value));
			Assert.Equal(15, value);
			Assert.True(cache.TryGetValue("4", out value));
			Assert.Equal(4, value);
		}

		[Fact]
		public void AddingNullThrows()
		{
			var cache = new LruCache<string, int>(30);
			Assert.Throws<ArgumentNullException>(() => cache.TryAdd(null, 3));
		}

		[Fact]
		public void StressTest()
		{
			int counter = 0;
			var cache = new LruCache<string, int>(30000);
			ThreadPool.SetMinThreads(100, 100);
			var sw = System.Diagnostics.Stopwatch.StartNew();
			var task = Task.Factory.StartNew(() =>
				{
					for (int i = 0; i < 300; i++)
					{
						Task.Factory.StartNew(() =>
							{
								//var start = Environment.TickCount;
								for (int j = 0; j < 1000; j++)
								{
									var val = Interlocked.Increment(ref counter);
									cache.TryAdd(val.ToString(), val);
									//if (j % 100 == 0) Thread.Sleep(1);
								}
								//Console.WriteLine("c:" + cache.Count + " end: " + (Environment.TickCount - start));
							}, TaskCreationOptions.AttachedToParent);
					}
				});

			task.Wait();
			Console.WriteLine(sw.Elapsed.TotalMilliseconds + "ms");
			int value;
			Assert.True(cache.Count < 34000);
			Assert.True(cache.Count > 26000);
			Assert.False(cache.TryGetValue("265000", out value));
			Assert.True(cache.TryGetValue("285000", out value));
			Assert.Equal(285000, value);
		}

		[Fact]
		public void LookupIdCacheProcessName()
		{
			var cache = new LookupIdCache(9, 9, 9);
			Assert.Null(cache.GetIdForProcessName("test.exe"));
			cache.AddProcessName("test.exe", 32);
			Assert.Equal(32, cache.GetIdForProcessName("test.exe"));
		}

		[Fact]
		public void LookupIdCacheProcessNameMissing()
		{
			var cache = new LookupIdCache(9, 9, 9);
			cache.AddTitle("test.exe", 32);
			cache.AddUrl("test.exe", 32);
			Assert.Null(cache.GetIdForProcessName("test.exe"));
		}

		[Fact]
		public void LookupIdCacheTitle()
		{
			var cache = new LookupIdCache(9, 9, 9);
			Assert.Null(cache.GetIdForTitle("test.exe"));
			cache.AddTitle("test.exe", 32);
			Assert.Equal(32, cache.GetIdForTitle("test.exe"));
		}

		[Fact]
		public void LookupIdCacheTitleMissing()
		{
			var cache = new LookupIdCache(9, 9, 9);
			cache.AddProcessName("test.exe", 32);
			cache.AddUrl("test.exe", 32);
			Assert.Null(cache.GetIdForTitle("test.exe"));
		}

		[Fact]
		public void LookupIdCacheUrl()
		{
			var cache = new LookupIdCache(9, 9, 9);
			Assert.Null(cache.GetIdForUrl("test.exe"));
			cache.AddUrl("test.exe", 32);
			Assert.Equal(32, cache.GetIdForUrl("test.exe"));
		}

		[Fact]
		public void LookupIdCacheUrlMissing()
		{
			var cache = new LookupIdCache(9, 9, 9);
			cache.AddProcessName("test.exe", 32);
			cache.AddTitle("test.exe", 32);
			Assert.Null(cache.GetIdForUrl("test.exe"));
		}
	}
}
