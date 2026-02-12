using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class ToolStripMenuItemForWorkData : ToolStripMenuItemWithProgressbar
	{
		public override WorkData WorkData
		{
			get { return base.WorkData; }
			set
			{
				if (base.WorkData == value) return;
				base.WorkData = value;
				ToolTipText = GetWorkDataDesc(base.WorkData);
				//we don't change the text here...
			}
		}

		public ToolStripMenuItemForWorkData(WorkData workData)
			: base(workData == null ? "" : workData.Name, null)
		{
			WorkData = workData;
		}

		public static string GetWorkDataDesc(WorkData workData, TimeSpan? totalWorkTime = null)
		{
			if (workData == null) return "";
			return (workData.Id.HasValue ? "Id: " + workData.Id : (Labels.WorkData_Project + (workData.ProjectId.HasValue ? " Id: " + workData.ProjectId : "")))
				+ (workData.ExtId.HasValue ? " (" + workData.ExtId + ")" : "")
				+ (workData.Priority.HasValue ? " " + Labels.WorkData_Priority + ": " + workData.Priority : "")
				+ (workData.StartDate.HasValue ? " " + Labels.WorkData_StartDate + ": " + workData.StartDate.Value.ToShortDateString() : "")
				+ (workData.EndDate.HasValue ? " " + Labels.WorkData_EndDate + ": " + workData.EndDate.Value.ToShortDateString() : "")
				+ (workData.TargetTotalWorkTime.HasValue ? " " + Labels.WorkData_TargetHours + ": " + workData.TargetTotalWorkTime.Value.ToHourMinuteString() : "")
				+ (totalWorkTime.HasValue ? " " + Labels.WorkData_WorkedHours + ": " + totalWorkTime.ToHourMinuteString() : "");
		}

	}
}
