using System;
using System.Diagnostics;
using log4net;

namespace Tct.ActivityRecorderClient
{
	public class DebugListener : TraceListener
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public override void Write(string message)
		{
			log.Fatal(message);
		}

		public override void WriteLine(string message)
		{
			log.Fatal(message);
		}
	}
}

