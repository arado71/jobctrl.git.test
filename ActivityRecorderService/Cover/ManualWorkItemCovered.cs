using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.OnlineStats;

namespace Tct.ActivityRecorderService
{
	public class ManualWorkItemCovered : IManualWorkItem
	{
		public int UserId { get; set; }
		public int? WorkId { get; set; }
		public ManualWorkItemTypeEnum ManualWorkItemTypeId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public byte? SourceId { get; set; }
		public string Comment { get; set; }
	}
}
