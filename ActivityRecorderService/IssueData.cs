using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "IssueData", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class IssueData
	{
		[DataMember(Name = "IssueCode", Order = 0, IsRequired = true, EmitDefaultValue = true)]
		public string IssueCode { get; set; }
		[DataMember(Name = "Name", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public string Name { get; set; }
		[DataMember(Name = "Company", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public string Company { get; set; }
		[DataMember(Name = "State", Order = 3, IsRequired = true, EmitDefaultValue = true)]
		public int State { get; set; }
		[DataMember(Name = "UserId", Order = 4, IsRequired = true, EmitDefaultValue = true)]
		public int UserId { get; set; }
		[DataMember(Name = "Modified", Order = 5, IsRequired = true, EmitDefaultValue = true)]
		public DateTime Modified { get; set; }
		[DataMember(Name = "ModifiedByName", Order = 6, IsRequired = false, EmitDefaultValue = true)]
		public string ModifiedByName { get; set; }
		[DataMember(Name = "CreatedByName", Order = 7, IsRequired = false, EmitDefaultValue = true)]
		public string CreatedByName { get; set; }
		[DataMember(Name = "CreatedByUserId", Order = 8, IsRequired = false, EmitDefaultValue = true)]
		public int CreatedByUserId { get; set; }

	}
}
