using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "AssignCompositeData", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class AssignCompositeData
	{
		[DataMember(Name = "WorkKey", Order = 0, IsRequired = true, EmitDefaultValue = true)]
		public string WorkKey { get; set; } //with suffix
		[DataMember(Name = "ProjectKeys", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public string[] ProjectKeys { get; set; } //suffix was added after every projectkey

		[DataMember(Name = "ServerRuleId", Order = 2, IsRequired = true, EmitDefaultValue = true)]
		public int ServerRuleId { get; set; }
		[DataMember(Name = "WorkName", Order = 3, IsRequired = false, EmitDefaultValue = false)]
		public string WorkName { get; set; }
		[DataMember(Name = "Description", Order = 4, IsRequired = false, EmitDefaultValue = false)]
		public string Description { get; set; }
		//[DataMember(Name = "ProjectId", Order = 4, IsRequired = false, EmitDefaultValue = false)]
		//public int? ProjectId { get; set; }

		public override string ToString()
		{
			return (ProjectKeys == null ? "" : (string.Join("\\", ProjectKeys) + "\\")) + WorkKey + " [" + WorkName + "] (" + ServerRuleId + ")" + (Description == null ? "" : " d:" + Description);
		}
	}
}
