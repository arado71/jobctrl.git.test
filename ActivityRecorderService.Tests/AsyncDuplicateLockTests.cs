using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Common;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class AsyncDuplicateLockTests
	{
		[Fact]
		public void SequentialLocks()
		{
			var keyedLockStore = new AsyncDuplicateLock<int>();
			using (keyedLockStore.Lock(1)) { }
			using (keyedLockStore.Lock(1)) { }
			using (keyedLockStore.Lock(1)) { }
		}

		[Fact]
		public void OverlappedLocks()
		{
			var keyedLockStore = new AsyncDuplicateLock<int>();
			var start1event = new ManualResetEvent(false);
			var start2event = new ManualResetEvent(false);
			var cont1event = new ManualResetEvent(false);
			var cont2event = new ManualResetEvent(false);
			var state = (string)null;

			void Step1()
			{
				start1event.WaitOne();
				using (keyedLockStore.Lock(1))
				{
					Assert.Null(state);
					state = "step1";
					start2event.Set();
					cont1event.WaitOne();
					state = null;
				}
			}

			void Step2()
			{
				start2event.WaitOne();
				using (keyedLockStore.Lock(1))
				{
					Assert.Null(state);
					state = "step2";
					cont2event.WaitOne();
					state = null;
				}
			}

			var tasks = new[] { Task.Run(() => Step1()), Task.Run(() => Step2()) };
			start1event.Set();
			Task.Delay(100);
			cont1event.Set();
			Task.Delay(100);
			cont2event.Set();

			Assert.True(Task.WaitAll(tasks, 2000));
			Assert.Null(state);

		}

	}
}
