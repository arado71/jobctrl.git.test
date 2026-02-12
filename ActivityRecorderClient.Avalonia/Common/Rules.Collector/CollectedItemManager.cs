using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Rules.Collector
{
	/// <summary>
	/// Class for aggregating collected items in order to reduce their overhead. (Bandwidth/CPU on the server)
	/// We persist raw collected items to the disk (in order not to lose any data) and aggregate in memory.
	/// After a given interval we will send the aggregate for further processing (to the server) and delete raw collected items.
	/// </summary>
	public class CollectedItemManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object thisLock = new object();
		private static readonly TimeSpan aggrFailedInterval = TimeSpan.FromMinutes(5); //aggregation interval if we have some error in persisting items
		private readonly FolderStore<CollectedItem> store = new FolderStore<CollectedItem>("CollectedItems");
		private AggregateCollectedItems currentAggr;
		private bool lastSaveFailed;

		public event EventHandler<AggregateCollectedItemCreatedEventArgs> ItemCreated;

		public CollectedItemManager() : base(log)
		{
		}

		protected override int ManagerCallbackInterval
		{
			get
			{
				return (int)(lastSaveFailed ? aggrFailedInterval.TotalMilliseconds : TimeSpan.FromMinutes(ConfigManager.CollectedItemAggregateInMins).TotalMilliseconds);
			}
		}

		protected override void ManagerCallbackImpl()
		{
			CreateAggregate();
		}

		public void Load()
		{
			foreach (var path in store.GetPersistedPaths())
			{
				Add(store.Load(path), false);
			}

			RestartTimer();
		}

		public void Add(CollectedItem collectedItem)
		{
			Add(collectedItem, true);
		}

		public override void Stop()
		{
			base.Stop();
			CreateAggregate(true); // force Send before shutdown
		}

		private void Add(CollectedItem collectedItem, bool isSaveNeeded)
		{
			lock (thisLock)
			{
				if (collectedItem == null) return;
				if (isSaveNeeded)
				{
					var isSaved = store.Save(collectedItem);
					if (!isSaved)
					{
						lastSaveFailed = true;
						log.ErrorAndFail("Unable to save collected item " + collectedItem);
					}
				}

				if (currentAggr == null || currentAggr.UserId != collectedItem.UserId ||
				    currentAggr.ComputerId != collectedItem.ComputerId) //we need to create a new currentAggr
				{
					if (currentAggr != null) //this could happen if we have offline data with different computerId
					{
						if (currentAggr.UserId != collectedItem.UserId)
							log.ErrorAndFail("AggregateCollectedItems userId changed " + currentAggr.UserId + " " + collectedItem.UserId);
								//userId should not change
						if (currentAggr.ComputerId != collectedItem.ComputerId)
							log.Info("AggregateCollectedItems computerId changed " + currentAggr.ComputerId + " " + collectedItem.ComputerId);
						CreateAggregate(); //create an aggregate with the old data so we could start a new one with matching userId and computerId
						RestartTimer(false);
					}
					Debug.Assert(currentAggr == null);
					currentAggr = new AggregateCollectedItems()
					{
						Id = Guid.NewGuid(),
						UserId = collectedItem.UserId,
						ComputerId = collectedItem.ComputerId,
						Items = new List<CollectedItemIdOnly>(),
						KeyLookup = new Dictionary<int, string>(),
						ValueLookup = new Dictionary<int, string>(),
					};
				}

				currentAggr.Add(collectedItem);
			}
		}

		private void CreateAggregate(bool isForced = false)
		{
			lock (thisLock)
			{
				if (currentAggr == null || currentAggr.Items.Count == 0) return;
				var sw = Stopwatch.StartNew();
				lastSaveFailed = false; //we will delete collected items so we don't care about failed saves anymore
				currentAggr.CreateDate = DateTime.UtcNow;
				OnItemCreated(currentAggr, isForced); //danger if you pull the plug here we will have duplicate data (better than losing data)
				log.Debug("Created AggregateCollectedItems " + currentAggr.Items.Count + " " +
				          sw.Elapsed.TotalMilliseconds.ToString("0.000") + "ms");
				sw.Reset();
				sw.Start();
				var isDeleted = store.DeleteAll(); //no transactions here :(
				log.Debug("Deleted raw CollectedItems in " + sw.Elapsed.TotalMilliseconds.ToString("0.000") + "ms");
				currentAggr = null;
				if (!isDeleted && !isForced) // if forced we skip waiting for retry
				{
					log.ErrorAndFail("Cannot delete raw collected items");
					System.Threading.Thread.Sleep(2000); //ok i'm not proud of this one...
					isDeleted = store.DeleteAll(); //but retry
					if (!isDeleted) log.Error("Retry also failed for deleting raw collected items");
					else log.Info("Retry for deleting raw collected items was successful");
				}
			}
		}

		private void OnItemCreated(AggregateCollectedItems item, bool isForced)
		{
			var del = ItemCreated;
			if (del != null) del(this, new AggregateCollectedItemCreatedEventArgs(item, isForced));
		}

		public class AggregateCollectedItemCreatedEventArgs : EventArgs
		{
			public AggregateCollectedItemCreatedEventArgs(AggregateCollectedItems item, bool shouldSendImmediately)
			{
				Item = item;
				ShouldSendImmediately = shouldSendImmediately;
			}

			public AggregateCollectedItems Item { get; private set; }
			public bool ShouldSendImmediately { get; private set; }
		}
	}
}
