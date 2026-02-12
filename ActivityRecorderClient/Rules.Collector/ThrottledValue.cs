using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Rules.Collector
{
	public class ThrottledValue
	{
		private ThrottledValue()
		{
		}

		public static ThrottledValue<T> Create<T>(T value, bool isThrottleLimitExceeded)
		{
			return ThrottledValue<T>.Create(value, isThrottleLimitExceeded);
		}
	}

	public class ThrottledValue<T>
	{
		public bool IsThrottleLimitExceeded { get; private set; }
		public T Value { get; private set; }

		private ThrottledValue()
		{
		}

		public static ThrottledValue<T> Create(T value, bool isThrottleLimitExceeded)
		{
			return new ThrottledValue<T> { Value = value, IsThrottleLimitExceeded = isThrottleLimitExceeded };
		}
	}
}
