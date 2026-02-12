using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxCTRL.Voice
{
	public enum RecordingState
	{
		Stopped,
		StopRequested,
		Recording,
		Paused,
		PauseRequested,
	}
}
