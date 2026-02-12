using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.EnvironmentInfo;

namespace Tct.ActivityRecorderClient.Telemetry
{
	public class TelemetryCoordinator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int DefaultMaxCount = 0;
		private static readonly TimeSpan DefaultSendInterval = TimeSpan.FromDays(2);
		private readonly AsyncWorkQueue<ITelemetryEvent> workQueue;
		private readonly ConcurrentQueue<ITelemetryEvent> tempQueue; 
		private readonly object thisLock = new object();
		private HashSet<string> eventNameFilter;
		private TelemetryManager telemetryManager;
		private WorkItemManager workItemManager;
		private ClientSettingsManager clientSettingsManager;
		private IEnvironmentInfoService environmentService;
		private static readonly TelemetryCoordinator instance = new TelemetryCoordinator();

		public static TelemetryCoordinator Instance { get { return instance; } }

		private volatile int isTelemetryEnabled = -1; // -1 not defined, 0 disabled, 1 enabled

		private TelemetryCoordinator()
		{
			workQueue = new AsyncWorkQueue<ITelemetryEvent>(ProcessStat);
			tempQueue = new ConcurrentQueue<ITelemetryEvent>();
		}

		private void AddToQueue(ITelemetryEvent item)
		{
			switch (isTelemetryEnabled)
			{
				case -1:
					tempQueue.Enqueue(item);
					break;
				case 1:
					ITelemetryEvent tempItem;
					while (tempQueue.TryDequeue(out tempItem))
						workQueue.EnqueueAsync(tempItem);
					workQueue.EnqueueAsync(item);
					break;
			}
		}

		public void RecordMeasurement<T>(string eventName, T value)
		{
			if (!ShouldCollect(eventName)) return;
			var measurement = new Measurement { EventName = eventName, Value = value, Timestamp = DateTime.UtcNow };
			AddToQueue(measurement);
		}

		public void RecordEvent(string eventName)
		{
			if (!ShouldCollect(eventName)) return;
			var eventData = new Event { EventName = eventName, Timestamp = DateTime.UtcNow };
			AddToQueue(eventData);
		}

		public void RecordObservation<T>(string eventName, T value)
		{
			if (!ShouldCollect(eventName)) return;
			var observation = new Observation { EventName = eventName, Value = value, Timestamp = DateTime.UtcNow };
			AddToQueue(observation);
		}

		public void SetNameFilters(IEnumerable<string> eventNames)
		{
			var newNames = new HashSet<string>(eventNames);
			eventNameFilter = newNames;
		}

		public void Start(WorkItemManager workItemManager, ClientSettingsManager clientSettingsManager, IEnvironmentInfoService environmentService)
		{
			lock (thisLock)
			{
				this.environmentService = environmentService;
				this.workItemManager = workItemManager;
				this.clientSettingsManager = clientSettingsManager;
				telemetryManager = new TelemetryManager();
				telemetryManager.SendTelemetryRequired += HandleSendStats;
				clientSettingsManager.SettingsChanged += HandleClientSettingsChanged;
				SetSettings(clientSettingsManager.ClientSettings);
				Monitor.Pulse(thisLock);
			}
		}

		public void Stop()
		{
			lock (thisLock)
			{
				if (telemetryManager == null)
				{
					log.Debug("Stop before start");
					return;
				};
				clientSettingsManager.SettingsChanged -= HandleClientSettingsChanged;
				telemetryManager.SendTelemetryRequired -= HandleSendStats;
				if (isTelemetryEnabled > 0)
					telemetryManager.Stop();
				telemetryManager = null;
				clientSettingsManager = null;
			}
		}

		public void Save()
		{
			lock (thisLock)
			{
				if (telemetryManager != null)
				{
					telemetryManager.ForceSave();
					log.Debug("Forced save");
				}
				else
				{
					log.Warn("Manager already disposed");
				}
			}
		}

		public bool IsEnabled(string eventName)
		{
			return ShouldCollect(eventName);
		}

		private bool ShouldCollect(string eventName)
		{
			lock (thisLock)
			{
				return eventNameFilter == null || eventNameFilter.Contains(eventName);
			}
		}

		private void ProcessStat(ITelemetryEvent stat)
		{
			EnsureInitialized();
			if (!ShouldCollect(stat.EventName)) return;
			telemetryManager.AddStat(stat);
		}

		private void EnsureInitialized()
		{
			if (telemetryManager == null)
			{
				lock (thisLock)
				{
					while (telemetryManager == null || eventNameFilter == null)
					{
						Monitor.Wait(thisLock);
					}
				}

				log.Debug("Waiting for initialization completed");
			}

			Debug.Assert(telemetryManager != null);
		}

		private void HandleClientSettingsChanged(object sender, SingleValueEventArgs<ClientSetting> e)
		{
			SetSettings(e.Value);
		}

		private void SetSettings(ClientSetting settings)
		{
			if (settings == null) return;
			if (settings.TelemetryMaxAgeInMins != null && settings.TelemetryMaxAgeInMins.Value > 0)
			{
				if (isTelemetryEnabled < 1)
				{
					telemetryManager.Start();
					log.Debug("Collection enabled");
				}
				isTelemetryEnabled = 1;
			}
			else
			{
				if (isTelemetryEnabled != 0)
				{
					telemetryManager.Stop();
					log.Debug("Collection disabled");
				}
				var prevEnabled = isTelemetryEnabled;
				isTelemetryEnabled = 0;
				ITelemetryEvent ignored;
				// temporary collected data cleared if telemetry disabled on server
				if (prevEnabled < 0)
					while (tempQueue.TryDequeue(out ignored))
					{
					}
			}
			telemetryManager.SetSendCount(settings.TelemetryMaxCount ?? DefaultMaxCount);
			var sendInterval = settings.TelemetryMaxAgeInMins != null
				? TimeSpan.FromMinutes(settings.TelemetryMaxAgeInMins.Value)
				: DefaultSendInterval;
			telemetryManager.SetSendInterval(sendInterval);
			var newKeys = settings.TelemetryCollectedKeys ?? TelemetryHelper.GetDefaultKeys();
			var newKeyArray = newKeys.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
			var newFilter = new HashSet<string>(newKeyArray, StringComparer.InvariantCulture);
			bool isChanged;
			lock (thisLock)
			{
				isChanged = eventNameFilter == null || !newFilter.SetEquals(eventNameFilter);
				eventNameFilter = newFilter;
				Monitor.Pulse(thisLock);
			}

			telemetryManager.FilterObservations(eventNameFilter);
			if (isChanged)
				log.Debug("Keys set to: " + newKeys);
		}

		private void HandleSendStats(object sender, SingleValueEventArgs<TelemetryContainer> e)
		{
			Debug.Assert(environmentService != null);
			Debug.Assert(telemetryManager != null);
			Debug.Assert(workItemManager != null);
			var statsItem = new TelemetryItem
			{
				EventNameValueOccurences = e.Value.Export(),
				Id = Guid.NewGuid(),
				UserId = ConfigManager.UserId,
				StartDate = telemetryManager.LastSend,
				EndDate = DateTime.UtcNow,
				ComputerId = environmentService.ComputerId,
			};
			workItemManager.PersistAndSend(statsItem);
		}
	}
}
