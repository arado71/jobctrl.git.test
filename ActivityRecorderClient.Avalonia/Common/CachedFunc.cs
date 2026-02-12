using System;
using System.Collections.Generic;

namespace Tct.ActivityRecorderClient
{
	public class CachedFuncBase<T>
	{
		protected TimeSpan MaxCacheAge;
		public class CachedValue //ageing is based on tick count (but only CachedValue knows about it)
		{
			public readonly T Value;
			private readonly int created;

			private CachedValue(T value, int created)
			{
				Value = value;
				this.created = created;
			}

			public static CachedValue Create(T value)
			{
				return new CachedValue(value, Environment.TickCount);
			}

			public TimeSpan GetAge()
			{
				return TimeSpan.FromMilliseconds((uint)(Environment.TickCount - created));
			}
		}
		protected bool IsExpired(CachedValue cachedValue)
		{
			return cachedValue.GetAge() > MaxCacheAge;
		}
	}

	public class CachedFunc<T> : CachedFuncBase<T>, ICachedFunc<T>
	{
		private readonly Func<T> cachedFunc;
		private CachedValue cachedValue;
		public CachedFunc(Func<T> funcToCache, TimeSpan maxCacheAge)
		{
			this.cachedFunc = funcToCache;
			MaxCacheAge = maxCacheAge;
		}
		public bool TryGetValueFromCache(out T value)
		{
			if (this.cachedValue == null)
			{
				value = default(T);
				return false;
			}
			var _cachedValue = this.cachedValue;
			if (IsExpired(_cachedValue))
			{
				this.cachedValue = null;
				value = default(T);
				return false;
			}
			value = _cachedValue.Value;
			return true;
		}
		public T GetOrCalculateValue()
		{
			T value;
			if (TryGetValueFromCache(out value))
			{
				return value;
			}
			return CalculateValue();
		}

		public T CalculateValue()
		{
			var value = cachedFunc();
			cachedValue = CachedValue.Create(value);
			return value;
		}
	}
	public class CachedFunc<TKey, TValue> : CachedFuncBase<TValue>, ICachedFunc<TKey, TValue>
	{
		private readonly Dictionary<TKey, CachedValue> dict = new Dictionary<TKey, CachedValue>();
		private readonly Func<TKey, TValue> cachedFunc;
		private readonly bool autoClearExpired;
		private const int itemsAddedBeforeAutoClear = 50;
		private int itemsAdded;

		public CachedFunc(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge, bool autoClearExpired = false)
		{
			cachedFunc = funcToCache;
			MaxCacheAge = maxCacheAge;
			this.autoClearExpired = autoClearExpired;
		}

		public bool TryGetValueFromCache(TKey key, out TValue value)
		{
			CachedValue cachedValue;
			if (!dict.TryGetValue(key, out cachedValue))
			{
				value = default(TValue);
				return false;
			}
			if (IsExpired(cachedValue))
			{
				dict.Remove(key);
				value = default(TValue);
				return false;
			}
			value = cachedValue.Value;
			return true;
		}

		public TValue GetOrCalculateValue(TKey key)
		{
			TValue value;
			if (TryGetValueFromCache(key, out value))
			{
				return value;
			}
			value = cachedFunc(key);
			if (autoClearExpired && ++itemsAdded % itemsAddedBeforeAutoClear == 0)
			{
				itemsAdded = 0;
				ClearExpired();
			}
			dict.Add(key, CachedValue.Create(value));
			return value;
		}

		public void Remove(TKey key)
		{
			dict.Remove(key);
		}

		public void Clear()
		{
			dict.Clear();
		}

		public void ClearExpired()
		{
			var keysToRemove = new List<TKey>();
			foreach (var keyValue in dict)
			{
				if (IsExpired(keyValue.Value))
				{
					keysToRemove.Add(keyValue.Key);
				}
			}
			foreach (var keyToRemove in keysToRemove)
			{
				dict.Remove(keyToRemove);
			}
		}
	}

	public static class CachedFunc
	{
		public static Func<TKey, TValue> Create<TKey, TValue>(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge)
		{
			return new CachedFunc<TKey, TValue>(funcToCache, maxCacheAge, true).GetOrCalculateValue;
		}

		public static Func<TKey, TValue> CreateThreadSafe<TKey, TValue>(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge)
		{
			return new CachedFuncThreadSafe<TKey, TValue>(funcToCache, maxCacheAge, true).GetOrCalculateValue;
		}
	}

	public class CachedFuncThreadSafe<TKey, TValue> : ICachedFunc<TKey, TValue>
	{
		private readonly CachedFunc<TKey, TValue> cachedFunc;
		private readonly object thisLock = new object();

		public CachedFuncThreadSafe(Func<TKey, TValue> funcToCache, TimeSpan maxCacheAge, bool autoClearExpired = false)
		{
			cachedFunc = new CachedFunc<TKey, TValue>(funcToCache, maxCacheAge, autoClearExpired);
		}

		#region ICachedFunc<TKey,TValue> Members

		public bool TryGetValueFromCache(TKey key, out TValue value)
		{
			lock (thisLock)
			{
				return cachedFunc.TryGetValueFromCache(key, out value);
			}
		}

		public TValue GetOrCalculateValue(TKey key)
		{
			lock (thisLock)
			{
				return cachedFunc.GetOrCalculateValue(key);
			}
		}

		public void Remove(TKey key)
		{
			lock (thisLock)
			{
				cachedFunc.Remove(key);
			}
		}

		public void ClearExpired()
		{
			lock (thisLock)
			{
				cachedFunc.ClearExpired();
			}
		}

		public void Clear()
		{
			lock (thisLock)
			{
				cachedFunc.Clear();
			}
		}

		#endregion
	}

	public interface ICachedFunc<in TKey, TValue>
	{
		bool TryGetValueFromCache(TKey key, out TValue value);
		TValue GetOrCalculateValue(TKey key);
		void Remove(TKey key);
		void ClearExpired();
		void Clear();
	}

	public interface ICachedFunc<TValue>
	{
		bool TryGetValueFromCache(out TValue value);
		TValue GetOrCalculateValue();
	}
}
