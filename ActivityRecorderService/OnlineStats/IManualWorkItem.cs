using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.OnlineStats
{
	public interface IManualWorkItem
	{
		int? WorkId { get; }
		ManualWorkItemTypeEnum ManualWorkItemTypeId { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
		byte? SourceId { get; }
		string Comment { get; }
	}
}
