using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Forms;

namespace Tct.ActivityRecorderClient.SystemEvents
{
	public interface ISystemEventsService : IDisposable
	{
		event PowerModeChangedEventHandler PowerModeChanged;
		event SessionSwitchEventHandler SessionSwitch;
	}
}
