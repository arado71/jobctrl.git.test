using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService.Stats
{
	public class TotalWorkTimeStatsBuilder
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<int, TotalWorkTimeStats> totalStatsForUserDict = new Dictionary<int, TotalWorkTimeStats>();
		private readonly object thisLock = new object();

		private readonly object refreshLock = new object();
		private bool isRefreshRequired;
		private bool isRefreshRunning;

		public TotalWorkTimeStatsBuilder()
		{
			SetTotalWorkTimeStatsRefreshRequired();
		}

		private void RefreshTotalWorkTimeStatsImpl()
		{
			log.Info("Refreshing TotalWorkTimeStats...");
			DateTime now = DateTime.UtcNow;
			try
			{
				using (var context = new JobControlDataClassesDataContext())
				{
					var usersToWatch = context.GetActiveUserIds().Select(n => n.Id).ToList();
					RefreshTotalWorkTimeStats(usersToWatch, now);
					//hax we don't delete users from totalStatsForUserDict so in theory this is a memory leak
				}
			}
			catch (Exception ex)
			{
				log.Error("Error refreshing TotalWorkTimeStats", ex);
			}
		}

		private void RefreshTotalWorkTimeStats(IList<int> userIds, DateTime endDate)
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				var sw = Stopwatch.StartNew();
				foreach (var userId in userIds) //parallel foreach would be nice here
				{
					sw.Reset();
					sw.Start();
					try
					{
						var totalStatsForUser = context.GetTotalWorkTimeByWorkIdForUser(userId, null, endDate)
							.Select(n => TotalWorkTimeStat.CreateFrom(n))
							.ToDictionary(n => n.WorkId);

						var newStat = new TotalWorkTimeStats()
						{
							UserId = userId,
							FromDate = DateTime.MinValue,
							ToDate = endDate,
							Stats = totalStatsForUser,
						};

						lock (thisLock)
						{
							totalStatsForUserDict[userId] = newStat;
						}
					}
					catch (Exception ex)
					{
						log.Error("Unable to exec GetTotalWorkTimeByWorkIdForUser(" + userId + ", " + endDate + ")", ex);
					}
					finally
					{
						log.Debug("RefreshTotalWorkTimeStats for userId: " + userId + " finished in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
					}
				}
			}
		}

		public TotalWorkTimeStats GetTotalWorkTimeStats(int userId)
		{
			TotalWorkTimeStats result;
			lock (thisLock)
			{
				if (totalStatsForUserDict.TryGetValue(userId, out result))
				{
					return result;
				}
				return null;
			}
		}

		public void SetTotalWorkTimeStatsRefreshRequired()
		{
			lock (refreshLock)
			{
				if (isRefreshRunning)
				{
					isRefreshRequired = true;
					return;
				}
				isRefreshRequired = false;
				isRefreshRunning = true;
			}
			RefreshTotalWorkTimeStatsAsync();
		}

		private void RefreshTotalWorkTimeStatsCallback(object state)
		{
			//Assert isRefreshRunning == true
			try
			{
				RefreshTotalWorkTimeStatsImpl();
			}
			catch (Exception ex) //don't throw on bg thread
			{
				log.Error("Unexpected exception from RefreshTotalWorkTimeStatsImpl", ex);
			}
			finally
			{
				bool startNewRefresh = false;
				lock (refreshLock)
				{
					if (isRefreshRequired)
					{
						startNewRefresh = true;
						isRefreshRequired = false;
					}
					else
					{
						isRefreshRunning = false;
					}
				}
				if (startNewRefresh)
				{
					RefreshTotalWorkTimeStatsAsync();
				}
			}
		}

		private void RefreshTotalWorkTimeStatsAsync()
		{
			try
			{
				ThreadPool.QueueUserWorkItem(RefreshTotalWorkTimeStatsCallback, null);
			}
			catch (Exception ex)
			{
				log.Fatal("Unable to start RefreshTotalWorkTimeStatsCallback on bg thread", ex);
				lock (refreshLock)
				{
					isRefreshRunning = false;
				}
			}
		}
	}
}
