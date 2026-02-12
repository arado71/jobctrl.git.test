using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace Tct.ActivityRecorderService.ActiveDirectoryIntegration
{
	public class ClientLoginTicketAuthenticator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int clientLoginTicketMaxAge = 60 * 60 * 1000 * ConfigManager.ClientLoginTicketMaxAgeInHour;
		public static readonly string ClientLoginTicketPrefix = "ad_";
		private static readonly int clientLoginTicketLength = GetTicketFrom(Guid.Empty).Length;

		private readonly object thisLock = new object();
		private static readonly Dictionary<string, ClientLoginTicket> clientLoginTickets = new Dictionary<string, ClientLoginTicket>();

		public ClientLoginTicket GetNewTicketForUser(int userId)
		{
			var result = new ClientLoginTicket() { UserId = userId, Ticket = GetTicketFrom(Guid.NewGuid()), ExpirationDate = DateTime.UtcNow.AddMilliseconds(clientLoginTicketMaxAge) };
			lock (thisLock)
			{
				clientLoginTickets[result.Ticket] = result;
			}
			return result;
		}

		public bool IsClientLoginTicket(string userName, string password)
		{
			return userName != null && password != null && password.Length == clientLoginTicketLength
				   && password.StartsWith(ClientLoginTicketPrefix);
		}

		public AuthenticationManager.AuthenticationResponse TryValidate(string userName, string password, out int userId)
		{
			if (!int.TryParse(userName, out userId))
			{
				log.Warn("Invalid userName " + userName);
				return AuthenticationManager.AuthenticationResponse.Denied;
			}
			var result = TryValidate(userId, password);
			if (result == AuthenticationManager.AuthenticationResponse.Denied)
			{
				log.Warn("Incorrect client login ticket for user " + userName);
			}
			return result;
		}

		private AuthenticationManager.AuthenticationResponse TryValidate(int userId, string ticket)
		{
			ClientLoginTicket clientLoginTicket;
			lock (thisLock)
			{
				if (!clientLoginTickets.TryGetValue(ticket, out clientLoginTicket)) return AuthenticationManager.AuthenticationResponse.Denied;
			}
			Debug.Assert(clientLoginTicket.Ticket == ticket);
			if (clientLoginTicket.UserId == userId
				&& DateTime.UtcNow < clientLoginTicket.ExpirationDate)
			{
				return AuthenticationManager.AuthenticationResponse.Successful;
			}
			return AuthenticationManager.AuthenticationResponse.Denied;
		}

		public void RemoveExpiredTickets()
		{
			lock (thisLock)
			{
				var keysToRemove = new List<string>();
				foreach (var ticket in clientLoginTickets)
				{
					if (DateTime.UtcNow >= ticket.Value.ExpirationDate)
					{
						keysToRemove.Add(ticket.Key);
					}
				}
				foreach (var toRemove in keysToRemove)
				{
					clientLoginTickets.Remove(toRemove);
				}
				log.Info("Removed " + keysToRemove.Count + " unused client login tickets");
			}
		}

		private static string GetTicketFrom(Guid guid)
		{
			return ClientLoginTicketPrefix + guid.ToString("D");
		}
	}
}
