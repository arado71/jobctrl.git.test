using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "AssignProjectData", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class AssignProjectData
	{
		[DataMember(Name = "ProjectKey", Order = 0, IsRequired = true, EmitDefaultValue = true)]
		public string ProjectKey { get; set; }
		[DataMember(Name = "ServerRuleId", Order = 1, IsRequired = true, EmitDefaultValue = true)]
		public int ServerRuleId { get; set; }
		[DataMember(Name = "ProjectName", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public string ProjectName { get; set; }
		[DataMember(Name = "Description", Order = 3, IsRequired = false, EmitDefaultValue = false)]
		public string Description { get; set; }

		public override string ToString()
		{
			return ProjectKey + " [" + ProjectName + "] (" + ServerRuleId + ")" + (Description == null ? "" : " d:"+ Description);
		}
	}
}
