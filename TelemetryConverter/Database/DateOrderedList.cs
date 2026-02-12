using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.TextFormatting;

namespace TelemetryConverter.Database
{
	public class DateOrderedList<T> : IEnumerable<T>
	{
		private readonly List<T> content;
		private readonly List<DateTime> dateList;

		public DateTime Earliest
		{
			get { return dateList[0]; }
		}

		public DateTime Latest
		{
			get { return dateList[dateList.Count - 1]; }
		}

		public int Count
		{
			get { return content.Count; }
		}

		public DateOrderedList()
		{
			content = new List<T>();
			dateList = new List<DateTime>();
		}

		public DateOrderedList(int capacity)
		{
			content = new List<T>(capacity);
			dateList = new List<DateTime>(capacity);
		}

		private DateOrderedList(List<DateTime> dateList, List<T> content)
		{
			this.content = content;
			this.dateList = dateList;
		}

		public void Add(DateTime timestamp, T item)
		{
			var idx = dateList.BinarySearch(timestamp);
			if (idx < 0) idx = ~idx;
			Insert(idx, timestamp, item);
		}

		private void Insert(int idx, DateTime timestamp, T item)
		{
			dateList.Insert(idx, timestamp);
			content.Insert(idx, item);
		}

		internal void Append(DateTime timestamp, T item)
		{
			dateList.Add(timestamp);
			content.Add(item);
		}

		public IntervalOrderedList<TTarget> ToIntervals<TTarget>(Func<T, TTarget> intervalGenerator) where TTarget : IInterval
		{
			var result = new IntervalOrderedList<TTarget>();
			foreach(var item in this)
			{
				var resultInterval = intervalGenerator(item);
				if (resultInterval != null) result.Add(resultInterval);
			}

			return result;
		}

		public DateOrderedList<T> CreateFiltered(DateTime firstInclusive, DateTime lastExclusive, Func<T, bool> predicate)
		{
			var startInclusiveIdx = IndexOfEarliest(firstInclusive, true);
			var endExclusiveIdx = IndexOfLatest(lastExclusive, false);
			if(startInclusiveIdx < 0 || endExclusiveIdx >= content.Count || endExclusiveIdx < startInclusiveIdx) return new DateOrderedList<T>();
			var filteredDateList = dateList.GetRange(startInclusiveIdx, endExclusiveIdx - startInclusiveIdx);
			var filteredContent = content.GetRange(startInclusiveIdx, endExclusiveIdx - startInclusiveIdx);
			for (var i = 0; i < filteredDateList.Count; ++i)
			{
				if (!predicate(filteredContent[i]))
				{
					filteredContent.RemoveAt(i);
					filteredDateList.RemoveAt(i--);
				}
			}

			return new DateOrderedList<T>(filteredDateList, filteredContent);
		}

		public DateOrderedList<TResult> CreateFiltered<TResult>(IEnumerable<Interval> intervals, Func<T, bool> predicate, Func<T, TResult> selector) where TResult : IEvent
		{
			var newDateList = new List<DateTime>();
			var newContent = new List<TResult>();
			foreach (var interval in intervals)
			{
				var startInclusiveIdx = IndexOfEarliest(interval.StartDate, true);
				var endExclusiveIdx = IndexOfLatest(interval.EndDate, false);
				if (startInclusiveIdx < 0 || endExclusiveIdx >= content.Count || endExclusiveIdx < startInclusiveIdx) continue;
				for (var i = startInclusiveIdx; i < endExclusiveIdx; ++i)
				{
					if (predicate(content[i]))
					{
						newDateList.Add(dateList[i]);
						newContent.Add(selector(content[i]));
					}
				}
			}

			return new DateOrderedList<TResult>(newDateList, newContent);
		}

		public DateOrderedList<T> Clone()
		{
			return new DateOrderedList<T>(new List<DateTime>(dateList), new List<T>(content));
		}

		public DateOrderedList<T> Merge(DateOrderedList<T> other)
		{
			// todo Determine when to use MergeNewWithMerge
			return MergeNewWithAdd(other);
		}

		private void EnsureCapacity(int capacity)
		{
			if (capacity < content.Capacity) return;
			content.Capacity = capacity;
			dateList.Capacity = capacity;
		}

		public void MergeWith(DateOrderedList<T> other)
		{
			var requiredCapacity = Count + other.Count;
			EnsureCapacity(requiredCapacity);
			MergeWithAdd(other);
			// todo Determine when to use MergeWithMerge
		}

		private void MergeWithAdd(DateOrderedList<T> other)
		{
			for (var i = 0; i < other.dateList.Count; ++i)
			{
				Add(other.dateList[i], other.content[i]);
			}
		}

		private void MergeWithMerge(DateOrderedList<T> other)
		{
			var startIndex = 0;
			var thisIdx = 0;
			var thisHasElements = Count > 0;
			var otherIdx = 0;
			var otherHasElements = other.Count > 0;
			while (true)
			{
				if (thisHasElements && otherHasElements)
				{
					var thisDate = dateList[thisIdx];
					var otherDate = other.dateList[otherIdx];
					if (thisDate < otherDate)
					{
						thisHasElements = ++thisIdx < Count;
					}
					else
					{
						Insert(thisIdx, otherDate, other.content[otherIdx]);
						otherHasElements = ++otherIdx < other.Count;
					}
				}
				else
				{
					if (otherHasElements)
					{
						Append(other.dateList[otherIdx], other.content[otherIdx]);
						if (++otherIdx < other.Count) break;
					}
					else
					{
						break;
					}
				}
			}
		}

		private DateOrderedList<T> MergeNewWithAdd(DateOrderedList<T> other)
		{
			var requiredCapacity = Count + other.Count;
			var cloneThis = Count >= other.Count;
			var result = cloneThis ? Clone() : other.Clone();
			result.EnsureCapacity(requiredCapacity);
			result.MergeWith(cloneThis ? other : this);
			return result;
		}

		private DateOrderedList<T> MergeNewWithMerge(DateOrderedList<T> other)
		{
			var requiredCapacity = Count + other.Count;
			var result = new DateOrderedList<T>(requiredCapacity);
			var thisIdx = 0;
			var thisHasElements = Count > 0;
			var otherIdx = 0;
			var otherHasElements = other.Count > 0;
			while (true)
			{
				if (thisHasElements && otherHasElements)
				{
					var thisDate = dateList[thisIdx];
					var otherDate = other.dateList[otherIdx];
					if (thisDate < otherDate)
					{
						result.Append(thisDate, content[thisIdx]);
						thisHasElements = ++thisIdx < Count;
					}
					else
					{
						result.Append(otherDate, other.content[otherIdx]);
						otherHasElements = ++otherIdx < other.Count;
					}
				}
				else
				{
					if (thisHasElements)
					{
						result.Append(dateList[thisIdx], content[thisIdx]);
						if (++thisIdx < Count) break;
					} else if (otherHasElements)
					{
						result.Append(other.dateList[otherIdx], other.content[otherIdx]);
						if (++otherIdx < other.Count) break;
					}
					else
					{
						break;
					}
				}
			}

			return result;
		}

		private IEnumerable<T> EnumerateBetween(int startIdx, int endIdx)
		{
			if (startIdx < 0 || startIdx >= content.Count) yield break;
			for (var i = startIdx; i <= Math.Min(endIdx, content.Count - 1); ++i)
			{
				yield return content[i];
			}
		}

		public IEnumerable<T> EnumerateBetween(DateTime start, bool startInclusive, DateTime end, bool endInclusive)
		{
			var startIdx = IndexOfEarliest(start, startInclusive);
			var endIdx = IndexOfLatest(end, endInclusive);
			return EnumerateBetween(startIdx, endIdx);
		}

		public IEnumerable<T> EnumerateBetween(DateTime startInclusive, DateTime endExclusive)
		{
			return EnumerateBetween(startInclusive, true, endExclusive, false);
		}

		public IEnumerable<T> EnumerateBefore(DateTime date, bool includeDate)
		{
			var endIndex = IndexOfLatest(date, includeDate);
			return EnumerateBetween(0, endIndex);
		}

		public IEnumerable<T> EnumerateAfter(DateTime date, bool includeDate)
		{
			var startIndex = IndexOfEarliest(date, includeDate);
			return EnumerateBetween(startIndex, content.Count - 1);
		}

		public void Remove(T item)
		{
			var index = content.IndexOf(item);
			content.RemoveAt(index);
			dateList.RemoveAt(index);
		}

		public void RemoveLatest(T item)
		{
			var index = content.LastIndexOf(item);
			content.RemoveAt(index);
			dateList.RemoveAt(index);
		}

		public long CountBetween(DateTime firstInclusive, DateTime lastExclusive)
		{
			return IndexOfLatest(lastExclusive, false) - IndexOfEarliest(firstInclusive, true) + 1;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return content.GetEnumerator();
		}

		public Dictionary<TKey, DateOrderedList<T>> GroupBy<TKey>(Func<T, TKey> keySelector)
		{
			var result = new Dictionary<TKey, DateOrderedList<T>>();
			for (var i = 0; i < dateList.Count; ++i)
			{
				var key = keySelector(content[i]);
				result.GetValueOrCreate(key, () => new DateOrderedList<T>()).Append(dateList[i], content[i]);
			}

			return result;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private int IndexOfEarliest(DateTime lowerBound, bool includeBound)
		{
			var idx = dateList.BinarySearch(lowerBound);
			if (idx >= 0)
			{
				while (idx >= 0 && dateList[idx] == lowerBound) --idx;
				return includeBound ? ++idx : idx;
			}

			return ~idx;
		}

		private int IndexOfLatest(DateTime upperBound, bool includeBound)
		{
			var idx = dateList.BinarySearch(upperBound);
			if (idx >= 0)
			{
				if (includeBound)
				{
					while (idx < dateList.Count && dateList[idx] == upperBound) ++idx;
					--idx;
				}
				else
				{
					while (idx > 0 && dateList[idx] == upperBound) --idx;
				}
				
				return idx;
			}

			return ~idx - 1;
		}
	}
}
