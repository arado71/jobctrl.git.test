using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum UserAccessLevel
	{
		[EnumMember]
		Undefined = 0,
		/* Registrator */
		[EnumMember]
		Reg = 1,
		/* Administrator */
		[EnumMember]
		Adm = 2,
		/* Supervisor */
		[EnumMember]
		Spv = 3,
		/* Worker */
		[EnumMember]
		Wrk = 4,
	}
}
