using System;
using JCAutomation.Data;

namespace JCAutomation
{
	public class CaptureEventArgs : EventArgs
	{
		public ScriptCapture ScriptCapture { get; set; }
		public ControlInfo ActiveControl { get; private set; }
		public CaptureEventArgs(){}
		public CaptureEventArgs(ControlInfo activeControl)
		{
			ActiveControl = activeControl;
		}
	}
}
