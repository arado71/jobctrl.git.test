using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public class WorkOrProjectWithParentNames
	{
		public WorkOrProjectName WorkOrProjectName { get; set; }
		public string FullName { get; set; }
	}
}
