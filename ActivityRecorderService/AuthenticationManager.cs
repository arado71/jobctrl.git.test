using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderService.ActiveDirectoryIntegration;
using Tct.ActivityRecorderService.Caching;

namespace Tct.ActivityRecorderService
{
	public class AuthenticationManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int cacheMaxAge = ConfigManager.AuthCacheMaxAgeInSec * 1000;
		private static readonly int cacheMaxAgeThreshold = ConfigManager.AuthCacheMaxAgeThresholdInSec * 1000;
		private static readonly int cachePasswordExpiryMaxAge = ConfigManager.AuthPasswordExpiryCacheMaxAgeInSec * 1000;
		private static readonly Func<HashAlgorithm> hashAlgorithmFactory;

		private static readonly ThreadSafeCachedFunc<string, int?> userIdLookup = new ThreadSafeCachedFunc<string, int?>(GetUserIdByEmail, TimeSpan.FromHours(1));

		private readonly object thisLock = new object();
		private readonly Dictionary<string, AuthToken> cachedUserPasswords = new Dictionary<string, AuthToken>();
		private readonly Dictionary<int, PasswordExpiryData> cachedUserPasswordExpiries = new Dictionary<int, PasswordExpiryData>();

		private static readonly string emptyPasswordHash = "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855";
		public static readonly string WebsiteUserPrefix = "website_";

		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public enum AuthenticationMethod
		{
			UserId,
			Email,
			ActiveDirectory,
			Website,
		}

		public class Token
		{
			public string UserName { get; set; }
			public int UserId { get; set; }
			public AuthenticationMethod AuthenticationMethod { get; set; }
		}

		private class AuthToken : Token
		{
			public string Password { get; set; }
			public int Created { get; set; }
			public bool IsBeingRefreshed { get; set; }

			public Token GetToken()
			{
				return new Token { UserName = UserName, UserId = UserId, AuthenticationMethod = AuthenticationMethod };
			}
		}

		private static readonly ClientLoginTicketAuthenticator clientLoginTicketAuthenticator = new ClientLoginTicketAuthenticator();

		static AuthenticationManager()
		{
			try
			{
				new SHA256CryptoServiceProvider();
				hashAlgorithmFactory = () => new SHA256CryptoServiceProvider();
				log.Info("Using SHA256CryptoServiceProvider");
			}
			catch (PlatformNotSupportedException)
			{
				hashAlgorithmFactory = () => new SHA256Managed();
				log.Info("Using SHA256Managed");
			}
		}

		public AuthenticationManager()
			: base(log)
		{
			ManagerCallbackInterval = 3600000; //Clear unused data every hour
		}

		public ClientLoginTicket GetNewTicketForUser(int userId)
		{
			return clientLoginTicketAuthenticator.GetNewTicketForUser(userId);
		}

		public static int? GetUserIdCached(string userName)
		{
			int userId;
			return int.TryParse(userName, out userId) ? userId : userIdLookup.GetOrCalculateValue(userName);
		}

		private static int? GetUserIdByEmail(string userName)
		{
			if (userName == null) return null;
			try
			{
				if (Regex.IsMatch(userName, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
				{
					using (var context = new JobControlDataClassesDataContext())
					{
						return context.GetUserId(userName);
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				log.Error("Failed to fetch userid for " + userName, ex);
				return null;
			}
		}

		private static AuthenticationResponse TryValidate(string userName, string password, out int userId, out AuthenticationMethod method)
		{
			if (userName != null && Regex.IsMatch(userName, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")) // email auth
			{
				method = AuthenticationMethod.Email;
				var dbResult = TryValidateInDb(userName, password, out userId);
				if (dbResult == AuthenticationResponse.Denied)
				{
					log.Warn("Failed login for user " + userName);
				}
				return dbResult;
			}

			if (userName != null && userName.StartsWith(WebsiteUserPrefix)) //ticket auth
			{
				method = AuthenticationMethod.Website;
				Guid ticket;
				if (!int.TryParse(userName.Substring(WebsiteUserPrefix.Length), out userId))
				{
					log.Warn("Invalid website userName " + userName);
					return AuthenticationResponse.Denied;
				}
				if (!Guid.TryParse(password, out ticket))
				{
					log.Warn("Invalid password ticket format");
					return AuthenticationResponse.Denied;
				}
				var dbResult = TryValidateTicketInDb(userId, ticket);
				if (dbResult == AuthenticationResponse.Denied)
				{
					log.Warn("Incorrect password ticket for user " + userName);
				}
				return dbResult;
			}
			else //sha256 password auth
			{
				method = AuthenticationMethod.UserId;
				if (!int.TryParse(userName, out userId))
				{
					log.Warn("Invalid userName " + userName);
					return AuthenticationResponse.Denied;
				}
				var dbResult = TryValidateInDb(userId, password);
				if (dbResult == AuthenticationResponse.Denied)
				{
					log.Warn("Incorrect password for user " + userName);
				}
				return dbResult;
			}
		}

		/// <summary>
		/// Validates whether the password is expired. Call this only after the password is validated!
		/// </summary>
		/// <param name="userName"></param>
		/// <returns>Returns True if the password is not expired.</returns>
		private bool ValidatePasswordExpiry(int userId)
		{
			var sw = Stopwatch.StartNew();
			lock (cachedUserPasswordExpiries)
			{
				if (cachedUserPasswordExpiries.TryGetValue(userId, out PasswordExpiryData expiryData))
				{
					if (!expiryData.ShouldValidate) { return true; }
					var age = (uint)(Environment.TickCount - expiryData.Created);
					if (age < cachePasswordExpiryMaxAge)
					{
						return DateTime.UtcNow < expiryData.ExpiryTime;
					}
				}
			}
			try
			{
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					var passwordExpiry = context.GetPasswordExpiry(userId);
					if (passwordExpiry == null)
					{
						lock (cachedUserPasswordExpiries)
						{
							cachedUserPasswordExpiries[userId] = new PasswordExpiryData { ShouldValidate = false, Created = Environment.TickCount };
						}
						return true;
					}
					else
					{
						lock (cachedUserPasswordExpiries)
						{
							cachedUserPasswordExpiries[userId] = new PasswordExpiryData { ShouldValidate = true, Created = Environment.TickCount, ExpiryTime = passwordExpiry.Value };
						}
						return DateTime.UtcNow < passwordExpiry.Value;
					}
				}
			}
			catch (Exception ex)
			{
				userId = 0;
				log.Error("Password expiry checking in DB for user " + userId + " failed (Unknown)", ex);
				return true;
			}
			finally
			{
				log.Debug("Password expiry checking in DB for user " + userId + " finished in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
			}

		}
		
		public AuthenticationResponse TryValidateWithCache(string userName, string password, out Token token)
		{
			//block empty password
			if (emptyPasswordHash.Equals(password))
			{
				log.Debug("Empty password is blocked");
				token = null;
				return AuthenticationResponse.Denied;
			}

			int userId;
			if (clientLoginTicketAuthenticator.IsClientLoginTicket(userName, password)) //Active Directory login
			{
				var res = clientLoginTicketAuthenticator.TryValidate(userName, password, out userId);
				token = new Token { UserName = userName, AuthenticationMethod = AuthenticationMethod.ActiveDirectory, UserId = userId };
				return res;
			}

			AuthToken authToken;
			lock (thisLock)
			{
				if (cachedUserPasswords.TryGetValue(userName, out authToken))
				{
					Debug.Assert(authToken.UserName == userName);
					if (authToken.Password == password)
					{
						token = authToken.GetToken();
						var age = (uint)(Environment.TickCount - authToken.Created);
						if (age < cacheMaxAge)
						{
							if (!ValidatePasswordExpiry(authToken.UserId)) return AuthenticationResponse.PasswordExpired;
							return AuthenticationResponse.Successful;
						}
						if (age < cacheMaxAge + cacheMaxAgeThreshold)
						{
							if (!authToken.IsBeingRefreshed) //avoid DB contention when cache is invalidated (YAGNI?)
							{
								authToken.IsBeingRefreshed = true; //first request after cacheMaxAge will use the DB
							}
							else
							{
								if (!ValidatePasswordExpiry(authToken.UserId)) return AuthenticationResponse.PasswordExpired;
								return AuthenticationResponse.Successful; //other requests within cacheMaxAge + cacheMaxAgeThreshold will use the cache
							}
						}
					}
				}
			}
			AuthenticationMethod method;
			var result = TryValidate(userName, password, out userId, out method);
			token = null;
			if (result == AuthenticationResponse.Successful)
			{
				if (!ValidatePasswordExpiry(userId)) result = AuthenticationResponse.PasswordExpired;
				else
				{
					authToken = new AuthToken() { UserName = userName, Password = password, Created = Environment.TickCount, UserId = userId, AuthenticationMethod = method };
					token = authToken.GetToken();
					lock (thisLock) //we don't care about the race here (i.e. overwriting newer data)
					{
						cachedUserPasswords[userName] = authToken;
					}
				}
			}
			else if (result == AuthenticationResponse.Denied)
			{
				//delay when authentication is denied
				//Thread.Sleep(2000); //todo figure out something, but we cannot use Thread.Sleep as it would block all other requests...
			}
			return result;
		}

		private static AuthenticationResponse TryValidateInDb(string email, string password, out int userId)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				using (var context = new JobControlDataClassesDataContext())
				{
					var userIdQuery = context.GetUserId(email);
					if (userIdQuery == null)
					{
						userId = 0;
						log.Debug("Authentication in DB for user " + email + " failed, email not found");
						return AuthenticationResponse.Denied;
					}

					userId = userIdQuery.Value;
					var salt = context.GetSalt(userId);
					if (salt == null) //user might be deleted then GetSalt would return null (Salt is a nvarchar(24) not null column in DB)
					{
						log.Warn("Authentication in DB for user " + email + " failed (Denied) No Salt");
						return AuthenticationResponse.Denied;
					}
					var clear = Encoding.UTF8.GetBytes(salt + password);
					using (var hashProvider = hashAlgorithmFactory())
					{
						var hashed = hashProvider.ComputeHash(clear);
						var hashedStr = BitConverter.ToString(hashed).Replace("-", string.Empty);
						return context.Validate(userId, hashedStr) ? AuthenticationResponse.Successful : AuthenticationResponse.Denied;
					}
				}
			}
			catch (Exception ex)
			{
				userId = 0;
				log.Error("Authentication in DB for user " + email + " failed (Unknown)", ex);
				return AuthenticationResponse.Unknown;
			}
			finally
			{
				log.Debug("Authentication in DB for user " + email + " finished in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
			}
		}

		private static AuthenticationResponse TryValidateInDb(int userId, string password)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				using (var context = new JobControlDataClassesDataContext())
				{
					var salt = context.GetSalt(userId);
					if (salt == null) //user might be deleted then GetSalt would return null (Salt is a nvarchar(24) not null column in DB)
					{
						log.Warn("Authentication in DB for user " + userId + " failed (Denied) No Salt");
						return AuthenticationResponse.Denied;
					}
					var clear = Encoding.UTF8.GetBytes(salt + password);
					using (var hashProvider = hashAlgorithmFactory())
					{
						var hashed = hashProvider.ComputeHash(clear);
						var hashedStr = BitConverter.ToString(hashed).Replace("-", string.Empty);
						return context.Validate(userId, hashedStr) ? AuthenticationResponse.Successful : AuthenticationResponse.Denied;
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Authentication in DB for user " + userId + " failed (Unknown)", ex);
				return AuthenticationResponse.Unknown;
			}
			finally
			{
				log.Debug("Authentication in DB for user " + userId + " finished in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
			}
		}

		private static AuthenticationResponse TryValidateTicketInDb(int userId, Guid ticket)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				using (var context = new JobControlDataClassesDataContext())
				{
					return context.ValidateTicket(userId, ticket) ? AuthenticationResponse.Successful : AuthenticationResponse.Denied;
				}
			}
			catch (Exception ex)
			{
				log.Error("Authentication with ticket in DB for user " + userId + " failed (Unknown)", ex);
				return AuthenticationResponse.Unknown;
			}
			finally
			{
				log.Debug("Authentication with ticket in DB for user " + userId + " finished in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
			}
		}

		protected override void ManagerCallbackImpl()
		{
			lock (thisLock)
			{
				var keysToRemove = new List<string>();
				foreach (var cachedUserPassword in cachedUserPasswords)
				{
					if (Environment.TickCount - cachedUserPassword.Value.Created >= cacheMaxAge + cacheMaxAgeThreshold)
					{
						keysToRemove.Add(cachedUserPassword.Key);
					}
				}
				foreach (var toRemove in keysToRemove)
				{
					cachedUserPasswords.Remove(toRemove);
				}
				log.Info("Removed " + keysToRemove.Count + " unused cached passwords");
			}
			lock (cachedUserPasswordExpiries)
			{
				var keysToRemove = new List<int>();
				foreach (var cachedUserPasswordExpiry in cachedUserPasswordExpiries)
				{
					if (Environment.TickCount - cachedUserPasswordExpiry.Value.Created >= cachePasswordExpiryMaxAge + cacheMaxAgeThreshold)
					{
						keysToRemove.Add(cachedUserPasswordExpiry.Key);
					}
				}
				foreach (var toRemove in keysToRemove)
				{
					cachedUserPasswordExpiries.Remove(toRemove);
				}
				log.Info("Removed " + keysToRemove.Count + " unused expiry times.");
			}
			clientLoginTicketAuthenticator.RemoveExpiredTickets();
		}

		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public enum AuthenticationResponse
		{
			Unknown = 0,
			Successful,
			Denied,
			PasswordExpired
		}

		private class PasswordExpiryData
		{
			public bool ShouldValidate { get; set; }
			public DateTime ExpiryTime { get; set; }
			public int Created { get; set; }
		}
	}
}
