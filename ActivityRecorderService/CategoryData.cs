using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CategoryData
	{
		[DataMember(Name = "Id", Order = 0, IsRequired = true, EmitDefaultValue = false)]
		public int Id { get; set; }
		[DataMember(Name = "Name", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public string Name { get; set; }
	}
}
