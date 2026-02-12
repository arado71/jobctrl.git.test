using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderClient.Communication
{
	/// <summary>
	/// Class for coordinating access to WCF proxies.
	/// Its goal is to have only one shared proxy. Only create additional proxies when one cannot be acquired in time.
	/// </summary>
	/// <remarks>
	/// We musn't call release inside the lock to avoid potential deadlock.
	/// (Release will acquire wcf channel lock and the same lock is held while the callback of BeginXXX is executing (even after EndXXX is called))
	/// </remarks>
	/// <typeparam name="T">WCF proxy type</typeparam>
	public sealed class BlockingClientPool<T> : IDisposable
	{
		// ReSharper disable StaticFieldInGenericType - we only use this with one closed type
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int timeoutDefault = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;
		private static readonly int timeoutRandomPart = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
		// ReSharper restore StaticFieldInGenericType

		private readonly object thisLock = new object();
		private readonly Random rnd = new Random();
		private readonly Queue<T> queue = new Queue<T>(); //client pool (queue.Count + outstandingItems.Count should be 1 unless there are long running blocking calls)
		private readonly List<T> outstandingItems = new List<T>(); //items which should be returned to the pool
		private readonly Queue<T> itemsToCleanup = new Queue<T>(); //items which should be cleaned up (i.e. call release() on them)
		private readonly Func<T> factory;
		private readonly Func<T, bool> isValid;
		private readonly Action<T> release;
		private int waiters;
		private bool isDisposed;

		public BlockingClientPool(Func<T> factory, Func<T, bool> isValid, Action<T> release)
		{
			this.factory = factory;
			this.isValid = isValid;
			this.release = release;
		}

		public T Get()
		{
			return Get(0);
		}

		public T Get(int timeout)
		{
			var cleanupNeeded = false;
			try
			{
				lock (thisLock)
				{
					var start = Environment.TickCount;
					var limit = timeout <= 0
						? timeoutDefault + rnd.Next(timeoutRandomPart + 1) //avoid bursts
						: timeout;
					T res;
					while (true)
					{
						if (isDisposed)
						{
							throw new ObjectDisposedException("BlockingClientPool");
						}
						if (queue.Count == 0)
						{
							var wait = limit - (Environment.TickCount - start);
							if (wait < 0 || outstandingItems.Count == 0) //no more time to wait, or there are no outstanding items
							{
								res = factory();
								Debug.Assert(isValid(res), "invalid item from factory");
								break;
							}
							waiters++;
							Monitor.Wait(thisLock, wait);
							waiters--;
						}
						else
						{
							res = queue.Dequeue();
							if (isValid(res))
							{
								//KISS our goal is to have only one outstanding item
								while (waiters == 0 && queue.Count > 0)
								{
									cleanupNeeded = true;
									EnqueueItemForCleanup(queue.Dequeue());
								}
								break;
							}
							cleanupNeeded = true;
							EnqueueItemForCleanup(res); //we might have to wait for a while for the actual cleanup
						}
					}
					outstandingItems.Add(res);
					log.VerboseFormat("Get Ready: {0} Out: {1} Waiters: {2}", queue.Count, outstandingItems.Count, waiters);
					return res;
				}
			}
			finally
			{
				if (cleanupNeeded) CleanupPendingItems();
			}
		}

		public void Release(T item)
		{
			var releaseItem = true;
			lock (thisLock)
			{
				var res = outstandingItems.Remove(item);
				Debug.Assert(res /*|| isDisposed*/, "released item not in outstandingItems");
				if (!isDisposed) //put back to pool
				{
					queue.Enqueue(item);
					releaseItem = false;
					Monitor.Pulse(thisLock);
				}
				else if (waiters != 0 && outstandingItems.Count == 0)
				{
					//if there are waiters and item is not valid anymore and outstandingItems.Count != 0 then we could create and Enqueue new item, but KISS atm.
					Monitor.Pulse(thisLock);
				}
				log.VerboseFormat("Release Ready: {0} Out: {1} Waiters: {2}", queue.Count, outstandingItems.Count, waiters);
			}
			if (releaseItem) release(item);
		}

		public void Dispose()
		{
			lock (thisLock)
			{
				isDisposed = true;
				log.VerboseFormat("Dispose Ready: {0} Out: {1} Waiters: {2}", queue.Count, outstandingItems.Count, waiters);
				while (queue.Count > 0)
				{
					EnqueueItemForCleanup(queue.Dequeue());
				}
				//don't cancel outstanding connections atm. (backward compatibility)
				//foreach (var outstandingItem in outstandingItems)
				//{
				//	EnqueueItemForCleanup(outstandingItem);
				//}
				//outstandingItems.Clear();
				Monitor.PulseAll(thisLock);
			}
			CleanupPendingItems();
			log.Debug("Disposed");
		}

		private void EnqueueItemForCleanup(T item)
		{
			lock (itemsToCleanup)
			{
				itemsToCleanup.Enqueue(item);
			}
		}

		private void CleanupPendingItems()
		{
			while (true)
			{
				T item;
				lock (itemsToCleanup)
				{
					if (itemsToCleanup.Count == 0) return;
					item = itemsToCleanup.Dequeue();
				}
				release(item);
				log.Verbose("Cleaned up item");
			}
		}
	}
}
