using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JcMon2
{
	public static class ExtensionMethods
	{
		public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> cache, TKey key,
			Func<TKey, TValue> creatorFunc)
		{
			TValue result;
			if (!cache.TryGetValue(key, out result))
			{
				result = creatorFunc(key);
				cache.Add(key, result);
			}

			return result;
		}
	}
}
