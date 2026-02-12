using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Stats
{
	[Serializable]
	public class WorkInterval : IntervalWithType
	{
		public int WorkId { get; set; }
	}
}
