using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Telemetry;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class ClientKickCoordinator : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan nfKickStopWorkDuration = TimeSpan.Zero;

		private readonly ClientKickManager clientKickManager = new ClientKickManager();
		private readonly SynchronizationContext context;
		private readonly CurrentWorkController currentWorkController;
		private readonly INotificationService notificationService;

		public ClientKickCoordinator(CurrentWorkController workController, SynchronizationContext guiSynchronizationContext, INotificationService notificationSvc)
		{
			if (workController == null || guiSynchronizationContext == null || notificationSvc == null) throw new ArgumentNullException();
			context = guiSynchronizationContext;
			currentWorkController = workController;
			notificationService = notificationSvc;

			clientKickManager.ClientKicked += ClientKicked;
			currentWorkController.PropertyChanged += CurrentWorkControllerPropertyChanged;
		}

		public void Start()
		{
			clientKickManager.Start();
		}

		public void Stop()
		{
			clientKickManager.Stop();
		}

		public void Dispose()
		{
			currentWorkController.PropertyChanged -= CurrentWorkControllerPropertyChanged;
			clientKickManager.ClientKicked -= ClientKicked;
		}

		private void ClientKicked(object sender, SingleValueEventArgs<ClientComputerKick> e)
		{
			context.Post(_ =>
			{
				if (currentWorkController.IsShuttingDown) return;
				var kick = e.Value;
				var kickMan = (ClientKickManager)sender;
				var result = KickResult.UnknownError;
				TelemetryHelper.RecordFeature("Kick", "Use");
				try
				{
					if (currentWorkController.MutualWorkTypeCoordinator.IsWorking)
					{
						log.Info("Stop working due to kick");
						currentWorkController.MutualWorkTypeCoordinator.RequestKickWork();
						result = KickResult.Ok;
					}
					else
					{
						log.Info("Cannot stop working due to kick because already offline");
						result = KickResult.AlreadyOffline;
					}
				}
				finally
				{
					kickMan.ConfirmKickAsync(kick.Id, result);
				}
				if (kick.CreatedBy < 0)
				{
					if (result == KickResult.AlreadyOffline) return;
					notificationService.ShowNotification(NotificationKeys.KickStopWork, nfKickStopWorkDuration, 
						Labels.NotificationKickedFromAnotherClientTitle, Labels.NotificationKickedFromAnotherClientBody, 
						CurrentWorkController.NotWorkingColor);
					return;
				}
				var title = Labels.NotificationKickedTitle;
				var body = string.Format(Labels.NotificationKickedBody, kick.Reason, kick.CreatedByName);
				notificationService.ShowNotification(NotificationKeys.KickStopWork + kick.Id, nfKickStopWorkDuration,
					title, body,
					CurrentWorkController.NotWorkingColor);
				notificationService.ShowMessageBox(body, title);
			}, null);
		}

		private WorkData lastWork;
		void CurrentWorkControllerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (ConfigManager.CoincidentalClientsEnabled) return;
			if (e.PropertyName == "IsOnline" && currentWorkController.IsOnline && currentWorkController.CurrentWork != null)
			{
				clientKickManager.MakeClientActiveAsync(true);
				return;
			}
			if (e.PropertyName != "CurrentWork") return;
			if (currentWorkController.CurrentWork != null && lastWork == null)
			{
				lastWork = currentWorkController.CurrentWork;
				notificationService.HideNotification(NotificationKeys.KickStopWork);
				clientKickManager.MakeClientActiveAsync(true);
			}
			else if (currentWorkController.CurrentWork == null && lastWork != null)
			{
				lastWork = null;
				if (!currentWorkController.MutualWorkTypeCoordinator.IsWorking)
					clientKickManager.MakeClientActiveAsync(false);
			}
		}

	}
}
