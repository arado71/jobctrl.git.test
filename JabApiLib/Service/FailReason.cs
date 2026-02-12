using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.Java.Service
{
	[DataContract(Namespace = "http://jobctrl.com/java", Name = "FailReason")]
	public class FailReason
	{
		[DataMember]
		public FailReasonType Type { get; set; }

		[DataMember]
		public string Message { get; set; }
	}
}
