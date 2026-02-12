using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu
{
	[Serializable]
	public class NewWorkStatistic
	{
		public int? Priority { get; set; }
		public int? CategoryId { get; set; }
		public int? TargetWorkTime { get; set; }
		public int? ProjectId { get; set; }
		public string Description { get; set; }
		public int? StartOffset { get; set; }
		public int? Length { get; set; }
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
