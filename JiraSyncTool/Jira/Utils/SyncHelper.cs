using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraSyncTool.Jira.Utils
{
	/// <summary>
	/// This class is the same as under the MSProj namespace.
	/// </summary>
	public static class SyncHelper
	{
		/// <summary>
		/// Calculates the differences between <paramref name="source"/> and <paramref name="target"/> collections.
		/// </summary>
		/// <typeparam name="T">Type of collection</typeparam>
		/// <param name="source">The source collection to get changes of.</param>
		/// <param name="target">The target collection to compare to.</param>
		/// <param name="comparer">Comparer which defines the equality.</param>
		/// <returns>The differences in source compared to target.</returns>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="source"/> or <paramref name="target"/> has duplicate elements based on <paramref name="comparer"/>.</exception>
		public static Difference<T> CalculateDifferences<T>(IEnumerable<T> source, IEnumerable<T> target,
			IEqualityComparer<T> comparer = null)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (target == null) throw new ArgumentNullException("target");

			if (comparer == null)
			{
				comparer = EqualityComparer<T>.Default;
			}

			var sourceOnly = source.ToDictionary(key => key, value => value, comparer);
			var targetOnly = target.ToDictionary(key => key, value => value, comparer);
			var common = new List<CommonElement<T>>();
			var isSourceSmaller = sourceOnly.Count < targetOnly.Count;
			foreach (var candidate in (isSourceSmaller ? sourceOnly.Keys : targetOnly.Keys))
			{
				if ((isSourceSmaller ? targetOnly : sourceOnly).ContainsKey(candidate))
				{
					var sourceElement = isSourceSmaller ? candidate : sourceOnly[candidate];
					var targetElement = isSourceSmaller ? targetOnly[candidate] : candidate;
					common.Add(new CommonElement<T>(sourceElement, targetElement));
				}
			}

			foreach (var commonElement in common)
			{
				sourceOnly.Remove(commonElement.Source);
				targetOnly.Remove(commonElement.Target);
			}

			return new Difference<T>(common, sourceOnly.Keys, targetOnly.Keys);
		}

		/// <summary>
		/// Contains the changes in a source collection compared to target collection.
		/// </summary>
		/// <typeparam name="T">Type of collection elements</typeparam>
		public class Difference<T>
		{
			/// <summary>
			/// Elements present in both collections.
			/// </summary>
			public IEnumerable<CommonElement<T>> Common { get; private set; }
            public int CommonCounter { get { return Common.Count(); } }
			/// <summary>
			/// Elements present only in source, which have been added to source or removed from target.
			/// </summary>
			public IEnumerable<T> Added { get; private set; }
            public int AddedCounter { get { return Added.Count(); } }

            /// <summary>
            /// Elements present only in target, which have been removed from source or added to target.
            /// </summary>
            public IEnumerable<T> Removed { get; private set; }
            public int RemovedCounter { get { return Removed.Count(); } }
            public int Count { get { return CommonCounter + AddedCounter + RemovedCounter; } }

            public Difference(IEnumerable<CommonElement<T>> common, IEnumerable<T> added, IEnumerable<T> removed)
			{
				Common = common;
				Added = added;
				Removed = removed;
			}
		}

		/// <summary>
		/// Represents a pair of elements, matched by equality comparison.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public class CommonElement<T>
		{
			/// <summary>
			/// Element from the source collection.
			/// </summary>
			public T Source { get; private set; }
			/// <summary>
			/// Element from the target collection.
			/// </summary>
			public T Target { get; private set; }

			public CommonElement(T source, T target)
			{
				Source = source;
				Target = target;
			}
		}

}
}
