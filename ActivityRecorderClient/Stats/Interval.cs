using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Stats
{
	[Serializable]
	public class Interval
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
