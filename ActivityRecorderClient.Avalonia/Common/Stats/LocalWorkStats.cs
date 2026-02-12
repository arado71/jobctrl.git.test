using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient.Stats
{
	[DataContract]
	public partial class LocalWorkStats
	{
		[DataMember]
		public Dictionary<WorkType, Dictionary<int, List<Interval>>> WorkIntervalsByTypeByWorkId { get; private set; } = new();
	}
}
