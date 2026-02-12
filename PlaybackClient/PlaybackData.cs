using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using PlaybackClient.ActivityRecorderServiceReference;

namespace PlaybackClient
{
	[Serializable]
	[DataContract(Namespace = "http://jobctrl.com/Playback")]
	public class PlaybackData
	{
		[DataMember]
		public int UserId { get; set; }
		[DataMember]
		public DateTime StartDate { get; set; }
		[DataMember]
		public DateTime EndDate { get; set; }
		[DataMember]
		public List<WorkItem> WorkItems { get; set; }
		[DataMember]
		public List<ManualWorkItem> ManualWorkItems { get; set; }
		[DataMember]
		public List<MobileWorkItem> MobileWorkItems { get; set; }
		[DataMember]
		public List<MobileClientLocation> MobileLocations { get; set; }
	}
}
