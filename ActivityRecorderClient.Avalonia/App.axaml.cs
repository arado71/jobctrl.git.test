using ActivityRecorderClient.Avalonia.ViewModels;
using ActivityRecorderClient.Avalonia.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Forms;

namespace ActivityRecorderClientAV;

public partial class App : Application
{
	private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	public static int MainWindowWidth = 365;
	public static int MainWindowHeight = 650;
	public static double maxScaleValue = 2.0;

	public static byte ThemeSetting { get; set; } = 2; // 1: Light, 2: Dark
	public static string ThemeLightDefault { get; set; } = "#D0FFFFFF";
	public static string ThemeLight { get; set; } = "#D0FFFFFF"; // might change
	public static string ThemeDarkDefault { get; set; } = "#D0101010";
	public static string ThemeDark { get; set; } = "#D0101010"; // might change
	public static string OverlayColorDefault { get; set; } = "#35505050";
	public static string OverlayColor { get; set; } = "#35505050"; // might change
	public static Screen? PrimaryScreen { get; set; }

	public static List<NotificationFormAV> OpenNotificationWindows = [];
	public static bool IsSystemTheme { get; set; } = true;

	public static ActivityRecorderFormAV MainWindow { get; private set; }
	public static Action? Initialized { get; set; }
	public static Action? Exiting { get; set; }
	public static bool IsShuttingDown { get; private set; }
	public static bool LogoutOnExit { get; set; }

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);

		Resources["CircleColor"] = new SolidColorBrush(Color.Parse("Orange"));
		Resources["TaskInitials"] = "Ma";
		Resources["TaskFolder"] = "TCT »  TCT - DEV »";
		Resources["TaskName"] = "MacOS Upgrade implementation";
		Resources["DaysLeft"] = "3 days left / 11 days";
		Resources["ProgressDays"] = (double)72;
		Resources["HoursLeft"] = "55:20 / 74:00";
		Resources["ProgressHours"] = (double)74;

		Resources["TodayText"] = "Today's working time:";
		Resources["WeeklyText"] = "Weekly working time:";
		Resources["MonthyText"] = "Monthly working time:";
		Resources["QuarterlyText"] = "Quarterly working time:";
		Resources["YearlyText"] = "Yearly working time:";
	}

	public override async void OnFrameworkInitializationCompleted()
	{
		// Line below is needed to remove Avalonia data validation.
		// Without this line you will get duplicate validations from both Avalonia and CT
		BindingPlugins.DataValidators.RemoveAt(0);

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			var loggedIn = await ConfigManager.EnsureLoggedInAsync(async () =>
			{
				desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;
				var loginForm = new LoginFormAV();
				loginForm.Show();

				var result = await loginForm.WaitForCloseAsync();
				if (result != DialogResult.OK && result != DialogResult.Ignore)
				{
					return null;
				}
				return new ConfigManager.LoginData()
				{
					UserId = loginForm.UserId,
					UserPassword = loginForm.UserPassword,
					RememberMe = loginForm.RememberMe,
					StartWorkAfterLogin = loginForm.StartInGreen,
					AuthData = loginForm.AuthData,
					Culture = loginForm.Culture,
				};
			});

			if (!loggedIn)
			{
				log.Info("Login cancelled");
				desktop.Shutdown();
				log.Info("Exit");
				Environment.Exit(0);
				return;
			}

			MainWindow = new ActivityRecorderFormAV();
			desktop.MainWindow = MainWindow;
			desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
			PrimaryScreen = desktop.MainWindow.Screens?.Primary;
			Initialized?.Invoke();
			desktop.Exit += static (_, _) =>
			{
				IsShuttingDown = true;
				ExecNoThrow("Static exiting", () => Exiting?.Invoke());
				ExecNoThrow("MainWindow exit", () => MainWindow.Exit());
				if (LogoutOnExit)
				{
					ConfigManager.Logout();
				}
				log.Info("Exit");
			};
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			singleViewPlatform.MainView = new MainView
			{
				DataContext = new MainViewModel()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}

	private static void ExecNoThrow(string name, Action action)
	{
		log.DebugFormat("Calling {0}", name);
		try
		{
			action();
		}
		catch (Exception ex)
		{
			log.Error("Unexpected error in " + name, ex);
		}
	}
}
