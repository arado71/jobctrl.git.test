using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Rules.Collector
{
	public class ThrottledStore<T>
	{
		private readonly IEqualityComparer<T> comparer;
		private readonly DateTime[] changeWindow;
		private readonly TimeSpan changeWindowSize;
		private int changeIdx;
		private bool isLastValueExceededLimit;
		private DateTime lastDateTime;
		private T lastValue;

		//public ThrottledStore(int startingQuota, int maxQuota, TimeSpan quotaWindow, TimeSpan quotaIncreaseInterval)

		public ThrottledStore(int maxChangeCount, TimeSpan changeWindowSize)
			: this(maxChangeCount, changeWindowSize, EqualityComparer<T>.Default)
		{
		}

		public ThrottledStore(int maxChangeCount, TimeSpan changeWindowSize, IEqualityComparer<T> comparer)
		{
			changeWindow = new DateTime[maxChangeCount];
			this.changeWindowSize = changeWindowSize;
			this.comparer = comparer;
		}

		private bool IsThrottleLimitExceeded(DateTime now)
		{
			Debug.Assert(now - changeWindow[changeIdx] >= TimeSpan.Zero);
			return now - changeWindow[changeIdx] < changeWindowSize;
		}

		public ThrottledValue<T> Set(T value, DateTime now)
		{
			if (now < lastDateTime) return ThrottledValue.Create(lastValue, isLastValueExceededLimit); //time is invalid
			var changed = !comparer.Equals(value, lastValue); //value is changed
			var isThrottleLimitExceeded = (changed || isLastValueExceededLimit) && IsThrottleLimitExceeded(now); //if the value is not changed and the last set was within limits, we won't check beacuse that could be a false positive
			changed |= !isThrottleLimitExceeded && isLastValueExceededLimit; //isThrottleLimitExceeded is changed so we threat is as a change

			if (changed)
			{
				lastDateTime = now;
				lastValue = value;
				isLastValueExceededLimit = isThrottleLimitExceeded;
				if (!isThrottleLimitExceeded)
				{
					changeWindow[changeIdx] = now;
					changeIdx = (changeIdx + 1) % changeWindow.Length;
				}
			}
			return ThrottledValue.Create(lastValue, isThrottleLimitExceeded);
		}

		public ThrottledValue<T> Get(DateTime now)
		{
			return Set(lastValue, now);
		}
	}
}
