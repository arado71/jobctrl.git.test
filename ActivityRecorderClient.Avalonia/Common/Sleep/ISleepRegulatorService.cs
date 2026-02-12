using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Sleep
{
	public interface ISleepRegulatorService
	{
		void PreventSleep();
		void AllowSleep();
	}
}
