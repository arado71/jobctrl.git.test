using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	//Note: This could use WorkTimeStatsBuilder to calculate worktimes... and then no code dupe needed but its slower
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class UserWorkStats : IFilterableStats<UserWorkStats>
	{
		[DataMember]
		public int WorkId { get; set; }
		[DataMember]
		public DateTime StartDate { get; set; }
		[DataMember]
		public TimeSpan WorkTime { get; set; }

		public void AddWorkItem(WorkItem item)
		{
			Debug.Assert(item.WorkId == WorkId);
			if (StartDate > item.StartDate)
			{
				StartDate = item.StartDate;
			}
			WorkTime += (item.EndDate - item.StartDate);
		}

		public UserWorkStats GetFilteredCopy(StatsFilter filter)
		{
			var result = new UserWorkStats()
			{
				StartDate = this.StartDate,
				WorkId = this.WorkId,
				WorkTime = this.WorkTime
			};
			return result;
		}

		public bool SatisfiesFilter(StatsFilter filter)
		{
			return true;
		}
	}
}
