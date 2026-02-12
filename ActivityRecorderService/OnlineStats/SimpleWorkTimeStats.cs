using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.OnlineStats
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class SimpleWorkTimeStats
	{
		[DataMember]
		public int UserId { get; set; }

		[DataMember]
		public DateTime FromDate { get; set; }

		[DataMember]
		public DateTime ToDate { get; set; }

		[DataMember]
		public Dictionary<int, SimpleWorkTimeStat> Stats { get; set; }

		public void MergeWith(SimpleWorkTimeStats other) //we use merge to reduce memory pressure
		{
			if (other == null || other.UserId != UserId || (other.FromDate != ToDate && other.ToDate != FromDate))
			{
				Debug.Fail("Cannot merge SimpleWorkTimeStats");
				return;
			}
			if (ToDate == other.FromDate)
			{
				ToDate = other.ToDate;
			}
			else //(FromDate == other.ToDate)
			{
				FromDate = other.FromDate;
			}
			if (Stats == null) return;
			foreach (var simpleWorkTimeStat in other.Stats.Values)
			{
				SimpleWorkTimeStat currStat;
				if (!Stats.TryGetValue(simpleWorkTimeStat.WorkId, out currStat))
				{
					currStat = new SimpleWorkTimeStat() { WorkId = simpleWorkTimeStat.WorkId };
					Stats.Add(currStat.WorkId, currStat);
				}
				currStat.TotalWorkTime += simpleWorkTimeStat.TotalWorkTime;
			}
		}
	}
}
