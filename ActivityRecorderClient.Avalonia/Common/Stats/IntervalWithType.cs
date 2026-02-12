using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient.Stats
{
	[DataContract]
	public class IntervalWithType : Interval
	{
		[DataMember]
		public WorkType WorkType { get; set; }
	}
}
