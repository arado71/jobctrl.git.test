using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.Search
{
	/// <summary>
	/// Very basic (and slow) implementation for matching work names.
	/// </summary>
	public class ProjectNameMatcher : StringMatcher
	{
		private readonly Dictionary<string, List<IndexData>> index = new Dictionary<string, List<IndexData>>();
		private readonly string[] splitArray = new[] { WorkDataWithParentNames.DefaultSeparator };

		public override void Add(int id, string text)
		{
			text = RemoveDiacritics(text);
			base.Add(id, text);

			var parts = text.Split(splitArray, StringSplitOptions.RemoveEmptyEntries);

			var posFromBack = 0;
			for (int i = parts.Length - 1; i >= 0; i--)
			{
				AddToIndex(parts[i], id, posFromBack++);
			}
		}

		public override void Clear()
		{
			base.Clear();

			index.Clear();
		}

		private void AddToIndex(string part, int id, int posFromBack)
		{
			List<IndexData> bucket;
			if (!index.TryGetValue(part, out bucket))
			{
				bucket = new List<IndexData>();
				index.Add(part, bucket);
			}
			bucket.Add(new IndexData() { Id = id, PositionFromBack = posFromBack });
		}

		public override IEnumerable<int> GetMatches(string text)
		{
			if (string.IsNullOrEmpty(text)) return Enumerable.Empty<int>();
			text = RemoveDiacritics(text);
			var parts = text
				.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries)
				.OrderByDescending(n => n.Length)
				.ToArray()
				;
			//we don't care about the order of the search words
			//but longest word have a better chance for higher selectivity

			var canditateDict = new Dictionary<int, SelectedData>();
			var isFirst = true;
			foreach (var part in parts) //todo this is slow for many search words
			{
				var idsFound = new HashSet<int>();
				var locPart = part;
				foreach (var currData in index.SelectMany(n => n.Value.Select(m => new SelectedData()
				{
					Index = n.Key.IndexOf(locPart, StringComparison.OrdinalIgnoreCase),
					Id = m.Id,
					PositionFromBack = m.PositionFromBack
				})))
				{
					if (currData.Index < 0) continue;
					idsFound.Add(currData.Id);
					SelectedData oldData;
					if (canditateDict.TryGetValue(currData.Id, out oldData))
					{
						if (oldData.Index > currData.Index)
						{
							oldData.Index = currData.Index;
							oldData.PositionFromBack = currData.PositionFromBack;
						}
						else if (oldData.Index == currData.Index)
						{
							oldData.PositionFromBack = Math.Min(oldData.PositionFromBack, currData.PositionFromBack);
						}
					}
					else if (isFirst)
					{
						canditateDict.Add(currData.Id, currData);
					}
				}
				foreach (var id in canditateDict.Keys.ToArray())
				{
					if (!idsFound.Contains(id))
					{
						canditateDict.Remove(id);
					}
				}
				isFirst = false;
			}

			return canditateDict.Values
				.OrderBy(n => n.Index == 0 ? 0 : 1) //match at the begining of the word
				.ThenBy(n => n.PositionFromBack) //last word match is more relevant
				.Select(n => n.Id)
				.Distinct();
		}

		private class IndexData
		{
			public int Id { get; set; }
			public int PositionFromBack { get; set; }
		}

		private class SelectedData : IndexData
		{
			public int Index { get; set; }

			public override string ToString()
			{
				return "Id: " + Id + " Idx: " + Index + " Pos: " + PositionFromBack;
			}
		}
	}
}
