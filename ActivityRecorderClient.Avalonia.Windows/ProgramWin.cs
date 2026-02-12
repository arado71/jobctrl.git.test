using log4net;
using log4net.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActiveDirectoryIntegration;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.Stability;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.Telemetry.Data;
using Tct.ActivityRecorderClient.Update;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient
{
	//todo if ui thread is dead (or frozen) or mutex is not owned kill app !
	//todo if we have no desktop captures for a long time issue some warnings. (and probably should restart DC thread)
	static class ProgramWin
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static Mutex singletonMutex;
		public static bool RestartOnExit { get; set; }
		private static int guiThreadId;
		private static bool isStartupCheck;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            Platform.RegisterFactory(PlatformWin.Factory);

			isStartupCheck = args.Length > 0 && args[0].Equals("StartupCheck", StringComparison.InvariantCulture);
			Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			guiThreadId = Thread.CurrentThread.ManagedThreadId;
			DebugEx.SetGuiThread();

			var getIsolated = args.Length > 0 && args[0].Equals("GetIsolatedStoragePath", StringComparison.InvariantCulture);
			if (getIsolated)
			{
				string path = IsolatedStorageSerializationHelper.GetIsolatedStoragePath();
				if (string.IsNullOrEmpty(path))
				{
					Environment.Exit(-1);
				}
				Console.WriteLine(path);
				log.DebugFormat("Isolated Storage path returned: {0}", path);
				Environment.Exit(0);
			}

			var deletePersonalData = args.Length > 0 && args[0].Equals("DeletePersonalData", StringComparison.InvariantCulture);
			if (deletePersonalData)
			{
				var msg = UninstallHelper.DeletePersonalData();
				Console.WriteLine(msg);
				Environment.Exit(0);
			}

			var checkConnection = args.Length > 0 && args[0].Equals("CheckConnectivity", StringComparison.InvariantCulture);
			if (checkConnection)
			{
				var status = NetworkStabilityManager.CheckConnectivity();
				if (status) Environment.Exit(0); else Environment.Exit(-500);
			}

			var checkLoginData = args.Length > 0 && args[0].Equals("SetLoginData", StringComparison.InvariantCulture);
			if(checkLoginData)
			{
				int userId = -1;
				if(args.Length < 3 || !int.TryParse(args[1], out userId))
				{
					Console.WriteLine("Not enough parameters or userid is incorrect.");
					Environment.Exit(-1);
				}

				ConfigManager.EnsureLoggedIn(() => new ConfigManager.LoginData() { UserId = userId, UserPassword = AuthenticationHelper.GetHashedHexString(args[2]), RememberMe = true });
				Console.WriteLine("Login data was set.");
				Environment.Exit(0);
			}
#if DEV
			try
			{
				var regValue = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\JobCTRL\\JobCTRL-DEV", "Test_environment", null);
				if(regValue == null && !isStartupCheck)
				if (MessageBox.Show("Press OK to start JobCTRL DEV build") != DialogResult.OK) return;
			}
			catch (Exception)
			{
			}
#endif
#if STRESSTEST
			return;
#endif
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			// TODO: mac
			//ExceptionHandler.AsynchronousThreadExceptionHandler = new WcfExceptionLogger();
			ServicePointManager.ServerCertificateValidationCallback += ValidateServerCertificate;

			ConfigManager.Initialize();
			var showHiddenClient = args.Length > 0 && args[0].Equals("ShowHiddenClient", StringComparison.InvariantCulture);
			if (showHiddenClient)
			{
				ConfigManager.ShowTaskBarIcon();
			}

			if (isStartupCheck)
			{
				Console.WriteLine(Encoding.Default.GetString(Encoding.UTF8.GetBytes(ConfigManager.Classifier ?? string.Empty)));
				Environment.Exit(0);
				return;
			}
			LogElevationInfo();
			LogEnvironmentInfo();
			if (!string.IsNullOrEmpty(ConfigManager.ValidCertificate))
			{
				acceptedCerts.Add(ConfigManager.ValidCertificate);
			}

			if (InstallNotifyHelper.IsInstallerRunning())
			{
				LocalizationHelper.InitLocalization();
				InstallNotifyHelper.NotifyInstaller();
				return;
			}

			var updateSerivce = Platform.Factory.GetUpdateService();
			try
			{
				if (!ElevatedPrivilegesHelper.IsElevated && args.Length > 0 && args[0].Equals("RestartAsAdmin", StringComparison.InvariantCulture))
					ElevatedPrivilegesHelper.RunAsAdmin = false;
				var initialized = true;// TODO: mac updateSerivce.Initialize();
				if (!initialized)
				{
					log.FatalAndFail("UpdateService is not initialized");
					return;
				}
			}
			catch (Exception ex)
			{
				if (ex.Message == UpdateWixWinService.InitializeExMsgRestartNeeded
					|| ex.Message == UpdateWixWinService.InitializeExMsgUpdatetNeeded
					)
				{
					return; //it's not an error
				}
				log.Error("Failed to initialize UpdateService", ex);
				if (ex.Message == UpdateWixWinService.InitializeExMsgMissingBootstrap)
				{
					MessageBox.Show(Labels.Program_NotificationWixNoBootstrapBody, Labels.Program_NotificationWixNoBootstrapTitle);
				}
				else if (ex.Message == UpdateWixWinService.InitializeExMsgMissingMsiexec)
				{
					MessageBox.Show(Labels.Program_NotificationWixNoMsiexecBody, Labels.Program_NotificationWixNoMsiexecTitle);
				}
				else
				{
					log.ErrorAndFail("Unknown error " + ex);
				}
				return;
			}

			var isEmergencyRestart = args.Length > 0 && args[0].Equals("EmergencyRestart", StringComparison.InvariantCulture);
			int? startingWorkId = null;
			if (isEmergencyRestart)
			{
				log.Debug("Emergency restart initiated");
				if (args.Length == 3)
				{
					int workId;
					if (int.TryParse(args[2], out workId) && workId > 0)
					{
						startingWorkId = workId;
						log.DebugFormat("Emergency restart will set previous task {0}", startingWorkId);
					}
				}

				if (int.TryParse(args[1], out var prevPid) && prevPid > 0)
				{
					try
					{
						var prevProcess = Process.GetProcessById(prevPid);
						prevProcess.WaitForExit(10000);
						if (!prevProcess.HasExited)
						{
							log.Debug("Previous instance has to be killed");
							prevProcess.Kill();
						}
						else
							log.Debug("Previous instance has just been exited");
					}
					catch (ArgumentException)
					{
						// process already exited, do nothing
						log.Debug("Previous instance already exited");
					}
				}
			}

			var owned = false;
			try
			{
				// TODO: mac
				var mSec = new MutexSecurity();
				var rule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
								   MutexRights.Modify | MutexRights.Synchronize | MutexRights.TakeOwnership | MutexRights.ReadPermissions,
								   AccessControlType.Allow);
				mSec.AddAccessRule(rule);
				//for legacy reasons we cannot use ConfigManager.ApplicationName here
				try
				{
					singletonMutex = new Mutex(true, "Local\\" + ConfigManager.MutexName, out owned); //Global would prevent proper usage on terminal servers
					if (!owned) //we can still have an AbandonedMutexException which is ok
					{
						try
						{
							owned = singletonMutex.WaitOne(isEmergencyRestart ? 5000 : 0); //this will throw on comps not having 3.5 SP1
						}
						catch (AbandonedMutexException)
						{
							log.Info("Abandoned app lock");
							owned = true;
						}
					}
				}
				catch (UnauthorizedAccessException) //raised when acquiring Global\ActivityRecorderClient when Local\ActivityRecorderClient is owned with no security
				{
					log.Info("No access to app lock");
					owned = false;
				}
#if (!DEBUG)
				if (!owned)
				{
					if(!isEmergencyRestart)
					{
						SameAppNotifyHelper.Notify();
						if (args.Length > 0 && args[0].Equals("SuppressRunningNotification", StringComparison.InvariantCulture)) return;
						MessageBox.Show(Labels.Program_NotificationAlreadyRunningBody, Labels.Program_NotificationAlreadyRunningTitle);
					} else {
						MessageBox.Show(Labels.Program_FailedRestartBody, Labels.Program_FailedRestartTitle);
					}

					return;
				}
#endif
				if (IsGlobalAppRunning()) //Local mutex can be created when Global is owned by an other (old) app
				{
#if (!DEBUG)
					MessageBox.Show(Labels.Program_NotificationAlreadyRunningBody, Labels.Program_NotificationAlreadyRunningTitle);
					return;
#endif
				}
				// TODO: mac
				//AppControlServiceHelper.RegisterProcess();
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
				TelemetryHelper.Measure(TelemetryHelper.KeyJcStarted);
				if (!ConfigManager.EnsureLoggedIn(
					() => ActiveDirectoryLoginServiceClientWrapper.IsActiveDirectoryAuthEnabled
						? ActiveDirectoryAuthenticationManager.LoginWithWindowsUser(ActiveDirectoryAuthenticationFailedFallback)
						: LoginForm.DisplayLoginForm())
					)
				{
					//AppControlServiceHelper.UnregisterProcess();
					return;
				}
				TelemetryHelper.Measure(TelemetryHelper.KeyJcLoggedIn);
				if (ConfigManager.StartWorkAfterLogin) TelemetryHelper.Measure(TelemetryHelper.KeyStartAfterLogin);
				LocalizationHelper.InitLocalization();
				Application.ThreadException += Application_ThreadException;	//This must be placed after LoginForm popup, because this event subscription disappears after LoginForm closed.
				var startType = isEmergencyRestart
					? ApplicationStartType.EmergencyRestart
					: ConfigManager.StartWorkAfterLogin
						? ApplicationStartType.StartWorkAfterLogin
						: ApplicationStartType.Normal;
				Application.Run(new ActivityRecorderForm(startType) { Text = SameAppNotifyHelper.WindowTitle, StartingWorkId = startingWorkId });
				log.Info("Exiting program");
			}
			catch (MissingMethodException ex) //Boolean System.Threading.WaitHandle.WaitOne(Int32)
			{
				log.Error("Framework method missing", ex);
				MessageBox.Show(Labels.Program_Req35SP1Body, Labels.Program_Req35SP1Title);
			}
			catch (Exception ex)
			{
				log.Error("Unexpected error in main", ex);
				throw;
			}
			finally
			{
				if (singletonMutex != null)
				{
					if (owned) singletonMutex.ReleaseMutex();
					singletonMutex.Close();
				}
				if (RestartOnExit)
				{
					string shortcutPath = null;
					try
					{
						shortcutPath = updateSerivce.GetAppPath();
						if (!File.Exists(shortcutPath))
						{
							log.Error("Unable to find file " + shortcutPath);
						}
						else
						{
							Process.Start(shortcutPath);
						}
					}
					catch (Exception ex)
					{
						log.Error("Unable to start process " + shortcutPath, ex);
					}
				}
			}
		}

		private static ConfigManager.LoginData ActiveDirectoryAuthenticationFailedFallback()
		{
			if (!ConfigManager.SuppressActiveDirectoryFallbackLogin) return LoginForm.DisplayLoginForm();
			if (ConfigManager.IsTaskBarIconShowing)
			{
				if (MessageBox.Show(Labels.Login_NotificationAuthenticationFailed, Labels.Login_LoginButton, MessageBoxButtons.RetryCancel) == DialogResult.Retry) 
					return ActiveDirectoryAuthenticationManager.LoginWithWindowsUser(ActiveDirectoryAuthenticationFailedFallback);
			
				Environment.Exit(-1);
				return null;
			}

			log.Debug("AD login failed and fallback login disabled. Waiting 5 mins...");
			Thread.Sleep(TimeSpan.FromMinutes(5));
			return ActiveDirectoryAuthenticationManager.LoginWithWindowsUser(ActiveDirectoryAuthenticationFailedFallback);
		}

		private static bool IsGlobalAppRunning()
		{
			try
			{
				var owned = false;
				var globalMutex = Mutex.OpenExisting("Global\\" + ConfigManager.MutexName);
				try
				{
					owned = globalMutex.WaitOne(1000);
				}
				catch (AbandonedMutexException)
				{
					owned = true;
				}
				finally
				{
					if (owned) globalMutex.ReleaseMutex();
					globalMutex.Close();
				}
				return !owned;
			}
			catch (WaitHandleCannotBeOpenedException)
			{
				return false; //ok
			}
			catch (Exception ex)
			{
				log.Error("Unexpected error getting global app lock", ex);
				return true; //not ok
			}
		}

		//This is called after the login form is closed so we cannot release the mutex here
		//static void Application_ApplicationExit(object sender, EventArgs e)
		//{
		//}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			if (isStartupCheck)
			{
				Console.Error.WriteLine(ex?.ToString() ?? "Unknown exception");
				Environment.Exit(-999);
			}
			if (ex != null
				&& ex is UnauthorizedAccessException
				&& ex.ToString().Contains("System.Deployment.Application.DisposableBase.Finalize()"))
			{
				log.Fatal("ClickOnce error, but preventing shutdown", ex);
				return;
			}
			ShowErrorDialog(ex);
			//old way before legacyUnhandledExceptionPolicy
			//if (e.IsTerminating)
			//{
			//    //don't show "xy encountered a problem and needs to close" message
			//    Environment.Exit(-1);
			//}
		}

		public static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			ShowErrorDialog(e.Exception);
		}


		private static void ShowErrorDialog(Exception ex)
		{
			DebugEx.Break();
			TelemetryHelper.Measure(TelemetryHelper.KeyException, new ExceptionData(ex));
			TelemetryHelper.Save();
			if (guiThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				ShowErrorDialog(ex, "(Main)", Application.MessageLoop);
			}
			else
			{
				ShowErrorDialog(ex, "(Background)", false);
			}
		}

		private static void ShowErrorDialog(Exception ex, string addInfo, bool canContinue)
		{
			try
			{
				using (var excptDlg = new ExceptionDialog(ex ?? new Exception("Unknown Exception")))
				{
					log.Fatal("Unhandled exception " + addInfo + " canContinue: " + canContinue, ex);
					if (!canContinue) ((Button) excptDlg.CancelButton).Enabled = false;
					//excptDlg.Text += " " + addInfo + ", Please send details to support";
					DialogResult result = excptDlg.ShowDialog();
					log.Info("Exception dialog: " + result);
					if (canContinue)
					{
						if (result != DialogResult.Abort) return;
						Debug.Assert(Application.MessageLoop);
						Application.Exit();
					}
					else
					{
						log.Fatal("Initiating shutdown...");
						Environment.Exit(-1);
					}
				}
			}
			catch(Exception e)
			{
				log.Fatal("Exception during reporting!", e);
				var reporter = Platform.Factory.GetErrorReporter();
				reporter.ReportClientError("Unattended error reporter" + Environment.NewLine + Environment.NewLine + ex, true, progress => { }, () => false );
				log.Fatal("Initiating shutdown...");
				Environment.Exit(-1);
			}
		}

		private static void LogElevationInfo() //cannot find a better place as this is platform specific atm. (i.e. cannot be in ConfigManager)
		{
			try
			{
				var isElevated = ProcessElevationHelper.IsElevated();
				log.Info(isElevated ? "Started with elevated privileges" : "Started without elevated privileges");
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to determine elevation level", ex);
			}
		}

		private static void LogEnvironmentInfo()
		{
			if (!log.IsVerboseEnabled()) return;
			log.Verbose("Command line: " + Environment.CommandLine);
			log.Verbose("Current directory: " + Environment.CurrentDirectory);
			log.Verbose("Is64BitOperatingSystem: " + Environment.Is64BitOperatingSystem);
			log.Verbose("Is64BitProcess: " + Environment.Is64BitProcess);
			log.Verbose("MachineName: " + Environment.MachineName);
			log.Verbose("OSVersion: " + ConfigManager.EnvironmentInfo.OSVersion + (ConfigManager.EnvironmentInfo.OSFullName != null ? " (" + ConfigManager.EnvironmentInfo.OSFullName + ")" : ""));
			log.Verbose("ProcessorCount: " + Environment.ProcessorCount);
			log.Verbose("SystemDirectory: " + Environment.SystemDirectory);
			log.Verbose("SystemPageSize: " + Environment.SystemPageSize);
			log.Verbose("UserDomainName: " + Environment.UserDomainName);
			log.Verbose("UserName: " + Environment.UserName);
			log.Verbose("UserInteractive: " + Environment.UserInteractive);
			log.Verbose("Version: " + Environment.Version);
			log.Verbose("WorkingSet: " + Environment.WorkingSet);
			foreach (var item in Assembly.GetExecutingAssembly().MyGetReferencedAssembliesRecursive())
			{
				log.VerboseFormat("Loaded assembly {0} [{1}, {2}]", item.Key, item.Value.FullName, item.Value.Location);
			}

			foreach (var missingAssembly in Assembly.GetExecutingAssembly().MyGetMissingAssembliesRecursive())
			{
				log.VerboseFormat("Missing assembly {0} <= {1}", missingAssembly.MissingAssemblyName, missingAssembly.MissingAssemblyNameParent);
			}
			var variables = Environment.GetEnvironmentVariables();
			foreach (var entry in variables.Keys.Cast<object>().ToDictionary(k => k.ToString(), v => variables[v].ToString()).OrderBy(k => k.Key))
			{
				log.VerboseFormat("Environment variable {0}={1}", entry.Key, entry.Value);
			}

			foreach (var process in Process.GetProcesses().OrderBy(p => p.Id))
			{
				log.VerboseFormat("Running process: {0} [{1}, {2}]", process.ProcessName, process.Id, GetModuleInfo(process));
			}
		}

		private static string GetModuleInfo(Process process)
		{
			try
			{
				return process.MainModule.FileName + ", " + process.MainModule.FileVersionInfo.ProductVersion;
			}
			catch (Exception)
			{
				return "n/a";
			}
		}

		public static readonly List<string> acceptedCerts = new List<string>();
		public static readonly List<string> rejectedCerts = new List<string>();
		public static readonly string jcPublic = "3082010A0282010100979C0E4038837F0B7460BFC50881DCBBC684647F7752A9DC14C316B633CE3B70D950F4F6BE4D670CA4F0BE71E8463EE7A69915611DDAA2E9FAC68A446E4118EF4E9FCE53965ED4E4C298703D6BA48CBF8BFA202604F51CB7607FC1B19BA46F5A4643CF20BCA75F3FA5F839DF88D43E9074A989DF7C0D3965A97E1929C2E26E5F32FD309E08658F7B971A86B5476480276ED7525B309AB30711575A16C6E43DE2A5756C2843544FB6F9282372D204E446AF9316B7630F5E31FF75B5ACE41AA73E52DEB8EC843D27F30610F541EDC54CCBC2F3F3457330F59427F1E5E41A20DA896A3A82B0171408E4BA6012E45BBC67DAF662A6CA488752065B60BAA681310CD50203010001";
		public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
#if DEBUG
			return true; //needed for self signed cert on https
#endif
			if (sslPolicyErrors == SslPolicyErrors.None)
			{
				return true;
			}
			else
			{
				lock (acceptedCerts)
				{
					var publicKey = certificate.GetPublicKeyString();
					if (acceptedCerts.Contains(publicKey)) return true;
					if (rejectedCerts.Contains(publicKey)) return false;
					log.Warn("Invalid certificate (" + sslPolicyErrors + ") " + certificate);
					if (publicKey == jcPublic)
					{
						acceptedCerts.Add(jcPublic); //don't log this again
						return true;
					}
					if (MessageBox.Show(Labels.Program_CertInvalidBody + Environment.NewLine
						+ sslPolicyErrors + Environment.NewLine + Environment.NewLine
						+ certificate, Labels.Program_CertInvalidTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						acceptedCerts.Add(publicKey);
						log.Info("Accepted certificate " + certificate);
						return true;
					}
					else
					{
						rejectedCerts.Add(publicKey);
						log.Info("Rejected certificate " + certificate);
						return false;
					}
				}
			}
		}
	}
}