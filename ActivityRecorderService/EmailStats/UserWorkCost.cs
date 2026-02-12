using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.Stats;

namespace Tct.ActivityRecorderService.EmailStats
{
	/// <summary>
	/// Unique values for a userId-workId combination (except names)
	/// </summary>
	public class UserWorkCost : WorkTimeAndCost, IWorkTimeStat
	{
		public int UserId { get; set; }
		public string UserName { get; set; }
		public int WorkId { get; set; }
		public string WorkName { get; set; }
		public string WorkShortName { get; set; }
		public DetailedWork DetailedWork { get; set; }
		public TotalWorkTimeStat TotalWorkTimeStat { get; set; }
	}
}
