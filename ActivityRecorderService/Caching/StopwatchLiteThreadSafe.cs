using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderService.Caching
{
	class StopwatchLiteThreadSafe //based on StopwatchLite but made threadsafe
	{
		private volatile int lastCheck;
		private readonly int interval;

		public StopwatchLiteThreadSafe(TimeSpan measureInterval, bool startWithIntervalElapsed) //we loose some accuracy here but this class is not accurate at all
			: this((int)measureInterval.TotalMilliseconds, startWithIntervalElapsed)
		{
		}

		public StopwatchLiteThreadSafe(TimeSpan measureInterval)
			: this((int)measureInterval.TotalMilliseconds)
		{
		}

		public StopwatchLiteThreadSafe(int measureInterval)
			: this(measureInterval, false)
		{
		}

		public StopwatchLiteThreadSafe(int measureInterval, bool startWithIntervalElapsed)
		{
			interval = measureInterval;
			if (startWithIntervalElapsed)
			{
				SetIntervalElapsed();
			}
			else
			{
				Restart();
			}
		}

		public void Restart()
		{
			lastCheck = Environment.TickCount;
		}

		public void SetIntervalElapsed()
		{
			lastCheck = Environment.TickCount - interval - 1;
		}

		public bool IsIntervalElapsed()
		{
			return IsIntervalElapsedSinceLastCheck(false);
		}

		public bool IsIntervalElapsedSinceLastCheck()
		{
			return IsIntervalElapsedSinceLastCheck(true);
		}

#pragma warning disable 0420
		private bool IsIntervalElapsedSinceLastCheck(bool autoRestart)
		{
			bool result;
			int now, last;
			do
			{
				last = lastCheck;
				now = Environment.TickCount;
				result = (uint)(now - last) > interval;
				if (!result || !autoRestart) return result;
			}
			while (Interlocked.CompareExchange(ref lastCheck, now, last) != last);  //this is not accurate, but we want this behaviour (not the lastCheck += interval)
			return result; //true
		}
#pragma warning restore 0420
	}
}
