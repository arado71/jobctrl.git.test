using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu
{
	[DataContract]
	public class NewWorkStatistic
	{
		[DataMember]
		public int? Priority { get; set; }
		[DataMember]
		public int? CategoryId { get; set; }
		[DataMember]
		public int? TargetWorkTime { get; set; }
		[DataMember]
		public int? ProjectId { get; set; }
		[DataMember]
		public string Description { get; set; }
		[DataMember]
		public int? StartOffset { get; set; }
		[DataMember]
		public int? Length { get; set; }
		[DataMember]
		public bool StartNew { get; set; }

		public NewWorkStatistic() { }

		public NewWorkStatistic(WorkData wd, int projectId)
		{
			Priority = wd.Priority;
			CategoryId = wd.CategoryId;
			TargetWorkTime = wd.TargetTotalWorkTime != null ? (int?)wd.TargetTotalWorkTime.Value.TotalMinutes : null;
			ProjectId = projectId;
			Description = wd.Description;
			StartOffset = wd.StartDate != null ? (int?)((wd.StartDate.Value - DateTime.Today.Date).Days) : null;
			Length = wd.StartDate != null && wd.EndDate != null ? (int?)((wd.EndDate.Value - wd.StartDate.Value).Days) : null;
		}
	}
}
