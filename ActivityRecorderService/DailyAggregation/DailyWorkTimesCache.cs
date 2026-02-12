using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService.DailyAggregation
{
	/// <summary>
	/// Class for caching last valid version of daily worktime per user, in order to avoid db hit for every call (i.e. GetDailyWorkTimeStatsImpl)
	/// </summary>
	public class DailyWorkTimesCache
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly ConcurrentDictionary<int, long> lastValidVersionDict = new ConcurrentDictionary<int, long>();

		public List<DailyWorkTimeStats> GetDailyWorkTimeStats(int userId, long oldVersion)
		{
			long cachedVer;
			if (lastValidVersionDict.TryGetValue(userId, out cachedVer))
			{
				if (cachedVer < oldVersion)
				{
					log.ErrorAndFail("Client's version is bigger than the server's");
				}
				if (cachedVer <= oldVersion) return null; //no new valid versions
			}

			var res = GetDailyWorkTimeStatsImpl(userId, oldVersion); //get data from the db
			if (res.Count == 0) //no new data in the db
			{
				//if oldVersion would be hostile than sending a big oldVersion after server restart would cause every 'normal' query to hit the db
				//by writing 0 (instead of oldVersion) we could avoid this, but then every call would hit the db until we call UpdateLastValidVersionCache
				lastValidVersionDict.AddOrUpdate(userId, oldVersion, (uId, dictVer) => Math.Max(dictVer, oldVersion)); //next time don't hit the db
				return null;
			}

			var maxVer = cachedVer; //get max version but no linq for perf reasons
			foreach (var dailyWorkTimeStat in res)
			{
				var currVer = dailyWorkTimeStat.Version;
				maxVer = maxVer > currVer ? maxVer : currVer;
			}

			if (cachedVer < maxVer) //we have to update the cache if we have a newer version
			{
				lastValidVersionDict.AddOrUpdate(userId, maxVer, (uId, dictVer) => Math.Max(dictVer, maxVer));
			}

			return res;
		}

		private static List<DailyWorkTimeStats> GetDailyWorkTimeStatsImpl(int userId, long oldVersion)
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				return context.GetLatestDailyAggregateWorkTimeTablesForUser(userId, oldVersion).ToList();
			}
		}

		public void UpdateLastValidVersionCache()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				foreach (var lastValid in context.ExecuteQuery<UserVersion>("SELECT UserId, MAX(Version) AS Version FROM dbo.[AggregateDailyWorkTimes] WHERE IsValid = 1 GROUP BY UserId")
					//context.AggregateDailyWorkTimes
					//.Where(n => n.IsValid)
					//.GroupBy(n => n.UserId)
					//.Select(n => new { UserId = n.Key, Version = n.Max(v => v.Version) })
					)
				{
					long cachedVer;
					if (lastValidVersionDict.TryGetValue(lastValid.UserId, out cachedVer)) //we only care about users in the cache
					{
						long dbVer = lastValid.Version.ToLong();
						if (dbVer > cachedVer) //we have to update the cache
						{
							lastValidVersionDict.AddOrUpdate(lastValid.UserId, dbVer, (uId, dictVer) => Math.Max(dictVer, dbVer));
						}
					}
				}
			}
		}

		// ReSharper disable ClassNeverInstantiated.Local, UnusedAutoPropertyAccessor.Local
		private class UserVersion
		{
			public int UserId { get; set; }
			public Binary Version { get; set; }
		}
		// ReSharper restore ClassNeverInstantiated.Local, UnusedAutoPropertyAccessor.Local
	}
}
