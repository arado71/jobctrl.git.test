using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient.Menu
{
	[DataContract]
	public class LearnedUserWorkSwitch
	{
		[DataMember]
		public int WorkId { get; set; }
		[DataMember]
		public int?[] ListPositions { get; set; }
		[DataMember]
		public DateTime Date { get; set; }
	}
}
