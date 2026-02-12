using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "AggregateCollectedItems", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class AggregateCollectedItems
	{
		[DataMember]
		public int UserId { get; set; }

		[DataMember]
		public int ComputerId { get; set; }

		[DataMember]
		public Dictionary<int, string> KeyLookup { get; set; }

		[DataMember]
		public Dictionary<int, string> ValueLookup { get; set; }

		[DataMember]
		public List<CollectedItemIdOnly> Items { get; set; }

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public DateTime? CreateDate { get; set; }
	}

	[DataContract(Name = "CollectedItemIdOnly", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CollectedItemIdOnly
	{
		[DataMember]
		public DateTime CreateDate { get; set; }

		[DataMember]
		public Dictionary<int, int?> CapturedValues { get; set; }
	}
}
