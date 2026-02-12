using AppKit;
using Foundation;
using log4net;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.ViewMac;
using static Tct.ActivityRecorderClient.ConfigManager;

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
					string.Format("Labels.Program_NotificationAppExpiredBody", installUrl),
					"Labels.Program_NotificationAppExpiredTitle",
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
					string.Format("Labels.Program_NotificationAppWillExpireBody", installUrl),
					"Labels.Program_NotificationAppWillExpireTitle",
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
					string.Format("Labels.Program_NotificationAppCorruptBody", installUrl),
					"Labels.Program_NotificationAppCorruptTitle",
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
					"Accessibility is disabled, please enable it in the settings",
					"Enable Accessibility"
				);
				return AXObject.IsApiEnabled();
			}

			// CGPreflightScreenCaptureAccess is cached so no use to periodically check nor to check if it is changed after the alert
			if (!ScreenRecordingPermission.CGPreflightScreenCaptureAccess())
			{
				log.Error("Screen recording is disabled");
				ScreenRecordingPermission.CGRequestScreenCaptureAccess();
				notificationService.ShowMessageBox(
					"Please enable Settings / Privacy & Security / Screen & System Audio Recording" + Environment.NewLine + "then restart the application",
					"Enable Screen Recording"
				);
				return false;
			}

			return true;
		}

		public static bool CanContinueRunning(INotificationService notificationService, CurrentWorkController currentWorkController)
		{
			if (DateTime.UtcNow > expirationDate)
			{
				log.Error("Application is expired while running");
				currentWorkController.UserStopWork();
				var res = notificationService.ShowMessageBox(
					string.Format("Labels.Program_NotificationAppExpiredBody", installUrl),
					"Labels.Program_NotificationAppExpiredTitle",
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
					"Accessibility is disabled, please enable it in the settings",
					"Enable Accessibility"
				);
				return AXObject.IsApiEnabled();
			}

			if (DateTime.UtcNow > expirationWarnDate
				&& DateTime.UtcNow > lastExpirationWarnShown + expirationWarnInterval)
			{
				log.Error("Application will expire soon while running");
				var res = notificationService.ShowMessageBox(
					string.Format("Labels.Program_NotificationAppWillExpireBody", installUrl),
					"Labels.Program_NotificationAppWillExpireTitle",
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

