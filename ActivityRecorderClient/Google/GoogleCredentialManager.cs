using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using log4net;
using Tct.ActivityRecorderClient.View;
using Google.Apis.Auth.OAuth2.Responses;

namespace Tct.ActivityRecorderClient.Google
{
	static class GoogleCredentialManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly object lockObject = new object();

		internal static UserCredential Credential { get; private set; }
		private static bool credentialInitialized = false;
#if DEBUG && TESTGOOGLESYNC
		private const string clientId = "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com";
		private const string clientSecret = "04985VH_hxB7kREVXR-ofb5C";
#else
		private static readonly string clientId = ConfigManager.GoogleClientId;
		private static readonly string clientSecret = ConfigManager.GoogleClientSecret;
#endif
		private const string NotificationKey = "GoogleCredentialManager";
		private static readonly GoogleCredentialWcfDataStore DataStore = new GoogleCredentialWcfDataStore();

		internal static bool IsCredentialInitializationNeeded()
		{
			return !credentialInitialized;
		}

		public static event EventHandler<SingleValueEventArgs<bool>> OnReportResult; 

		public static void GetNewCredentialsIfNeeded(bool isForCalendar, bool isForDrive)
		{
			log.Debug($"GetNewCredentialsIfNeeded isForCalendar:{isForCalendar} isForDrive:{isForDrive}");
			lock (lockObject)
			{
				Debug.Assert(isForCalendar | isForDrive, "Google credential - Credential needs at least Calendar or Drive permission.");
				if (credentialInitialized) return;
				GetCredentials(isForCalendar, isForDrive);
			}
		}

		private static void GetCredentials(bool isForCalendar, bool isForDrive)
		{
			if (clientId.IsNullOrWhiteSpace() || clientSecret.IsNullOrWhiteSpace())
			{
				log.Debug("No google credentials provided");
				return;
			}
			ClientSecrets secrets = new ClientSecrets
			{
				ClientId = clientId,
				ClientSecret = clientSecret
			};

			List<string> scopesList = new List<string>();
			if (isForDrive) scopesList.Add(DriveService.Scope.DriveReadonly);
			if (isForCalendar)
			{
				scopesList.Add(CalendarService.Scope.CalendarReadonly);
				scopesList.Add(CalendarService.Scope.CalendarEventsReadonly);
			}
			string[] scopes = scopesList.ToArray();

			if(!IsUserAuthenticated(secrets, scopes, DataStore))
			{
				log.Debug("Showing GoogleAuthenticationForm.");
				bool shouldAuthenticate = false;
				Platform.Factory.GetGuiSynchronizationContext().Send(__ =>
				{
					var form = new GoogleAuthenticationForm();
					var formResult = form.ShowDialog();
					if (formResult == System.Windows.Forms.DialogResult.OK) shouldAuthenticate = true;
				}, null);
				if (!shouldAuthenticate)
				{
					log.Debug("The user clicked cancel on the form.");
					return;
				}
			}
			var task = GoogleWebAuthorizationBroker.AuthorizeAsync(
				secrets,
				scopes,
				"user",
				CancellationToken.None,
				DataStore);
			log.Debug("Task prepared, starting...");
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					log.Debug("Starting GoogleWebAuthorizationBroker.AuthorizeAsync");
					Credential = task.Result;
					log.Debug("Finished GoogleWebAuthorizationBroker.AuthorizeAsync");
					log.Debug("Starting GoogleWebAuthorizationBroker.AuthorizeAsync");
					if (Credential != null && isForCalendar)
					{
						var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer()
						{
							ClientSecrets = secrets,
							DataStore = DataStore,
						});
						var cred = flow.LoadTokenAsync(ConfigManager.UserId.ToString(), CancellationToken.None).Result;
						var calserv = new CalendarService(new BaseClientService.Initializer()
						{
							HttpClientInitializer = new UserCredential(flow, ConfigManager.UserId.ToString(), cred),
							ApplicationName = "JobCTRL",
						});
						var req = calserv.Events.List("primary");
						req.TimeMin = DateTime.UtcNow.AddDays(-7);
						req.TimeMax = DateTime.UtcNow;
						req.SingleEvents = true;
						req.ShowDeleted = true;
						try
						{
							req.Execute();
							OnReportResult?.Invoke(null, SingleValueEventArgs.Create(true));
						}
						catch (Exception ex)
						{
							log.Warn("Google OAuth query execution is unsuccessful", ex);
							OnReportResult?.Invoke(null, SingleValueEventArgs.Create(false));
							return;
						}
					}
					DataStore.StoreOnServer();
					log.Debug("Google OAuth token stored successfully");
					lock (lockObject)
					{
						if (Credential != null) credentialInitialized = true;
					}
				}
				catch (Exception ex)
				{
					if (!(ex.InnerException is TaskCanceledException)) log.Error("Unexpected error during AuthorizeAsync task", ex);
					else log.Debug("Task cancelled");
				}
			});

		}

		public static bool IsUserAuthenticated(ClientSecrets secrets, IEnumerable<string> scopes, IDataStore dataStore)
		{
			object token = DataStore.GetAsync<object>(null).Result;
			return token != null;
		}
	}
}
