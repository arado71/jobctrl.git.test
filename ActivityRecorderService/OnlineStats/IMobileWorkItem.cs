using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.OnlineStats
{
	public interface IMobileWorkItem
	{
		int WorkId { get; }
		long Imei { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
		bool IsBeacon { get; }
	}
}
