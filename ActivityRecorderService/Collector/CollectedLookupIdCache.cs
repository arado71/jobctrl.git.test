using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderService.Caching;

namespace Tct.ActivityRecorderService.Collector
{
	/// <summary>
	/// Thread-safe class for caching lookupids in order to reduce db reads on CollectedItem inserts
	/// </summary>
	public sealed class CollectedLookupIdCache
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly LruCache<string, int> keyCache;
		private readonly LruCache<string, int> valueCache;

		public CollectedLookupIdCache(int keyCacheSize, int valueCacheSize)
		{
			keyCache = new LruCache<string, int>(keyCacheSize, StringComparer.OrdinalIgnoreCase);
			valueCache = new LruCache<string, int>(valueCacheSize, StringComparer.OrdinalIgnoreCase);
		}

		public int? GetIdForKey(string key)
		{
			if (key == null) return null;
			int id;
			if (keyCache.TryGetValue(key, out id))
			{
				return id;
			}
			return null;
		}

		public int? GetIdForValue(string value)
		{
			if (value == null) return null;
			int id;
			if (valueCache.TryGetValue(value, out id))
			{
				return id;
			}
			return null;
		}

		public void AddValue(string value, int valueId)
		{
			valueCache.TryAdd(value, valueId);
		}

		public void AddKey(string key, int keyId)
		{
			log.DebugFormat("Key \"{0}\" added with id: {1}", key, keyId);
			keyCache.TryAdd(key, keyId);
		}

		public void Clear()
		{
			keyCache.Clear();
			valueCache.Clear();
		}
	}
}
