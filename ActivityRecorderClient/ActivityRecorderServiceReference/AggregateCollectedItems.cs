using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	partial class AggregateCollectedItems : IUploadItem
	{
		private readonly Dictionary<string, int> reverseValueLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, int> reverseKeyLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		private int lastKeyId;
		private int lastValueId;

		public void Add(CollectedItem collectedItem)
		{
			Debug.Assert(collectedItem.UserId == this.UserId);
			Debug.Assert(collectedItem.ComputerId == this.ComputerId);
			if (collectedItem.CapturedValues == null || collectedItem.CapturedValues.Count == 0) return;

			var item = new CollectedItemIdOnly()
			{
				CreateDate = collectedItem.CreateDate,
				CapturedValues = new Dictionary<int, int?>()
			};
			Items.Add(item);

			foreach (var capturedValue in collectedItem.CapturedValues)
			{
				var keyId = GetCapturedValueId(capturedValue.Key, KeyLookup, reverseKeyLookup, ref lastKeyId);
				var valueId = capturedValue.Value == null ? new int?() : GetCapturedValueId(capturedValue.Value, ValueLookup, reverseValueLookup, ref lastValueId);
				item.CapturedValues[keyId] = valueId;
			}
		}

		private static int GetCapturedValueId(string capturedValue, Dictionary<int, string> keyLookup, Dictionary<string, int> reverseKeyLookup, ref int lastValueId)
		{
			int valueId;
			if (!reverseKeyLookup.TryGetValue(capturedValue, out valueId))
			{
				lastValueId++;
				if (keyLookup.ContainsKey(lastValueId)) //inconsistent last value
				{
					lastValueId = keyLookup.Keys.Max() + 1;
				}
				keyLookup.Add(lastValueId, capturedValue);
				reverseKeyLookup.Add(capturedValue, lastValueId);
				valueId = lastValueId;
			}
			return valueId;
		}


		public Guid Id { get; set; }

		public DateTime StartDate
		{
			get { return Items == null || Items.Count == 0 ? DateTime.MinValue : Items[0].CreateDate; }
		}
	}
}
