using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.DailyAggregation
{
	/// <summary>
	/// Class for caching daily worktime stats.
	/// </summary>
	public class TodaysWorkTimeCache
	{
		private readonly Func<int, DailyWorkTimeStats> getDailyWorkTimeStatsForUser;

		public TodaysWorkTimeCache(int cacheMaxAgeInMs)
		{
			getDailyWorkTimeStatsForUser = Caching.CachedFunc.CreateThreadSafe<int, DailyWorkTimeStats>(
				GetDailyWorkTimeStatsForUserImpl,
				TimeSpan.FromMilliseconds(cacheMaxAgeInMs));
		}

		private static DailyWorkTimeStats GetDailyWorkTimeStatsForUserImpl(int userId)
		{
			var now = DateTime.UtcNow.AddMinutes(-5); //since client's clock could differ from server's 'make sure' the client clock is after this instant
			var res = DailyWorkTimesHelper.GetDailyWorkTimeStatsFromDb(userId, now.Date, now.TimeOfDay);
			return res;
		}

		public DailyWorkTimeStats GetDailyWorkTimeStatsForUser(int userId)
		{
			return getDailyWorkTimeStatsForUser(userId);
		}
	}
}
