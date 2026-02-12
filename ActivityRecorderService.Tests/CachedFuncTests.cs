using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Caching;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class CachedFuncTests
	{
		const int it = 10000; //small vaule to avoid long running tests
		int counter = 0;
		private int IncrementThreadSafe(string key)
		{
			return Interlocked.Increment(ref counter);
		}

		private int Increment(string key)
		{
			return ++counter;
		}

		[Fact]
		public void CacheOneValue()
		{
			var cache = CachedFunc.CreateThreadSafe<string, int>(Increment, TimeSpan.FromMinutes(1));
			Assert.Equal(1, cache(""));
			Assert.Equal(1, cache(""));
			Assert.Equal(1, cache(""));
		}

		[Fact]
		public void CacheExpire()
		{
			var cache = CachedFunc.CreateThreadSafe<string, int>(Increment, TimeSpan.FromTicks(1));
			Assert.Equal(1, cache(""));
			Thread.Sleep(100);
			Assert.Equal(2, cache(""));
			Thread.Sleep(100);
			Assert.Equal(3, cache(""));
		}

		[Fact]
		public void CacheException()
		{
			var count = 0;
			var cache = CachedFunc.CreateThreadSafe<string, int>(_ =>
																	{
																		if (count++ == 0) throw new ArgumentOutOfRangeException("Inital ex");
																		return count;
																	}, TimeSpan.FromTicks(1));
			Assert.Throws<ArgumentOutOfRangeException>(() => cache(""));
			Thread.Sleep(100);
			Assert.Equal(2, cache(""));
		}

		[Fact]
		public void StressTest()
		{
			//var sw = Stopwatch.StartNew();
			ThreadPool.SetMinThreads(100, 100);
			var cache = new ThreadSafeCachedFunc<string, int>(Increment, TimeSpan.FromTicks(1), TimeSpan.FromTicks(1));
			var task = Task.Factory.StartNew(() =>
			{
				for (int i = 0; i < Environment.ProcessorCount * 2; i++)
				{
					Task.Factory.StartNew(() =>
					{
						for (int j = 0; j < it; j++)
						{
							var res = cache.GetOrCalculateValue(RandomHelper.Next(100).ToString());
						}
					}, TaskCreationOptions.AttachedToParent);
				}
			});
			task.Wait();
			//Console.WriteLine(sw.Elapsed.TotalMilliseconds.ToString());
		}

#if DEBUG
		[Fact]
		public void StressTestExpired()
		{
			ThreadPool.SetMinThreads(100, 100);
			var cache = new ThreadSafeCachedFuncExpired<string, int>(Increment, TimeSpan.FromTicks(1), TimeSpan.FromTicks(1));
			var task = Task.Factory.StartNew(() =>
			{
				for (int i = 0; i < Environment.ProcessorCount * 2; i++)
				{
					Task.Factory.StartNew(() =>
					{
						for (int j = 0; j < it; j++)
						{
							var res = cache.GetOrCalculateValue(""); //we have to use the same key
						}
					}, TaskCreationOptions.AttachedToParent);
				}
			});
			task.Wait();
			Assert.Equal(Environment.ProcessorCount * 2 * it + 1, cache.GetOrCalculateValue(""));
		}

		[Fact]
		public void StressTestExpiredThreadSafeInc()
		{
			ThreadPool.SetMinThreads(100, 100);
			var cache = new ThreadSafeCachedFuncExpired<string, int>(IncrementThreadSafe, TimeSpan.FromTicks(1), TimeSpan.FromTicks(1));
			var task = Task.Factory.StartNew(() =>
			{
				for (int i = 0; i < Environment.ProcessorCount * 2; i++)
				{
					Task.Factory.StartNew(() =>
					{
						for (int j = 0; j < it; j++)
						{
							var res = cache.GetOrCalculateValue(RandomHelper.Next(100).ToString());
						}
					}, TaskCreationOptions.AttachedToParent);
				}
			});
			task.Wait();
			Assert.Equal(Environment.ProcessorCount * 2 * it + 1, cache.GetOrCalculateValue(""));
		}

		private class ThreadSafeCachedFuncExpired<TKey, TValue> : ThreadSafeCachedFunc<TKey, TValue>
		{
			public ThreadSafeCachedFuncExpired(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge, TimeSpan autoClearInterval)
				: base(funcToCache, maxCacheAge, autoClearInterval)
			{
			}

			protected override bool IsExpired(CachedValue cachedValue)
			{
				return true;
			}

			//we have to use AddOrUpdate here (instead of GetOrAdd) to avoid race using newly created value without checking IsExpired
			//(which otherwise would be fine, because newly created value should not be expired after one use)
			public override TValue GetOrCalculateValue(TKey key)
			{
				TValue value;
				if (TryGetValueFromCache(key, out value))
				{
					return value;
				}
				return dict.AddOrUpdate(key,
					new Lazy<CachedValue>(() => CachedValue.Create(cachedFunc(key))),
					(k, old) => IsExpired(old.Value) ? new Lazy<CachedValue>(() => CachedValue.Create(cachedFunc(k))) : old).Value.Value;
			}
		}
#endif

	}
}
