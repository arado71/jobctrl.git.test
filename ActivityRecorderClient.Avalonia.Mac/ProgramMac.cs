using ActivityRecorderClientAV;
using AppKit;
using Avalonia;
using Avalonia.Threading;
using Foundation;
using log4net;
using ObjCRuntime;
using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel.Dispatcher;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.ViewMac;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Tct.ActivityRecorderClient
{
	public class ProgramMac
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static MacInit? init;
		private static DispatcherTimer? timer;

		//[STAThread]
		static void Main(string[] args)
		{
			LogConfig.ConfigureLogging();

			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			Runtime.MarshalManagedException += (_, args) =>
			{
				args.ExceptionMode = MarshalManagedExceptionMode.Default;
				log.Error("MarshalManagedException", args.Exception);
			};
			Runtime.MarshalObjectiveCException += (object sender, MarshalObjectiveCExceptionEventArgs args) =>
			{
				log.Error("MarshalObjectiveCException " + args.Exception.ToString());
			};

			Platform.RegisterFactory(PlatformMac.Factory);

			//ExceptionHandler.AsynchronousThreadExceptionHandler = new WcfExceptionLogger();

			log.Info("Starting");
			DebugEx.SetGuiThread();

			App.Initialized = () =>
			{
				Dispatcher.UIThread.UnhandledException += (_, e) => ShowErrorDialog(e.Exception, "(Main)", true);
				// hide dock icon
				NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;

				var notificationSvc = Platform.Factory.GetNotificationService();
				if (!PrerequisiteHelper.CanStartApplication(notificationSvc))
				{
					log.Info("Prerequisites not matched, exiting...");
					NSApplication.SharedApplication.Terminate(null);
				}
				timer = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.Normal,
					(_, __) =>
					{
						if (!PrerequisiteHelper.CanContinueRunning(notificationSvc, App.MainWindow.CurrentWorkController))
						{
							log.Info("Prerequisites not matched now, exiting...");
							NSApplication.SharedApplication.Terminate(null);
						}
					});

				init = new MacInit();
				init.MainWindowReady();
			};

			App.Exiting = () =>
			{
				log.Info("Exiting...");
				timer?.Stop();
				init?.MainWindowExiting();
			};

			NSApplication.Init();
			NSApplication.SharedApplication.Delegate = new AppDelegate();
			NSApplication.Main(args);
		}

		private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
		{
			log.Error("UnobservedTaskException", e.Exception);
		}

		public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			if (ex != null
				&& ex is UnauthorizedAccessException
				&& ex.ToString().Contains("System.Deployment.Application.DisposableBase.Finalize()"))
			{
				log.Fatal("ClickOnce error, but preventing shutdown", ex);
				return;
			}
			ShowErrorDialog(ex, "(Background)", false);
			log.Fatal("Initiating shutdown...");
			Environment.Exit(-1);
		}

		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			ShowErrorDialog(e.Exception, "(Main)", true);
		}

		private static void ShowErrorDialog(Exception ex, string addInfo, bool canContinue)
		{
			// TODO: mac
			log.Error("ShowErrorDialog " + addInfo, ex);
			/*using (ThreadExceptionDialog excptDlg = new ThreadExceptionDialog(ex ?? new Exception("Unknown Exception")))
			{
				log.Fatal("Unhandled exception " + addInfo, ex);
				if (!canContinue)
					((Button)excptDlg.CancelButton).Enabled = false;
				excptDlg.Text += " " + addInfo + ", Please send details to support";
				DialogResult result = excptDlg.ShowDialog();
				log.Info("Exception dialog: " + result);
				if (result == DialogResult.Abort)
					Application.Exit();
			}*/
		}

	}
}

