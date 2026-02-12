using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlookMeetingCaptureService
{
	[Serializable]
	public class MeetingStore
	{
		public Dictionary<string, Dictionary<string, MeetingData>> MeetingDatas { get; set; }
		public Dictionary<string, Dictionary<string, DateTime>> ItemFirstSeen { get; set; }
		public Dictionary<string, DateTime> LastModificationDates { get; set; }
		public Dictionary<string, Dictionary<string, DateTime>> ItemsToBeDeleted { get; set; }
	}
}
