using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Microsoft.Win32;
using Tct.ActivityRecorderClient.ActiveDirectoryIntegration;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Desktop;
using Tct.ActivityRecorderClient.Capturing.Extra;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Tct.ActivityRecorderClient.ClientErrorReporting;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Hotkeys;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Menu.Management;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.Stability;
using Tct.ActivityRecorderClient.Update;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.ToolStrip;
using Tct.ActivityRecorderClient.Meeting;
using Tct.ActivityRecorderClient.MessageNotifier;
using Tct.ActivityRecorderClient.ProjectSync;
using Tct.ActivityRecorderClient.Taskbar;
using Tct.ActivityRecorderClient.TodoLists;
using Tct.ActivityRecorderClient.WorktimeHistory;
using Message = System.Windows.Forms.Message;

namespace Tct.ActivityRecorderClient.View
{
	//?todo fix windows7 hook timeout bug without periodic refreshes
	//?todo fix windows7 unlock lag?
	//?todo research ClickOnce NullRef bug which prevents updates and restarts (maybe it has to do sg with sleep)
	public partial class ActivityRecorderForm : Form, ILocalizableControl
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly TimeSpan nfIdleStopWorkDuration = TimeSpan.Zero;
		private static readonly TimeSpan nfPersistAndSendErrorDuration = TimeSpan.Zero;
		private static readonly TimeSpan nfActiveOnlyDuration = TimeSpan.FromSeconds(30);
		private static readonly TimeSpan nfAssigntaskErrorDuration = TimeSpan.Zero;
		//this doesn't always work if we do the assignment in ctor it's ok
		//private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;

		private readonly SynchronizationContext context;

		private readonly WorkTimeCounter workTimeCounter = new WorkTimeCounter();

		private readonly WorkTimeStatsFromWebsiteManager workTimeStatsFromWebsiteManager;
		private readonly INotificationService notificationService = Platform.Factory.GetNotificationService();
		private readonly AutoUpdateManager autoUpdateManager = new AutoUpdateManager();
		private readonly ClientSettingsManager clientSettingsManager = new ClientSettingsManager();
		private readonly WorkTimeStatsManager workTimeStatsManager = new WorkTimeStatsManager();
		private readonly AllWorksManager allWorksManager = new AllWorksManager();
		private readonly IWorkManagementService workManagementService;
		private readonly MenuBuilder menuBuilder;
		private readonly WorkTimeStatsMenuItem workTimeStatsMenu = new WorkTimeStatsMenuItem();
		private readonly ToolStripLabelWithButton currentWorkItem = new ToolStripLabelWithButton(Labels.Menu_NoWorkToContinue) { Enabled = false, TextAlign = ContentAlignment.MiddleLeft, ButtonText = "+", IsButtonVisible = true, ButtonToolTipText = Labels.Menu_AddReasonToCurrentWorkOnClick };
		private readonly ToolStripMenuItem offlineWork = new ToolStripMenuItem(Labels.Menu_UploadWorkItemsStop) { Enabled = false, Available = false };
		private readonly RecentWorksMenuItem recentWorks = new RecentWorksMenuItem(Labels.Menu_RecentWorks);
		private readonly SearchWorksMenuItem searchWorks = new SearchWorksMenuItem();
		private readonly RecentUrlMenuItem lastUrl = new RecentUrlMenuItem();
		private readonly ToolStripItem userNameItem = new ToolStripLabel("");
		private readonly ToolStripButtonRenderer toolStripRenderer = new ToolStripButtonRenderer();
		private readonly CurrentWorkController currentWorkController;
		private readonly IHotkeyService hotkeyService = Platform.Factory.GetHotkeyService();
		private readonly IWorkTimeService workTimeService;
		private readonly IProjectSyncService projectSyncService;
		private readonly HotkeyRegistrar hotkeyRegistrar;
		private readonly FreezeHandlingService freezeHandlingService;
		private readonly StopwatchLite swUpdateTargetEndDate = new StopwatchLite(TimeSpan.FromMinutes(1), true);
		private readonly CaptureCoordinator captureCoordinator;
		private ActiveDirectoryAuthenticationManager activeDirectoryAuthManager;
		private Point? lastContextPosition = null;
		private readonly NetworkStabilityManager networkStabilityManager;
		private readonly MessageManager messageManager;
		private readonly TodoListsManager todoManager;
		private readonly AcquireLogsManager acquireLogsManager;
		private readonly MenuReportHelper menuReportHelper = new MenuReportHelper();
		private readonly MenuTabHelper menuTabHelper = new MenuTabHelper();
		private AcceptanceData dppData;
		private bool DppNeedAccept;
		private bool init2completed;
		public bool IsDuringExit { get; private set; }
		private volatile bool isOfflineWorkWindowSessionSwitchPending;
		private readonly AutoResetEvent offlineWorkWindowSessionSwitchResetEvent = new AutoResetEvent(false);
		private WatchdogManager guiThreadWatchdogManager;

		private readonly Icon iconNotWorking = Resources.NotWorking;
		private readonly Icon iconWorkingOnline = Resources.WorkingOnline;
		private readonly Icon iconWorkingOffline = Resources.WorkingOffline;
		private readonly Icon iconWorkingLockOnline = Resources.WorkingLockOnline;
		private readonly Icon iconWorkingLockOffline = Resources.WorkingLockOffline;

		private bool stopping;
		private bool logoutOnStop;
		private bool restartForNewVersion;

		private static string IdleQuitPath { get { return "IdleQuit-" + ConfigManager.UserId; } }
		private static string NewFeaturesPath { get { return "NewFeatures-" + ConfigManager.UserId; } }
		private static string WelcomeStatePath { get { return "WelcomeState-" + ConfigManager.UserId; } }
		public static string ActiveWorkIdPath { get { return "ActiveWorkId-" + ConfigManager.UserId; } }

		private bool lastSearchAllWorks;
		private List<AllWorkItem> allWorkItems;
		private readonly ContextMenu contextMenuForm;
		private FirstTimeWelcomeForm welcomeForm;

		public event EventHandler ConfigChanged;

		public NotificationPosition NotificationPosition
		{
			get
			{
				return notificationService.Position;
			}

			set
			{
				notificationService.Position = value;
			}
		}

		public int RecentItemCount
		{
			get
			{
				return ConfigManager.LocalSettingsForUser.MenuRecentItemsCount;
			}

			set
			{
				ConfigManager.LocalSettingsForUser.MenuRecentItemsCount = Math.Min(Math.Max(0, value), 100);
				recentWorks.TrimExcess();
				RaiseConfigChange();
			}
		}

		public WorkTimeStatsManager WorkTimeStatsManager
		{
			get
			{
				return workTimeStatsManager;
			}
		}

		public WorkTimeStatsFromWebsiteManager WorkTimeStatsFromWebsiteManager => workTimeStatsFromWebsiteManager;

		public IWorkTimeService HandleWorkTimeService
		{
			get
			{
				return workTimeService;
			}
		}

		public bool ShowDynamicWorks
		{
			get
			{
				return ConfigManager.LocalSettingsForUser.ShowDynamicWorks;
			}

			set
			{
				if (ConfigManager.LocalSettingsForUser.ShowDynamicWorks != value)
				{
					ConfigManager.LocalSettingsForUser.ShowDynamicWorks = value;
					menuBuilder.UpdateMenu(captureCoordinator.CurrentMenuLookup);
				}
			}
		}

		public bool ShowSearch
		{
			get
			{
				return ConfigManager.LocalSettingsForUser.DisplaySearchWorks;
			}

			set
			{
				if (ConfigManager.LocalSettingsForUser.DisplaySearchWorks != value)
				{
					ConfigManager.LocalSettingsForUser.DisplaySearchWorks = value;
					searchWorks.Available = value;
				}
			}
		}

		public bool ShowWeeklyStats
		{
			get => ConfigManager.LocalSettingsForUser.DisplayThisWeeksStats;

			set
			{
				if (ConfigManager.LocalSettingsForUser.DisplayThisWeeksStats != value)
				{
					ConfigManager.LocalSettingsForUser.DisplayThisWeeksStats = value;
					if (value)
						ConfigManager.LocalSettingsForUser.DisplayWorktimeStats |= WorktimeStatIntervals.Week;
					else
						ConfigManager.LocalSettingsForUser.DisplayWorktimeStats &= ~WorktimeStatIntervals.Week;
					workTimeStatsMenu.WorkTimeStatsControl.SetWeeksVisible(value);
					RaiseConfigChange();
				}
			}
		}

		public bool ShowQuarterlyStats
		{
			get => (ConfigManager.LocalSettingsForUser.DisplayWorktimeStats & WorktimeStatIntervals.Quarter) == WorktimeStatIntervals.Quarter;

			set
			{
				if (((ConfigManager.LocalSettingsForUser.DisplayWorktimeStats & WorktimeStatIntervals.Quarter) == WorktimeStatIntervals.Quarter) == value) return;
				if (value)
					ConfigManager.LocalSettingsForUser.DisplayWorktimeStats |= WorktimeStatIntervals.Quarter;
				else
					ConfigManager.LocalSettingsForUser.DisplayWorktimeStats &= ~WorktimeStatIntervals.Quarter;
				RaiseConfigChange();
			}
		}

		public bool ShowYearlyStats
		{
			get => (ConfigManager.LocalSettingsForUser.DisplayWorktimeStats & WorktimeStatIntervals.Year) == WorktimeStatIntervals.Year;

			set
			{
				if (((ConfigManager.LocalSettingsForUser.DisplayWorktimeStats & WorktimeStatIntervals.Year) == WorktimeStatIntervals.Year) == value) return;
				if (value)
					ConfigManager.LocalSettingsForUser.DisplayWorktimeStats |= WorktimeStatIntervals.Year;
				else
					ConfigManager.LocalSettingsForUser.DisplayWorktimeStats &= ~WorktimeStatIntervals.Year;
				RaiseConfigChange();
			}
		}

		public bool ShowMonthlyStats
		{
			get => (ConfigManager.LocalSettingsForUser.DisplayWorktimeStats & WorktimeStatIntervals.Month) == WorktimeStatIntervals.Month;

			set
			{
				if (((ConfigManager.LocalSettingsForUser.DisplayWorktimeStats & WorktimeStatIntervals.Month) == WorktimeStatIntervals.Month) == value) return;
				if (value)
					ConfigManager.LocalSettingsForUser.DisplayWorktimeStats |= WorktimeStatIntervals.Month;
				else
					ConfigManager.LocalSettingsForUser.DisplayWorktimeStats &= ~WorktimeStatIntervals.Month;
				RaiseConfigChange();
			}
		}

		public bool ShowSum
		{
			get => ConfigManager.DisplayOptions != null ? ConfigManager.DisplayOptions.Value.HasFlag(DisplayOptions.ShowTargetWorkTimes) : ConfigManager.LocalSettingsForUser.DisplaySummaDelta;

			set
			{
				workTimeStatsMenu.WorkTimeStatsControl.SetSummaVisible(value);
				RaiseConfigChange();
			}
		}

		public bool ShowDelta
		{
			get => ConfigManager.DisplayOptions != null ? ConfigManager.DisplayOptions.Value.HasFlag(DisplayOptions.ShowDiffWorkTimes) : ConfigManager.LocalSettingsForUser.DisplaySummaDelta;

			set
			{
				workTimeStatsMenu.WorkTimeStatsControl.SetDeltaVisible(value);
				RaiseConfigChange();
			}
		}

		private void ClientSettingsManagerSettingsChanged(object sender, SingleValueEventArgs<ClientSetting> e)
		{
			context.Post(_ =>
			{
				workTimeStatsMenu.WorkTimeStatsControl.SetSummaVisible(ShowSum);
				workTimeStatsMenu.WorkTimeStatsControl.SetDeltaVisible(ShowDelta);
				RaiseConfigChange();
			}, null);
		}

		public int TopItemsCount
		{
			get
			{
				return ConfigManager.LocalSettingsForUser.MenuTopItemsCount;
			}

			set
			{
				ConfigManager.LocalSettingsForUser.MenuTopItemsCount = Math.Min(Math.Max(0, value), 50); //we have some performance issues here atm....
				menuBuilder.UpdateMenu(captureCoordinator.CurrentMenuLookup);
				RaiseConfigChange();
			}
		}

		public int FlattenFactor
		{
			get
			{
				return ConfigManager.LocalSettingsForUser.MenuFlattenFactor;
			}

			set
			{
				ConfigManager.LocalSettingsForUser.MenuFlattenFactor = value;
				menuBuilder.UpdateMenu(captureCoordinator.CurrentMenuLookup);
			}
		}

		public bool WorkingWarnDisplayable
		{
			get
			{
				return ConfigManager.LocalSettingsForUser.IsWorkingWarnDisplayable;
			}

			set
			{
				ConfigManager.LocalSettingsForUser.IsWorkingWarnDisplayable = value;
			}
		}

		public SynchronizationContext GuiContext { get { return context; } }

		public ApplicationStartType StartType { get; set; }

		public int? StartingWorkId { get; set; }

		public IdleDetector IdleDetector => captureCoordinator.IdleDetector;

		public MeetingNotifier MeetingNotifier => captureCoordinator.MeetingNotifier;

		public CurrentWorkController CurrentWorkController
		{
			get
			{
				Debug.Assert(currentWorkController != null);
				return currentWorkController;
			}
		}

		public MenuReportHelper MenuReportHelper
		{
			get { return menuReportHelper; }
		}

		public MenuTabHelper MenuTabHelper
		{
			get { return menuTabHelper; }
		}

		public IMessageService MessageService
		{
			get { return messageManager.MessageService; }
		}

		public void RaiseConfigChange()
		{
			var evt = ConfigChanged;
			if (evt != null) evt(this, EventArgs.Empty);
		}

		public bool IsDesktopLocked { get; private set; }

		public ActivityRecorderForm(ApplicationStartType startType)
		{
			StartType = startType;
			context = AsyncOperationManager.SynchronizationContext;
			((PlatformWin.PlatformFactory)Platform.Factory).MainForm = this; //hax for external dependency for RuleManagementWinService
			Debug.Assert(context is WindowsFormsSynchronizationContext);
			Icon = Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe

			hotkeyRegistrar = new HotkeyRegistrar(hotkeyService);

			captureCoordinator = new CaptureCoordinator(context, notificationService, CurrentWorkControllerPropertyChanged, clientSettingsManager, StartType); //if Assert is raised (e.g. in PeriodicManager's ctor) taskbarTimer_Tick would fire so to avoid nullRef we create it before InitializeComponent()
			captureCoordinator.DesktopCaptureFrozen += HandleDesktopCaptureFrozen;
			captureCoordinator.DesktopCaptured += HandleDesktopCaptured;
			captureCoordinator.OnUserActivity += HandleUserActivity;
			currentWorkController = captureCoordinator.CurrentWorkController;
			networkStabilityManager = new NetworkStabilityWinManager(currentWorkController);
			freezeHandlingService = new FreezeHandlingWinService(context, this);
			var workTimeHistory = new WcfWorkTimeQuery();
			workManagementService = captureCoordinator.WorkManagementService;
			workTimeStatsFromWebsiteManager = new WorkTimeStatsFromWebsiteManager(workTimeCounter, captureCoordinator.WorkItemManager, workTimeHistory);
			workTimeService = Platform.Factory.GetWorkTimeService(workTimeHistory);
			projectSyncService = Platform.Factory.GetProjectSyncService();

			ToolStripManager.Renderer = toolStripRenderer; //set cusom renderer for displaying close buttons 
			InitializeComponent();
			contextMenuForm = new ContextMenu(this);

			SetLocalizedMenu();
			UpdateWorkStatVisibilities();
			searchWorks.Available = ConfigManager.LocalSettingsForUser.DisplaySearchWorks;
			offlineWork.Click += offlineWork_Click;
			currentWorkItem.MouseDown += currentWorkItem_MouseDown;
			menuBuilder = new MenuBuilder(cmMenu);
			UpdateWorkTimeAndTaskbarInfo(null);
			captureCoordinator.SystemEventsService.SessionSwitch += SystemEventsService_SessionSwitch;

			messageManager = new MessageManager(context, notificationService);
			todoManager = new TodoListsManager(context, currentWorkController);
			acquireLogsManager = new AcquireLogsManager();
#if OcrPlugin
			ContributionController.Instance.AddCurrentWorkController(currentWorkController);
			miOpenContributionForm.Click += HandleOpenContributionFormClicked;
#else
			miErrorResolution.DropDownItems.Remove(miOpenContributionForm);
#endif

		}

		private void HandleDesktopCaptureFrozen(object sender, ThreadFrozenEventArgs e)
		{
			DebugEx.EnsureBgThread();
			log.Fatal("Thread " + e.FrozenThread.Name + " frozen");
			context.Post(_ => freezeHandlingService.HandleFreeze(e.FrozenThread, e.StackTrace != null ? e.StackTrace.ToString() : ""), null);
		}

		private void HandleDesktopCaptured(object sender, SingleValueEventArgs<DesktopCapture> e)
		{
			DebugEx.EnsureGuiThread();
			if (debugCaptureForm != null && !debugCaptureForm.IsDisposed)
			{
				debugCaptureForm.SetCapture(e.Value);
			}
		}

		private void UpdateWorkStatVisibilities()
		{
			workTimeStatsMenu.WorkTimeStatsControl.SetWeeksVisible(ConfigManager.LocalSettingsForUser.DisplayThisWeeksStats);
			workTimeStatsMenu.WorkTimeStatsControl.SetSummaVisible(ShowSum);
			workTimeStatsMenu.WorkTimeStatsControl.SetDeltaVisible(ShowDelta);
		}

		private void SetLocalizedMenu()
		{
			Localize();
			miRunAsAdmin.Available = !ConfigManager.IsAppLevelStorageNeeded && ElevatedPrivilegesHelper.IsLocalAdmin && (!ConfigManager.IsRunAsAdminDefault.HasValue || ConfigManager.IsRunAsAdminDefault.Value);
			if (miRunAsAdmin.Available)
				miRunAsAdmin.Checked = ElevatedPrivilegesHelper.RunAsAdmin;
			miIdleAlertVisual.Checked = ConfigManager.LocalSettingsForUser.IdleAlertVisual;
			miIdleAlertBeep.Checked = ConfigManager.LocalSettingsForUser.IdleAlertBeep;
		}

		public void Localize()
		{
			miSettings.Text = Labels.Menu_Settings;
			miLogout.Text = Labels.Menu_ChangeUser;
			miWorkDetectorRules.Text = Labels.Menu_WorkDetectorRules + @"...";
			miExit.Text = Labels.Menu_Exit;
			miErrorResolution.Text = Labels.Menu_Troubleshooting;
			miLogLevelChange.Checked = false;
			miLogLevelChange.Text = Labels.Menu_LogLevelChangeDebug;
			miOpenLog.Text = Labels.Menu_OpenLog + @"...";
			miOpenErrorReporting.Text = Labels.Menu_ErrorReporting + @"...";
			miDiagnosticTool.Text = Labels.Menu_Diagnostics + @"...";
			miDiagDebugMode.Text = Labels.Menu_DiagDebugMode;
			miDiagDebugDisableDomCapture.Text = Labels.Menu_DiagDebugDisableDomCapture;
			miDiagDebugDisableJcMail.Text = Labels.Menu_DiagDebugDisableJcMail;
			miDiagDebugDisableOlAddin.Text = Labels.Menu_DiagDebugDisableOlAddin;
			miDiagDebugDisableOutlookMeetingSync.Text = Labels.Menu_DiagDebugDisableOutlookMeetingSync;
			miDiagDebugDisableLotusMeetingSync.Text = Labels.Menu_DiagDebugDisableLotusMeetingSync;
			miDiagDebugDisableAutomationPlugin.Text = Labels.Menu_DiagDebugDisableAutomationPlugin;
			miDiagDebugDisableAllPlugin.Text = Labels.Menu_DiagDebugDisableAllPlugin;
			miDiagDebugDisableUrlCapture.Text = Labels.Menu_DiagDebugDisableUrlCapture;
			miDiagDebugDisableTitleCapture.Text = Labels.Menu_DiagDebugDisableTitleCapture;
			miDiagDebugDisableProcessCapture.Text = Labels.Menu_DiagDebugDisableProcessCapture;
			miPreferences.Text = Labels.Menu_Preferences + @"...";
			adminCenterToolStripMenuItem.Text = Labels.Menu_MainPage;
			miRunAsAdmin.Text = Labels.Menu_RunAsAdmin;
			menuReportHelper.ResetLastQueryTime();
			menuTabHelper.ResetLastQueryTime();
		}

		private void CurrentWorkControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentWork" || e.PropertyName == "IsOnline" || e.PropertyName == "IsRuleOverrideEnabled")
			{
				//todo different icon for NotWorkingTemp?
				SetIcon();
			}

			if (e.PropertyName != "CurrentWork") return;
			if (currentWorkController.CurrentWork != null &&
				currentWorkController.LastWorkStateChangeReason == WorkStateChangeReason.UserSelect)
			{
				IntelligentSuggestionQuery.Instance.Learn(currentWorkController.CurrentWork);
			}

			var cwc = (CurrentWorkController)sender;
			//display current workitem in menu (dis/enable)
			currentWorkItem.Text = MenuBuilder.EscapeText(cwc.CurrentOrLastWorkNameInTwoLines);
			UpdateCurrentItemTooltipText();
			contextMenuForm.SetCurrentWork(cwc.CurrentWork ?? cwc.LastUserSelectedOrPermWork, cwc.CurrentOrLastWorkName, cwc.CurrentWork != null);

			//hax to resize cmMenu
			cmMenu.Items.Insert(0, new ToolStripSeparator());
			cmMenu.Items.RemoveAtWithDispose(0);
			var isWorking = cwc.CurrentWork != null;
			currentWorkItem.Enabled = isWorking;
			//display current workitem in tooltip and recalculate worktimes
			UpdateWorkTimeAndTaskbarInfo(isWorking ? cwc.CurrentOrLastWorkNameInTwoLines : null);
			//display icon
			//misc.
			if (isWorking)
			{
				notificationService.HideNotification(NotificationKeys.IdleStopWork);
				if (cwc.IsCurrentWorkValid && cwc.LastWorkStateChangeReason == WorkStateChangeReason.UserSelect) recentWorks.AddRecentWork(cwc.CurrentWork);
			}
			else
			{
				IdleDetector.ResetIdleWorkTime();
			}
		}

		private void UpdateCurrentItemTooltipText()
		{
			var cwc = currentWorkController;
			if (cwc.CurrentWork != null && cwc.CurrentWork.Id.HasValue)
			{
				var workTime = workManagementService.GetTotalWorkTimeForWork(cwc.CurrentWork.Id.Value);
				currentWorkItem.ToolTipText = ToolStripMenuItemForWorkData.GetWorkDataDesc(cwc.CurrentWork, workTime);
			}
			else
			{
				currentWorkItem.ToolTipText = "";
			}
		}

		void currentWorkItem_ButtonClick(object sender, EventArgs e)
		{
			if (currentWorkController.CurrentWork != null && currentWorkController.CurrentWork.Id.HasValue)
				workManagementService.DisplayReasonWorkGui(currentWorkController.CurrentWork);
		}

		public void SetClipboardData(int workId)
		{
			var work = currentWorkController.GetWorkDataWithParentNames(workId);
			if (work == null)
			{
				ClipboardHelper.SetClipboardData(workId, null);
			}
			else
			{
				ClipboardHelper.SetClipboardData(work);
			}
		}

		private void currentWorkItem_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right) return;
			if (currentWorkController.CurrentWork == null || !currentWorkController.CurrentWork.Id.HasValue) return;
			SetClipboardData(currentWorkController.CurrentWork.Id.Value);
			cmMenu.Close();
		}

		private static readonly string taskbarHeader = (ConfigManager.AppNameOverride ?? ConfigManager.ApplicationName) + " v" + ConfigManager.VersionWithClassifier;
		private void UpdateWorkTimeAndTaskbarInfo(string currentWork)
		{
			userNameItem.Text = ConfigManager.IsAnonymModeEnabled ? ConfigManager.ApplicationName : (MenuBuilder.EscapeText(ConfigManager.UserName) + " (" + ConfigManager.UserId + ")");
			lastUrl.Visible = !ConfigManager.IsAnonymModeEnabled;
			var stats = workTimeStatsManager.ClientWorkTimeStats;
			var ctrl = workTimeStatsMenu.WorkTimeStatsControl;
			var workTime = ctrl.SetWorkTimeStats(workTimeCounter.TodaysWorkTime, captureCoordinator.WorkItemManager.UnsentItemsCount, stats);

			
			ShowOfflineWorkOption();
			var isWorkingAwayFromDesktop = !currentWorkController.IsWorking && currentWorkController.MutualWorkTypeCoordinator.IsWorking;
			var createdNew = !isWorkingAwayFromDesktop //only show (not) working popups when there is no 'working form' visible
				&& currentWorkController.ShowPeriodicNotificationIfApplicable();
			if (createdNew && cmMenu.Visible)
			{
				//if new notification is shown and menu is visible then bring the menu to the front
				cmMenu.BringToFrontAll(); //this won't hide the dropdown if it's over the contextmenu
				searchWorks.BringToFrontAll(); //search should be at the top
			}
			if (swUpdateTargetEndDate.IsIntervalElapsedSinceLastCheck())
			{
				menuBuilder.UpdateTargetEndDatePercentages();
			}
			RestartIfNewVersionInstalledAndOfflineAndNoFormsOpened();
		}

		private void ShowOfflineWorkOption()
		{
			offlineWork.Available = ConfigManager.MaxOfflineWorkItems > 0;
			if (!offlineWork.Available) return;
			if (!captureCoordinator.WorkItemManager.CanPause)
			{
				offlineWork.Checked = false;
				offlineWork.Enabled = false;
			}
			else if (captureCoordinator.WorkItemManager.Paused)
			{
				offlineWork.Checked = true;
				offlineWork.Enabled = true;
			}
			else
			{
				offlineWork.Checked = false;
				offlineWork.Enabled = true;
			}
		}

		private void offlineWork_Click(object sender, EventArgs e)
		{
			log.Debug("UI - Offline work clicked from old menu");
			captureCoordinator.WorkItemManager.Paused = !offlineWork.Checked;
			ShowOfflineWorkOption();
		}

		private void ActivityRecorderForm_Load(object sender, EventArgs e)
		{
			log.Info("Form loading");
			this.Visible = false;
			niTaskbar.Visible = ConfigManager.IsTaskBarIconShowing;
			miDiagDebugMode.DropDown.AutoClose = false;
			miDiagDebugMode.DropDown.MouseLeave += (s, _) => miDiagDebugMode.DropDown.Close();
			Platform.Factory.GetAutoStartHelper().Register(Platform.Factory.GetUpdateService());
			if (ActiveDirectoryAuthenticationManager.CanStart())
			{
				log.Debug("ActiveDirectoryAuthenticationManager Starting");
				activeDirectoryAuthManager = new ActiveDirectoryAuthenticationManager();
				activeDirectoryAuthManager.UserIdChanged += ActiveDirectoryAuthManagerOnUserIdChanged;
				activeDirectoryAuthManager.Start();
				log.Debug("ActiveDirectoryAuthenticationManager Started");
			}
			log.Debug("NetworkStabilityManager Starting");
			networkStabilityManager.Start();
			log.Debug("NetworkStabilityManager Started");
			workTimeCounter.Load();
			log.Debug("workTimeCounter Loaded");
			UpdateWorkTimeAndTaskbarInfo(null);
			log.Debug("UpdateWorkTimeAndTaskbarInfo");

			if (ConfigManager.AutoUpdateManagerEnabled)
			{
				autoUpdateManager.NewVersionInstalledEvent += AutoUpdateManagerNewVersionInstalledEvent;
				log.Debug("AutoUpdateManager Starting");
				autoUpdateManager.Start();
				log.Debug("AutoUpdateManager Started");
			}
			else log.Debug("AutoUpdateManager disabled");
			dppData = DppHelper.GetAcceptanceData();
			if (dppData != null && !dppData.AcceptedAt.HasValue)
			{
				DppNeedAccept = true;
				DisplayWelcome(true, dppData, DppAccepted);
			}
			else
			{
				ContinueInit();
			}
		}

		private void DppAccepted()
		{
			DppNeedAccept = false;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				context.Post(__ =>
				{
					ContinueInit();
					DppHelper.SetAcceptanceDate(captureCoordinator);
				}, null);
			});
		}

		private void ContinueInit()
		{ 
			clientSettingsManager.SettingsChanged += ClientSettingsManagerSettingsChanged;
			captureCoordinator.CurrentMenuChanged += MenuManagerCurrentMenuChanged;
			captureCoordinator.WorkItemManager.ConnectionStatusChanged += WorkItemManagerConnectionStatusChanged;
			captureCoordinator.WorkItemManager.CannotPersistAndSendWorkItem += WorkItemManagerCannotPersistAndSendWorkItem;
			captureCoordinator.WorkItemCreated += CaptureCoordinatorWorkItemCreated;
			captureCoordinator.Start();
			captureCoordinator.MutualWorktypeLoader();
			log.Debug("captureCoordinator Started");
			if (IsolatedStorageSerializationHelper.Exists(IdleQuitPath))
			{
				bool idleQuit;
				if (IsolatedStorageSerializationHelper.Load(IdleQuitPath, out idleQuit) && idleQuit)
				{
					log.Info("Found idle info");
					ShowIdleMessage();
				}
				IsolatedStorageSerializationHelper.Delete(IdleQuitPath);
			}
			log.Debug("Creating Menu");
			miLogout.Text += " (" + ConfigManager.UserId + ")";
			cmMenu.Items.Insert(0, userNameItem);
			cmMenu.Items.Insert(1, new ToolStripSeparator());
			cmMenu.Items.Insert(2, workTimeStatsMenu);
			cmMenu.Items.Insert(3, new ToolStripSeparator());
			cmMenu.Items.Insert(4, currentWorkItem);
			cmMenu.Items.Insert(5, new ToolStripSeparator());
			cmMenu.Items.Insert(6, menuBuilder.PlaceHolder);
			cmMenu.Items.Insert(7, new ToolStripSeparator());
			cmMenu.Items.Insert(8, recentWorks);
			cmMenu.Items.Insert(9, searchWorks);

			cmMenu.Items.Insert(cmMenu.Items.Count - 2, lastUrl); //place it under the separator
			foreach (var item in lastUrl.UrlItems)
			{
				adminCenterToolStripMenuItem.DropDownItems.Add(item); //populate url links
			}
			cmMenu.Items.Insert(cmMenu.Items.Count - 3, offlineWork); //place it over lastUrl (and under the separator)
#if DEBUG
			cmMenu.Items.Insert(9, GetDebugMenuItems());
#endif
			miExit.Enabled = true;// TODO: mac !AppControlServiceHelper.IsRegistered;
			workManagementService.OnTaskReasonsChanged += TaskReasonsChanged;
			menuBuilder.UpdateMenu(captureCoordinator.CurrentMenuLookup);
			menuBuilder.MenuClick += MenuBuilderMenuClick;
			menuBuilder.MenuButtonClick += MenuBuilderMenuButtonClick;
			contextMenuForm.WorkClick += MenuBuilderMenuClick;
			currentWorkItem.ButtonClick += currentWorkItem_ButtonClick;
			log.Debug("menuBuilder Menu Updated");
			recentWorks.LoadRecentWorks(captureCoordinator.CurrentMenuLookup);
			recentWorks.MenuClick += MenuBuilderMenuClick;
			recentWorks.MenuButtonClick += MenuBuilderMenuButtonClick;
			log.Debug("recentWorks Loaded");
			searchWorks.UpdateMenu(captureCoordinator.CurrentMenuLookup);
			searchWorks.MenuClick += MenuBuilderMenuClick;
			log.Debug("searchWorks Loaded");
			this.niTaskbar.Icon = iconNotWorking;

			messageManager.Start();
			todoManager.Start();
			acquireLogsManager.Start();
			//GoogleCredentialManager.OnReportResult += GoogleCredentialManagerOnReportResult;
			//GeneralLongIntervalQueryManager.Instance.Start();

			hotkeyRegistrar.HotkeyPressed += HandleHotkey;
			Application.AddMessageFilter(hotkeyService as IMessageFilter);
			hotkeyRegistrar.LoadSettings();
			if (ConfigManager.AutoReturnFromMeeting)
				UserActivityWinService.Instance.SetHotkeys(hotkeyRegistrar.GetHotkeys());
			if (captureCoordinator.WorkDetectorHotKey.HasValue)
			{
				var hks = MigrateLegacyHotkey((Keys)captureCoordinator.WorkDetectorHotKey.Value, HotkeyActionType.NewWorkDetectorRule);
				captureCoordinator.WorkDetectorHotKey = null;
				log.InfoFormat("Legacy WorkDetectorHotKey has been migrated. ({0})", hks);
			}
			if (ConfigManager.LocalSettingsForUser.HotKey.HasValue)
			{
				var hks = MigrateLegacyHotkey((Keys)ConfigManager.LocalSettingsForUser.HotKey.Value, HotkeyActionType.ResumeOrStopWork);
				ConfigManager.LocalSettingsForUser.HotKey = null;
				log.InfoFormat("Legacy (ResumeOrStopWork) HotKey has been migrated. ({0})", hks);
			}
			if (ConfigManager.LocalSettingsForUser.ManualMeetingHotKey.HasValue && (Keys)ConfigManager.LocalSettingsForUser.ManualMeetingHotKey.Value != Keys.None)
			{
				var hks = MigrateLegacyHotkey((Keys)ConfigManager.LocalSettingsForUser.ManualMeetingHotKey.Value, HotkeyActionType.StartManualMeeting);
				ConfigManager.LocalSettingsForUser.ManualMeetingHotKey = Forms.Keys.None;
				log.InfoFormat("Legacy ManualMeetingHotKey has been migrated. ({0})", hks);
			}
			if (ConfigManager.LocalSettingsForUser.ClearAutoRuleTimersHotKey.HasValue && (Keys)ConfigManager.LocalSettingsForUser.ClearAutoRuleTimersHotKey.Value != Keys.None)
			{
				var hks = MigrateLegacyHotkey((Keys)ConfigManager.LocalSettingsForUser.ClearAutoRuleTimersHotKey.Value, HotkeyActionType.ClearAutoRuleTimer);
				ConfigManager.LocalSettingsForUser.ClearAutoRuleTimersHotKey = Forms.Keys.None;
				log.InfoFormat("Legacy ClearAutoRuleTimersHotKey has been migrated. ({0})", hks);
			}

#if LegacySimple
			workTimeStatsManager.SimpleWorkTimeStatsReceived += SimpleWorkTimeStatsReceived;
#else
			captureCoordinator.StatsCoordinator.SimpleWorkTimeStatsCalculated += SimpleWorkTimeStatsReceived;
#endif
			workTimeStatsManager.PasswordError += PasswordError;
			workTimeStatsManager.ActiveOnlyError += ActiveOnlyError;
			workTimeStatsManager.PasswordExpiredError += PasswordExpiredError;
			workTimeStatsManager.LoadData();

			workTimeStatsManager.Start();
			log.Debug("workTimeStatsManager Started");
			allWorksManager.AllWorksChanged += AllWorksChanged;
			if (!ConfigManager.LocalSettingsForUser.SearchOwnTasks || ConfigManager.LocalSettingsForUser.SearchInClosed)
			{
				lastSearchAllWorks = true;
				allWorksManager.Start();
				log.Debug("allWorksManager Started");
			}
			UpdateWorkTimeAndTaskbarInfo(null);

			CheckIfUpdateFailed();

			// sometimes caused AccessViolationExpection and APPCRASH. More investigation needed.
			// TrayStateChanger.SetTrayItemPref(Assembly.GetExecutingAssembly().Location, true);

			log.Info("Form loaded");
#if WELCOME_ENABLED
			log.Debug("DisplayWelcome enabled");
			DisplayWelcome(false, dppData);
#endif
			//DisplayNewFeatures();
			BlinkIconAfterBoot();
			if (StartType == ApplicationStartType.EmergencyRestart)
			{
				if (StartingWorkId != null)
				{
					log.InfoFormat("Trying to start work {0}", StartingWorkId);
					currentWorkController.StartOrQueueWork(new WorkData { Id = StartingWorkId.Value });
				}

				notificationService.ShowNotification(NotificationKeys.EmergencyRestart, TimeSpan.FromSeconds(20),
					Labels.NotificationDcFreezeTitle, Labels.NotificationDcFreezeBody);
			}

			int? lastActiveWorkId = null;
			log.Debug("Checking for last active work when update started");
			if (IsolatedStorageSerializationHelper.Exists(ActiveWorkIdPath) && IsolatedStorageSerializationHelper.Load(ActivityRecorderForm.ActiveWorkIdPath, out lastActiveWorkId))
			{
				IsolatedStorageSerializationHelper.Delete(ActivityRecorderForm.ActiveWorkIdPath);
				if (lastActiveWorkId.HasValue)
				{
					log.Debug("Work id {0} found, trying to start work".FS(lastActiveWorkId.Value));
					currentWorkController.StartOrQueueWork(new WorkData() { Id = lastActiveWorkId.Value });
				}
			}

			if (StartType == ApplicationStartType.StartWorkAfterLogin && captureCoordinator.CurrentMenuLookup != null && captureCoordinator.CurrentMenuLookup.ClientMenu != null)
			{
				log.Info("StartWorkAfterStart is set and ClientMenu exists");
				// starting with last work or default
				CurrentWorkController.UserResumeWork();
				StartType = ApplicationStartType.Normal;
			}

			guiThreadWatchdogManager = new WatchdogManager(10000, 30000);
			var watchedThread = Thread.CurrentThread;
			guiThreadWatchdogManager.MissingReset += (_, __) =>
			{
				try
				{
					var shouldSuspend = (watchedThread.ThreadState & (System.Threading.ThreadState.Suspended | System.Threading.ThreadState.SuspendRequested)) == 0;
					if (shouldSuspend) watchedThread.Suspend(); //the process will go down, so obsolote method is not an issue here
					var stackTrace = new StackTrace(false); // TODO: mac
					log.Warn("Gui thread frozen, thread's stack is:" + Environment.NewLine + stackTrace.ToString());
					if (shouldSuspend) watchedThread.Resume();
				}
				catch (Exception ex)
				{
					log.Error("Can't obtain gui thread state after frozen", ex);
				}
				guiThreadWatchdogManager.Reset();

			};
			guiThreadWatchdogManager.Start(10000);

			init2completed = true;
			taskbarTimer.Start();
		}

		private void GoogleCredentialManagerOnReportResult(object sender, SingleValueEventArgs<bool> e)
		{
			context.Post(_ =>
			{
				if (e.Value)
					notificationService.ShowNotification(NotificationKeys.GoogleCredential, TimeSpan.FromSeconds(10), Labels.GoogleCredetialManager_ReportTitle, Labels.GoogleCredetialManager_ReportSuccessText);
				else
					notificationService.ShowMessageBox(Labels.GoogleCredetialManager_ReportFailedText, Labels.GoogleCredetialManager_ReportTitle, Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
			}, null);
		}

		private DateTime? lastUpdateFailed;
		private void CheckIfUpdateFailed()
		{
			if (autoUpdateManager.UpdateService.LastUpdateFailed != lastUpdateFailed)
			{
				lastUpdateFailed = autoUpdateManager.UpdateService.LastUpdateFailed;
				notificationService.ShowNotification(NotificationKeys.UpdateFailed, TimeSpan.Zero, autoUpdateManager.UpdateService.LastUpdateFailureReason == UpdateFailureReason.RestartRequired ? Labels.NotificationWindowsRestartRequiredTitle : Labels.NotificationUpdateFailedTitle, autoUpdateManager.UpdateService.LastUpdateFailureReason == UpdateFailureReason.RestartRequired ? Labels.NotificationWindowsRestartRequiredBody : Labels.NotificationUpdateFailedBody);
			}
		}

		private DateTime lastTimeZoneCheck;
		private void CheckTimeZone()
		{
			if (ConfigManager.TimeZoneWeb == null || lastTimeZoneCheck == DateTime.Today) return;
			lastTimeZoneCheck = DateTime.Today;
			if (TimeZoneInfo.Local.Equals(ConfigManager.TimeZoneWeb)) return;
			notificationService.ShowNotification(NotificationKeys.TimeZoneNotSet, TimeSpan.Zero, Labels.NotificationTimeZoneTitle, Labels.NotificationTimeZoneBody);
		}

		private void ActiveDirectoryAuthManagerOnUserIdChanged(object sender, EventArgs eventArgs)
		{
			context.Post(_ =>
			{
				notificationService.ShowMessageBox(Labels.UserIdMappingChangedBody, Labels.UserIdMappingChangedTitle, Forms.MessageBoxButtons.OK);
				log.Info("Restarting application because windows user mapped to different JC user.");
				logoutOnStop = true;
				Restart();
			}, null);
		}

		void TaskReasonsChanged(object sender, SingleValueEventArgs<TaskReasons> e)
		{
			context.Post(_ =>
				{
					menuBuilder.UpdateReasonStats(e.Value);
				}, null);
		}

		void AllWorksChanged(object sender, SingleValueEventArgs<List<AllWorkItem>> e)
		{
			context.Post(_ =>
							{
								if (stopping) return;
								allWorkItems = e.Value;
								if (!ConfigManager.LocalSettingsForUser.SearchOwnTasks || ConfigManager.LocalSettingsForUser.SearchInClosed)
									RefreshSearchContent();
							}, null);
		}

		private void AutoUpdateManagerNewVersionInstalledEvent(object sender, SingleValueEventArgs<bool> e)
		{
			context.Post(_ =>
							{
								if (!e.Value)
								{
									log.Debug("Update found during application startup");
									if (currentWorkController.IsWorking)
									{
										var hasLastUserSelectedWork = currentWorkController != null && currentWorkController.LastUserSelectedOrPermWork != null && currentWorkController.LastUserSelectedOrPermWork.Id != null;
										int? workId = hasLastUserSelectedWork ? currentWorkController.LastUserSelectedOrPermWork.Id : null;
										IsolatedStorageSerializationHelper.Save(ActiveWorkIdPath, workId);
									}
									else if (IsolatedStorageSerializationHelper.Exists(ActiveWorkIdPath))
										IsolatedStorageSerializationHelper.Delete(ActiveWorkIdPath);
									log.Debug("Restarting");
									//AppControlServiceHelper.UnregisterProcess();
									if (autoUpdateManager.UpdateService.RestartWithNewVersion())
									{
										// forcing restart even if dpp not accepted yet
										DppNeedAccept = false;
										Exit(true);
									}
									return;
								}
								restartForNewVersion = true;
								log.Debug("New version available, showing notification");

								notificationService.ShowNotification("UPD_NOT", TimeSpan.Zero, Labels.AutoUpdate_UpdateAvailable,
										Labels.AutoUpdate_Prompt, null,
										() =>
										{

											log.Debug("Notification clicked");
											var result = notificationService.ShowMessageBox(Labels.ConfirmExitStillWorkingBody,
											Labels.ConfirmExitStillWorkingTitle, Forms.MessageBoxButtons.OKCancel);
											if (result != Forms.DialogResult.OK) return;
											forceRestartAfterPrompt = true;
											if (currentWorkController.IsWorking)
											{
												var hasLastUserSelectedWork = currentWorkController != null
																			  && currentWorkController.LastUserSelectedOrPermWork != null
																			  && currentWorkController.LastUserSelectedOrPermWork.Id != null;
												var workId = hasLastUserSelectedWork ? currentWorkController.LastUserSelectedOrPermWork.Id : null;
												IsolatedStorageSerializationHelper.Save(ActiveWorkIdPath, workId);
											}
											else
											{
												if (IsolatedStorageSerializationHelper.Exists(ActiveWorkIdPath))
													IsolatedStorageSerializationHelper.Delete(ActiveWorkIdPath);
											}
											log.Debug("Restarting");
											RestartIfNewVersionInstalled(false);

										});
							}, null);
		}

		private void RestartIfNewVersionInstalledAndOfflineAndNoFormsOpened()
		{
			if (!restartForNewVersion
				|| stopping
				|| OwnedForms.Any(f => !(f is OfflineWorkForm))
				|| currentWorkController.MutualWorkTypeCoordinator.IsWorking
				|| cmMenu.Visible)
			{
				return;
			}
			RestartIfNewVersionInstalled(notificationService.IsActive(NotificationKeys.IdleStopWork));
		}

#if DEBUG
		private static ToolStripItem GetDebugMenuItems()
		{
			var result = new ToolStripMenuItem("Debug menu");

			var cL = AddChildDebugMenuItems(result, "Debug Lvl1 ", 100)
				.SelectMany((c1, idx) => AddChildDebugMenuItems(c1, "Debug Lvl2 ", idx < 3 ? 100 : 1), (c1, c2) => new { c1, c2 })
				.SelectMany((n, idx) => AddChildDebugMenuItems(n.c2, "Debug Lvl3 ", idx < 3 ? 100 : 1), (n, c3) => new { n, c3 })
				.SelectMany((n, idx) => AddChildDebugMenuItems(n.c3, "Debug Lvl4 ", idx < 3 ? 100 : 1));

			//var cL = from c1 in AddChildDebugMenuItems(result, "Debug Lvl1 ", 100) //too many items... dispose froze...
			//         from c2 in AddChildDebugMenuItems(c1, "Debug Lvl2 ", 50)
			//         from c3 in AddChildDebugMenuItems(c2, "Debug Lvl3 ", 10)
			//         from c4 in AddChildDebugMenuItems(c3, "Debug Lvl4 ", 3)
			//         select c4;
			cL.ToList(); //execute
			return result;
		}

		private static IEnumerable<ToolStripMenuItem> AddChildDebugMenuItems(ToolStripMenuItem parent, string prefix, int count)
		{
			var children = Enumerable.Range(0, count).Select(n => new ToolStripMenuItem(prefix + n));
			foreach (var child in children)
			{
				parent.DropDownItems.Add(child);
				yield return child;
			}
		}
#endif

		private System.Windows.Forms.Timer blinkAfterBootTimer;
		private void BlinkIconAfterBoot() //try this hax to pervent hidden icon on WinXP, Vista, Win7
		{
			int tickCount = Environment.TickCount;
			if (tickCount > 0 && tickCount < (int)TimeSpan.FromMinutes(10).TotalMilliseconds)
			{
				blinkAfterBootTimer = new System.Windows.Forms.Timer { Interval = 30000 };
				blinkAfterBootTimer.Tick += blinkAfterBootTimer_Tick;
				blinkAfterBootTimer.Enabled = true;
			}
		}

		private void blinkAfterBootTimer_Tick(object sender, EventArgs e)
		{
			blinkAfterBootTimer.Enabled = false;
			blinkAfterBootTimer.Dispose();
			blinkAfterBootTimer = null;
			BlinkIcon();
		}

		internal const int featuresData = 3; //increment this if we want to display new feature info
		private void DisplayNewFeatures()
		{
			int lastFeatures;
			if (IsolatedStorageSerializationHelper.Exists(NewFeaturesPath)
				&& IsolatedStorageSerializationHelper.Load(NewFeaturesPath, out lastFeatures)
				&& lastFeatures == featuresData) return;

			if (string.IsNullOrEmpty(Labels.NewFeaturesTitle) || string.IsNullOrEmpty(Labels.NewFeaturesBody)) return;
			log.Debug("Showing welcome form");
			//Welcome form
			new WelcomeForm().Show();

			//Notification
			//notificationService.ShowNotification(NotificationKeys.DisplayNewFeatures, TimeSpan.Zero, Labels.NewFeaturesTitle, Labels.NewFeaturesBody);

			//MessageBox
			//var result = MessageBox.Show(Labels.NewFeaturesBody, Labels.NewFeaturesTitle, MessageBoxButtons.OKCancel);
			//if (result != DialogResult.OK) return; //repeat next time

			IsolatedStorageSerializationHelper.Save(NewFeaturesPath, featuresData);
		}

		private void DisplayWelcome(bool force, AcceptanceData dppData, Action recordAgree = null)
		{
			if (welcomeForm != null)
			{
				if (welcomeForm.WindowState == FormWindowState.Minimized) welcomeForm.WindowState = FormWindowState.Normal;
				welcomeForm.BringToFront();
				welcomeForm.Activate();
				return;
			}
			bool welcomeSkipped = false;
			if (IsolatedStorageSerializationHelper.Exists(WelcomeStatePath))
			{
				IsolatedStorageSerializationHelper.Load(WelcomeStatePath, out welcomeSkipped);
			}
			else
			{
				if (!autoUpdateManager.UpdateService.IsFirstRun)
				{
					IsolatedStorageSerializationHelper.Save(WelcomeStatePath, true);
					welcomeSkipped = true;
				}
			}
			if (welcomeSkipped && !force) return;
			welcomeForm = new FirstTimeWelcomeForm(welcomeSkipped, dppData, recordAgree);
			welcomeForm.AfterClose += WelcomeFormOnAfterClose;
			welcomeForm.FormClosing += WelcomeFormOnFormClosing;
			welcomeForm.Show(this);
			TelemetryHelper.RecordFeature("Welcome", "Show");
		}

		private void WelcomeFormOnFormClosing(object sender, FormClosingEventArgs formClosingEventArgs)
		{
			if (!DppNeedAccept) return;
			if (MessageBox.Show(Labels.ConfirmExitBeforeDppAccept, Labels.Menu_Exit, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				log.Debug("Dpp not accepted, exiting...");
				DppNeedAccept = false;
				stopping = true;
				Exit(true);
			}
			else formClosingEventArgs.Cancel = true;
		}

		private void WelcomeFormOnAfterClose(object sender, SingleValueEventArgs<bool> e)
		{
			TelemetryHelper.RecordFeature("Welcome", "Close");
			welcomeForm.AfterClose -= WelcomeFormOnAfterClose;
			welcomeForm.FormClosing -= WelcomeFormOnFormClosing;
			IsolatedStorageSerializationHelper.Save(WelcomeStatePath, e.Value);
			welcomeForm = null;
		}

		private StopwatchLite swPassDialog = new StopwatchLite(1, true);
		private void PasswordError(object sender, EventArgs e)
		{
			if (activeDirectoryAuthManager == null)
			{
				context.Post(_ =>
							{
								if (stopping) return;
								if (swPassDialog == null || !swPassDialog.IsIntervalElapsedSinceLastCheck()) return;
								swPassDialog = null; //don't show pass dialog again
								try
								{
									notificationService.ShowMessageBox(Labels.Login_NotificationPasswordErrorBody, Labels.Login_NotificationPasswordErrorTitle, Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
									ConfigManager.ShowPasswordDialog(LoginForm.ShowChangePasswordDialog);
								}
								finally
								{
									//hax don't show pass dialog for another 1 min
									//(so we won't notice errors from the pervious bad password)
									swPassDialog = new StopwatchLite(TimeSpan.FromMinutes(1), false);
								}
							}, null);
			}
			else
			{
				ConfigManager.ShowPasswordDialog(RefreshClientTicketOrLogin);
			}
		}

		private void PasswordExpiredError(object sender, EventArgs e)
		{
			if (activeDirectoryAuthManager == null)
			{
				context.Post(_ =>
				{
					if (stopping) return;
					if (swPassDialog == null || !swPassDialog.IsIntervalElapsedSinceLastCheck()) return;
					swPassDialog = null; //don't show pass dialog again
					try
					{
						
						notificationService.ShowPasswordExpiredMessageBox();
						ConfigManager.ShowPasswordDialog(LoginForm.ShowChangePasswordDialog);
					}
					finally
					{
						//hax don't show pass dialog for another 1 min
						//(so we won't notice errors from the pervious bad password)
						swPassDialog = new StopwatchLite(TimeSpan.FromMinutes(1), false);
					}
				}, null);
			}
			else
			{
				ConfigManager.ShowPasswordDialog(RefreshClientTicketOrLogin);
			}
		}

		private ConfigManager.LoginData RefreshClientTicketOrLogin(ConfigManager.LoginData current)
		{
			ConfigManager.LoginData loginData;
			try
			{
				loginData = activeDirectoryAuthManager.RefreshClientLoginTicket(true);
			}
			catch (CommunicationException)
			{
				using (var c = new ActivityRecorderClientWrapper())
				{
					try
					{
						c.Client.Authenticate("");
					}
					catch (CommunicationException ex)
					{
						if (!(ex.InnerException is FaultException && ex.InnerException.Message == "Invalid user or password"))
						{
							log.Debug("Both JC and AD interfaces are unavailable, login ticket refreshing failed");
							return null;
						}
					}
					catch (Exception ex)
					{
						log.Warn("Unexpected error during login ticket refreshing", ex);
						return null;
					}
				}
				log.Debug("AD interface becomes unavailable, try login with password...");
				return ActiveDirectoryAuthenticationManager.LoginWithWindowsUser(() => ConfigManager.SuppressActiveDirectoryFallbackLogin ? null : LoginForm.ShowChangePasswordDialog(current)); 
			}
			return loginData;
		}

		private void ActiveOnlyError(object sender, EventArgs e)
		{
			context.Post(_ =>
							{
								if (stopping) return;
								notificationService.ShowNotification(NotificationKeys.ActiveOnly, nfActiveOnlyDuration, Labels.NotificationActiveOnlyErrorTitle,
									Labels.NotificationActiveOnlyErrorBody);
							}, null);
		}

		private void SimpleWorkTimeStatsReceived(object sender, SingleValueEventArgs<SimpleWorkTimeStats> e)
		{
			context.Post(stats =>
							{
								if (stopping) return;
								var totalStats = (SimpleWorkTimeStats)stats;
								var sumTime = new TimeSpan(totalStats.Stats.Values.Sum(n => n.TotalWorkTime.Ticks));
								userNameItem.ToolTipText = string.Format(Labels.SumWorkHours, sumTime.TotalHours.ToString("0.#"));
								MenuQuery.Instance.SimpleWorkTimeStats.Update(totalStats);
								menuBuilder.UpdateTargetTotalWorkTimePercentages(totalStats);
								contextMenuForm.UpdateSimpleStats(totalStats);
								UpdateCurrentItemTooltipText();
							}, e.Value);
		}

		private void StopWorkFromGui()
		{
			if (currentWorkController.CurrentWorkState != WorkState.NotWorking)
			{
				currentWorkController.UserStopWork();
			}
		}

		private void MenuManagerCurrentMenuChanged(object sender, MenuEventArgs e)
		{
			menuBuilder.UpdateMenu(e.MenuLookup);
			recentWorks.UpdateMenu(e.MenuLookup);
			searchWorks.UpdateMenu(e.MenuLookup);

			if (!ConfigManager.LocalSettingsForUser.SearchOwnTasks || ConfigManager.LocalSettingsForUser.SearchInClosed)
				allWorksManager.SynchronizeDataAsync(); // we can't merge current menu change with all works => full refresh

			//there is a race here if the menu is updated and the current work is no longer valid but we are quitting the app
			//then the invalid work will remain presisted and on the next start it won't be invalidated till the menu changes again
			//so that is why we have to validate at startup too

			if (StartType == ApplicationStartType.StartWorkAfterLogin && (e.OldMenu == null || e.OldMenu.Works == null) &&
				CurrentWorkController.CurrentWorkState == WorkState.NotWorking)
			{
				CurrentWorkController.UserResumeWork();
			}
		}

		private Action<WorkDataEventArgs> menuItemClick;
		private Action<WorkDataEventArgs> menuButtonClick;
		public void SetAlternativeMenu(Action<WorkDataEventArgs> itemClick, Action<WorkDataEventArgs> buttonClick, string caption)
		{
			bool isAlternative = itemClick != null && buttonClick != null;
			toolStripRenderer.SetAlternativeColors(isAlternative);
			cmMenu.Invalidate(); //redraw with new colors
			for (int i = 0; i < cmMenu.Items.Count; i++)
			{
				if (cmMenu.Items[i].Tag == this)
				{
					cmMenu.Items.RemoveAtWithDispose(i--);
				}
			}
			if (isAlternative)
			{
				//insert warning header so user will know that she is choosing a work for a learning rule
				cmMenu.Items.Insert(0, new ToolStripLabel(caption) { Tag = this });
				cmMenu.Items.Insert(1, new ToolStripSeparator() { Tag = this });

				//simple Show would case some weird problems (workStatus disappearing) when item is selected after menu changed
				niTaskbar.ShowContextMenuStrip();
				//if there was no right-click popup previously then this will appear on the top left corner, so in that case we force bottom right corner (taskbar might not be there but I don't care)
				if (cmMenu.Bounds.Location == Point.Empty)
				{
					cmMenu.SetBounds(
						System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - cmMenu.Bounds.Width,
						System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - cmMenu.Bounds.Height,
						0, 0, BoundsSpecified.Location);
				}
			}
			menuItemClick = itemClick;
			menuButtonClick = buttonClick;
		}

		private void MenuBuilderMenuClick(object sender, WorkDataEventArgs e)
		{
			if ((MouseButtons & MouseButtons.Right) != 0)
			{
				SetClipboardData(e.WorkData.Id.Value);
				return;
			}
			if (menuItemClick == null)
			{
				if (!TryAssignWorkIfApplicable(e, true))
				{
					MenuBuilderMenuClickDefault(e);
				}
			}
			else
			{
				if (!TryAssignWorkIfApplicable(e, false))
				{
					menuItemClick(e);
				}
			}
			cmMenu.Visible = false; //if menu is changed during ContextMenuStrip is visible then we have to hide it manually
		}

		private void MenuBuilderMenuButtonClick(object sender, WorkDataEventArgs e)
		{
			if (menuButtonClick == null)
			{
				MenuBuilderMenuButtonClickDefault(e);
			}
			else
			{
				menuButtonClick(e);
			}
			cmMenu.Visible = false; //if menu is changed during ContextMenuStrip is visible then we have to hide it manually
		}

		private void MenuBuilderMenuButtonClickDefault(WorkDataEventArgs e)
		{
			workManagementService.DisplayWorkDetailsGui(e.WorkData);
		}

		private void MenuBuilderMenuClickDefault(WorkDataEventArgs e)
		{
			Debug.Assert(e.WorkData.Id.HasValue);
			currentWorkController.UserStartWork(e.WorkData);
		}

		private bool TryAssignWorkIfApplicable(WorkDataEventArgs e, bool switchToWork)
		{
			if (e.OwnTask) return false;
			var res = notificationService.ShowMessageBox(string.Format(Labels.NotificationAssignMeConfirmBody, e.WorkData.Name),
				Labels.NotificationAssignMeConfirmTitle,
                Forms.MessageBoxButtons.YesNo,
                Forms.MessageBoxIcon.Question);
			log.Info("Confirmation of assign workId " + e.WorkData.Id.Value + " switch: " + switchToWork + " response: " + res);
			if (res == Forms.DialogResult.Yes)
			{
				//don't block the gui (quick and dirty)
				ThreadPool.QueueUserWorkItem(_ =>
				{
					var result = AllWorksManager.TryAssignTask(e.WorkData);
					context.Post(__ =>
					{
						if (result == AssignTaskResult.Ok)
						{
							if (switchToWork) currentWorkController.StartOrQueueWork(new WorkData() { Id = e.WorkData.Id });
							captureCoordinator.RefreshMenuAsync();
						}
						else if (result == AssignTaskResult.AccessDenied)
						{
							notificationService.ShowNotification(NotificationKeys.AssignTaskError,
								nfAssigntaskErrorDuration,
								Labels.NotificationAssignTaskAccessDeniedTitle,
								Labels.NotificationAssignTaskAccessDeniedBody,
								CurrentWorkController.NotWorkingColor);
						}
						else
						{
							notificationService.ShowNotification(NotificationKeys.AssignTaskError,
								nfAssigntaskErrorDuration,
								Labels.NotificationAssignTaskErrorTitle,
								Labels.NotificationAssignTaskErrorBody,
								CurrentWorkController.NotWorkingColor);
						}
					}, null);
				});
			}
			return true;
		}

		//there was a bugreport where this function never returned (which froze the CaptureWorkItemCallback in CaptureManager)
		//private void PersistWorkItem(object sender, WorkItemEventArgs e)
		//{
		//    workItemManager.PersistAndSend(e.WorkItem);
		//    workTimeCounter.AddWorkItem(e.WorkItem);
		//    idleDetector.AddWorkItem(e.WorkItem);
		//}

		private void CaptureCoordinatorWorkItemCreated(object sender, WorkItemEventArgs e)
		{
			workTimeCounter.AddWorkItem(e.WorkItem);
			IdleDetector.AddWorkItem(e.WorkItem);
		}

		public void Exit(bool force = false)
		{
			if (IsDuringExit) return;
			IsDuringExit = true;
			if (currentWorkController.MutualWorkTypeCoordinator.IsWorking)
			{
				log.Info("Exit clicked while working");
				if ((currentWorkController.IsWorking && force) || forceRestartAfterPrompt)
				{
					log.Info("Forcing exit");
				}
				else
				{
					var result = notificationService.ShowMessageBox(Labels.ConfirmExitStillWorkingBody, 
						Labels.ConfirmExitStillWorkingTitle, Forms.MessageBoxButtons.OKCancel);
					if (result != Forms.DialogResult.OK)
					{
						IsDuringExit = false;
						return;
					}
					log.Info("Exit confirmed while working");
				}

				currentWorkController.MutualWorkTypeCoordinator.RequestStopWork(true, "Exit");
			}
			else
			{
				log.Info("Exit clicked and not working");
			}

			//AppControlServiceHelper.UnregisterProcess();
			Close();
		}

		private void miExit_Click(object sender, EventArgs e)
		{
			log.Debug("UI - Exit clicked from old menu");
			TelemetryHelper.RecordFeature("Exit", "Clicked");
			Exit();
		}

		private void ActivityRecorderForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (stopping) return;
			log.Debug("Form closed event started");
			niTaskbar.Visible = false; //hide balloontip and icon on exit
			stopping = true;
			if (init2completed)
			{
				todoManager.Stop();
				log.Debug("todoListManager stopped");
				messageManager.Stop();
				log.Debug("messageManager stopped");
				acquireLogsManager.Stop();
				log.Debug("acquireLogsManager stopped");
				currentWorkController.IsShuttingDown = true;
				this.niTaskbar.ContextMenuStrip = null;
				contextMenuForm.Close();
				log.Debug("contextMenuForm Closed");
				notificationService.CloseAll();
				log.Debug("notificationService CloseAll called");
				hotkeyRegistrar.Dispose();
				log.Debug("hotkeyRegistrar Disposed");
				Application.RemoveMessageFilter(hotkeyService as IMessageFilter);
				log.Debug("RemoveMessageFilter Finished");
				hotkeyRegistrar.HotkeyPressed -= HandleHotkey;
				guiThreadWatchdogManager.Stop();
				taskbarTimer.Enabled = false;
				workTimeStatsFromWebsiteManager.Dispose();
				captureCoordinator.Stop();
				captureCoordinator.CurrentMenuChanged -= MenuManagerCurrentMenuChanged;
				captureCoordinator.WorkItemManager.ConnectionStatusChanged -= WorkItemManagerConnectionStatusChanged;
				captureCoordinator.WorkItemManager.CannotPersistAndSendWorkItem -= WorkItemManagerCannotPersistAndSendWorkItem;
				captureCoordinator.WorkItemCreated -= CaptureCoordinatorWorkItemCreated;
				captureCoordinator.Dispose();
				log.Debug("captureCoordinator Stopped");
				workManagementService.OnTaskReasonsChanged -= TaskReasonsChanged;
				workTimeStatsManager.Stop();
#if LegacySimple
			workTimeStatsManager.SimpleWorkTimeStatsReceived -= SimpleWorkTimeStatsReceived;
#else
				captureCoordinator.StatsCoordinator.SimpleWorkTimeStatsCalculated += SimpleWorkTimeStatsReceived;
#endif
				workTimeStatsManager.PasswordError -= PasswordError;
				workTimeStatsManager.ActiveOnlyError -= ActiveOnlyError;
				workTimeStatsManager.PasswordExpiredError += PasswordExpiredError;
				log.Debug("workTimeStatsManager Stopped");
				allWorksManager.Stop();
				allWorksManager.AllWorksChanged -= AllWorksChanged;
				log.Debug("allWorksManager Stopped");
				menuBuilder.MenuClick -= MenuBuilderMenuClick;
				menuBuilder.MenuButtonClick -= MenuBuilderMenuButtonClick;
				recentWorks.MenuClick -= MenuBuilderMenuClick;
				searchWorks.MenuClick -= MenuBuilderMenuClick;
				currentWorkItem.ButtonClick -= currentWorkItem_ButtonClick;
			}

			if (ConfigManager.AutoUpdateManagerEnabled)
			{
				autoUpdateManager.Stop();
				autoUpdateManager.NewVersionInstalledEvent -= AutoUpdateManagerNewVersionInstalledEvent;
				log.Debug("autoUpdateManager Stopped");
			}
			networkStabilityManager.Stop();
			log.Debug("networkStabilityManager Stopped");

			//ScreenshotAnalystManager.Stop();

			if (activeDirectoryAuthManager != null)
			{
				activeDirectoryAuthManager.Stop();
				activeDirectoryAuthManager.UserIdChanged -= ActiveDirectoryAuthManagerOnUserIdChanged;
				log.Debug("windowsUserAuthManager Stopped");
			}
			if (logoutOnStop)
			{
				ConfigManager.Logout();
			}
			log.Info("Form closed");
		}

		private void WorkItemManagerCannotPersistAndSendWorkItem(object sender, EventArgs e)
		{
			context.Post(_ =>
				notificationService.ShowNotification(NotificationKeys.PersistAndSendError, nfPersistAndSendErrorDuration,
													Labels.NotificationPersistAndSendErrorTitle, Labels.NotificationPesistAndSendErrorBody)
				, null);
		}

		private void WorkItemManagerConnectionStatusChanged(object sender, EventArgs e)
		{
			var isOnline = captureCoordinator.WorkItemManager.IsOnline;
			context.Post(_ =>
				{
					if (stopping) return;
					currentWorkController.IsOnline = isOnline;
					if (!isOnline || DppNeedAccept || dppData?.AcceptedAt != null) return; 
					log.Debug("Start deferred dpp check after become online");
					dppData = DppHelper.GetAcceptanceData();
					if (dppData != null && !dppData.AcceptedAt.HasValue)
					{
						log.Debug("Not accepted dpp, need to show dpp dialog");
						DppNeedAccept = true;
						DisplayWelcome(true, dppData, () =>
						{
							DppNeedAccept = false;
							DppHelper.SetAcceptanceDate(captureCoordinator);
						});
					}
				}, null);
		}

		private void SetIcon()
		{
			if (IsDisposed) { return; }
			niTaskbar.Icon = currentWorkController.CurrentWork != null
				? currentWorkController.IsOnline
					? currentWorkController.IsRuleOverrideEnabled
						? iconWorkingLockOnline
						: iconWorkingOnline
					: currentWorkController.IsRuleOverrideEnabled
						? iconWorkingLockOffline
						: iconWorkingOffline
				: iconNotWorking;
		}

		private void ShowContextMenu(Point position)
		{
			if (stopping) return;
			Debug.Assert(contextMenuForm != null && !contextMenuForm.IsDisposed);
			UpdateWorkTimeAndTaskbarInfoNew();
			contextMenuForm.Show(position);
			ThreadPool.QueueUserWorkItem(_ => { menuTabHelper.RefreshTabNames(); menuReportHelper.RefreshFavoriteReports(); menuReportHelper.RefreshDisplayedReports(); });
		}

		// You might be saying WTF? But this is the WinForms way... http://stackoverflow.com/questions/2208690/invoke-notifyicons-context-menu
		private void ShowOldContextMenu()
		{
			niTaskbar.ContextMenuStrip = cmMenu;
			var mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
			mi.Invoke(niTaskbar, null);
			niTaskbar.ContextMenuStrip = null;
		}

		private void niTaskbar_MouseClick(object sender, MouseEventArgs e)
		{
			log.Debug($"UI - Taskbar icon clicked (single click, button: {e.Button})");
			if (e.Button == MouseButtons.Right)
			{
				if (ModifierKeys == (Keys.Shift | Keys.Control) || ConfigManager.LocalSettingsForUser.ShowOldMenu)
				{
					log.Debug("UI - Taskbar menu clicked (old menu)");
					ShowOldContextMenu();
				}
				else
				{
					log.Debug("UI - Taskbar icon clicked (new menu)");
					TelemetryHelper.RecordFeature("MainMenu", "Open");
					lastContextPosition = Cursor.Position;
					ShowContextMenu(Cursor.Position);
				}

				return;
			}
			if (e.Button != MouseButtons.Left ||
				stopping ||
				!ConfigManager.LocalSettingsForUser.UseDoubleClickForStatusChange.HasValue ||
				(ConfigManager.LocalSettingsForUser.UseDoubleClickForStatusChange.HasValue && ConfigManager.LocalSettingsForUser.UseDoubleClickForStatusChange.Value))
				return;
			TaskbarClicked();
		}

		private void niTaskbar_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			log.Debug($"UI - Taskbar icon clicked (double click, button: {e.Button})");
			if (e.Button == MouseButtons.Right)
			{
				if (ModifierKeys == (Keys.Shift | Keys.Control) || ConfigManager.LocalSettingsForUser.ShowOldMenu)
				{
					log.Debug("UI - Taskbar menu clicked (old menu)");
					TelemetryHelper.RecordFeature("OldMainMenu", "Open");
					ShowOldContextMenu();
				}
				else
				{
					log.Debug("UI - Taskbar menu clicked (new menu)");
					TelemetryHelper.RecordFeature("MainMenu", "Open");
					lastContextPosition = Cursor.Position;
					ShowContextMenu(Cursor.Position);
				}

				return;
			}

			if (e.Button != MouseButtons.Left ||
				stopping ||
				!ConfigManager.LocalSettingsForUser.UseDoubleClickForStatusChange.HasValue ||
				(ConfigManager.LocalSettingsForUser.UseDoubleClickForStatusChange.HasValue && !ConfigManager.LocalSettingsForUser.UseDoubleClickForStatusChange.Value))
				return;
			TaskbarClicked();
		}

		private void TaskbarClicked()
		{
			//let the forms WndProc handle this message, otherwise reentrancy could occur in managed blocking
			if (Control.ModifierKeys == Keys.Control)
			{
				TelemetryHelper.RecordFeature("CurrentWork", "Lock");
				context.Post(_ => LockOrUnlockCurrentWork(), null);
			}
			else
			{
				TelemetryHelper.RecordFeature("CurrentWork", "StartStop");
				context.Post(_ => ResumeOrStopWork(), null);
			}
		}

		private void ResumeOrStopWork()
		{
			if (currentWorkController.CurrentWorkState != WorkState.NotWorking)
			{
				currentWorkController.UserStopWork();
				RestartIfNewVersionInstalled(false);
			}
			else
			{
				currentWorkController.UserResumeWork();
			}
		}

		private void LockOrUnlockCurrentWork()
		{
			if ((ConfigManager.RuleRestrictions & RuleRestrictions.CannotOverrideRules) != 0)
			{
				if (currentWorkController.IsRuleOverrideEnabled)
				{
					currentWorkController.IsRuleOverrideEnabled = false;
					log.Info("Rule override is " + (currentWorkController.IsRuleOverrideEnabled ? "Enabled" : "Disabled"));
				}
				return;
			}
			//todo can we toggle override when offline ? should we disable override when work is changed or when switching to non-working state (probably not)?
			var currentWork = currentWorkController.CurrentWork;
			if (currentWork == null) return;
			if (currentWorkController.CurrentWorkState == WorkState.WorkingTemp) //we need WorkState.Working to override rules
			{
				currentWorkController.TempEndEffect(); //probably we want to EndTempEffect rather than currentWorkController.UserStartWork(currentWork);
			}
			currentWorkController.IsRuleOverrideEnabled = !currentWorkController.IsRuleOverrideEnabled;
			log.Info("Rule override is " + (currentWorkController.IsRuleOverrideEnabled ? "Enabled" : "Disabled"));
		}

		private bool isRestarting;
		private void RestartIfNewVersionInstalled(bool isIdle)
		{
			if (!restartForNewVersion) return;
			if (isRestarting) return; //there can be more messages in the queue that can cause restart
			isRestarting = true;
			if (isIdle)
			{
				log.Info("Saving idle info");
				IsolatedStorageSerializationHelper.Save(IdleQuitPath, true);
			}
			//AppControlServiceHelper.UnregisterProcess();
			var canRestart = autoUpdateManager.UpdateService.RestartWithNewVersion();
			if (!canRestart)
			{
				log.Info("RestartOnExit");
				ProgramWin.RestartOnExit = true;
				this.Close();
			}
			else
				Exit();
		}

		private void Restart()
		{
			try
			{
				//AppControlServiceHelper.UnregisterProcess();
				Application.Restart();
			}
			catch (Exception ex) //ClickOnce bugs (NullReferenceException, InvalidOperationException)
			{
				log.Error("Failed to restart program", ex);
				ProgramWin.RestartOnExit = true;
				this.Close();
			}
		}

		private const int WM_CLOSE = 0x0010;
		private const int WM_DESTROY = 0x0002;
		private const int WM_QUERYENDSESSION = 0x0011;
		private const int WM_ENDSESSION = 0x0016;
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 0x800)
			{
				//Do something meaningful
			}
			if (m.Msg == SameAppNotifyHelper.WM_CUSTOMNOTIFY)
			{
				//refresh icon display (workaround for XP, Vista, Win7 tray bug) http://www.tech-pro.net/howto_013.html // http://winhlp.com/node/16 // http://support.microsoft.com/kb/945011
				if (stopping) return;
				log.Info("Other instance started");
				BlinkIcon();
				return;
			}
			else if (m.Msg == 536 && m.WParam.ToInt32() == 4) //WM_POWERBROADCAST PBT_APMSUSPEND //http://msdn.microsoft.com/en-us/library/aa372716
			{
				//redundant check because we have some lost suspends on Win7
				log.Info("PowerModeChange: Suspend (main)"); //if this is faster than SystemEvents...
				StopWorkFromGui(); //performance critical
			}
			else if (m.Msg == WinApi.WM_SETTINGCHANGE && m.WParam.ToInt32() == WinApi.SPI_WORKAREA)
			{
				log.Info("Screen resolution changed");
			}
			else if (m.Msg == WM_CLOSE)
			{
				log.Info("Close message received");
			}
			else if (m.Msg == WM_DESTROY)
			{
				log.Info("Destroy message received");
			}
			else if (m.Msg == WM_QUERYENDSESSION)
			{
				log.Info("Query end session message received LParam: " + m.LParam);
			}
			else if (m.Msg == WM_ENDSESSION)
			{
				log.Info("End session message received WParam: " + m.WParam + " LParam: " + m.LParam);
			}


			base.WndProc(ref m);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			log.Info("Form closing reason: " + e.CloseReason);
			base.OnFormClosing(e);
			if (e.Cancel)
			{
				log.Info("Form closing cancelled");
			}
		}

		//try to figure out form closed event bug: http://stackoverflow.com/questions/13526534/are-there-any-cases-where-application-exit-doesnt-raise-the-formclosing-event
		private bool isHandleCreatedLocal;
		protected override void OnHandleCreated(EventArgs e)
		{
			if (isHandleCreatedLocal)
			{
				log.ErrorAndFail("Handle is recreated at: " + Environment.NewLine + new StackTrace());
			}
			isHandleCreatedLocal = true;
			base.OnHandleCreated(e);
		}

		private void BlinkIcon()
		{
			if (stopping || !ConfigManager.IsTaskBarIconShowing) return;
			this.niTaskbar.Visible = false;
			this.niTaskbar.Visible = true;
			log.Info("Refreshed icon");
		}

		public void Logout()
		{
			var result = notificationService.ShowMessageBox(Labels.ConfirmChangeUserBody, Labels.ConfirmChangeUserTitle, Forms.MessageBoxButtons.OKCancel);
			if (result != Forms.DialogResult.OK) return;
			log.Info("Change userId confirmed");
			logoutOnStop = true;
			Restart();
		}

		private void miLogout_Click(object sender, EventArgs e)
		{
			log.Debug("UI - Quit clicked from old menu");
			Logout();
		}
		private static bool OfflineWorkIsAllowed { get { return ConfigManager.DuringWorkTimeIdleManualInterval >= 0; } }
		private FullScreenBorderAlertForm fullScreenBorderAlertForm;
		private void taskbarTimer_Tick(object sender, EventArgs e)
		{
			guiThreadWatchdogManager.Reset();
			var isWorking = currentWorkController.CurrentWork != null;
			UpdateWorkTimeAndTaskbarInfo(isWorking ? currentWorkController.CurrentOrLastWorkNameInTwoLines : null);
			//todo we cannot put this into UpdateWorkTimeAndTaskbarInfo becuase it is called before we have all the data causing more popups instead of one
			var createdNew = workManagementService.DisplayWarnNotificationIfApplicable();
			if (createdNew && cmMenu.Visible)
			{
				//if new notification is shown and menu is visible then bring the menu to the front
				cmMenu.BringToFrontAll(); //this won't hide the dropdown if it's over the contextmenu
				searchWorks.BringToFrontAll(); //search should be at the top
			}
			if (IdleDetector.IsIdleAfterWorkTime)
			{
				log.Info("Idling and working after work time so stop working");
				if (currentWorkController.CurrentWorkState != WorkState.NotWorking)
				{
					currentWorkController.UserStopWork();
				}
				ShowIdleMessage();
			}
			if (IdleDetector.IsIdleDuringWorkTime)
			{
				log.Info("Idling and working during work time so stop working");
				if (currentWorkController.CurrentWorkState != WorkState.NotWorking)
				{
					if (OfflineWorkIsAllowed)
					{
						ShowAddMeetingWorkForm(ConfigManager.DuringWorkTimeIdleInMins);			// create new one
						TelemetryHelper.RecordFeature("Meeting", "StartAdhoc");
					}
					else
					{
						currentWorkController.UserStopWork();
					}
				}
				ShowIdleMessage();
			}
			CheckIfUpdateFailed();
			CheckTimeZone();

			var remaining = IdleDetector.RemainingIdleTime;
			var idleFlashInterval = ConfigManager.LocalSettingsForUser.IdleAlertVisual || ConfigManager.LocalSettingsForUser.IdleAlertBeep ? 
				remaining < 4000 ? 500 : remaining < 12000 ? 1000 : remaining < 30000 ? 2000 : 0 
				: 0;
			if (idleFlashInterval > 0 && fullScreenBorderAlertForm == null) fullScreenBorderAlertForm = new FullScreenBorderAlertForm();
			fullScreenBorderAlertForm?.SetInterval(idleFlashInterval);
		}

		private void HandleUserActivity(object sender, SingleValueEventArgs<bool> e)
		{
			context.Post(_ =>
			{
				IdleDetector.ResetRemainingLastFraction();
				fullScreenBorderAlertForm?.SetInterval(0);
			}, null);
		}

		private void ShowIdleMessage()
		{
			notificationService.ShowNotification(NotificationKeys.IdleStopWork, nfIdleStopWorkDuration,
												 Labels.NotificationWorkIdleTitle, Labels.NotificationWorkIdleBody,
												 CurrentWorkController.NotWorkingColor);
		}

		private void ShowAddMeetingWorkForm(int? idleMins = null, int? workId = null)
		{
			currentWorkController.AdhocMeetingService.StartWork(idleMins, workId);
		}

		public void ShowRules()
		{
			captureCoordinator.RuleManagementService.DisplayWorkDetectorRulesEditingGui(false);
		}

		public void ShowHelp()
		{
			DisplayWelcome(true, dppData);
		}

		private void miWorkDetectorRules_Click(object sender, EventArgs e)
		{
			ShowRules();
		}

		#region Create dummy menu for testing
		//using (var c = new ActivityRecorderClientWrapper())
		//{
		//    c.Client.SetClientMenu(13, new ClientMenu()
		//    {
		//        Works = new List<WorkData>()
		//                    {
		//                        new WorkData() {Name = "őőúűáóüöíé", Id = 23,},
		//                        new WorkData()
		//                            {
		//                                Name = "X",
		//                                Children = new List<WorkData>()
		//                                            {
		//                                                new WorkData()
		//                                                    {
		//                                                        Name = "XY",
		//                                                        Children = new List<WorkData>()
		//                                                                    {
		//                                                                        new WorkData() {Name = "XYX2", Id = 2,},
		//                                                                        new WorkData() {Name = "XYY4", Id = 4,},
		//                                                                        new WorkData() {Name = "XYZ3", Id = 3,},
		//                                                                    },
		//                                                    },
		//                                                new WorkData() {Name = "",},
		//                                                new WorkData() {Name = "XZ1", Id = 1,},
		//                                            },
		//                            },
		//                        new WorkData() {Name = "Meet", Id = 26, ManualAddWorkDuration = TimeSpan.FromHours(1),},
		//                    }
		//    });
		//} 
		#endregion

		private void miOpenLog_Click(object sender, EventArgs e)
		{
			log.Debug("UI - Open log clicked from old menu");
			try
			{
				var logFilePath = LogManager.GetRepository().GetAppenders().OfType<log4net.Appender.FileAppender>().First().File;
				Process.Start("notepad.exe", logFilePath);
			}
			catch (Exception ex)
			{
				log.Error("Unable to open log file", ex);
				TopMostMessageBox.Show(Labels.NotificationCannotOpenLogBody, Labels.NotificationCannotOpenLogTitle, MessageBoxButtons.OK);
			}
		}

		private void miLogLevelChange_Click(object sender, EventArgs e)
		{
			var rep = LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;
			if (rep == null)
			{
				TopMostMessageBox.Show(Labels.NotificationCannotChangeLogLevelBody, Labels.NotificationCannotChangeLogLevelTitle, MessageBoxButtons.OK);
				return;
			}
			if (!miLogLevelChange.Checked)
			{
				log.Info("Switching to VERBOSE logging");
				rep.Root.Level = rep.LevelMap["VERBOSE"];
				miLogLevelChange.Checked = true;
			}
			else
			{
				log.Info("Switching to DEBUG logging");
				rep.Root.Level = rep.LevelMap["DEBUG"];
				miLogLevelChange.Checked = false;
			}
		}

		//TODO: Localizing name of the related menu item
		private bool isMeetingToolOpen;
		private void miOpenMeetingTool_Click(object sender, EventArgs e)
		{
			if (isMeetingToolOpen) return;	//TODO: using real synchronization //TODO: bring to front
			isMeetingToolOpen = true;

			using (MeetingCaptureTestTool meetingTestToolForm = new MeetingCaptureTestTool())
			{
				meetingTestToolForm.ShowDialog(this);
			}

			isMeetingToolOpen = false;
		}

		private void miOpenMeetingLog_Click(object sender, EventArgs e)
		{
			log.Debug("UI - Open logs clicked from old menu");
			try
			{
				//var logFilePath = LogManager.GetRepository().GetAppenders().OfType<log4net.Appender.FileAppender>().First().File;
				var logFilePath = "OutlookSync\\Logs\\JC.Meeting.log";
				if (System.IO.File.Exists(logFilePath))
				{
					Process.Start("notepad.exe", logFilePath);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to open log file", ex);
				TopMostMessageBox.Show(Labels.NotificationCannotOpenLogBody, Labels.NotificationCannotOpenLogTitle, MessageBoxButtons.OK);
			}
		}

		private DomCaptureForm domCaptureForm;
		private void miDomCapture_Click(object sender, EventArgs e)
		{
			log.Debug("UI - Dom capture clicked from old menu");
			if (domCaptureForm != null && !domCaptureForm.IsDisposed) return;
			using (domCaptureForm = new DomCaptureForm() { Owner = this })
			{
				domCaptureForm.ShowDialog();
			}
		}

		private void cmMenu_Opening(object sender, CancelEventArgs e)
		{
			miErrorResolution.Available = (Control.ModifierKeys == (Keys.Control | Keys.Shift));
			miDiagDebugMode.Visible = ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.Enabled);
			miDiagDebugDisableDomCapture.Checked = ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableDomCapture);
			miDiagDebugDisableJcMail.Checked = ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableOutlookJcMailCapture);
			miDiagDebugDisableOlAddin.Checked = ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableOutlookAddinCapture);
			miDiagDebugDisableAutomationPlugin.Checked = ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableAutomationCapture);
			miDiagDebugDisableAllPlugin.Checked = ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableAllPluginCapture);
			miDiagDebugDisableUrlCapture.Checked = ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableUrlCapture);
			miDiagDebugDisableTitleCapture.Checked = ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableTitleCapture);
			miDiagDebugDisableProcessCapture.Checked = ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableProcessCapture);
			miDiagDebugDisableOutlookMeetingSync.Checked = !ConfigManager.IsOutlookMeetingTrackingEnabled;
			miDiagDebugDisableLotusMeetingSync.Checked = !ConfigManager.IsLotusNotesMeetingTrackingEnabled;
			miSafeMailItemCommit.Checked = ConfigManager.LocalSettingsForUser.IsSafeMailItemCommitUsable;
		}

		private static void ShowLanguageChangeNotification(System.Globalization.CultureInfo newCulture)
		{
			var title = Labels.ResourceManager.GetString("NotificationLanguageChangeTitle", newCulture);
			var body = Labels.ResourceManager.GetString("NotificationLanguageChangeBody", newCulture);
			TopMostMessageBox.Show(body, title, MessageBoxButtons.OK);
		}

		public void RefreshWork()
		{
			allWorksManager.SynchronizeDataAsync();
		}

		public void RefreshSearchContent()
		{
			bool needSearchAllWorks = !ConfigManager.LocalSettingsForUser.SearchOwnTasks || ConfigManager.LocalSettingsForUser.SearchInClosed;
			if (needSearchAllWorks)
			{
				if (!lastSearchAllWorks)
				{
					lastSearchAllWorks = true;
					allWorksManager.LoadData();
					allWorksManager.Start(0); // refresh immediatelly after start
					log.Debug("allWorksManager Started");
					return;
				}

				var works = AllWorksManager.GenWorkDatas(allWorkItems, ConfigManager.LocalSettingsForUser.SearchOwnTasks,
					ConfigManager.LocalSettingsForUser.SearchInClosed);
				contextMenuForm.UpdateAllWorks(works);
				searchWorks.UpdateAllWorks(works);
			}
			else
				if (lastSearchAllWorks)
				{
					lastSearchAllWorks = false;
					allWorksManager.Stop();
					log.Debug("allWorksManager Stopped");
					MenuQuery.Instance.ClientMenuLookup.Update(MenuQuery.Instance.ClientMenuLookup.Value);
					searchWorks.UpdateMenu(captureCoordinator.CurrentMenuLookup);
					return;
				}
			lastSearchAllWorks = needSearchAllWorks;
		}

		private void HandleHotkey(object sender, SingleValueEventArgs<HotkeySetting> e)
		{
			log.Info("HandleHotkey");
			if (e.Value == null) return;
			if (stopping) return;

			log.InfoFormat("Hotkey has been pressed: ({0})", e.Value.ToString());
			var type = e.Value.ActionType;
			int? workId = e.Value.WorkDataId;

			switch (type)
			{
				case HotkeyActionType.ResumeOrStopWork:
					TelemetryHelper.RecordFeature("Hotkey", "StartStop");
					ResumeOrStopWork();
					break;
				case HotkeyActionType.StartWork:
					TelemetryHelper.RecordFeature("Hotkey", "StartWork");
					Debug.Assert(workId.HasValue);
					currentWorkController.UserStartWork(new WorkData() { Id = workId });
					break;
				case HotkeyActionType.StartManualMeeting:
					TelemetryHelper.RecordFeature("Hotkey", "StartMeeting");
					if (ConfigManager.MaxManualMeetingInterval >= 0)
						ShowAddMeetingWorkForm(null, workId);	// create manual meeting
					else
						notificationService.ShowMessageBox(Labels.AddMeeting_DisabledManualMeetingWarningBody, Labels.AddMeeting_DisabledManualMeetingWarningTitle, Forms.MessageBoxButtons.OK);
					break;
				case HotkeyActionType.ToggleMenu:
					TelemetryHelper.RecordFeature("Hotkey", "ToggleMenu");
					if (!contextMenuForm.Visible)
					{
						ShowContextMenu(lastContextPosition ?? Cursor.Position);
					}
					else
					{
						contextMenuForm.Hide();
					}

					break;
				case HotkeyActionType.NewWorkDetectorRule:
					TelemetryHelper.RecordFeature("Hotkey", "NewWorkDetector");
					captureCoordinator.RuleManagementService.DisplayWorkDetectorRulesEditingGui(true);
					break;
				case HotkeyActionType.DeleteCurrentWorkDetectorRule:
					TelemetryHelper.RecordFeature("Hotkey", "DeleteCurrentWorkDetector");
					captureCoordinator.RuleManagementService.DisplayWorkDetectorRuleDeletingGui();
					break;
				case HotkeyActionType.JobCTRL_com:
					TelemetryHelper.RecordFeature("Hotkey", "Web");
					RecentUrlQuery.Instance.OpenLink(e.Value.Website);
					break;
				case HotkeyActionType.CreateWork:
					TelemetryHelper.RecordFeature("Hotkey", "CreateWork");
					DisplayCreateWorkGui();
					break;
				case HotkeyActionType.AddReason:
					TelemetryHelper.RecordFeature("Hotkey", "AddReason");
					if (currentWorkController.CurrentWork != null && currentWorkController.CurrentWork.Id.HasValue)
						workManagementService.DisplayReasonWorkGui(currentWorkController.CurrentWork);
					break;
				case HotkeyActionType.TodoList:
					TelemetryHelper.RecordFeature("Hotkey", "TodoList");
					todoManager.ShowTodoList();
					break;
				case HotkeyActionType.ClearAutoRuleTimer:
					TelemetryHelper.RecordFeature("Hotkey", "ClearAutoRuleTimer");
					captureCoordinator.ClearLearningRuleTimers();
					break;
				case HotkeyActionType.WorkTimeHistory:
					HandleWorkTimeService.ShowModification();
					break;
			}
		}

		private void SystemEventsService_SessionSwitch(object sender, Forms.SessionSwitchEventArgs e)
		{
			if (isOfflineWorkWindowSessionSwitchPending)
			{
				switch ((Microsoft.Win32.SessionSwitchReason)e.Reason)
				{
					case SessionSwitchReason.SessionUnlock:
					case SessionSwitchReason.RemoteDisconnect:
						log.Debug(e.Reason + " event signaled, meeting window will be abandoned");
						offlineWorkWindowSessionSwitchResetEvent.Set();
						break;
				}
			}

			switch ((Microsoft.Win32.SessionSwitchReason)e.Reason)
			{
				case SessionSwitchReason.SessionUnlock:
				case SessionSwitchReason.RemoteConnect:
					IsDesktopLocked = false;
					//ScreenshotAnalystManager.Start();
					break;
				case SessionSwitchReason.SessionLock:
				case SessionSwitchReason.RemoteDisconnect:
					IsDesktopLocked = true;
					//ScreenshotAnalystManager.Stop();
					break;
			}
			if ((Microsoft.Win32.SessionSwitchReason)e.Reason != SessionSwitchReason.SessionLock) return;
			if (!ConfigManager.IsManualMeetingStartsOnLock) return;
			if (!currentWorkController.IsWorking) return;
			var isInWorkTime = captureCoordinator.GetIsWorkTime(DateTime.UtcNow);
			if (isInWorkTime && ConfigManager.MaxManualMeetingInterval < 0) return;	//Or we could stop user work
			if (!isInWorkTime && ConfigManager.DuringWorkTimeIdleManualInterval < 0) return; //Or we could stop user work
			isOfflineWorkWindowSessionSwitchPending = true;
			log.Debug("Pending meeting window while other events received...");
			ThreadPool.QueueUserWorkItem(_ =>
			{
				if (!offlineWorkWindowSessionSwitchResetEvent.WaitOne(60000)) 
					context.Post(__ =>
					{
						log.Info("Starting meeting " + (isInWorkTime ? "in" : "outside") + " worktime due to computer lock");
						var idleTime = IdleDetector.IdleDuringWorkTime + IdleDetector.IdleAfterWorkTime;
						// we lose minute fragments from idletime, but previously we didn't care for whole idle time after computer lock...
						TelemetryHelper.RecordFeature("Meeting", "StartLock");
						ShowAddMeetingWorkForm((int)idleTime.TotalMinutes); //null -> manualmeeting, int -> idlemeeting
					}, null);
				isOfflineWorkWindowSessionSwitchPending = false;
			});
		}

		public List<HotkeySetting> HotKeys
		{
			get
			{
				return hotkeyRegistrar.GetHotkeys();
			}

			set
			{
				hotkeyRegistrar.SetHotkeys(value);
			}
		}

		private HotkeySetting MigrateLegacyHotkey(Keys legacyHotkey, HotkeyActionType actionType)
		{
			Keys keyCode = legacyHotkey & Keys.KeyCode;
			bool shift = (legacyHotkey & Keys.Shift) == Keys.Shift;
			bool ctrl = (legacyHotkey & Keys.Control) == Keys.Control;
			bool alt = (legacyHotkey & Keys.Alt) == Keys.Alt;
			bool win = false;

			var hks = new HotkeySetting()
			{
				KeyCode = (Forms.Keys)keyCode,
				Control = ctrl,
				Shift = shift,
				Alt = alt,
				Windows = win,
				ActionType = actionType,
				WorkDataId = null,
			};

			var hotkeySettings = hotkeyRegistrar.GetHotkeys();
			if (!hotkeySettings.Exists(e => e.ActionType == hks.ActionType && hks.WorkDataId == e.WorkDataId))
			{
				hotkeySettings.Add(hks);
			}
			hotkeyRegistrar.SetHotkeys(hotkeySettings);

			return hks;
		}

		private void miRunAsAdmin_Click(object sender, EventArgs e)
		{
			var result = notificationService.ShowMessageBox(Labels.ConfirmRestartDueElevatedModeBody, Labels.ConfirmRestartDueElevatedModeTitle, Forms.MessageBoxButtons.OKCancel);
			if (result != Forms.DialogResult.OK) return;
			log.Info("Change elevated mode confirmed");
			miRunAsAdmin.Checked = !miRunAsAdmin.Checked;
			ElevatedPrivilegesHelper.RunAsAdmin = miRunAsAdmin.Checked;
			if (ElevatedPrivilegesHelper.IsElevated && !ElevatedPrivilegesHelper.RunAsAdmin)
			{
				log.Info("Restarting to switch limited mode");
				try
				{
					//AppControlServiceHelper.UnregisterProcess();
					var startInfo = new ProcessStartInfo("explorer.exe") { Arguments = System.Reflection.Assembly.GetEntryAssembly().Location };
					Process.Start(startInfo);
					// restart lower privileges (hax running via explorer.exe)
					Application.Exit();
					return;
				}
				catch (Win32Exception ex)
				{
					log.Error("Restarting failed", ex);
				}
			}
			if (!ElevatedPrivilegesHelper.IsElevated && ElevatedPrivilegesHelper.RunAsAdmin)
				Restart(); // switch to elevated after restart
		}

		public void ShowPreferences()
		{
			if (settingsForm != null && !settingsForm.IsDisposed)
			{
				settingsForm.BringToFront();
				return;
			}

			EventHandler<MenuEventArgs> currentMenuChangedEvent = (s, args) => settingsForm.UpdateClientMenuLookup(args.MenuLookup); // invoked from GUI
			captureCoordinator.CurrentMenuChanged += currentMenuChangedEvent;

			settingsForm = new PreferencesForm(this, hotkeyService, captureCoordinator.CurrentMenuLookup);
			settingsForm.Closed += (s, args) => captureCoordinator.CurrentMenuChanged -= currentMenuChangedEvent;
			settingsForm.Show(this);
			TelemetryHelper.RecordFeature("Settings", "Open");
		}

		private PreferencesForm settingsForm;
		private void miPreferencesClick(object sender, EventArgs e)
		{
			log.Debug("UI - Preferences clicked in old menu");
			ShowPreferences();
		}

		public void ShowErrorReport()
		{
			if (errorReportingForm != null && !errorReportingForm.IsDisposed)
			{
				errorReportingForm.BringToFront();
				return;
			}

			errorReportingForm = new ErrorReportingForm();
			errorReportingForm.Show(this);
		}

		private ErrorReportingForm errorReportingForm;

		private DebugCaptureForm debugCaptureForm;
		private bool forceRestartAfterPrompt;

		private void ShowDebugTools()
		{
			if (debugCaptureForm != null && !debugCaptureForm.IsDisposed)
			{
				debugCaptureForm.BringToFront();
				return;
			}

			debugCaptureForm = new DebugCaptureForm();
			debugCaptureForm.Show(this);
		}

		private void miOpenErrorReporting_Click(object sender, EventArgs e)
		{
			ShowErrorReport();
		}

		public void DisplayCreateWorkGui()
		{
			workManagementService.DisplayCreateWorkGui();
		}

		public void DisplayProjectSyncGui()
		{
			projectSyncService.ShowSync();
		}

		public void DisplayUpdateWorkGui(WorkData workToUpdate)
		{
			workManagementService.DisplayUpdateWorkGui(workToUpdate);
		}

		private void HandleDiagnosticToolClicked(object sender, EventArgs e)
		{
			ShowDebugTools();
		}

#if OcrPlugin
		private void HandleOpenContributionFormClicked(object sender, EventArgs e)
		{
			ContributionController.Instance.PopupContributionForm(true);
		}
#endif

		public long AddEtcExtraMenuitem(Func<string> textAccessor, Action clickHandler)
		{
			return contextMenuForm.AddEtcExtraMenuitem(textAccessor, clickHandler);
		}

		public void RemoveEtcExtraMenuitem(long menuid)
		{
			contextMenuForm.RemoveEtcExtraMenuitem(menuid);
		}

		internal void HideIcon()
		{
			niTaskbar.Visible = false;
		}

		public string ErrorStatus
		{
			get
			{
				var r = "";
				if (currentWorkController.IsWorking && !currentWorkController.IsOnline)
				{
					switch (captureCoordinator.WorkItemManager.OfflineReason)
					{
						case WorkItemManager.OfflineReasonEnum.Timeout:
							r += Labels.OfflineReason_Timeout+"\n";
							break;
						case WorkItemManager.OfflineReasonEnum.EndpointNotFound:
							r += Labels.OfflineReason_EndpointNotFound+"\n";
							break;
					}

					if (captureCoordinator.HasRejectedWorkItem)
					{
						r += Labels.OfflineDataNotUploaded+"\n";

					}
				}
				return r != "" ? "\n" + r + "\n" : "";
			}
		}

		private void miDiagDebugDisableClick(object sender, EventArgs e)
		{
			if (!(sender is ToolStripMenuItem control) || !(control.Tag is string)) return;
			if ((string)control.Tag == "DisableOutlookMeetingSync")
			{
				ConfigManager.IsOutlookMeetingTrackingEnabled = !ConfigManager.IsOutlookMeetingTrackingEnabled;
				miDiagDebugDisableOutlookMeetingSync.Checked = !ConfigManager.IsOutlookMeetingTrackingEnabled;
				return;
			}
			if ((string)control.Tag == "DisableLotusMeetingSync")
			{
				ConfigManager.IsLotusNotesMeetingTrackingEnabled = !ConfigManager.IsLotusNotesMeetingTrackingEnabled;
				miDiagDebugDisableLotusMeetingSync.Checked = !ConfigManager.IsLotusNotesMeetingTrackingEnabled;
				return;
			}
			
			var flag = (Common.DiagnosticOperationMode)Enum.Parse(typeof(Common.DiagnosticOperationMode), (string)control.Tag);
			ConfigManager.DiagnosticOperationMode = ConfigManager.DiagnosticOperationMode ^ (flag & ~Common.DiagnosticOperationMode.Enabled);
			control.Checked = ConfigManager.CheckDiagnosticOperationMode(flag);
		}

		public bool IsExcludedFromObserving(Point position)
		{
			if  (lastContextPosition.HasValue && lastContextPosition.Value.X - 8 <= position.X && position.X <= lastContextPosition.Value.X + 8 && lastContextPosition.Value.Y - 8 <= position.Y && position.Y <= lastContextPosition.Value.Y + 8) return true;
			return contextMenuForm.Visible && contextMenuForm.DesktopBounds.Contains(position);
		}

		private volatile bool isUpdatingWorkTime = false;
		private volatile bool isMouseMoveOccured = false;
		private const int mouseMoveRefreshIntervalInMilliseconds = 10 * 1000;

		private void UpdateWorkTimeAndTaskbarInfoNew()
		{
			Debug.Assert(SynchronizationContext.Current == GuiContext, "Call from the GUI thread!");
			if (isUpdatingWorkTime) return;
			isUpdatingWorkTime = true;
			var todaysWorkTime = workTimeCounter.TodaysWorkTime;
			var stats = workTimeStatsFromWebsiteManager.GetLocalWorkTimeStatsIfExact();
			if (workTimeStatsFromWebsiteManager.HasExactLocalWorkTime)
			{
				try
				{
					contextMenuForm.UpdateStats(stats);
					updateTaskBarText(TimeSpan.FromMilliseconds(stats.TodaysWorkTimeInMs).ToHourMinuteSecondString());
					return;
				}
				finally
				{
					isUpdatingWorkTime = false;
				}
			}

			if (stats == null)
			{
				contextMenuForm.UpdateStats(true, todaysWorkTime);
				updateTaskBarText(todaysWorkTime.ToHourMinuteSecondString());
			}
			else
			{
				contextMenuForm.UpdateStats(stats);
				updateTaskBarText(TimeSpan.FromMilliseconds(stats.TodaysWorkTimeInMs).ToHourMinuteSecondString());
			}

			ThreadPool.QueueUserWorkItem(_ =>
				workTimeStatsFromWebsiteManager.GetWorkTimeStatsFromServer(
					timeStats => GuiContext.Post(__ =>
					{
						try
						{ 
							contextMenuForm.UpdateStats(timeStats);
							updateTaskBarText(TimeSpan.FromMilliseconds(timeStats.TodaysWorkTimeInMs).ToHourMinuteSecondString());
						}
						finally
						{
							isUpdatingWorkTime = false;
						}
					}, null),
					timeSpan =>
					{
						GuiContext.Post(__ =>
						{
							try { 
								contextMenuForm.UpdateStats(false, timeSpan);
								updateTaskBarText(timeSpan.ToHourMinuteSecondString());
							}
							finally
							{
								isUpdatingWorkTime = false;
							}
						}, null);
					}));
		}

		private void updateTaskBarText(string timeStat)
		{
			var isWorking = currentWorkController.CurrentWork != null;
			var currentWork = isWorking ? currentWorkController.CurrentOrLastWorkNameInTwoLines : null;
			var textToSet = taskbarHeader + "\n"
			                              + ErrorStatus
			                              + timeStat
			                              + (currentWork == null ? "" : "\n" + currentWork);
			this.niTaskbar.SetText(textToSet.Length > 127 ? textToSet.Substring(0, 124) + "..." : textToSet);
		}

		private void niTaskbar_MouseMove(object sender, MouseEventArgs e)
		{
			if (isMouseMoveOccured) return;
			isMouseMoveOccured = true;
			UpdateWorkTimeAndTaskbarInfoNew();
			System.Threading.Timer timer = null;
			timer = new System.Threading.Timer(_ =>
			{
				isMouseMoveOccured = false;
				timer.Dispose();
			}, null, mouseMoveRefreshIntervalInMilliseconds, System.Threading.Timeout.Infinite);
		}

		private void miIdleAlertVisual_Click(object sender, EventArgs e)
		{
			ConfigManager.LocalSettingsForUser.IdleAlertVisual = !ConfigManager.LocalSettingsForUser.IdleAlertVisual;
			miIdleAlertVisual.Checked = ConfigManager.LocalSettingsForUser.IdleAlertVisual;
		}

		private void miIdleAlertBeep_Click(object sender, EventArgs e)
		{
			ConfigManager.LocalSettingsForUser.IdleAlertBeep = !ConfigManager.LocalSettingsForUser.IdleAlertBeep;
			miIdleAlertBeep.Checked = ConfigManager.LocalSettingsForUser.IdleAlertBeep;
		}

		private void miSafeMailItemCommit_Click(object sender, EventArgs e)
		{
			ConfigManager.LocalSettingsForUser.IsSafeMailItemCommitUsable = !ConfigManager.LocalSettingsForUser.IsSafeMailItemCommitUsable;
			miSafeMailItemCommit.Checked = ConfigManager.LocalSettingsForUser.IsSafeMailItemCommitUsable;
		}

		private void miOpenJCMon_Click(object sender, EventArgs e)
		{
			//new JCIAccessibilityForm().Show();
		}
	}
}
