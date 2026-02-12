using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "AssignWorkData", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class AssignWorkData
	{
		[DataMember(Name = "WorkKey", Order = 0, IsRequired = true, EmitDefaultValue = true)]
		public string WorkKey { get; set; }
		[DataMember(Name = "ServerRuleId", Order = 1, IsRequired = true, EmitDefaultValue = true)]
		public int ServerRuleId { get; set; }
		[DataMember(Name = "WorkName", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public string WorkName { get; set; }
		[DataMember(Name = "ProjectId", Order = 3, IsRequired = false, EmitDefaultValue = false)]
		public int? ProjectId { get; set; }
		[DataMember(Name = "Description", Order = 4, IsRequired = false, EmitDefaultValue = false)]
		public string Description { get; set; }

		public override string ToString()
		{
			return WorkKey + " [" + WorkName + "] (" + ServerRuleId + ")" + (ProjectId == null ? "" : " p:" + ProjectId) + (Description == null ? "" : " d:" + Description);
		}
	}
}
