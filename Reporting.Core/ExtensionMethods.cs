using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;
using Reporter.Processing;

namespace Reporter
{
	public static class ExtensionMethods
	{
		public static bool IsOrderedBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
		{
			return source.OrderBy(keySelector).SequenceEqual(source);
		}

		public static bool IsSorted<T>(this IEnumerable<T> source) where T : IComparable<T>
		{
			return source.All((prev, next) => prev.CompareTo(next) < 1);
		}

		public static bool IsSortedAndNonOverlapping(this IEnumerable<IInterval> source)
		{
			return source.All((prev, next) => prev.StartDate < next.StartDate && prev.EndDate <= next.StartDate);
		}

		public static bool IsSorted<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
			where TKey : IComparable<TKey>
		{
			return source.Select(selector).IsSorted();
		}

		public static bool All<T>(this IEnumerable<T> source, Func<T, T, bool> predicate)
		{
			return source.Pairwise(predicate).All(x => x);
		}

		[Pure]
		public static string ToMinuteSecondStringNonNeg(this TimeSpan timeSpan)
		{
			return string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
		}

		// http://stackoverflow.com/questions/577590/pair-wise-iteration-in-c-sharp-or-sliding-window-enumerator
		[Pure]
		public static IEnumerable<TResult> Pairwise<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> resultSelector)
		{
			TSource previous = default(TSource);

			using (var it = source.GetEnumerator())
			{
				if (it.MoveNext())
					previous = it.Current;

				while (it.MoveNext())
					yield return resultSelector(previous, previous = it.Current);
			}
		}
	}
}
