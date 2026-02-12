using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using log4net;

namespace PlaybackClient
{
	class Program
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) => true;

			var appName = ConfigManager.ApplicationName; //load ConfigManager

			Func<PlaybackDataScheduler> startFunc = null;

#if DEBUG
			startFunc = () =>
				{
					var scheduler = new PlaybackDataScheduler();
					scheduler.Add(new PlaybackSchedule()
					{
						UserId = 13,
						//DataStartDate = DateTime.Parse("2012-12-10 02:00:00.000"),
						StartDate = DateTime.Parse("2012-12-10 08:21:25.233"),
						//DataEndDate = DateTime.Parse("2012-12-10 08:21:28.870"),
						EndDate = DateTime.Parse("2012-12-11 02:00:00.000"),
						FirstScheduleDate = DateTime.Parse("2012-12-10 02:00:00.000"),
						TimeZoneId = TimeZoneInfo.Local.Id,
						LocalSchedule = new Scheduling.ScheduleData() { Type = Scheduling.ScheduleType.Daily, Interval = 1, StartDate = DateTimeEx.Now(), }.CreateSchedule(),
						//LocalSchedule = new Scheduling.ScheduleData() { Type = Scheduling.ScheduleType.EvenInterval, Interval = (int)TimeSpan.FromMinutes(10).TotalMilliseconds, StartDate = DateTime.Parse("2012-12-10 02:00:00.000"), }.CreateSchedule(),
					});
					return scheduler;
				};

			//startFunc = () => PlaybackSchedulerFactory.OneTimeImport(13, DateTime.Parse("2012-12-10 02:00:00.000"), DateTime.Parse("2012-12-11 02:00:00.000"), 14);
#endif

			if (startFunc == null
				&& args.Length == 1
				&& string.Equals(args[0], "db", StringComparison.OrdinalIgnoreCase))
			{
				startFunc = PlaybackSchedulerFactory.StartDbSchedules;
			}
			else if (startFunc == null
				&& (args.Length >= 4 && args.Length <= 6)
				&& string.Equals(args[0], "import", StringComparison.OrdinalIgnoreCase))
			{
				int userId;
				DateTime startDate;
				DateTime endDate;
				int newUserId = 0;
				Func<int, int> mappingFunc = null;
				if (int.TryParse(args[1], out userId)
					&& DateTime.TryParse(args[2], out startDate)
					&& DateTime.TryParse(args[3], out endDate)
					&& (args.Length < 5 || int.TryParse(args[4], out newUserId))
					&& (args.Length < 6 || TryParseMapping(args[5], out mappingFunc))
					)
				{
					startFunc = () => PlaybackSchedulerFactory.OneTimeImport(userId, startDate, endDate, args.Length < 5 ? (int?)null : newUserId, mappingFunc);
				}
			}

			if (startFunc == null)
			{
				var msg = "Exit - started with wrong params: " + string.Join(" ", args);
				Console.WriteLine(msg);
				log.Info(msg);
				PrintUsage();
				return;
			}

			try
			{
				using (startFunc())
				{
					Console.WriteLine("Press <ENTER> to terminate client.");
					Console.WriteLine();
					Console.ReadLine();
					Console.WriteLine("Exit");
					log.Info("Exit");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exit - due to error" + Environment.NewLine + ex);
				log.Error("Exit - due to error", ex);
			}
		}

		internal static bool TryParseMapping(string mappingStr, out Func<int, int> mappingFunc)
		{
			mappingFunc = null;
			if (string.IsNullOrEmpty(mappingStr)) return false;
			var availWorkIds = new HashSet<int>();
			var mappingDict = new Dictionary<int, int>();
			var parts = mappingStr.Split(',');
			foreach (var part in parts)
			{
				int toId;
				if (part.IndexOf('=') > -1)
				{
					int fromId;
					var fromTo = part.Split('=');
					if (!int.TryParse(fromTo[0], out fromId)) return false;
					if (!int.TryParse(fromTo[1], out toId)) return false;
					if (mappingDict.ContainsKey(fromId)) return false;
					mappingDict.Add(fromId, toId);
				}
				else
				{
					if (!int.TryParse(part, out toId)) return false;
					availWorkIds.Add(toId);
				}
			}
			mappingFunc = CreateMappingFunc(mappingDict, availWorkIds.ToList());
			return true;
		}

		private static Func<int, int> CreateMappingFunc(Dictionary<int, int> mappingDict, List<int> availWorkIds)
		{
			var nextIdx = 0;
			return fromId =>
				{
					int toId;
					if (!mappingDict.TryGetValue(fromId, out toId))
					{
						if (availWorkIds.Count > 0) //we have to choose from availWorkIds
						{
							toId = availWorkIds[nextIdx];
							nextIdx = (nextIdx + 1) % availWorkIds.Count;
						}
						else //we have no availWorkIds so don't map this value
						{
							toId = fromId;
						}
						mappingDict.Add(fromId, toId); //next time we will use the same mapping
					}
					return toId;
				};
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine(" Start schedules from DB: " + ConfigManager.ApplicationName + " db");
			Console.WriteLine(" Import data for user: " + ConfigManager.ApplicationName + " import 2 2013-09-04 2013-09-05");
			Console.WriteLine(" Import data parameters are: userId utcStartDate utcEndDate [newUserId] [fromWorkId1=toWorkId1,availId1,availId2]");
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			log.Fatal("Unexpected error in current domain", e.ExceptionObject as Exception);
			if (e.IsTerminating)
			{
				log.Fatal("Initiating shutdown...");
				Debug.Fail("Unexpected error in current domain", (e.ExceptionObject as Exception ?? new Exception("(null)")).ToString());
				//don't show "xy encountered a problem and needs to close" message
				Environment.Exit(-1);
			}
		}

	}
}
