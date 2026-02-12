using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;

namespace TelemetryConverter.Database
{
	public class IntervalOrderedList<T> : IEnumerable<T> where T : IInterval
	{

		private readonly DateOrderedList<T> starts = new DateOrderedList<T>();
		private readonly DateOrderedList<T> ends = new DateOrderedList<T>();

		public DateTime Earliest
		{
			get { return starts.Earliest; }
		}

		public DateTime Latest
		{
			get { return ends.Latest; }
		}

		public int Count
		{
			get { return starts.Count; }
		}

		public IntervalOrderedList()
		{

		}

		public IntervalOrderedList(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				Add(item);
			}
		}

		private IntervalOrderedList(DateOrderedList<T> starts, DateOrderedList<T> ends)
		{
			this.starts = starts;
			this.ends = ends;
		}

		public void Add(T item)
		{
			Add(item.StartDate, item.EndDate, item);
		}

		public void Add(DateTime startDate, DateTime endDate, T item)
		{
			starts.Add(startDate, item);
			ends.Add(endDate, item);
		}

		public IntervalOrderedList<T> Clone()
		{
			return new IntervalOrderedList<T>(starts.Clone(), ends.Clone());
		}

		public void MergeWith(IntervalOrderedList<T> other)
		{
			starts.MergeWith(other.starts);
			ends.MergeWith(other.ends);
		}

		public IEnumerable<TTarget> EnumerateBetween<TTarget>(Interval interval, Func<T, Interval, TTarget> intervalResizer)
			where TTarget : IInterval
		{
			var intersectingIntervals = new HashSet<T>(starts.EnumerateBefore(interval.EndDate, false));
			intersectingIntervals.IntersectWith(new HashSet<T>(ends.EnumerateAfter(interval.StartDate, true)));
			foreach (var intersectingInterval in intersectingIntervals)
			{
				var intersection = interval.Intersect(intersectingInterval);
				if (intersection == null) continue;
				yield return intervalResizer(intersectingInterval, intersection);
			}
		}

		public IEnumerable<T> EnumerateContains(Interval interval)
		{
			var intersectingIntervals = new HashSet<T>(starts.EnumerateBefore(interval.EndDate, false));
			intersectingIntervals.IntersectWith(new HashSet<T>(ends.EnumerateAfter(interval.StartDate, true)));
			return intersectingIntervals;
		}

		public void Remove(T item)
		{
			starts.Remove(item);
			ends.RemoveLatest(item);
		}

		public void RemoveLatest(T item)
		{
			starts.RemoveLatest(item);
			ends.Remove(item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return starts.GetEnumerator();
		}

		private static Tuple<int, TValue> MinIndexAndValue<TValue>(IEnumerable<TValue> enumerable)
		{
			var comparer = Comparer<TValue>.Default;
			var minIndex = -1;
			var minValue = default(TValue);
			var currentIndex = 0;
			var enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (minIndex == -1 || comparer.Compare(minValue, enumerator.Current) == 1)
				{
					minIndex = currentIndex;
					minValue = enumerator.Current;
				}
				++currentIndex;
			}

			return Tuple.Create(minIndex, minValue);
		}

		public Dictionary<TKey, IntervalOrderedList<T>> GroupBy<TKey>(Func<T, TKey> keySelector)
		{
			var result = new Dictionary<TKey, IntervalOrderedList<T>>();
			foreach (var item in this)
			{
				var key = keySelector(item);
				result.GetValueOrCreate(key, () => new IntervalOrderedList<T>()).Add(item.StartDate, item.EndDate, item);
			}

			return result;
		}

		public IntervalOrderedList<TTarget> Flatten<TTarget>(Func<IEnumerable<T>, Interval, TTarget> mergerFunc)
			where TTarget : IInterval
		{
			return Flatten(new[] { this }, mergerFunc);
		}

		public IntervalOrderedList<TTarget> Join<TTarget, T1>(IntervalOrderedList<T1> intervals,
			Func<IEnumerable<T>, IEnumerable<T1>, Interval, TTarget> mergerFunc)
			where TTarget : IInterval
			where T1 : IInterval
		{
			var resultStarts = new DateOrderedList<TTarget>();
			var resultEnds = new DateOrderedList<TTarget>();
			var otherEnumerator = intervals.GetEnumerator();
			var enumerator = GetEnumerator();
			var activeValues = new List<T>();
			var otherActiveValues = new List<T1>();
			var otherHasValue = otherEnumerator.MoveNext();
			var hasValue = enumerator.MoveNext();
			DateTime? lastDate = null;
			while (true)
			{
				var valueList = activeValues.Select(x => x.EndDate).Union(otherActiveValues.Select(x => x.EndDate)).ToList();
				if (hasValue) valueList.Add(enumerator.Current.StartDate);
				if (otherHasValue) valueList.Add(otherEnumerator.Current.StartDate);
				var earliest = MinIndexAndValue(activeValues.Select(x => x.EndDate).Union(otherActiveValues.Select(x => x.EndDate)));
				if (earliest.Item1 == -1) break;

				if (lastDate != null && lastDate != earliest.Item2)
				{
					var currentInterval = new Interval(lastDate.Value, earliest.Item2);
					var resultInterval = mergerFunc(activeValues, otherActiveValues, currentInterval);
					resultStarts.Append(resultInterval.StartDate, resultInterval);
					resultEnds.Append(resultInterval.EndDate, resultInterval);
				}

				lastDate = earliest.Item2;

				if (earliest.Item1 < activeValues.Count)
				{
					// activeValue deactivated
					activeValues.RemoveAt(earliest.Item1);
				} else if (earliest.Item1 < activeValues.Count + otherActiveValues.Count)
				{
					// other active value deactivated
					otherActiveValues.RemoveAt(earliest.Item1 - activeValues.Count);
				} else if (earliest.Item1 < activeValues.Count + otherActiveValues.Count + 1 && hasValue)
				{
					// value activated
					var currentValue = enumerator.Current;
					hasValue = enumerator.MoveNext();
					activeValues.Add(currentValue);
				}
				else
				{
					// other value activated
					var currentValue = otherEnumerator.Current;
					otherHasValue = otherEnumerator.MoveNext();
					otherActiveValues.Add(currentValue);
				}
			}

			return new IntervalOrderedList<TTarget>(resultStarts, resultEnds);
		}

		public static IntervalOrderedList<TTarget> Flatten<TTarget>(IEnumerable<IntervalOrderedList<T>> intervals, Func<IEnumerable<T>, Interval, TTarget> mergerFunc)
			where TTarget : IInterval
		{
			var resultStarts = new DateOrderedList<TTarget>();
			var resultEnds = new DateOrderedList<TTarget>();
			var intervalsToListed = intervals.Select(i => i.Clone()).ToList();
			var enumerators = intervalsToListed.Select(x => x.GetEnumerator()).Where(x => x.MoveNext()).ToList();
			var nextStartDates = enumerators.Select(x => x.Current.StartDate).ToList();
			var activeValues = new List<T>();
			DateTime? lastDate = null;
			while (true)
			{
				// this can be further improved with a priority queue
				var earliest = MinIndexAndValue(nextStartDates.Union(activeValues.Select(x => x.EndDate)));
				if (earliest.Item1 == -1) break;

				if (lastDate != null && lastDate != earliest.Item2) // Skip 0 length intervals
				{
					var currentInterval = new Interval(lastDate.Value, earliest.Item2);
					var resultInterval = mergerFunc(activeValues, currentInterval);
					resultStarts.Append(resultInterval.StartDate, resultInterval);
					resultEnds.Append(resultInterval.EndDate, resultInterval);
				}

				lastDate = earliest.Item2;

				if (earliest.Item1 < nextStartDates.Count)
				{
					var addIndex = earliest.Item1;
					// Add interval
					activeValues.Add(enumerators[addIndex].Current);
					if (enumerators[addIndex].MoveNext())
					{
						nextStartDates[addIndex] = enumerators[addIndex].Current.StartDate;
					}
					else
					{
						enumerators.RemoveAt(addIndex);
						nextStartDates.RemoveAt(addIndex);
					}
				}
				else
				{
					// Remove interval
					var activeIdx = earliest.Item1 - nextStartDates.Count;
					activeValues.RemoveAt(activeIdx);
				}
			}

			return new IntervalOrderedList<TTarget>(resultStarts, resultEnds);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
