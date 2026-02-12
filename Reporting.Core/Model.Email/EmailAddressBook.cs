using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Processing;

namespace Reporter.Model.Email
{
	internal class EmailAddressBook
	{
		private readonly Dictionary<int, IEmailUser> databaseUsers = new Dictionary<int, IEmailUser>();
		private readonly AutoDictionary<string, List<IEmailUser>> userLookup = new AutoDictionary<string, List<IEmailUser>>(() => new List<IEmailUser>());
		private readonly HashSet<string> resolved = new HashSet<string>();

		public List<IEmailUser> Users
		{
			get
			{
				return userLookup.SelectMany(x => x.Value).Distinct().ToList();
			}
		}

		public EmailAddressBook(IEmailDbContext context, int[] userIds)
		{
			// Try to resolve from db
			databaseUsers = context.GetUsers(userIds).ToDictionary(x => x.Id.Value, y => y);
			foreach (var address in databaseUsers.Values)
			{
				Debug.WriteLine("Resolved {1} to user {0} (db)", address.Id, address.Email);
				userLookup[address.Email].Add(address);
				resolved.Add(address.Email);
			}
		}

		public void GuessAddresses(List<Email> emails)
		{
			GuessByMostSeenAddress(emails);
			GuessByOnlyUnknownAddress(emails);
		}

		public List<IEmailUser> GetOrAddUser(string address)
		{
			var list = userLookup[address];
			if (list.Count == 0)
			{
				list.Add(new EmailUser { Email = address });
			}

			return list;
		}

		public List<IEmailUser> GetUsers(string address)
		{
			return userLookup[address];
		}

		private void AddMatch(string address, int userId)
		{
			IEmailUser user;
			if (databaseUsers.TryGetValue(userId, out user))
			{
				userLookup[address].Add(user);
				resolved.Add(address);
			}
			else
			{
				Debug.WriteLine("Unable to get user {0} from database", user);
			}
		}

		private void GuessByMostSeenAddress(List<Email> emails)
		{
			var emailCandidates = new AutoDictionary<int, AutoDictionary<string, int>>(() => new AutoDictionary<string, int>());
			foreach (var email in emails)
			{
				foreach (var address in email.GetAddresses())
				{
					emailCandidates[email.User][address] += 1;
				}
			}
			foreach (var item in emailCandidates)
			{
				var ordered = item.Value.OrderByDescending(x => Tuple.Create(x.Value, x.Key)).ToList();
				if (ordered.Count == 0) continue;
				if (ordered.Count > 1 && ordered[0].Value == ordered[1].Value) Debug.WriteLine("Close match found!");
				if (resolved.Contains(ordered[0].Key))
				{
					// Already resolved
					Debug.WriteLine("Email address {0} already resolved {1}", ordered[0].Key, userLookup[ordered[0].Key].Any(x => x.Id == item.Key) ? "correctly" : "differently");
					continue;
				}
				Debug.WriteLine("Resolved {1} to user {0} (most)", item.Key, ordered[0].Key);
				AddMatch(ordered[0].Key, item.Key);
			}
		}

		private void GuessByOnlyUnknownAddress(List<Email> emails)
		{
			bool newFound;
			// Try to resolve new by guessing known sender
			do
			{
				newFound = false;
				foreach (var email in emails)
				{
					if (resolved.Contains(email.From) // Sender is already resolved
						&& email.To.Length == 1 // Has only one to/cc field
						&& userLookup[email.To[0]].All(x => x.Id != email.User) // ...which isn't the current user
						&& userLookup[email.From].All(x => x.Id != email.User) // the sender is not the current user
						&& email.Span > TimeSpan.FromSeconds(1)) // we might not be able to read the values correctly for a single capture
					{
						Debug.WriteLine("Resolved {1} to user {0} (only) based on {2} -> {1}", email.User, email.To[0], email.From);
						AddMatch(email.To[0], email.User);
						newFound = true;
					}
					// Is email resolved? 

				}
			} while (newFound);
		}
	}
}
