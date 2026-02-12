using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Class for executing a given action asynchronously on a thread pool thread. Only at most one item is processed at a time using a FIFO queue.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	//todo extend this to be able to use with WorkAssigner?
	public class AsyncWorkQueue<T> : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Queue<T> pendingKeys = new Queue<T>();
		private readonly object thisLock = new object();
		private readonly Action<T> action;
		private readonly Action<T, Exception> onError;
		private bool isProcessing;
		private bool isDisposed;

		public AsyncWorkQueue(Action<T> action, Action<T, Exception> onError = null)
		{
			if (action == null) throw new ArgumentNullException("action");
			this.action = action;
			if (onError == null) onError = (key, ex) => log.ErrorAndFail("Unexpected error during execution", ex);
			this.onError = onError;
		}

		public void EnqueueAsync(T key)
		{
			bool shouldStart;
			lock (thisLock)
			{
				if (isDisposed) return;
				shouldStart = pendingKeys.Count == 0 && !isProcessing;
				pendingKeys.Enqueue(key);
				if (shouldStart) isProcessing = true;
			}
			if (shouldStart)
			{
				ThreadPool.QueueUserWorkItem(_ => ProcessPendingKeys());
			}

		}

		private void ProcessPendingKeys()
		{
			while (true)
			{
				T currentKey;
				lock (thisLock)
				{
					Debug.Assert(isProcessing);
					if (isDisposed)
					{
						pendingKeys.Clear(); //drop all pending
						Monitor.Pulse(thisLock); //signal waiter on dispose
					}
					if (pendingKeys.Count == 0)
					{
						isProcessing = false;
						return; //there shouldn't be any exceptions after return
					}
					currentKey = pendingKeys.Dequeue();
				}
				try
				{
					action(currentKey);
				}
				catch (Exception ex)
				{
					onError(currentKey, ex);
				}
			}
		}

		public void Dispose()
		{
			lock (thisLock)
			{
				if (isDisposed) return;
				isDisposed = true;
				if (isProcessing)
				{
					Monitor.Wait(thisLock);
				}
			}
		}
	}
}
