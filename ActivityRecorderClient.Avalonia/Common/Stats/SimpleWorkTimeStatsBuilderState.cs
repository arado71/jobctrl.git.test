using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Stats
{
	[DataContract]
	public partial class SimpleWorkTimeStatsBuilderState //dailyWorkTimeStats and localWorkStats should contain only disjoint intervals
	{
		[DataMember]
		public LocalWorkStats LocalWorkStats { get; private set; }
		[DataMember]
		public Dictionary<DateTime, DailyWorkTimeStats> DailyWorkTimeStats { get; private set; }

		public SimpleWorkTimeStatsBuilderState(LocalWorkStats localWorkStats, Dictionary<DateTime, DailyWorkTimeStats> dailyWorkTimeStats)
		{
			if (localWorkStats == null) throw new ArgumentNullException("localWorkStats");
			if (dailyWorkTimeStats == null) throw new ArgumentNullException("dailyWorkTimeStats");
			LocalWorkStats = localWorkStats;
			DailyWorkTimeStats = dailyWorkTimeStats;
		}

		[IgnoreDataMember]
		public static SimpleWorkTimeStatsBuilderState Empty
		{
			get { return new SimpleWorkTimeStatsBuilderState(new LocalWorkStats(), new Dictionary<DateTime, DailyWorkTimeStats>()); }
		}
	}
}
