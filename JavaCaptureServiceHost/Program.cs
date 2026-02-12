using log4net;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace JavaCaptureServiceHost
{
	static class Program
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static string ApplicationName = "JC JavaCapture";
		public static string Bitness = Environment.Is64BitProcess ? "64 bit" : "32 bit";
		public static Version Version = Assembly.GetExecutingAssembly().GetName().Version;
		public static readonly string DebugOrReleaseString = (Assembly.GetExecutingAssembly()
																  .GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false)
																  .OfType<AssemblyConfigurationAttribute>()
																  .FirstOrDefault(n => !string.IsNullOrEmpty(n.Configuration)) ?? new AssemblyConfigurationAttribute("Unknown")).Configuration + " build";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			try
			{
				log.Info("Initializing " + ApplicationName + " (" + Bitness + ") " + DebugOrReleaseString + " " + " Ver.:" + Version);
				log.Info(string.Format("Machinename: {0}, OSVersion: {1}, Framework version: {2}", Environment.MachineName, Environment.OSVersion, Environment.Version));
				log.Info("TickCount: " + Environment.TickCount + " Now: " + DateTime.Now + " UtcNow: " + DateTime.UtcNow);
				if (args == null || args.Length != 1)
				{
					log.Error("JavaCaptureServiceHost process must be started with exactly one argument.");
					return;
				}
				int parentPid;
				if (!int.TryParse(args[0], out parentPid))
				{
					log.Error("JavaCaptureServiceHost process must be started with exactly one argument that is the PID of the parent process.");
					return;
				}

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.ThreadException += Application_ThreadException;
				Application.Run(new JavaCaptureHostForm(parentPid));
			}
			catch (Exception ex)
			{
				log.Error("Unable to start JavaCaptureServiceHost process", ex);
				throw;
			}
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			ShowErrorDialog(ex, "(Background)", false);
			log.Fatal("Unhandled exception (Background)", ex);
			log.Fatal("Initiating shutdown...");
			Environment.Exit(-1);
		}

		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			ShowErrorDialog(e.Exception, "(Main)", false);
			var ex = e.Exception;
			log.Fatal("Unhandled exception (Main)", ex);
			log.Fatal("Initiating shutdown...");
			Environment.Exit(-1);
		}

		private static void ShowErrorDialog(Exception ex, string addInfo, bool canContinue)
		{
			using (var excptDlg = new ThreadExceptionDialog(ex ?? new Exception("Unknown Exception")))
			{
				log.Fatal("Unhandled exception " + addInfo, ex);
				if (!canContinue) ((Button)excptDlg.CancelButton).Enabled = false;
				excptDlg.Text += " " + addInfo + ", Please send details to support";
				DialogResult result = excptDlg.ShowDialog();
				log.Info("Exception dialog: " + result);
				if (result == DialogResult.Abort)
					Application.Exit();
			}
		}
	}
}
