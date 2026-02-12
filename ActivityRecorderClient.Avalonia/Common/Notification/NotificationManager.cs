using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Notification
{
	/// <summary>
	/// Class for periodically getting pending notifications from the server.
	/// </summary>
	public class NotificationManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int interval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

		private static string FilePath { get { return "LastNotificationId-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<NotificationData>> NotificationOpened;

		private int? lastId;
		private int? LastId
		{
			get { return lastId; }
			set
			{
				lastId = value;
				if (value.HasValue)
				{
					IsolatedStorageSerializationHelper.Save(FilePath, value.Value);
				}
				else
				{
					IsolatedStorageSerializationHelper.Delete(FilePath);
				}
			}
		}

		protected override int ManagerCallbackInterval
		{
			get { return interval; }
		}

		public NotificationManager()
			: base(log)
		{
			if (IsolatedStorageSerializationHelper.Exists(FilePath))
			{
				int value;
				if (IsolatedStorageSerializationHelper.Load(FilePath, out value))
				{
					log.Info("Loaded LastId from disk " + value);
					lastId = value;
				}
			}
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				var data = ActivityRecorderClientWrapper.Execute(n => n.GetPendingNotification(ConfigManager.UserId, ConfigManager.EnvironmentInfo.ComputerId, LastId));
				if (data == null) return;
				log.Debug("Pending notification received with id " + data.Id);
				OnNotificationOpened(data);
				LastId = data.Id;
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get pending notification", log, ex);
			}
		}

		private void OnNotificationOpened(NotificationData data)
		{
			var del = NotificationOpened;
			if (del == null)
			{
				var message = "Notification with id " + data.Id + " is not handled";
				log.ErrorAndFail(message);
				throw new Exception(message);
			}
			del(this, SingleValueEventArgs.Create(data));
		}

	}
}
