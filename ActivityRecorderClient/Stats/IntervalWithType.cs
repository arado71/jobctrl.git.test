using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Stats
{
	[Serializable]
	public class IntervalWithType : Interval
	{
		public WorkType WorkType { get; set; }
	}
}
