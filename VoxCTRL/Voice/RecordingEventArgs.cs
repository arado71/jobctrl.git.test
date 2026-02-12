using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxCTRL.Voice
{
	public class RecordingEventArgs : EventArgs
	{
		public readonly TimeSpan RecordingTime;
		public readonly byte[] Data;
		public readonly bool Ended;

		public RecordingEventArgs(TimeSpan recordingTime, byte[] data, bool ended)
		{
			RecordingTime = recordingTime;
			Data = data;
			Ended = ended;
		}
	}
}
