using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Stats
{
	[Serializable]
	public class LocalWorkStats
	{
		public Dictionary<WorkType, Dictionary<int, List<Interval>>> WorkIntervalsByTypeByWorkId { get; private set; }

		public LocalWorkStats()
		{
			WorkIntervalsByTypeByWorkId = new Dictionary<WorkType, Dictionary<int, List<Interval>>>();
		}
	}
}
