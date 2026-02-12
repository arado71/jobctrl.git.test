using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Telemetry;

namespace Tct.ActivityRecorderClient.Rules.Collector
{
	/// <summary>
	/// Class for creating CollectedItems by calculating value changes from all captured values.
	/// </summary>
	public class CollectedItemCreator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		internal static readonly TimeSpan resetInterval = TimeSpan.FromHours(22);
		internal static readonly int MaxValueLength = 4000;

		private readonly Dictionary<string, MaskedValue> lastCapturedValues = new Dictionary<string, MaskedValue>(StringComparer.OrdinalIgnoreCase);
		private readonly int throttleMaxChangeCount;
		private readonly TimeSpan throttleMaxChangeWindow;
		private DateTime lastResetDate;

		public event EventHandler<SingleValueEventArgs<CollectedItem>> ItemCreated;

		public CollectedItemCreator()
			: this(60, TimeSpan.FromMinutes(2)) //todo we need to lower limits?
		{
		}

		public CollectedItemCreator(int throttleMaxChangeCount, TimeSpan throttleMaxChangeWindow)
		{
			this.throttleMaxChangeCount = throttleMaxChangeCount;
			this.throttleMaxChangeWindow = throttleMaxChangeWindow;
		}

		public void UpdateCapturedValues(DateTime createDate, Dictionary<string, string> capturedValues)
		{
			DebugEx.EnsureGuiThread();
			var isResend = IsResendAllRequired(createDate);
			var changedValues = ChangeLastCapturedValues(createDate, capturedValues, isResend);
			if (changedValues == null) return;
			var data = new CollectedItem()
			{
				Id = Guid.NewGuid(),
				UserId = ConfigManager.UserId,
				ComputerId = ConfigManager.EnvironmentInfo.ComputerId,
				CreateDate = createDate,
				CapturedValues = changedValues,
			};
			log.DebugFormat("Created collected item {0}", data);
			TelemetryHelper.Measure(TelemetryHelper.KeyCollectedItem, changedValues);
			OnItemCreated(data);
		}

		private bool IsResendAllRequired(DateTime now)
		{
			if (lastResetDate == DateTime.MinValue)
			{
				lastResetDate = now;
				return true;
			}
			if (now - lastResetDate > resetInterval)
			{
				log.Info("Reseting last captured values");
				RemoveEmptyCapturedKeys(); // Only empty captures are safe to remove, because we might miss a key removal.
				lastResetDate = now;
				return true;
			}

			return false;
		}

		private void RemoveEmptyCapturedKeys()
		{
			var emptyKeys = lastCapturedValues.Where(x => x.Value.CapturedValue == null).Select(x => x.Key).ToArray();
			foreach (var emptyKey in emptyKeys)
			{
				lastCapturedValues.Remove(emptyKey);
			}
		}

		private Dictionary<string, string> ChangeLastCapturedValues(DateTime createDate, Dictionary<string, string> capturedValues, bool forceChange)
		{
			DebugEx.EnsureGuiThread();
			var changedValues = new Dictionary<string, string>();
			var missingKeys = new HashSet<string>(lastCapturedValues.Keys, StringComparer.OrdinalIgnoreCase);
			foreach (var capturedValue in capturedValues)
			{
				missingKeys.Remove(capturedValue.Key);
				RegisterChange(capturedValue.Key, capturedValue.Value, createDate, changedValues, forceChange);
			}
			foreach (var missingKey in missingKeys)
			{
				RegisterChange(missingKey, null, createDate, changedValues, forceChange);
			}
			return changedValues.Count != 0 ? changedValues : null;
		}

		private void RegisterChange(string capturedKey, string capturedValue, DateTime createDate, Dictionary<string, string> changedValues, bool forceChange)
		{
			MaskedValue maskedValue;
			var isNew = !lastCapturedValues.TryGetValue(capturedKey, out maskedValue);
			if (isNew)
			{
				maskedValue = new MaskedValue(this);
				lastCapturedValues.Add(capturedKey, maskedValue);
			}
			string rawValue;
			if (maskedValue.TrySetValue(capturedValue.Truncate(MaxValueLength), createDate, out rawValue) || isNew || forceChange)
			{
				changedValues[capturedKey] = rawValue;
			}
		}

		private void OnItemCreated(CollectedItem item)
		{
			var del = ItemCreated;
			if (del != null) del(this, SingleValueEventArgs.Create(item));
		}

		internal class MaskedValue
		{
			internal static readonly string LimitExceededValue = "*T*";

			private readonly ThrottledStore<string> store;

			public string CapturedValue { get; private set; }

			public MaskedValue(CollectedItemCreator parent)
			{
				store = new ThrottledStore<string>(parent.throttleMaxChangeCount, parent.throttleMaxChangeWindow);
			}

			public bool TrySetValue(string value, DateTime createDate, out string maskedValue)
			{
				var throttledValue = store.Set(value, createDate);
				maskedValue = throttledValue.IsThrottleLimitExceeded ? LimitExceededValue : throttledValue.Value;
				var changed = !string.Equals(CapturedValue, maskedValue, StringComparison.OrdinalIgnoreCase);
				CapturedValue = maskedValue;
				return changed;
			}
		}
	}
}
