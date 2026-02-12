using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ProjectServer.Client;

namespace Tct.ActivityRecorderClient.ProjectSync
{
	public class SyncContext
	{
		public ProjectContext ProjectContext { get; set; }
		public TimeSheetPeriod[] Periods { get; set; }
		public EnterpriseResource Self { get; set; }
		public bool ShouldSubmit { get; set; }
			
		public Dictionary<Guid, Guid> TaskGuidAssignmentGuidLookup { get; set; }
		public Dictionary<Guid, Guid> TaskGuidProjectGuidLookup { get; set; }

		public SyncContext() { }

		public SyncContext(SyncContext context)
		{
			ProjectContext = context.ProjectContext;
			Periods = context.Periods;
			Self = context.Self;
			ShouldSubmit = context.ShouldSubmit;
			TaskGuidProjectGuidLookup = context.TaskGuidProjectGuidLookup;
			TaskGuidAssignmentGuidLookup = context.TaskGuidAssignmentGuidLookup;
		}
	}
}
