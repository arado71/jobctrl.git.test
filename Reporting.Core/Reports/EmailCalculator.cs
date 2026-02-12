using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Model;
using Reporter.Model.Email;
using Reporter.Model.ProcessedItems;
using Reporter.Processing;

namespace Reporter.Reports
{
	internal class EmailCalculator
	{
		private readonly EmailAddressBook addressBook;
		private readonly AutoDictionary<Tuple<IEmailUser, IEmailUser, bool>, TimeSpan> aggregateSpans = new AutoDictionary<Tuple<IEmailUser, IEmailUser, bool>, TimeSpan>(() => TimeSpan.Zero);

		public Dictionary<Tuple<IEmailUser, IEmailUser>, TimeSpan> Outbound { 
			get
			{
				return aggregateSpans.Where(x => x.Key.Item3).ToDictionary(x => Tuple.Create(x.Key.Item1, x.Key.Item2), y => y.Value);
			} 
		}

		public Dictionary<Tuple<IEmailUser, IEmailUser>, TimeSpan> Inbound
		{
			get
			{
				return aggregateSpans.Where(x => !x.Key.Item3).ToDictionary(x => Tuple.Create(x.Key.Item1, x.Key.Item2), y => y.Value);
			}
		}

		public List<IEmailUser> Users
		{
			get
			{
				return addressBook.Users;
			}
		} 

		public EmailCalculator(EmailAddressBook addressBook)
		{
			this.addressBook = addressBook;
		}

		public void Generate(List<Email> emails)
		{
			aggregateSpans.Clear();
			foreach (var email in emails)
			{
				bool isEmailResolved = false;
				var fromUser = addressBook.GetOrAddUser(email.From).First();

				var toAddressList = email.To.ToList();
				var recipientCountGuess = 0;
				foreach (var toAddress in toAddressList)
				{
					var toUserList = addressBook.GetOrAddUser(toAddress);
					foreach (var toUser in toUserList)
					{
						if (toAddressList.Count == 1 || toUser != fromUser) recipientCountGuess++;
					}
				}

				foreach (var toAddress in toAddressList)
				{
					bool skipEmail = false;
					var toUserList = addressBook.GetOrAddUser(toAddress);
					foreach (var toUser in toUserList)
					{
						if (toUser.Id.HasValue && toUser.Id.Value == email.User) // Incoming email
						{
							if (toAddressList.Count != 1 && toUser == fromUser)
							{
								skipEmail = true;
								continue; // If replyAll contains the sender skip
							}

							Debug.WriteLine("Incoming email: {0} -> {1} ({2})", email.From, string.Join(", ", email.To), email.User);
							aggregateSpans[Tuple.Create(fromUser, toUser, false)] += email.Span; // Calculate with full time
							isEmailResolved = true;
						}
					}

					if (fromUser.Id.HasValue && fromUser.Id.Value == email.User && !skipEmail) // Outgoing email
					{
						Debug.WriteLine("Outgoing email: {0} -> {1} ({2})", email.From, string.Join(", ", email.To), email.User);
						foreach (var toUser in toUserList)
						{
							if (toAddressList.Count != 1 && fromUser == toUser) continue;
							aggregateSpans[Tuple.Create(fromUser, toUser, true)] += new TimeSpan(email.Span.Ticks/recipientCountGuess);
						}
						isEmailResolved = true;
					}
				}

				if (!isEmailResolved)
				{
					Debug.WriteLine("Unresolved email: {0} -> {1} ({2})", email.From, string.Join(", ", email.To), email.User);
				}
			}
		}

		public static List<Email> GetEmails(Dictionary<Device, List<PcWorkItem>> unpivoted)
		{
			var emails = new List<Email>();
			var lastEmail = (Email)null;
			foreach (var computer in unpivoted.Keys)
			{
				foreach (var v in unpivoted[computer])
				{
					string from, tos;
                    string from2, tos2;
				    v.Values.TryGetValue("MailFrom", out from);
				    v.Values.TryGetValue("emailfrom", out from2);
				    v.Values.TryGetValue("MailTo", out tos);
				    v.Values.TryGetValue("emailto", out tos2);
				    from = string.IsNullOrEmpty(from) ? from2 : from;
                    tos = string.IsNullOrEmpty(tos) ? tos2 : tos;
					if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(tos)) continue;
					var toAddresses = tos.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					if (lastEmail == null ||
						lastEmail.From != from || !lastEmail.To.SequenceEqual(toAddresses) || computer.UserId != lastEmail.User)
					{
						if (lastEmail != null && lastEmail.Span > TimeSpan.FromSeconds(1)) emails.Add(lastEmail);
						lastEmail = new Email { Span = v.Duration, From = from, To = toAddresses, User = computer.UserId };
					}
					else
					{
						lastEmail.Span += v.Duration;
					}
				}
			}

			if (lastEmail != null && lastEmail.Span > TimeSpan.FromSeconds(1)) emails.Add(lastEmail);
			return emails;
		}
	}
}
