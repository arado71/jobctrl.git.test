using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
#pragma warning disable 0169
	partial class WorkItem : IWorkItem
	{
		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public AssignData AssignData { get; set; }

		public bool HasWorkId
		{
			get { return true; }
		}

		public int GetWorkId()
		{
			return WorkId;
		}

		public void SetWorkId(int workId)
		{
			WorkId = workId;
		}

	}
#pragma warning restore 0169
}
