using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PlaybackClient.Tests
{
	public class DoneDetector
	{
		private int signalsNeeded;
		private readonly ManualResetEventSlim done = new ManualResetEventSlim();

		public DoneDetector()
			: this(1)
		{
		}

		public DoneDetector(int signalsNeeded)
		{
			this.signalsNeeded = signalsNeeded;
		}

		public void Signal()
		{
			var rem = Interlocked.Decrement(ref signalsNeeded);
			if (rem == 0)
			{
				done.Set();
			}
			else if (rem < 0)
			{
				throw new Exception("Too many signals");
			}
		}

		public bool Wait(TimeSpan timeout)
		{
			return done.Wait(timeout);
		}
	}
}
