using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public interface IUploadItem
	{
		int UserId { get; }
		Guid Id { get; }
		DateTime StartDate { get; }
	}
}
