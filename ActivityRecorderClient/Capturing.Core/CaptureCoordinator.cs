using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop;
using Tct.ActivityRecorderClient.Capturing.Extra;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.InterProcess;
using Tct.ActivityRecorderClient.Meeting;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Menu.Management;
using Tct.ActivityRecorderClient.Menu.Selector;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Rules.Actions;
using Tct.ActivityRecorderClient.Rules.Collector;
using Tct.ActivityRecorderClient.Sleep;
using Tct.ActivityRecorderClient.Stats;
using Tct.ActivityRecorderClient.SystemEvents;
using Tct.ActivityRecorderClient.Telemetry;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Class for coordinating capturing related messages accross several classes/threads. 
	/// </summary>
	public class CaptureCoordinator : IDisposable
	{
		public event EventHandler<SingleValueEventArgs<bool>> OnUserActivity;

		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly CaptureManager captureManager;
		public DesktopCaptureManager desktopCaptureManager;
		private readonly IPluginCaptureService pluginCaptureService;
		private readonly WorkDetector workDetector = new WorkDetector();
		private readonly CollectorCoordinator collector;
		private readonly WorkIdAssigner workIdAssigner = new WorkIdAssigner();
		private readonly WorkDetectorRulesManager workDetectorRulesManager = new WorkDetectorRulesManager();
		private readonly Censor censor = new Censor();
		private readonly CensorRulesManager censorRulesManager = new CensorRulesManager();
		private readonly LearningRuleManager learningRuleManager = new LearningRuleManager();
		private readonly MenuCoordinator menuCoordinator;
		private readonly WorkItemManager workItemManager;
		private readonly MeetingNotifier meetingManager;
		private readonly VersionReportManager versionReportManager = new VersionReportManager();
		private readonly ClientSettingsManager clientSettingsManager;
		private readonly PreferredEndpointManager endpointManager = new PreferredEndpointManager();
		private readonly CreditRunOutManager creditRunOutManager = new CreditRunOutManager();
		private readonly CloseReasonsManager closeReasonsManager = new CloseReasonsManager();
		private readonly CalendarManager calendarManager = new CalendarManager();
		private readonly StatsCoordinator statsCoordinator;
		private readonly ClientKickCoordinator clientKickCoordinator;
		private readonly WorkStatusReporter workStatusReporter;
		private readonly ISystemEventsService systemEvents;
		private readonly ISleepRegulatorService sleepRegulator;
		private readonly IUserActivityService userActivity;
		private readonly IRuleManagementService ruleManagement;
		private readonly IWorkManagementService workManagementService;
		private readonly TimeManager timeManager;
		private readonly IInterProcessManager interProcessManager;
		private readonly DefaultWorkIdSelector defaultWorkIdSelector;

		private readonly CurrentWorkController currentWorkController;
		private readonly WorkAndLayoutGuiCoordinator workAndLayoutGuiCoordinator;
		private readonly DesktopCaptureCoordinator desktopCoordinator;
		private readonly SystemEventsCoordinator systemEventsCoordinator;
		private readonly SynchronizationContext context;
		private readonly INotificationService notificationService;
		private readonly PendingNotificationCoordinator notificationCoordinator;

		private static CaptureCoordinator instance;
		public static CaptureCoordinator Instance
		{
			get
			{
				DebugEx.EnsureGuiThread();
				return instance;
			}
		}

		private static volatile bool isStopping;
		public static bool IsStopping { get { return isStopping; } }

		private volatile bool isStarting;
		private volatile bool hasAllMatchingRule;
		public bool HasAllMatchingRule => hasAllMatchingRule;

		//accessed on the GUI Thread only
		private ClientMenuLookup currentMenuLookup = new ClientMenuLookup();
		public ClientMenuLookup CurrentMenuLookup
		{
			get
			{
				DebugEx.EnsureGuiThread();
				return currentMenuLookup;
			}
			private set
			{
				DebugEx.EnsureGuiThread();
				currentMenuLookup = value;
			}
		}

		public IdleDetector IdleDetector { get; }

		public void PublishMenu(Action<Exception> onErrorCallback)
		{
			menuCoordinator.PublishMenu(onErrorCallback);
		}
		public Keys? WorkDetectorHotKey //this is thread-safe
		{
			get { return workDetector.HotKey; }
			set { workDetector.HotKey = value; }
		}

		//WorkItemManager
		//public event EventHandler<EventArgs> ConnectionStatusChanged;
		//public event EventHandler<EventArgs> CannotPersistAndSendWorkItem;
		public WorkItemManager WorkItemManager { get { return workItemManager; } }
		public CurrentWorkController CurrentWorkController { get { return currentWorkController; } }
		public CreditRunOutManager CreditRunOutManager { get { return creditRunOutManager; } }

		//CaptureManager
		public event EventHandler<WorkItemEventArgs> WorkItemCreated;
		//public event EventHandler<SingleValueEventArgs<WorkStatusChange>> WorkStatusChanged;

		//MenuManager
		public event EventHandler<MenuEventArgs> CurrentMenuChanged;

		public event EventHandler<ThreadFrozenEventArgs> DesktopCaptureFrozen;
		public event EventHandler<SingleValueEventArgs<DesktopCapture>> DesktopCaptured;

		//Rules
		public IRuleManagementService RuleManagementService { get { return ruleManagement; } }

		public ISystemEventsService SystemEventsService { get { return systemEvents; } }

		public StatsCoordinator StatsCoordinator { get { return statsCoordinator; } }

		public MeetingNotifier MeetingNotifier => meetingManager;

		public IWorkManagementService WorkManagementService { get { return workManagementService; } }
		public ClientSettingsManager ClientSettingsManager => clientSettingsManager;
		internal ApplicationStartType StartType { private set; get; }
		private readonly int guiThreadId;

		//Created on the gui thread (and that is crucial)
		public CaptureCoordinator(SynchronizationContext guiSynchronizationContext, INotificationService notificationService, PropertyChangedEventHandler guiCurrentWorkControllerPropertyChanged, ClientSettingsManager clientSettingsManager, ApplicationStartType startType)
		{
			Debug.Assert(guiCurrentWorkControllerPropertyChanged != null);
			Debug.Assert(instance == null);
			isStarting = true;
			StartType = startType;
			this.clientSettingsManager = clientSettingsManager;
			IdleDetector = new IdleDetector(calendarManager);
			defaultWorkIdSelector = new DefaultWorkIdSelector(guiSynchronizationContext);
			menuCoordinator = new MenuCoordinator(defaultWorkIdSelector, clientSettingsManager);
			statsCoordinator = new StatsCoordinator(guiSynchronizationContext);
			workItemManager = new WorkItemManager(menuCoordinator, statsCoordinator);
			guiThreadId = Thread.CurrentThread.ManagedThreadId;
			sleepRegulator = Platform.Factory.GetSleepRegulatorService();
			systemEvents = Platform.Factory.GetSystemEventsService();
			userActivity = Platform.Factory.GetUserActivityService();
			timeManager = new TimeManager(systemEvents);
			ruleManagement = Platform.Factory.GetRuleManagementService(guiSynchronizationContext, this);
			captureManager = new CaptureManager(userActivity, systemEvents);
			pluginCaptureService = Platform.Factory.GetPluginCaptureService();
			collector = new CollectorCoordinator(captureManager, workItemManager, pluginCaptureService);
			desktopCaptureManager = new DesktopCaptureManager(() => Platform.Factory.GetDesktopCaptureService(systemEvents, pluginCaptureService)); //we don't want DesktopCaptureManager to know about a dependency on systemEvents
			desktopCaptureManager.ThreadFrozen += HandleDesktopCaptureManagerFrozen;
			workAndLayoutGuiCoordinator = new WorkAndLayoutGuiCoordinator(captureManager);
			currentWorkController = new CurrentWorkController(workAndLayoutGuiCoordinator, null, notificationService, timeManager, this);
			currentWorkController.PropertyChanged += CurrentWorkControllerPropertyChangedBefore;
			currentWorkController.PropertyChanged += guiCurrentWorkControllerPropertyChanged; //ui update icon change etc.
			currentWorkController.PropertyChanged += CurrentWorkControllerPropertyChangedAfter;
			currentWorkController.MeetingCountDownService.ManualWorkItemCreated += ManualWorkItemCreated;
			workManagementService = Platform.Factory.GetWorkManagementService(currentWorkController, workItemManager);
			clientKickCoordinator = new ClientKickCoordinator(currentWorkController, guiSynchronizationContext, notificationService);
			systemEventsCoordinator = new SystemEventsCoordinator(systemEvents, currentWorkController, guiSynchronizationContext, userActivity);
			workStatusReporter = new WorkStatusReporter(captureManager);
			desktopCoordinator = new DesktopCaptureCoordinator(guiSynchronizationContext, this);
			meetingManager = new MeetingNotifier(guiSynchronizationContext, notificationService);
			interProcessManager = Platform.Factory.GetInterProcessManager(guiSynchronizationContext, currentWorkController, menuCoordinator);
			this.notificationService = notificationService;
			notificationCoordinator = new PendingNotificationCoordinator(notificationService, guiSynchronizationContext, this);
			context = guiSynchronizationContext;
#if DEBUG
			guiSynchronizationContext.Post(_ => Debug.Assert(Thread.CurrentThread.ManagedThreadId == guiThreadId), null);
#endif
			if (instance == null) instance = this;
		}

		private bool lastSetIsWorking;
		private bool CheckIfIsWorkingChanged(bool setValue, out bool isWorking)
		{
			isWorking = currentWorkController.CurrentWork != null;
			if (isWorking == lastSetIsWorking) return false;
			if (setValue)
			{
				lastSetIsWorking = isWorking;
			}
			return true;
		}

		private void HandleDesktopCaptureManagerFrozen(object sender, ThreadFrozenEventArgs e)
		{
			OnDesktopCaptureFrozen(e);
		}

		private void OnDesktopCaptureFrozen(ThreadFrozenEventArgs e)
		{
			var evt = DesktopCaptureFrozen;
			if (evt != null) evt(this, e);
		}

		private void CurrentWorkControllerPropertyChangedBefore(object sender, PropertyChangedEventArgs e)
		{
			DebugEx.EnsureGuiThread();
			if (e.PropertyName != "CurrentWork") return;
			bool isWorking;
			if (!CheckIfIsWorkingChanged(false, out isWorking)) return;
			if (isWorking) return;
			desktopCaptureManager.SetIsSending(false); //don't send desktop captures / screenshots when not working (stop before the icon is changed!)
			sleepRegulator.AllowSleep();
		}

		private int? lastWorkId;
		private void CurrentWorkControllerPropertyChangedAfter(object sender, PropertyChangedEventArgs e)
		{
			DebugEx.EnsureGuiThread();
			if (e.PropertyName == "IsRuleOverrideEnabled" && !currentWorkController.IsRuleOverrideEnabled) //if rule override is disabled
			{
				desktopCoordinator.ProcessNextCapture(); //notify desktopCoordinator that rules should be re-evaluated
			}
			if (e.PropertyName != "CurrentWork") return;
			var newWorkId = currentWorkController.CurrentWork == null ? null : currentWorkController.CurrentWork.Id;
			if (lastWorkId != newWorkId)
			{
				if (currentWorkController.IsChangeReasonWasUserSelectOrResume)
				{
					desktopCoordinator.ProcessNextCapture(); //notify desktopCoordinator that rules should be re-evaluated [user selected a new work than before (the rules might not like it)]
				}
			}
			lastWorkId = newWorkId;
			bool isWorking;
			if (!CheckIfIsWorkingChanged(true, out isWorking)) return;
			if (!isWorking) return;
			desktopCaptureManager.SetIsSending(true); //start sending desktop captures / screenshots when working (start after the icon is changed!)
			sleepRegulator.PreventSleep();
		}

		public void Start()
		{
			DebugEx.EnsureGuiThread();
			log.Debug("CaptureCoordinator Starting...");
			clientSettingsManager.LoadSettings();
			log.Debug("clientSettingsManager Loaded");
			clientSettingsManager.Start();
			log.Debug("clientSettingsManager Started");
			var menuLookup = menuCoordinator.LoadMenu();
			MenuQuery.Instance.ClientMenuLookup.Update(menuLookup);
			log.Debug("menuCoordinator Loaded Menu");
			currentWorkController.LoadLastWorkData(menuLookup);
			log.Debug("currentWorkController Loaded");
			CurrentMenuLookup = menuLookup;
			defaultWorkIdSelector.UpdateMenu(menuLookup);
			censorRulesManager.RulesChanged += CensorRulesManagerRulesChanged;
			censorRulesManager.LoadRules();
			censorRulesManager.Start();
			log.Debug("censorRulesManager Started");
			learningRuleManager.LearningRuleGeneratorsChanged += LearningRuleGeneratorsChanged;
			learningRuleManager.LoadRules();
			learningRuleManager.Start();
			log.Debug("learningRuleManager Started");
			meetingManager.Start();
			log.Debug("meetingManager Started");
			workIdAssigner.DataAssigned += WorkIdAssignerDataAssigned;
			log.Debug("workIdAssigner Started");
			workDetector.WorkIdsMissing += WorkDetectorWorkIdsMissing;
			workDetector.LoadSettings(menuLookup.ClientMenu, rule => ruleManagement.ShouldSkipLoadingUserRule(rule));
			RefreshPlugins();
			log.Debug("workDetector Loaded");
			workDetectorRulesManager.RulesChanged += WorkDetectorRulesManagerRulesChanged;
			workDetectorRulesManager.LoadRules();
			workDetectorRulesManager.Start();
			log.Debug("workDetectorRulesManager Started");
			userActivity.Start();
			log.Debug("userActivity Started");
			desktopCaptureManager.DesktopCaptured += HandleDesktopCaptured;
			desktopCaptureManager.Start();
			log.Debug("desktopCaptureManager Started");
			creditRunOutManager.StateChanged += CreditRunOutManagerOnStateChanged;
			creditRunOutManager.Start();
			log.Debug("creditRunOutManager Started");
			statsCoordinator.SimpleWorkTimeStatsCalculated += SimpleWorkTimeStatsCalculated;
			statsCoordinator.LoadData();
			statsCoordinator.Start();
			log.Debug("statsCoordinator Started");

			//workItemManager.ConnectionStatusChanged += WorkItemManagerConnectionStatusChanged;
			//workItemManager.CannotPersistAndSendWorkItem += WorkItemManagerCannotPersistAndSendWorkItem;
			workItemManager.Start(); //should be started before captureManager or anything that is sending data
			log.Debug("workItemManager Started");
			TelemetryCoordinator.Instance.Start(workItemManager, clientSettingsManager, Platform.Factory.GetEnvironmentInfoService());
			log.Debug("UsageStats Started");
			collector.RulesChanged += CollectorRulesChanged;
			collector.Start();
			log.Debug("collector Started");
			captureManager.WorkItemCreated += CaptureManagerWorkItemCreated;
			captureManager.Start();
			log.Debug("captureManager Started");

			versionReportManager.Start();
			log.Debug("versionReportManager Started");
			clientKickCoordinator.Start();
			log.Debug("clientKickCoordinator Started");
			endpointManager.Start();
			log.Debug("endpointManager Started");

			//don't change menu until now
			menuCoordinator.CurrentMenuChanged += MenuManagerCurrentMenuChanged;
			menuCoordinator.Start();
			log.Debug("menuManager Started");
			timeManager.ClockSkewError += ClockSkewError;
			timeManager.Start();
			log.Debug("timeManager Started");
			interProcessManager.Start();
			log.Debug("interProcessManager Started");
			notificationCoordinator.LoadNotifications();
			notificationCoordinator.Start();
			log.Debug("notificationCoordinator Started");
			workManagementService.MenuRefreshNeeded += MenuRefreshNeeded;
			workManagementService.UpdateMenu(CurrentMenuLookup); //todo research (LoadMenu should raise)
			workManagementService.Start();
			log.Debug("workManagementService Started");
			closeReasonsManager.CannedCloseReasonsChanged += CannedCloseReasonsChanged;
			closeReasonsManager.LoadData();
			closeReasonsManager.Start();
			log.Debug("closeReasonsManager Started");
			calendarManager.Start();
			log.Debug("calendarManager Started");
			clientSettingsManager.SettingsChanged += ClientSettingsManagerOnSettingsChanged;
			log.Debug("CaptureCoordinator Started");
			isStarting = false;
		}

		public void Stop()
		{
			DebugEx.EnsureGuiThread();
			log.Debug("CaptureCoordinator Stopping...");
			isStopping = true;
			desktopCoordinator.Stop(); //must be stopped before desktopCaptureManager to avoid deadlock !!!
			log.Debug("desktopCoordinator Stopped");
			captureManager.Stop();
			captureManager.WorkItemCreated -= CaptureManagerWorkItemCreated;
			log.Debug("captureManager Stopped");
			TelemetryCoordinator.Instance.Stop();
			log.Debug("UsageStats Stopped");
			collector.Stop();
			collector.RulesChanged -= CollectorRulesChanged;
			log.Debug("collector Stopped");
			censorRulesManager.Stop();
			censorRulesManager.RulesChanged -= CensorRulesManagerRulesChanged;
			log.Debug("censorRulesManager Stopped");
			notificationCoordinator.Stop();
			notificationCoordinator.Dispose();
			log.Debug("notificationCoordinator Stopped");
			learningRuleManager.Stop();
			learningRuleManager.LearningRuleGeneratorsChanged -= LearningRuleGeneratorsChanged;
			log.Debug("learningRuleManager Stopped");
			meetingManager.Stop();
			log.Debug("meetingManager Stopped");
			workDetectorRulesManager.Stop();
			workDetectorRulesManager.RulesChanged -= WorkDetectorRulesManagerRulesChanged;
			log.Debug("workDetectorRulesManager Stopped");
			desktopCaptureManager.Stop();
			desktopCaptureManager.DesktopCaptured -= HandleDesktopCaptured;
			log.Debug("desktopCaptureManager Stopped");
			menuCoordinator.Stop();
			menuCoordinator.Dispose();
			menuCoordinator.CurrentMenuChanged -= MenuManagerCurrentMenuChanged;
			log.Debug("menuCoordinator Stopped");
			workDetector.WorkIdsMissing -= WorkDetectorWorkIdsMissing;
			log.Debug("workDetector Stopped");
			workIdAssigner.DataAssigned -= WorkIdAssignerDataAssigned;
			workIdAssigner.Dispose();
			log.Debug("workIdAssigner Stopped");
			interProcessManager.Stop();
			log.Debug("interProcessManager Stopped");
			timeManager.Stop();
			timeManager.ClockSkewError -= ClockSkewError;
			timeManager.Dispose();
			log.Debug("timeManager Stopped");
			endpointManager.Stop();
			log.Debug("endpointManager Stopped");
			versionReportManager.Stop();
			log.Debug("versionReportManager Stopped");
			clientKickCoordinator.Stop();
			clientKickCoordinator.Dispose();
			log.Debug("clientKickCoordinator Stopped");
			clientSettingsManager.SettingsChanged -= ClientSettingsManagerOnSettingsChanged;
			clientSettingsManager.Stop();
			log.Debug("clientSettingsManager Stopped");

			workItemManager.Stop();
			log.Debug("workItemManager Stopped");
			creditRunOutManager.StateChanged -= CreditRunOutManagerOnStateChanged;
			creditRunOutManager.Stop();
			log.Debug("creditRunOutManager Stopped");
			statsCoordinator.Stop();
			statsCoordinator.SimpleWorkTimeStatsCalculated -= SimpleWorkTimeStatsCalculated;
			log.Debug("statsCoordinator Stopped");
			workStatusReporter.Dispose();
			log.Debug("workStatusReporter Stopped");
			//workItemManager.ConnectionStatusChanged -= WorkItemManagerConnectionStatusChanged;
			//workItemManager.CannotPersistAndSendWorkItem -= WorkItemManagerCannotPersistAndSendWorkItem;
			userActivity.Stop();
			log.Debug("userActivity Stopped");
			workManagementService.Stop();
			workManagementService.MenuRefreshNeeded -= MenuRefreshNeeded;
			workManagementService.Dispose();
			log.Debug("workManagementService Stopped");
			closeReasonsManager.Stop();
			closeReasonsManager.CannedCloseReasonsChanged -= CannedCloseReasonsChanged;
			log.Debug("calendarManager Stopped");
			calendarManager.Stop();
			log.Debug("closeReasonsManager Stopped");
			log.Debug("CaptureCoordinator Stopped");

			using (var c = new ActivityRecorderClientWrapper())
			{
				try
				{
					c.Client.Disconnect(ConfigManager.UserId, ConfigManager.EnvironmentInfo.ComputerId, string.IsNullOrEmpty(ConfigManager.EnvironmentInfo.OSFullName) ? ConfigManager.EnvironmentInfo.OSVersion.ToString() : ConfigManager.EnvironmentInfo.OSFullName + " (" + ConfigManager.EnvironmentInfo.OSVersion + ")");
				}
				catch (Exception exception)
				{
					log.Error("Disconnect from server failed", exception);
				}
			}
			log.Debug("Server disconnected");

			ActivityRecorderClientWrapper.Shutdown();
		}

		public void MutualWorktypeLoader()
		{
			// in the interest of postponed execution is posted on context
			currentWorkController.AdhocMeetingService.CheckPostponedMeetings(a => context.Post(_ => a(), null));
			currentWorkController.AdhocMeetingService.CheckUnfinishedOnGoingMeeting(a => context.Post(_ => a(), null));
			context.Post(_ =>
			{
				currentWorkController.MeetingCountDownService.CheckUnfinishedTimedTask();
			}, null);
		}

		public DesktopCapture GetDesktopCapture()
		{
			DebugEx.EnsureGuiThread();
			return desktopCaptureManager.GetDesktopCapture();
		}

		public WorkDetectorRule GetMatchingRule(DesktopCapture desktopCapture)
		{
			DebugEx.EnsureGuiThread();
			DesktopWindow _;
			AssignData __;
			DateTime? ___;
			var matchingRule = workDetector.DetectWork(desktopCapture, true, out _, out __, out ___, out var ____);
			return matchingRule != null ? matchingRule.OriginalRule : null;
		}

		public void RefreshMenuAsync()
		{
			ThreadPool.QueueUserWorkItem(_ => menuCoordinator.RefreshMenu()); //RefreshMenu can block if a refresh is in progress
		}

		internal void UpdateCollectedItem(DateTime dateTime, Dictionary<string, string> capturedValues)
		{
			collector.UpdateImmediate(dateTime, capturedValues);
		}

		private void WorkIdAssignerDataAssigned(object sender, SingleValueEventArgs<int> e)
		{
			RefreshMenuAsync();
		}

		private void MenuRefreshNeeded(object sender, EventArgs e)
		{
			RefreshMenuAsync();
		}

		private void CannedCloseReasonsChanged(object sender, SingleValueEventArgs<CannedCloseReasons> e)
		{
			workManagementService.SetCannedCloseReasons(e.Value);
		}

		private void SimpleWorkTimeStatsCalculated(object sender, SingleValueEventArgs<SimpleWorkTimeStats> e)
		{
			workManagementService.SetSimpleWorkTimeStats(e.Value);
		}

		private void WorkDetectorWorkIdsMissing(object sender, SingleValueEventArgs<int[]> e)
		{
			return;
			//todo This feature is under reconsideration
			//foreach (var workId in e.Value)
			//{
			//	workIdAssigner.AssignWorkIdAsync(workId);
			//}
		}

		private void MenuManagerCurrentMenuChanged(object sender, MenuEventArgs e)
		{
			DebugEx.EnsureBgThread();
			//menu change is marshalled to the GUI Thread
			desktopCoordinator.Change(() =>
				{
					using (MenuQuery.Instance.ClientMenuLookup.UpdateWithDeferredEvent(e.MenuLookup)) //by the time currentWorkController.MenuChanged is reached we should have the new menu in MenuQuery
					{
						CurrentMenuLookup = e.MenuLookup;
						workDetector.UpdateMenu(e.Menu);
						currentWorkController.MenuChanged(e.MenuLookup, e.OldMenu, workManagementService);
						interProcessManager.UpdateMenu(e.Menu);
						defaultWorkIdSelector.UpdateMenu(e.MenuLookup);
						statsCoordinator.UpdateMenu(e.MenuLookup);
						workManagementService.UpdateMenu(e.MenuLookup);
					}
					RaiseCurrentMenuChanged(e);
				});
		}

		private void ManualWorkItemCreated(object sender, SingleValueEventArgs<ManualWorkItem> e)
		{
			workItemManager.PersistAndSend(e.Value);
		}

		//there was a bugreport where this function never returned (which froze the CaptureWorkItemCallback in CaptureManager)
		private void CaptureManagerWorkItemCreated(object sender, WorkItemEventArgs e)
		{
			//invoked on BG or GUI Thread
			workItemManager.PersistAndSend(e.WorkItem);
			RaiseWorkItemCreated(e);
			//workTimeCounter.AddWorkItem(e.WorkItem);
			//idleDetector.AddWorkItem(e.WorkItem);
		}

		private void HandleDesktopCaptured(object sender, DesktopCapturedEventArgs e)
		{
			//capture - detect - censor - marshall and send it to captureManager (on the GUI thread)
			desktopCoordinator.ProcessOrDrop(e.DesktopCapture, e.ShouldSendCapture, e.IsWorking);
		}

		protected void OnDesktopCaptured(DesktopCapture e)
		{
			DebugEx.EnsureGuiThread();
			var evt = DesktopCaptured;
			if (evt != null) evt(this, new SingleValueEventArgs<DesktopCapture>(e));
		}

		private void CensorRulesManagerRulesChanged(object sender, SingleValueEventArgs<List<CensorRule>> e)
		{
			//invoked on BG Thread (and GUI when loading rules)
			//no need to use desktopCoordinator atm because effect of censor cannot be rolled back
			censor.SetCensorRules(e.Value);
			if (isStarting) return;
			desktopCoordinator.Change(NotificationRulesChanged);
		}

		private void LearningRuleGeneratorsChanged(object sender, SingleValueEventArgs<List<RuleGeneratorData>> e)
		{
			//invoked on BG Thread (and GUI when loading rules)
			//no need to use desktopCoordinator because we don't care when this is changed (and we can change it on bg thread)
			ruleManagement.SetLearningRuleGenerators(e.Value);
		}

		private void WorkDetectorRulesManagerRulesChanged(object sender, SingleValueEventArgs<List<WorkDetectorRule>> e)
		{
			//invoked on BG Thread (and GUI when loading rules)
			var isStartingTmp = isStarting; // to avoid changes
			desktopCoordinator.Change(() =>
				{
					workDetector.SetServerRules(e.Value);
					workDetector.SaveSettings();
					RefreshPlugins();
					hasAllMatchingRule = e.Value.Any(r => r.IsEnabled && r.IsRegex && r.TitleRule == ".*" && r.UrlRule == ".*" && r.ProcessRule == @"((?=.*\.).*)" && (r.ExtensionRules == null || !r.ExtensionRules.Any()));
					if (isStartingTmp) return;
					NotificationRulesChanged();
				});
		}

		public void RestartClientRulesManager()
		{
			workDetectorRulesManager.Stop();
			workDetectorRulesManager.Start();
		}

		public void ClearLearningRuleTimers()
		{
			workDetector.ClearLearningRuleTimers();
		}

		private void CollectorRulesChanged(object sender, EventArgs e)
		{
			if (isStarting) return;
			desktopCoordinator.Change(NotificationRulesChanged);
		}

		private readonly TimeSpan nfNotificationRulesChangedDuration = TimeSpan.FromSeconds(30);
		private void NotificationRulesChanged()
		{
			notificationService.ShowNotification(NotificationKeys.RulesChanged, nfNotificationRulesChangedDuration, Labels.NotificationRulesChangedTitle,
				Labels.NotificationRulesChangedText);
		}

		public void SetUserRules(List<WorkDetectorRule> newRules)
		{
			DebugEx.EnsureGuiThread(); // Doesn't really matter
			desktopCoordinator.Change(() =>
				{
					workDetector.SetUserRules(newRules);
					workDetector.SaveSettings(); //todo refactor SaveSettings (include into SetXXX)
					RefreshPlugins();
				});
		}

		public void AddUserRule(WorkDetectorRule rule)
		{
			DebugEx.EnsureGuiThread(); // Doesn't really matter
			desktopCoordinator.Change(() =>
			{
				workDetector.AddUserRule(rule);
				workDetector.SaveSettings(); //todo refactor SaveSettings (include into SetXXX)
				RefreshPlugins();
			});
		}

		public void RemoveUserRules(Func<WorkDetectorRule, bool> predicate)
		{
			DebugEx.EnsureGuiThread(); // Doesn't relly matter
			if (predicate == null) return;
			desktopCoordinator.Change(() =>
			{
				workDetector.RemoveUserRules(predicate);
			});
		}

		public List<WorkDetectorRule> GetUserRules()
		{
			DebugEx.EnsureGuiThread(); // Doesn't relly matter
			return workDetector.GetUserRules();
		}

		public bool GetIsWorkTime(DateTime utcNow)
		{
			if (!ConfigManager.AutoStartClientOnNonWorkDays && !calendarManager.IsWorkday(utcNow)) return false;
			var curr = TimeZone.CurrentTimeZone.ToLocalTime(utcNow).TimeOfDay;
			var start = TimeSpan.FromMinutes(ConfigManager.WorkTimeStartInMins);
			var end = TimeSpan.FromMinutes(ConfigManager.WorkTimeEndInMins);
			return start <= curr && curr <= end; //we don't consider end < start to be valid atm.
		}

		private void RefreshPlugins()
		{
			pluginCaptureService.LoadCaptureExtensionsFromWorkDetectorRulesAsync(
				workDetector.GetServerRules()
				.Concat(workDetector.GetUserRules())
				.Where(n => n.IsEnabled) //we only care about enabled rules
				.ToArray());
		}

		//We want to prevent workDetector detecting some works which are invalid with the current menu.
		//So we don't want to send rules with the new menu while currentWorkController only knows the old one.
		//(and vice versa we don't want to send rules based on the old menu while currentWorkController knows the new one)
		//So we have to sync somehow the menu change in workDetector and currentWorkController.
		//We achive this by invoking all changes on the gui thread. And if we have a pending change
		//(which was already posted to the gui but not yet executed) we wait with the processing until changes
		//invoked by the gui. [If the change arrives during the processing (but before posting) then we drop the current processing]
		private class DesktopCaptureCoordinator
		{
			private int version;
			private int waitForGuiChange;
			private bool aborted;
			private readonly object thisLock = new object();
			private readonly SynchronizationContext context;
			private readonly CaptureCoordinator parent;
			private readonly TimeSpan nfAssignWorkNotAllowedDuration = TimeSpan.FromSeconds(30);
			private readonly IRuleActionCoordinator ruleActionCoordinator;
			private DesktopCapture lastProcessedCapture;
			private int shouldPorcessNextCapture;
			private bool lastIsIdle;
			private DateTime? lastMatchValidUntil;

			public DesktopCaptureCoordinator(SynchronizationContext guiSynchronizationContext, CaptureCoordinator parent)
			{
				context = guiSynchronizationContext;
				ruleActionCoordinator = Platform.Factory.GetRuleActionCoordinator();
				this.parent = parent;
			}

			private int? previousActivityTime = null;

			//We need to process the capture if any of these are true:
			// - DesktopCapture is changed (from the viewpoint of WorkDetector! i.e. new rule might be choosen)
			// - shouldSendDesktopCapture is true
			// - User started to work (and not working before)
			// - User changed work (but rule might have to change it again)
			// - Rule Override is disabled
			// - WorkDetector rules changed
			// - WorkDetector rule validity changed
			// - Menu changed
			//(censor is kinda special because it is not necessary to marshall that to the gui atm.)
			public void ProcessOrDrop(DesktopCapture desktopCapture, bool shouldSendDesktopCapture, bool isWorking)
			{
				DebugEx.EnsureBgThread();
				if (desktopCapture == null) return;
				int currentVersion;
				DesktopCapture currentLastProcessedCapture; //local copy of lastProcessedCapture
				lock (thisLock)
				{
					while (waitForGuiChange != 0 && !aborted)
					{
						Monitor.Wait(thisLock); //this cannot be called on the GUI thread or we have a deadlock
					}
					if (aborted) return;

					currentLastProcessedCapture = lastProcessedCapture;
					currentVersion = version;
				}

				var lastUserActivityTime = parent.userActivity.GetLastActivity();
				// var elapsed = Environment.TickCount - lastUserActivity.Value;
				var elapsed = previousActivityTime.HasValue && lastUserActivityTime.HasValue ? lastUserActivityTime.Value - previousActivityTime.Value : lastUserActivityTime.HasValue ? Environment.TickCount - lastUserActivityTime.Value : 0;
				if (previousActivityTime.HasValue && lastUserActivityTime != null && 0 < elapsed)
					RaiseOnUserActivity(parent.userActivity.GetLastKeyboardActivityTime() < parent.userActivity.GetLastMouseActivityTime());
				previousActivityTime = lastUserActivityTime;
				var isIdle = lastUserActivityTime != null && (Environment.TickCount - lastUserActivityTime.Value >= 60000);
				if (Interlocked.Exchange(ref shouldPorcessNextCapture, 0) == 0
					&& !shouldSendDesktopCapture
					&& currentLastProcessedCapture != null
					&& (!lastMatchValidUntil.HasValue || lastMatchValidUntil.Value > DateTime.UtcNow)
					&& isIdle == lastIsIdle
					&& RuleMatcher.SameRuleApplies(desktopCapture, currentLastProcessedCapture)) //don't do this inside the lock (only one ProcessOrDrop can be executed at a time so it's fine)
				{
					return; //don't need to process
				}

				lastIsIdle = isIdle;
				AssignData assignData;
				DesktopWindow matchedWindow;
				IWorkChangingRule matchingRule;
				bool matchingRuleIsEnabledInNonWorkStatus;
				using (TelemetryHelper.MeasureElapsed(TelemetryHelper.KeyRuleEvaluationTimer))
				{
					matchingRule = parent.workDetector.DetectWork(desktopCapture, parent.currentWorkController.IsWorking, out matchedWindow, out assignData,
						out lastMatchValidUntil, out matchingRuleIsEnabledInNonWorkStatus);
				}
				var isCaptureChanged = parent.censor.CensorCaptureIfApplicable(desktopCapture);
				DesktopCaptureService.FilterCapture(desktopCapture, ConfigManager.ClientDataCollectionSettings);
				var collectorRuleDetectorResult = parent.collector.DetectCaptures(desktopCapture);
				if (shouldSendDesktopCapture)
				{
					isCaptureChanged |= DesktopCaptureService.RemoveAllWindowsWhichAreNotSentToServer(desktopCapture) != 0; //matchedWindow might not be in desktopCapture anymore
					DesktopCaptureService.EncodeImages(desktopCapture);
				}
				else isCaptureChanged = false;

				lock (thisLock)
				{
					if (aborted) return;
					if (version != currentVersion) return; //we've lost the race against a change so drop capture

					if (isWorking) //todo during meetings isWorking is false
					{
						log.DebugFormat("Processing capture: {0} [Rule: {1} ({2})]", desktopCapture, matchingRule, matchedWindow); //don't execute ToString() if level is not debug
					}
					else
					{
						log.VerboseFormat("Processing capture: {0} [Rule: {1} ({2})]", desktopCapture, matchingRule, matchedWindow); //don't expose dc-s in debug log when not working
					}
					lastProcessedCapture = isCaptureChanged ? null : desktopCapture; //if desktopCapture is censored or some windows are removed we should use null (i.e. force processing of the next capture because we don't clone)

					if (matchingRule != null
						&& !ruleActionCoordinator.ContainsRuleAction(matchingRule)
						&& matchingRule.RuleType == WorkChangingRuleType.DoNothing
						&& !parent.collector.IsEnabled
						&& !shouldSendDesktopCapture) //DoNothing for better maintainability of rules
					{
						//swallow change (don't do context switch for nothing)
						return;
					}
					context.Post(_ =>
					{
						if (parent.currentWorkController.IsShuttingDown) return;
						matchingRuleIsEnabledInNonWorkStatus = matchingRule != null
							&& matchingRuleIsEnabledInNonWorkStatus
							&& parent.GetIsWorkTime(DateTime.UtcNow) //we only enable non work status rules during worktime
							&& !isIdle //we don't enable if idling
							;
						var isWorkingOrSpecialRule = parent.currentWorkController.IsWorking || matchingRuleIsEnabledInNonWorkStatus;
						var canExecuteActions = isWorkingOrSpecialRule && !parent.currentWorkController.IsRuleOverrideEnabled;
						using (parent.collector.GetTimeStamper(collectorRuleDetectorResult))
						using (shouldSendDesktopCapture ? parent.workAndLayoutGuiCoordinator.SetDesktopCapture(desktopCapture) : null)
						using (GetDisposable(() => parent.OnDesktopCaptured(desktopCapture)))
						using (canExecuteActions ? ruleActionCoordinator.GetExecuter(matchingRule, assignData) : null)
						{
							parent.IdleDetector.ResetIdleWorkTimeIfNecessary(matchingRule);
							//meeting
							if (matchingRule != null && matchingRule.RuleType == WorkChangingRuleType.StopWork)
							{
								parent.currentWorkController.AdhocMeetingService.PauseWork();
							}
							else if (matchingRule != null && matchingRule.RuleType == WorkChangingRuleType.DoNothing)
							{
								//Do nothing
							}
							else
							{
								parent.currentWorkController.AdhocMeetingService.ResumeWork();
							}

							//computer
							if (matchingRule == null //no work detected
								|| matchingRule.RuleType == WorkChangingRuleType.EndEffect)
							{
								parent.currentWorkController.TempEndEffect();
							}
							else if (matchingRule.RuleType == WorkChangingRuleType.StopWork)
							{
								parent.currentWorkController.TempStopWork();
							}
							else if (parent.currentWorkController.IsRuleOverrideEnabled) //rules are overriden by user
							{
								Debug.Assert(parent.currentWorkController.CurrentWorkState != WorkState.WorkingTemp);
								parent.currentWorkController.TempEndEffect(); //end any temp effects (only TempStopWorks, TempStartWorks should not be active)
								return; //we don't want to do anything else if rules are overriden
							}
							else if (matchingRule.RuleType == WorkChangingRuleType.StartWork) //work detected
							{
								parent.currentWorkController.TempOrPermStartWork(new WorkData() { Id = matchingRule.RelatedId, Name = matchingRule.Name }, matchingRule.IsPermanent, matchingRuleIsEnabledInNonWorkStatus);
							}
							else if (matchingRule.RuleType == WorkChangingRuleType.StartCategory) //category detected
							{
								parent.currentWorkController.TempOrPermStartCategory(matchingRule.RelatedId, matchingRule.IsPermanent, matchingRuleIsEnabledInNonWorkStatus);
							}
							else if (matchingRule.RuleType == WorkChangingRuleType.StartOrAssignWork)
							{
								var isContextBased = matchingRule.OriginalRule.IsEnabledInProjectIds != null && matchingRule.OriginalRule.IsEnabledInProjectIds.Count != 0;
								if (isContextBased)
								{
									var currentProject = parent.currentWorkController.GetProjectForLastUserSelectedOrPermWork();
									var currentProjectId = currentProject != null ? currentProject.WorkData.ProjectId : null;
									if (currentProjectId == null
										|| !matchingRule.OriginalRule.IsEnabledInProjectIds.Contains(currentProjectId.Value))
									{
										log.Info("Rule '" + matchingRule.Name + "' not allowed in projectId: " + currentProjectId);
										parent.notificationService.ShowNotification(NotificationKeys.AssignWorkNotAllowed, nfAssignWorkNotAllowedDuration, Labels.NotificationAssignWorkNotAllowedTitle,
											String.Format(Labels.NotificationAssignWorkNotAllowedBody, matchingRule.Name));
										return;
									}
									if (assignData != null && assignData.Work != null && assignData.Work.WorkKey != null)
									{
										int workId;
										if (currentProject.WorkData.ExternalWorkIdMapping != null
											&& currentProject.WorkData.ExternalWorkIdMapping.TryGetValue(assignData.Work.WorkKey, out workId)) //todo case
										{
											//todo check if workId is assigned to us
											assignData.Work.WorkId = workId;
										}
										else //we need to assign work under this key
										{
											assignData.Work.ProjectId = currentProjectId;
										}
									}
								}
								if (assignData != null && assignData.Work != null && assignData.Work.WorkId.HasValue)
								{
									parent.currentWorkController.TempOrPermStartWork(new WorkData() { Id = assignData.Work.WorkId.Value, Name = matchingRule.Name }, matchingRule.IsPermanent, matchingRuleIsEnabledInNonWorkStatus);
								}
								else
								{
									//parent.currentWorkController.TempEndEffect(); //cannot start work immediately so end any temp effects
									if (assignData != null
										&& (parent.currentWorkController.CurrentWorkState != WorkState.NotWorking || matchingRuleIsEnabledInNonWorkStatus))
									{
										if (assignData.Work != null)
										{
											parent.menuCoordinator.AssignWorkAsync(assignData);
										}
									}
								}
							}
							else if (matchingRule.RuleType == WorkChangingRuleType.StartOrAssignProject)
							{
								if (assignData != null && assignData.Project != null && assignData.Project.WorkId.HasValue)
								{
									parent.currentWorkController.TempOrPermStartWork(new WorkData() { Id = assignData.Project.WorkId.Value, Name = matchingRule.Name }, matchingRule.IsPermanent, matchingRuleIsEnabledInNonWorkStatus);
								}
								else
								{
									//parent.currentWorkController.TempEndEffect(); //cannot start work immediately so end any temp effects
									if (assignData != null
										&& assignData.Project != null
										&& !assignData.Project.ProjectId.HasValue //if the project is already assigned but the WorkSelector couldn't find a valid work... we won't assign again (forever)
										&& (parent.currentWorkController.CurrentWorkState != WorkState.NotWorking || matchingRuleIsEnabledInNonWorkStatus))
									{
										parent.menuCoordinator.AssignWorkAsync(assignData);
									}
								}
							}
							else if (matchingRule.RuleType == WorkChangingRuleType.StartOrAssignProjectAndWork)
							{
								if (assignData != null && assignData.Composite != null && assignData.Composite.WorkId.HasValue)
								{
									parent.currentWorkController.TempOrPermStartWork(new WorkData() { Id = assignData.Composite.WorkId.Value, Name = matchingRule.Name }, matchingRule.IsPermanent, matchingRuleIsEnabledInNonWorkStatus);
								}
								else
								{
									//parent.currentWorkController.TempEndEffect(); //cannot start work immediately so end any temp effects
									if (assignData != null
										&& assignData.Composite != null
										&& (parent.currentWorkController.CurrentWorkState != WorkState.NotWorking || matchingRuleIsEnabledInNonWorkStatus))
									{
										parent.menuCoordinator.AssignWorkAsync(assignData);
									}
								}
							}
							else if (matchingRule.RuleType == WorkChangingRuleType.DoNothing)
							{
								//Do nothing
							}
							else
							{
								log.ErrorAndFail("Invalid rule type detected: " + matchingRule);
							}
						}
						if (parent.currentWorkController.IsRuleOverrideEnabled) return; //we don't want to do anything else if rules are overriden
						if (matchingRule != null
							&& matchingRule.IsLearning
							&& (parent.currentWorkController.CurrentWorkState != WorkState.NotWorking || matchingRuleIsEnabledInNonWorkStatus))
						{
							//todo should we do the matching on the BG thread (WorkDetector) ?
							//todo handle the race when the rule is created but the same detection is in the messageQueue already
							//todo remove legacy cancel handling
							parent.ruleManagement.DisplayLearnRuleFromCaptureGui(matchingRule, desktopCapture, matchedWindow);
						}
					}
					, null); //assume context.Post is FIFO queue so we don't need to check for changes on the GUI
				}
			}
			private void RaiseOnUserActivity(bool mouseAct)
			{
				var handler = parent.OnUserActivity;
				if (handler != null)
					handler(this, new SingleValueEventArgs<bool>(mouseAct));
			}
			public void Change(Action changeOperationOnGuiThread)
			{
				lock (thisLock)
				{
					if (aborted) return;
					++version;
					lastProcessedCapture = null; //invalidate last capture (so we have to process the next)
					++waitForGuiChange;
					context.Post(_ => //assumes Post won't throw otherwise waitForGuiChange won't reach 0 again... (if it throws user will see it...)
					{
						try
						{
							if (!parent.currentWorkController.IsShuttingDown)
							{
								changeOperationOnGuiThread();
							}
						}
						catch (Exception ex)
						{
							log.ErrorAndFail("Unexpected error in changeOperationOnGuiThread", ex);
							throw;
						}
						finally
						{
							lock (thisLock)
							{
								if (--waitForGuiChange == 0 && !aborted) //we don't have to pulse again when aborted
								{
									Monitor.PulseAll(thisLock);
								}
							}
						}
					}, null);
				}
			}

			//there is no race here (so no need to increment version), but we might start to work when a TempStop rule is active,
			//(which is not prevented atm.) so we have to re-evaluate rules after starting to work
			//it is not prevented to change work while a rule is active, so re-evaluate after change
			//we also need to process after rule override is disabled
			//invoked on the GUI Thread
			public void ProcessNextCapture()
			{
				Interlocked.Exchange(ref shouldPorcessNextCapture, 1);
			}

			public void Stop()
			{
				lock (thisLock)
				{
					aborted = true;
					Monitor.PulseAll(thisLock);
				}
			}

			private IDisposable GetDisposable(Action actionOnDispose)
			{
				return new InvokeOnDispose(actionOnDispose);
			}

			private class InvokeOnDispose : IDisposable
			{
				private Action onDispose;

				public InvokeOnDispose(Action onDispose)
				{
					this.onDispose = onDispose;
				}

				public void Dispose()
				{
					onDispose();
				}
			}
		}

		private void CreditRunOutManagerOnStateChanged(object sender, SingleValueEventArgs<CreditRunOutState> e)
		{
			workItemManager.SendingBlocked = e.Value != CreditRunOutState.Settled;
			meetingManager.SendingBlocked = e.Value != CreditRunOutState.Settled;
			context.Post(_ =>
				{
					currentWorkController.MutualWorkTypeCoordinator.CreditRunOutState = e.Value;
					currentWorkController.MutualWorkTypeCoordinator.CreditRunOutRemainingDays = creditRunOutManager.RemainingDays ?? 0;
					if (e.Value == CreditRunOutState.RunOut && currentWorkController.MutualWorkTypeCoordinator.IsWorking)
						currentWorkController.MutualWorkTypeCoordinator.RequestStopWork(true, "No Credit");
					currentWorkController.MutualWorkTypeCoordinator.ShowCreditRunOutNotification();
				}, null);
		}

		private void ClientSettingsManagerOnSettingsChanged(object sender, SingleValueEventArgs<ClientSetting> singleValueEventArgs)
		{
			if (singleValueEventArgs.Value.IsGoogleCalendarTrackingEnabled ?? false)
			{
				if (!Google.GoogleCredentialManager.IsCredentialInitializationNeeded()) return;
				Google.GoogleCredentialManager.GetNewCredentialsIfNeeded(true, false);
			}
		}

		private static readonly TimeSpan nfClockSkewDuration = TimeSpan.FromSeconds(30);
		private void ClockSkewError(object sender, SingleValueEventArgs<ClockSkewData> e)
		{
			context.Post(skewData =>
			{
				if (currentWorkController.IsShuttingDown) return;
				var clockSkewData = (ClockSkewData)skewData;
				notificationService.ShowNotification(NotificationKeys.ClockSkew, nfClockSkewDuration, Labels.NotificationClockSkewErrorTitle,
					String.Format(Labels.NotificationClockSkewErrorBody, clockSkewData.ClientTime, clockSkewData.ServerTime));
			}, e.Value);
		}

		private void RaiseWorkItemCreated(WorkItemEventArgs e)
		{
			EventHandler<WorkItemEventArgs> created = WorkItemCreated;
			if (created == null) return;
			try
			{
				created(this, e);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in RaiseWorkItemCreated", ex);
			}
		}

		private void RaiseCurrentMenuChanged(MenuEventArgs e)
		{
			EventHandler<MenuEventArgs> updated = CurrentMenuChanged;
			if (updated == null) return;
			try
			{
				updated(this, e);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in RaiseCurrentMenuChanged", ex);
			}
		}

		private bool isDisposed;

		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			log.Debug("Disposing ruleManagement");
			using (ruleManagement) { }
			log.Debug("Disposing desktopCaptureManager");
			using (desktopCaptureManager) { } //desktopCaptureManager should be disposed before systemEvents
			log.Debug("Disposing systemEventsCoordinator");
			using (systemEventsCoordinator) { } //systemEventsCoordinator should be disposed before systemEvents
			log.Debug("Disposing systemEvents");
			using (systemEvents) { }
			log.Debug("Disposing userActivity");
			using (userActivity) { }
			log.Debug("Disposing pluginCaptureService");
			using (pluginCaptureService) { } //pluginCaptureService should be disposed after desktopCaptureManager
			log.Debug("Disposing collector");
			using (collector) { }
			log.Info("CaptureCoordinator Disposed");
		}

		public bool HasRejectedWorkItem
		{
			get
			{
				if (workItemManager.UnsentItemsCount > 0 && workItemManager.OfflineReason > WorkItemManager.OfflineReasonEnum.NotOffline)
				{
					return true;
				}
				workItemManager.OfflineReason = WorkItemManager.OfflineReasonEnum.NotOffline;
				
				return false;
			}
		}
	}
}
