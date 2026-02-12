using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderClient
{
	public class ThreadFrozenEventArgs : EventArgs
	{
		public ThreadFrozenEventArgs(Thread frozenThread, StackTrace stackTrace)
		{
			FrozenThread = frozenThread;
			StackTrace = stackTrace;
		}

		public StackTrace StackTrace { get; private set; }
		public Thread FrozenThread { get; private set; }
	}
}
