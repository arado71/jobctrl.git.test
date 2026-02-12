using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.Cover
{
	public class AllWorkItemsCovered
	{
		public List<AggregateWorkItemIntervalCovered> AggregateWorkItems { get; set; }
		public List<ManualWorkItemCovered> ManualWorkItems { get; set; }
		public List<MobileWorkItemCovered> MobileWorkItems { get; set; }
	}
}
