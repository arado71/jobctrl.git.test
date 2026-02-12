using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter
{
	public class OrderedList<T> : IEnumerable<T>
	{
		private readonly List<T> list = new List<T>();
		private readonly IComparer<T> comparer;

		public OrderedList(IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public void Add(T item)
		{
			var idx = list.BinarySearch(item, comparer);
			if (idx < 0) idx = ~idx;
			list.Insert(idx, item);
		}

		public IEnumerable<T> EnumerateBetween(T firstInclusive, T lastExclusive)
		{
			var startInclusiveIdx = IndexOfLatestBefore(firstInclusive);
			var endExclusiveIdx = IndexOfLatestBefore(lastExclusive);
			for (var i = startInclusiveIdx; i < endExclusiveIdx; ++i)
			{
				yield return list[i];
			}
		}

		public long CountBetween(T firstInclusive, T lastExclusive)
		{
			return IndexOfLatestBefore(lastExclusive) - IndexOfLatestBefore(firstInclusive);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private int IndexOfLatestBefore(T item)
		{
			var idx = list.BinarySearch(item, comparer);
			if (idx >= 0)
			{
				while (idx >= 0 && comparer.Compare(list[idx], item) == 0) --idx;
				return idx;
			}

			return ~idx - 1;
		}
	}
}
