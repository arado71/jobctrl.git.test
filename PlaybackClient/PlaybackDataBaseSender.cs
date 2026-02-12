using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Communication;
using log4net;

namespace PlaybackClient
{
	/// <summary>
	/// Thread-safe class for sending PlaybackDataItems (at the right time) to the server.
	/// </summary>
	public abstract class PlaybackDataBaseSender<T> : Tct.ActivityRecorderService.PeriodicManager, IPlaybackDataSender, IDisposable where T : class, ICommunicationObject
	{
		private readonly ILog log;

		private int numSendingThreads;
		private readonly object thisLock = new object();
		private T client;
		private readonly PriorityQueue<PlaybackDataItem> scheduledData = new PriorityQueue<PlaybackDataItem>();
		private bool isDisposed;

		public PlaybackDataBaseSender(ILog log)
			: base(log)
		{
			this.log = log;
			ManagerCallbackInterval = 1000;
			Start();
		}

		protected abstract T GetWcfClient();
		protected abstract bool CanHandleItem(PlaybackDataItem item);
		protected abstract IAsyncResult BeginSendItem(AsyncCallback callback, AsyncData data);
		protected abstract void EndSendItem(IAsyncResult ar);

		public void SendAsync(List<PlaybackDataItem> items)
		{
			var sw = Stopwatch.StartNew();
			lock (thisLock)
			{
				foreach (var item in items)
				{
					if (!CanHandleItem(item)) continue;
					scheduledData.Enqueue(item);
				}
			}
			System.Threading.ThreadPool.QueueUserWorkItem(_ => SendDataIfApplicable(), null); //GetClient(); is synchronous and can block
			log.DebugFormat("SendAsync returned in {0:0.000}ms", sw.Elapsed.TotalMilliseconds);
		}

		protected override void ManagerCallbackImpl()
		{
			SendDataIfApplicable();
			if (log.IsDebugEnabled)
			{
				lock (thisLock)
				{
					DateTime? next = null;
					if (scheduledData.Count > 0)
					{
						next = scheduledData.Peek().ScheduledTime;
					}
					log.Debug("Next: " + (next == null ? "" : (next.Value - DateTimeEx.UtcNow()).ToHourMinuteSecondString()) + " Count: " + scheduledData.Count + " Senders " + numSendingThreads);
				}
			}
		}

		private void SendDataIfApplicable()
		{
			var data = GetDataToSend(true);
			if (data == null) return;
			SendDataLoop(data);
		}

		//it is not trivial how to avoid stack dive so that is why this is a bit complicated (based on WorkStatusReporter)
		private void SendDataLoop(PlaybackDataItem firstData = null)
		{
			var data = firstData ?? GetDataToSend();
			while (data != null)
			{
				T currClient = null;
				try
				{
					currClient = GetClient();
					if (currClient == null) return; //disposed
					var aState = new AsyncData(currClient, data);
					if (IterateToMultipliedUsers(aState)) return; //callback will continue the SendDataLoop
				}
				catch (Exception ex)
				{
					if (ex is CommunicationException && !(ex is FaultException))
					{
						log.Warn("Error while sending data", ex);
						//we can receive some exceptions when calling BeginXXX (e.g. ProtocolException, CommunicationObjectAbortedException, CommunicationObjectFaultedException)
						//but state will remain Opened and Faulted is not raised, so we have to create a new client.
						//As I see unrecoverable errors from EndXXX puts the client in Faulted state (but I am not sure).
						ResetClient(currClient);
					}
					else if (ex is ObjectDisposedException) //race between GetClient and BeginAddWorkItemEx
					{
						log.Info("Error while sending data", ex);
					}
					else
					{
						log.Error("Error while sending data", ex);
					}
					EnqueueRetryItem(data);
				}
				data = GetDataToSend();
			}
		}

		private bool IterateToMultipliedUsers(AsyncData first)
		{
			if (isDisposed) return true;
			for (var aState = first; aState.HasNext; aState = aState.Next())
			{
				try
				{
					aState.Data.PrepareForSend(); //e.g. loading screenshots from disk
				}
				catch (Exception ex)
				{
					log.ErrorAndFail("PrepareForSend failed" + aState.Data, ex);
				}
				if (isDisposed) return true;
				var ar = BeginSendItem(EndSendItemCallback, aState);
				if (ar != null) //some error occured, ignore item
				{
					if (!ar.CompletedSynchronously) return true;
					EndSendItem(ar);
				}
			}
			return false;
		}

		private void EndSendItemCallback(IAsyncResult ar)
		{
			if (ar.CompletedSynchronously) return;
			EndSendItem(ar);
			var next = (ar.AsyncState as AsyncData)?.Next();
			if (next != null && IterateToMultipliedUsers(next)) return; //callback will continue the SendDataLoop
			SendDataLoop();
		}

		protected void EnqueueRetryItem(PlaybackDataItem data)
		{
			var newItem = data.GetRetryData(TimeSpan.FromSeconds((data.RetryCount + 1) * ConfigManager.SendBaseRetryIntervalInSec), ConfigManager.SendRetries);
			if (newItem == null)
			{
				log.Error("No longer retrying sending data " + data);
				return;
			}
			lock (thisLock)
			{
				scheduledData.Enqueue(newItem);
			}
		}

		private PlaybackDataItem GetDataToSend(bool startingLoop = false)
		{
			lock (thisLock)
			{
				if (startingLoop && numSendingThreads >= ConfigManager.ParallelSenders) return null; //limit max sender threads
				if (!isDisposed
					&& scheduledData.Count > 0
					&& scheduledData.Peek().ScheduledTime <= DateTimeEx.UtcNow())
				{
					if (startingLoop) numSendingThreads++;
					return scheduledData.Dequeue();
				}
				if (!startingLoop) numSendingThreads--;
			}
			return null;
		}

		private void ResetClient(T currClient)
		{
			if (currClient == null) return;
			lock (thisLock)
			{
				if (client != currClient) return;
				WcfClientDisposeHelper.Dispose(client); //todo Dispose on bg thread ?
				client = null;
			}
			log.Info("Client closed (reset)");
		}

		private T GetClient()
		{
			T curr;
			lock (thisLock)
			{
				if (client != null && client.State == CommunicationState.Faulted)
				{
					WcfClientDisposeHelper.Dispose(client); //todo Dispose on bg thread ?
					client = null;
					log.Info("Client closed (faulted)");
				}
				if (isDisposed) return null;
				if (client == null)
				{
					curr = GetWcfClient();
					curr.Open();
					//Faulted += must be called after open
					//curr.Faulted += (s, ea) => log.Info("Client faulted");
					log.Info("Client created");
					client = curr;
				}
				else
				{
					curr = client;
				}
			}
			return curr;
		}

		public void Dispose()
		{
			lock (thisLock)
			{
				isDisposed = true;
				if (client != null) WcfClientDisposeHelper.Dispose(client);
				client = null;
			}
			Stop();
		}

		//immutable class (thread-safe)
		protected class AsyncData
		{
 			public readonly T Client;
			public readonly PlaybackDataItem Data;
			private readonly int count;

			public AsyncData(T client, PlaybackDataItem data)
			{
				Client = client;
				Data = data;
				count = ConfigManager.UserMultiplierLimit > 0 && data.RetryCount == 0 ? ConfigManager.UserMultiplierLimit : 1;
			}

			private AsyncData(T client, PlaybackDataItem data, int count)
			{
				Client = client;
				Data = data;
				this.count = count;
			}

			public bool HasNext => count > MultiplierCalculator.ComputedThreshold;

			public AsyncData Next()
			{
				if (count <= MultiplierCalculator.ComputedThreshold + 1) return null;
				var next = new AsyncData(Client, new PlaybackDataItem(Data), count - 1);
				if (next.Data.WorkItem != null) next.Data.WorkItem.UserId += PlaybackSchedulerFactory.UserCount;
				if (next.Data.ManualWorkItem != null) next.Data.ManualWorkItem.UserId += PlaybackSchedulerFactory.UserCount;
				if (next.Data.MobileRequest != null) next.Data.MobileRequest.UserId += PlaybackSchedulerFactory.UserCount;
				MultiplierCalculator.Add(next.Data.WorkItem?.UserId ?? next.Data.ManualWorkItem?.UserId ?? next.Data.MobileRequest.UserId);
				return next;
			}

		}
	}

	internal static class MultiplierCalculator
	{
		public static void Add(int userId)
		{
			lastUsers.AddOrUpdate(userId, DateTime.UtcNow, (i, time) => DateTime.UtcNow);
		}

		private static readonly ConcurrentDictionary<int, DateTime> lastUsers = new ConcurrentDictionary<int, DateTime>();

		public static int ComputedThreshold { get; private set; } = ConfigManager.UserMultiplierLimit > 0 ? ConfigManager.UserMultiplierLimit - 22 : 0;

		private static readonly Timer multiplierComputerTimer = new Timer(multiplierComputerTimerCallback, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(0.5));

		private static void multiplierComputerTimerCallback(object state)
		{
			var tres = DateTime.UtcNow - TimeSpan.FromSeconds(65);
			var lastUsersCount = lastUsers.Count(l => l.Value > tres);
			if (lastUsersCount == 0) return;
			var newmult = ConfigManager.UserTargetCount * (ConfigManager.UserMultiplierLimit - ComputedThreshold) / lastUsersCount;
			if (lastUsersCount > 0 && lastUsersCount < ConfigManager.UserTargetCount && ComputedThreshold > 0)
			{
				var value = ComputedThreshold - (newmult - ConfigManager.UserMultiplierLimit + ComputedThreshold) / 2;
				ComputedThreshold = value > 0 ? value : 0;
			}
			else if (lastUsersCount > ConfigManager.UserTargetCount && ComputedThreshold < ConfigManager.UserMultiplierLimit)
			{
				var value = ComputedThreshold + (ConfigManager.UserMultiplierLimit - ComputedThreshold - newmult) / 2;
				ComputedThreshold = value < ConfigManager.UserMultiplierLimit ? value : ConfigManager.UserMultiplierLimit - 1;
			}
			LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Debug($"lastUsersCount:{lastUsersCount} multiplier:{ConfigManager.UserMultiplierLimit - ComputedThreshold}");
		}
	}
}
