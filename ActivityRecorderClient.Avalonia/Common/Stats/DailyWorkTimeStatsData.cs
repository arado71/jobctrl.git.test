using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Stats
{
	[DataContract]
	public class DailyWorkTimeStatsData
	{
		[DataMember]
		public long CurrentVersion;
		[DataMember]
		public Dictionary<DateTime, DailyWorkTimeStats> DailyWorkTimes;
	}
}
