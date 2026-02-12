using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.ClientErrorReporting
{
	class AcquireLogsManager: PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string AcquireLogNotificationKey = "AcquireLogs";
		private AcquireLogsMessageBox messageBox;

#if DEBUG
		private const int managerCallbackInterval = 1 * 60 * 1000;
#else
		private const int managerCallbackInterval = 5 * 60 * 1000;
#endif

		public AcquireLogsManager()
		:base(log)
		{
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				if (!(Platform.Factory is Platform.PlatformWinFactory winPlatform))
				{
					log.Warn("Unexpected error");
					return;
				}
#if !DEBUG
				if (!winPlatform.MainForm.CurrentWorkController.IsWorking)
					return;
				if (!ActivityRecorderClientWrapper.Execute(x => x.ShouldSendLogs(ConfigManager.UserId)))
					return;
#endif
				switch (ConfigManager.LocalSettingsForUser.AutoSendLogFiles)
				{
					case false:
						return;
					case true:
						sendLogs();
						return;
				}

				if (!ConfigManager.IsTaskBarIconShowing)
				{
					sendLogs();
					return;
				}
				Platform.Factory.GetGuiSynchronizationContext()?.Send(x =>
				{
					var notificationService = winPlatform.GetNotificationService();
					notificationService.HideNotification(AcquireLogNotificationKey);
					notificationService.ShowNotification(AcquireLogNotificationKey,
						TimeSpan.Zero,
						Labels.AcquireLogs_NotificationTitle,
						Labels.AcquireLogs_NotificationMessage,
						MetroFramework.MetroColors.Magenta,
						showMessageBoxAndSendLogs);
				}, null);
				
			}
			catch (Exception ex)
			{
				log.Warn("Unexpected error in AcquireLogsManager's ManagercallbackImpl.", ex);
			}
		}

		private void showMessageBoxAndSendLogs()
		{
			if (messageBox != null)
			{
				messageBox.WindowState = FormWindowState.Minimized;
				messageBox.Show();
				messageBox.WindowState = FormWindowState.Normal;
				messageBox.Activate();
				return;
			}
			messageBox = new AcquireLogsMessageBox();
			messageBox.FormClosed += (sender, args) =>
			{
				try
				{
					if (messageBox.DialogResult == DialogResult.None) return;
					if (messageBox.Remember)
					{
						if (messageBox.DialogResult != DialogResult.Yes)
						{
							ConfigManager.LocalSettingsForUser.AutoSendLogFiles = false;
							return;
						}

						ConfigManager.LocalSettingsForUser.AutoSendLogFiles = true;
					}

					if (messageBox.DialogResult != DialogResult.Yes) return;
					ThreadPool.QueueUserWorkItem(state => { sendLogs(); });
				}
				finally
				{
					messageBox = null;
				}
			};
			messageBox.Show();
		}

		private void sendLogs()
		{
			Platform.Factory.GetErrorReporter().ReportClientError("Sending requested logs.", true);
		}

		protected override int ManagerCallbackInterval => managerCallbackInterval;
	}
}
