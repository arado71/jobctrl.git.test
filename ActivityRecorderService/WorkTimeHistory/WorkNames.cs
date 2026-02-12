using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.WorkTimeHistory
{
	[DataContract(Namespace = "http://jobctrl.com/WorkTimeHistory")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class WorkOrProjectName
	{
		[DataMember(Name = "Name", Order = 0, IsRequired = true, EmitDefaultValue = false)]
		public string Name { get; set; }
		[DataMember(Name = "Id", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public int? Id { get; set; }
		[DataMember(Name = "ProjectId", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public int? ProjectId { get; set; }
		[DataMember(Name = "ParentId", Order = 3, IsRequired = false, EmitDefaultValue = false)]
		public int? ParentId { get; set; }
		[DataMember(Name = "CategoryId", Order = 4, IsRequired = false, EmitDefaultValue = false)]
		public int? CategoryId { get; set; }
		[DataMember(Name = "ExtId", Order = 5, IsRequired = false, EmitDefaultValue = false)]
		public string ExtTaskId { get; set; }
	}

	[DataContract(Namespace = "http://jobctrl.com/WorkTimeHistory")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class WorkNames
	{
		[DataMember]
		public List<WorkOrProjectName> Names { get; set; }
	}
}