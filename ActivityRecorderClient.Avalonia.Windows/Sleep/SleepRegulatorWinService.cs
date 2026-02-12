using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderClient.Sleep
{
	public class SleepRegulatorWinService : ISleepRegulatorService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void PreventSleep()
		{
			var oldState = WinApi.SetThreadExecutionState(WinApi.EXECUTION_STATE.ES_CONTINUOUS | WinApi.EXECUTION_STATE.ES_SYSTEM_REQUIRED);
			if (oldState != 0) return;
			var errCode = Marshal.GetLastWin32Error();
			if (errCode != 0)
			{
				log.Error("Unable to prevent sleep", new Win32Exception(errCode));
			}
		}

		public void AllowSleep()
		{
			var oldState = WinApi.SetThreadExecutionState(WinApi.EXECUTION_STATE.ES_CONTINUOUS);
			if (oldState != 0) return;
			var errCode = Marshal.GetLastWin32Error();
			if (errCode != 0)
			{
				log.Error("Unable to allow sleep", new Win32Exception(errCode));
			}
		}
	}
}
