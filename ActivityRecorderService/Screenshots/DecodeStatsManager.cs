using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService.Screenshots
{
	public class DecodeStatsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private int numMaster;
		private int numNonMaster;
		private int numMasterNeeded;
		private long numAllMaster;
		private long numAllNonMaster;
		private long numAllMasterNeeded;

		public DecodeStatsManager()
			: base(log)
		{
			ManagerCallbackInterval = (int)TimeSpan.FromMinutes(20).TotalMilliseconds;
		}

		public void IncrementMaster()
		{
			Interlocked.Increment(ref numMaster);
		}

		public void IncrementNonMaster()
		{
			Interlocked.Increment(ref numNonMaster);
		}

		public void IncrementMasterNeeded()
		{
			Interlocked.Increment(ref numMasterNeeded);
		}

		protected override void ManagerCallbackImpl()
		{
			var currMaster = Interlocked.Exchange(ref numMaster, 0);
			var currNonMaster = Interlocked.Exchange(ref numNonMaster, 0);
			var currMasterNeeded = Interlocked.Exchange(ref numMasterNeeded, 0);
			var allMaster = Interlocked.Add(ref numAllMaster, currMaster);
			var allNonMaster = Interlocked.Add(ref numAllNonMaster, currNonMaster);
			var allMasterNeeded = Interlocked.Add(ref numAllMasterNeeded, currMasterNeeded);

			var sum = currMaster + currNonMaster + currMasterNeeded;
			var allSum = allMaster + allNonMaster + allMasterNeeded;

			var currMasterPct = (sum == 0 ? 0d : currMaster / (double)sum).ToString("0.00%");
			var currNonMasterPct = (sum == 0 ? 0d : currNonMaster / (double)sum).ToString("0.00%");
			var currMasterNeededPct = (sum == 0 ? 0d : currMasterNeeded / (double)sum).ToString("0.00%");
			var allMasterPct = (allSum == 0 ? 0d : allMaster / (double)allSum).ToString("0.00%");
			var allNonMasterPct = (allSum == 0 ? 0d : allNonMaster / (double)allSum).ToString("0.00%");
			var allMasterNeededPct = (allSum == 0 ? 0d : allMasterNeeded / (double)allSum).ToString("0.00%");

			log.Info("Master/NonM/NeedM: " + currMaster + "/" + currNonMaster + "/" + currMasterNeeded + " (" + currMasterPct + " / " + currNonMasterPct + " / " + currMasterNeededPct + ")");
			log.Info("All Master/NonM/NeedM: " + allMaster + "/" + allNonMaster + "/" + allMasterNeeded + " (" + allMasterPct + " / " + allNonMasterPct + " / " + allMasterNeededPct + ")");
		}
	}
}
