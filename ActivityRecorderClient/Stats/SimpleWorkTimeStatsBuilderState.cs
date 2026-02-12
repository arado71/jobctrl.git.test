using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Stats
{
	[Serializable]
	public class SimpleWorkTimeStatsBuilderState //dailyWorkTimeStats and localWorkStats should contain only disjoint intervals
	{
		public readonly LocalWorkStats LocalWorkStats;
		public readonly Dictionary<DateTime, DailyWorkTimeStats> DailyWorkTimeStats;

		public SimpleWorkTimeStatsBuilderState(LocalWorkStats localWorkStats, Dictionary<DateTime, DailyWorkTimeStats> dailyWorkTimeStats)
		{
			if (localWorkStats == null) throw new ArgumentNullException("localWorkStats");
			if (dailyWorkTimeStats == null) throw new ArgumentNullException("dailyWorkTimeStats");
			LocalWorkStats = localWorkStats;
			DailyWorkTimeStats = dailyWorkTimeStats;
		}

		public static SimpleWorkTimeStatsBuilderState Empty
		{
			get { return new SimpleWorkTimeStatsBuilderState(new LocalWorkStats(), new Dictionary<DateTime, DailyWorkTimeStats>()); }
		}
	}
}
