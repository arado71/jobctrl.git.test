using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderService.MobileServiceReference;

namespace Tct.ActivityRecorderService.OnlineStats
{
	/// <summary>
	/// Thread-safe class for managing online/offline statuses for all mobile users. The statuses are provided by an other service.
	/// </summary>
	public class MobileStatusManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan timeOutInterval = TimeSpan.FromMilliseconds(Math.Max(2 * 60 * 1000, ConfigManager.MobileStatusUpdateInterval * 3)); //2 mins or 3 times MobileStatusUpdateInterval which is longer
		private readonly object thisLock = new object();
		private Dictionary<int, MobileWorksForUser> mobileStatusDict = new Dictionary<int, MobileWorksForUser>();

		private Dictionary<int, MobileWorksForUser> MobileStatusDict
		{
			get { lock (thisLock) { return mobileStatusDict; } }
			set { lock (thisLock) { mobileStatusDict = value; } }
		}

		public MobileStatusManager()
			: base(log)
		{
			ManagerCallbackInterval = ConfigManager.MobileStatusUpdateInterval;
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				var oldUsers = MobileStatusDict;
				var offline = new Dictionary<Tuple<int, long>, MobileWork>();
				foreach (var oldUser in oldUsers)
				{
					foreach (var mobileWork in oldUser.Value.MobileWorks)
					{
						offline[Tuple.Create(oldUser.Key, mobileWork.Imei)] = mobileWork;
					}
				}

				var onlineUsersFormService = GetOnlineMobileUsers();
				var online = new Dictionary<Tuple<int, long>, MobileWork>();
				foreach (var onlineMobileUser in onlineUsersFormService)
				{
					var key = Tuple.Create(onlineMobileUser.UserId, onlineMobileUser.Imei);
					online[key] = new MobileWork(onlineMobileUser.WorkId, onlineMobileUser.Imei, onlineMobileUser.BatteryPercent, onlineMobileUser.ConnectionType, onlineMobileUser.LastCameraShotPath, onlineMobileUser.DeviceType);
					offline.Remove(key);
				}

				var newUsers = offline.Select(n => new KeyValuePair<Tuple<int, long>, MobileWork>(n.Key, n.Value.ToOffline()))
					.Where(n => n.Value != null)
					.Concat(online)
					.ToLookup(n => n.Key.Item1, n => n.Value)
					.ToDictionary(n => n.Key, n => new MobileWorksForUser(n.ToArray(), DateTime.UtcNow));

				MobileStatusDict = newUsers;
			}
			catch (Exception ex)
			{
				log.Warn("Unable to update online mobile users", ex);
			}
		}

		public MobileWork[] GetMobileWorksForUser(int userId, out bool isUpToDate)
		{
			var dictToRead = MobileStatusDict;
			MobileWorksForUser worksForUser;
			if (!dictToRead.TryGetValue(userId, out worksForUser) //user has no stats
				|| worksForUser.UpdateDate + timeOutInterval < DateTime.UtcNow) //stats are too old
			{
				isUpToDate = false;
				return worksForUser != null ? worksForUser.MobileWorks : MobileWorksForUser.Empty.MobileWorks;
			}
			else
			{
				isUpToDate = true;
				return worksForUser.MobileWorks;
			}
		}

		private static List<OnlineMobileUser> GetOnlineMobileUsers()
		{
#if DEBUG
			return Enumerable.Empty<OnlineMobileUser>()
				.Concat(new[] { new OnlineMobileUser() { UserId = 13, Imei = 2, WorkId = 213 } }) //test data
				.ToList();
#endif
			using (var mobile = new MobileClientWrapper())
			{
				var users = mobile.Client.GetOnlineUsers();
				if (users == null)
				{
					log.Info("Received online mobile users: null");
					return Enumerable.Empty<OnlineMobileUser>().ToList();
				}
				log.Info("Received online mobile users: " + string.Join(", ", users.Select(n => "[uid:" + n.UserId + " imei:" + n.Imei + " wid:" + n.WorkId + " bat: " + n.Battery + " Con: " + n.Connection + "]")));
				return users
					.Select(n => new OnlineMobileUser() { UserId = n.UserId, WorkId = n.WorkId, Imei = n.Imei, BatteryPercent = (byte?)n.Battery, ConnectionType = n.Connection, LastCameraShotPath = n.LastCameraShotPath, DeviceType = n.DeviceType })
					.ToList();
			}
		}

		private class MobileWorksForUser //immutable
		{
			private static readonly MobileWork[] emptyMobileWorks = new MobileWork[0];
			public static readonly MobileWorksForUser Empty = new MobileWorksForUser(emptyMobileWorks, DateTime.MinValue);

			public readonly DateTime UpdateDate;
			public readonly MobileWork[] MobileWorks;

			public MobileWorksForUser(MobileWork[] mobileWorks, DateTime updateDate)
			{
				MobileWorks = mobileWorks ?? emptyMobileWorks;
				UpdateDate = updateDate;
			}

			public static MobileWorksForUser GetMobileWorks(IEnumerable<OnlineMobileUser> onlineUsers)
			{
				Debug.Assert(!onlineUsers.Any() || onlineUsers.Select(n => n.UserId).Distinct().Count() == 1); //it's called only for the same user
				//we can only have one entry for every imei
				var imeiSet = new HashSet<long>();
				var newMobileWorks = new List<MobileWork>();
				foreach (var onlineMobileUser in onlineUsers)
				{
					if (imeiSet.Add(onlineMobileUser.Imei))
					{
						newMobileWorks.Add(new MobileWork(onlineMobileUser.WorkId, onlineMobileUser.Imei, onlineMobileUser.BatteryPercent, onlineMobileUser.ConnectionType, onlineMobileUser.LastCameraShotPath, onlineMobileUser.DeviceType));
					}
					else
					{
						var bad = onlineMobileUser; //don't close over loop variable
						log.Error("Cannot add duplicate online data for mobile userId " + bad.UserId + " and imei " + bad.Imei + " (workId: " + onlineUsers.Where(n => n.Imei == bad.Imei).First().Imei + " ignored workId:" + bad.WorkId + ")");
					}
				}
				return new MobileWorksForUser(newMobileWorks.ToArray(), DateTime.UtcNow);
			}
		}

		public class MobileWork //immutable
		{
			public readonly int? WorkId;
			public readonly long Imei;
			public readonly byte? BatteryPercent;
			public readonly string ConnectionType;
			public readonly string LastCameraShotPath;
			public readonly DeviceType DeviceType;
			private readonly DateTime? offlineDate; //don't leak memory

			private MobileWork(int? workId, long imei, byte? batteryPercent, string connectionType, string lastCameraShotPath, DeviceType deviceType)
			{
				WorkId = workId;
				Imei = imei;
				BatteryPercent = batteryPercent;
				ConnectionType = connectionType;
				LastCameraShotPath = lastCameraShotPath;
				DeviceType = deviceType;
				offlineDate = workId != null ? (DateTime?)null : DateTime.UtcNow;
			}

			public MobileWork(int workId, long imei, byte? batteryPercent, string connectionType, string lastCameraShotPath, DeviceType deviceType)
				: this((int?)workId, imei, batteryPercent, connectionType, lastCameraShotPath, deviceType)
			{
			}

			public MobileWork ToOffline()
			{
				if (WorkId == null) return DateTime.UtcNow - offlineDate.Value > TimeSpan.FromDays(2) ? null : this;
				return new MobileWork(null, Imei, BatteryPercent, ConnectionType, LastCameraShotPath, DeviceType);
			}
		}

		private class OnlineMobileUser //received by the service
		{
			public int UserId { get; set; }
			public long Imei { get; set; }
			public int WorkId { get; set; }
			public byte? BatteryPercent { get; set; }
			public string ConnectionType { get; set; }
			public string LastCameraShotPath { get; set; }
			public DeviceType DeviceType { get; set; }
		}
	}
}
