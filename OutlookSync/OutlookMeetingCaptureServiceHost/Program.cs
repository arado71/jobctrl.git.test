using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.Reflection;
using log4net;

namespace OutlookMeetingCaptureServiceHost
{
	static class Program
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);	//TODO: adding log4net.dll to installer!!!

		public static string ApplicationName = "JC Meeting";
		public static string Bitness = IntPtr.Size == 8 ? "64 bit" : "32 bit";
		public static Version Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
		public static readonly string DebugOrReleaseString = (System.Reflection.Assembly.GetExecutingAssembly()
			.GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
			.OfType<System.Reflection.AssemblyConfigurationAttribute>()
			.FirstOrDefault(n => !string.IsNullOrEmpty(n.Configuration)) ?? new System.Reflection.AssemblyConfigurationAttribute("Unknown")).Configuration + " build";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			try
			{
				log.Info("Initializing " + ApplicationName + " (" + Bitness + ") " + DebugOrReleaseString + " " + " Ver.:" + Version);
				log.Info(string.Format("Machinename: {0}, OSVersion: {1}, Framework version: {2}", Environment.MachineName, Environment.OSVersion, Environment.Version));
				log.Info("TickCount: " + Environment.TickCount + " (" + TimeSpan.FromMilliseconds(Environment.TickCount).ToHourMinuteSecondString() + ") Now: " + DateTime.Now + " UtcNow: " + DateTime.UtcNow);

				if (args == null || args.Length < 1)
				{
					log.Error("OutlookSync process must be started with min one argument.");
					return;
				}
				int parentPID;
				if (!int.TryParse(args[0], out parentPID))
				{
					log.Error("OutlookSync process must be started with exactly one argument that is the PID of the parent process.");
					return;
				}
				log.Info("OutlookSync process started with parent PID: " + parentPID);
				string storePattern = args.Length > 1 ? args[1] : null;
				var serviceTimeout = args.Length > 2 && int.TryParse(args[2], out var timeoutSecs)
					? TimeSpan.FromSeconds(timeoutSecs)
					: default(TimeSpan?);

				InitializeLibraries();

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
				Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

				Application.Run(new OutlookMeetingCaptureHostForm(parentPID, storePattern, serviceTimeout));

				log.Info("OutlookSync process stopped.\n\n");

			}
			catch (Exception ex)
			{
				log.Error("Unexpected error in main", ex);
				throw;
			}
		}

		private static void InitializeLibraries()
		{
			try
			{
				string executingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				string redemptionInteropDllPath = Path.Combine(executingDir, "Interop.Redemption.dll");
				string redemptionDllPath = Path.Combine(executingDir, "Redemption.dll");
				string redemption64DllPath = Path.Combine(executingDir, "Redemption64.dll");
				string redemptionZipPath = Path.Combine(executingDir, "Redemption.zip");

				if ((!File.Exists(redemptionInteropDllPath) || !File.Exists(redemptionDllPath) || !File.Exists(redemption64DllPath))
					&& File.Exists(redemptionZipPath))
				{
					UnZip(redemptionZipPath);
				}
			}
			catch (Exception e)
			{
				log.Error("Error occured while initializing libraries.", e);
				//throw;	//TODO: throw this error
			}
		}

		private static void UnZip(string path)
		{
			try
			{
				log.Info("Unzipping libraries...");

				using (ZipInputStream s = new ZipInputStream(File.OpenRead(path)))
				{
					ZipEntry theEntry;
					while ((theEntry = s.GetNextEntry()) != null)
					{
						//Console.WriteLine(theEntry.Name);

						string zipDirectoryName = Path.GetDirectoryName(path);
						string directoryName = Path.GetDirectoryName(theEntry.Name);
						string fileName = Path.GetFileName(theEntry.Name);

						// create directory
						if (directoryName.Length > 0)
						{
							Directory.CreateDirectory(Path.Combine(zipDirectoryName, directoryName));
						}

						if (fileName != String.Empty)
						{
							using (FileStream streamWriter = File.Create(Path.Combine(zipDirectoryName, theEntry.Name)))
							{
								int size = 2048;
								byte[] data = new byte[2048];
								while (true)
								{
									size = s.Read(data, 0, data.Length);
									if (size > 0)
									{
										streamWriter.Write(data, 0, size);
									}
									else
									{
										break;
									}
								}
							}
						}
					}
				}

				log.Info("Libraries were unzipped successfully.");
			}
			catch (Exception e)
			{
				log.Error("Error occured while unzipping libraries.", e);
			}
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			//ShowErrorDialog(ex, "(Background)", false);
			log.Fatal("Unhandled exception (Background)", ex);
			log.Fatal("Initiating shutdown...");
			Environment.Exit(-1);
		}

		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			//ShowErrorDialog(e.Exception, "(Main)", false);
			var ex = e.Exception;
			log.Fatal("Unhandled exception (Main)", ex);
			log.Fatal("Initiating shutdown...");
			Environment.Exit(-1);
		}

		private static void ShowErrorDialog(Exception ex, string addInfo, bool canContinue)
		{
			using (ThreadExceptionDialog excptDlg = new ThreadExceptionDialog(ex ?? new Exception("Unknown Exception")))
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
