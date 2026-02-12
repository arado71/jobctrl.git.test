using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderService.EmailStats;

namespace Tct.ActivityRecorderService.OnlineStats
{
	/// <summary>
	/// Class for calculating SimpleWorkTimeStats from data from OM and from DB.
	/// </summary>
	public class SimpleWorkTimeStatsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly OnlineStatsManager onlineStatsManager;
		private readonly ConcurrentDictionary<int, CachedDbStats> cachedStats = new ConcurrentDictionary<int, CachedDbStats>();

		public SimpleWorkTimeStatsManager(OnlineStatsManager onlineStatsManager)
		{
			ManagerCallbackInterval = ConfigManager.OnlineOldDataUpdateInterval;
			this.onlineStatsManager = onlineStatsManager;
		}

		public SimpleWorkTimeStats GetSimpleWorkTimeStats(int userId, DateTime desiredEndDate)
		{
			var sw = Stopwatch.StartNew();
			var omStats = onlineStatsManager.GetSimpleWorkTimeStatsFromOM(userId, desiredEndDate); //caculated in memory
			log.Debug("GetSimpleWorkTimeStatsFromOM userId: " + userId + " endDate: " + desiredEndDate + " finished in " + sw.Elapsed.ToTotalMillisecondsString() + " ms");
			if (omStats == null) return null; //user might be inactive //todo other cases ???? throw?
			var userStat = cachedStats.GetOrAdd(userId, i => new CachedDbStats(i, omStats.FromDate));
			if (userStat.GetIsExpired() || userStat.EndDate != omStats.FromDate)  //expired db data or intervals don't match (might be because of day change (on month/week start))
			{
				if (userStat.EndDate != omStats.FromDate) log.Info("GetSimpleWorkTimeStats db.EndDate " + userStat.EndDate + " and om.FromDate " + omStats.FromDate + " don't match");
				//store the new entry and avoid race
				userStat = cachedStats.AddOrUpdate(userId, new CachedDbStats(userId, omStats.FromDate),
					(k, old) => old.GetIsExpired() || old.EndDate != omStats.FromDate
						? new CachedDbStats(userId, omStats.FromDate)
						: old);
			}
			Debug.Assert(userStat.EndDate == omStats.FromDate);
			//intervals match we can sum
			SimpleWorkTimeStats dbStats;
			try
			{
				dbStats = userStat.Stats.Value; //this might be slow with DB access (or fast memory access)
				Debug.Assert(dbStats != null);
			}
			catch (Exception ex)
			{
				log.Warn("Cannot get SimpleWorkTimeStats from db userId: " + userId + " endDate: " + userStat.EndDate + " initialization failed", ex);
				((ICollection<KeyValuePair<int, CachedDbStats>>)cachedStats).Remove(new KeyValuePair<int, CachedDbStats>(userId, userStat)); //remove bad entry
				throw;
			}
			//reduce memory pressure so 'reuse' omStats object (dbStats is cached and musn't be modified)
			omStats.MergeWith(dbStats); //we have to return the sum of omStats and dbStats
			return omStats;
		}

		protected override void ManagerCallbackImpl()
		{
			log.Info("Clearing DB cache");
			//clear expired db data
			foreach (var cachedStat in cachedStats)
			{
				if (cachedStat.Value.GetIsExpired())
				{
					log.Debug("Clearing DB cache for user " + cachedStat.Key);
					((ICollection<KeyValuePair<int, CachedDbStats>>)cachedStats).Remove(cachedStat); //remove expired value
				}
			}
		}

		private class CachedDbStats //nothing should be changed in this class after creation (or it should be thread-safe)
		{
			public DateTime EndDate { get; private set; }
			public Lazy<SimpleWorkTimeStats> Stats { get; private set; }
			private readonly int created;
			private readonly int lifespan;

			public bool GetIsExpired()
			{
				return ((uint)(Environment.TickCount - created) > lifespan);
			}

			public CachedDbStats(int userId, DateTime endDate)
			{
				created = Environment.TickCount;
				lifespan = GetRandomInterval();
				EndDate = endDate;
				Stats = new Lazy<SimpleWorkTimeStats>(() => //load db data ondemand
					{
						var totalStats = StatsDbHelper.GetTotalWorkTimeByWorkIdForUser(userId, endDate); //slow db access (might also throw)
						log.Info("Loaded TotalWorkTimeStats for user " + userId + " until " + endDate);
						Debug.Assert(totalStats != null);
						var result = new SimpleWorkTimeStats()
						{
							FromDate = DateTime.MinValue,
							ToDate = endDate,
							UserId = userId,
							Stats = new Dictionary<int, SimpleWorkTimeStat>(),
						};
						foreach (var totalWorkTimeStat in totalStats)
						{
							result.Stats.Add(totalWorkTimeStat.Key,
								new SimpleWorkTimeStat()
								{
									WorkId = totalWorkTimeStat.Value.WorkId,
									TotalWorkTime = totalWorkTimeStat.Value.TotalWorkTime,
								});
						}
						return result;
					});
			}

			private static int GetRandomInterval()
			{
				return GetRandomIntervalFrom(ConfigManager.OnlineOldDataUpdateInterval);
			}

			private static int GetRandomIntervalFrom(int fixedInterval)
			{
				return fixedInterval / 2 + RandomHelper.Next(fixedInterval);
			}
		}
	}
}
