using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Caching
{
	public static class CachedFunc
	{
		public static Func<TKey, TValue> CreateThreadSafe<TKey, TValue>(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge)
		{
			return new ThreadSafeCachedFunc<TKey, TValue>(funcToCache, maxCacheAge, TimeSpan.FromTicks(maxCacheAge.Ticks * 10)).GetOrCalculateValue;
		}
	}
}
