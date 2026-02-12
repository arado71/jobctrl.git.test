using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.OnlineStats;

namespace Tct.ActivityRecorderService
{
	public class MobileWorkItemCovered : IMobileWorkItem
	{
		public int UserId { get; set; }
		public int WorkId { get; set; }
		public long Imei { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public bool IsBeacon { get; set; }
	}
}
