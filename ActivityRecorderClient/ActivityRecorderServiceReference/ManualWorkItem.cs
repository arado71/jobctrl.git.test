using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	partial class ManualWorkItem : IWorkItem
	{
		public Guid Id { get; set; }
		public AssignData AssignData { get; set; }

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
