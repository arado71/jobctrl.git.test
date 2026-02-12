using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient.Stats
{
	[DataContract]
	public enum WorkType
	{
		[EnumMember(Value = "Computer")]
		Computer,
		[EnumMember(Value = "Manual")]
		Manual,
		[EnumMember(Value = "Meeting")]
		Meeting,
	}
}
