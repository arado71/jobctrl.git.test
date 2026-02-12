using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.Kicks
{
	[DataContract(Name = "KickResult", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum KickResult
	{
		[EnumMember]
		Ok = 0,
		[EnumMember]
		AlreadyOffline = 1,
		[EnumMember]
		UnknownError = 2,
	}
}
