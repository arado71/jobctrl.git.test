using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService
{
	public class IntervalConcatenator : ICloneable
	{
		//intervals are inserted ordered by startdate, overlapping interval are merged (so only disjoint intervals are stored)
		private readonly List<Interval> intervals;

		private IntervalConcatenator(IntervalConcatenator icToCopy)
		{
			if (icToCopy == null)
			{
				intervals = new List<Interval>();
				return;
			}
			intervals = icToCopy.GetIntervals();
		}

		public IntervalConcatenator()
			: this(null)
		{
		}

		public void Add(DateTime startDate, DateTime endDate)
		{
			if (endDate < startDate)
			{
				Debug.Fail("Invalid interval");
				return;
			}
			if (startDate == endDate) return;

			var newInterval = new Interval(startDate, endDate);

			int idx = intervals.BinarySearch(newInterval, Interval.StartDateComparer);
			if (idx >= 0) //found an interval with same startdate
			{
				if (intervals[idx].EndDate < newInterval.EndDate)
				{
					intervals[idx] = new Interval(intervals[idx].StartDate, newInterval.EndDate);
					MergeAfterIfApplicable(idx);
				}
			}
			else
			{
				int newIdx = ~idx;
				//insert, skip or merge
				if (newIdx > 0)
				{
					if (intervals[newIdx - 1].EndDate > newInterval.EndDate) return; //full overlap so skip it
					if (intervals[newIdx - 1].EndDate >= newInterval.StartDate) //partial overlap
					{
						intervals[newIdx - 1] = new Interval(intervals[newIdx - 1].StartDate, newInterval.EndDate);
						MergeAfterIfApplicable(newIdx - 1);
						return;
					}
				}
				intervals.Insert(newIdx, newInterval);
				MergeAfterIfApplicable(newIdx);
			}
		}

		public void Remove(DateTime startDate, DateTime endDate)
		{
			if (endDate < startDate)
			{
				Debug.Fail("Invalid interval");
				return;
			}
			if (startDate == endDate) return;

			var newInterval = new Interval(startDate, endDate);

			int idx = intervals.BinarySearch(newInterval, Interval.StartDateComparer);
			if (idx >= 0) //found an interval with same startdate
			{
				RemoveAfterUntil(idx, newInterval.EndDate);
			}
			else
			{
				int newIdx = ~idx;
				if (newIdx > 0
					&& intervals[newIdx - 1].EndDate > newInterval.StartDate)
				{
					var oldEndDate = intervals[newIdx - 1].EndDate;
					intervals[newIdx - 1] = new Interval(intervals[newIdx - 1].StartDate, newInterval.StartDate);
					if (newInterval.EndDate < oldEndDate) //if we cut out the middle than we should increase the number of intervals
					{
						intervals.Insert(newIdx, new Interval(newInterval.EndDate, oldEndDate));
						return;
					}
				}
				RemoveAfterUntil(newIdx, newInterval.EndDate);
			}
		}

		public IntervalConcatenator Merge(IntervalConcatenator ic)
		{
			if (ic == null) return this;
			foreach (var interval in ic.intervals)
			{
				Add(interval.StartDate, interval.EndDate);
			}
			return this;
		}

		public IntervalConcatenator Subtract(IntervalConcatenator ic)
		{
			if (ic == null || intervals.Count == 0) return this;
			var subtractSearch = new Interval(intervals[0].StartDate, DateTime.MaxValue);
			int firstIdx = ic.intervals.BinarySearch(subtractSearch, Interval.StartDateComparer);
			firstIdx = firstIdx < -1
				? (~firstIdx) - 1
				: firstIdx >= 0
					? firstIdx
					: 0;

			var lastEndDate = intervals[intervals.Count - 1].EndDate;
			for (int i = firstIdx; i < ic.intervals.Count; i++)
			{
				var curr = ic.intervals[i];
				if (curr.StartDate >= lastEndDate) break;
				Remove(curr.StartDate, curr.EndDate);
			}
			return this;
		}

		public List<Interval> GetIntervals()
		{
			return new List<Interval>(intervals);
		}

		public StartEndDateTime? GetBoundaries()
		{
			return intervals.Count == 0
				? new StartEndDateTime?()
				: new StartEndDateTime(intervals[0].StartDate, intervals[intervals.Count - 1].EndDate);
		}

		public TimeSpan Duration()
		{
			var result = TimeSpan.Zero;
			foreach (var interval in intervals)
			{
				result += interval.EndDate - interval.StartDate;
			}
			return result;
		}

		public void Clear()
		{
			intervals.Clear();
		}

		private void RemoveAfterUntil(int idx, DateTime date)
		{
			int currIdx = idx;
			while (currIdx < intervals.Count
				&& intervals[currIdx].EndDate <= date)
			{
				intervals.RemoveAt(currIdx);
			}

			if (currIdx < intervals.Count
				&& intervals[currIdx].StartDate < date)
			{
				intervals[currIdx] = new Interval(date, intervals[currIdx].EndDate);
			}
		}

		private void MergeAfterIfApplicable(int idx)
		{
			int lastMergeCandidate = idx + 1;
			DateTime? endDateCandidate = null;
			while (lastMergeCandidate < intervals.Count
				&& intervals[idx].EndDate >= intervals[lastMergeCandidate].StartDate)
			{
				endDateCandidate = intervals[lastMergeCandidate].EndDate;
				intervals.RemoveAt(lastMergeCandidate);
			}

			if (endDateCandidate.HasValue
				&& endDateCandidate > intervals[idx].EndDate)
			{
				intervals[idx] = new Interval(intervals[idx].StartDate, endDateCandidate.Value);
			}
		}

		public struct Interval : ICloneable, IEquatable<Interval>
		{
			public readonly DateTime StartDate;
			public readonly DateTime EndDate;
			public static readonly IComparer<Interval> StartDateComparer = new IntervalStartDateComparer();
			private class IntervalStartDateComparer : IComparer<Interval>
			{
				public int Compare(Interval x, Interval y)
				{
					return DateTime.Compare(x.StartDate, y.StartDate);
				}
			}

			public Interval(DateTime startDate, DateTime endDate)
			{
				StartDate = startDate;
				EndDate = endDate;
			}

			#region ICloneable Members

			object ICloneable.Clone()
			{
				return Clone();
			}

			public Interval Clone()
			{
				return new Interval(this.StartDate, this.EndDate);
			}

			#endregion

			#region IEquatable<Interval> Members

			public override bool Equals(object obj)
			{
				if (Object.ReferenceEquals(obj, null))
					return false;
				return obj is Interval && this.Equals((Interval)obj);
			}

			public override int GetHashCode()
			{
				int result = 17;
				result = 31 * result + StartDate.GetHashCode();
				result = 31 * result + EndDate.GetHashCode();
				return result;
			}

			public bool Equals(Interval other)
			{
				return DateTime.Equals(StartDate, other.StartDate)
					   && DateTime.Equals(EndDate, other.EndDate);
			}

			#endregion

			public override string ToString()
			{
				return StartDate + " - " + EndDate;
			}
		}


		#region ICloneable Members

		object ICloneable.Clone()
		{
			return Clone();
		}

		public IntervalConcatenator Clone()
		{
			return new IntervalConcatenator(this);
		}

		#endregion
	}
}
