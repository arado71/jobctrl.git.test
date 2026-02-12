using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using log4net;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Avalonia.UI.ViewModels;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Extra;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Hotkeys;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.WorktimeHistory;
using ApplicationAV = Avalonia.Application;
using ButtonAV = Avalonia.Controls.Button;
using ColorAV = Avalonia.Media.Color;
using ContextMenuAV = Avalonia.Controls.ContextMenu;
using TextBlockAV = Avalonia.Controls.TextBlock;
using ToolTipAV = Avalonia.Controls.ToolTip;
using WindowAV = Avalonia.Controls.Window;

namespace ActivityRecorderClientAV
{
	public partial class ActivityRecorderFormAV : WindowAV
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public bool fullworktimestats = false;
		private PreferencesFormAV? settingsWindow;
		private TaskNavigatorViewModel viewModel;
		private readonly DispatcherTimer workTimeStatsTimer;

		public HotkeyRegistrar HotkeyRegistrar { get; set; }

		public ActivityRecorderFormAV()
		{
			InitializeComponent();

			viewModel = new TaskNavigatorViewModel(); // Initialize the ViewModel
			this.DataContext = viewModel; // Set the DataContext for the entire form

			workTimeStatsTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(30)
			};
			workTimeStatsTimer.Tick += OnTimerTick;

			AddToolTips();

			if (!Design.IsDesignMode)
			{
				viewModel.UserName = ConfigManager.IsAnonymModeEnabled ? ConfigManager.AppNameOverride ?? ConfigManager.ApplicationName : ConfigManager.UserName;
				viewModel.UserId = ConfigManager.IsAnonymModeEnabled ? "" : $"({ConfigManager.UserId})";
			}
			else
			{
				viewModel.UserName = "Gipsz Jakab";
				viewModel.UserId = "(13)";
			}

			RowVisible("RowQuarterly", fullworktimestats);
			SeparatorVisible("Separator3", fullworktimestats);
			RowVisible("RowYearly", fullworktimestats);
			SeparatorVisible("Separator4", fullworktimestats);

			this.Activated += OnActivated;
			this.Deactivated += OnDeactivated; // Hide MainWindow when clicked elsewhere on the desktop
			this.Closing += OnMainWindowClosing;
			this.Closed += OnMainWindowClosed;

			this.SearchField.ItemFilter = (search, item) =>
			{
				if (item is not TaskFlattenedViewModel taskItem)
				{
					return false;
				}

				if (string.IsNullOrWhiteSpace(search))
				{
					return true;
				}
				var workName = RemoveDiacritics(taskItem.FullName);
				var searchParts = search.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(RemoveDiacritics);
				return searchParts.All(part => workName.Contains(part, StringComparison.OrdinalIgnoreCase));
			};
		}

		static string RemoveDiacritics(string src)
		{
			string stFormD = src.Normalize(NormalizationForm.FormD);
			var sb = new StringBuilder();

			for (int ich = 0; ich < stFormD.Length; ich++)
			{
				UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
				if (uc != UnicodeCategory.NonSpacingMark)
				{
					sb.Append(stFormD[ich]);
				}
			}

			return (sb.ToString().Normalize(NormalizationForm.FormC));
		}

		private void OnTimerTick(object? sender, EventArgs e)
		{
			if (this.IsActive)
			{
				UpdateWorkTimeAndTaskbarInfoNew();
			}
			if (IdleDetector.IsIdleAfterWorkTime)
			{
				log.Info("Idling and working after work time so stop working");
				if (CurrentWorkController.CurrentWorkState != WorkState.NotWorking)
				{
					CurrentWorkController.UserStopWork();
				}
				ShowIdleMessage();
			}
			if (IdleDetector.IsIdleDuringWorkTime)
			{
				log.Info("Idling and working during work time so stop working");
				if (CurrentWorkController.CurrentWorkState != WorkState.NotWorking)
				{
					if (OfflineWorkIsAllowed)
					{
						ShowAddMeetingWorkForm(ConfigManager.DuringWorkTimeIdleInMins);         // create new one
						TelemetryHelper.RecordFeature("Meeting", "StartAdhoc");
					}
					else
					{
						CurrentWorkController.UserStopWork();
					}
				}
				ShowIdleMessage();
			}
		}

		private void ShowIdleMessage()
		{
			NotificationService.ShowNotification(NotificationKeys.IdleStopWork, TimeSpan.Zero,
												 Labels.NotificationWorkIdleTitle, Labels.NotificationWorkIdleBody,
												 CurrentWorkController.NotWorkingColor);
		}

		private void ShowAddMeetingWorkForm(int? idleMins = null, int? workId = null)
		{
			CurrentWorkController.AdhocMeetingService.StartWork(idleMins, workId);
		}

		public void SetWorkstateIcon()
		{
			if (CurrentWorkController?.CurrentWork != null)
			{
				AppResourcesAV.SetIcon(this, "SvgWorkstateOnIcon", "WorkstateIcon", AppResourcesAV.IsLightTheme());
			}
			else
			{
				AppResourcesAV.SetIcon(this, "SvgWorkstateOffIcon", "WorkstateIcon", AppResourcesAV.IsLightTheme());
			}
		}

		private void AddToolTips()
		{
			// Ensure existing ToolTips are removed
			RemoveToolTips();

			ToolTipAV.SetTip(WorkstateButton, CreateToolTip("Switch Work State"));
			ToolTipAV.SetTip(GlobeButton, CreateToolTip("JobCTRL.com My JobCTRL"));
			ToolTipAV.SetTip(SettingsButton, CreateToolTip("Open Preferences"));
			ToolTipAV.SetTip(MoreButton, CreateToolTip("Show More Options"));
			ToolTipAV.SetTip(ExitButton, CreateToolTip("Exit"));
		}

		private LayoutTransformControl CreateToolTip(string text)
		{
			return new LayoutTransformControl
			{
				LayoutTransform = new ScaleTransform(ScaleHelperAV.GlobalWindowScale, ScaleHelperAV.GlobalWindowScale),
				Child = new TextBlockAV
				{
					Text = text,
				}
			};
		}

		private void RemoveToolTips()
		{
			ToolTipAV.SetTip(WorkstateButton, null);
			ToolTipAV.SetTip(GlobeButton, null);
			ToolTipAV.SetTip(SettingsButton, null);
			ToolTipAV.SetTip(MoreButton, null);
			ToolTipAV.SetTip(ExitButton, null);
		}

		private void RowVisible(string rowName, bool visibility)
		{
			var row = this.FindControl<Grid>(rowName);
			if (row != null)
			{
				row.IsVisible = visibility;
			}
		}

		private void SeparatorVisible(string separatorName, bool visibility)
		{
			var separator = this.FindControl<Separator>(separatorName);
			if (separator != null)
			{
				separator.IsVisible = visibility;
			}
		}

		public void OnMoreClick(object? sender, RoutedEventArgs e)
		{
			// Create the ContextMenu
			var contextMenu = new ContextMenuAV();

			// List of menu option headers and click event handlers
			var menuItemsData = new (string Header, EventHandler<RoutedEventArgs> ClickEvent)[]
			{
				("Logout", SwitchUser_Click),
			};

			// Create and add menu items
			foreach (var (header, clickEvent) in menuItemsData)
			{
				contextMenu.Items.Add(CreateMenuItem(header, clickEvent));
			}

			var currentTheme = ApplicationAV.Current?.ActualThemeVariant;
			var lightBrush = (SolidColorBrush)ApplicationAV.Current!.Resources["LightBackground"]!;
			//var halfTransparentLightBrush = new SolidColorBrush(ColorAV.FromArgb((byte)((510 + lightBrush.Color.A)/3), lightBrush.Color.R, lightBrush.Color.G, lightBrush.Color.B));
			byte newLightTransparency = (byte)((765 + (255 - (255 - lightBrush.Color.A) / 2)) / 4);
			var halfTransparentLightBrush = new SolidColorBrush(ColorAV.FromArgb(newLightTransparency, lightBrush.Color.R, lightBrush.Color.G, lightBrush.Color.B));

			var darkBrush = (SolidColorBrush)ApplicationAV.Current!.Resources["DarkBackground"]!;
			//var halfTransparentDarkBrush = new SolidColorBrush(ColorAV.FromArgb((byte)((510 + darkBrush.Color.A)/3), darkBrush.Color.R, darkBrush.Color.G, darkBrush.Color.B));
			byte newDarkTransparency = (byte)((765 + (255 - (255 - darkBrush.Color.A) / 2)) / 4);
			var halfTransparentDarkBrush = new SolidColorBrush(ColorAV.FromArgb(newDarkTransparency, darkBrush.Color.R, darkBrush.Color.G, darkBrush.Color.B));


			if (currentTheme == Avalonia.Styling.ThemeVariant.Light) contextMenu.Background = halfTransparentLightBrush;
			else contextMenu.Background = halfTransparentDarkBrush;

			contextMenu.Open(MoreButton);
		}

		private static MenuItem CreateMenuItem(string header, EventHandler<RoutedEventArgs> clickEvent)
		{
			var menuItem = new MenuItem
			{
				Header = header,
			};

			menuItem.Click += clickEvent;

			return menuItem;
		}

		private void SwitchUser_Click(object? sender, RoutedEventArgs e)
		{
			var result = NotificationService.ShowMessageBox(Labels.ConfirmChangeUserBody, Labels.ConfirmChangeUserTitle, MessageBoxButtons.OKCancel);
			if (result != DialogResult.OK) return;
			log.Info("Change userId confirmed");
			App.LogoutOnExit = true;
			// TODO: mac, start after exit
			StartExiting();
		}

		private void OnDeactivated(object? sender, EventArgs e)
		{
			this.Hide();
		}

		private void OnActivated(object? sender, EventArgs e)
		{
			UpdateWorkTimeAndTaskbarInfoNew();
		}

		private void OnGlobeClick(object sender, RoutedEventArgs e)
		{
			RecentUrlQuery.Instance.OpenLink();
		}

		private void OnSettingsClick(object sender, RoutedEventArgs? e)
		{
			if (settingsWindow != null && settingsWindow.IsVisible)
			{
				settingsWindow.Topmost = true; // This works better than .Activate() on Windows

				if (settingsWindow.WindowState == WindowState.Minimized)
				{
					settingsWindow.WindowState = WindowState.Normal;
				}
				settingsWindow.Activate();
				settingsWindow.Topmost = false; // This works better than .Activate() on Windows
			}
			else
			{
				settingsWindow = new PreferencesFormAV();
				settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				settingsWindow.Show();
			}
		}

		private void OnHomeClick(object sender, RoutedEventArgs e)
		{
			viewModel.GoToHome();
		}

		private void OnUpClick(object sender, RoutedEventArgs e)
		{
			viewModel.GoToParent();
		}

		private void OnAddTaskClick(object sender, RoutedEventArgs e)
		{
		}

		private void TaskItemDisplay_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
		{
			if (sender is StyledElement clickedDisplay)
			{
				if (clickedDisplay.DataContext is TaskViewModel clickedTaskItem)
				{
					viewModel.SelectTask(clickedTaskItem);
					if (clickedTaskItem.WorkId.HasValue)
					{
						SelectedTaskDisplay.DataContext = clickedTaskItem;
					}

					Debug.WriteLine($"Clicked on: {clickedTaskItem.Name}");
				}
			}
		}

		private async void OnExitClick(object sender, RoutedEventArgs e)
		{
			StartExiting();
		}

		private void StartExiting()
		{
			if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.Shutdown(0);
			}
			Environment.Exit(0);
		}

		public void Exit()
		{
			workTimeStatsTimer.Stop();
			WorkTimeStatsFromWebsiteManager?.Dispose();
		}

		private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
		{
			Debug.WriteLine("MainWindow OnMainWindowClosing()");

			if (ApplicationAV.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime _)
			{
				foreach (var window in App.OpenNotificationWindows.ToList())
				{
					window.Close();
				}
			}
		}

		private void OnMainWindowClosed(object? sender, EventArgs e)
		{
			Debug.WriteLine("MainWindow OnMainWindowClosed()");
		}

		private void OnWorkstateClick(object sender, RoutedEventArgs e)
		{
			if (CurrentWorkController.IsWorking)
			{
				CurrentWorkController.UserStopWork();
			}
			else
			{
				CurrentWorkController.UserResumeWork();
			}
		}


		public CurrentWorkController CurrentWorkController { get; private set; }
		private WorkTimeCounter WorkTimeCounter { get; } = new WorkTimeCounter();
		private WorkTimeStatsFromWebsiteManager WorkTimeStatsFromWebsiteManager { get; set; }
		public CaptureCoordinator CaptureCoordinator { get; private set; }
		private IdleDetector IdleDetector => CaptureCoordinator.IdleDetector;
		private static bool OfflineWorkIsAllowed { get { return ConfigManager.DuringWorkTimeIdleManualInterval >= 0; } }
		private INotificationService _notificationService;
		private INotificationService NotificationService { get => _notificationService ??= Platform.Factory.GetNotificationService(); }

		public void SetCaptureCoordinator(CaptureCoordinator captureCoordinator)
		{
			CaptureCoordinator = captureCoordinator;
			CurrentWorkController = captureCoordinator.CurrentWorkController;
			viewModel.SetCaptureCoordinator(captureCoordinator);
			CurrentWorkController.PropertyChanged += OnPropertyChanged;

			var workTimeHistory = new WcfWorkTimeQuery();
			WorkTimeStatsFromWebsiteManager = new WorkTimeStatsFromWebsiteManager(WorkTimeCounter, captureCoordinator.WorkItemManager, workTimeHistory);

			workTimeStatsTimer.Start();
			WorkTimeCounter.Load();

			CaptureCoordinator.StatsCoordinator.SimpleWorkTimeStatsCalculated += SimpleWorkTimeStatsReceived;
			CaptureCoordinator.WorkItemCreated += CaptureCoordinatorWorkItemCreated;
		}

		private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(CurrentWorkController.CurrentWork)
				|| e.PropertyName == nameof(CurrentWorkController.IsOnline)
				|| e.PropertyName == nameof(CurrentWorkController.IsRuleOverrideEnabled))
			{
				SetWorkstateIcon();
			}

			// should be moved to platform agnostic code
			if (e.PropertyName == nameof(CurrentWorkController.CurrentWork))
			{
				viewModel.SetCurrentTaskById(CurrentWorkController.CurrentWork?.Id);

				if (CurrentWorkController.CurrentWork != null
					&& CurrentWorkController.IsCurrentWorkValid
					&& CurrentWorkController.LastWorkStateChangeReason == WorkStateChangeReason.UserSelect)
				{
					RecentHelper.AddRecent(CurrentWorkController.CurrentWork);
					viewModel.UpdateTasksAndProgress();
				}
			}

			var isWorking = CurrentWorkController.CurrentWork != null;
			if (isWorking)
			{
				NotificationService.HideNotification(NotificationKeys.IdleStopWork);
			}
			else
			{
				IdleDetector.ResetIdleWorkTime();
			}
		}

		private void CaptureCoordinatorWorkItemCreated(object sender, WorkItemEventArgs e)
		{
			WorkTimeCounter.AddWorkItem(e.WorkItem);
			CaptureCoordinator.IdleDetector.AddWorkItem(e.WorkItem);
		}

		private void SimpleWorkTimeStatsReceived(object sender, SingleValueEventArgs<SimpleWorkTimeStats> e)
		{
			var guiContext = Platform.Factory.GetGuiSynchronizationContext();
			guiContext.Post(stats =>
			{
				if (App.IsShuttingDown) return;
				var totalStats = (SimpleWorkTimeStats)stats;
				var sumTime = new TimeSpan(totalStats.Stats.Values.Sum(n => n.TotalWorkTime.Ticks));
				//userNameItem.ToolTipText = string.Format(Labels.SumWorkHours, sumTime.TotalHours.ToString("0.#"));
				MenuQuery.Instance.SimpleWorkTimeStats.Update(totalStats);
				//menuBuilder.UpdateTargetTotalWorkTimePercentages(totalStats);
				//contextMenuForm.UpdateSimpleStats(totalStats);
				//UpdateCurrentItemTooltipText();
			}, e.Value);
		}

		bool isUpdatingWorkTime = false;
		private readonly StopwatchLite updateTimer = new StopwatchLite(TimeSpan.FromSeconds(20), true);
		private void UpdateWorkTimeAndTaskbarInfoNew()
		{
			if (!updateTimer.IsIntervalElapsedSinceLastCheck()) return;

			var guiContext = Platform.Factory.GetGuiSynchronizationContext();
			if (isUpdatingWorkTime) return;
			isUpdatingWorkTime = true;
			try
			{
				var todaysWorkTime = WorkTimeCounter.TodaysWorkTime;
				var stats = WorkTimeStatsFromWebsiteManager.GetLocalWorkTimeStatsIfExact();
				if (WorkTimeStatsFromWebsiteManager.HasExactLocalWorkTime)
				{
					viewModel.UpdateWorkTimeStats(stats);
					return;
				}

				if (stats == null)
				{
					viewModel.UpdateTodaysStats(todaysWorkTime);
				}
				else
				{
					viewModel.UpdateWorkTimeStats(stats);
				}
			}
			finally
			{
				isUpdatingWorkTime = false;
			}

			ThreadPool.QueueUserWorkItem(_ =>
				WorkTimeStatsFromWebsiteManager.GetWorkTimeStatsFromServer(
					timeStats => guiContext.Post(__ =>
					{
						try
						{
							viewModel.UpdateWorkTimeStats(timeStats);
						}
						finally
						{
							isUpdatingWorkTime = false;
						}
					}, null),
					timeSpan =>
					{
						guiContext.Post(__ =>
						{
							try
							{
								viewModel.UpdateTodaysStats(timeSpan);
							}
							finally
							{
								isUpdatingWorkTime = false;
							}
						}, null);
					}));
		}
	}
}