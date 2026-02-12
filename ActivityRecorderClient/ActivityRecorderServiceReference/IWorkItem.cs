using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public interface IWorkItem : IUploadItem
	{
		DateTime EndDate { get; }
		AssignData AssignData { get; set; }
		bool HasWorkId { get; }
		int GetWorkId();
		void SetWorkId(int workId);
	}
}
