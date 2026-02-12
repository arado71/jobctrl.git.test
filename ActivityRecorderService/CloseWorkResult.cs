using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "CloseWorkResult", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum CloseWorkResult
	{
		[EnumMember]
		Ok = 0,
		[EnumMember]
		ReasonRequired = 1,
		[EnumMember]
		AlreadyClosed = 2,
		[EnumMember]
		UnknownError = 3,
	}
}
