using System;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Meeting
{
	[Serializable]
	public class PostponedMeetingItem : ManualMeetingItem
	{
		public PostponedMeetingItem(ManualMeetingData data)
			: base(data)
		{
		}

		// override member is needed for backward compatibility (previously stored postponed meeting data)
		public override bool IsPoppedUpAfterInactivity { get ; set ; }
	}
}
