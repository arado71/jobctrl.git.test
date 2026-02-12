using log4net;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Tct.ActivityRecorderService.ClientComputerData
{
	static class AcquireClientLogHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ReaderWriterLockSlim cacheLockSlim = new ReaderWriterLockSlim();
		private static List<int> usersToSendLogs = new List<int>();
		private const int MaxRequestAgeInDays = 30;
		private const int ListRefreshIntervalInSeconds = 10;
		private static long lastUpdateTickCount = -1;


		public static bool ShouldSendLogs(int userId)
		{
			EnsureListUpToDate();
			cacheLockSlim.EnterReadLock();
			try
			{
				return usersToSendLogs.Contains(userId);
			}
			finally
			{
				cacheLockSlim.ExitReadLock();
			}
		}

		private static void UpdateRequestList()
		{
#if DEBUG
			usersToSendLogs = new List<int> { 13 };
			return;
#endif
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var minRequestDay = DateTime.Today.AddDays(-MaxRequestAgeInDays);
				var ret = context.Client_GetAcquireClientLogRequest();
				usersToSendLogs.Clear();
				foreach (var client_getAcquireClientLogRequestResult in ret)
				{
					if (client_getAcquireClientLogRequestResult.RequestedAt < minRequestDay)
					{
						try
						{
							context.Client_DeleteAcquireClientLogRequest(client_getAcquireClientLogRequestResult.UserId);
							log.Debug($"Client log acquiring deleted for user {client_getAcquireClientLogRequestResult.UserId}");
						}
						catch (Exception ex)
						{
							log.Warn("Couldn't delete AcquireClientLog.", ex);
						}
					}
					else
					{
						usersToSendLogs.Add(client_getAcquireClientLogRequestResult.UserId);
					}
				}
			}
		}

		private static void EnsureListUpToDate()
		{
			if (DateTime.UtcNow.Ticks - ListRefreshIntervalInSeconds * 1000 > lastUpdateTickCount)
			{
				cacheLockSlim.EnterWriteLock();
				try
				{
					if (DateTime.UtcNow.Ticks - ListRefreshIntervalInSeconds * 1000 <= lastUpdateTickCount) return;
					UpdateRequestList();
					lastUpdateTickCount = DateTime.UtcNow.Ticks;
				}
				catch (Exception ex)
				{
					log.Error("Couldn't get AcquireClientLogRequests.", ex);
					throw;
				}
				finally
				{
					cacheLockSlim.ExitWriteLock();
				}
			}
		}

		public static void DeleteAcquireClientRequest(int userId)
		{
			cacheLockSlim.EnterUpgradeableReadLock();
			try
			{
				if (!usersToSendLogs.Contains(userId)) return;
				cacheLockSlim.EnterWriteLock();
				try
				{
					usersToSendLogs.Remove(userId);
#if !DEBUG
					using (var context = new ActivityRecorderDataClassesDataContext())
					{
						context.Client_DeleteAcquireClientLogRequest(userId);
					}
#endif
				}
				catch (Exception ex)
				{
					log.Error("Couldn't remove acquireClientRequest.", ex);
				}
				finally
				{
					cacheLockSlim.ExitWriteLock();
				}
			}
			finally
			{
				cacheLockSlim.ExitUpgradeableReadLock();
			}
		}
	}
}
