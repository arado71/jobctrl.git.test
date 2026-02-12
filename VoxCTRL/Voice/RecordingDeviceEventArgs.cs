using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxCTRL.Voice
{
	public class RecordingDeviceEventArgs : EventArgs
	{
		public readonly TimeSpan RecordingTime;
		public readonly float MinVolume;
		public readonly float MaxVolume;
		public readonly int BytesWritten;

		public RecordingDeviceEventArgs(TimeSpan recTime, float minVolume, float maxVolume, int bytes)
		{
			RecordingTime = recTime;
			MinVolume = minVolume;
			MaxVolume = maxVolume;
			BytesWritten = bytes;
		}
	}
}
