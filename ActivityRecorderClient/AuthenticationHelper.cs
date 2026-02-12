using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using log4net;
using log4net.Core;
using Newtonsoft.Json;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Configuration;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient
{
	public static class AuthenticationHelper
	{
		private static readonly Type thisType = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Func<HashAlgorithm> hashAlgorithmFactory;
		private static readonly TimeSpan shortTimeout = TimeSpan.FromSeconds(10);
		private const string FilePath = "EmailLookup";

		private static Dictionary<string, int> emailUserIdLookup;

		private static bool isAuthenticated;

		static AuthenticationHelper()
		{
			try
			{
				new SHA256CryptoServiceProvider();
				hashAlgorithmFactory = () => new SHA256CryptoServiceProvider();
				log.Info(ConfigManager.ApplicationName + " is using SHA256CryptoServiceProvider");
			}
			catch (PlatformNotSupportedException)
			{
				hashAlgorithmFactory = () => new SHA256Managed();
				log.Info(ConfigManager.ApplicationName + " is using SHA256Managed");
			}

			LoadEmailUserIdLookup();
		}

		private static void LoadEmailUserIdLookup()
		{
			try
			{
				if (!IsolatedStorageSerializationHelper.Exists(FilePath) || !IsolatedStorageSerializationHelper.Load(FilePath, out emailUserIdLookup))
				{
					emailUserIdLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				}
			}
			catch (Exception ex)
			{
				log.Warn("Failed to load offline email lookup", ex);
			}
		}

		public static void UpdateEmail(string email, int userId)
		{
			emailUserIdLookup[email] = userId;
			IsolatedStorageSerializationHelper.Save(FilePath, emailUserIdLookup);
		}

		public static int? GetUserId(string email)
		{
			int result;
			return emailUserIdLookup.TryGetValue(email, out result) ? (int?)result : null;
		}

		public static string GetClientInfo()
		{
			return ""; //we don't want to send anything atm.
		}

		public static AuthenticationResponse TryAuthenticate(string username, string password, out AuthData authData, out string detailedError)
		{
			return TryAuthenticate(false, username, password, out authData, out detailedError);
		}

		public static void DetectPreferredEndpoint(string username, string password)
		{
			TryAuthenticate(true, username, password, out var authData, out _);
			ConfigManager.SetAndSaveAuthDataIfApplicable(authData);
		}

		private static readonly bool[] useShortTimeouts = new[] { true, false };
		private static readonly bool[] dontUseShortTimeouts = new[] { false };
		//it is important that we try all possible endpoints when trying to authenticate
		//When we are not trying to detect endpoints we try with short timeout first (with the preferred endpoint first then the others)
		//if all those fail we will fall back to default timeout and try again
		//When we are detecting endpoints we use the order defined by ActivityRecorderClientWrapper.EndpointNames with default timeout
		//this function is a mess...
		private static AuthenticationResponse TryAuthenticate(bool detectPreferredEndpoint, string username, string password, out AuthData authData, out string detailedError)
		{
			var logLevel = detectPreferredEndpoint ? Level.Debug : Level.Info;
			var prefEndpoint = detectPreferredEndpoint ? null : ActivityRecorderClientWrapper.PreferredEndpoint;
			var timeouts = detectPreferredEndpoint ? dontUseShortTimeouts : useShortTimeouts;
			var endpointConfigs = prefEndpoint == null
				? AppConfig.Current.ServiceEndpointConfigurations.Values.OrderBy(e => e.Order).ToArray()
				: new[] { prefEndpoint }.Concat(AppConfig.Current.ServiceEndpointConfigurations.Values.OrderBy(e=> e.Order).Where(n => n != prefEndpoint)).ToArray();
			//var f = timeouts.Join(endpointNames, n => true, k => true, (n, k) => new { Endpoint = k, UseShortTimeout = n }).ToArray();
			var locErrors = new List<KeyValuePair<EndpointConfiguration, Exception>>();
			foreach (var useShortTimeout in timeouts)
			{
				foreach (var endpointConfig in endpointConfigs)
				{
					Exception ex;
					ClockSkewData clockData;
					var result = TryAuthenticateWithEndpoint(endpointConfig, useShortTimeout, logLevel, username, password, !isAuthenticated, out authData, out ex);
					if (result == AuthenticationResponse.Successful)
						isAuthenticated = true;
					if (result != AuthenticationResponse.Unknown)
					{
						if (prefEndpoint == null) ActivityRecorderClientWrapper.PreferredEndpoint = endpointConfig; //don't have to set prefEndpoint because we will return
						detailedError = null; //we don't care if there is a working endpoint
						return result;
					}
					if (!useShortTimeout) locErrors.Add(new KeyValuePair<EndpointConfiguration, Exception>(endpointConfig, ex)); //log errors for long timeouts only
					if (ClockSkewHelper.IsClockSkewException(ex, out clockData) && prefEndpoint == null) //the result is unknown but the communication (endpoint) works
					{
						ActivityRecorderClientWrapper.PreferredEndpoint = endpointConfig;
						prefEndpoint = endpointConfig; //don't try to find a better endpoint
					}
				}
			}
			detailedError = GetErrorText(locErrors);
			authData = null;

		    WinApi.DnsFlushResolverCache(); //Flush the dns cache

			return AuthenticationResponse.Unknown;
		}

		private static AuthenticationResponse TryAuthenticateWithEndpoint(EndpointConfiguration endpointConfig, bool useShortTimeout, Level logLevel, string username, string password, bool hasToSendComputerInfo, out AuthData authData, out Exception exc)
		{
			try
			{
				using (var client = new ActivityRecorderClientWrapper(endpointConfig))
				{
					client.Client.ClientCredentials.UserName.UserName = username;
					client.Client.ClientCredentials.UserName.Password = password;
					if (useShortTimeout)
					{
						client.SetTimeout(shortTimeout);
					}

					authData = client.Client.Authenticate(hasToSendComputerInfo ?
						JsonConvert.SerializeObject(new
						{
							ComputerId = ConfigManager.EnvironmentInfo.ComputerId.ToString("x"),
							OSName = string.IsNullOrEmpty(ConfigManager.EnvironmentInfo.OSFullName) ? ConfigManager.EnvironmentInfo.OSVersion.ToString() : ConfigManager.EnvironmentInfo.OSFullName + " (" + ConfigManager.EnvironmentInfo.OSVersion + ")"
						})
						: GetClientInfo());
					log.Logger.Log(thisType, logLevel, "Authentication Successful for user " + username + " (" + endpointConfig + ")" + (useShortTimeout ? " short" : ""), null);
					exc = null;
					return AuthenticationResponse.Successful;
				}
			}
			catch (Exception ex)
			{
				authData = null;
				exc = ex;
				if (IsInvalidUserOrPasswordException(ex))
				{
					log.Logger.Log(thisType, logLevel, "Authentication Denied for user " + username + " (" + endpointConfig + ")" + (useShortTimeout ? " short" : ""), null);
					return AuthenticationResponse.Denied;
				}
				if (IsActiveUserOnlyException(ex))
				{
					log.Logger.Log(thisType, logLevel, "Authentication NotActive for user " + username + " (" + endpointConfig + ")" + (useShortTimeout ? " short" : ""), null);
					return AuthenticationResponse.NotActive;
				}
				if (IsPasswordExpiredException(ex))
				{
					log.Logger.Log(thisType, logLevel, "Authentication PasswordExpired for user " + username + " (" + endpointConfig + ")" + (useShortTimeout ? " short" : ""), null);
					return AuthenticationResponse.PasswordExpired;
				}
				//clockskew error is unknown too
				log.Logger.Log(thisType, logLevel, "Authentication Unknown for user " + username + " (" + endpointConfig + ")" + (useShortTimeout ? " short" : ""), ex);
				return AuthenticationResponse.Unknown;
			}
		}

		private static string GetErrorText(IEnumerable<KeyValuePair<EndpointConfiguration, Exception>> errors)
		{
			if (errors == null) return null;
			var sb = new StringBuilder();
			foreach (var error in errors)
			{
				sb.AppendLine().AppendLine();
				sb.Append(error.Key.Name).Append(": ");
				AppendExceptionMessage(sb, error.Value);
			}
			return sb.ToString();
		}

		private static void AppendExceptionMessage(StringBuilder sb, Exception ex)
		{
			if (ex == null)
			{
				sb.Append("(null)");
				return;
			}
			var curr = ex;
			while (true)
			{
				sb.Append(curr.GetType()).Append(": ").Append(curr.Message);
				curr = curr.InnerException;
				if (curr == null) break;
				sb.Append(" ---> ");
			}
		}

		public static string GetHashedHexString(string clearPassword)
		{
			var clear = Encoding.UTF8.GetBytes(clearPassword);
			using (var hashProvider = hashAlgorithmFactory())
			{
				var hashed = hashProvider.ComputeHash(clear);
				var hashedStr = BitConverter.ToString(hashed).Replace("-", string.Empty);
				return hashedStr;
			}
		}

		public static bool IsInvalidUserOrPasswordException(Exception ex)
		{
			return (ex is MessageSecurityException
					&& ex.InnerException is FaultException
					&& ex.InnerException.Message == "Invalid user or password") //tcp win srv - win client
				|| (ex is MessageSecurityException
					&& ex.InnerException is WebException
					&& ((WebException)ex.InnerException).Status == WebExceptionStatus.ProtocolError
					&& ((WebException)ex.InnerException).Response is HttpWebResponse
					&& ((HttpWebResponse)((WebException)ex.InnerException).Response).StatusCode == HttpStatusCode.Forbidden) //http win srv - win client error
				|| (ex is WebException
					&& ((WebException)ex).Status == WebExceptionStatus.ProtocolError
					&& ((WebException)ex).Response is HttpWebResponse
					&& ((HttpWebResponse)((WebException)ex).Response).StatusCode == HttpStatusCode.Unauthorized) //http mono srv - mono client error
				|| (ex is WebException
					&& ((WebException)ex).Status == WebExceptionStatus.ProtocolError
					&& ((WebException)ex).Response is HttpWebResponse
					&& ((HttpWebResponse)((WebException)ex).Response).StatusCode == HttpStatusCode.Forbidden) //http win srv - mono client error
					;
		}

		public static bool IsActiveUserOnlyException(Exception ex)
		{
			return (ex is MessageSecurityException
					&& ex.InnerException is FaultException
					&& ex.InnerException.Message == "User is not active");
		}

		public static bool IsPasswordExpiredException(Exception ex)
		{
			return (ex is MessageSecurityException
					&& ex.InnerException is FaultException
					&& ex.InnerException.Message == "User password is expired");
		}

		public enum AuthenticationResponse
		{
			Unknown = 0,
			Successful,
			Denied,
			NotActive,
			PasswordExpired
		}

		public static string GetAuthTicket()
		{
			try
			{
				using (var client = new ActivityRecorderClientWrapper(ActivityRecorderClientWrapper.GetPreferredEndpointOrDefault()))
				{
					client.SetTimeout(shortTimeout);
					return client.Client.GetAuthTicket(ConfigManager.UserId);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to get auth ticket", ex);
				return null;
			}
		}
	}
}
