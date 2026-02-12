using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.EmailStats
{
	public static class EmailStatsAutoSendHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void SendStatsEmailsIfApplicable()
		{
			//insert new users into EmailStats, because only users in that table will receive automatic emails
			InsertNewUsersIntoEmailStats();

			var utcNow = DateTime.UtcNow;

			using (var context = new AggregateDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var combinedStats = GetUserStatsWhereEmailIsRequired(utcNow, context);

				if (combinedStats.Count == 0) return;
				foreach (var emailStatUser in combinedStats.Select(n => n.EmailStat))
				{
					emailStatUser.LastSendDate = utcNow;
				}
				context.SubmitChanges();

				foreach (var stat in combinedStats.ToLookup(n => n.LocalReportDate))
				{
					var userStats = stat.Select(n => n.UserStatInfo).ToList();
					log.Info("Sending out daily stats in email for " + stat.Key);
					EmailStatsHelper.Send(stat.Key, ReportType.Daily, userStats);
					SendProjectReportsToUsersInterested(stat.Key, ReportType.Daily, userStats);
					SendLocationReportsToUsersInterested(stat.Key, ReportType.Daily, userStats);
					if (stat.Key.DayOfWeek == DayOfWeek.Sunday)
					{
						log.Info("Sending out weekly stats in email for the week including " + stat.Key);
						EmailStatsHelper.Send(stat.Key, ReportType.Weekly, userStats);
						SendProjectReportsToUsersInterested(stat.Key, ReportType.Weekly, userStats);
						SendLocationReportsToUsersInterested(stat.Key, ReportType.Weekly, userStats);
					}
					if (stat.Key.AddDays(1).Day == 1)
					{
						log.Info("Sending out monthly stats in email for the month including " + stat.Key);
						EmailStatsHelper.Send(stat.Key, ReportType.Monthly, userStats);
						SendProjectReportsToUsersInterested(stat.Key, ReportType.Monthly, userStats);
						SendLocationReportsToUsersInterested(stat.Key, ReportType.Monthly, userStats);
					}
				}
			}
		}

		private static List<CombinedUserStat> GetUserStatsWhereEmailIsRequired(DateTime utcNow, AggregateDataClassesDataContext context)
		{
			var emailStats = context.EmailStats.ToDictionary(n => n.UserId);

			using (var contextJc = new JobControlDataClassesDataContext())
			{
				var userStats = contextJc.GetUserStatsInfo().ToList();

				return userStats
					.Where(n => emailStats.ContainsKey(n.Id))
					.Where(n => IsEmailRequired(utcNow, emailStats[n.Id].LastSendDate, n.TimeZone, n.StartOfDayOffset))
					.Select(n => new CombinedUserStat()
					{
						EmailStat = emailStats[n.Id],
						UserStatInfo = n,
						LocalReportDate = CalculatorHelper.GetLocalReportDate(utcNow, n.TimeZone, n.StartOfDayOffset).AddDays(-1),
					})
					.ToList();
			}
		}

		private static bool IsEmailRequired(DateTime utcNow, DateTime? utcLastSendDate, TimeZoneInfo userTimeZoneInfo, TimeSpan startOfDayOffset)
		{
			if (!utcLastSendDate.HasValue) return true;

			var thisReportDay = CalculatorHelper.GetLocalReportDate(utcNow, userTimeZoneInfo, startOfDayOffset).AddDays(-1);
			var lastReportDay = CalculatorHelper.GetLocalReportDate(utcLastSendDate.Value, userTimeZoneInfo, startOfDayOffset).AddDays(-1);

			return lastReportDay < thisReportDay;

			/*
			-1
			nov 18 23:55 - nov 18
			nov 19 > 23 -> nov 19

			3
			nov 19 3:50 - nov 18
			nov 20 > 3 -> nov 19
			*/
		}

		private static void InsertNewUsersIntoEmailStats()
		{
			log.Info("Inserting missing users into EmailStats table");
			using (var context = new AggregateDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var missingUsersFromStat = new HashSet<int>();
				//var computerUsers = context.AggregateWorkItems.Select(n => n.UserId).Distinct();
				//missingUsersFromStat.UnionWith(computerUsers);
				//using (var contextIvr = new IvrDataClassesDataContext())
				//{
				//    var ivrUsers = contextIvr.IvrWorkItems.Select(n => n.UserId).Distinct();
				//    missingUsersFromStat.UnionWith(ivrUsers);
				//}
				//using (var contextMan = new ManualDataClassesDataContext())
				//{
				//    var manualusers = contextMan.ManualWorkItems.Select(n => n.UserId).Distinct();
				//    missingUsersFromStat.UnionWith(manualusers);
				//}

				//use this method so users without any work will receive project emails
				using (var contextJc = new JobControlDataClassesDataContext())
				{
					var allUsers = contextJc.GetUserStatsInfo().Select(n => n.Id).Distinct();
					missingUsersFromStat.UnionWith(allUsers);
				}

				var statUsers = context.EmailStats.Select(n => n.UserId);
				missingUsersFromStat.ExceptWith(statUsers);


				log.Info("Found " + missingUsersFromStat.Count + " missing user" + (missingUsersFromStat.Count == 1 ? "" : "s"));
				foreach (var missingUser in missingUsersFromStat)
				{
					try
					{
						context.EmailStats.InsertOnSubmit(new EmailStat()
						{
							UserId = missingUser,
							LastSendDate = DateTime.UtcNow,
							// send first mail tomorrow or whatever
						});
						context.SubmitChanges();
					}
					catch (Exception ex)
					{
						log.Error("Unable to insert new user with id: " + missingUser, ex);
					}
				}
			}
			log.Info("Finished inserting missing users into EmailStats table");
		}

		private static void SendProjectReportsToUsersInterested(DateTime localDate, ReportType reportType, List<UserStatInfo> availableUsers)
		{
			try
			{
				var freq = GetReportFrequencyFromType(reportType);
				var interestedUsers = StatsDbHelper.GetProjectEmailRequests().Where(n => (n.Frequency & freq) != 0);
				var interestedAndAvailableUsers = (from availUser in availableUsers
												   join interestedUser in interestedUsers
													 on availUser.Id equals interestedUser.ReportUserId
												   select new { UserStatInfo = availUser, ProjectEmailRequest = interestedUser }).ToList();

				var projectLookup = StatsDbHelper.GetProjectsById().Values.ToLookup(n => n.ParentId);
				var curr = 0;
				foreach (var interestedAndAvailableUser in interestedAndAvailableUsers)
				{
					++curr;
					var sw = Stopwatch.StartNew();
					//calculate startEndDate based on reportUserId is a joke... but good for now
					var startEndDate = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(
						reportType,
						localDate,
						interestedAndAvailableUser.UserStatInfo);
					EmailProjectStatsHelper.Send(startEndDate.StartDate,
												 startEndDate.EndDate,
												 interestedAndAvailableUser.ProjectEmailRequest.ReportUserId,
												 interestedAndAvailableUser.ProjectEmailRequest.IsInternal,
												 interestedAndAvailableUser.ProjectEmailRequest.ProjectRootIds,
												 interestedAndAvailableUser.ProjectEmailRequest.EmailsTo, projectLookup);
					log.Info("Executed SendProjectReportsToUsersInterested " + curr + " out of " + interestedAndAvailableUsers.Count + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to execute SendProjectReportsToUsersInterested", ex);
			}
		}

		private static void SendLocationReportsToUsersInterested(DateTime localDate, ReportType reportType, List<UserStatInfo> availableUsers)
		{
			try
			{
				var freq = GetReportFrequencyFromType(reportType);
				var emailRequests = StatsDbHelper.GetLocationReportRequests().Where(r => (r.Frequency & freq) != 0).Join(availableUsers, o => o.UserId, i => i.Id, (o, i) => i);

				foreach (var user in emailRequests)
				{
					var startEndDate = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(
						reportType,
						localDate,
						user);
					try
					{
						EmailMobileLocationStatsHelper.Send(user,
															startEndDate.StartDate,
															startEndDate.EndDate,
															reportType);
					}
					catch (Exception ex)
					{
						log.Error("Error in EmailMobileLocationStatsHelper.Send for userId " + user.Id + " from " + startEndDate.StartDate + " to " + startEndDate.EndDate + " type " + reportType, ex);
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to execute SendLocationReportsToUsersInterested", ex);
			}
		}

		internal static ReportFrequency GetReportFrequencyFromType(ReportType reportType)
		{
			switch (reportType)
			{
				case ReportType.Daily:
					return ReportFrequency.Daily;
				case ReportType.Weekly:
					return ReportFrequency.Weekly;
				case ReportType.Monthly:
					return ReportFrequency.Monthly;
				default:
					throw new ArgumentOutOfRangeException("reportType");
			}
		}

		private class CombinedUserStat
		{
			public UserStatInfo UserStatInfo { get; set; }
			public EmailStat EmailStat { get; set; }
			public DateTime LocalReportDate { get; set; }
		}
	}
}
