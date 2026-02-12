using ActivityRecorderClientAV;
using AppKit;
using Avalonia;
using Avalonia.Input;
using Avalonia.Platform;
using CoreAnimation;
using log4net;
using System.ComponentModel;
using System.Diagnostics;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Hotkeys;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Telemetry;

namespace Tct.ActivityRecorderClient
{
	public class MacInit
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private NSStatusItem statusItem;
		private CaptureCoordinator captureCoordinator;
		private SynchronizationContext context;
		private HotkeyRegistrar hotkeyRegistrar;

		private bool stopping;

		public void MainWindowReady()
		{
			context = new NSRunLoopSynchronizationContext();
			((PlatformMac.PlatformFactory)Platform.Factory).GuiSynchronizationContext = context;
			captureCoordinator = new CaptureCoordinator(
							Platform.Factory.GetGuiSynchronizationContext(),
							Platform.Factory.GetNotificationService(),
							(_, _) => { },
							new ClientSettingsManager(),
							ApplicationStartType.Normal
						);
			App.MainWindow.SetCaptureCoordinator(captureCoordinator);
			captureCoordinator.Start();

			LoadIcons();

			CreateTrayIcon();

			App.MainWindow.Opened += (_, __) => Highlight(true);
			App.MainWindow.Activated += (_, __) => Highlight(true);

			App.MainWindow.Closed += (_, __) => Highlight(false);
			App.MainWindow.Deactivated += (_, __) => Highlight(false);
			if (App.MainWindow.IsVisible)
			{
				Highlight(true);
			}

			App.MainWindow.Closing += (_, __) => { stopping = true; };

			captureCoordinator.CurrentWorkController.PropertyChanged += CurrentWorkController_PropertyChanged;
			captureCoordinator.WorkItemManager.ConnectionStatusChanged += WorkItemManagerConnectionStatusChanged;
			captureCoordinator.WorkItemManager.CannotPersistAndSendWorkItem += WorkItemManagerCannotPersistAndSendWorkItem;
			hotkeyRegistrar = new HotkeyRegistrar(Platform.Factory.GetHotkeyService());
			hotkeyRegistrar.HotkeyPressed += HandleHotkey;
			hotkeyRegistrar.LoadSettings();
			App.MainWindow.HotkeyRegistrar = hotkeyRegistrar;
			if (ConfigManager.LocalSettingsForUser.ManualMeetingHotKey.HasValue && ConfigManager.LocalSettingsForUser.ManualMeetingHotKey.Value != Keys.None)
			{
				var hks = MigrateLegacyHotkeyForMac(ConfigManager.LocalSettingsForUser.ManualMeetingHotKey.Value, HotkeyActionType.StartManualMeeting);
				ConfigManager.LocalSettingsForUser.ManualMeetingHotKey = Keys.None;
			}
		}

		private HotkeySetting? MigrateLegacyHotkeyForMac(Keys legacyHotkey, HotkeyActionType actionType)
		{
			Keys keyCode = legacyHotkey & Keys.KeyCode;
			bool shift = (legacyHotkey & Keys.Shift) == Keys.Shift;
			bool ctrl = false;
			bool alt = (legacyHotkey & Keys.Alt) == Keys.Alt;
			bool win = (legacyHotkey & Keys.Control) == Keys.Control;

			var hks = new HotkeySetting()
			{
				KeyCode = keyCode,
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
				hotkeyRegistrar.SetHotkeys(hotkeySettings);
				log.InfoFormat("Legacy Hotkey has been migrated. ({0})", hks);
				return hks;
			}

			log.InfoFormat("Legacy Hotkey migration skipped. ({0})", hks);
			return null;
		}

		// TODO: mac, split platform agnostic and dependent things
		public void MainWindowExiting()
		{
			hotkeyRegistrar.Dispose();
			log.Debug("hotkeyRegistrar Disposed");
			hotkeyRegistrar.HotkeyPressed -= HandleHotkey;
			captureCoordinator.Stop();
			captureCoordinator.CurrentWorkController.PropertyChanged -= CurrentWorkController_PropertyChanged;
			captureCoordinator.WorkItemManager.ConnectionStatusChanged -= WorkItemManagerConnectionStatusChanged;
			captureCoordinator.WorkItemManager.CannotPersistAndSendWorkItem -= WorkItemManagerCannotPersistAndSendWorkItem;
			captureCoordinator.Dispose();
			log.Debug("captureCoordinator Stopped");
			Platform.Factory.GetHotkeyService().Dispose();
		}

		private void HandleHotkey(object? sender, SingleValueEventArgs<HotkeySetting> e)
		{
			var notificationService = Platform.Factory.GetNotificationService();
			var currentWorkController = captureCoordinator.CurrentWorkController;
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
					{
						ShowAddMeetingWorkForm(null, workId);   // create manual meeting
					}
					else
					{
						notificationService.ShowMessageBox(Labels.AddMeeting_DisabledManualMeetingWarningBody, Labels.AddMeeting_DisabledManualMeetingWarningTitle, Forms.MessageBoxButtons.OK);
					}
					break;
				case HotkeyActionType.ToggleMenu:
					//TelemetryHelper.RecordFeature("Hotkey", "ToggleMenu");
					//if (!contextMenuForm.Visible)
					//{
					//	ShowContextMenu(lastContextPosition ?? Cursor.Position);
					//}
					//else
					//{
					//	contextMenuForm.Hide();
					//}
					break;
				case HotkeyActionType.NewWorkDetectorRule:
					//TelemetryHelper.RecordFeature("Hotkey", "NewWorkDetector");
					//captureCoordinator.RuleManagementService.DisplayWorkDetectorRulesEditingGui(true);
					break;
				case HotkeyActionType.DeleteCurrentWorkDetectorRule:
					//TelemetryHelper.RecordFeature("Hotkey", "DeleteCurrentWorkDetector");
					//captureCoordinator.RuleManagementService.DisplayWorkDetectorRuleDeletingGui();
					break;
				case HotkeyActionType.JobCTRL_com:
					//TelemetryHelper.RecordFeature("Hotkey", "Web");
					//RecentUrlQuery.Instance.OpenLink(e.Value.Website);
					break;
				case HotkeyActionType.CreateWork:
					//TelemetryHelper.RecordFeature("Hotkey", "CreateWork");
					//DisplayCreateWorkGui();
					break;
				case HotkeyActionType.AddReason:
					//TelemetryHelper.RecordFeature("Hotkey", "AddReason");
					//if (currentWorkController.CurrentWork != null && currentWorkController.CurrentWork.Id.HasValue)
					//	workManagementService.DisplayReasonWorkGui(currentWorkController.CurrentWork);
					break;
				case HotkeyActionType.TodoList:
					//TelemetryHelper.RecordFeature("Hotkey", "TodoList");
					//todoManager.ShowTodoList();
					break;
				case HotkeyActionType.ClearAutoRuleTimer:
					//TelemetryHelper.RecordFeature("Hotkey", "ClearAutoRuleTimer");
					//captureCoordinator.ClearLearningRuleTimers();
					break;
				case HotkeyActionType.WorkTimeHistory:
					//HandleWorkTimeService.ShowModification();
					break;
			}
		}

		private void ShowAddMeetingWorkForm(int? idleMins = null, int? workId = null)
		{
			captureCoordinator.CurrentWorkController.AdhocMeetingService.StartWork(idleMins, workId);
		}

		private void ResumeOrStopWork()
		{
			var currentWorkController = captureCoordinator.CurrentWorkController;
			if (currentWorkController.CurrentWorkState != WorkState.NotWorking)
			{
				currentWorkController.UserStopWork();
				//RestartIfNewVersionInstalled(false);
			}
			else
			{
				currentWorkController.UserResumeWork();
			}
		}

		private void WorkItemManagerConnectionStatusChanged(object? sender, EventArgs e)
		{
			var isOnline = captureCoordinator.WorkItemManager.IsOnline;
			context.Post(_ =>
			{
				if (stopping) return;
				captureCoordinator.CurrentWorkController.IsOnline = isOnline;
			}, null);
		}

		private void WorkItemManagerCannotPersistAndSendWorkItem(object? sender, EventArgs e)
		{
			context.Post(_ =>
				Platform.Factory.GetNotificationService().ShowNotification(NotificationKeys.PersistAndSendError, TimeSpan.Zero,
					Labels.NotificationPersistAndSendErrorTitle, Labels.NotificationPesistAndSendErrorBody)
			, null);
		}

		private void CurrentWorkController_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(CurrentWorkController.CurrentWork)
				|| e.PropertyName == nameof(CurrentWorkController.IsOnline)
				|| e.PropertyName == nameof(CurrentWorkController.IsRuleOverrideEnabled))
			{
				SetWorkstateIcon();
			}
		}

		public void SetWorkstateIcon()
		{
			var currentWorkController = captureCoordinator.CurrentWorkController;
			if (currentWorkController.CurrentWork != null)
			{
				statusItem.Button!.Image =
					currentWorkController.IsOnline
						? currentWorkController.IsRuleOverrideEnabled
							? IconByName[IconName.WorkingLockOnline]
							: IconByName[IconName.WorkingOnline]
						: currentWorkController.IsRuleOverrideEnabled
							? IconByName[IconName.WorkingLockOffline]
							: IconByName[IconName.WorkingOffline];
				statusItem.Button!.ToolTip = "JobCTRL - Working";
			}
			else
			{
				statusItem.Button!.Image = IconByName[IconName.NotWorking];
				statusItem.Button!.ToolTip = "JobCTRL - Idle";
			}
		}

		private enum IconName
		{
			NotWorking,
			WorkingOnline,
			WorkingOffline,
			WorkingLockOnline,
			WorkingLockOffline,
		}

		private readonly Dictionary<IconName, NSImage> IconByName = new();

		private void LoadIcons()
		{
			foreach (var value in Enum.GetValues(typeof(IconName)))
			{
				var iconPath = value + ".ico";
				NSImage image = LoadFromAvalonia(iconPath);
				IconByName[(IconName)value] = image;
			}
		}

		private void CreateTrayIcon()
		{
			statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);

			statusItem.Button.Activated += Button_Activated;
			SetWorkstateIcon();
		}

		private void Button_Activated(object? sender, EventArgs e)
		{
			var window = App.MainWindow;
			if (window.IsActive)
			{
				window.Hide();

				//TODO: mac, this doesn't work and now the highlighting is not perfect....
				//NSApplication.SharedApplication.Hide(sender);
				//NSApplication.SharedApplication.Hide(NSApplication.SharedApplication);
			}
			else
			{
				var buttonWindow = statusItem.Button.Window;
				var frame = buttonWindow.Frame;
				var screen = buttonWindow.Screen;
				var visible = screen.VisibleFrame;

				double x = frame.X + frame.Width / 2 - window.Width / 2;
				double y = (screen.Frame.Y + screen.Frame.Height) - frame.Y;
				x = Math.Max(visible.X, Math.Min(x, visible.X + visible.Width - window.Width));
				y = Math.Min((screen.Frame.Y + screen.Frame.Height) - visible.Y - window.Height, y);

				window.Position = new PixelPoint((int)x, (int)y);
				window.Show();
				window.Activate();

				/*if (OperatingSystem.IsOSPlatformVersionAtLeast("macOS", 14))
				{
					NSApplication.SharedApplication.Activate();
				}
				else
				{
					NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
				}*/
			}
		}

		private static NSImage LoadFromAvalonia(string iconPath)
		{
			var uri = new Uri(AppResourcesAV.BaseIconPath + iconPath);
			using var stream = AssetLoader.Open(uri);
			var data = NSData.FromStream(stream);
			var img = new NSImage(data);
			img.Size = new CGSize(18, 18);
			return img;
		}

		private void Highlight(bool value)
		{
			statusItem.Button.Highlight(value);
		}
	}
}
