using System;
using System.Collections.Generic;
using System.Linq;

namespace Tct.ActivityRecorderClient.Search
{
	public sealed class PrefixMatchResult
	{
		public int Index { get; init; }
		public int PrefixLength { get; init; }
		public bool IsPerfectMatch { get; init; }
	}

	public static class PrefixMatcher
	{
		public static IReadOnlyList<PrefixMatchResult> FindMatches(string source, IReadOnlyList<string?> candidates)
		{
			if (candidates == null) throw new ArgumentNullException(nameof(candidates));
			if (source == null) return [];

			var results = new List<PrefixMatchResult>();

			for (int i = 0; i < candidates.Count; i++)
			{
				var candidate = candidates[i];
				if (string.IsNullOrEmpty(candidate))
					continue;

				int prefixLength = GetCommonPrefixLength(source, candidate);
				if (prefixLength == 0)
					continue;

				results.Add(new PrefixMatchResult
				{
					Index = i,
					PrefixLength = prefixLength,
					IsPerfectMatch = source.Length == candidate.Length &&
									 prefixLength == source.Length
				});
			}

			return results
				.OrderByDescending(r => r.IsPerfectMatch)
				.ThenByDescending(r => r.PrefixLength)
				.ToList();
		}

		private static int GetCommonPrefixLength(string a, string b)
		{
			int length = Math.Min(a.Length, b.Length);
			int i = 0;

			while (i < length && a[i] == b[i])
				i++;

			return i;
		}
	}
}
