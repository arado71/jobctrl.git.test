namespace Tct.ActivityRecorderClient
{
    using System;
    using System.Runtime.InteropServices;

    public class CachedFuncThreadSafe<TKey, TValue> : ICachedFunc<TKey, TValue>
    {
        private readonly CachedFunc<TKey, TValue> cachedFunc;
        private readonly object thisLock;

        public CachedFuncThreadSafe(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge) : this(funcToCache, maxCacheAge, false)
        {
        }

        public CachedFuncThreadSafe(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge, bool autoClearExpired)
        {
            this.thisLock = new object();
            this.cachedFunc = new CachedFunc<TKey, TValue>(funcToCache, maxCacheAge, autoClearExpired);
        }

        public void Clear()
        {
            lock (this.thisLock)
            {
                this.cachedFunc.Clear();
            }
        }

        public void ClearExpired()
        {
            lock (this.thisLock)
            {
                this.cachedFunc.ClearExpired();
            }
        }

        public TValue GetOrCalculateValue(TKey key)
        {
            lock (this.thisLock)
            {
                return this.cachedFunc.GetOrCalculateValue(key);
            }
        }

        public void Remove(TKey key)
        {
            lock (this.thisLock)
            {
                this.cachedFunc.Remove(key);
            }
        }

        public bool TryGetValueFromCache(TKey key, out TValue value)
        {
            lock (this.thisLock)
            {
                return this.cachedFunc.TryGetValueFromCache(key, out value);
            }
        }
    }
}

