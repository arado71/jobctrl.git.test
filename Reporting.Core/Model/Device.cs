using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;
using Reporter.Model.ProcessedItems;
using Reporter.Processing;

namespace Reporter.Model
{
	public class Device : IEquatable<Device>
	{
		public int UserId { get; set; }
		public ItemType Type { get; set; }
		public string DeviceId { get; set; }

		public bool IsDeletionSupported { get { return Type == ItemType.Pc || Type == ItemType.Mobile; } }

		public bool IsFlatteningRequired { get { return Type == ItemType.Pc || Type == ItemType.Mobile; } }

		public bool HasCollectedItems { get { return Type == ItemType.Pc || Type == ItemType.Mobile; } }

		public Device(int userId, ItemType itemType, string deviceId)
		{
			UserId = userId;
			Type = itemType;
			DeviceId = deviceId;
		}

		[Pure]
		public static Device FromWorkItem(IWorkItem workItem)
		{
			return new Device(workItem.UserId, WorkItemHelper.GetWorkType(workItem), WorkItemHelper.GetDeviceId(workItem));

		}

		[Pure]
		public static Device FromCollectedItem(ICollectedItem collectedItem)
		{
			return new Device(collectedItem.UserId, CollectedItemHelper.GetWorkType(collectedItem), CollectedItemHelper.GetDeviceId(collectedItem));
		}

		[Pure]
		public static Device FromProcessedItem(WorkItem slice)
		{
			return new Device(slice.UserId, slice.Type, GetIdFromProcessedItem(slice));
		}

		[Pure]
		private static string GetIdFromProcessedItem(WorkItem slice)
		{
			if (slice is PcWorkItem)
			{
				return ((PcWorkItem)slice).ComputerId.ToString(CultureInfo.InvariantCulture);
			}

			if (slice is MobileWorkItem)
			{
				return ((MobileWorkItem)slice).Imei.ToString(CultureInfo.InvariantCulture);
			}

			return "";
		}

		public override bool Equals(object obj)
		{
			var other = obj as Device;
			if (other == null) return false;
			return Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17 + UserId.GetHashCode();
				hash = hash * 23 + Type.GetHashCode();
				hash = hash * 23 + DeviceId.GetHashCode();
				return hash;
			}
		}

		public bool Equals(Device other)
		{
			return UserId == other.UserId && Type == other.Type && DeviceId == other.DeviceId;
		}
	}
}
