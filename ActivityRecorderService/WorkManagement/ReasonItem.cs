using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.WorkManagement
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ReasonItem
	{
		[DataMember]
		public int UserId { get; set; }
		[DataMember]
		public int WorkId { get; set; }
		[DataMember]
		public DateTime StartDate { get; set; }
		[DataMember]
		public string Reason { get; set; }
		[DataMember]
		public int? ReasonItemId { get; set; }
	}
}
