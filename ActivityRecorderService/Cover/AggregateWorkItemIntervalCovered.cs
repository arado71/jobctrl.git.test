using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService
{
	public class AggregateWorkItemIntervalCovered : IAggregateWorkItemIntervalCovered
	{
		public int WorkId { get; set; }
		public int UserId { get; set; }
		public int ComputerId { get; set; }
		public Guid PhaseId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
