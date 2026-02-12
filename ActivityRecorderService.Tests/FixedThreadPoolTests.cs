using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class FixedThreadPoolTests
	{
		[Fact]
		public void CreateTwoThreadsInPool()
		{
			using (var pool = new FixedThreadPool(2))
			using (var syncEvent = new ManualResetEventSlim())
			using (var syncEvent11 = new ManualResetEventSlim())
			using (var syncEvent12 = new ManualResetEventSlim())
			using (var syncEvent2 = new ManualResetEventSlim())
			{
				var bag = new ConcurrentBag<int>();
				pool.QueueUserWorkItem(_ =>
										{
											bag.Add(Thread.CurrentThread.ManagedThreadId);
											syncEvent11.Set();
											syncEvent.Wait();
										});
				pool.QueueUserWorkItem(_ =>
										{
											bag.Add(Thread.CurrentThread.ManagedThreadId);
											syncEvent12.Set();
											syncEvent.Wait();
										});
				Assert.True(syncEvent11.Wait(1000)); //wait for data
				Assert.True(syncEvent12.Wait(1000)); //wait for data
				Assert.True(bag.Count == bag.Distinct().Count()); //run on two different threads
				pool.QueueUserWorkItem(_ =>
										{
											bag.Add(Thread.CurrentThread.ManagedThreadId);
											syncEvent2.Set();
										});
				Assert.False(syncEvent2.Wait(1000)); //this won't run until the other two are executing
				syncEvent.Set();
				Assert.True(syncEvent2.Wait(100));
				Assert.Equal(2, bag.Distinct().Count()); //run on two different threads
			}
		}

		[Fact]
		public void DisposeTwoThreadsInPool()
		{
			using (var pool = new FixedThreadPool(2))
			using (var syncEvent = new ManualResetEventSlim())
			using (var syncEvent11 = new ManualResetEventSlim())
			using (var syncEvent12 = new ManualResetEventSlim())
			using (var syncEvent2 = new ManualResetEventSlim())
			using (var syncEvent3 = new ManualResetEventSlim())
			{
				var bag = new ConcurrentBag<int>();
				pool.QueueUserWorkItem(_ =>
				{
					bag.Add(Thread.CurrentThread.ManagedThreadId);
					syncEvent11.Set();
					syncEvent.Wait();
				});
				pool.QueueUserWorkItem(_ =>
				{
					bag.Add(Thread.CurrentThread.ManagedThreadId);
					syncEvent12.Set();
					syncEvent.Wait();
				});

				Assert.True(syncEvent11.Wait(1000)); //wait for data
				Assert.True(syncEvent12.Wait(1000)); //wait for data

				//pool is blocked these won't run

				pool.QueueUserWorkItem(_ =>
				{
					bag.Add(Thread.CurrentThread.ManagedThreadId);
				});
				pool.QueueUserWorkItem(_ =>
				{
					bag.Add(Thread.CurrentThread.ManagedThreadId);
				});

				ThreadPool.QueueUserWorkItem(_ =>
				{
					syncEvent2.Set();
					pool.Dispose(); //this will block
					syncEvent3.Set();
				});
				syncEvent2.Wait(); //dispose is starting...
				Thread.Sleep(200); //dispose is starting... (we hope, this is not watertight)
				syncEvent.Set(); //release blocked pool threads
				syncEvent3.Wait(); //dispose returned

				Assert.Equal(2, bag.Count);

				Assert.Throws<ObjectDisposedException>(() => pool.QueueUserWorkItem(_ =>
				{
					bag.Add(Thread.CurrentThread.ManagedThreadId);
				}));

			}
		}
	}
}
