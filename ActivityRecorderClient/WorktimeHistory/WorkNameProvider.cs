using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public class WorkNameProvider 
	{
		private readonly CachedDictionary<int, WorkOrProjectWithParentNames> cache = new CachedDictionary<int, WorkOrProjectWithParentNames>(new TimeSpan(0, 5, 0), false);
		private readonly Func<List<int>, WorkNames> workNameQuery;
		private const string UnkownWork = "???";
		private const string UnknownProject = "???";

		public WorkNameProvider(Func<List<int>, WorkNames> workNameQuery)
		{
			this.workNameQuery = workNameQuery;
		}

		public IEnumerable<WorkOrProjectWithParentNames> GetWorkOrProjectWithParentNames(IEnumerable<int> workIds)
		{
			var remainingWorkIds = new List<int>(workIds);
			var result = new List<WorkOrProjectWithParentNames>();
			result.AddRange(GetEntriesFromCache(remainingWorkIds));
			result.AddRange(GetEntriesFromService(remainingWorkIds));
			result.AddRange(GetEntriesFallback(remainingWorkIds));
			cache.ClearExpired();
			return result;
		}

		private IEnumerable<WorkOrProjectWithParentNames> GetEntriesFromCache(IList<int> remainingIds)
		{
			for (int i = 0; i < remainingIds.Count; ++i)
			{
				WorkOrProjectWithParentNames res;
				if (cache.TryGetValue(remainingIds[i], out res))
				{
					remainingIds.RemoveAt(i--);
					yield return res;
				}
			}
		}

		private void BuildCacheEntry(WorkOrProjectName currentElement, IList<WorkOrProjectName> queriedElements)
		{
			Debug.Assert(currentElement.Id != null || currentElement.ProjectId != null);
			var id = currentElement.Id ?? currentElement.ProjectId.Value;
			if (currentElement.ParentId == null)
			{
				cache.Set(id, new WorkOrProjectWithParentNames { FullName = currentElement.Name, WorkOrProjectName = currentElement });
				return;
			}

			// Not in menu, look up in queried elements
			for (int i = 0; i < queriedElements.Count; i++)
			{
				if (queriedElements[i].ProjectId == currentElement.ParentId)
				{
					var parentElement = queriedElements[i];
					queriedElements.RemoveAt(i);
					BuildCacheEntry(parentElement, queriedElements); // Element gets placed or updated in cache
					break;
				}
			}

			WorkOrProjectWithParentNames parentEntry;
			cache.TryGetValue(currentElement.ParentId.Value, out parentEntry);
			cache.Set(id, new WorkOrProjectWithParentNames { FullName = (parentEntry != null ? parentEntry.FullName : UnknownProject) + WorkDataWithParentNames.DefaultSeparator + currentElement.Name, WorkOrProjectName = currentElement });
		}

		private IEnumerable<WorkOrProjectWithParentNames> GetEntriesFromService(List<int> remainingIds)
		{
			var result = new List<WorkOrProjectWithParentNames>();
			var queriedWorks = workNameQuery(remainingIds).Names;
			while (queriedWorks.Count > 0)
			{
				var currentElement = queriedWorks[0];
				Debug.Assert(currentElement.Id != null || currentElement.ProjectId != null);
				queriedWorks.RemoveAt(0);
				BuildCacheEntry(currentElement, queriedWorks);
				var id = currentElement.Id ?? currentElement.ProjectId.Value;
				if (remainingIds.Contains(id))
				{
					remainingIds.Remove(id);
					WorkOrProjectWithParentNames entry;
					if (cache.TryGetValue(id, out entry))
					{
						result.Add(entry);
					}
				}
			}

			return result;
		}

		private IEnumerable<WorkOrProjectWithParentNames> GetEntriesFallback(IEnumerable<int> ids)
		{
			return ids.Select(id => new WorkOrProjectWithParentNames { FullName = UnkownWork, WorkOrProjectName = new WorkOrProjectName { Id = id } });
		}

	}
}
