namespace Tct.ActivityRecorderClient
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CachedFunc<TKey, TValue> : ICachedFunc<TKey, TValue>
    {
        private readonly bool autoClearExpired;
        private readonly Func<TKey, TValue> cachedFunc;
        private readonly Dictionary<TKey, CachedValue<TKey, TValue>> dict;
        private int itemsAdded;
        private const int itemsAddedBeforeAutoClear = 50;
        private readonly TimeSpan maxCacheAge;

        public CachedFunc(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge) : this(funcToCache, maxCacheAge, false)
        {
        }

        public CachedFunc(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge, bool autoClearExpired)
        {
            this.dict = new Dictionary<TKey, CachedValue<TKey, TValue>>();
            this.cachedFunc = funcToCache;
            this.maxCacheAge = maxCacheAge;
            this.autoClearExpired = autoClearExpired;
        }

        public void Clear()
        {
            this.dict.Clear();
        }

        public void ClearExpired()
        {
            List<TKey> list = new List<TKey>();
            foreach (KeyValuePair<TKey, CachedValue<TKey, TValue>> pair in this.dict)
            {
                if (this.IsExpired(pair.Value))
                {
                    list.Add(pair.Key);
                }
            }
            foreach (TKey local in list)
            {
                this.dict.Remove(local);
            }
        }

        public TValue GetOrCalculateValue(TKey key)
        {
            TValue local;
            if (!this.TryGetValueFromCache(key, out local))
            {
                local = this.cachedFunc(key);
                if (this.autoClearExpired && ((++this.itemsAdded % 50) == 0))
                {
                    this.itemsAdded = 0;
                    this.ClearExpired();
                }
                this.dict.Add(key, CachedValue<TKey, TValue>.Create(local));
            }
            return local;
        }

        private bool IsExpired(CachedValue<TKey, TValue> cachedValue)
	    {
		    return (cachedValue.GetAge() > this.maxCacheAge);
	    }

	    public void Remove(TKey key)
        {
            this.dict.Remove(key);
        }

        public bool TryGetValueFromCache(TKey key, out TValue value)
        {
            CachedValue<TKey, TValue> value2;
            if (!this.dict.TryGetValue(key, out value2))
            {
                value = default(TValue);
                return false;
            }
            if (this.IsExpired(value2))
            {
                this.dict.Remove(key);
                value = default(TValue);
                return false;
            }
            value = value2.Value;
            return true;
        }

        private class CachedValue<T, U>
        {
            private readonly int created;
            public readonly U Value;

            private CachedValue(U value, int created)
            {
                this.Value = value;
                this.created = created;
            }

            public static CachedFunc<T, U>.CachedValue<T,U> Create(U value)
	        {
		        return new CachedFunc<T, U>.CachedValue<T, U>(value, Environment.TickCount);
	        }

	        public TimeSpan GetAge()
	        {
		        return TimeSpan.FromMilliseconds((double) (Environment.TickCount - this.created));
	        }
        }
    }
}

