using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.Kicks
{
	public static class KickDbHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan defaultExpiration = TimeSpan.FromMinutes(1);

		public static ClientComputerKick CreateKick(int userId, long deviceId, string reason, TimeSpan expiration, int createdBy)
		{
			if (expiration <= TimeSpan.Zero) expiration = defaultExpiration;
			var now = DateTime.UtcNow;
			using (var context = new KickDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var kickToInsert = new ClientComputerKick()
				{
					UserId = userId,
					ComputerId = deviceId,
					Reason = reason,
					CreateDate = now,
					ExpirationDate = now + expiration,
					CreatedBy = createdBy,
				};
				context.ClientComputerKicks.InsertOnSubmit(kickToInsert);
				context.SubmitChanges();
				log.Info("Created kick " + kickToInsert);
				return kickToInsert;
			}
		}

		public static List<ClientComputerKick> GetActiveKicks()
		{
			var now = DateTime.UtcNow;
			using (var context = new KickDataClassesDataContext())
			{
				return context.ClientComputerKicks
					.Where(n => n.ExpirationDate > now)
					.Where(n => n.ConfirmDate != null)
					.ToList();
			}
		}

		public static int ConfirmKick(int id, int userId, long deviceId, DateTime confirmDate, KickResult result)
		{
			using (var context = new KickDataClassesDataContext())
			{
				var numRows = context.ClientComputerKickConfirm(id, userId, deviceId, confirmDate, (int)result);
				if (numRows == 1)
				{
					log.Info("Confirmed kick id:" + id + " userId:" + userId + " devId:" + deviceId + " res:" + result);
				}
				else
				{
					log.Warn("Unable to confirm kick id:" + id + " userId:" + userId + " devId:" + deviceId + " res:" + result);
				}
				return numRows;
			}
		}

		public static int SendKick(int id, int userId, long deviceId, DateTime sendDate)
		{
			using (var context = new KickDataClassesDataContext())
			{

				var numRows = context.ClientComputerKickSend(id, userId, deviceId, sendDate);
				if (numRows == 1)
				{
					log.Info("Sent kick id:" + id + " userId:" + userId + " devId:" + deviceId);
				}
				else
				{
					log.Error("Unable to send kick id:" + id + " userId:" + userId + " devId:" + deviceId);
				}
				return numRows;
			}
		}

		//private static Kick GetKickFromClientComputerKick(ClientComputerKick dbData)
		//{
		//    return GetKicksFromClientComputerKicks(new[] { dbData }).Single();
		//}

		//private static IEnumerable<Kick> GetKicksFromClientComputerKicks(IEnumerable<ClientComputerKick> dbData)
		//{
		//    var userNames = StatsDbHelper.GetUserStatsInfo(dbData.Select(n => n.CreatedBy).Distinct().ToList()).ToDictionary(n => n.Id);
		//    return dbData.Select(n => new Kick() { Id = n.Id, Reason = n.Reason, CreatedBy = n.CreatedBy, CreatedByName = userNames[n.CreatedBy].Name });
		//}

		public static string GetUserName(int userId)
		{
			try
			{
				var info = StatsDbHelper.GetUserStatsInfo(new List<int>() { userId }).SingleOrDefault();
				if (info != null && !string.IsNullOrEmpty(info.Name))
				{
					return info.Name;
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to get user name for id " + userId, ex);
			}
			return userId.ToString();
		}

		public static List<ActiveDevice> GetActiveDevices()
		{
			using (var context = new KickDataClassesDataContext())
			{
				return context.ActiveDevices
					.ToList();
			}
		}

		public static void AddDevice(ActiveDevice device)
		{
			using (var context = new KickDataClassesDataContext())
			{
				context.SetXactAbortOn();
				context.ActiveDevices.InsertOnSubmit(device);
				context.SubmitChanges();
			}
		}

		public static void UpdateDevice(int userId, long deviceId)
		{
			using (var context = new KickDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var device = context.ActiveDevices.FirstOrDefault(d => d.UserId == userId && d.DeviceId == deviceId);
				if (device != null)
				{
					device.LastSeen = DateTime.UtcNow;
					context.SubmitChanges();
				}
			}
		}

		public static void RemoveDevice(int userId, long deviceId)
		{
			using (var context = new KickDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var device = context.ActiveDevices.FirstOrDefault(d => d.UserId == userId && d.DeviceId == deviceId);
				if (device != null)
				{
					context.ActiveDevices.DeleteOnSubmit(device);
					context.SubmitChanges();
				}
			}
		}
	}
}
