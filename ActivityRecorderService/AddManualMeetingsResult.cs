using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "AddManualMeetingsResult", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum AddManualMeetingsResult
	{
		[EnumMember]
		OK = 0,
		[EnumMember]
		UnknownError = 1,
		[EnumMember]
		AuthCodeNotValid = 2,
		[EnumMember]
		AddManualMeetingError = 3,
	}
}
