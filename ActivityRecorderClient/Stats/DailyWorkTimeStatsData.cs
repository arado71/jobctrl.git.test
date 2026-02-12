using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Stats
{
	[Serializable]
	public class DailyWorkTimeStatsData
	{
		public long CurrentVersion;
		public Dictionary<DateTime, DailyWorkTimeStats> DailyWorkTimes;
	}
}
