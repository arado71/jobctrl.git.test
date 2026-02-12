using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "CompositeMapping", Namespace = "http://jobctrl.com/")]
	internal class CompositeMapping
	{
		[DataMember(Name = "ChildrenByKey", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, CompositeMapping> ChildrenByKey { get; set; }

		[DataMember(Name = "WorkIdByKey", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, int> WorkIdByKey { get; set; }
	}
}
