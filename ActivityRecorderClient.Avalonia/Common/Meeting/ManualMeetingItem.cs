using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using System.Runtime.Serialization;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.Meeting
{
	[DataContract]
	[KnownType(typeof(ManualMeetingItem))]
	public partial class ManualMeetingItem : IWorkItem
	{
		[DataMember]
		protected ManualMeetingData manualMeetingData;

		public ManualMeetingItem(ManualMeetingData data)
		{
			manualMeetingData = data;
		}

		[IgnoreDataMember]
		public ManualMeetingData ManualMeetingData { get { return manualMeetingData; } }

		[DataMember]
		public int UserId { get; set; }

		[DataMember]
		public Guid Id { get; set; }

		[IgnoreDataMember]
		public DateTime StartDate { get { return manualMeetingData.StartTime; } }

		[IgnoreDataMember]
		public DateTime EndDate { get { return manualMeetingData.EndTime; } }

		[DataMember]
		public TimeSpan NonAccountableDuration { get; set; }

		[DataMember]
		public AssignData AssignData { get; set; }

		[IgnoreDataMember]
		public bool HasWorkId { get { return manualMeetingData.WorkId != 0; } }

		[DataMember]
		public virtual bool IsPoppedUpAfterInactivity { get; set; }

		[DataMember]
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
