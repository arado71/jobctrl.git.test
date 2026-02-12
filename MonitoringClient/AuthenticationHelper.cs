using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonitoringClient
{
	public class AuthenticationHelper
	{
		private static readonly Func<HashAlgorithm> hashAlgorithmFactory;

		static AuthenticationHelper()
		{
			hashAlgorithmFactory = () => new SHA256Managed();
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
