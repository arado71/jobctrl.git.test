using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlaybackClient.ActivityRecorderServiceReference
{
	partial class ManualWorkItem
	{
		public override string ToString()
		{
			return "manualWorkItem userId: " + UserId + " workId: " + WorkId + " type: " + ManualWorkItemTypeId + " start: " + StartDate + " end: " + EndDate;
		}
	}
}
