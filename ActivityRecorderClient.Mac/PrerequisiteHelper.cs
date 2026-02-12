using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using log4net;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.ViewMac;

namespace Tct.ActivityRecorderClient
{
	public static class PrerequisiteHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly DateTime expirationDate = DateTime.MaxValue; //prohibited by CEO...
		private static readonly DateTime expirationWarnDate = expirationDate.AddDays(-7);
		private static readonly TimeSpan expirationWarnInterval = TimeSpan.FromHours(6);
		private static string installUrl = "http://jobctrl.com/Install/JobCTRL-OSX.zip";
		private static DateTime lastExpirationWarnShown = DateTime.MinValue;

		public static bool CanStartApplication(INotificationService notificationService)
		{
			if (DateTime.UtcNow > expirationDate)
			{
				log.Error("Application is expired");
				var res = notificationService.ShowMessageBox(
					string.Format(Labels.Program_NotificationAppExpiredBody, installUrl),
					Labels.Program_NotificationAppExpiredTitle,
					MessageBoxButtons.OKCancel
				);
				if (res == DialogResult.OK)
					NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(installUrl));
				return false;
			}

			if (DateTime.UtcNow > expirationWarnDate)
			{
				log.Error("Application will expire soon");
				var res = notificationService.ShowMessageBox(
					string.Format(Labels.Program_NotificationAppWillExpireBody, installUrl),
					Labels.Program_NotificationAppWillExpireTitle,
					MessageBoxButtons.OKCancel
				);
				lastExpirationWarnShown = DateTime.UtcNow;
				if (res == DialogResult.OK)
					NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(installUrl));
			}

			//todo proper check without races?
			var runninJcs = NSWorkspace.SharedWorkspace.RunningApplications.Where(n => n.BundleIdentifier == "com.JobCTRL.JobCTRL")
				.ToArray();
			int currProcId;
			using (var currProc = Process.GetCurrentProcess())
			{
				currProcId = currProc.Id;
			}
			if (runninJcs.All(n => n.ProcessIdentifier != currProcId)) //application is modified
			{
				log.Error("Application is corrupted");
				var res = notificationService.ShowMessageBox(
					string.Format(Labels.Program_NotificationAppCorruptBody, installUrl),
					Labels.Program_NotificationAppCorruptTitle,
					MessageBoxButtons.OKCancel
				);
				if (res == DialogResult.OK)
					NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(installUrl));
				return false;
			}
			if (runninJcs.Length != 1)
			{
#if !DEBUG
				log.Error("Another JobCTRL is running");
				notificationService.ShowMessageBox(
					Labels.Program_NotificationAlreadyRunningBody,
					Labels.Program_NotificationAlreadyRunningTitle
				);
				return false;
#endif
			}

			if (!AXObject.IsApiEnabled())
			{
				log.Error("Accessibility is disabled");
				notificationService.ShowMessageBox(
					Labels.Program_NotificationEnableMacAccessibilityBody,
					Labels.Program_NotificationEnableMacAccessibilityTitle
				);
				return false;
			}

			return ConfigManager.EnsureLoggedIn(LoginWindowController.DisplayLoginForm);
		}

		public static bool CanContinueRunning(INotificationService notificationService, CurrentWorkController currentWorkController)
		{
			if (DateTime.UtcNow > expirationDate)
			{
				log.Error("Application is expired while running");
				currentWorkController.UserStopWork();
				var res = notificationService.ShowMessageBox(
					string.Format(Labels.Program_NotificationAppExpiredBody, installUrl),
					Labels.Program_NotificationAppExpiredTitle,
					MessageBoxButtons.OKCancel
				);
				if (res == DialogResult.OK)
					NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(installUrl));
				return false;
			}

			if (!AXObject.IsApiEnabled())
			{
				log.Error("Accessibility is disabled while running");
				currentWorkController.UserStopWork();
				notificationService.ShowMessageBox(
					Labels.Program_NotificationEnableMacAccessibilityBody,
					Labels.Program_NotificationEnableMacAccessibilityTitle
				);
				return false;
			}

			if (DateTime.UtcNow > expirationWarnDate
				&& DateTime.UtcNow > lastExpirationWarnShown + expirationWarnInterval)
			{
				log.Error("Application will expire soon while running");
				var res = notificationService.ShowMessageBox(
					string.Format(Labels.Program_NotificationAppWillExpireBody, installUrl),
					Labels.Program_NotificationAppWillExpireTitle,
					MessageBoxButtons.OKCancel
				);
				lastExpirationWarnShown = DateTime.UtcNow;
				if (res == DialogResult.OK)
					NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(installUrl));
			}

			return true;
		}
	}
}

