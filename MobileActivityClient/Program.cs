using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileActivityClient.ActivityMobileServiceReference;

namespace MobileActivityClient
{
	class Program
	{
		static void Main(string[] args)
		{
			int userId = 45234532;
			string password = AuthenticationHelper.GetHashedHexString("topsecret");
			try
			{
				using (var client = WcfClientDisposeHelper.Create(new ActivityMobileClient()))
				{
					client.Client.ClientCredentials.UserName.UserName = userId.ToString();
					client.Client.ClientCredentials.UserName.Password = password;

					string newVersion;
					var menu = client.Client.GetClientMenu(out newVersion, userId, "");
					Console.WriteLine("Done. Got version " + newVersion);
				}
			}
			catch (Exception ex)
			{
				if (AuthenticationHelper.IsInvalidUserOrPasswordException(ex))
				{
					Console.WriteLine("Invalid user or password");
				}
				else if (AuthenticationHelper.IsActiveUserOnlyException(ex))
				{
					Console.WriteLine("User is not active");
				}
				else
				{
					Console.WriteLine("Error: " + ex.Message);
				}
			}
			Console.ReadKey(true);
		}
	}
}
