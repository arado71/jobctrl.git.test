using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "CompositeMapping", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CompositeMapping
	{
		[DataMember(Name = "ChildrenByKey", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, CompositeMapping> ChildrenByKey { get; set; }

		[DataMember(Name = "WorkIdByKey", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, int> WorkIdByKey { get; set; }
	}
}
