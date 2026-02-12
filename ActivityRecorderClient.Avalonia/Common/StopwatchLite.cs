using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient
{
	public class StopwatchLite
	{
		private int lastCheck;
		private readonly int interval;

		public StopwatchLite(TimeSpan measureInterval, bool startWithIntervalElapsed) //we loose some accuracy here but this class is not accurate at all
			: this((int)measureInterval.TotalMilliseconds, startWithIntervalElapsed)
		{
		}

		public StopwatchLite(TimeSpan measureInterval)
			: this((int)measureInterval.TotalMilliseconds)
		{
		}

		public StopwatchLite(int measureInterval)
			: this(measureInterval, false)
		{
		}

		public StopwatchLite(int measureInterval, bool startWithIntervalElapsed)
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

		private bool IsIntervalElapsedSinceLastCheck(bool autoRestart)
		{
			var result = (uint)(Environment.TickCount - lastCheck) > interval;
			if (result && autoRestart)
			{
				Restart(); //this is not accurate, but we want this behaviour (not the lastCheck += interval)
			}
			return result;
		}
	}
}
