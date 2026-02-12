using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace JCAutomation
{
	public static class Extensions
	{
		public static int IndexOf(this TreeNode node, TreeNode tvi)
		{
			return node.Nodes.Cast<object>().TakeWhile(n => !n.Equals(tvi)).Count();
		}

		public static bool IsEmpty(this string s)
		{
			return string.IsNullOrEmpty(s);
		}
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
