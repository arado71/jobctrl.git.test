using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.OnlineStats
{
	public interface IComputerWorkItem
	{
		int WorkId { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
	}
}
