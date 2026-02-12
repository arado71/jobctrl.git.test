using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Configuration;

namespace Tct.ActivityRecorderClient.ActiveDirectoryIntegration
{
	class ActiveDirectoryAuthenticationManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int callbackInterval = 10 * 60 * 1000;  //10 mins
		private const int callbackRetryInterval = 30 * 1000;  //30 secs
		private static readonly TimeSpan refreshBeforeExpiration = TimeSpan.FromMinutes(20);
		private bool retryShortly;
		private const int connectionCheckInterval = 5 * 60 * 1000;
		private static int lastConnectionCheck = Environment.TickCount - connectionCheckInterval;

		public event EventHandler UserIdChanged;

		public ActiveDirectoryAuthenticationManager()
			: base(log)
		{
		}

		public static bool CanStart() //if we have an AD endpoint and user is not logged in via LoginForm
		{
			return ActiveDirectoryLoginServiceClientWrapper.IsActiveDirectoryAuthEnabled && ConfigManager.UserPasswordExpirationDate.HasValue;
		}

		public static ConfigManager.LoginData LoginWithWindowsUser(Func<ConfigManager.LoginData> fallbackLoginDataFactory)
		{
			ClientLoginTicket ticket;
			var success = TryGetNewTicket(false, out ticket);
			if (!success || ticket == null)
			{
				log.Info(!success
					? "LoginWithWindowsUser failed."
					: string.Format("Current windows user: {0} does not mapped to any JC user.", GetCurrentUserSid()));
				return fallbackLoginDataFactory();
			}

			log.InfoFormat("Current windows user: {0} mapped to JC user: {1}", GetCurrentUserSid(), ticket.UserId);

			AuthData authData;
			string detailedErrorText;
			var authResult = AuthenticationHelper.TryAuthenticate(ticket.UserId.ToString(CultureInfo.InvariantCulture), ticket.Ticket, out authData, out detailedErrorText);

			if (authResult != AuthenticationHelper.AuthenticationResponse.Successful)	//Unknown, Denied, NotActive
			{
				log.Info("Unsuccesful authentication: " + authResult);
			}

			return new ConfigManager.LoginData()
			{
				UserId = ticket.UserId,
				UserPassword = ticket.Ticket,
				UserPasswordExpirationDate = ticket.ExpirationDate,
				AuthData = authData,
				RememberMe = true
			};
		}

		public ConfigManager.LoginData RefreshClientLoginTicket(bool throwCommException = false)
		{
			ClientLoginTicket ticket;
			if (!TryGetNewTicket(throwCommException, out ticket)) return null;

			if (ticket == null)
			{
				log.WarnFormat("Current windows user ({0}) previously was mapped to JC user {1}, but now does not map to any JC user.", GetCurrentUserSid(), ConfigManager.UserId);
				return null;
			}
			if (ticket.UserId != ConfigManager.UserId)
			{
				log.ErrorFormat("Current windows user ({0}) mapped to different JC user ({1}) than previously ({2}).", GetCurrentUserSid(), ticket.UserId, ConfigManager.UserId);
				OnUserIdChanged();
				return null;
			}

			log.Info("Client login ticket refreshed successfully. New ticket valid until " + ticket.ExpirationDate);
			return new ConfigManager.LoginData()
			{
				UserId = ticket.UserId,
				UserPassword = ticket.Ticket,
				UserPasswordExpirationDate = ticket.ExpirationDate,
				RememberMe = true
			};
		}

		protected override void ManagerCallbackImpl()
		{
			Debug.Assert(ConfigManager.UserPasswordExpirationDate != null);
			if (DateTime.UtcNow < ConfigManager.UserPasswordExpirationDate.Value.Subtract(refreshBeforeExpiration)) return;

			var loginData = RefreshClientLoginTicket();
			retryShortly = loginData == null;
			ConfigManager.RefreshPasswordTo(loginData);
		}

		protected override int ManagerCallbackInterval
		{
			get { return retryShortly ? callbackRetryInterval : callbackInterval; }
		}

		private static bool TryGetNewTicket(bool throwCommException, out ClientLoginTicket ticket)
		{
			foreach (var endpointConfig in AppConfig.Current.ActiveDirectoryEndpointConfigurations.Values.OrderBy(e => e.Order).ToList())
			{
				try
				{
					using (var client = new ActiveDirectoryLoginServiceClientWrapper(endpointConfig))
					{
						ticket = client.Client.GetClientLoginTicket();
						return true;
					}
				}
				catch (CommunicationException ex)
				{
					if (throwCommException) throw;
					log.Error("GetNewTicket failed. (" + endpointConfig + ")", ex);
					if (Environment.TickCount - lastConnectionCheck > connectionCheckInterval)
					{
						lastConnectionCheck = Environment.TickCount;
						try
						{
							var result = endpointConfig.CheckConnection();
							if (result != null)
								log.Warn("Check connection result: " + result);
						}
						catch (Exception ex2)
						{
							log.Error("Check connection failed", ex2);
						}
					}
				}
				catch (Exception ex)
				{
					log.Error("GetNewTicket failed. (" + endpointConfig + ")", ex);
				}
			}
			ticket = null;
			return false;
		}

		private static string GetCurrentUserSid()
		{
			try
			{
				var identity = WindowsIdentity.GetCurrent();
				return (identity != null && identity.User != null)
					? identity.User.Value
					: "";
			}
			catch (Exception ex)
			{
				log.Error("Get SID of current user failed.", ex);
			}
			return "";
		}

		private void OnUserIdChanged()
		{
			var del = UserIdChanged;
			if (del != null) del(this, EventArgs.Empty);
		}
	}
}
