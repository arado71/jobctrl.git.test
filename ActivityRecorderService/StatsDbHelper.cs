using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dapper;
using log4net;
using Reporter.Interfaces;
using Reporter.Model;
using Tct.ActivityRecorderService.Caching.Works;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.Stats;
using Tct.ActivityRecorderService.Voice;

namespace Tct.ActivityRecorderService
{
	public static class StatsDbHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly TimeSpan maxWorkItemLength = TimeSpan.FromMinutes(10);
		private static readonly TimeSpan maxAggrWorkItemLength = TimeSpan.FromDays(7); //hax nothing enfoces this atm.
		private static readonly TimeSpan maxManualWorkItemLength = TimeSpan.FromDays(2); //hax nothing enfoces this atm.
		private static readonly TimeSpan maxIvrWorkItemLength = TimeSpan.FromDays(2); //hax nothing enfoces this atm.
		private static readonly TimeSpan maxMobileWorkItemLength = TimeSpan.FromDays(2); //hax nothing enfoces this atm.
		private static readonly TimeSpan cacheMaxAge = TimeSpan.FromHours(1);

		private static Func<KeyValuePair<int, DateTime>, Dictionary<int, TotalWorkTimeStat>> getCachedTotalWorkTimeStat =
			Caching.CachedFunc.CreateThreadSafe<KeyValuePair<int, DateTime>, Dictionary<int, TotalWorkTimeStat>>(
				key => GetTotalWorkTimeByWorkIdForUser(key.Key, key.Value), cacheMaxAge);

		private static Func<int, Dictionary<int, DetailedWork>> getCachedDetailedWorkStat =
			Caching.CachedFunc.CreateThreadSafe<int, Dictionary<int, DetailedWork>>(GetDetailedWorkForUser, cacheMaxAge);

		public static List<WorkItem> GetWorkItemsForUser(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				//not accurate at the beginning and at the end of the interval
				var result = conn.Query<WorkItem>(
					"SELECT [Id], [WorkId], [PhaseId], [StartDate], [EndDate], [ReceiveDate], [UserId], [GroupId], [CompanyId], [ComputerId], [MouseActivity], [KeyboardActivity], [IsRemoteDesktop], [IsVirtualMachine]"
					+ " FROM [dbo].[WorkItems] WHERE"
					+ " @MinStartDate <= [StartDate]" //no index on EndDate
					+ " AND [StartDate] < @EndDate"
					//+ " AND @StartDate < [EndDate]"
					+ " AND [UserId] = @UserId",
					new
					{
						UserId = userId,
						MinStartDate = startDate - maxWorkItemLength,
						//StartDate = startDate,
						EndDate = endDate
					},
					commandTimeout: 180) //3mins
					.EnsureList();

				result.RemoveAll(n => !(startDate < n.EndDate && n.StartDate < endDate));

				log.Debug("Loaded " + result.Count.ToInvariantString() + " WorkItems for user " + userId.ToInvariantString() + " between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}

		public static ILookup<int, WorkItem> GetWorkItemsByUser(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				//not accurate at the beginning and at the end of the interval
				var allItems = conn.Query<WorkItem>(
					"SELECT [Id], [WorkId], [PhaseId], [StartDate], [EndDate], [ReceiveDate], [UserId], [GroupId], [CompanyId], [ComputerId], [MouseActivity], [KeyboardActivity], [IsRemoteDesktop], [IsVirtualMachine]"
					+ " FROM [dbo].[WorkItems] WHERE"
					+ " @MinStartDate <= [StartDate]" //no index on EndDate
					+ " AND [StartDate] < @EndDate"
					//+ " AND @StartDate < [EndDate]"
					,
					new
					{
						MinStartDate = startDate - maxWorkItemLength,
						//StartDate = startDate,
						EndDate = endDate
					},
					commandTimeout: 180) //3mins
					.EnsureList()
					.ToLookup(n => n.UserId);

				log.Debug("Loaded WorkItems between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " for " + allItems.Count + " users in " + sw.ToTotalMillisecondsString() + "ms");
				return allItems;
			}
		}

		private static string GetFormattedProcessName(string processName)
		{
			switch (processName)
			{
				case "Locked":
					return EmailStats.EmailStats.LockedComputer;
				case "Idle":
					return EmailStats.EmailStats.UnknownProcess;
				default:
					return processName;
			}
		}

		public static ILookup<int, ManualWorkItem> GetManualWorkItemsByUser(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ObjectTrackingEnabled = false;

				//Get all items that are in the given interval 
				var allItems = context.ManualWorkItems
					.Where(n => startDate < n.EndDate)
					.Where(n => startDate - maxManualWorkItemLength <= n.StartDate) //no index on EndDate
					.Where(n => n.StartDate < endDate)
					//.Where(n =>
					//       startDate <= n.StartDate && n.StartDate < endDate
					//    || startDate < n.EndDate && n.EndDate <= endDate
					//    || n.StartDate < startDate && endDate < n.EndDate)
					.ToList();
				//don't exceed the given interval
				//foreach (var item in allItems)
				//{
				//    if (item.StartDate < startDate) item.StartDate = startDate;
				//    if (item.EndDate > endDate) item.EndDate = endDate;
				//}
				log.Debug("Loaded " + allItems.Count + " ManualWorkItems in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");

				return allItems.ToLookup(n => n.UserId);
			}
		}

		public static List<ManualWorkItem> GetManualWorkItemsForUser(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var context = new ManualDataClassesDataContext())
			{
				context.ObjectTrackingEnabled = false;

				var result = context.ManualWorkItems
					.Where(n => startDate < n.EndDate)
					.Where(n => startDate - maxManualWorkItemLength <= n.StartDate) //no index on EndDate
					.Where(n => n.StartDate < endDate)
					.Where(n => n.UserId == userId)
					.ToList();

				log.Debug("Loaded " + result.Count + " ManualWorkItems for user " + userId + " between " + startDate + " and " + endDate + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}

		public static ILookup<int, AggregateWorkItemInterval> GetAggregateWorkItemIntervalsByUser(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			ILookup<int, AggregateWorkItemInterval> stats;
			using (var context = new AggregateDataClassesDataContext())
			{
				context.CommandTimeout = 300; //5mins
				context.ObjectTrackingEnabled = false;

				stats = context.AggregateWorkItemIntervals
					.Where(n => startDate < n.EndDate)
					.Where(n => startDate - maxAggrWorkItemLength <= n.StartDate) //no index on EndDate
					.Where(n => n.StartDate < endDate)
					.ToLookup(n => n.UserId);
			}
			log.Debug("Loaded " + stats.Count + " AggregateWorkItemIntervals in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return stats;
		}

		public static ILookup<int, AggregateWorkItemIntervalCovered> GetAggregateWorkItemIntervalsByUserCovered(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			ILookup<int, AggregateWorkItemIntervalCovered> stats;
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				stats = conn.Query<AggregateWorkItemIntervalCovered>(
					"SELECT [UserId], [WorkId], [StartDate], [EndDate], [ComputerId], [PhaseId] FROM [dbo].[AggregateWorkItemIntervals] WHERE"
					+ " @MinStartDate <= [StartDate]" //no usable index on EndDate
					+ " AND [StartDate] < @EndDate"
					+ " AND @StartDate < DATEADD(ms, 0, [EndDate])", //don't use index on EndDate
					new
					{
						MinStartDate = startDate - maxAggrWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					},
					commandTimeout: 300) // 5mins
					.ToLookup(n => n.UserId);
			}
			log.Debug("Loaded " + stats.Count.ToInvariantString() + " AggregateWorkItemIntervals covered between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
			return stats;
		}

		public static List<AggregateWorkItemIntervalCovered> GetAggregateWorkItemIntervalsForUserCovered(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<AggregateWorkItemIntervalCovered>(
					"SELECT [UserId], [WorkId], [StartDate], [EndDate], [ComputerId] FROM [dbo].[AggregateWorkItemIntervals] WHERE"
					+ " @MinStartDate <= [StartDate]" //no usable index on EndDate
					+ " AND [StartDate] < @EndDate"
					+ " AND @StartDate < DATEADD(ms, 0, [EndDate])" //don't use index on EndDate
					+ " AND [UserId] = @UserId",
					new
					{
						UserId = userId,
						MinStartDate = startDate - maxAggrWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					})
					.EnsureList();

				log.Debug("Loaded " + result.Count.ToInvariantString() + " AggregateWorkItemIntervals covered for user " + userId.ToInvariantString() + " between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}

		public static ILookup<int, ManualWorkItemCovered> GetManualWorkItemsByUserCovered(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			ILookup<int, ManualWorkItemCovered> stats;
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				stats = conn.Query<ManualWorkItemCovered>(
					"SELECT [UserId], [WorkId], [ManualWorkItemTypeId], [StartDate], [EndDate], [SourceId], [Comment] FROM [dbo].[ManualWorkItems] WHERE"
					+ " @MinStartDate <= [StartDate]" //no usable index on EndDate
					+ " AND [StartDate] < @EndDate"
					+ " AND @StartDate < DATEADD(ms, 0, [EndDate])", //don't use index on EndDate
					new
					{
						MinStartDate = startDate - maxManualWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					})
					.ToLookup(n => n.UserId);
			}
			log.Debug("Loaded " + stats.Count.ToInvariantString() + " ManualWorkItems covered between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
			return stats;
		}

		public static List<ManualWorkItemCovered> GetManualWorkItemsForUserCovered(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<ManualWorkItemCovered>(
					"SELECT [UserId], [WorkId], [ManualWorkItemTypeId], [StartDate], [EndDate], [SourceId], [Comment] FROM [dbo].[ManualWorkItems] WHERE"
					+ " @MinStartDate <= [StartDate]" //no usable index on EndDate
					+ " AND [StartDate] < @EndDate"
					+ " AND @StartDate < DATEADD(ms, 0, [EndDate])" //don't use index on EndDate
					+ " AND [UserId] = @UserId",
					new
					{
						UserId = userId,
						MinStartDate = startDate - maxManualWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					})
					.EnsureList();

				log.Debug("Loaded " + result.Count.ToInvariantString() + " ManualWorkItems covered for user " + userId.ToInvariantString() + " between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}

		public static ILookup<int, MobileWorkItemCovered> GetMobileWorkItemsByUserCovered(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			ILookup<int, MobileWorkItemCovered> stats;
			using (var conn = new SqlConnection(Properties.Settings.Default._jobcontrolConnectionString))
			{
				stats = conn.Query<MobileWorkItemCovered>(
					"SELECT [UserId], [WorkId], [StartDate], [EndDate], [Imei], [IsBeacon] FROM [dbo].[MobileWorkItems] WHERE"
					+ " @MinStartDate <= [StartDate]" //no index on EndDate
					+ " AND [StartDate] < @EndDate"
					+ " AND @StartDate < [EndDate]",
					new
					{
						MinStartDate = startDate - maxMobileWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					})
					.ToLookup(n => n.UserId);
			}
			log.Debug("Loaded " + stats.Count.ToInvariantString() + " MobileWorkItems covered between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
			return stats;
		}

		public static List<MobileWorkItemCovered> GetMobileWorkItemsForUserCovered(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default._jobcontrolConnectionString))
			{
				var result = conn.Query<MobileWorkItemCovered>(
					"SELECT [UserId], [WorkId], [StartDate], [EndDate], [Imei], [IsBeacon] FROM [dbo].[MobileWorkItems] WHERE"
					+ " @MinStartDate <= [StartDate]" //no index on EndDate
					+ " AND [StartDate] < @EndDate"
					+ " AND @StartDate < [EndDate]"
					+ " AND [UserId] = @UserId",
					new
					{
						UserId = userId,
						MinStartDate = startDate - maxMobileWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					})
					.EnsureList();

				log.Debug("Loaded " + result.Count.ToInvariantString() + " MobileWorkItems covered for user " + userId.ToInvariantString() + " between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}

		public static List<UserStatInfo> GetUserStatsInfo(List<int> userIdsFilter)
		{
			var sw = Stopwatch.StartNew();
			List<UserStatInfo> result;
			using (var context = new JobControlDataClassesDataContext())
			{
				if (userIdsFilter != null && userIdsFilter.Count != 0)
				{
					//select only specified users
					result = userIdsFilter
						.Select(n => context.GetUserStatInfoById(n))
						.Where(n => n != null)
						.ToList();
				}
				else
				{
					//select all users
					result = context.GetUserStatsInfo().ToList();
				}
			}
			log.Debug("Loaded " + result.Count + " user stats info (for " + GetUserFilterString(userIdsFilter) + ") in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static List<int> GetUserIdsInSameCompany(int userId)
		{
			var sw = Stopwatch.StartNew();
			List<int> result;
			using (var context = new JobControlDataClassesDataContext())
			{
				result = context.GetUserIdsInSameCompany(userId).ToList();
			}
			log.Debug("Loaded " + result.Count + " user ids in same company as " + userId + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		private static string GetUserFilterString(List<int> userIdsFilter)
		{
			string users = (userIdsFilter == null || userIdsFilter.Count == 0
				? "all users"
				: (userIdsFilter.Count == 1
					? "userId: " + userIdsFilter[0]
					: "userIds: " + string.Join(", ", userIdsFilter.Select(n => n.ToString()).ToArray())));
			return users;
		}

		public static Dictionary<int, Work> GetWorksById()
		{
			var sw = Stopwatch.StartNew();
			Dictionary<int, Work> works;
			using (var context = new JobControlDataClassesDataContext())
			{
				works = context.GetWorks().ToDictionary(n => n.Id);
			}
			log.Debug("Loaded " + works.Keys.Count + " works in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return works;
		}

		public static Work GetWorkById(int workId)
		{
			using (var context = new JobControlDataClassesDataContext())
			{
				return context.GetWorkById(workId);
			}
		}

		public static Dictionary<int, Project> GetProjectsById()
		{
			var sw = Stopwatch.StartNew();
			Dictionary<int, Project> projects;
			using (var context = new JobControlDataClassesDataContext())
			{
				projects = context.GetProjects().ToDictionary(n => n.Id);
			}
			log.Debug("Loaded " + projects.Keys.Count + " projects in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return projects;
		}

		public static Project GetProjectById(int workId)
		{
			using (var context = new JobControlDataClassesDataContext())
			{
				return context.GetProjectById(workId);
			}
		}

		public static Dictionary<int, DetailedWork> GetDetailedWorkForUser(int userId)
		{
			var sw = Stopwatch.StartNew();
			Dictionary<int, DetailedWork> works;
			using (var context = new JobControlDataClassesDataContext())
			{
				works = context.GetDetailedWorksForUser(userId).ToDictionary(n => n.Id);
			}
			log.Debug("Loaded " + works.Keys.Count + " works for user " + userId + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return works;
		}

		public static Dictionary<int, DetailedWork> GetDetailedWorkForUserCached(int userId)
		{
			return getCachedDetailedWorkStat(userId);
		}

		//todo i think this can throw deadlock ex if updateaggr is running
		public static Dictionary<int, TotalWorkTimeStat> GetTotalWorkTimeByWorkIdForUser(int userId, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			Dictionary<int, TotalWorkTimeStat> totalStatsForUser;
			using (var context = new AggregateDataClassesDataContext())
			{
				context.CommandTimeout = 300; //5mins
				totalStatsForUser = context.GetTotalWorkTimeByWorkIdForUser(userId, null, endDate)
					.Select(n => TotalWorkTimeStat.CreateFrom(n))
					.ToDictionary(n => n.WorkId);
			}
			log.Debug("Loaded " + totalStatsForUser.Keys.Count + " total stats for user " + userId + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return totalStatsForUser;
		}

		public static Dictionary<int, TotalWorkTimeStat> GetTotalWorkTimeByWorkIdForUserCached(int userId, DateTime endDate)
		{
			return getCachedTotalWorkTimeStat(new KeyValuePair<int, DateTime>(userId, endDate));
		}

		public static Wage GetWageData(int workId, int userId, int reportUserId, bool isInternal)
		{
			var sw = Stopwatch.StartNew();
			Wage wage;
			using (var context = new JobControlDataClassesDataContext())
			{
				wage = context.GetWageData(workId, userId, reportUserId, isInternal);
			}
			log.Debug("Loaded wage for work/user " + workId + " / " + userId + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return wage;
		}

		public static List<int> GetReportableProjectRootsForUser(int reportUserId, bool isInternal)
		{
			var sw = Stopwatch.StartNew();
			List<int> result;
			using (var context = new JobControlDataClassesDataContext())
			{
				result = context.GetReportableProjectRootsForUser(reportUserId, isInternal).ToList();
			}
			log.Debug("Loaded " + result.Count + " reportable reportIds for user " + reportUserId + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static List<ProjectEmailRequest> GetProjectEmailRequests()
		{
			var sw = Stopwatch.StartNew();
			List<ProjectEmailRequest> result;
			using (var context = new JobControlDataClassesDataContext())
			{
				context.CommandTimeout = 300; //5mins
				result = context.GetProjectEmailRequests().ToList();
			}
			log.Debug("Loaded " + result.Count + " project email requests in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static List<AggregateEmailRequest> GetAggregateEmailRequests()
		{
			var sw = Stopwatch.StartNew();
			List<AggregateEmailRequest> result;
			using (var context = new JobControlDataClassesDataContext())
			{
				context.CommandTimeout = 300; //5mins
				result = context.GetAggregateEmailRequests().ToList();
			}
			log.Debug("Loaded " + result.Count + " aggregate email requests in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static bool HasUserGotCreditForInterval(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			bool result;
			using (var context = new JobControlDataClassesDataContext())
			{
				try
				{
					result = context.HasUserGotCreditForInterval(userId, startDate, endDate);
				}
				catch (Exception ex)
				{
					log.Error("Unable to execute HasUserGotCreditForInterval for userId " + userId + " from " + startDate + " to " + endDate, ex);
					result = true; //we have credit on error
				}
			}
			log.Debug("Checked unpaid intervals for user " + userId + " between " + startDate + " and " + endDate + " result " + result + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static ILookup<int, MobileLocationInfo> GetMobileLocationInfoByUser(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			ILookup<int, MobileLocationInfo> result;
			using (var context = new MobileDataClassesDataContext())
			{
				result = context.GetMobileLocationInfoByUser(startDate, endDate);
			}
			log.Debug("Loaded mobile location info for " + result.Count + " users in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static ILookup<int, MobileActivityInfo> GetMobileActivityInfoByUser(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			ILookup<int, MobileActivityInfo> result;
			using (var context = new MobileDataClassesDataContext())
			{
				result = context.GetMobileActivityInfoByUser(startDate, endDate);
			}
			log.Debug("Loaded mobile activity info for " + result.Count + " users in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static ILookup<int, MobileWorkItem> GetMobileWorkItemsByUser(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			ILookup<int, MobileWorkItem> result;
			using (var context = new MobileDataClassesDataContext())
			{
				result = context.GetMobileWorkItemsByUser(startDate, endDate);
			}
			log.Debug("Loaded mobile workitems between " + startDate + " and " + endDate + " for " + result.Count + " users in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static List<MobileWorkItem> GetMobileWorkItemsForUser(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var context = new MobileDataClassesDataContext())
			{
				var result = context.GetMobileWorkItemsForUser(userId, startDate, endDate);
				log.Debug("Loaded " + result.Count + " mobile workitems for user " + userId + " between " + startDate + " and " + endDate + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}

		public static List<int> GetUsersForUsageStats(DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			List<int> result;
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.CommandTimeout = 900; //15mins
				context.Connection.Open();
				using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted)) //we don't want to lock the db for 5 mins
				{
					result = context.GetUsersForUsageStats(startDate, endDate).Select(n => n.UserId).ToList();
				}
			}
			log.Debug("Found " + result.Count + " users between " + startDate + " and " + endDate + " for UsageStats in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static List<MonitorableUser> GetMonitorableUsers(int userId)
		{
			var sw = Stopwatch.StartNew();
			List<MonitorableUser> result;
			using (var context = new JobControlDataClassesDataContext())
			{
				result = context.GetMonitorableUsers(userId).ToList();
			}
			log.Debug("Found " + result.Count + " monitorable users between for user " + userId + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static List<UserReportFrequency> GetLocationReportRequests()
		{
			var sw = Stopwatch.StartNew();
			List<UserReportFrequency> result;
			using (var context = new JobControlDataClassesDataContext())
			{
				result = context.GetAggregateMobileLocationRequests().ToList();
			}
			log.Debug("Loaded " + result.Count + " location report requests in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
			return result;
		}

		public static List<WorkOrProject> GetWorksOrProjects(IEnumerable<int> ids)
		{
#if DEBUG
			return ids.Where(n => n != 213421323).Select(n => new WorkOrProject() { Id = n, Name = "Munka " + n, ParentId = 213421323, IsProject = false }).Concat(new[] { new WorkOrProject() { Id = 213421323, IsProject = true, Name = "Apu", ParentId = null } }).ToList();
#endif
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default._jobcontrolConnectionString))
			{
				var idTable = new DataTable();
				idTable.Columns.Add("Id", typeof(int)); //IntIdTableType

				foreach (var id in ids)
				{
					idTable.Rows.Add(id);
				}

				var result = conn.Query<WorkOrProject>("Client_GetWorksOrProjects",
					new
					{
						Ids = idTable,
					},
					commandType: CommandType.StoredProcedure)
					.EnsureList();

				log.Debug("Loaded " + result.Count.ToInvariantString() + " WorkOrProjects in " + sw.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}

		public static List<ComputerCollectedItem> GetCollectedItemsForUser(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<ComputerCollectedItem>(
					"exec GetcollectedItemsforuser @UserId, @StartDate, @EndDate",
					new
					{
						UserId = userId,
						StartDate = startDate,
						EndDate = endDate
					},
					commandTimeout: 180) //3mins
					.EnsureList();

				if (ConfigManager.IsCollectedValuesEncrypted)
				{
					using (var encrypter = new Collector.StringCipher())
					{
						foreach (var item in result)
						{
							try
							{
								item.Value = encrypter.Decrypt(item.Value);
							}
							catch (Exception ex)
							{
								log.DebugFormat("Decrypting failed. Key: {0} Value: {1}", item.Key, item.Value);
							}
						}
					}
				}

				log.Debug("Loaded " + result.Count.ToInvariantString() + " CollectedItems for user " + userId.ToInvariantString() + " between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}

		public static List<VoiceRecording> GetVoiceRecordingsForUser(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.ObjectTrackingEnabled = false;

				var result = context.VoiceRecordings
					.Where(n => startDate < n.EndDate && n.StartDate < endDate && n.UserId == userId)
					.ToList();

				log.Debug("Loaded " + result.Count + " VoiceRecordings for user " + userId + " between " + startDate + " and " + endDate + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}
	}
}
