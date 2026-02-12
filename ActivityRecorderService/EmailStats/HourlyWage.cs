using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	public class HourlyWage : Wage
	{
		public HourlyWage(decimal defaultWage)
			: base(defaultWage)
		{
		}

		public override TimeSpan Interval
		{
			get { return TimeSpan.FromHours(1); }
		}
	}
}
