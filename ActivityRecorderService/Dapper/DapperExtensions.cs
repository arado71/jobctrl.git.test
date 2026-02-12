using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper
{
	public static class DapperExtensions
	{
		public static List<T> EnsureList<T>(this IEnumerable<T> enumerable)
		{
			var res = enumerable as List<T>;
			if (res != null) return res;
			return enumerable.ToList();
		}
	}
}
