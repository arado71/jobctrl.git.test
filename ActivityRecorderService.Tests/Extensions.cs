using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.Tests.ActivityRecorderService
{
	public static class Extensions
	{
		//http://stackoverflow.com/questions/756223/can-you-write-a-permutation-function-just-as-elegantly-in-c
		public static IEnumerable<T[]> Permute<T>(this T[] xs, params T[] pre)
		{
			if (xs.Length == 0) yield return pre;
			for (int i = 0; i < xs.Length; i++)
			{
				foreach (T[] y in Permute(xs.Take(i).Union(xs.Skip(i + 1)).ToArray(), pre.Union(new[] { xs[i] }).ToArray()))
				{
					yield return y;
				}
			}
		}

	}
}
