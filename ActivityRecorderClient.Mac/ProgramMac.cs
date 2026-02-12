using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using log4net;
using System.ServiceModel.Dispatcher;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient
{
	public class ProgramMac
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		//[STAThread]
		static void Main(string [] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			ExceptionHandler.AsynchronousThreadExceptionHandler = new WcfExceptionLogger();
//			Application.EnableVisualStyles();
//			Application.SetCompatibleTextRenderingDefault(false);
//			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
//			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

			NSApplication.Init();
			NSApplication.Main(args);
		}

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
			//old way before legacyUnhandledExceptionPolicy
			//if (e.IsTerminating)
			//{
			//    //don't show "xy encountered a problem and needs to close" message
			//    Environment.Exit(-1);
			//}
		}

		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			ShowErrorDialog(e.Exception, "(Main)", true);
		}

		private static void ShowErrorDialog(Exception ex, string addInfo, bool canContinue)
		{
			using (ThreadExceptionDialog excptDlg = new ThreadExceptionDialog(ex ?? new Exception("Unknown Exception")))
			{
				log.Fatal("Unhandled exception " + addInfo, ex);
				if (!canContinue)
					((Button)excptDlg.CancelButton).Enabled = false;
				excptDlg.Text += " " + addInfo + ", Please send details to support";
				DialogResult result = excptDlg.ShowDialog();
				log.Info("Exception dialog: " + result);
				if (result == DialogResult.Abort)
					Application.Exit();
			}
		}

	}
}

