using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using System.Runtime.Serialization;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.Meeting
{
	[Serializable]
	[KnownType(typeof(ManualMeetingItem))]
	public class ManualMeetingItem : IWorkItem
	{
		private ManualMeetingData manualMeetingData;

		public ManualMeetingItem(ManualMeetingData data)
		{
			manualMeetingData = data;
		}

		public ManualMeetingData ManualMeetingData { get { return manualMeetingData; } }

		public int UserId { get; set; }

		public Guid Id { get; set; }

		public DateTime StartDate { get { return manualMeetingData.StartTime; } }

		public DateTime EndDate { get { return manualMeetingData.EndTime; } }

		public TimeSpan NonAccountableDuration { get; set; }

		public AssignData AssignData { get; set; }

		public bool HasWorkId { get { return manualMeetingData.WorkId != 0; } }

		public virtual bool IsPoppedUpAfterInactivity { get; set; }

		public bool ResumeWorkOnClose { get; set; }

		public int GetWorkId()
		{
			return manualMeetingData.WorkId;
		}

		public void SetWorkId(int workId)
		{
			if (!HasWorkId) return;
			manualMeetingData.WorkId = workId;
		}
	}
}
