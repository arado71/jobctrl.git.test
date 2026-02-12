using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Mail;

namespace OutlookInteropServiceHost
{
	static class Program
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static string ApplicationName = "JC Mail";
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
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

			try
			{
				log.Info("Initializing " + ApplicationName + " (" + Bitness + ") " + DebugOrReleaseString + " " + " Ver.:" + Version);
				log.Info(string.Format("Machinename: {0}, OSVersion: {1}, Framework version: {2}", Environment.MachineName, Environment.OSVersion, Environment.Version));
				log.Info("TickCount: " + Environment.TickCount + " (" + TimeSpan.FromMilliseconds(Environment.TickCount).ToHourMinuteSecondString() + ") Now: " + DateTime.Now + " UtcNow: " + DateTime.UtcNow);
				if (args == null || args.Length < 1 || args.Length > 3)
				{
					log.Error("outlook interop process must be started with minimum of one argument.");
					return;
				}
				int parentPID;
				if (!int.TryParse(args[0], out parentPID))
				{
					log.Error("outlook interop process must be started with minimum of one argument that is the PID of the parent process.");
					return;
				}
				var trackingType = default(MailTrackingType);
				var trackingSettings = default(MailTrackingSettings);
				if (args.Length > 1)
				{
					if (!int.TryParse(args[1], out var trackTypeInt))
					{
						log.Error("invalid TrackingType value: " + args[1]);
						return;
					}
					trackingType = (MailTrackingType)trackTypeInt;
				}
				if (args.Length > 2)
				{
					if (!int.TryParse(args[2], out var trackingSettingsInt))
					{
						log.Error("invalid TrackingSettings value: " + args[2]);
						return;
					}
					trackingSettings = (MailTrackingSettings)trackingSettingsInt;
				}
				InitializeLibraries();

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new OutlookInpteropServiceHostForm(parentPID, trackingType, trackingSettings));
			}
			catch (Exception ex)
			{
				log.Error("Unable to start outlook interop process", ex);
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

				//todo mutex lock ? (don't race on unzipping)
				if ((!File.Exists(redemptionInteropDllPath) || !File.Exists(redemptionDllPath) || !File.Exists(redemption64DllPath))
					&& File.Exists(redemptionZipPath))
				{
					UnZip(redemptionZipPath);
				}
			}
			catch (Exception ex)
			{
				log.Error("Error occured while initializing libraries.", ex);
				throw;
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
								byte[] data = new byte[2048];
								while (true)
								{
									var size = s.Read(data, 0, data.Length);
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
			catch (Exception ex)
			{
				log.Error("Error occured while unzipping libraries.", ex);
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
