using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using log4net.Appender;

namespace ConnectionTester
{
    static class Program
    {

		private static readonly ILog log =
			LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		/// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0)
            {
                int pid = 0;
                int.TryParse(args[0], out pid);
                if (pid != 0)
                {
                    var p = Process.GetProcessById(pid);
                    if (p != null)
                    {
                        RollingFileAppender app =LogManager.GetRepository()
                            .GetAppenders().OfType<RollingFileAppender>().First(appender => appender.Name == "RollingDebugLogFileAppender");
                        app.File = @"Logs\ConnectionTesterSidekick-Debug.log";
						app.ActivateOptions();
						log.Debug("Sidekick ConnectionTester started");
                        ConnectionTesterForm.TestConnectivity(true);
                        while (!p.HasExited)
                        {
                            ConnectionTesterForm.TestConnectivity(true);
                            Thread.Sleep(5000);
                        }
                    }
                }

            }

            else
            {
                Application.Run(new ConnectionTesterForm());
            }
        }
    }
}
