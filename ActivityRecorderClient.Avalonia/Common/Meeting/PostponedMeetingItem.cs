using System;
using System.Runtime.Serialization;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Meeting
{
	[DataContract]
	public partial class PostponedMeetingItem : ManualMeetingItem
	{
		public PostponedMeetingItem(ManualMeetingData data)
			: base(data)
		{
		}

		// override member is needed for backward compatibility (previously stored postponed meeting data)
		[DataMember]
		public override bool IsPoppedUpAfterInactivity { get ; set ; }
	}
}
