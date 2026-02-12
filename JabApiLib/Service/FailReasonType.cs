using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.Java.Service
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	[DataContract(Namespace = "http://jobctrl.com/java", Name = "FailReasonType")]
	public enum FailReasonType
	{
		[EnumMember]
		CaptureImpossible,
		[EnumMember]
		UnexpectedException
	}
}
