using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;

namespace MobileActivityClient
{
	public static class AuthenticationHelper
	{
		private static readonly Func<HashAlgorithm> hashAlgorithmFactory;

		static AuthenticationHelper()
		{
			try
			{
				new SHA256CryptoServiceProvider();
				hashAlgorithmFactory = () => new SHA256CryptoServiceProvider();
			}
			catch (PlatformNotSupportedException)
			{
				hashAlgorithmFactory = () => new SHA256Managed();
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
					&& ex.InnerException.Message == "Invalid user or password");
		}

		public static bool IsActiveUserOnlyException(Exception ex)
		{
			return (ex is MessageSecurityException
					&& ex.InnerException is FaultException
					&& ex.InnerException.Message == "User is not active");
		}
	}
}
