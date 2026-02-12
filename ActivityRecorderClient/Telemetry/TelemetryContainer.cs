using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;

namespace Tct.ActivityRecorderClient.Telemetry
{
	[Serializable]
	public class TelemetryContainer
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly Dictionary<string, Dictionary<string,List<DateTime>>> measurements = new Dictionary<string, Dictionary<string, List<DateTime>>>();
		private readonly Dictionary<string, string> observations = new Dictionary<string, string>();
		private int statCount;

		public int Count { get { return statCount; } }

		public void Clear()
		{
			statCount = 0;
			measurements.Clear();
			// Observations are not cleared!
		}

		public void Add(ITelemetryEvent stat)
		{
			var measurement = stat as Measurement;
			if (measurement != null)
			{
				Add(measurement);
				return;
			}

			var obs = stat as Observation;
			if (obs != null)
			{
				Add(obs);
				return;
			}

			var evt = stat as Event;
			if (evt != null)
			{
				Add(evt);
				return;
			}

			Debug.Fail("Unknown ITelemetryEvent type " + stat.GetType().FullName);
		}

		public void FilterObservations(HashSet<string> active)
		{
			var inactiveKeys = observations.Select(x => x.Key).Where(x => !active.Contains(x)).ToList();
			foreach (var inactiveKey in inactiveKeys)
			{
				observations.Remove(inactiveKey);
			}
		}

		public void FlushObservations()
		{
			foreach (var observation in observations)
			{
				Add(observation.Key, DateTime.UtcNow, observation.Value);
			}
		}

		public Dictionary<string, Dictionary<string, List<DateTime>>> Export()
		{
			return new Dictionary<string, Dictionary<string, List<DateTime>>>(measurements);
		}

		private void Add(string eventName, DateTime timestamp, string serializedValue)
		{
			++statCount;
			Dictionary<string, List<DateTime>> valueOccurenceCollection;
			if (!measurements.TryGetValue(eventName, out valueOccurenceCollection))
			{
				valueOccurenceCollection = new Dictionary<string, List<DateTime>>();
				measurements.Add(eventName, valueOccurenceCollection);
			}

			List<DateTime> occurenceCollection;
			if (!valueOccurenceCollection.TryGetValue(serializedValue, out occurenceCollection))
			{
				occurenceCollection = new List<DateTime>();
				valueOccurenceCollection.Add(serializedValue, occurenceCollection);
			}

			occurenceCollection.Add(timestamp);
			log.DebugFormat("Stat '{0}' recorded with value '{1}'", eventName, serializedValue);
		}

		private void Add(Measurement measurement)
		{
			Add(measurement.EventName, measurement.Timestamp, JsonHelper.SerializeData(measurement.Value));
		}

		private void Add(Observation observation)
		{
			string observedValue = JsonHelper.SerializeData(observation.Value);
			string observed;
			if (!observations.TryGetValue(observation.EventName, out observed))
			{
				observations.Add(observation.EventName, observedValue);
				Add(observation);
			}
			else
			{
				if (!string.Equals(observedValue, observed, StringComparison.InvariantCulture))
				{
					Add(observation.EventName, observation.Timestamp, observedValue);
				}
			}
		}

		private void Add(Event eventData)
		{
			Add(eventData.EventName, eventData.Timestamp, "");
		}
	}
}
