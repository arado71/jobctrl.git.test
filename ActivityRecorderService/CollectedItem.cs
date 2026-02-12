using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "CollectedItem", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CollectedItem
	{
		[DataMember]
		public int UserId { get; set; }

		[DataMember]
		public int ComputerId { get; set; }

		[DataMember]
		public DateTime CreateDate { get; set; }

		[DataMember]
		public Dictionary<string, string> CapturedValues { get; set; }
	}
}
