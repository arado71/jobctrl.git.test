using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Meeting;
using Tct.ActivityRecorderClient.Stats;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	//todo IDisposable
	/// <summary>
	/// Persists workItems on the client and sends them to the service
	/// </summary>
	public class WorkItemManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly MenuCoordinator menuCoordinator;
		private readonly StatsCoordinator statsCoordinator;
		private readonly Queue<IUploadItem> itemsToSend = new Queue<IUploadItem>();
		private readonly Queue<IUploadItem> itemsInErrorAndNotPersisted = new Queue<IUploadItem>();
		private readonly Queue<string> itemsToLoad = new Queue<string>();
		private readonly object thisLock = new object();
		private readonly Thread senderThread;
		private readonly ManualResetEvent mreCanSend = new ManualResetEvent(false);
		private readonly ManualResetEvent mreWait = new ManualResetEvent(false);
		private volatile bool stopped;
		private volatile bool paused;
		private volatile bool sendingBlocked;
		private int itemsSendingCount; //ugly hax (with lot of races) but better than nothing

		private OfflineReasonEnum offlineReason = OfflineReasonEnum.NotOffline;

		public OfflineReasonEnum OfflineReason
		{
			get { return offlineReason; }
			set { offlineReason = value; }
		}
		public enum OfflineReasonEnum
		{
			NotOffline,
			Timeout,
			EndpointNotFound,
			WorkItemUploadError
		}

		public bool Paused
		{
			get { return paused; }
			set
			{
				paused = value && CanPause;
				log.Info("Sending is " + (paused ? "Paused" : "Resumed"));
			}
		}
		public bool CanPause
		{
			get
			{
				return ConfigManager.MaxOfflineWorkItems != 0
					&& UnsentItemsCount < ConfigManager.MaxOfflineWorkItems;
			}
		}

		public int UnsentItemsCount //this is still not accurate or race free but its good enough...
		{
			get
			{
				lock (thisLock)
				{
					return itemsToSend.Count + itemsToLoad.Count + itemsInErrorAndNotPersisted.Count + Interlocked.CompareExchange(ref itemsSendingCount, 0, 0);
				}
			}
		}

		private bool isOnline = true; //the defaut icon is green so we assume the client is online
		public bool IsOnline
		{
			get { return isOnline; }
			private set
			{
				if (isOnline == value) return;
				isOnline = value;
				log.Info("Service is " + (isOnline ? "Online" : "Offline"));
				RaiseConnectionStatusChanged();
			}
		}

		public bool SendingBlocked
		{
			get { return sendingBlocked; }
			set
			{
				sendingBlocked = value;
				log.Info("Sending is " + (value ? "" : "un") + "blocked");
				if (!sendingBlocked)
					IsOnline = true; // it's not true surely, but it will be cleared up after next sending
			}
		}

		private int ProxyTimeout
		{
			get
			{
				return UnsentItemsCount > 30
					? (int)TimeSpan.FromSeconds(5).TotalMilliseconds //don't starve WorkItemManager (not ideal but enough atm.)
					: ActivityRecorderClientWrapper.DefaultTimeout;
			}
		}

		public event EventHandler<EventArgs> ConnectionStatusChanged;
		public event EventHandler<EventArgs> CannotPersistAndSendWorkItem;
		public event EventHandler<SingleValueEventArgs<IUploadItem>> ItemSentSuccessfully;

		public WorkItemManager(MenuCoordinator menuCoordinator, StatsCoordinator statsCoordinator)
		{
			this.menuCoordinator = menuCoordinator;
			this.statsCoordinator = statsCoordinator;
			SendingBlocked = true; // sending blocked while credit runout state isn't determined
			senderThread = new Thread(SendItemsCycle) { IsBackground = true, Name = "WS" };
		}

		private const int itemsToSendUpperBound = 100;

		private void SendItemsCycle()
		{
			while (!stopped)
			{
				try
				{
					SendItemsImpl();
				}
				catch (Exception ex)
				{
					log.ErrorAndFail("Unexpected exception in SendItemsImpl: " + ex);
				}
			}
			//RaiseConnectionStatusChanged(); //force deadlock when Gui thread is using Thread.Join() and RaiseConnectionStatusChanged is waiting for the gui (Invoke)
		}

		private void SendItemsImpl()
		{
			if (Paused || SendingBlocked)
			{
				if (CanPause || SendingBlocked)
				{
					IsOnline = false;
					WaitFor(1000, false);
					return; //do not even try to connect 
				}
				else
				{
					Paused = false;
				}
			}
			var itemToSend = DequeueItem();
			if (itemToSend == null)
			{
				mreCanSend.WaitOne();
				return;
			}
			var sendSuccess = false;
			var isPersistenceAndRetryNeeded = IsPersistenceAndRetryNeeded(itemToSend);
			try
			{
				while (itemToSend != null)
				{
					sendSuccess = Send(itemToSend);
					if (!sendSuccess) break;
					bool deleteSuccess = !isPersistenceAndRetryNeeded || WorkItemSerializationHelper.Delete(itemToSend);
					if (!deleteSuccess)
					{
						log.Error("Unable to delete workItem id: " + itemToSend.Id);
					}
					//look for next item to send if we are not stopping or paused
					itemToSend = (stopped || Paused) ? null : DequeueItem();
					isPersistenceAndRetryNeeded = IsPersistenceAndRetryNeeded(itemToSend);
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error when sending workItem " + (itemToSend == null ? "NULL" : "id: " + itemToSend.Id), ex);
			}
			if (!sendSuccess)
			{
				if (!stopped) WaitFor(3000, true); //retry delay
				if (isPersistenceAndRetryNeeded)
				{
					EnqueueRetryItem(itemToSend);
				}
				else
				{
					log.Info("Discarding item " + itemToSend.Id);
				}
			}
		}

		private bool WaitFor(int timeout, bool icrementSendingCount)
		{
			try
			{
				if (icrementSendingCount) Interlocked.Increment(ref itemsSendingCount); //we have an item 'floatig' around
				return mreWait.WaitOne(timeout, false); //use this overload to avoid System.MissingMethodException for "Boolean System.Threading.WaitHandle.WaitOne(Int32)" on comps without 3.5 sp1
			}
			catch (Exception ex) //well we don't expect this anymore but just in case...
			{
				log.ErrorAndFail("Unexpected error in WaitFor", ex);
				Thread.Sleep(timeout); //fallback wait
				return false;
			}
			finally
			{
				if (icrementSendingCount) Interlocked.Decrement(ref itemsSendingCount);
			}
		}

		private void EnqueueRetryItem(IUploadItem item)
		{
			//we don't know if the server will accept this item for the next time so we will put it to the end of the queue.
			//If we put back into itemsToSend and it cannot be processed by the server than we would end up not sending any
			//new items from the itemsToLoad queue.
			//But the item might not be persisted... in that case we use a special container
			string path;
			bool isPersisted = WorkItemSerializationHelper.IsSuccessfullyPersisted(item, out path);
			if (!isPersisted) //try to persist it again
			{
				WorkItemSerializationHelper.Delete(item);
				WorkItemSerializationHelper.Save(item);
				isPersisted = WorkItemSerializationHelper.IsSuccessfullyPersisted(item, out path);
				if (isPersisted) log.Info("Successfully persisted item id: " + item.Id);
			}
			lock (thisLock)
			{
				bool allQueuesAreEmpty = AllQueuesAreEmpty();
				if (isPersisted)
				{
					itemsToLoad.Enqueue(path);
				}
				else
				{
					itemsInErrorAndNotPersisted.Enqueue(item);
				}
				if (allQueuesAreEmpty)
				{
					mreCanSend.Set();
				}
			}
		}

		private void EnqueueItem(IUploadItem item, bool persistSuccess)
		{
			if (item == null) return;
			bool storeInMemory;
			lock (thisLock)
			{
				if (persistSuccess && itemsToSend.Count > itemsToSendUpperBound)
				{
					storeInMemory = false;
				}
				else
				{
					storeInMemory = true;
					bool allQueuesAreEmpty = AllQueuesAreEmpty();
					itemsToSend.Enqueue(item);
					if (allQueuesAreEmpty)
					{
						mreCanSend.Set();
					}
				}
			}
			if (storeInMemory) return;
			string path;
			if (!WorkItemSerializationHelper.IsSuccessfullyPersisted(item, out path)) //double check
			{
				lock (thisLock)
				{
					bool allQueuesAreEmpty = AllQueuesAreEmpty();
					itemsToSend.Enqueue(item);
					if (allQueuesAreEmpty)
					{
						mreCanSend.Set();
					}
				}
				log.Error("WorkItem is not presisted successfully, so we have to store it in memory id: " + item.Id);
				return;
			}
			log.Debug("Unloading workItem from memory " + path);
			lock (thisLock)
			{
				bool allQueuesAreEmpty = AllQueuesAreEmpty();
				itemsToLoad.Enqueue(path);
				if (allQueuesAreEmpty)
				{
					mreCanSend.Set();
				}
			}
		}

		private int errorItemsCounter;
		public bool OfflineBecauseTimeout;
		private IUploadItem DequeueItem()
		{
			string loadFromPath;
			lock (thisLock)
			{
				if (itemsToSend.Count != 0)
				{
					return itemsToSend.Dequeue();
				}
				else if (itemsToLoad.Count != 0) //if we have an erroneous item this will never be 0
				{
					if (itemsInErrorAndNotPersisted.Count != 0 && ++errorItemsCounter == 10)
					{
						//every 10th send will try to reduce itemsInErrorAndNotPersisted to avoid starving 
						//if we have some erroneous item in itemsToLoad.
						errorItemsCounter = 0;
						return itemsInErrorAndNotPersisted.Dequeue();
					}
					loadFromPath = itemsToLoad.Dequeue();
				}
				else if (itemsInErrorAndNotPersisted.Count != 0) //if we have an erroneous item which cannot be persisted this will never be 0
				{
					return itemsInErrorAndNotPersisted.Dequeue();
				}
				else //if all queues are empty then stop
				{
					Debug.Assert(AllQueuesAreEmpty());
					if (!stopped) mreCanSend.Reset();
					return null;
				}
			}
			var workItem = WorkItemSerializationHelper.LoadWorkItem(loadFromPath);
			if (workItem == null)
			{
				log.Error("Unable to load WorkItem from " + loadFromPath);
			}
			else
			{
				log.Debug("Loaded workItem into memory from " + loadFromPath);
			}
			return workItem;
		}

		private bool AllQueuesAreEmpty()
		{
			return (itemsToSend.Count == 0 && itemsToLoad.Count == 0 && itemsInErrorAndNotPersisted.Count == 0);
		}

		//hax to avoid that ongoing items can be sent after the final deletion is sent... causing remaining ongoing items
		//I don't like this spaghetti solution as WIM shouldn't know any impl detail of erroneous ManualMeetingItems, but I cannot figure out easier fix atm.
		//this is still not water-tight but better than nothing...
		private static bool IsPersistenceAndRetryNeeded(IUploadItem item)
		{
			var meeting = item as ManualMeetingItem;
			return meeting == null || meeting.ManualMeetingData == null || !meeting.ManualMeetingData.OnGoing;
		}

		public bool SendOrPersist(IUploadItem item, TimeSpan? sendTimeout = null)
		{
			Debug.Assert(item != null);
			var timeout = sendTimeout != null ? (int?)sendTimeout.Value.TotalMilliseconds : null;
			log.Debug("Sending immediatly workItem id: " + item.Id);
			if (Send(item, timeout)) return true;
			log.Debug("Failed to immediatly send workItem id: " + item.Id);
			PersistAndSend(item); // fall back to slow method
			return false;
		}

		public void PersistAndSend(IUploadItem item)
		{
			Debug.Assert(item != null);
			var isPersistenceAndRetryNeeded = IsPersistenceAndRetryNeeded(item);
			var persistSuccess = isPersistenceAndRetryNeeded && WorkItemSerializationHelper.Save(item);
			EnsureItemIsKnown(item);
			statsCoordinator.UpdateStats(item as IWorkItem);
			if (persistSuccess || !isPersistenceAndRetryNeeded)
			{
				EnqueueItem(item, persistSuccess);
			}
			else
			{
				log.Error("Unable to persist workItem id: " + item.Id);
				if (Send(item)) //sending on different thread should be ok. (Setting Online/Offline is ok as it is just an indicator)
				{
					log.Info("Successfully sent workItem id: " + item.Id);
					return; //still ok
				}
				else
				{
					EnqueueItem(item, persistSuccess); //let's hope it can be sent later while the app is running
					log.Fatal("Unable to persist and send workItem id: " + item.Id);
					RaiseCannotPersistAndSendWorkItem();
				}
				//maybe some partial file is created which cannot be parsed so delete it if we can
				WorkItemSerializationHelper.Delete(item);
			}
		}

		[Conditional("DEBUG")]
		private static void EnsureItemIsKnown(IUploadItem item)
		{
			if (!IsPersistenceAndRetryNeeded(item)) return;
			string path;
			WorkItemSerializationHelper.IsSuccessfullyPersisted(item, out path);
		}

		private bool Send(IUploadItem item, int? timeout = null)
		{
			Debug.Assert(item != null);
			bool isChanged = false;
			try
			{
				Interlocked.Increment(ref itemsSendingCount);
				if (!menuCoordinator.CanSendItem(item as IWorkItem, out isChanged)) return false;
				if (item is WorkItem)
				{
#if EncodeTransmissionScreen
					Screenshots.ScreenshotEncoderHelper.EncodeAndAddWorkItemEx((WorkItem)item, timeout ?? ProxyTimeout);
#else
                    ActivityRecorderClientWrapper.Execute(n => n.AddWorkItemEx((WorkItem)item), timeout ?? ProxyTimeout);
#endif
					log.Debug("Successfully sent workItem id: " + item.Id);
				}
				else if (item is ManualWorkItem)
				{
					ActivityRecorderClientWrapper.Execute(n => n.AddManualWorkItem((ManualWorkItem)item), timeout ?? ProxyTimeout);
					log.Debug("Successfully sent manualWorkItem id: " + item.Id);
				}
				else if (item is ManualMeetingItem)
				{
					var meeting = item as ManualMeetingItem;
					ActivityRecorderClientWrapper.Execute(n => n.AddManualMeeting(meeting.UserId, meeting.ManualMeetingData, ConfigManager.EnvironmentInfo.ComputerId), timeout ?? ProxyTimeout);
					log.Debug("Successfully sent manualMeetingItem id: " + item.Id);
				}
				else if (item is ParallelWorkItem)
				{
					ActivityRecorderClientWrapper.Execute(n => n.AddParallelWorkItem((ParallelWorkItem)item), timeout ?? ProxyTimeout);
					log.Debug("Successfully sent parallelWorkItem id: " + item.Id);
				}
				else if (item is CollectedItem)
				{
					Debug.Fail("Should not be used atm (legacy code path), send AggregateCollectedItems instead");
					ActivityRecorderClientWrapper.Execute(n => n.AddCollectedItem((CollectedItem)item), timeout ?? ProxyTimeout);
					log.Debug("Successfully sent collectedItem id: " + item.Id);
				}
				else if (item is AggregateCollectedItems)
				{
					ActivityRecorderClientWrapper.Execute(n => n.AddAggregateCollectedItems((AggregateCollectedItems)item), timeout ?? ProxyTimeout);
					log.Debug("Successfully sent aggregateCollectedItems id: " + item.Id);
				} else if (item is ReasonItem)
				{
					ActivityRecorderClientWrapper.Execute(n => n.AddReasonEx((ReasonItem) item));
					log.Debug("Successfully sent reasonItem id: " + item.Id);
				}
				else if (item is TelemetryItem)
				{
					ActivityRecorderClientWrapper.Execute(n => n.AddTelemetry((TelemetryItem)item), timeout ?? ProxyTimeout);
					log.Debug("Successfully sent stats id: " + item.Id);
				}
				else if (item is AcceptanceDateItem)
				{
					ActivityRecorderClientWrapper.Execute(n => n.SetDppAcceptanceDate(item.UserId, item.StartDate), timeout ?? ProxyTimeout);
					log.Debug("Successfully sent acceptance date id: " + item.Id);
				}
				else
				{
					var errorMsg = "Invalid IUploadData type " + item.GetType();
					log.Error(errorMsg);
					Debug.Fail(errorMsg);
					return false;
				}
				IsOnline = true;
				OfflineReason = OfflineReasonEnum.NotOffline;
				ItemSentSuccessfully?.Invoke(this, SingleValueEventArgs.Create(item));
				return true;
			}
			catch (Exception ex)
			{
				if (isChanged && IsPersistenceAndRetryNeeded(item)) WorkItemSerializationHelper.Save(item); //save changes only on error, otherwise it would be deleted anyway
				IsOnline = false;
				if (ex is TimeoutException)
				{
					OfflineReason = OfflineReasonEnum.Timeout;
				}
				else if (ex is EndpointNotFoundException)
				{
					OfflineReason = OfflineReasonEnum.EndpointNotFound;
				}
				else if (ex is FaultException)
				{
					OfflineReason = OfflineReasonEnum.WorkItemUploadError;
				}
				WcfExceptionLogger.LogWcfError("send workItem id: " + item.Id, log, ex);
				return false;
			}
			finally
			{
				Interlocked.Decrement(ref itemsSendingCount);
			}
		}

		//musn't block for long time (called from GUI thread) and should be started before capture manager
		public void Start()
		{
			log.Info("Starting workItem sender timer");
			senderThread.Start();
			//Loading here is too slow on windows startup so we load on a bg thread
			var previousItems = WorkItemSerializationHelper.GetPersistedWorkItemPaths();

			log.Info("Found " + previousItems.Count + " unsent workItem" + (previousItems.Count == 1 ? "" : "s"));
			if (previousItems.Count == 0) return;
			lock (thisLock)
			{
				foreach (var item in previousItems)
				{
					itemsToLoad.Enqueue(item);
				}
				mreCanSend.Set();
			}
		}

		public void Stop()
		{
			stopped = true;
			lock (thisLock) //avoid race (if Reset is called right after this Set then join will deadlock)
			{
				mreCanSend.Set();
			}
			mreWait.Set(); //stop waiting
			//Stop is called from the GUI thread so RaiseConnectionStatusChanged musn't wait for the GUI to avoid deadlocks.
			senderThread.Join();
			log.Info("Stopped workItem sender timer");
		}

		private void RaiseConnectionStatusChanged()
		{
			EventHandler<EventArgs> changed = ConnectionStatusChanged;
			if (changed == null) return;
			try
			{
				changed(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in RaiseConnectionStatusChanged", ex);
			}
		}

		private void RaiseCannotPersistAndSendWorkItem()
		{
			EventHandler<EventArgs> del = CannotPersistAndSendWorkItem;
			if (del == null) return;
			try
			{
				del(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in RaiseCannotPersistAndSendWorkItem", ex);
			}
		}
	}
}
