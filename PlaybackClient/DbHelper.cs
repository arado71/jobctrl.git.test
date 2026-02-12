using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using log4net;
using PlaybackClient.MobileServiceReference;
using Tct.ActivityRecorderClient;
using WorkItem = Tct.ActivityRecorderService.WorkItem;

namespace PlaybackClient
{
	public static class DbHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly TimeSpan maxWorkItemLength = TimeSpan.FromMinutes(10);
		//private static readonly TimeSpan maxAggrWorkItemLength = TimeSpan.FromDays(7); //hax nothing enfoces this atm.
		private static readonly TimeSpan maxManualWorkItemLength = TimeSpan.FromDays(2); //hax nothing enfoces this atm.
		//private static readonly TimeSpan maxIvrWorkItemLength = TimeSpan.FromDays(2); //hax nothing enfoces this atm.
		private static readonly TimeSpan maxMobileWorkItemLength = TimeSpan.FromDays(2); //hax nothing enfoces this atm.

		public static List<ManualWorkItem> GetManualWorkItemsForUser(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			var wiCount = 0;
			try
			{
				using (var context = new ManualDataClassesDataContext())
				using (context.BeginTransaction(IsolationLevel.ReadUncommitted))
				{
					var result = context.ManualWorkItems
						.Where(n => startDate < n.EndDate)
						.Where(n => startDate - maxManualWorkItemLength <= n.StartDate) //no index on EndDate
						.Where(n => n.StartDate < endDate)
						.Where(n => n.UserId == userId)
						.ToList();

					wiCount = result.Count;
					return result;
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("GetManualWorkItemsForUser failed userId {0} start {1} end {2}{3}{4}", userId, startDate, endDate, Environment.NewLine, ex);
				throw;
			}
			finally
			{
				log.InfoFormat("Executed GetManualWorkItemsForUser ({4}) userId {0} start {1} end {2} in {3:0.000}ms", userId, startDate, endDate, sw.Elapsed.TotalMilliseconds, wiCount);
			}
		}

		private static readonly CachedDictionary<Tuple<int, DateTime, DateTime>, List<WorkItem>> workitemCache = new CachedDictionary<Tuple<int, DateTime, DateTime>, List<WorkItem>>(TimeSpan.FromHours(2), true);
		public static List<WorkItem> GetWorkItemsWithAllDataForUser(int userId, DateTime startDate, DateTime endDate)
		{
			lock (workitemCache)
			{
				var key = Tuple.Create(userId, startDate, endDate);
				if (workitemCache.TryGetValue(key, out var result)) return result;
				var wiCount = 0;
				var sw = Stopwatch.StartNew();
				try
				{
					using (var context = new ActivityRecorderDataClassesDataContext())
					using (context.BeginTransaction(IsolationLevel.ReadUncommitted))
					{
						var loadOpt = new DataLoadOptions();
						loadOpt.LoadWith<WorkItem>(n => n.ScreenShotsInt);
						context.LoadOptions = loadOpt;

						var workItems = context.WorkItems.Where(n => startDate - maxWorkItemLength <= n.StartDate && n.StartDate < endDate) //no index on EndDate
							.Where(n => n.UserId == userId).ToList();
						workItems.RemoveAll(n => !(startDate < n.EndDate && n.StartDate < endDate));

						var scrs = context.ScreenShots
							.Where(n => startDate <= n.CreateDate)
							.Where(n => n.CreateDate < endDate)
							.Where(n => n.UserId == userId)
							.ToLookup(n =>new DateTime(n.CreateDate.Year, n.CreateDate.Month, n.CreateDate.Day, n.CreateDate.Hour, 0, 0));
	
						foreach (var workItem in workItems)
						{
							workItem.ScreenShots.AddRange(scrs[new DateTime(workItem.StartDate.Year, workItem.StartDate.Month, workItem.StartDate.Day, workItem.StartDate.Hour, 0, 0)].Where(s => s.CreateDate >= workItem.StartDate && s.CreateDate < workItem.EndDate));
						}

						wiCount = workItems.Count;
						workitemCache.Add(key, workItems);
						return workItems;
					}
				}
				catch (Exception ex)
				{
					log.ErrorFormat("GetWorkItemsWithAllDataForUser failed userId {0} start {1} end {2}{3}{4}", userId, startDate, endDate, Environment.NewLine, ex);
					throw;
				}
				finally
				{
					log.InfoFormat("Executed GetWorkItemsWithAllDataForUser ({4}) userId {0} start {1} end {2} in {3:0.000}ms", userId, startDate, endDate, sw.Elapsed.TotalMilliseconds, wiCount);
				}
			}
		}

		public static List<MobileWorkItem> GetMobileWorkItemsForUser(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			var wiCount = 0;
			try
			{
				using (var context = new MobileDataClassesDataContext())
				using (context.BeginTransaction(IsolationLevel.ReadUncommitted))
				{
					var result = context.MobileWorkItems
						.Where(n => startDate < n.EndDate)
						.Where(n => startDate - maxMobileWorkItemLength <= n.StartDate) //no index on EndDate
						.Where(n => n.StartDate < endDate)
						.Where(n => n.StartDate < n.EndDate) //valid interval
						.Where(n => n.UserId == userId)
						.ToList();

					wiCount = result.Count;
					return result;
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("GetMobileWorkItemsForUser failed userId {0} start {1} end {2}{3}{4}", userId, startDate, endDate, Environment.NewLine, ex);
				throw;
			}
			finally
			{
				log.InfoFormat("Executed GetMobileWorkItemsForUser ({4}) userId {0} start {1} end {2} in {3:0.000}ms", userId, startDate, endDate, sw.Elapsed.TotalMilliseconds, wiCount);
			}
		}

		public static List<MobileClientLocation> GetMobileLocationsForUser(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			var wiCount = 0;
			try
			{
				using (var context = new MobileDataClassesDataContext())
				using (context.BeginTransaction(IsolationLevel.ReadUncommitted))
				{
					var result = context.MobileClientLocations
						.Where(n => startDate <= n.Date)
						.Where(n => n.Date < endDate)
						.Where(n => n.UserId == userId)
						.ToList();

					wiCount = result.Count;
					return result;
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("GetMobileLocationsForUser failed userId {0} start {1} end {2}{3}{4}", userId, startDate, endDate, Environment.NewLine, ex);
				throw;
			}
			finally
			{
				log.InfoFormat("Executed GetMobileLocationsForUser ({4}) userId {0} start {1} end {2} in {3:0.000}ms", userId, startDate, endDate, sw.Elapsed.TotalMilliseconds, wiCount);
			}
		}
	}
}
