using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using log4net;

namespace PlaybackClient
{
	public static class AuthenticationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Func<HashAlgorithm> hashAlgorithmFactory;

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
	}
}
