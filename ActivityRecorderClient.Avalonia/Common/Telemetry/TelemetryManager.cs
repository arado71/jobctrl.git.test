using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using log4net;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Telemetry
{
	public class TelemetryManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string FileName = "Telemetry";
		private readonly object thisLock = new object();
		private readonly TelemetryContainer telemetry;
		private int saveInterval;
		private TimeSpan sendInterval;
		private int sendCount;

		public event EventHandler<SingleValueEventArgs<TelemetryContainer>> SendTelemetryRequired;

		public DateTime LastSend { get; private set; }

		public TelemetryManager() : base(log)
		{
			saveInterval = ConfigManager.UsageSaveInSecs * 1000;
			StatsSaveData savedData;
			if (!IsolatedStorageSerializationHelper.Exists(FileName) ||
				!IsolatedStorageSerializationHelper.Load(FileName, out savedData))
			{
				log.Debug("No telemetry data found on disk");
				savedData = new StatsSaveData();
			}

			LastSend = savedData.LastSend;
			telemetry = savedData.Telemetry;
			log.DebugFormat("Loaded {0} telemetry data", telemetry.Count);
		}

		public override void Stop()
		{
			base.Stop();
			lock (thisLock)
			{
				Save();
			}
		}

		public void AddStat(ITelemetryEvent stat)
		{
			lock (thisLock)
			{
				telemetry.Add(stat);
				if (sendCount != 0 && telemetry.Count >= sendCount)
				{
					SendStats();
				}
			}
		}

		public void SetSendCount(int sendCount)
		{
			lock (thisLock)
			{
				this.sendCount = sendCount;
			}
		}

		public void SetSaveInterval(int interval)
		{
			lock (thisLock)
			{
				saveInterval = interval;
			}

			RestartTimer(false);
		}

		public void SetSendInterval(TimeSpan interval)
		{
			lock (thisLock)
			{
				sendInterval = interval;
			}
		}

		public void ForceSave()
		{
			RestartTimer(false);
			lock (thisLock)
			{
				Save();
			}
		}

		public void FilterObservations(HashSet<string> activeObservations)
		{
			lock (thisLock)
			{
				telemetry.FilterObservations(activeObservations);
			}
		}

		private void Save()
		{
			// Assert thisLock is held
			IsolatedStorageSerializationHelper.Save(FileName, new StatsSaveData(telemetry, LastSend));
			log.Debug("Telemetry data saved.");
		}

		private bool ShouldSend()
		{
			// Assert thisLock is held
			return LastSend + sendInterval < DateTime.UtcNow;
		}

		private void SendStats()
		{
			// Assert thisLock is held
			OnSendStats(telemetry);
			log.Debug("Telemetry data created.");
			LastSend = DateTime.UtcNow;
			telemetry.Clear();
			telemetry.FlushObservations();
			Save();
		}

		protected override void ManagerCallbackImpl()
		{
			lock (thisLock)
			{
				if (ShouldSend())
				{
					Save();
					SendStats();
				}
				else
				{
					Save();
				}
			}
		}

		protected override int ManagerCallbackInterval
		{
			get { return saveInterval; }
		}

		protected void OnSendStats(TelemetryContainer telemetryToSend)
		{
			var evt = SendTelemetryRequired;
			if (evt == null) return;
			var args = new SingleValueEventArgs<TelemetryContainer>(telemetryToSend);
			evt(this, args);
		}

		[DataContract]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		internal partial class StatsSaveData
		{
			[DataMember]
			public TelemetryContainer Telemetry { get; private set; }
			[DataMember]
			public DateTime LastSend { get; private set; }

			public StatsSaveData()
			{
				Telemetry = new TelemetryContainer();
				LastSend = DateTime.UtcNow;
			}

			public StatsSaveData(TelemetryContainer telemetry, DateTime lastSend)
			{
				Telemetry = telemetry;
				LastSend = lastSend;
			}
		}
	}
}
