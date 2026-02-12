using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderService.Caching
{
	/// <summary>
	/// Quck and dirty (again?) but thread-safe class for LRU caching.
	/// </summary>
	/// <remarks>
	/// There are some race conditions and capacity is not accurate at all but good enough for us atm.
	/// </remarks>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class LruCache<TKey, TValue>
	{
		private readonly ConcurrentDictionary<TKey, CachedValue> dict;
		private readonly int capacity;
		private readonly int threshold;
		private int currentTimestamp;
		private int itemsAdded; //a value between [0..threshold-1] for calculating automatic clears

		public LruCache(int capacity)
			: this(capacity, EqualityComparer<TKey>.Default)
		{
		}

		public LruCache(int capacity, IEqualityComparer<TKey> comparer)
		{
			if (comparer == null) throw new ArgumentNullException("comparer");
			if (capacity < 1) throw new ArgumentOutOfRangeException("capacity");
			dict = new ConcurrentDictionary<TKey, CachedValue>(comparer);
			this.capacity = capacity;
			threshold = capacity / 10 + 1;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			CachedValue cachedValue;
			if (!dict.TryGetValue(key, out cachedValue))
			{
				value = default(TValue);
				return false;
			}
			cachedValue.Timestamp = GetNewTimestamp();
			value = cachedValue.Value;
			return true;
		}

		public bool TryAdd(TKey key, TValue value)
		{
			if (!dict.TryAdd(key, new CachedValue(value, GetNewTimestamp()))) return false;
			int added;
			int newAdded;
			do
			{
				added = itemsAdded;
				newAdded = (added + 1) % threshold;
			} while (Interlocked.CompareExchange(ref itemsAdded, newAdded, added) != added);
			if (newAdded == 0) ClearOldValues();
			return true;
		}

		public bool TryRemove(TKey key)
		{
			CachedValue _;
			return dict.TryRemove(key, out _);
		}

		public void Clear()
		{
			dict.Clear();
		}

		public int Count { get { return dict.Count; } }

		public ICollection<TKey> Keys { get { return dict.Keys; } }

		private int GetNewTimestamp()
		{
			return Interlocked.Increment(ref currentTimestamp);
		}

		private int GetCurrentTimestamp()
		{
			return Interlocked.CompareExchange(ref currentTimestamp, 0, 0);
		}

		private int clearInProgress;
		private void ClearOldValues()
		{
			//avoid race where multiple threads start ClearOldValues an using invalid dict.Count and removing too much
			//but now under heavy inserts ClearingOldValues can block so dict can grow well beyond capacity (until it got enough cpu time)
			//we could clear values on a an other thread to avoid blocking TryAdd but then we have to introduce WaitForPendingClears() for unit tests...
			if (Interlocked.Increment(ref clearInProgress) != 1) return;
			do
			{
				var targetCount = capacity - threshold;
				var numToRemove = dict.Count - targetCount;
				if (numToRemove <= 0) continue;
				var currTimestamp = GetCurrentTimestamp();
				var keysToRemove = new EnumeratorHolder(dict) //.ToArray() //there is a bug in the FW so we either need EnumeratorHolder or .ToArray() (instance method)
					.OrderByDescending(n => currTimestamp - n.Value.Timestamp)
					.Select(n => n.Key)
					.Take(numToRemove)
					.ToArray();
				CachedValue _;
				foreach (var keyToRemove in keysToRemove)
				{
					dict.TryRemove(keyToRemove, out _);
				}
			} while (Interlocked.Decrement(ref clearInProgress) != 0);
		}

		private class CachedValue
		{
			internal readonly TValue Value;
			private int timestamp;

			internal int Timestamp
			{
				get { return Interlocked.CompareExchange(ref timestamp, 0, 0); }
				set { Interlocked.Exchange(ref timestamp, value); }
			}

			internal CachedValue(TValue value, int timestamp)
			{
				Value = value;
				Timestamp = timestamp;
			}
		}

		//http://connect.microsoft.com/VisualStudio/feedback/details/756059/conccurentdictionary-concurrentbag-concurrency-issue-when-used-with-enumerable-tolist#details
		private class EnumeratorHolder : IEnumerable<KeyValuePair<TKey, CachedValue>>
		{
			private readonly ConcurrentDictionary<TKey, CachedValue> parent;
			public EnumeratorHolder(ConcurrentDictionary<TKey, CachedValue> parent)
			{
				this.parent = parent;
			}

			public IEnumerator<KeyValuePair<TKey, CachedValue>> GetEnumerator()
			{
				return parent.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return parent.GetEnumerator();
			}
		}
	}
}
