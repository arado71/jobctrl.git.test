using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Quick and dirty thread-safe class for sending assign workId requests to the server. (until communication is rewritten)
	/// </summary>
	/// <remarks>
	/// Copy/paste from WorkAssigner - we won't create new subtype in AssignData for workId assignment atm.
	/// Because we might not have rights to assign work to us... (and for AssignData we expect that the assignment will usually succeed)
	/// </remarks>
	public class WorkIdAssigner : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public event EventHandler<SingleValueEventArgs<int>> DataAssigned;
		public event EventHandler<SingleValueEventArgs<int>> DataRejected;

		private readonly object thisLock = new object();
		private readonly Queue<int> pendingKeys = new Queue<int>();
		private bool isProcessing;
		private bool isDisposed;

		public void AssignWorkIdAsync(int key)
		{
			bool shouldStart;
			lock (thisLock)
			{
				if (pendingKeys.Contains(key)) return; //skip already sent keys (this is new to WorkAssigner but we don't want to create a wrapper just for this, also we don't expect to have more than just a few pendingKeys)
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
			int? currentKey = null;
			try
			{
				while (true)
				{
					currentKey = null;
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
					var res = ActivityRecorderClientWrapper.Execute(n => n.AssignTask(ConfigManager.UserId, currentKey.Value));
					log.Info("Assign work by id " + currentKey + " result was " + res);
					if (res == AssignTaskResult.Ok)
					{
						RaiseDataAssigned(currentKey.Value);
					}
					else
					{
						RaiseDataRejected(currentKey.Value);
					}
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("Assign work by id " + currentKey, log, ex);
				lock (thisLock)
				{
					Debug.Assert(isProcessing);
					if (!isProcessing) return; //make it watertight if there would be an exception in the finally block (Dispose)
					if (currentKey != null)
					{
						pendingKeys.Enqueue(currentKey.Value);
					}
				}
				//retry after a dealy
				var timer = new Timer(self =>
				{
					((Timer)self).Dispose();
					ProcessPendingKeys();
				});
				timer.Change(3000, Timeout.Infinite);
			}
		}

		private void RaiseDataAssigned(int e)
		{
			var handler = DataAssigned;
			if (handler == null) return;
			try
			{
				handler(this, SingleValueEventArgs.Create(e));
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpcted error in RaiseDataRejected", ex);
			}
		}

		private void RaiseDataRejected(int e)
		{
			var handler = DataRejected;
			if (handler == null) return;
			try
			{
				handler(this, SingleValueEventArgs.Create(e));
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpcted error in RaiseDataRejected", ex);
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
