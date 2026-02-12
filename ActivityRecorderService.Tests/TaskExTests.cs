using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	//http://stackoverflow.com/questions/4959149/do-timer-object-get-gc-ed-when-no-other-object-references-them ???
	public class TaskExTests
	{
		private static void GCCollect()
		{
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			GC.WaitForPendingFinalizers();
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
		}

		[Fact]
		public void TimerIsNotDisposedInDelay()
		{
			var init = 0;
			var set = TaskEx.Delay(3000).ContinueWith(n => init = 1);
			GCCollect();
			Assert.False(set.IsCompleted);
			set.Wait();
			Assert.Equal(1, init);
		}

		int local;
		[Fact]
		public void TimerIsNotDisposedWhenStateIsThis()
		{
			local = 0;
			new Timer(CallBack).Change(1000, Timeout.Infinite); //this ctor keeps the timer alive! //http://msdn.microsoft.com/en-us/library/ms149618.aspx
			GCCollect();
			Thread.Sleep(2200);
			Thread.MemoryBarrier();
			Assert.Equal(2, local);
		}

		private void CallBack(object state)
		{
			local = 2;
			Thread.MemoryBarrier();
		}

		[Fact]
		public void TimerIsDisposedOtherwise()
		{
			local = 0;
			new Timer(CallBack, null, 3000, Timeout.Infinite); //this ctor won't keep the timer alive!
			GCCollect();
			Thread.Sleep(4500);
			Thread.MemoryBarrier();
			Assert.Equal(0, local);
		}

	}
}
