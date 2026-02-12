using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace JiraSyncTool
{
	class Program
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public static ServiceBase Service { get; set; }

		static void Main()
		{
			log4net.Config.XmlConfigurator.Configure();
			log.Info("Starting Jira Synchronizer tool...");
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#if DEBUG
			log.Error("Debug build does not work as windows service!");
			SyncService.StartService();
			Thread.Sleep(Timeout.Infinite);
#else
            Service = new SyncService();

			ServiceBase[] servicesToRun = new[] { 
				Service
			};
            ServiceBase.Run(servicesToRun);
#endif
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exo = e.ExceptionObject as Exception;
			log.Fatal("Jira sync crashed", exo);

			try
			{
				//var mailer = new MailSender(SendAccount);
				//var email = new MailDescriptor { Subject = "TMS UnhandledException" };
				//if (exc != null)
				//    email.Body = exc.ToString();

				//mailer.SendMail(email);
			}
			catch (Exception ex)
			{
				log.Error("SendMail failed", ex);
			}

			Environment.Exit(1);
		}
	}

}
