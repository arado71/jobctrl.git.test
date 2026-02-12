using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxCTRL.Voice
{
	public class RecordingStateEventArgs : EventArgs
	{
		public readonly RecordingState State;

		public RecordingStateEventArgs(RecordingState state)
		{
			State = state;
		}
	}
}
