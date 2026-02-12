using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService.DailyAggregation
{
	/// <summary>
	/// Class for periodically recalculate and update invalid daily work time stats for days before DateTime.UtcNow.Date
	/// </summary>
	public class DailyWorkTimesManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly DailyWorkTimesCache dailyWorkTimesCache = new DailyWorkTimesCache(); //aggregated db data
		private readonly TodaysWorkTimeCache todaysWorkTimeCache = new TodaysWorkTimeCache(ConfigManager.AggregateDailyTimesInterval); //not yet aggregated db data

		public DailyWorkTimesManager()
			: base(log)
		{
			ManagerCallbackInterval = ConfigManager.AggregateDailyTimesInterval;
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				DailyWorkTimesHelper.Aggregate();
				dailyWorkTimesCache.UpdateLastValidVersionCache();
			}
			catch (Exception ex)
			{
				log.Error("Daily worktime aggregation failed", ex);
			}
		}

		public List<DailyWorkTimeStats> GetDailyWorkTimeStats(int userId, long oldVersion)
		{
			var curr = todaysWorkTimeCache.GetDailyWorkTimeStatsForUser(userId);
			var aggr = dailyWorkTimesCache.GetDailyWorkTimeStats(userId, oldVersion);
			if (aggr == null) aggr = new List<DailyWorkTimeStats>();

			var found = false; //no linq for performance reasons
			for (int i = 0; i < aggr.Count; i++)
			{
				if (aggr[i].Day == curr.Day)
				{
					found = true; //this should be rare. We might have a later thus better version in curr but we don't want to mess with Version logic so we won't use it
				}
			}

			if (!found)
			{
				aggr.Add(curr);
			}

			return aggr;
		}
	}
}
