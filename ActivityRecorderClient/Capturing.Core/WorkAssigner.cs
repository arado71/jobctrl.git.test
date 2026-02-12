using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Rules;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Class for calling the server to create permanent server-side tasks instead of local-only temporary dynamic tasks.
	/// </summary>
	/// <remarks>
	/// Implementation is based on async worker and a thread-safe queue. Quick and dirty thread-safe class for sending AssignWorkByKey requests to the server.
	/// </remarks>
	public class WorkAssigner : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Event raised when a task based on <see cref="AssignData" /> is successfully created.
		/// </summary>
		/// <remarks>Event raised on a background worker thread.</remarks>
		public event EventHandler<SingleValueEventArgs<AssignData>> DataAssigned;
		/// <summary>
		/// Event raised when a task based on <see cref="AssignData"/> can't *ever* be created.
		/// </summary>
		/// /// <remarks>Event raised on a background worker thread.</remarks>
		public event EventHandler<SingleValueEventArgs<AssignData>> DataRejected;

		private readonly object thisLock = new object();
		private readonly Queue<AssignData> pendingKeys = new Queue<AssignData>();
		private bool isProcessing;
		private bool isDisposed;

		/// <summary>
		/// Registers an <see cref="AssignData" />, from which a new server-side task will be created.
		/// </summary>
		/// <param name="key">An <see cref="AssignData"/>, which serves as the base of the permanent task.</param>
		public void AssignWorkAsync(AssignData key)
		{
			if (key == null) return;
			Debug.Assert((key.Work == null && key.Project != null && key.Composite == null)
				|| (key.Work != null && key.Project == null && key.Composite == null)
				|| (key.Work == null && key.Project == null && key.Composite != null));
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

		// ReSharper disable AccessToModifiedClosure
		private void ProcessPendingKeys()
		{
			AssignData currentKey = null;
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
					var result = false;
					if (currentKey.Work != null)
					{
						Debug.Assert(currentKey.Project == null);
						result = ActivityRecorderClientWrapper.Execute(n => n.AssignWorkByKey(ConfigManager.UserId, currentKey.Work));
						log.Info("Assigned work key " + currentKey.Work + " result: " + result);
					}
					else if (currentKey.Project != null)
					{
						result = ActivityRecorderClientWrapper.Execute(n => n.AssignProjectByKey(ConfigManager.UserId, currentKey.Project));
						log.Info("Assigned project key " + currentKey.Project + " result: " + result);
					}
					else if (currentKey.Composite != null)
					{
						result = ActivityRecorderClientWrapper.Execute(n => n.AssignProjectAndWorkByKey(ConfigManager.UserId, currentKey.Composite));
						log.Info("Assigned composite key " + currentKey.Composite + " result: " + result);
					}
					else
					{
						log.ErrorAndFail("AssignData invalid " + currentKey);
					}
					if (result)
					{
						RaiseDataAssigned(currentKey);
					}
					else
					{
						RaiseDataRejected(currentKey);
					}
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("assign by key " + currentKey, log, ex);
				lock (thisLock)
				{
					Debug.Assert(isProcessing);
					if (!isProcessing) return; //make it watertight if there would be an exception in the finally block
					if (currentKey != null)
					{
						pendingKeys.Enqueue(currentKey);
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
		// ReSharper restore AccessToModifiedClosure

		private void RaiseDataAssigned(AssignData e)
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

		private void RaiseDataRejected(AssignData e)
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
