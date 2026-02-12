using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderService.Persistence;

namespace Tct.ActivityRecorderService.Notifications
{
	public class NotificationService : INotificationService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public NotificationData GetPendingNotification(int userId, int computerId, int? lastId)
		{
			using (var context = new NotificationDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var loadOpt = new System.Data.Linq.DataLoadOptions();
				loadOpt.LoadWith<ClientNotification>(n => n.NotificationForm);
				context.LoadOptions = loadOpt;

				var qry = context.ClientNotifications
					.Where(n => n.UserId == userId)
					.Where(n => n.ReceiveDate == null)
					.Where(n => n.DeviceId == null || n.DeviceId == computerId);
				if (lastId.HasValue)
				{
					qry = qry.Where(n => n.Id > lastId.Value);
				}

				var pending = qry.OrderBy(n => n.Id).FirstOrDefault();

				if (pending == null) return null;

				pending.DeviceId = computerId; //this notification is bound to this device now
				pending.SendDate = DateTime.UtcNow; //just for the first time ?

				var result = new NotificationData()
				{
					Id = pending.Id,
					WorkId = pending.NotificationForm.WorkId,
					FormId = pending.FormId,
					Name = pending.NotificationForm.Name,
					Form = DeserializeData(pending.NotificationForm.Data),
				};

				context.SubmitChanges(); //optimistic concurrency checking is enough for us

				return result;
			}
		}

		private static readonly int? maxResultLength = LinqHelper.GetLengthLimit(typeof(ClientNotification), "Result");
		public void ConfirmNotification(NotificationResult result)
		{
			using (var context = new NotificationDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var notif = context.ClientNotifications.Where(n => n.Id == result.Id && n.UserId == result.UserId).Single();
				if (notif.ReceiveDate != null)
				{
					log.Info("NotificationResult dropped " + result);
					return; //dupe
				}
				notif.ReceiveDate = DateTime.UtcNow;
				notif.Result = result.Result;
				if (maxResultLength.HasValue && notif.Result != null && notif.Result.Length > maxResultLength.Value) //enforce length limit
				{
					log.Warn("NotificationResult " + notif + " is truncated");
					notif.Result = notif.Result.Substring(0, maxResultLength.Value);
				}
				notif.ShowDate = result.ShowDate;
				notif.ConfirmDate = result.ConfirmDate;

				context.SubmitChanges();
			}
		}

		public static JcForm DeserializeData(string data)
		{
			return XmlPersistenceHelper<JcForm>.LoadFromString(data);
		}

		public static string SerializeData(JcForm data)
		{
			return XmlPersistenceHelper<JcForm>.SaveToString(data);
		}
	}
}
