using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.Notifications
{
	/// <summary>
	/// Thread-safe class for caching notifications in order to avoid DB hit every time a client calls into the server.
	/// </summary>
	/// <remarks>
	/// This class doesn't care about deviceIds
	/// </remarks>
	public class NotificationCacheManager : PeriodicManager, INotificationService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ConcurrentDictionary<int, int[]> pendingNotifications = new ConcurrentDictionary<int, int[]>();
		private readonly NotificationService notificationService = new NotificationService();
		private int maxId;
		private int lastMaxId = -1;
		private bool firstCall = true;

		public NotificationCacheManager()
			: base(log)
		{
			ManagerCallbackInterval = ConfigManager.NotificationCacheUpdateInterval;
		}

		protected override void ManagerCallbackImpl()
		{
			using (var context = new NotificationDataClassesDataContext())
			{
				context.ObjectTrackingEnabled = false;

				var firstMaxId = 0; //max id for the first call (i.e. service start)
				if (firstCall) //avoid possible table scans until we have one row with ReceiveDate == null
				{
					firstMaxId = context.ExecuteQuery<int>("SELECT ISNULL(MAX([Id]),0) FROM [dbo].[ClientNotifications] WITH (TABLOCK)").Single();
					firstCall = false;
				}

				var pending = context.ClientNotifications
					.Where(n => n.ReceiveDate == null)
					.Where(n => n.Id > maxId)
					.ToList();

				foreach (var notification in pending)
				{
					var id = notification.Id;
					var idArr = new[] { id };
					pendingNotifications.AddOrUpdate(notification.UserId, idArr, (key, old) => old.Concat(idArr).ToArray());
					if (maxId < id) maxId = id;
				}

				if (maxId < firstMaxId)
				{
					maxId = firstMaxId;
				}

				if (maxId != lastMaxId)
				{
					log.Debug("New max id is " + maxId);
					lastMaxId = maxId;
				}
			}
		}

		public NotificationData GetPendingNotification(int userId, int computerId, int? lastId)
		{
			int[] idArr;
			if (pendingNotifications.TryGetValue(userId, out idArr))
			{
				if (idArr.Length == 0)
				{
					log.Error("Empty id array found for user " + userId);
				}
				log.Debug("Calling into the DB with uid: " + userId + " cid: " + computerId + " lastId: " + lastId);
				return notificationService.GetPendingNotification(userId, computerId, lastId);
			}
			return null;
		}

		public void ConfirmNotification(NotificationResult result)
		{
			notificationService.ConfirmNotification(result);
			int[] idArr;
			while (pendingNotifications.TryGetValue(result.UserId, out idArr))
			{
				if (!idArr.Contains(result.Id))
				{
					log.Warn("Possible double confirmation for NotificationResult " + result); //dupes are ok due to comm errors (otherwise they are not)
					return;
				}
				if (idArr.Length == 1 && idArr[0] == result.Id)
				{
					if (((ICollection<KeyValuePair<int, int[]>>)pendingNotifications).Remove(new KeyValuePair<int, int[]>(result.UserId, idArr)))
					{
						return;
					}
					continue; //lost race, try again
				}
				var newIdArr = idArr.Where(n => n != result.Id).ToArray();
				if (pendingNotifications.TryUpdate(result.UserId, newIdArr, idArr))
				{
					return;
				}
			}
		}
	}
}
