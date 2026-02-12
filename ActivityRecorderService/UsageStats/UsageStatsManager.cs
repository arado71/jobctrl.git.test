using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderService.Common;

namespace Tct.ActivityRecorderService.UsageStats
{
	public class UsageStatsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public UsageStatsManager()
			: base(log)
		{
			ManagerCallbackInterval = ConfigManager.UsageStatsUpdateInterval;
		}

		protected override void ManagerCallbackImpl()
		{
			UsageStatsHelper.GenerateUsageStats();
			UsageStatsHelper.CommitUsageStatsToEcomm();
			HeapHelper.CompactLargeObjectHeap(log);
		}
	}
}
