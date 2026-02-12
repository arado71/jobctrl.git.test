using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.OnlineStats;
using log4net;
using Tct.ActivityRecorderService.Stats;
using Tct.ActivityRecorderService.Voice;

namespace Tct.ActivityRecorderService.UsageStats
{
	public static class UsageStatsHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static DateTime lastDayFullStats = new DateTime(1900, 1, 1);

		public static void CommitUsageStatsToEcomm()
		{
			var sw = Stopwatch.StartNew();
			try
			{
				using (var context = new AggregateDataClassesDataContext())
				{
					context.CommandTimeout = 5 * 60; //5 mins
					context.CommitUsageStatsToEcomm();
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to execute CommitUsageStatsToEcomm", ex);
			}
			finally
			{
				log.Info("CommitUsageStatsToEcomm finished in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
			}
		}

		public static void GenerateUsageStats()
		{
			var sw = Stopwatch.StartNew();
			var isNeedFullStat = !lastDayFullStats.Equals(DateTime.Now.Date); // we use localdate, taking care on server maintenance
			try
			{
				var now = DateTime.UtcNow;
				var start = isNeedFullStat
					? new DateTime(1900, 1, 1)
					: now.AddDays(-ConfigManager.UsageStatsShortSummaryDays);
				var end = now.AddHours(2);
				//get users for which we have to calculate things
				//we calculate stats for users one by one to reduce memory pressure
				var userIds = StatsDbHelper.GetUsersForUsageStats(start, end);
				//we need TimeZone, StartOfDayOffset to calclulate start/end of days
				var userStats = StatsDbHelper.GetUserStatsInfo(null).ToLookup(n => n.Id);
				foreach (var userId in userIds)
				{
					var compIntervals = StatsDbHelper.GetAggregateWorkItemIntervalsForUserCovered(userId, start, end);
					var mobileIntervals = StatsDbHelper.GetMobileWorkItemsForUserCovered(userId, start, end);
					var manualIntervals = StatsDbHelper.GetManualWorkItemsForUserCovered(userId, start, end);
					var voxIntervals = StatsDbHelper.GetVoiceRecordingsForUser(userId, start, end);

					GenerateUsageStatsForUserImpl(userId, start, userStats[userId].FirstOrDefault(), compIntervals, mobileIntervals, manualIntervals, voxIntervals);
				}
				if (isNeedFullStat)
					lastDayFullStats = DateTime.Now.Date;
			}
			finally
			{
				log.InfoFormat("GenerateUsageStats({0}) finished in {1}ms", isNeedFullStat ? "full" : "partial", sw.Elapsed.ToTotalMillisecondsString());
			}
		}

		//legacy method used by tests
		internal static void GenerateUsageStatsImpl(
			ILookup<int, AggregateWorkItemInterval> compIntervals,
			ILookup<int, MobileWorkItem> mobileIntervals,
			ILookup<int, ManualWorkItem> manualIntervals,
			ILookup<int, VoiceRecording> voxIntervals,
			ILookup<int, UserStatInfo> userStats)
		{
			var allUsers = compIntervals.Select(n => n.Key)
				.Union(mobileIntervals.Select(n => n.Key))
				.Union(manualIntervals.Select(n => n.Key));

			foreach (var userId in allUsers)
			{
				GenerateUsageStatsForUserImpl(userId,
					new DateTime(1900, 1, 1),
					userStats[userId].FirstOrDefault(),
					compIntervals[userId],
					mobileIntervals[userId],
					manualIntervals[userId],
					voxIntervals[userId]);
			}
		}

		private static void GenerateUsageStatsForUserImpl(int userId, DateTime start, UserStatInfo userStat, IEnumerable<IComputerWorkItem> computerIntervals, IEnumerable<IMobileWorkItem> mobileIntervals, IEnumerable<IManualWorkItem> manualIntervals, IEnumerable<VoiceRecording> voxIntervals)
		{
			var dbFetch = TimeSpan.Zero;
			var memUpd = TimeSpan.Zero;
			var dbUpd = TimeSpan.Zero;
			var sw = Stopwatch.StartNew();
			try
			{
				if (userStat == null)
				{
					log.Verbose("No user stats (TimeZone, StartOfDayOffset) found for UserId " + userId);
					userStat = new UserStatInfo() { Id = userId, StartOfDayOffset = TimeSpan.FromHours(3), TimeZone = TimeZoneInfo.Local }; //omg hax
				}

				using (var context = new AggregateDataClassesDataContext())
				{
					//context.Connection.Open();
					//using (context.Transaction = context.Connection.BeginTransaction(IsolationLevel.Serializable))
					context.SetXactAbortOn();
					{
						var oldUsageStatsDict = context.UsageStats.Where(n => n.UserId == userId && n.StartDate >= start).ToDictionary(n => n.Id);
						dbFetch = sw.Elapsed;
						//we recalculate usually a short period after 'start' (all entries only once per day)
						var usageStatsWithoutTime = oldUsageStatsDict.Values.Select(n => new UsageStat()
																							{
																								LocalDate = n.LocalDate,
																								StartDate = n.StartDate,
																								EndDate = n.EndDate,
																								UserId = n.UserId,
																								Id = n.Id,
																							});
						var builder = new UsageStatsBuilder(usageStatsWithoutTime, userStat);
						builder.AddAggregateWorkItemIntervals(computerIntervals);
						builder.AddMobileWorkItems(mobileIntervals);
						builder.AddManualWorkItems(manualIntervals);
						builder.AddVoiceRecordings(voxIntervals);

						//merge changes
						var now = DateTime.UtcNow;
						foreach (var newUsageStat in builder.GetUsageStats().Where(n => n.StartDate >= start))
						{
							if (newUsageStat.Id == 0) //new entry
							{
								newUsageStat.CreateDate = now;
								newUsageStat.UpdateDate = now;
								context.UsageStats.InsertOnSubmit(newUsageStat);
							}
							else
							{
								UsageStat oldUsageStat;
								if (!oldUsageStatsDict.TryGetValue(newUsageStat.Id, out oldUsageStat))
								{
									log.Error("Cannot find new UsageStats Id " + newUsageStat.Id + " for UserId " + newUsageStat.UserId +
											  " LocalDate " + newUsageStat.LocalDate + " in old stats");
									// Debug.Fail("Cannot find new UseageStats Id");
									continue;
								}
								if (oldUsageStat.ComputerWorkTime != newUsageStat.ComputerWorkTime)
								{
									if (oldUsageStat.ComputerWorkTime > newUsageStat.ComputerWorkTime)
									{
										log.Warn("New UsageStats has less computer time than before Id " + newUsageStat.Id + " UserId " +
												  newUsageStat.UserId + " LocalDate " + newUsageStat.LocalDate + " old: " + oldUsageStat.ComputerWorkTime + " new: " + newUsageStat.ComputerWorkTime);
										// Debug.Fail("New UsageStats has less computer time than before");
									}
									else //otherwise update stats
									{
										oldUsageStat.UpdateDate = now;
										oldUsageStat.ComputerWorkTime = newUsageStat.ComputerWorkTime;
										oldUsageStat.IsAcked = false;
									}
								}
								if (oldUsageStat.MobileWorkTime != newUsageStat.MobileWorkTime)
								{
									if (oldUsageStat.MobileWorkTime > newUsageStat.MobileWorkTime)
									{
										log.Warn("New UsageStats has less mobile time than before Id " + newUsageStat.Id + " UserId " +
												  newUsageStat.UserId + " LocalDate " + newUsageStat.LocalDate);
										// Debug.Fail("New UsageStats has less mobile time than before");
									}
									else //otherwise update stats
									{
										oldUsageStat.UpdateDate = now;
										oldUsageStat.MobileWorkTime = newUsageStat.MobileWorkTime;
										oldUsageStat.IsAcked = false;
									}
								}
								if (oldUsageStat.ManuallyAddedWorkTime != newUsageStat.ManuallyAddedWorkTime)
								{
									if (oldUsageStat.ManuallyAddedWorkTime > newUsageStat.ManuallyAddedWorkTime)
									{
										log.Verbose("New UsageStats has less manual time than before Id " + newUsageStat.Id + " UserId " +
												  newUsageStat.UserId + " LocalDate " + newUsageStat.LocalDate);
									}
									else //otherwise update stats
									{
										oldUsageStat.UpdateDate = now;
										oldUsageStat.ManuallyAddedWorkTime = newUsageStat.ManuallyAddedWorkTime;
										oldUsageStat.IsAcked = false;
									}
								}
								if (oldUsageStat.UsedMobile != newUsageStat.UsedMobile)
								{
									if (!newUsageStat.UsedMobile)
									{
										log.Verbose("New UsageStats becomes cleared UsedMobile Id " + newUsageStat.Id + " UserId " +
										          newUsageStat.UserId + " LocalDate " + newUsageStat.LocalDate);
									}
									else //otherwise update stats
									{
										oldUsageStat.UpdateDate = now;
										oldUsageStat.UsedMobile = newUsageStat.UsedMobile;
										oldUsageStat.IsAcked = false;
									}
								}
								if (oldUsageStat.UsedBeaconClient != newUsageStat.UsedBeaconClient)
								{
									if (!newUsageStat.UsedBeaconClient)
									{
										log.Verbose("New UsageStats becomes cleared UsedBeaconClient Id " + newUsageStat.Id + " UserId " +
												  newUsageStat.UserId + " LocalDate " + newUsageStat.LocalDate);
									}
									else //otherwise update stats
									{
										oldUsageStat.UpdateDate = now;
										oldUsageStat.UsedBeaconClient = newUsageStat.UsedBeaconClient;
										oldUsageStat.IsAcked = false;
									}
								}
								if (oldUsageStat.UsedVoxCtrl != newUsageStat.UsedVoxCtrl)
								{
									if (!newUsageStat.UsedVoxCtrl)
									{
										log.Verbose("New UsageStats becomes cleared UsedVoxCtrl Id " + newUsageStat.Id + " UserId " +
												  newUsageStat.UserId + " LocalDate " + newUsageStat.LocalDate);
									}
									else //otherwise update stats
									{
										oldUsageStat.UpdateDate = now;
										oldUsageStat.UsedVoxCtrl = newUsageStat.UsedVoxCtrl;
										oldUsageStat.IsAcked = false;
									}
								}
							}
						}
						memUpd = sw.Elapsed;
						context.SubmitChanges(); //this is one transaction with optimistic concurrency checking which is enough atm.
						dbUpd = sw.Elapsed;
						//context.Transaction.Commit();
						//log.Info("GenerateUsageStats transaction completed");
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to update UsageStats for UserId " + userId, ex); //swallow error maybe we can process it next time
			}
			finally
			{
				log.Verbose("Updating UsageStats for UserId " + userId + " finished in " + sw.Elapsed.ToTotalMillisecondsString() + "ms "
					+ "(DBFetch/MemUpd/DBUpd: " + (dbFetch < TimeSpan.Zero ? "-" : dbFetch.ToTotalMillisecondsString())
					+ " / " + (memUpd - dbFetch < TimeSpan.Zero ? "-" : (memUpd - dbFetch).ToTotalMillisecondsString())
					+ " / " + (dbUpd - memUpd < TimeSpan.Zero ? "-" : (dbUpd - memUpd).ToTotalMillisecondsString())
					+ " )");
			}
		}
	}
}
