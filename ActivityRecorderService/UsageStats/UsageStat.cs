using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService
{
	public partial class UsageStat
	{
		public TimeSpan ComputerWorkTime
		{
			get { return TimeSpan.FromMilliseconds(ComputerWorkTimeInMs); }
			set { ComputerWorkTimeInMs = (int)value.TotalMilliseconds; }
		}

		public TimeSpan MobileWorkTime
		{
			get { return TimeSpan.FromMilliseconds(MobileWorkTimeInMs); }
			set { MobileWorkTimeInMs = (int)value.TotalMilliseconds; }
		}

		public TimeSpan ManuallyAddedWorkTime
		{
			get { return TimeSpan.FromMilliseconds(ManuallyAddedWorkTimeInMs); }
			set { ManuallyAddedWorkTimeInMs = (int)value.TotalMilliseconds; }
		}
	}
}
