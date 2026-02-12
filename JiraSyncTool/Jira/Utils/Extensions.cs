using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Utils
{
	public static class Extensions
	{
		public static IEnumerable<T> Flatten<T>(this T source, Func<T, IEnumerable<T>> selector) where T : IFlattenable
		{
			yield return source;

			var childSeq = selector(source);
			if (childSeq != null)
			{
				foreach (T item in Flatten(childSeq, selector))
				{
					yield return item;
				}
			}
		}

		public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
		{
			foreach (T item in source)
			{
				yield return item;

				var childSeq = selector(item);
				if (childSeq != null)
				{
					// recursive call to Traverse to get child of child ...
					foreach (T itemRecurse in Flatten(childSeq, selector))
					{
						yield return itemRecurse;
					}
				}
			}
		}

		public interface IFlattenable
		{
		}
	}
}
