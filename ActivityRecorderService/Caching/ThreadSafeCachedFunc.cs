using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderService.Caching
{
	public class ThreadSafeCachedFunc<TKey, TValue>
	{
		protected readonly ConcurrentDictionary<TKey, Lazy<CachedValue>> dict = new ConcurrentDictionary<TKey, Lazy<CachedValue>>();
		protected readonly Func<TKey, TValue> cachedFunc;
		private readonly TimeSpan maxCacheAge;
		private readonly int autoClearInterval;
		private volatile int lastClear;

		public ThreadSafeCachedFunc(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge)
			 : this(funcToCache, maxCacheAge, TimeSpan.Zero)
		{
		}

		public ThreadSafeCachedFunc(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge, TimeSpan autoClearInterval)
		{
			if (maxCacheAge >= TimeSpan.FromMilliseconds(int.MaxValue)) throw new ArgumentOutOfRangeException("maxCacheAge");
			if (autoClearInterval >= TimeSpan.FromMilliseconds(int.MaxValue)) throw new ArgumentOutOfRangeException("autoClearInterval");
			cachedFunc = funcToCache;
			this.maxCacheAge = maxCacheAge;
			this.autoClearInterval = (int)autoClearInterval.TotalMilliseconds;
			lastClear = Environment.TickCount;
		}

		protected class CachedValue //ageing is based on tick count (but only CachedValue knows about it)
		{
			public readonly TValue Value;
			private readonly int created;

			private CachedValue(TValue value, int created)
			{
				Value = value;
				this.created = created;
			}

			public static CachedValue Create(TValue value)
			{
				return new CachedValue(value, Environment.TickCount);
			}

			public TimeSpan GetAge()
			{
				return TimeSpan.FromMilliseconds((uint)(Environment.TickCount - created));
			}
		}
#if DEBUG
		protected virtual bool IsExpired(CachedValue cachedValue)
#else
		private bool IsExpired(CachedValue cachedValue)
#endif
		{
			return cachedValue.GetAge() > maxCacheAge;
		}

		public bool TryGetValueFromCache(TKey key, out TValue value)
		{
			ClearExpiredIfApplicable();
			Lazy<CachedValue> cachedValue;
			if (!dict.TryGetValue(key, out cachedValue))
			{
				value = default(TValue);
				return false;
			}
			bool isExpired;
			try
			{
				isExpired = IsExpired(cachedValue.Value);
			}
			catch //this should be logged when value is added (GetOrCalculateValue)
			{
				isExpired = true; //remove if original func throwed (Lazy will always throw)
			}
			if (isExpired)
			{
				//http://blogs.msdn.com/b/pfxteam/archive/2011/04/02/10149222.aspx
				((ICollection<KeyValuePair<TKey, Lazy<CachedValue>>>)dict).Remove(new KeyValuePair<TKey, Lazy<CachedValue>>(key, cachedValue)); //remove expired value
				value = default(TValue);
				return false;
			}
			value = cachedValue.Value.Value;
			return true;
		}

#if DEBUG
		public virtual TValue GetOrCalculateValue(TKey key)
#else
		public TValue GetOrCalculateValue(TKey key)
#endif
		{
			TValue value;
			if (TryGetValueFromCache(key, out value))
			{
				return value;
			}
			return dict.GetOrAdd(key, new Lazy<CachedValue>(() => CachedValue.Create(cachedFunc(key)))).Value.Value;
		}

#pragma warning disable 0420
		private void ClearExpiredIfApplicable()
		{
			if (autoClearInterval <= 0) return;
			int now, currClear;
			do
			{
				currClear = lastClear;
				now = Environment.TickCount;
				if ((uint)(now - currClear) < autoClearInterval) return;
			} while (Interlocked.CompareExchange(ref lastClear, now, currClear) != currClear);
			ClearExpired();
		}
#pragma warning restore 0420

		public void Remove(TKey key)
		{
			Lazy<CachedValue> _;
			dict.TryRemove(key, out _);
		}

		public void Clear()
		{
			dict.Clear();
		}

		public void ClearExpired()
		{
			foreach (var lazyKvP in dict)
			{
				if (IsExpired(lazyKvP.Value.Value))
				{
					Remove(lazyKvP.Key);
				}
			}
		}
	}

	public class ThreadSafeCachedFunc<TKey1, TKey2, TValue> : ThreadSafeCachedFunc<Tuple<TKey1, TKey2>, TValue>
	{
		public ThreadSafeCachedFunc(Func<TKey1, TKey2, TValue> funcToCache, TimeSpan maxCacheAge) : base(tuple => funcToCache(tuple.Item1, tuple.Item2), maxCacheAge)
		{
		}

		public ThreadSafeCachedFunc(Func<TKey1, TKey2, TValue> funcToCache, TimeSpan maxCacheAge, TimeSpan autoClearInterval) : base(tuple => funcToCache(tuple.Item1, tuple.Item2), maxCacheAge, autoClearInterval)
		{
		}

		public TValue GetOrCalculateValue(TKey1 key1, TKey2 key2)
		{
			return base.GetOrCalculateValue(Tuple.Create(key1, key2));
		}
	}
}