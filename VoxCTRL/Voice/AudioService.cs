using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Mixer;
using NAudio.Wave;
using System.Diagnostics;

namespace VoxCTRL.Voice
{
	public class AudioService
	{
		public int RecordingDeviceCount { get { return WaveIn.DeviceCount; } }

		public IEnumerable<RecordingDevice> GetRecordingDevices()
		{
			var result = new RecordingDevice[RecordingDeviceCount];
			for (int i = 0; i < result.Length; i++)
			{
				var cap = WaveIn.GetCapabilities(i);
				result[i] = new RecordingDevice() { Id = i, Name = cap.ProductName, Channels = cap.Channels };
			}
			return result;
		}
	}
}
