using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using log4net;
using LotusNotesMeetingCaptureServiceNamespace;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Capturing.Meeting.LotusNotes
{
	//Meeting Tool and MeetingManager can use this service concurrently!
	public class LotusNotesMeetingCaptureService : IMeetingCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly List<FinishedMeetingEntry> emptyFinishedMeetings = new List<FinishedMeetingEntry>();

		private readonly ProcessCoordinator processCoordinator = ProcessCoordinator.LotusNotesProcessCoordinator;

		private readonly string mailServer;
		private readonly string mailFile;

		private readonly Func<LNLoginData> loginDataFactory;
		private bool loggedIn;  //If the service has been initialized successfully with a password.
		private bool loginCancelled;


		private bool? isLotusNotesInstalled;
		private bool IsLotusNotesInstalled
		{
			get
			{
				if (!isLotusNotesInstalled.HasValue)
				{
					isLotusNotesInstalled = LotusNotesSettingsHelper.IsLotusNotesInstalled;
				}
				return isLotusNotesInstalled.Value;
			}
		}

		public LotusNotesMeetingCaptureService() : this("", "", "") { }

		public LotusNotesMeetingCaptureService(string password) : this(password, "", "") { }

		public LotusNotesMeetingCaptureService(string password, string mailServer, string mailFile) : this(() => new LNLoginData() { Password = password, RememberMe = false }, mailServer, mailFile) { }

		public LotusNotesMeetingCaptureService(Func<LNLoginData> loginDataFactory) : this(loginDataFactory, "", "") { }

		private LotusNotesMeetingCaptureService(Func<LNLoginData> loginDataFactory, string mailServer, string mailFile)
		{
			this.mailServer = mailServer;
			this.mailFile = mailFile;
			this.loginDataFactory = loginDataFactory;
		}

		public void Initialize()
		{
			if (!IsLotusNotesInstalled)
			{
				log.Info("Lotus Notes is not installed.");
				return;
			}

			processCoordinator.Start();
			processCoordinator.ProcessRestarted += OnProcessRestarted;

			log.Info("Version info - " + GetVersionInfo());
		}

		public string GetVersionInfo()
		{
			if (!EnsureLoggedIn()) return "";

			try
			{
				using (var client = new LotusNotesMeetingCaptureClientWrapper())
				{
					return client.Client.GetVersionInfo();
				}
			}
			catch (Exception e)
			{
				WcfExceptionLogger.LogWcfError("get version info", log, e);
				HandleError(e);
				return "";
			}
		}

		public string[] ProcessNames => new[] { "notes2", "notes", "nlnotes" };

		public List<FinishedMeetingEntry> CaptureMeetings(IList<string> calendarAccountEmails, DateTime startDate, DateTime endDate)
		{
			if (!EnsureLoggedIn()) return emptyFinishedMeetings;

			try
			{
				using (var client = new LotusNotesMeetingCaptureClientWrapper())
				{
					var finishedMeetings = client.Client.CaptureMeetings(calendarAccountEmails.ToList(), startDate, endDate);
					return MeetingDataMapper.To(finishedMeetings);
				}
			}
			catch (Exception e)
			{
				WcfExceptionLogger.LogWcfError("capture meetings", log, e);
				HandleError(e);
				return emptyFinishedMeetings;
			}
		}

		public void Dispose()
		{
			log.Info("Stopping service");
			if (!IsLotusNotesInstalled) return;
			processCoordinator.ProcessRestarted -= OnProcessRestarted;
			processCoordinator.Stop();
			loggedIn = false;
		}

		private bool EnsureLoggedIn()
		{
			if (!IsLotusNotesInstalled) return false;

			if (!loggedIn && !loginCancelled)
			{
				try
				{
					var loginData = GetLoginData();

					using (var client = new LotusNotesMeetingCaptureClientWrapper())
					{
						client.Client.Initialize(loginData.Password, mailServer, mailFile);
						log.Info("Login succeded.");
					}

					if (loginData.RememberMe) LNLoginData.SavePassword(loginData.Password);
					else LNLoginData.DeletePassword();

					loggedIn = true;
				}
				catch (Exception e)
				{
					WcfExceptionLogger.LogWcfError("initialize", log, e);
					HandleError(e);
				}
			}

			return loggedIn;
		}

		private void HandleError(Exception e)
		{
			if (e is FaultException && e.Message == "NotInitialized")
			{
				loggedIn = false;
			}

			//Wrong password
			if (e is FaultException && e.Message == "Wrong Password")
			{
				LNLoginData.DeletePassword();
				loggedIn = false;
			}

			//Endpoint not found
			if (e is EndpointNotFoundException && e.InnerException is PipeException
				&& (uint)((PipeException)e.InnerException).ErrorCode == 0x80131620)
			{
				processCoordinator.RestartIfNeeded();
				loggedIn = false;
			}
		}

		private void OnProcessRestarted(object sender, EventArgs eventArgs)
		{
			loggedIn = false;
		}

		private LNLoginData GetLoginData()
		{
			var password = LNLoginData.LoadPassword();
			var loginData = !String.IsNullOrEmpty(password)
				? new LNLoginData() { Password = password, RememberMe = true }
				: loginCancelled ? null : loginDataFactory();
			if (!loginCancelled && loginData == null)
			{
				log.Info("Lotus Notes login has been cancelled.");
				loginCancelled = true;
			}
			return loginData;
		}

		public class LNLoginData
		{
			private const string passwordPath = "LNPassword";

			public string Password { get; set; }
			public bool RememberMe { get; set; }

			public static void SavePassword(string password)
			{
				ProtectedDataSerializationHelper.Save(passwordPath, password);
			}

			public static void DeletePassword()
			{
				if (ProtectedDataSerializationHelper.Exists(passwordPath)) ProtectedDataSerializationHelper.Delete(passwordPath);
			}

			public static string LoadPassword()
			{
				string password = null;
				if (ProtectedDataSerializationHelper.Exists(passwordPath))
				{
					ProtectedDataSerializationHelper.Load(passwordPath, out password);
				}
				return password;
			}
		}
	}
}
