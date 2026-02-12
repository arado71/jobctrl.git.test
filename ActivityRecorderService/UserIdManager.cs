using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService
{
	public class UserIdManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public static readonly UserIdManager Instance;
		private Dictionary<int, UserId> userIdDict = new Dictionary<int, UserId>();
		private readonly object thisLock = new object();
		private readonly HashSet<int> inactiveUserIds = new HashSet<int>();

		static UserIdManager()
		{
			Instance = new UserIdManager();
			Instance.Start();
		}

		public UserIdManager()
			: base(log)
		{
			ManagerCallbackInterval = ConfigManager.UserIdRefreshInterval;
		}

		protected override void ManagerCallbackImpl()
		{
			RefreshUserIds(false);
		}

		public bool TryGetIdsForUser(int userId, out int groupId, out int companyId)
		{
			bool isInactive;
			if (TryGetIdsFromDict(userId, true, out groupId, out companyId, out isInactive)) return true;

			if (isInactive) return false; //don't refresh for known inactive users every time (refresh costs 15ms and all wcf requests are blocked while refreshing)

			RefreshUserIds(false);

			return TryGetIdsFromDict(userId, false, out groupId, out companyId, out isInactive);
		}

		public bool IsActive(int userId)
		{
			int groupId, companyId;
			return TryGetIdsForUser(userId, out groupId, out companyId);
		}

		private bool TryGetIdsFromDict(int userId, bool firstPass, out int groupId, out int companyId, out bool isInactive)
		{
			lock (thisLock)
			{
				isInactive = inactiveUserIds.Contains(userId);

				if (!isInactive)
				{
					UserId uid;
					if (userIdDict.TryGetValue(userId, out uid))
					{
						groupId = uid.GroupId;
						companyId = uid.CompanyId;
						return true;
					}
					if (!firstPass) //there was a resfresh before this
					{
						inactiveUserIds.Add(userId);
						isInactive = true;
					}
				}
			}
			groupId = -1;
			companyId = -1;
			return false;
		}

		public void RefreshUserIds()
		{
			RefreshUserIds(true);
		}

		private void RefreshUserIds(bool throwOnError)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				Dictionary<int, UserId> newUserIdDict;
				using (var context = new JobControlDataClassesDataContext())
				{
					//Ids should be unique
					newUserIdDict = context.GetActiveUserIds().ToDictionary(n => n.Id);
				}
				log.Info("Found " + newUserIdDict.Count + " userId" + (newUserIdDict.Count == 1 ? "" : "s"));
				List<int> activeUserIds = null;
				lock (thisLock)
				{
					userIdDict = newUserIdDict;
					foreach (var inactiveUserId in inactiveUserIds) //no linq for perf reasons
					{
						if (userIdDict.ContainsKey(inactiveUserId))
						{
							if (activeUserIds == null) activeUserIds = new List<int>();
							activeUserIds.Add(inactiveUserId);
						}
					}
					if (activeUserIds != null) inactiveUserIds.ExceptWith(activeUserIds);
				}
				if (activeUserIds != null) log.Info("Reactivated " + activeUserIds.Count + " userId" + (activeUserIds.Count == 1 ? "" : "s"));
			}
			catch (Exception ex)
			{
				log.Error("Unable to fetch userIds", ex);
				if (throwOnError) throw;
			}
			finally
			{
				log.Info("Refreshed userIds in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
			}
		}
	}
}
