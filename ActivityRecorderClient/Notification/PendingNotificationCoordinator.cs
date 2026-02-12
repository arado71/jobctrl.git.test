using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Notification
{
	/// <summary>
	/// Class for coordinating/sending confirmations for pending notifications
	/// </summary>
	public class PendingNotificationCoordinator : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly NotificationManager notificationManager = new NotificationManager();
		private readonly INotificationService notificationService;
		private readonly SynchronizationContext context;
		private readonly AsyncWorkQueue<PendingNotification> notificationSender;
		private readonly CaptureCoordinator captureCoordinator;

		private bool isDisposed; //isDisposed is only read and modified on the GUI thread

		public PendingNotificationCoordinator(INotificationService notificationSvc, SynchronizationContext guiContext, CaptureCoordinator captureCoord)
		{
			context = guiContext;
			captureCoordinator = captureCoord;
			notificationService = notificationSvc;
			notificationManager.NotificationOpened += notificationManager_NotificationOpened;
			notificationSender = new AsyncWorkQueue<PendingNotification>(ConfirmNotification, RetryConfirmNotification);
		}

		private void notificationManager_NotificationOpened(object sender, SingleValueEventArgs<NotificationData> e)
		{
			var pending = NotificationSerializationHelper.CreatePending(e.Value);
			log.Info("Notification pending " + pending);
			context.Post(_ => ShowNotificationGui(pending), null);
		}

		public void LoadNotifications()
		{
			foreach (var pending in NotificationSerializationHelper.LoadPendings())
			{
				log.Info("Notification loaded from disk " + pending);
				if (pending.IsConfirmed)
				{
					notificationSender.EnqueueAsync(pending);
				}
				else
				{
					var pend = pending;
					context.Post(_ => ShowNotificationGui(pend), null);
				}
			}
		}

		public void Start()
		{
			notificationManager.Start();
		}

		public void Stop()
		{
			notificationManager.Stop();
		}

		private void ShowNotificationGui(PendingNotification pending)
		{
			//we can receive this after we are disposed (since it's a posted message)
			if (isDisposed)
			{
				log.Info("Notification form skipped " + pending);
				return;
			}
			var showDate = DateTime.UtcNow;
			var showTicks = Environment.TickCount;
			if (pending.Result.ShowDate == DateTime.MinValue) pending.Result.ShowDate = showDate;
			NotificationSerializationHelper.SavePending(pending);

			var formResult = "ERROR"; //let's hope it will be never used (if it would be a recoverable error then we should use null)
			try
			{
				log.Info("Notification form show " + pending);
				ExecuteCustomActions(pending.Data.Form);
				formResult = notificationService.ShowServerNotification(pending.Data.Form);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Error in ShowServerNotification for " + pending, ex);
			}
			if (formResult == null) //handle porgram exit
			{
				log.Info("Notification form exited " + pending + " confirmation skipped result was: (null)");
				return;
			}

			var duration = TimeSpan.FromMilliseconds((uint)(Environment.TickCount - showTicks));
			var confirmDate = showDate + duration;
			pending.Result.ConfirmDate = confirmDate; //this is not water-tight but good enough atm.
			pending.Result.Result = formResult;
			NotificationSerializationHelper.SavePending(pending);
			log.Info("Notification form confirmed " + pending);

			notificationSender.EnqueueAsync(pending);
		}

		private void ExecuteCustomActions(JcForm jcForm)
		{
			if (jcForm.BeforeShowActions == null) return;
			if (jcForm.BeforeShowActions.Contains("RefreshMenu"))
			{
				log.Debug("Executing custom action RefreshMenu");
				captureCoordinator.RefreshMenuAsync();
			}
		}

		private void ConfirmNotification(PendingNotification pending)
		{
			ActivityRecorderClientWrapper.Execute(n => n.ConfirmNotification(pending.Result));
			NotificationSerializationHelper.DeletePending(pending);
			log.Info("Notification sent " + pending + " to the server");
		}

		private void RetryConfirmNotification(PendingNotification pending, Exception ex)
		{
			WcfExceptionLogger.LogWcfError("confirm notification " + pending, log, ex);
			//retry after a dealy
			var timer = new Timer(self =>
			{
				((Timer)self).Dispose();
				notificationSender.EnqueueAsync(pending);
			});
			timer.Change(3000, Timeout.Infinite);
		}

		public void Dispose()
		{
			isDisposed = true;
			notificationSender.Dispose();
		}
	}
}
