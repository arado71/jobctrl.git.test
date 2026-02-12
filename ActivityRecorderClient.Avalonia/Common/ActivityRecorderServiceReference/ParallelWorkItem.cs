using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	partial class ParallelWorkItem : IWorkItem
	{
		public Guid Id { get; set; }
		public AssignData AssignData { get; set; }

		public bool HasWorkId { get { return true; } }

		public int GetWorkId()
		{
			return WorkId;
		}

		public void SetWorkId(int workId)
		{
			WorkId = workId;
		}
	}
}
