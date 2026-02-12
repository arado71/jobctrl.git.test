using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace JiraSyncTool.Jira.Utils
{
	public class ReferenceComparer<T> : IEqualityComparer<T> where T : class
	{
		public static ReferenceComparer<T> Default = new ReferenceComparer<T>();

		public bool Equals(T x, T y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}
}
