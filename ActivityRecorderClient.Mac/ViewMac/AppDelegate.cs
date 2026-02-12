using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using log4net;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Extra;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.ViewMac;

namespace Tct.ActivityRecorderClient
{
	//todo register for automatic startup
	//todo hotkey
	//todo ManualWorkTime / countdown form
	//todo fix wcf client thread-safety issues
	[MonoMac.Foundation.Register("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string nfIdleStopWorkKey = "IdleStopWork";
		private static readonly TimeSpan nfIdleStopWorkDuration = TimeSpan.Zero;
		private const string nfPersistAndSendErrorKey = "PersistAndSendError";
		private static readonly TimeSpan nfPersistAndSendErrorDuration = TimeSpan.Zero;
		private const string nfClockSkewKey = "ClockSkew";
		private static readonly TimeSpan nfClockSkewDuration = TimeSpan.FromSeconds(30);
		private const string nfInvalidTimeCannotWorkKey = "InvalidTimeCannotWork";
		private static TimeSpan nfInvalidTimeCannotWorkDuration { get { return TimeSpan.FromMilliseconds(Math.Max(0, ConfigManager.LocalSettingsForUser.NotWorkingWarnDuration)); } }
		//private const string nfActiveOnlyKey = "ActiveOnly";
		//private static readonly TimeSpan nfActiveOnlyDuration = TimeSpan.FromSeconds(30);
		private readonly IdleDetector idleDetector = new IdleDetector();
		private readonly WorkTimeCounter workTimeCounter = new WorkTimeCounter();
		private readonly WorkTimeStatsManager workTimeStatsManager = new WorkTimeStatsManager();
		private readonly TimeManager timeManager;
		private readonly StopwatchLite swUpdateTargetEndDate = new StopwatchLite(
				TimeSpan.FromMinutes(1),
				true
			);
		private readonly ControllerCollection OwnedControllers = new ControllerCollection();
		private readonly NSImage workingOnlineImage;
		private readonly NSImage workingOfflineImage;
		private readonly NSImage notWorkingImage;
		private readonly SynchronizationContext context;
		private readonly INotificationService notificationService;
		private readonly CurrentWorkController currentWorkController;
		private readonly CaptureCoordinator captureCoordinator;
		private NSMenuItem miExit;
		private NSMenuItem miSettings;
		private NSMenuItem miLanguages;
		private NSMenuItem miLangEng;
		private NSMenuItem miLangHun;
		private NSMenuItem miChangeUser;
		private NSMenuItem miUserName;
		private NSMenuItem miCurrentWork;
		private NSMenuItem miTodaysWorkTime;
		private NSMenuItem miWorkTimeFromSrv;
		private NSMenuItem miJobCtrlWeb;
		private NSMenuItem miSettWarnings;
		private NSMenuItem miSettWarnDurations;
		private NSMenuItem miSettMenu;
#pragma warning disable 414
		private NSMenuItem miSettFlattenFactor;
		private NSMenuItem miSettNotWorkingDuration;
		private NSMenuItem miSettWorkingDuration;
		private NSMenuItem miSettMenuChangeDuration;
		private NSMenuItem miSettNotWorkingInterval;
		private NSMenuItem miSettWorkingInterval;
		private NSMenuItem miSettAutoRules;
#pragma warning restore 414
		private RecentWorksNSMenuItem miRecentWorks;
		private RecentUrlNSMenuItem miLastUrl;
		private MenuMacBuilder menuBuilder;
		private NSTimer taskbarTimer;
		private NSStatusItem niTaskBar;
		private NSImage workingImage; //workingOnlineImage or workingOfflineImage
		private bool isStarted;
		private bool logoutOnStop;
		private bool stopping;

		public AppDelegate()
		{
			//log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(@"/Users/z/Projects/ActivityRecorder/ActivityRecorderClient/app.config"));
#if DEBUG
			Debug.Listeners.Add(new DebugListener());
#endif
			System.Net.ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback; //(_,__,___,____) => true;

			var height = NSStatusBar.SystemStatusBar.Thickness - 3;
			var width = height;
			workingOnlineImage = ImageHelper.GetScaledImage(
				"WorkingOnline.png",
				new SizeF(width, height)
			);
			workingOfflineImage = ImageHelper.GetScaledImage(
				"WorkingOffline.png",
				new SizeF(width, height)
			);
			notWorkingImage = ImageHelper.GetScaledImage(
				"NotWorking.png",
				new SizeF(width, height)
			);
			workingImage = workingOnlineImage;
			context = new NSRunLoopSynchronizationContext();
			//Console.WriteLine("ctor: " + Thread.CurrentThread.ManagedThreadId);
			//context.Post(_ => Console.WriteLine(Thread.CurrentThread.ManagedThreadId), null);

			((Platform.PlatformMacFactory)Platform.Factory).OwnedControllers = OwnedControllers; //hax for external dependency for RuleManagementMacService

			notificationService = Platform.Factory.GetNotificationService();
			captureCoordinator = new CaptureCoordinator(
				context,
				notificationService,
				CurrentWorkControllerPropertyChanged
			); //if Assert is raised (e.g. in PeriodicManager's ctor) taskbarTimer_Tick would fire so to avoid nullRef we create it before InitializeComponent()
			currentWorkController = captureCoordinator.CurrentWorkController;
			timeManager = new TimeManager(captureCoordinator.SystemEventsService);
		}

		//avoid man in the middle attacks
		private bool RemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
#if DEBUG
			if (1.ToString()=="1") return true;
#endif
			var result = certificate == null
				? false
				: certificate.GetPublicKeyString() == "3082010A0282010100979C0E4038837F0B7460BFC50881DCBBC684647F7752A9DC14C316B633CE3B70D950F4F6BE4D670CA4F0BE71E8463EE7A69915611DDAA2E9FAC68A446E4118EF4E9FCE53965ED4E4C298703D6BA48CBF8BFA202604F51CB7607FC1B19BA46F5A4643CF20BCA75F3FA5F839DF88D43E9074A989DF7C0D3965A97E1929C2E26E5F32FD309E08658F7B971A86B5476480276ED7525B309AB30711575A16C6E43DE2A5756C2843544FB6F9282372D204E446AF9316B7630F5E31FF75B5ACE41AA73E52DEB8EC843D27F30610F541EDC54CCBC2F3F3457330F59427F1E5E41A20DA896A3A82B0171408E4BA6012E45BBC67DAF662A6CA488752065B60BAA681310CD50203010001";
			if (!result)
			{
				log.Error("Invalid certificate: " + Convert.ToString(certificate).Replace(
					Environment.NewLine,
					" "
				)
				);
			}
			return result;
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
		{
			return false;
		}

		//http://www.cimgf.com/2008/03/26/cocoa-tutorial-awakefromnib-vs-applicationdidfinishlaunching/
		public override void FinishedLaunching(NSObject notification)
		{
			log.Info("FinishedLaunching");

			if (!PrerequisiteHelper.CanStartApplication(notificationService))
			{
				NSApplication.SharedApplication.Terminate(this);
				return;
			}

			LocalizationHelper.InitLocalization();

			miSettWarnings = new NSMenuItem(Labels.Menu_PopupIntervals);
			miSettWarnDurations = new NSMenuItem(Labels.Menu_PopupDurations);
			miSettMenu = new NSMenuItem(Labels.Menu_Menu);
			miSettMenu.Submenu = new NSMenu();
			miSettMenu.Submenu.AddItem(miSettFlattenFactor = new NSMenuItem(
				Labels.Menu_MenuFlattenFactor,
				miMenuFlattenFactor_Click
			)
			);
			miSettWarnDurations.Submenu = new NSMenu();
			miSettWarnDurations.Submenu.AddItem(miSettNotWorkingDuration = new NSMenuItem(
				Labels.Menu_NotWorking,
				miNotWorkingWranDuration_Click
			)
			);
			miSettWarnDurations.Submenu.AddItem(miSettWorkingDuration = new NSMenuItem(
				Labels.Menu_Working,
				miWorkingWarnDuration_Click
			)
			);
			miSettWarnDurations.Submenu.AddItem(miSettMenuChangeDuration = new NSMenuItem(
				Labels.Menu_MenuChange,
				miMenuChangeWarnDuration_Click
			)
			);
			miSettWarnings.Submenu = new NSMenu();
			miSettWarnings.Submenu.AddItem(miSettNotWorkingInterval = new NSMenuItem(
				Labels.Menu_NotWorking,
				miNotWorkingWranInterval_Click
			)
			);
			miSettWarnings.Submenu.AddItem(miSettWorkingInterval = new NSMenuItem(
				Labels.Menu_Working,
				miWorkingWarnInterval_Click
			)
			);

			miCurrentWork = new NSMenuItem(
				Labels.Menu_NoWorkToContinue,
				CurrentWorkClick
			);
			miTodaysWorkTime = new NSMenuItem();
			miWorkTimeFromSrv = new NSMenuItem();
			miExit = new NSMenuItem(Labels.Menu_Exit, miExit_Click);
			miSettings = new NSMenuItem(Labels.Menu_Settings);
			miLanguages = new NSMenuItem(Labels.Menu_Languages);
			miUserName = new NSMenuItem();
			miChangeUser = new NSMenuItem(
				Labels.Menu_ChangeUser + " (" + ConfigManager.UserId + ")",
				miChangeUser_Click
			);
			miLangEng = new NSMenuItem("English", miLangEng_Click);
			miLangHun = new NSMenuItem("Magyar", miLangHun_Click);
			miRecentWorks = new RecentWorksNSMenuItem(Labels.Menu_RecentWorks);
			miLastUrl = new RecentUrlNSMenuItem();
			miJobCtrlWeb = new NSMenuItem(Labels.Menu_MainPage);
			miJobCtrlWeb.Submenu = new NSMenu();
			foreach (var item in miLastUrl.UrlItems)
			{
				miJobCtrlWeb.Submenu.AddItem(item);
			}
			miSettings.Submenu = new NSMenu();
			miSettings.Submenu.AddItem(miChangeUser);
			miSettings.Submenu.AddItem(miSettAutoRules = new NSMenuItem(
				Labels.Menu_WorkDetectorRules,
				miSettAutoRules_Click
			)
			);
			miSettings.Submenu.AddItem(miSettWarnings);
			miSettings.Submenu.AddItem(miSettWarnDurations);
			miSettings.Submenu.AddItem(miSettMenu);
			miSettings.Submenu.AddItem(miLanguages);
			miSettings.Submenu.AddItem(miJobCtrlWeb);
			miLanguages.Submenu = new NSMenu();
			miLanguages.Submenu.AddItem(miLangEng);
			miLanguages.Submenu.AddItem(miLangHun);

			niTaskBar = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusBar.SystemStatusBar.Thickness + 50);
			niTaskBar.HighlightMode = true;
			niTaskBar.Title = "--:--";
			niTaskBar.Image = notWorkingImage;

			//niTaskBar.DoubleClick += (sender, e) => Console.WriteLine("Double Click");

			var menu = new NSMenu();
			menuBuilder = new MenuMacBuilder(menu);
			menuBuilder.MenuClick += MenuBuilderMenuClick;

			niTaskBar.Menu = menu;
			niTaskBar.Menu.AddItem(miCurrentWork);
			niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
			niTaskBar.Menu.AddItem(miTodaysWorkTime);
			niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
			niTaskBar.Menu.AddItem(miWorkTimeFromSrv);
			niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
			niTaskBar.Menu.AddItem(menuBuilder.PlaceHolder);
			niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
			niTaskBar.Menu.AddItem(miRecentWorks);
			niTaskBar.Menu.AddItem(NSMenuItem.SeparatorItem);
			niTaskBar.Menu.AddItem(miLastUrl);
			niTaskBar.Menu.AddItem(miSettings);
			niTaskBar.Menu.AddItem(miExit);

			captureCoordinator.WorkItemCreated += CaptureCoordinatorWorkItemCreated;
			captureCoordinator.WorkItemManager.ConnectionStatusChanged += WorkItemManagerConnectionStatusChanged;
			captureCoordinator.WorkItemManager.CannotPersistAndSendWorkItem += WorkItemManagerCannotPersistAndSendWorkItem;
			captureCoordinator.CurrentMenuChanged += MenuManagerCurrentMenuChanged;
			captureCoordinator.Start();
			isStarted = true;

			workTimeCounter.Load();
			log.Debug("workTimeCounter Loaded");
			menuBuilder.UpdateMenu(captureCoordinator.CurrentMenu);
			log.Debug("menuBuilder Menu Updated");
			miRecentWorks.MenuClick += MenuBuilderMenuClick;
			miRecentWorks.LoadRecentWorks(captureCoordinator.CurrentMenu);
			log.Debug("recentWorks Loaded");
			timeManager.ClockSkewError += ClockSkewError;
			timeManager.Start();
			log.Debug("timeManager Started");
			workTimeStatsManager.TotalWorkTimeStatsReceived += TotalWorkTimeStatsReceived;
			workTimeStatsManager.PasswordError += PasswordError;
			//workTimeStatsManager.ActiveOnlyError += ActiveOnlyError;
			workTimeStatsManager.Start();
			log.Debug("workTimeStatsManager Started");

			UpdateWorkTimeAndTaskbarInfo(null);

			taskbarTimer = NSTimer.CreateRepeatingTimer(
				TimeSpan.FromSeconds(1),
				taskbarTimer_Tick
			);
			NSRunLoop.Current.AddTimer(taskbarTimer, NSRunLoopMode.Common); //update menu while being displayed http://stackoverflow.com/questions/6301338/update-nsmenuitem-while-the-host-menu-is-shown

		}

		public override void WillTerminate(NSNotification notification)
		{
			log.Info("WillTerminate");
			stopping = true;
			currentWorkController.IsShuttingDown = true;
			if (!isStarted)
				return;
			if (taskbarTimer != null)
			{
				//if (taskbarTimer.IsValid)	taskbarTimer.Invalidate(); //this before Dispose frzore once.... don't know why, but Dispose should Invalidate anyway
				log.Info("Disposing gui timer");
				taskbarTimer.Dispose();
				taskbarTimer = null;
			}
			log.Info("Stopping captureCoordinator");
			captureCoordinator.Stop();
			captureCoordinator.CurrentMenuChanged -= MenuManagerCurrentMenuChanged;
			captureCoordinator.WorkItemManager.ConnectionStatusChanged -= WorkItemManagerConnectionStatusChanged;
			captureCoordinator.WorkItemManager.CannotPersistAndSendWorkItem -= WorkItemManagerCannotPersistAndSendWorkItem;
			captureCoordinator.WorkItemCreated -= CaptureCoordinatorWorkItemCreated;
			captureCoordinator.Dispose();
			log.Debug("captureCoordinator Disposed");
			timeManager.Stop();
			timeManager.ClockSkewError -= ClockSkewError;
			log.Debug("timeManager Stopped");
			workTimeStatsManager.Stop();
			workTimeStatsManager.TotalWorkTimeStatsReceived -= TotalWorkTimeStatsReceived;
			workTimeStatsManager.PasswordError -= PasswordError;
			//workTimeStatsManager.ActiveOnlyError -= ActiveOnlyError;
			log.Debug("workTimeStatsManager Stopped");
			menuBuilder.MenuClick -= MenuBuilderMenuClick;
			miRecentWorks.MenuClick -= MenuBuilderMenuClick;
			//autoUpdateManager.Stop();
			//autoUpdateManager.NewVersionInstalledEvent -= AutoUpdateManagerNewVersionInstalledEvent;
			if (logoutOnStop)
			{
				ConfigManager.Logout();
			}
			log.Info("Form closed");
		}

		private void CurrentWorkControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "CurrentWork")
				return;
			var cwc = (CurrentWorkController)sender;
			//display current workitem in menu (dis/enable)

			bool isWorking = cwc.CurrentWorkState == WorkState.Working || cwc.CurrentWorkState == WorkState.WorkingTemp;
			miCurrentWork.Title = (isWorking ? '\u25FC' : '\u25BA') + " " + cwc.CurrentOrLastWorkName;

			//miCurrentWork.Enabled = isWorking;
			//display current workitem in tooltip and recalculate worktimes
			UpdateWorkTimeAndTaskbarInfo(isWorking ? cwc.CurrentOrLastWorkNameInTwoLines : null);
			//display icon
			//todo different icon for NotWorkingTemp
			niTaskBar.Image = isWorking ? workingImage : notWorkingImage;
			//misc.
			if (isWorking)
			{
				notificationService.HideNotification(nfIdleStopWorkKey);
				if (cwc.IsCurrentWorkValid)
					miRecentWorks.AddRecentWork(cwc.CurrentWork);
			}
			else
			{
				idleDetector.ResetIdleWorkTime();
			}
		}

		private static readonly string taskbarHeader = "JobCTRL \uF8FF v" + ConfigManager.Version;
		private bool userNameSet;

		private void UpdateWorkTimeAndTaskbarInfo(string currentWork)
		{
			if (!userNameSet && ConfigManager.UserName != null)
			{
				userNameSet = true;
				using (var userName = new NSMutableAttributedString(ConfigManager.UserName + " (" + ConfigManager.UserId + ")"))
				{
					miUserName.AttributedTitle = userName;
					niTaskBar.Menu.InsertItematIndex(NSMenuItem.SeparatorItem, 2);
					niTaskBar.Menu.InsertItematIndex(miUserName, 2);
				}
			}
			var title = workTimeStatsManager.TodaysWorkTime.ToHourMinuteString();
			if (title == "")
				title = workTimeCounter.TodaysWorkTime.ToHourMinuteString() + ".";
			niTaskBar.Title = title;
			string workTime = workTimeCounter.TodaysWorkTime.ToHourMinuteSecondString();
			string textToSet = taskbarHeader + "\n"
				+ workTime
				+ (currentWork == null ? "" : "\n" + currentWork);
			niTaskBar.ToolTip = textToSet;

			//todo only set if changed
			using (var titleTodaysWorkTime = new NSAttributedString(Labels.TodaysLocalWorkTime + ": " + workTime +
				"\n" + string.Format(Labels.NumberOfItemsToUpload, captureCoordinator.WorkItemManager.UnsentItemsCount)))
				miTodaysWorkTime.AttributedTitle = titleTodaysWorkTime;
			using (var titleWorkTimeFromSrv = new NSAttributedString(workTimeStatsManager.WorkTimeStatsString))
				miWorkTimeFromSrv.AttributedTitle = titleWorkTimeFromSrv;
			miWorkTimeFromSrv.ToolTip = workTimeStatsManager.WorkTimeStatsStringTooltip;

			currentWorkController.ShowPeriodicNotificationIfApplicable();
			if (swUpdateTargetEndDate.IsIntervalElapsedSinceLastCheck())
			{
				menuBuilder.UpdateTargetEndDatePercentages();
			}
		}

		private int prerequisiteCheck;

		private void taskbarTimer_Tick()
		{
			bool isWorking = currentWorkController.CurrentWorkState == WorkState.Working || currentWorkController.CurrentWorkState == WorkState.WorkingTemp;
			UpdateWorkTimeAndTaskbarInfo(isWorking ? currentWorkController.CurrentOrLastWorkNameInTwoLines : null);
			if (++prerequisiteCheck % 30 == 0)
			{
				prerequisiteCheck = 0;
				if (!PrerequisiteHelper.CanContinueRunning(
					notificationService,
					currentWorkController
				))
				{
					NSApplication.SharedApplication.Terminate(this);
					return;
				}
			}
//			if (notificationService.IsActive(nfIdleStopWorkKey)) //if idle notification is active check for new version and restart
//			{
//				RestartIfNewVersionInstalled(true);
//			}
			if (!idleDetector.IsIdleAfterWorkTime)
				return;
			log.Info("Idling and working after work time so stop working");
			if (currentWorkController.CurrentWorkState != WorkState.NotWorking)
			{
				currentWorkController.UserStopWork();
			}
			ShowIdleMessage();
		}

		private void ShowIdleMessage()
		{
			notificationService.ShowNotification(nfIdleStopWorkKey, nfIdleStopWorkDuration,
												 Labels.NotificationWorkIdleTitle, Labels.NotificationWorkIdleBody,
												 CurrentWorkController.NotWorkingColor);
		}

		private void MenuManagerCurrentMenuChanged(object sender, MenuEventArgs e)
		{
			menuBuilder.UpdateMenu(e.Menu);
			miRecentWorks.UpdateMenu(e.Menu);
		}

		private bool CanStartWorkOrWarn()
		{
			if (currentWorkController.CurrentWorkState == WorkState.NotWorking && timeManager.IsTimeInvalid)
			{
				log.Info("Trying to start work but client time is invalid");
				notificationService.ShowNotification(nfInvalidTimeCannotWorkKey, nfInvalidTimeCannotWorkDuration,
				                                     Labels.NotificationCannotStartUserSelectedWorkInvalidTimeTitle, Labels.NotificationCannotStartUserSelectedWorkInvalidTimeBody, CurrentWorkController.NotWorkingColor);
				return false;
			}
			return true;
		}

		private void CurrentWorkClick(object sender, EventArgs e)
		{
			if (currentWorkController.CurrentWork == null)
			{
				if (!CanStartWorkOrWarn()) return;
				currentWorkController.UserResumeWork();
			}
			else
			{
				currentWorkController.UserStopWork();
			}
		}

		private void MenuBuilderMenuClick(object sender, WorkDataEventArgs e)
		{
			Debug.Assert(e.WorkData.Id.HasValue);
			if (e.WorkData.ManualAddWorkDuration.HasValue)
			{
				//todo not supported atm.
				return;
			}
			if (!CanStartWorkOrWarn()) return;
			currentWorkController.UserStartWork(e.WorkData);
		}

		private void CaptureCoordinatorWorkItemCreated(object sender, WorkItemEventArgs e)
		{
			workTimeCounter.AddWorkItem(e.WorkItem);
			idleDetector.AddWorkItem(e.WorkItem);
		}

		private void WorkItemManagerCannotPersistAndSendWorkItem(object sender, EventArgs e)
		{
			context.Post(_ =>
				notificationService.ShowNotification(
				nfPersistAndSendErrorKey,
				nfPersistAndSendErrorDuration,
				Labels.NotificationPersistAndSendErrorTitle,
				Labels.NotificationPesistAndSendErrorBody
			)
				, null);
		}

		private void WorkItemManagerConnectionStatusChanged(object sender, EventArgs e)
		{
			if (captureCoordinator.WorkItemManager.IsOnline)
			{
				context.Post(_ =>
				{
					if (stopping)
						return;
					workingImage = workingOnlineImage;
					if (niTaskBar.Image != notWorkingImage)
					{
						niTaskBar.Image = workingImage;
					}
				}, null);
			}
			else
			{
				context.Post(_ =>
				{
					if (stopping)
						return;
					workingImage = workingOfflineImage;
					if (niTaskBar.Image != notWorkingImage)
					{
						niTaskBar.Image = workingImage;
					}
				}, null);
			}
		}

		private StopwatchLite swPassDialog = new StopwatchLite(1, true);

		private void PasswordError(object sender, EventArgs e)
		{
			context.Post(_ =>
			{
				if (stopping)
					return;
				if (swPassDialog == null || !swPassDialog.IsIntervalElapsedSinceLastCheck())
					return;
				swPassDialog = null; //don't show pass dialog again
				try
				{
					ConfigManager.ShowPasswordDialog(
						LoginWindowController.ShowChangePasswordDialog,
						notificationService
					);
				}
				finally
				{
					//hax don't show pass dialog for another 1 min
					//(so we won't notice errors from the pervious bad password)
					swPassDialog = new StopwatchLite(TimeSpan.FromMinutes(1), false);
				}
			}, null);
		}

		private void ClockSkewError(object sender, SingleValueEventArgs<ClockSkewData> e)
		{
			context.Post(skewData =>
			{
				if (stopping)
					return;
				var clockSkewData = (ClockSkewData)skewData;
				notificationService.ShowNotification(nfClockSkewKey, nfClockSkewDuration, Labels.NotificationClockSkewErrorTitle,
				                                     string.Format(Labels.NotificationClockSkewErrorBody, clockSkewData.ClientTime, clockSkewData.ServerTime));
			}, e.Value);
		}

		private void miExit_Click(object sender, EventArgs e)
		{
			if (currentWorkController.CurrentWorkState != WorkState.NotWorking) //we also need to confirm when not working temp
			{
				var result = notificationService.ShowMessageBox(
					Labels.ConfirmExitStillWorkingBody,
					Labels.ConfirmExitStillWorkingTitle,
					MessageBoxButtons.OKCancel
				);
				if (result != DialogResult.OK)
					return;
				log.Info("Exit confirmed while working");
			}
			else
			{
				log.Info("Exit clicked and not working");
			}
			NSApplication.SharedApplication.Terminate(this);
		}

		private void miLangEng_Click(object sender, EventArgs e)
		{
			var newCulture = new System.Globalization.CultureInfo("en-US");
			LocalizationHelper.SaveLocalization(newCulture);
			ShowLanguageChangeNotification(newCulture);
		}

		private void miLangHun_Click(object sender, EventArgs e)
		{
			var newCulture = new System.Globalization.CultureInfo("hu-HU");
			LocalizationHelper.SaveLocalization(newCulture);
			ShowLanguageChangeNotification(newCulture);
		}

		private void ShowLanguageChangeNotification(System.Globalization.CultureInfo newCulture)
		{
			var title = Labels.ResourceManager.GetString(
				"NotificationLanguageChangeTitle",
				newCulture
			);
			var body = Labels.ResourceManager.GetString(
				"NotificationLanguageChangeBody",
				newCulture
			);
			notificationService.ShowMessageBox(body, title, MessageBoxButtons.OK);
		}

		private void miChangeUser_Click(object sender, EventArgs e)
		{
			var res = notificationService.ShowMessageBox(Labels.ConfirmChangeUserManualRestartBody, Labels.ConfirmChangeUserTitle, MessageBoxButtons.OKCancel);
			if (res != DialogResult.OK)
				return;
			logoutOnStop = true;
			//todo Restart
		}

		private void TotalWorkTimeStatsReceived(object sender, SingleValueEventArgs<TotalWorkTimeStats> e)
		{
			context.Post(stats =>
			{
				if (stopping)
					return;
				var totalStats = (TotalWorkTimeStats)stats;
				var sumTime = new TimeSpan(totalStats.Stats.Values.Sum(n => n.TotalWorkTime.Ticks));
				miUserName.ToolTip = string.Format(
					Labels.SumWorkHours,
					sumTime.TotalHours.ToString("0.#")
				);
				menuBuilder.UpdateTargetTotalWorkTimePercentages(totalStats);
			}, e.Value);
		}

		private void miNotWorkingWranInterval_Click(object sender, EventArgs e)
		{
			var ctrl = new SetIntWindowController();
			OwnedControllers.Add(ctrl, (_,__) => {
				if (!ctrl.Window.ShouldUseValue)
					return;
				ConfigManager.LocalSettingsForUser.NotWorkingWarnInterval = ctrl.Window.Value * 1000;
			}
			);
			ctrl.Window.Title = Labels.NotWorkingWarnIntervalInSec;
			ctrl.Window.Description = Labels.NotWorkingWarnIntervalInSec;
			ctrl.Window.ValueTitle = Labels.Value + ":";
			ctrl.Window.Value = ConfigManager.LocalSettingsForUser.NotWorkingWarnInterval / 1000;
			ctrl.Window.Show();
		}

		private void miWorkingWarnInterval_Click(object sender, EventArgs e)
		{
			var ctrl = new SetIntWindowController();
			OwnedControllers.Add(ctrl, (_,__) => {
				if (!ctrl.Window.ShouldUseValue)
					return;
				ConfigManager.LocalSettingsForUser.WorkingWarnInterval = ctrl.Window.Value * 1000;
			}
			);
			ctrl.Window.Title = Labels.WorkingWarnIntervalInSec;
			ctrl.Window.Description = Labels.WorkingWarnIntervalInSec;
			ctrl.Window.ValueTitle = Labels.Value + ":";
			ctrl.Window.Value = ConfigManager.LocalSettingsForUser.WorkingWarnInterval / 1000;
			ctrl.Window.Show();
		}

		private void miNotWorkingWranDuration_Click(object sender, EventArgs e)
		{
			var ctrl = new SetIntWindowController();
			OwnedControllers.Add(ctrl, (_,__) => {
				if (!ctrl.Window.ShouldUseValue)
					return;
				ConfigManager.LocalSettingsForUser.NotWorkingWarnDuration = ctrl.Window.Value * 1000;
			}
			);
			ctrl.Window.Title = Labels.NotWorkingWarnDurationInSec;
			ctrl.Window.Description = Labels.NotWorkingWarnDurationInSec;
			ctrl.Window.ValueTitle = Labels.Value + ":";
			ctrl.Window.Value = ConfigManager.LocalSettingsForUser.NotWorkingWarnDuration / 1000;
			ctrl.Window.Show();
		}

		private void miWorkingWarnDuration_Click(object sender, EventArgs e)
		{
			var ctrl = new SetIntWindowController();
			OwnedControllers.Add(ctrl, (_,__) => {
				if (!ctrl.Window.ShouldUseValue)
					return;
				ConfigManager.LocalSettingsForUser.WorkingWarnDuration = ctrl.Window.Value * 1000;
			}
			);
			ctrl.Window.Title = Labels.WorkingWarnDurationInSec;
			ctrl.Window.Description = Labels.WorkingWarnDurationInSec;
			ctrl.Window.ValueTitle = Labels.Value + ":";
			ctrl.Window.Value = ConfigManager.LocalSettingsForUser.WorkingWarnDuration / 1000;
			ctrl.Window.Show();
		}

		private void miMenuChangeWarnDuration_Click(object sender, EventArgs e)
		{
			var ctrl = new SetIntWindowController();
			OwnedControllers.Add(ctrl, (_,__) => {
				if (!ctrl.Window.ShouldUseValue)
					return;
				ConfigManager.LocalSettingsForUser.MenuChangeWarnDuration = ctrl.Window.Value * 1000;
			}
			);
			ctrl.Window.Title = Labels.MenuChangeWarnDurationInSec;
			ctrl.Window.Description = Labels.MenuChangeWarnDurationInSec;
			ctrl.Window.ValueTitle = Labels.Value + ":";
			ctrl.Window.Value = ConfigManager.LocalSettingsForUser.MenuChangeWarnDuration / 1000;
			ctrl.Window.Show();
		}

		private void miMenuFlattenFactor_Click(object sender, EventArgs e)
		{
			var ctrl = new SetIntWindowController();
			OwnedControllers.Add(ctrl, (_,__) => {
				if (!ctrl.Window.ShouldUseValue)
					return;
				ConfigManager.LocalSettingsForUser.MenuFlattenFactor = ctrl.Window.Value;
				menuBuilder.UpdateMenu(captureCoordinator.CurrentMenu);
			}
			);
			ctrl.Window.Title = Labels.MenuFlattenFactor;
			ctrl.Window.Description = Labels.MenuFlattenFactor;
			ctrl.Window.ValueTitle = Labels.Value + ":";
			ctrl.Window.Value = ConfigManager.LocalSettingsForUser.MenuFlattenFactor;
			ctrl.Window.Show();
		}

		private void miSettAutoRules_Click(object sender, EventArgs e)
		{
			captureCoordinator.RuleManagementService.DisplayWorkDetectorRulesEditingGui(false);
		}
	}
}

