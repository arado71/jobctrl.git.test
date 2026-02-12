using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	//todo this should be immutable
	public class AggregatedActivity
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int KeyboardActivity { get; set; }
		public int MouseActivity { get; set; }
	}
}
