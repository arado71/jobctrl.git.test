using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.Java.Common
{
	[Serializable]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CachedDictionary<TKey, TValue>
	{
		private readonly Dictionary<TKey, CachedValue> dict = new Dictionary<TKey, CachedValue>();
		private readonly TimeSpan defaultExpirationTime;
		private readonly bool autoClearExpired;
		private const int itemsAddedBeforeAutoClear = 50;
		private int itemsAdded;

		public CachedDictionary(TimeSpan defaultExpirationTime, bool autoClearExpired)
		{
			this.defaultExpirationTime = defaultExpirationTime;
			this.autoClearExpired = autoClearExpired;
		}

		[Serializable]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		private class CachedValue //ageing is based on tick count (but only CachedValue knows about it)
		{
			public readonly TValue Value;
			private readonly int startTicks;
			private readonly uint maxAge;

			private CachedValue(TValue value, uint maxAge)
			{
				Value = value;
				startTicks = Environment.TickCount;
				this.maxAge = maxAge;
			}

			public static CachedValue Create(TValue value, TimeSpan expirationTime)
			{
				if (expirationTime < TimeSpan.Zero || expirationTime.TotalMilliseconds > int.MaxValue) throw new ArgumentOutOfRangeException("expirationTime");
				return new CachedValue(value, (uint)expirationTime.TotalMilliseconds);
			}

			public bool IsExpired()
			{
				return (uint)(Environment.TickCount - startTicks) > maxAge;
			}
		}

		private static bool IsExpired(CachedValue cachedValue)
		{
			return cachedValue.IsExpired();
		}
		public virtual bool ContainsKey(TKey key)
		{
			return dict.ContainsKey(key);
		}
		public bool TryGetValue(TKey key, out TValue value)
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

		public void Remove(TKey key)
		{
			dict.Remove(key);
		}

		public void Add(TKey key, TValue value)
		{
			Add(key, value, defaultExpirationTime);
		}

		public void Add(TKey key, TValue value, TimeSpan expirationTime)
		{
			if (autoClearExpired && ++itemsAdded % itemsAddedBeforeAutoClear == 0)
			{
				itemsAdded = 0;
				ClearExpired();
			}
			dict.Add(key, CachedValue.Create(value, expirationTime));
		}

		public void Set(TKey key, TValue value)
		{
			Set(key, value, defaultExpirationTime);
		}

		public void Set(TKey key, TValue value, TimeSpan expirationTime)
		{
			Remove(key);
			Add(key, value, expirationTime);
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

		public Dictionary<TKey, TValue> ToDictionary()
		{
			return dict.Where(d => !d.Value.IsExpired()).ToDictionary(d => d.Key, d => d.Value.Value);
		}
	}
}
