using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.WorkManagement
{
	[DataContract(Namespace = "http://jobctrl.com/WorkManagement")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ProjectManagementConstraints
	{
		[DataMember]
		public int ProjectId { get; set; }

		[DataMember]
		public int WorkMandatoryFields { get; set; } //merge TaskMandatoryFields and AssignmentMandatoryFields into this

		[DataMember]
		public int ProjectManagementPermissions { get; set; }

		[DataMember]
		public DateTime? WorkMinStartDate { get; set; }

		[DataMember]
		public DateTime? WorkMaxEndDate { get; set; }

		[DataMember]
		public TimeSpan? WorkMaxTargetWorkTime { get; set; }

		[DataMember]
		public decimal? WorkMaxTargetCost { get; set; }

		//client don't need to know about tasks vs assignments
		public ManagementFields TaskMandatoryFields { get; set; }
		public ManagementFields AssignmentMandatoryFields { get; set; }
	}
}
