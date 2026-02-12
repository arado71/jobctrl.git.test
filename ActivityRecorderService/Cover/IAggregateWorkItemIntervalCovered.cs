using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.OnlineStats;

namespace Tct.ActivityRecorderService
{
	public interface IAggregateWorkItemIntervalCovered : IComputerWorkItem
	{
		int UserId { get; }
		int ComputerId { get; }
		Guid PhaseId { get; }
	}
}
