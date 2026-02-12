using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService.Caching
{
	/// <summary>
	/// Thread-safe class for caching lookupids in order to reduce db reads on DesktopWindow inserts
	/// </summary>
	/// <remarks>
	/// magic numbers in ConfigManager are based on DB data:
	/// on 2012-08-08 we had 378, 17667, 3602 distinct process, title, url ids. The daily maxes for that week were (380, 19032, 3730)
	/// on 2013-05-13 we had 700, 50186, 7470 from 2013-05-06 to 2013-05-11 we had 1212, 194434, 27401 distinct values
	/// </remarks>
	public class LookupIdCache
	{
		private readonly LruCache<string, int> processCache;
		private readonly LruCache<string, int> titleCache;
		private readonly LruCache<string, int> urlCache;
		private readonly LookupStatsManager stats;

		public LookupIdCache(int processNameCapacity, int titleCapacity, int urlCapacity)
		{
			processCache = new LruCache<string, int>(processNameCapacity, StringComparer.OrdinalIgnoreCase);
			titleCache = new LruCache<string, int>(titleCapacity, StringComparer.OrdinalIgnoreCase);
			urlCache = new LruCache<string, int>(urlCapacity, StringComparer.OrdinalIgnoreCase);
			stats = new LookupStatsManager(processCache, titleCache, urlCache);
			stats.Start();
		}

		public int? GetIdForProcessName(string processName)
		{
			if (processName == null) return null;
			int id;
			if (processCache.TryGetValue(processName, out id))
			{
				stats.IncrementProcessHit();
				return id;
			}
			stats.IncrementProcessMiss();
			return null;
		}

		public int? GetIdForTitle(string title)
		{
			if (title == null) return null;
			int id;
			if (titleCache.TryGetValue(title, out id))
			{
				stats.IncrementTitleHit();
				return id;
			}
			stats.IncrementTitleMiss();
			return null;
		}

		public int? GetIdForUrl(string url)
		{
			if (url == null) return null;
			int id;
			if (urlCache.TryGetValue(url, out id))
			{
				stats.IncrementUrlHit();
				return id;
			}
			stats.IncrementUrlMiss();
			return null;
		}

		public void AddProcessName(string processName, int processId)
		{
			processCache.TryAdd(processName, processId);
		}

		public void AddTitle(string title, int titleId)
		{
			titleCache.TryAdd(title, titleId);
		}

		public void AddUrl(string url, int urlId)
		{
			urlCache.TryAdd(url, urlId);
		}

		public void Clear()
		{
			processCache.Clear();
			titleCache.Clear();
			urlCache.Clear();
		}
	}
}
