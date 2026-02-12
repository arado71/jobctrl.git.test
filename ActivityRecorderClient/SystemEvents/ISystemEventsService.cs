using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Tct.ActivityRecorderClient.SystemEvents
{
	public interface ISystemEventsService : IDisposable
	{
		event PowerModeChangedEventHandler PowerModeChanged;
		event SessionSwitchEventHandler SessionSwitch;
	}
}
