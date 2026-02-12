using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using VoxCTRL.ActivityRecorderServiceReference;
using VoxCTRL.View;
using log4net;

namespace VoxCTRL
{
	static class Program
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static Mutex singletonMutex;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			ServicePointManager.ServerCertificateValidationCallback += ValidateServerCertificate;
	
			if (!string.IsNullOrEmpty(ConfigManager.ValidCertificate))
			{
				acceptedCerts.Add(ConfigManager.ValidCertificate);
			}

			var owned = false;
			var mSec = new MutexSecurity();
			var rule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
							   MutexRights.Modify | MutexRights.Synchronize | MutexRights.TakeOwnership | MutexRights.ReadPermissions,
							   AccessControlType.Allow);
			mSec.AddAccessRule(rule);
			try
			{
				singletonMutex = new Mutex(true, "Local\\" + ConfigManager.ApplicationName, out owned, mSec); //Global would prevent proper usage on terminal servers
				if (!owned) //we can still have an AbandonedMutexException which is ok
				{
					try
					{
						owned = singletonMutex.WaitOne(0); //this will throw on comps not having 3.5 SP1
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
			if (!owned)
			{
				//MessageBox.Show(Labels.Program_NotificationAlreadyRunningBody, Labels.Program_NotificationAlreadyRunningTitle);
				MessageBox.Show("Ezen a gépen már fut egy másik VoxCTRL alkalmazás.", "Hiba!");
				return;
			}

			ConfigManager.UserId = -1;
			var loginData = LoginData.LoadFromDisk();
			if (loginData == null)
			{
				loginData = ActiveDirectoryLoginServiceClientWrapper.IsActiveDirectoryAuthEnabled ? ActiveDirectoryAuthenticationManager.LoginWithWindowsUser(LoginForm.DisplayLoginForm) : LoginForm.DisplayLoginForm();
				if (loginData == null)
				{
					log.Info("LoginData is null");
					return;
				}
				if (loginData.RememberMe)
				{
					LoginData.SaveToDisk(loginData);
				}
				else
				{
					LoginData.DeleteFromDisk();
				}
			}
			
			ConfigManager.UserId = loginData.UserId;
			ConfigManager.UserPassword = loginData.UserPassword;
			ConfigManager.UserName = loginData.AuthData == null ? "" : loginData.AuthData.Name;
			ConfigManager.UserPasswordExpirationDate = loginData.UserPasswordExpirationDate;

			//ConfigManager.UserId = 13;
			//ConfigManager.UserPassword = Communication.AuthenticationHelper.GetHashedHexString("1");
			if (ConfigManager.UserId == -1) return;
			Application.Run(new RecorderForm());
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			//if (ex != null
			//    && ex is UnauthorizedAccessException
			//    && ex.ToString().Contains("System.Deployment.Application.DisposableBase.Finalize()"))
			//{
			//    log.Fatal("ClickOnce error, but preventing shutdown", ex);
			//    return;
			//}
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

		private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			ShowErrorDialog(e.Exception, "(Main)", true);
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
					if (MessageBox.Show("Invalid certificate. Do you accept?" + Environment.NewLine
						+ sslPolicyErrors + Environment.NewLine + Environment.NewLine
						+ certificate, "Do you want to accept this certificate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
