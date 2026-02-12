using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public class Interval : IEquatable<Interval>
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public Interval()
		{
		}

		public Interval(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}

		public Interval(Interval interval)
		{
			StartDate = interval.StartDate;
			EndDate = interval.EndDate;
		}

		public TimeSpan Duration
		{
			get
			{
				return EndDate - StartDate;
			}
		}

		public static readonly IComparer<Interval> StartDateComparer = new IntervalStartDateComparer();
		private class IntervalStartDateComparer : IComparer<Interval>
		{
			public int Compare(Interval x, Interval y)
			{
				return DateTime.Compare(x.StartDate, y.StartDate);
			}
		}

		public static IEnumerable<Interval> Split(Interval i, DateTime splitAt)
		{
			yield return new Interval { StartDate = i.StartDate, EndDate = splitAt };
			yield return new Interval { StartDate = splitAt, EndDate = i.EndDate };
		}

		public Interval FromLocalToUtc()
		{
			return new Interval { StartDate = StartDate.FromLocalToUtc(), EndDate = EndDate.FromLocalToUtc() };
		}

		public Interval FromUtcToLocal()
		{
			return new Interval { StartDate = StartDate.FromUtcToLocal(), EndDate = EndDate.FromUtcToLocal() };
		}

		private void RemoveFromStart(Interval removeInterval)
		{
			StartDate = removeInterval.EndDate;
		}

		private void RemoveFromEnd(Interval removeInterval)
		{
			EndDate = removeInterval.StartDate;
		}

		private Interval RemoveFromMiddle(Interval middleInterval)
		{
			var endDate = EndDate;
			EndDate = middleInterval.StartDate;
			return new Interval { StartDate = middleInterval.EndDate, EndDate = endDate };
		}

		public static TimeSpan GetLength(IEnumerable<Interval> intervals)
		{
			return intervals.Aggregate(new TimeSpan(), ((timeSpan, i) => timeSpan + (i.EndDate - i.StartDate)));
		}

		private static List<Interval> RemoveInternal(IList<Interval> baseIntervals, IEnumerable<Interval> removeIntervals)
		{
			var intervals = baseIntervals.Select(x => new Interval() { StartDate = x.StartDate, EndDate = x.EndDate }).ToList();
			foreach (var removeInterval in removeIntervals)
			{
				for (int i = 0; i < intervals.Count; i++)
				{
					var interval = intervals[i];
					// No intersection if these occur
					if (interval.StartDate >= removeInterval.EndDate) continue;
					if (interval.EndDate <= removeInterval.StartDate) continue;
					// Full coverage, remove current interval
					if (interval.StartDate >= removeInterval.StartDate && interval.EndDate <= removeInterval.EndDate)
					{
						intervals.RemoveAt(i);
						i--;
						continue;
					}

					// Easy trimming if these occur
					if (interval.StartDate >= removeInterval.StartDate && interval.EndDate >= removeInterval.EndDate)
					{
						interval.RemoveFromStart(removeInterval);
						if (interval.StartDate == interval.EndDate)
						{
							intervals.RemoveAt(i);
							i--;
						}

						continue;
					}
					if (interval.StartDate <= removeInterval.StartDate && interval.EndDate <= removeInterval.EndDate)
					{
						interval.RemoveFromEnd(removeInterval);
						if (interval.StartDate == interval.EndDate)
						{
							intervals.RemoveAt(i);
							i--;
						}

						continue;
					}

					// Trim from the middle
					Debug.Assert(interval.StartDate < removeInterval.EndDate && interval.EndDate > removeInterval.EndDate);
					intervals.Insert(i + 1, interval.RemoveFromMiddle(removeInterval));
					i++;
				}
			}

			return intervals;
		}

		public IEnumerable<Interval> Subtract(Interval other)
		{
			if (IsNonOverlapping(other))
			{
				// Non overlapping
				yield break;
			}

			// Full overlap
			if (StartDate >= other.StartDate && EndDate <= other.EndDate)
			{
				yield break;
			}

			// Partial overlap on the left
			if (StartDate >= other.StartDate && EndDate >= other.EndDate)
			{
				yield return new Interval { StartDate = other.EndDate, EndDate = EndDate };
				yield break;
			}

			// Partial overlap on the right
			if (StartDate <= other.StartDate && EndDate <= other.EndDate)
			{
				yield return new Interval { StartDate = StartDate, EndDate = other.StartDate };
				yield break;
			}

			// Overlap inside
			yield return new Interval { StartDate = StartDate, EndDate = other.StartDate };
			yield return new Interval { StartDate = other.EndDate, EndDate = EndDate };
		}

		public static List<Interval> Remove(IEnumerable<Interval> baseIntervals, IEnumerable<Interval> removeIntervals)
		{
			var result = new List<Interval>(baseIntervals);
			return RemoveInternal(result, removeIntervals);
		}

		public List<Interval> Remove(IEnumerable<Interval> intervals)
		{
			return Remove(new[] { this }, intervals);
		}

		public Interval Intersect(Interval interval)
		{
			Debug.Assert(interval.StartDate <= interval.EndDate);
			return IsNonOverlapping(interval) ? null : new Interval() {StartDate = interval.StartDate > StartDate ? interval.StartDate : StartDate, EndDate = interval.EndDate > EndDate ? EndDate : interval.EndDate};
		}

		public IEnumerable<Interval> Intersect(IEnumerable<Interval> intervals)
		{
			return intervals.Select(interval => Intersect(interval)).Where(res => res != null);
		}

		public bool IsNonOverlapping(Interval other)
		{
			if (other == null) return true;
			return (other.EndDate <= StartDate || other.StartDate >= EndDate);
		}

		public static Interval GetBounds(IEnumerable<Interval> intervals)
		{
			DateTime min = DateTime.MaxValue;
			DateTime max = DateTime.MinValue;
			foreach (var interval in intervals)
			{
				Debug.Assert(interval.StartDate <= interval.EndDate);
				if (interval.StartDate < min)
				{
					min = interval.StartDate;
				}

				if (interval.EndDate > max)
				{
					max = interval.EndDate;
				}
			}

			return new Interval() { StartDate = min, EndDate = max };
		}

		public static Interval FindSpace(DateTime time, List<Interval> intervals)
		{
			Debug.Assert(intervals.SequenceEqual(intervals.OrderBy(x => x, StartDateComparer))); // We assume intervals are ordered
			var res = intervals.BinarySearch(new Interval(time, time), StartDateComparer);
			if (res < 0)
			{
				res = ~res;
			}

			if (res == 0 || res == intervals.Count) return null; // No result on sides
			if (intervals[res-1].EndDate <= time && intervals[res].StartDate >= time) return new Interval(intervals[res-1].EndDate, intervals[res].StartDate);
			return null;
		}

		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			if (obj.GetType() != typeof(Interval))
				return false;
			return Equals((Interval) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (EndDate.GetHashCode() * 397) ^ StartDate.GetHashCode();
			}
		}

		public bool Equals(Interval other)
		{
			if (other == null) return false;
			return StartDate == other.StartDate && EndDate == other.EndDate;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", StartDate, EndDate);
		}
	}
}
