namespace Tct.ActivityRecorderClient
{
    using System;

    public static class CachedFunc
    {
	    public static Func<TKey, TValue> Create<TKey, TValue>(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge)
	    {
		    return new Func<TKey, TValue>(new CachedFunc<TKey, TValue>(funcToCache, maxCacheAge, true).GetOrCalculateValue);
	    }

	    public static Func<TKey, TValue> CreateThreadSafe<TKey, TValue>(Func<TKey, TValue> funcToCache,
		    TimeSpan maxCacheAge)
	    {
		    return new Func<TKey, TValue>(new CachedFuncThreadSafe<TKey, TValue>(funcToCache, maxCacheAge, true).GetOrCalculateValue);
	    }
    }
}

