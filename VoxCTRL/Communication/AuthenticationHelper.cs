using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using VoxCTRL.ActivityRecorderServiceReference;
using log4net;
using VoxCTRL.Serialization;

namespace VoxCTRL.Communication
{
	public static class AuthenticationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Func<HashAlgorithm> hashAlgorithmFactory;
		private const string FilePath = "EmailLookup";
		private static Dictionary<string, int> emailUserIdLookup;

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
			return ConfigManager.Version.ToString();
		}

		public static AuthenticationResponse TryAuthenticate(string userId, string password, out AuthData authData)
		{
			try
			{
				using (var client = new ActivityRecorderClientWrapper())
				{
					client.Client.ClientCredentials.UserName.UserName = userId;
					client.Client.ClientCredentials.UserName.Password = password;
					client.Client.Endpoint.Binding.OpenTimeout = TimeSpan.FromSeconds(10);
					client.Client.Endpoint.Binding.SendTimeout = TimeSpan.FromSeconds(10);
					client.Client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromSeconds(10);
					client.Client.OperationTimeout = TimeSpan.FromSeconds(10);
					authData = client.Client.Authenticate(ConfigManager.ApplicationName + " " + ConfigManager.Version);
					log.Info("Authentication Successful for user " + userId);
					return AuthenticationResponse.Successful;
				}
			}
			catch (Exception ex)
			{
				authData = null;
				if (IsInvalidUserOrPasswordException(ex))
				{
					log.Info("Authentication Denied for user " + userId);
					return AuthenticationResponse.Denied;
				}
				if (IsActiveUserOnlyException(ex))
				{
					log.Info("Authentication NotActive for user " + userId);
					return AuthenticationResponse.NotActive;
				}
				log.Info("Authentication Unknown for user " + userId, ex);
				return AuthenticationResponse.Unknown;
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

		public enum AuthenticationResponse
		{
			Unknown = 0,
			Successful,
			Denied,
			NotActive,
		}

		//private static readonly TimeSpan authTicketTimeout = TimeSpan.FromSeconds(10);
		//public static string GetAuthTicket()
		//{
		//    try
		//    {
		//        using (var client = new ActivityRecorderClientWrapper())
		//        {
		//            client.Client.Endpoint.Binding.OpenTimeout = authTicketTimeout;
		//            client.Client.Endpoint.Binding.SendTimeout = authTicketTimeout;
		//            client.Client.Endpoint.Binding.ReceiveTimeout = authTicketTimeout;
		//            client.Client.OperationTimeout = authTicketTimeout;

		//            return client.Client.GetAuthTicket(ConfigManager.UserId);
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        log.Error("Unable to get auth ticket", ex);
		//        return null;
		//    }
		//}
	}
}
