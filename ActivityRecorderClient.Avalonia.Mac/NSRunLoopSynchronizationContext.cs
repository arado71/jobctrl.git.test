using System;
using System.Threading;
using Foundation;

namespace Tct.ActivityRecorderClient
{
	//idea from: https://gist.github.com/1100597
	public class NSRunLoopSynchronizationContext : SynchronizationContext
	{
		NSRunLoop currentRunLoop;

		public NSRunLoopSynchronizationContext()
		{
			currentRunLoop = NSRunLoop.Current;
			if (currentRunLoop == null)
				throw new InvalidOperationException();
		}

		private NSRunLoopSynchronizationContext(NSRunLoop nsRunLoop)
		{
			if (nsRunLoop == null)
				throw new ArgumentNullException();
			currentRunLoop = nsRunLoop;
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			currentRunLoop.BeginInvokeOnMainThread(() => d(state));
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			currentRunLoop.InvokeOnMainThread(() => d(state));
		}

		public override SynchronizationContext CreateCopy()
		{
			return new NSRunLoopSynchronizationContext(currentRunLoop);
		}
	}
}

