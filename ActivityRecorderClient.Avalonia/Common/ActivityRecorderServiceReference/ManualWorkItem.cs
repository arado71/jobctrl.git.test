using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	partial class ManualWorkItem : IWorkItem
	{
		[DataMember]
		public Guid Id { get; set; }
		[DataMember]
		public AssignData AssignData { get; set; }

		[IgnoreDataMember]
		public bool HasWorkId { get { return WorkId.HasValue; } }

		public int GetWorkId()
		{
			return WorkId.Value;
		}

		public void SetWorkId(int workId)
		{
			if (!HasWorkId) return;
			WorkId = workId;
		}
	}
}
