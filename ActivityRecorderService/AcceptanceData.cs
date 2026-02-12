using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "AcceptanceData", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class AcceptanceData
	{
		[DataMember(Name = "Message", Order = 0, IsRequired = true, EmitDefaultValue = true)]
		public string Message { get; set; }

		[DataMember(Name = "AcceptedAt", Order = 1, IsRequired = true, EmitDefaultValue = true)]
		public DateTime? AcceptedAt { get; set; }
	}
}
