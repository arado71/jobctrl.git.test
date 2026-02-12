using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Tct.ActivityRecorderClient.Capturing.Extra
{
	public static class UsbDevices
	{
		public static List<USBDeviceInfo> GetUSBDevices(string filter = null)
		{
			List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

			var queryString = @"Select * From Win32_PnPEntity";
			if (filter != null) queryString += " Where " + filter;
			using (var searcher = new ManagementObjectSearcher(queryString))
			using (var collection = searcher.Get())
			{
				foreach (var device in collection)
				{
					try
					{
						var props = device.Properties.Cast<PropertyData>().Select(p => new KeyValuePair<string, object>(p.Name, p.Value)).ToArray();
						devices.Add(new USBDeviceInfo(
						(string)device.GetPropertyValue("DeviceID"),
						(string)device.GetPropertyValue("PNPDeviceID"),
						(string)device.GetPropertyValue("Description"),
						(string)device.GetPropertyValue("PnpClass"),
						(string)device.GetPropertyValue("Manufacturer")
						));
					}
					finally
					{
						device.Dispose();
					}
				}
			}
			return devices;
		}
	}

	public class USBDeviceInfo : IComparable<USBDeviceInfo>
	{
		public USBDeviceInfo(string deviceID, string pnpDeviceID, string description, string pnpClass, string manufacturer)
		{
			DeviceID = deviceID;
			PnpDeviceID = pnpDeviceID;
			Description = description;
			PnpClass = pnpClass;
			Manufacturer = manufacturer;
		}
		public string DeviceID { get; private set; }
		public string PnpDeviceID { get; private set; }
		public string Description { get; private set; }
		public string PnpClass { get; private set; }
		public string Manufacturer { get; private set; }

		public int CompareTo(USBDeviceInfo other)
		{
			return string.Compare(DeviceID, other.DeviceID, StringComparison.Ordinal) == 0 ? string.Compare(PnpDeviceID, other.PnpDeviceID, StringComparison.Ordinal) == 0 ? string.Compare(Description, other.Description, StringComparison.Ordinal) == 0 ? string.Compare(PnpClass, other.PnpClass, StringComparison.Ordinal)  == 0 ? string.Compare(Manufacturer, other.Manufacturer, StringComparison.Ordinal) : string.Compare(PnpClass, other.PnpClass, StringComparison.Ordinal) : string.Compare(Description, other.Description, StringComparison.Ordinal) : string.Compare(PnpDeviceID, other.PnpDeviceID, StringComparison.Ordinal) : string.Compare(DeviceID, other.DeviceID, StringComparison.Ordinal);
		}

		public override string ToString()
		{
			return $"{DeviceID}, {Manufacturer}, {PnpClass}, {Description}";
		}
	}

}
