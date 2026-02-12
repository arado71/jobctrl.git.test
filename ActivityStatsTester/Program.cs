using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using ActivityStatsTester.ActivityStatsServiceReference;

namespace ActivityStatsTester
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args != null && args.Length != 0)
			{
				if (args.Length < 3)
				{
					Console.WriteLine("Too few parameters");
					PrintUsage();
					return;
				}
				if (args[0].ToLowerInvariant() != "d" && args[0].ToLowerInvariant() != "w" && args[0].ToLowerInvariant() != "m")
				{
					if (args[0].ToLowerInvariant() == "p")
					{
						ProcessProjectEmail(args);
						return;
					}
					Console.WriteLine("Invalid email type");
					PrintUsage();
					return;
				}
				DateTime dt;
				if (!DateTime.TryParse(args[1], out dt))
				{
					Console.WriteLine("Unable to parse '" + args[1] + "' as DateTime");
					PrintUsage();
					return;
				}

				List<int> userIds = new List<int>();
				if (args[2].ToLowerInvariant() != "all" || args.Length != 3)
				{
					for (int i = 2; i < args.Length; i++)
					{
						int userId;
						if (!int.TryParse(args[i], out userId))
						{
							Console.WriteLine("Unable to parse '" + args[i] + "' as int");
							PrintUsage();
							return;
						}
						userIds.Add(userId);
					}
				}

				var client = new ActivityStatsServiceReference.ActivityStatsClient();
				try
				{
					string users = (userIds.Count == 0
										? "all users"
										: (userIds.Count == 1
											? "userId: " + userIds[0]
											: "userIds: " + string.Join(", ", userIds.Select(n => n.ToString()).ToArray())));
					client.ClientCredentials.UserName.UserName = "asd";
					client.ClientCredentials.UserName.Password = "asd";
					client.OperationTimeout = TimeSpan.FromMinutes(60);
					if (args[0].ToLowerInvariant() == "d")
					{
						Console.WriteLine(DateTime.Now + " SendDailyEmails for " + dt + " to " + users);
						client.SendDailyEmails(dt, userIds);
					}
					else if (args[0].ToLowerInvariant() == "w")
					{
						Console.WriteLine(DateTime.Now + " SendWeeklyEmails for " + dt + " to " + users);
						client.SendWeeklyEmails(dt, userIds);
					}
					else if (args[0].ToLowerInvariant() == "m")
					{
						Console.WriteLine(DateTime.Now + " SendMonthlyEmails for " + dt + " to " + users);
						client.SendMonthlyEmails(dt, userIds);
					}
					Console.WriteLine(DateTime.Now + " Done.");
					client.Close();
				}
				catch (CommunicationException cex)
				{
					Console.WriteLine(DateTime.Now + " Error CommunicationException: " + cex);
					client.Abort();
				}
				catch (TimeoutException tex)
				{
					Console.WriteLine(DateTime.Now + " Error TimeoutException: " + tex);
					client.Abort();
				}
				catch (Exception ex)
				{
					Console.WriteLine(DateTime.Now + " Error: " + ex);
					client.Abort();
					//throw;
				}
				Console.WriteLine("Press a key to exit...");
				Console.ReadKey(true);
				return;
			}

			ConsoleKeyInfo k = new ConsoleKeyInfo();

			while (k.Key != ConsoleKey.Escape)
			{
				try
				{
					//string data;
					DailyStats stats;
					using (var client = new ActivityStatsServiceReference.ActivityStatsClient())
					{
						client.ClientCredentials.UserName.UserName = "asd";
						client.ClientCredentials.UserName.Password = "asd";
						try
						{
							stats = client.GetDailyStats();
							XmlPersistenceManager.SaveToFile("stats.xml", stats);
							Console.WriteLine(
								string.Join(Environment.NewLine,
											stats.Users.Select(
												n =>
												new
													{
														n.UserId,
														n.Status,
														n.LastMouseActivity,
														n.LastKeyboardActivity,
														n.AverageKeyboardActivity,
														n.AverageMouseActivity,
														Works = (new[] { n.CurrentWork }).Concat(n.RecentWorks ?? new List<UserWorkStats>())
													})
												.Select(
												n =>
												"UId: " + n.UserId + " Start: " + n.Works.Min(w => w.StartDate) + " Work: " +
												n.Works.Select(w => w.WorkTime.TotalSeconds).Sum() + " S: " + n.Status + "(" +
												n.LastKeyboardActivity +
												"," + n.LastMouseActivity + ")" + "(" + n.AverageKeyboardActivity +
												"," + n.AverageMouseActivity + ")" + " C: " + n.Works.First().WorkId + " R: " +
												string.Join(", ", n.Works.Skip(1).Select(w => w.WorkId.ToString()).ToArray()))
												.ToArray()
									));
						}
						catch (Exception ex)
						{
							Console.WriteLine("Error: " + ex);
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error: " + ex);
				}
				k = Console.ReadKey(true);
			}
		}

		private static void ProcessProjectEmail(string[] args)
		{
			if (args.Length != 8)
			{
				Console.WriteLine("Invalid number of arguments");
				PrintUsage();
				return;
			}
			int userId;
			string userName;
			string password;
			if (!int.TryParse(args[1], out userId))
			{
				if (args[1].StartsWith("asd") && int.TryParse(args[1].Substring(3), out userId))
				{
					userName = "asd";
					password = args[2]; //no hash this time
				}
				else
				{
					Console.WriteLine("Unable to parse '" + args[1] + "' as int");
					PrintUsage();
					return;
				}
			}
			else
			{
				userName = userId.ToString();
				password = GetHashedHexString(args[2]);
			}
			bool isInternal;
			if (!bool.TryParse(args[3], out isInternal))
			{
				Console.WriteLine("Unable to parse '" + args[3] + "' as bool");
				PrintUsage();
				return;
			}
			DateTime startDate;
			if (!DateTime.TryParse(args[4], out startDate))
			{
				Console.WriteLine("Unable to parse '" + args[4] + "' as DateTime");
				PrintUsage();
				return;
			}
			DateTime endDate;
			if (!DateTime.TryParse(args[5], out endDate))
			{
				Console.WriteLine("Unable to parse '" + args[5] + "' as DateTime");
				PrintUsage();
				return;
			}
			var rootIds = new List<int>();
			foreach (var s in args[6].Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
			{
				int id;
				if (!int.TryParse(s, out id))
				{
					Console.WriteLine("Unable to parse '" + s + "' as int");
					PrintUsage();
					return;
				}
				rootIds.Add(id);
			}
			var toAddresses = new List<string>();
			foreach (var s in args[7].Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
			{
				toAddresses.Add(s);
			}
			var client = new ActivityStatsServiceReference.ActivityStatsClient();
			try
			{
				client.ClientCredentials.UserName.UserName = userName;
				client.ClientCredentials.UserName.Password = password;
				client.OperationTimeout = TimeSpan.FromMinutes(60);
				Console.WriteLine(DateTime.Now + " Sending email(s)...");
				client.SendProjectEmails(startDate, endDate, userId, isInternal, rootIds, toAddresses);
				Console.WriteLine(DateTime.Now + " Done.");
				client.Close();
			}
			catch (CommunicationException cex)
			{
				Console.WriteLine(DateTime.Now + " Error CommunicationException: " + cex);
				client.Abort();
			}
			catch (TimeoutException tex)
			{
				Console.WriteLine(DateTime.Now + " Error TimeoutException: " + tex);
				client.Abort();
			}
			catch (Exception ex)
			{
				Console.WriteLine(DateTime.Now + " Error: " + ex);
				client.Abort();
				//throw;
			}
			Console.WriteLine("Press a key to exit...");
			Console.ReadKey(true);
			return;
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage: send daily email to users 2 3 4 5 6: " + Assembly.GetExecutingAssembly().GetName().Name + " d 2009-12-31 2 3 4 5 6");
			Console.WriteLine("       send weekly email to all users: " + Assembly.GetExecutingAssembly().GetName().Name + " w 2009-12-31 all");
			Console.WriteLine("       send monthly email to all users: " + Assembly.GetExecutingAssembly().GetName().Name + " m 2009-12-31 all");
			Console.WriteLine("       send internal project email asked by user 13 for roots 1 3: " + Assembly.GetExecutingAssembly().GetName().Name + " p 13 password true \"2011-02-28 03:00\" \"2011-03-01 03:00\" \"1 3\" \"example@tct.hu ex2@tct.hu\"");
			Console.WriteLine("       send external project email asked by user 13 for roots 1 3: " + Assembly.GetExecutingAssembly().GetName().Name + " p 13 password false \"2011-02-28 03:00\" \"2011-03-01 03:00\" \"1 3\" \"example@tct.hu ex2@tct.hu\"");
		}


		public static string GetHashedHexString(string clearPassword)
		{
			var clear = Encoding.UTF8.GetBytes(clearPassword);
			using (var hashProvider = new SHA256Managed())
			{
				var hashed = hashProvider.ComputeHash(clear);
				var hashedStr = BitConverter.ToString(hashed).Replace("-", string.Empty);
				return hashedStr;
			}
		}
	}
}
namespace ActivityStatsTester.ActivityStatsServiceReference
{
	public partial class ActivityStatsClient
	{
		public TimeSpan OperationTimeout
		{
			get { return ((IContextChannel)base.Channel).OperationTimeout; }
			set { ((IContextChannel)base.Channel).OperationTimeout = value; }
		}
	}
}
