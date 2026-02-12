using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Scheduling
{
	public enum ScheduleType
	{
		[Description("Egyszeri")]
		OneTime = 0,
		[Description("Intervallum")]
		EvenInterval,
		[Description("Napi")]
		Daily,
		[Description("Heti")]
		Weekly,
		[Description("Havi")]
		Monthly
	}
}
