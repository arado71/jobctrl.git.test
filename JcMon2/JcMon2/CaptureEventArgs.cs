using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JcMon2
{
	public class CaptureEventArgs : EventArgs
	{
		public CaptureEventArgs(ControlInfo activeControl)
		{
			ActiveControl = activeControl;
		}

		public ControlInfo ActiveControl { get; private set; }
	}
}
