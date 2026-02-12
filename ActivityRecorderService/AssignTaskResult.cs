using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "AssignTaskResult", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum AssignTaskResult
	{
		[EnumMember]
		Ok = 0,
		[EnumMember]
		AccessDenied = 1,
		[EnumMember]
		UnknownError = 2,
	}
}
