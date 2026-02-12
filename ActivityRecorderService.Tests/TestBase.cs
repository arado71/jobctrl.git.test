using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class TestBase
	{

		#region Helpers

		public static void AssertValueTypeOrStringPropertiesAreTheSame(object first, object second)
		{
			Assert.NotNull(first);
			Assert.NotNull(second);
			Type type = first.GetType();
			Assert.True(type == second.GetType(), "Type mismatch");
			foreach (var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(n => n.CanRead))
			{
				var method = propInfo.GetGetMethod();
				if (method.ReturnType.IsValueType || method.ReturnType == typeof(string))
				{
					object firstValue = method.Invoke(first, null);
					object secondValue = method.Invoke(second, null);
					Assert.True(object.Equals(firstValue, secondValue), string.Format("'{0}' != '{1}'", firstValue, secondValue));
				}
			}
		}

		public static Type GetAssertExceptionType<T>() where T : Exception
		{
#if DEBUG
			return typeof(Xunit.Sdk.TraceAssertException);
#else
			return typeof(T);
#endif
		}

		#endregion
	}
}
