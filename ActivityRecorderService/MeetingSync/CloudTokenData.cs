using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.MeetingSync
{
	[DataContract(Name = "CloudTokenData", Namespace = "http://jobctrl.com")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CloudTokenData
	{
		[DataMember]
		public string GoogleCalendarToken { get; set; }
	}
}
