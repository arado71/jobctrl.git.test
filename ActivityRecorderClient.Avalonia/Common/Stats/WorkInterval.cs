using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient.Stats
{
	[DataContract]
	public class WorkInterval : IntervalWithType
	{
		[DataMember]
		public int WorkId { get; set; }
	}
}
