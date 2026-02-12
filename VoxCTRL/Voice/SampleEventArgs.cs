using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VoxCTRL.Voice
{
	public class SampleEventArgs : EventArgs
	{
		[DebuggerStepThrough]
		public SampleEventArgs(float minValue, float maxValue)
		{
			this.MaxSample = maxValue;
			this.MinSample = minValue;
		}
		public float MaxSample { get; private set; }
		public float MinSample { get; private set; }
	}
}
