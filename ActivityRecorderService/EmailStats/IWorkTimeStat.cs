using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.Stats;

namespace Tct.ActivityRecorderService.EmailStats
{
	public interface IWorkTimeStat
	{
		string WorkName { get; }
		TimeSpan WorkTime { get; }
		DetailedWork DetailedWork { get; }
		TotalWorkTimeStat TotalWorkTimeStat { get; }
	}
}
