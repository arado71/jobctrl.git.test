using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderService
{
	/// <summary>
	/// Static threadpool with a fixed number of threads
	/// </summary>
	public class FixedThreadPool : IDisposable
	{
		private readonly Thread[] poolThreads;
		private readonly BlockingCollection<UserWorkItem> workQueue = new BlockingCollection<UserWorkItem>();
		private readonly CancellationTokenSource disposeSource = new CancellationTokenSource();

		public FixedThreadPool(int numThreads)
		{
			if (numThreads < 1) throw new ArgumentOutOfRangeException("numThreads");
			poolThreads = new Thread[numThreads];
			for (int i = 0; i < poolThreads.Length; i++)
			{
				poolThreads[i] = new Thread(ThreadLoop)
				{
					Name = "FP" + i,
					IsBackground = true, //don't prevent service stoppage
				};
				poolThreads[i].Start();
			}
		}

		public void QueueUserWorkItem(WaitCallback callback)
		{
			QueueUserWorkItem(callback, null);
		}

		public void QueueUserWorkItem(WaitCallback callback, object state)
		{
			if (disposeSource.IsCancellationRequested) throw new ObjectDisposedException(GetType().Name);
			if (callback == null) throw new ArgumentNullException("callback");
			var userWorkItem = new UserWorkItem(callback, state);
			workQueue.Add(userWorkItem);
		}

		private void ThreadLoop()
		{
			while (!disposeSource.IsCancellationRequested)
			{
				try
				{
					var currItem = workQueue.Take(disposeSource.Token);
					currItem.callback(currItem.state);
				}
				catch (OperationCanceledException)
				{
				}
			}
		}

		public void Dispose()
		{
			if (disposeSource.IsCancellationRequested) return;
			disposeSource.Cancel();
			foreach (var thread in poolThreads)
			{
				thread.Join();
			}
			disposeSource.Dispose();
			workQueue.Dispose();
		}

		private struct UserWorkItem
		{
			internal readonly WaitCallback callback;
			internal readonly object state;

			internal UserWorkItem(WaitCallback callback, object state)
			{
				this.callback = callback;
				this.state = state;
			}
		}
	}
}
