using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using log4net;
using Reporter.Interfaces;
using Reporter.Model.WorkItems;
using Tct.ActivityRecorderService.Caching.Works;
using Tct.ActivityRecorderService.Stats;
using Reporter.Model;

namespace Tct.ActivityRecorderService.EmailStats
{
	public static class EmailStatsHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string DefaultCulture = "hu-hu";

		public static void Send(DateTime localDate, ReportType reportType, List<int> userIdsFilter, List<string> onlyTo = null)
		{
			var emailStatUsers = StatsDbHelper.GetUserStatsInfo(userIdsFilter);
			Send(localDate, reportType, emailStatUsers, onlyTo);
		}

		public static void Send(DateTime localDate, ReportType reportType, List<UserStatInfo> emailStatUsers, List<string> onlyTo = null)
		{
			foreach (var usersWithSameStartEndTime in emailStatUsers
				.ToLookup(n => CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(reportType, localDate, n)))
			{
				var startEndTime = usersWithSameStartEndTime.Key;
				Send(startEndTime.StartDate, startEndTime.EndDate, reportType, usersWithSameStartEndTime.ToDictionary(n => n.Id), onlyTo != null ? new HashSet<string>(onlyTo) : null);
			}
		}

		private static void Send(DateTime startDate, DateTime endDate, ReportType reportType, Dictionary<int, UserStatInfo> userIdsFilterDict, HashSet<string> onlyTo = null)
		{
			try
			{
				var emailsToSend = GetEmailsToSend(startDate, endDate, reportType, userIdsFilterDict);
				foreach (var emailToSendLoop in emailsToSend.OrderByDescending(n => n.SortKey))
				{
					var emailToSend = emailToSendLoop; //don't close over loop variable
					if (onlyTo != null && !onlyTo.Contains(emailToSend.To))
					{
						log.Debug("Skip sending email To: " + emailToSend.To + " Sub: " + emailToSend.Subject + " because filtered out");
						continue;
					}

					if (ShouldSendEmailToUser(userIdsFilterDict[emailToSend.UserId], reportType))
					{
						LogInfoAndVerbose("Sending " + reportType + " email To: " + emailToSend.To + " Sub: " + emailToSend.Subject, "Body: " +
								 Environment.NewLine + emailToSend.Body);
						EmailManager.Instance.Send(new EmailMessage()
						{
							To = emailToSend.To,
							Subject = emailToSend.Subject,
							PlainBody = emailToSend.Body,
							HtmlBody = emailToSend.BodyHtml,
							HtmlResources = emailToSend.HtmlResources,
						});
					}
					else
					{
						LogInfoAndVerbose("Skip sending " + reportType + " email To: " + emailToSend.To + " Sub: " + emailToSend.Subject, "Body: " +
								 Environment.NewLine + emailToSend.Body);
					}
				}

				//send out aggreagte emails
				var aggregateEmailGroups = EmailStatsAggregateHelper.GetAggregateEmailGroups(emailsToSend, reportType);
				foreach (var aggregateEmailGroup in aggregateEmailGroups)
				{
					var emailToSend = aggregateEmailGroup.GenerateAggregatedEmail(startDate, endDate, reportType);
					if (emailToSend.Count == 0 || aggregateEmailGroup.EmailsTo == null || aggregateEmailGroup.EmailsTo.Count == 0) continue;

					foreach (var grp in aggregateEmailGroup.EmailsTo.GroupBy(e => e.CultureId ?? EmailStatsHelper.DefaultCulture, e => e.Address))
						LogInfoAndVerbose("Sending " + reportType + " aggregated email Culture: " + grp.Key + " To: " + string.Join(", ", grp.ToList()) + " Sub: " + emailToSend[grp.Key].Subject, "HTMLBody: " +
							 Environment.NewLine + emailToSend[grp.Key].BodyHtml);

					foreach (var emailToLoop in aggregateEmailGroup.EmailsTo)
					{
						var emailTo = emailToLoop.Address; //don't close over loop variable
						if (onlyTo != null && !onlyTo.Contains(emailTo))
						{
							log.Debug("Skip sending aggregated email To: " + emailTo + " because filtered out");
							continue;
						}

						var emailLocalized = emailToSend[emailToLoop.CultureId ?? EmailStatsHelper.DefaultCulture];
						if (emailLocalized == null) continue;
						EmailManager.Instance.Send(new EmailMessage()
						{
							To = emailTo,
							Subject = emailLocalized.Subject,
							PlainBody = emailLocalized.Body,
							HtmlBody = emailLocalized.BodyHtml,
							HtmlResources = emailLocalized.HtmlResources,
						});
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Unexpected error in Send", ex);
			}
		}

		private static bool ShouldSendEmailToUser(UserStatInfo userStatInfo, ReportType reportType)
		{
			var freq = EmailStatsAutoSendHelper.GetReportFrequencyFromType(reportType);
			return (userStatInfo.ReportFreqType & freq) != 0;
		}

		internal static List<EmailToSend> GetEmailsToSend(DateTime startDate, DateTime endDate, ReportType reportType, Dictionary<int, UserStatInfo> userIdsFilterDict)
		{
			var wstats = StatsDbHelper.GetWorkItemsByUser(startDate, endDate);
			var manStats = StatsDbHelper.GetManualWorkItemsByUser(startDate, endDate);
			var intervalStats = StatsDbHelper.GetAggregateWorkItemIntervalsByUser(startDate, endDate);
			var mobStats = StatsDbHelper.GetMobileWorkItemsByUser(startDate, endDate);

			var compusers = wstats.Select(n => n.Key);
			var manusers = manStats.Select(n => n.Key);
			var mobusers = mobStats.Select(n => n.Key);
			var allUsersStats = compusers
				.Union(manusers)
				.Union(mobusers)
				.Where(n => userIdsFilterDict.ContainsKey(n))
				.Select(n => new UserEmailStats
								{
									UserId = n,
									UserStatInfo = userIdsFilterDict[n],
									ManualStats = manStats[n],
									IntervalStats = intervalStats[n],
									MobileStats = mobStats[n],
								});
			var emailsToSend = new List<EmailToSend>();
			foreach (var userStats in allUsersStats)
			{
				try
				{
					Debug.Assert(userStats.UserStatInfo != null);
					var culture = CultureInfo.GetCultureInfo(string.IsNullOrEmpty(userStats.UserStatInfo.CultureId) ? DefaultCulture : userStats.UserStatInfo.CultureId);
					Thread.CurrentThread.CurrentCulture = culture;
					Thread.CurrentThread.CurrentUICulture = culture;
					var subject = GetSubjectForUser(userStats.UserStatInfo, userStats.UserId, startDate, reportType, culture);

					log.Debug("Fetching workItems for user " + userStats.UserId);
					var workItems = StatsDbHelper.GetWorkItemsForUser(userStats.UserId, startDate, endDate);
					log.Debug("Fetching collectedItems for user " + userStats.UserId);
					var collectedItems = StatsDbHelper.GetCollectedItemsForUser(userStats.UserId, startDate, endDate);
					log.Debug("Fetching work details for user " + userStats.UserId);
					var detailedWorks = StatsDbHelper.GetDetailedWorkForUserCached(userStats.UserId);
					log.Debug("Fetching total work times for user " + userStats.UserId);
					var totalWorkTimes = StatsDbHelper.GetTotalWorkTimeByWorkIdForUserCached(userStats.UserId, endDate);
					log.Debug("Checking credits for user " + userStats.UserId);
					var localDays = GetLocalDaysForInterval(userStats.UserStatInfo, startDate, endDate).ToList();
					var hasCredits = StatsDbHelper.HasUserGotCreditForInterval(userStats.UserId, localDays.First(), localDays.Last());

					var statsBuilder = new DetailedWorkTimeStatsBuilder();
					var workTimesForUser = new Dictionary<DateTime, FullWorkTimeStats>();
					statsBuilder.AddManualWorkItems(userStats.ManualStats);
					statsBuilder.AddMobileWorkItems(userStats.MobileStats);
					statsBuilder.AddWorkItems(workItems);
					log.Debug("Created DetailedWorkTimeStatsBuilder for user " + userStats.UserId);
					var resultFullTimeSpan = statsBuilder.GetDetailedWorkTime(startDate, endDate);
					log.Debug("Created stats for the entire timespan for user " + userStats.UserId + " stats: " + resultFullTimeSpan);

					subject += " (" + EmailStats.AllWorkShort + ": " + resultFullTimeSpan.SumWorkTime.ToHourMinuteSecondString() + ")";
					var sb = new EmailBuilder();
					sb.BodyHtml.Append("<span style=\"font-family: Arial,sans-serif;\">");

					if (!hasCredits) //we need to change the contents of the EmailBuilder because its used in the aggragate email
					{
						sb.AppendLine();
						sb.AppendLines(EmailStats.NoCreditText);
					}

					//Add summary
					AppendSumWorkTimeStats(resultFullTimeSpan, reportType, userStats.UserStatInfo, sb);

					//Add worktime stats by work
					var workStats = resultFullTimeSpan.AllWorkTimeById.Select(
						n => new WorkTimeStat
						{
							WorkName = WorkHierarchyService.Instance.GetWorkNameWithProjects(n.Key),
							WorkTime = n.Value,
							DetailedWork = detailedWorks.GetValueOrDefault(n.Key),
							TotalWorkTimeStat = totalWorkTimes.GetValueOrDefault(n.Key),
						});

					var table = GetWorkTimeStatsTable(sb, workStats, startDate, endDate, userStats.UserStatInfo);
					if (table != null)
					{
						sb.AppendTable(table);
						sb.AppendLine();
					}

					//AppendProgressWarningCharts(sb, totalWorkTimes, detailedWorks, works, startDate, endDate, userStats.UserStatInfo);

					if (reportType == ReportType.Daily)
					{
						AddFullWorkTimeStats(workTimesForUser, resultFullTimeSpan, userStats.UserStatInfo);
						AppendWorkTimeDescriptions(sb);

						//probably nobody cares about this (todo hourly aggregates could be removed later)
						//if (resultFullTimeSpan.WorkItems.Count > 0)
						//{
						//    AppendWorkItemStats(sb, resultFullTimeSpan, works, detailedWorks, userStats.UserStatInfo);
						//}
						if (resultFullTimeSpan.ManualWorkItems.Count > 0)
						{
							AppendManualWorkItemStats(sb, resultFullTimeSpan, userStats.UserStatInfo);
						}

						var sw = Stopwatch.StartNew();
						var chart = ChartHelper.CreateWorkIntervalsAndActivityChart(userStats.IntervalStats.ToList(),
																					resultFullTimeSpan.WorkItems,
																					resultFullTimeSpan.ManualWorkItems,
																					resultFullTimeSpan.MobileWorkItems,
																					startDate,
																					endDate,
																					userStats.UserStatInfo);
						if (chart != null)
						{
							sb.AppendHtmlChart(chart, EmailStats.WorksByTime);
							if (resultFullTimeSpan.WorkItems.Count > 0) //if there is an activity chart area
							{
								sb.BodyHtml.AppendLine("<div style=\"font-size:smaller;\">");
								sb.BodyHtml.AppendLine(EmailStats.KeyboardActivityInfo);
								sb.BodyHtml.AppendLine("<BR/>");
								sb.BodyHtml.AppendLine(EmailStats.MouseActivityInfo);
								sb.BodyHtml.AppendLine("</div>");
								sb.BodyHtml.AppendLine("<BR/>");
							}
						}
						log.Debug("Created chart for user " + userStats.UserId + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms");
					}
					else
					{
						AppendDailySumWorkTimeStats(startDate, endDate, userStats, statsBuilder, resultFullTimeSpan, workTimesForUser, sb);
						AppendWorkTimeDescriptions(sb);
					}

					var reporterWorkItems = workItems.Select(w => new ComputerWorkItem() { UserId = w.UserId, StartDate = w.StartDate, EndDate = w.EndDate, ComputerId = w.ComputerId, WorkId = w.WorkId, MouseActivity = w.MouseActivity, KeyboardActivity = w.KeyboardActivity });
					var workItemsDeletions =
						userStats.ManualStats.Where(
							m =>
								m.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteComputerInterval ||
								m.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteInterval ||
								m.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteIvrInterval ||
								m.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteMobileInterval)
							.Select(
								m =>
									new WorkItemDeletion()
									{
										UserId = m.UserId,
										StartDate = m.StartDate,
										EndDate = m.EndDate,
										Type =
											m.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteComputerInterval
												? DeletionTypes.Computer
												: m.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteIvrInterval
													? DeletionTypes.Ivr
													: m.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteMobileInterval
														? DeletionTypes.Mobile
														: DeletionTypes.All
									});
					AppendActiveWindowStats(sb, reporterWorkItems, workItemsDeletions, collectedItems, resultFullTimeSpan.ComputerWorkTime);

					sb.BodyHtml.Append("</span>");

					if (!hasCredits) //we need to change the contents of the EmailBuilder because its used in the aggragate email
					{
						subject = ObfuscateWorkTimes(subject);
						ObfuscateWorkTimes(sb.Body);
						ObfuscateWorkTimes(sb.BodyHtml);

						sb.AppendLine();
						sb.AppendLines(EmailStats.NoCreditText);
					}

					string body = sb.Body.ToString();
					string htmlBody = "<HTML><HEAD></HEAD><BODY><!--PRE style=\"font-size:0.95em;font-family:Courier New,monospace;\"-->" +
									  sb.BodyHtml.ToString() + "<!--/PRE--></BODY></HTML>";

					emailsToSend.Add(new EmailToSend()
										{
											To = userStats.UserStatInfo.Email,
											Subject = subject,
											Body = body,
											BodyHtml = htmlBody,
											SortKey = userStats.UserStatInfo.Name,
											FirstName = userStats.UserStatInfo.FirstName,
											LastName = userStats.UserStatInfo.LastName,
											UserId = userStats.UserId,
											WorkTimes = workTimesForUser,
											FullWorkTime = resultFullTimeSpan.CloneFullWorkTimeStats(),
											EmailBuilder = sb,
											HtmlResources = sb.HtmlResources,
											HasCredits = hasCredits,
										});
					log.Info("Created " + reportType + " email report for userId : " + userStats.UserId);
				}
				catch (Exception ex)
				{
					log.Error("Unable to create stat for userId: " + userStats.UserId + " between " + startDate + " and " + endDate, ex);
#if DEBUG
					throw;
#endif
				}
			}
			return emailsToSend;
		}

		public static void AppendWorkTimeDescriptions(EmailBuilder sb)
		{
			sb.Body.AppendLine(EmailStats.NetWorkTimeDescription);
			sb.Body.AppendLine(EmailStats.WorkTimeDescription);

			sb.BodyHtml.AppendLine("<div style=\"font-size:smaller;\">");
			sb.BodyHtml.AppendLine(EmailStats.NetWorkTimeDescription);
			sb.BodyHtml.AppendLine("<BR/>");
			sb.BodyHtml.AppendLine(EmailStats.WorkTimeDescription);
			sb.BodyHtml.AppendLine("</div>");

			sb.AppendLine();
		}

		internal static void ObfuscateWorkTimes(StringBuilder sb)
		{
			if (sb == null) return;
			var content = sb.ToString();
			sb.Clear();
			sb.Append(ObfuscateWorkTimes(content));
		}

		//omg hax - but we can avoid extending all methods with a hasCredits param
		internal static string ObfuscateWorkTimes(string stringToChange)
		{
			if (stringToChange == null) return null;
			return Regex.Replace(stringToChange, "\\d{2,}(:[0-5][0-9]){1,2}", WorkTimeMatchEvaluator,
				RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
		}

		private static string WorkTimeMatchEvaluator(Match match)
		{
			if (match.Success)
			{
				if (match.Length > 5 && match.Value[match.Value.Length - 1 - 5] == ':')
				{
					return "**:**:**";
				}
				else
				{
					return "**:**";
				}
			}
			return match.Value;
		}

		internal static IEnumerable<DateTime> GetLocalDaysForInterval(UserStatInfo userStatInfo, DateTime utcStartDate, DateTime utcEndDate)
		{
			var localStartDate = CalculatorHelper.GetLocalReportDate(utcStartDate, userStatInfo.TimeZone, userStatInfo.StartOfDayOffset);
			yield return localStartDate;
			var localEndDate = localStartDate;
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localEndDate, userStatInfo);
			while (startEnd.EndDate < utcEndDate)
			{
				localEndDate = localEndDate.AddDays(1);
				yield return localEndDate;
				startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localEndDate, userStatInfo);
			}
		}

		private static void AppendActiveWindowStats(EmailBuilder sb, IEnumerable<IWorkItem> workItems, IEnumerable<IWorkItemDeletion> workItemDeletions, IEnumerable<ICollectedItem> collectedItems, TimeSpan computerWorkTime)
		{
			var sw = Stopwatch.StartNew();
			var processedItems = Reporter.Processing.ReportHelper.Transform(collectedItems, workItemDeletions, workItems).ToList();
			log.Debug("Generated " + processedItems.Count.ToInvariantString() + " processedItem for user " + (processedItems.Count > 0 ? processedItems[0].UserId.ToInvariantString() : "()") + " in " + sw.ToTotalMillisecondsString() + "ms");
			AppendActiveWindowProcessStats(sb, processedItems, computerWorkTime);
			AppendActiveWindowTitleStats(sb, processedItems, computerWorkTime);
			AppendActiveWindowUrlStats(sb, processedItems, computerWorkTime);
		}

		private const double detailedThresholdPct = 0.01;
		private static void AppendActiveWindowTitleStats(EmailBuilder sb, List<Reporter.Model.ProcessedItems.WorkItem> processedItems, TimeSpan computerWorkTime)
		{
			if (processedItems.Count == 0) return;
			//chart is unusable for titles imho so stick with the table
			var processes = processedItems
				.GroupBy(n => new { Title = n.Values.GetValueOrDefault("Title"), ProcessName = n.Values.GetValueOrDefault("ProcessName") })
				.Select(n => new { Title = n.Key.Title, ProcessName = n.Key.ProcessName, Duration = n.Sum(m => m.Duration.TotalMilliseconds) })
				.OrderByDescending(n => n.Duration)
				.ToList();
			var allDuration = processes.Sum(n => n.Duration);
			var table = new EmailTable();
			table.SetColumnStyle(2, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(3, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.Title = EmailStats.FrequentTitles;

			var remainingDuration = 0d;
			foreach (var process in processes)
			{
				var percentage = process.Duration / allDuration;
				if (percentage >= detailedThresholdPct && !string.IsNullOrEmpty(process.ProcessName))
				{
					var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
					table.AddRow(
						EmailTable.CellData.CreateFrom(process.Title),
						EmailTable.CellData.CreateFrom(process.ProcessName),
						GetPercentageCellData((float)percentage),
						EmailTable.CellData.CreateFrom(time.ToHourMinuteString()));
				}
				else
				{
					remainingDuration += process.Duration;
				}
			}
			if (remainingDuration > 0d)
			{
				var percentage = remainingDuration / allDuration;
				var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
				table.AddRow(
					EmailTable.CellData.CreateFrom(EmailStats.TitleOthers),
					EmailTable.CellData.CreateFrom(""),
					GetPercentageCellData((float)percentage),
					EmailTable.CellData.CreateFrom(time.ToHourMinuteString()));
			}

			sb.AppendTable(table);
			sb.AppendLine();
		}

		private static void AppendActiveWindowProcessStats(EmailBuilder sb, List<Reporter.Model.ProcessedItems.WorkItem> processedItems, TimeSpan computerWorkTime)
		{
			if (processedItems.Count == 0) return;
			//use chart for Html
			var chart = ChartHelper.CreateProcessChart(processedItems, computerWorkTime);
			if (chart != null)
			{
				sb.AppendHtmlChart(chart, EmailStats.FrequentUsage);
				sb.BodyHtml.AppendLine("<BR/>");
			}
			//and table for Plain
			var processes = processedItems
				.GroupBy(n => n.Values.GetValueOrDefault("ProcessName"))
				.Select(n => new { ProcessName = n.Key, Duration = n.Sum(m => m.Duration.TotalMilliseconds) })
				.OrderByDescending(n => n.Duration)
				.ToList();
			var allDuration = processes.Sum(n => n.Duration);
			var table = new EmailTable();
			table.SetColumnStyle(1, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(2, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.Title = EmailStats.FrequentUsage;

			var remainingDuration = 0d;
			foreach (var process in processes)
			{
				var percentage = process.Duration / allDuration;
				if (percentage >= detailedThresholdPct && !string.IsNullOrEmpty(process.ProcessName))
				{
					var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
					table.AddRow(process.ProcessName, percentage.ToString("0.%"), time.ToHourMinuteString());
				}
				else
				{
					remainingDuration += process.Duration;
				}
			}
			if (remainingDuration > 0)
			{
				var percentage = remainingDuration / allDuration;
				var time = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds * percentage); //this is not accurate at all...
				table.AddRow(EmailStats.UsageOthers, percentage.ToString("0.%"), time.ToHourMinuteString());
			}

			table.GetAsciiTable(sb.Body);
			sb.Body.AppendLine();
			//sb.AppendTable(table);
			//sb.AppendLine();
		}

		private static void AppendActiveWindowUrlStats(EmailBuilder sb, List<Reporter.Model.ProcessedItems.WorkItem> processedItems, TimeSpan computerWorkTime)
		{
			if (processedItems.Count == 0) return;
			var allWindowsDuration = processedItems.Sum(n => n.Duration.TotalMilliseconds);
			var processes = processedItems
				.Where(n => !string.IsNullOrEmpty(n.Values.GetValueOrDefault("Url")))
				.GroupBy(n => UrlFormatHelper.GetShortUrlFrom(n.Values.GetValueOrDefault("Url")))
				.Select(n => new { Url = n.Key, Duration = n.Sum(m => m.Duration.TotalMilliseconds) })
				.OrderByDescending(n => n.Duration)
				.ToList();
			if (processes.Count == 0) return;
			var allDuration = processes.Sum(n => n.Duration); //allCount where we have an url
			var computerWorkTimeWithUrl = TimeSpan.FromMilliseconds(computerWorkTime.TotalMilliseconds / allWindowsDuration * allDuration);
			var table = new EmailTable();
			table.SetColumnStyle(1, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(2, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.Title = EmailStats.FrequentBrowsing;

			var remainingDuration = 0d;
			foreach (var process in processes)
			{
				var percentage = process.Duration / allDuration;
				if (percentage >= detailedThresholdPct)
				{
					var time = TimeSpan.FromMilliseconds(computerWorkTimeWithUrl.TotalMilliseconds * percentage); //this is not accurate at all...
					table.AddRow(
						EmailTable.CellData.CreateFrom(process.Url),
						GetPercentageCellData((float)percentage),
						EmailTable.CellData.CreateFrom(time.ToHourMinuteString()));
				}
				else
				{
					remainingDuration += process.Duration;
				}
			}
			if (remainingDuration > 0)
			{
				var percentage = remainingDuration / allDuration;
				var time = TimeSpan.FromMilliseconds(computerWorkTimeWithUrl.TotalMilliseconds * percentage); //this is not accurate at all...
				table.AddRow(
					EmailTable.CellData.CreateFrom(EmailStats.BrowsingOthers),
					GetPercentageCellData((float)percentage),
					EmailTable.CellData.CreateFrom(time.ToHourMinuteString()));
			}

			sb.AppendTable(table);
			sb.AppendLine();
		}

		private static void AppendProgressWarningCharts(EmailBuilder sb, Dictionary<int, TotalWorkTimeStat> totalWorkTimes, Dictionary<int, DetailedWork> detailedWorks, DateTime utcStartDate, DateTime utcEndDate, UserStatInfo userStatInfo)
		{
			var detailedWorksThatCanBeMeasured = detailedWorks.Values
				.Where(n => (!n.CloseDate.HasValue || n.CloseDate.Value > utcEndDate.FromUtcToLocal(userStatInfo.TimeZone))
					&& n.StartDate.HasValue
					&& n.EndDate.HasValue
					&& n.TargetTotalWorkTime.HasValue
					&& n.TargetTotalWorkTime.Value > TimeSpan.Zero);
			var now = utcEndDate.FromUtcToLocal(userStatInfo.TimeZone);
			var workProgresses = detailedWorksThatCanBeMeasured
				.Select(n => new WorkWithProgress()
				{
					Id = n.Id,
					Name = n.Name,
					Priority = n.Priority,
					StartDate = n.StartDate.Value,
					EndDate = n.EndDate.Value,
					Now = now,
					TargetWorkTime = n.TargetTotalWorkTime.Value,
					TotalWorkTime = GetTotalWorkTimeOrDefault(n.Id, totalWorkTimes),
				})
				.ToList();

			if (workProgresses.Count == 0) return;

			var workDays = CalendarHelper.GetWorkDays(userStatInfo.CalendarId, now, workProgresses.Max(n => n.EndDate)); //todo Use CalendarManager implementation
			foreach (var workWithProgress in workProgresses)
			{
				workWithProgress.WorkingDaysLeft = CalendarHelper.CountDaysBetween(workDays, workWithProgress.Now.Date, workWithProgress.EndDate);
				workWithProgress.WorkingDaysCount = CalendarHelper.CountDaysBetween(workDays, workWithProgress.StartDate, workWithProgress.EndDate);
			}

			var chart = ChartHelper.CreateWorkWarnEndDateChart(workProgresses);
			if (chart != null)
			{
				sb.AppendHtmlChart(chart, EmailStats.ExceedDeadLine);
				sb.BodyHtml.AppendLine("<BR/>");
			}

			chart = ChartHelper.CreateWorkWarnTargetTimeChart(workProgresses);
			if (chart != null)
			{
				sb.AppendHtmlChart(chart, EmailStats.ExceedTargetTime);
				sb.BodyHtml.AppendLine("<BR/>");
			}

			chart = ChartHelper.CreateWorkWarnHeuristic(workProgresses);
			if (chart != null)
			{
				sb.AppendHtmlChart(chart, EmailStats.WarnedTasks);
				sb.BodyHtml.AppendLine("<BR/>");
			}
		}

		public class WorkWithProgress
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int? Priority { get; set; }
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
			public DateTime Now { get; set; }
			public TimeSpan TargetWorkTime { get; set; }
			public TimeSpan TotalWorkTime { get; set; }
			public TimeSpan RemainingWorkTime { get { return TargetWorkTime - TotalWorkTime; } }
			//public TimeSpan WorkingTimeMissing { get; set; }
			public int WorkingDaysCount { get; set; }
			public int WorkingDaysLeft { get; set; }
			public TimeSpan TimeAfterEndDate { get { return Now - EndDate.AddDays(1); } }
			public TimeSpan TimeAfterTargetWorkTime { get { return -RemainingWorkTime; } }
			public float TagetPctEndDate { get { return CalculatorHelper.GetTargetEndDatePct(StartDate, EndDate, Now); } }
			public float TagetPctWorkTime { get { return CalculatorHelper.GetTargetWorkTimePct(TargetWorkTime, TotalWorkTime); } }
			public TimeSpan AvgWorkTimePerDayToCompletion
			{
				get
				{
					return
						RemainingWorkTime <= TimeSpan.Zero
							? TimeSpan.Zero
							: (WorkingDaysLeft <= 0
								? RemainingWorkTime //hax (as if 1 day was left)
								: TimeSpan.FromMilliseconds(RemainingWorkTime.TotalMilliseconds / WorkingDaysLeft));
				}
			}
		}

		private static void AddFullWorkTimeStats(Dictionary<DateTime, FullWorkTimeStats> workTimesForUser, DetailedWorkTimeStats detailedStats, UserStatInfo userStatInfo)
		{
			if (detailedStats.SumWorkTime == TimeSpan.Zero
				&& detailedStats.ComputerWorkTime == TimeSpan.Zero
				&& detailedStats.ManuallyAddedTime == TimeSpan.Zero
				&& detailedStats.MobileWorkTime == TimeSpan.Zero
				)
			{
				return;
			}
			var localReportDate = CalculatorHelper.GetLocalReportDate(detailedStats.StartDate, userStatInfo.TimeZone, userStatInfo.StartOfDayOffset);
			if (workTimesForUser.ContainsKey(localReportDate)) //this should not be true
			{
				throw new Exception("Cannot insert stat for userId " + userStatInfo.Id + " for day: " + localReportDate);
			}
			workTimesForUser.Add(localReportDate, detailedStats.CloneFullWorkTimeStats());
		}

		private static void AppendSumWorkTimeStats(DetailedWorkTimeStats resultFullTimeSpan, ReportType reportType, UserStatInfo userStatInfo, EmailBuilder sb)
		{
			var table = new EmailTable();
			table.SetColumnStyle(1, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.AddRow(EmailStats.WorkTimeAll, resultFullTimeSpan.SumWorkTime.ToHourMinuteSecondString());

			var currRow = new List<string>();
			currRow.Clear();
			if (resultFullTimeSpan.MobileWorkTime != TimeSpan.Zero)
			{
				currRow.AddRange(new[] { EmailStats.WorkTimeSmartPhone, resultFullTimeSpan.MobileWorkTime.ToHourMinuteSecondString(), null, null });
				//users don't care
				//if (resultFullTimeSpan.MobileWorkTime != resultFullTimeSpan.NetMobileWorkTime)
				//{
				//    currRow[2] = "(" + EmailStats.WorkTimeNetto + ": " + resultFullTimeSpan.NetMobileWorkTime.ToHourMinuteSecondString() + ")";
				//}
				//if (resultFullTimeSpan.MobileWorkTime != resultFullTimeSpan.MobileWorkTimeWithoutCorrection)
				//{
				//    currRow[3] = "(" + EmailStats.WorkTimeWOCorrection + ": " +
				//                 resultFullTimeSpan.MobileWorkTimeWithoutCorrection.ToHourMinuteSecondString() + ")";
				//}
				table.AddRow(currRow.ToArray());
			}

			currRow.Clear();
			if (resultFullTimeSpan.ComputerWorkTime != TimeSpan.Zero)
			{
				currRow.AddRange(new[] { EmailStats.WorkTimeComputer, resultFullTimeSpan.ComputerWorkTime.ToHourMinuteSecondString(), null, null, null, null });
				//users don't care
				//if (resultFullTimeSpan.ComputerWorkTime != resultFullTimeSpan.NetComputerWorkTime)
				//{
				//    currRow[2] = "(" + EmailStats.WorkTimeNetto + ": " + resultFullTimeSpan.NetComputerWorkTime.ToHourMinuteSecondString() + ")";
				//}
				//if (resultFullTimeSpan.ComputerWorkTime != resultFullTimeSpan.ComputerWorkTimeWithoutCorrection)
				//{
				//    currRow[3] = "(" + EmailStats.WorkTimeWOCorrection + ": " +
				//                 resultFullTimeSpan.ComputerWorkTimeWithoutCorrection.ToHourMinuteSecondString() + ")";
				//}
				if (resultFullTimeSpan.RemoteDesktopComputerWorkTime != TimeSpan.Zero)
				{
					currRow[4] = "(" + EmailStats.WorkTimeRemoteDesktop + ": " +
								 resultFullTimeSpan.RemoteDesktopComputerWorkTime.ToHourMinuteSecondString() + ")";
				}
				if (resultFullTimeSpan.VirtualMachineComputerWorkTime != TimeSpan.Zero)
				{
					currRow[5] = "(" + EmailStats.WorkTimeVirtualMachine + ": " +
								 resultFullTimeSpan.VirtualMachineComputerWorkTime.ToHourMinuteSecondString() + ")";
				}
				table.AddRow(currRow.ToArray());
			}

			if (resultFullTimeSpan.ManuallyAddedWorkTime != TimeSpan.Zero)
			{
				table.AddRow(EmailStats.WorkTimeManuallyAdded, resultFullTimeSpan.ManuallyAddedWorkTime.ToHourMinuteSecondString());
			}

			if (resultFullTimeSpan.HolidayTime != TimeSpan.Zero)
			{
				table.AddRow(EmailStats.HolidayTime, resultFullTimeSpan.HolidayTime.ToHourMinuteSecondString());
			}

			if (resultFullTimeSpan.SickLeaveTime != TimeSpan.Zero)
			{
				table.AddRow(EmailStats.SickLeaveTime, resultFullTimeSpan.SickLeaveTime.ToHourMinuteSecondString());
			}

			if (reportType == ReportType.Daily && resultFullTimeSpan.WorkEndDate.HasValue && resultFullTimeSpan.WorkStartDate.HasValue)
			{
				string start, end;
				GetShortStartEndTimeStrings(resultFullTimeSpan.WorkStartDate.Value.FromUtcToDateTimeOffset(userStatInfo.TimeZone), resultFullTimeSpan.WorkEndDate.Value.FromUtcToDateTimeOffset(userStatInfo.TimeZone), out start, out end);
				table.AddRow(EmailStats.WorkStarted, start);
				table.AddRow(EmailStats.WorkEnd, end);
				table.AddRow(EmailStats.WorkStartEndDifference, (resultFullTimeSpan.WorkEndDate.Value - resultFullTimeSpan.WorkStartDate.Value).ToHourMinuteSecondString());
			}

			sb.AppendTable(table);
			sb.AppendLine();
		}

		private static void GetShortStartEndTimeStrings(DateTimeOffset startTime, DateTimeOffset endTime, out string start, out string end)
		{
			if (startTime.Date == endTime.Date && startTime.Offset == endTime.Offset) //short form
			{
				start = startTime.ToString("T");
				end = endTime.ToString("T");
			}
			else //long form
			{
				start = startTime.ToString(/* new CultureInfo("hu-HU") */); // to make date culture specific
				end = endTime.ToString(/* new CultureInfo("hu-HU") */);
			}
		}

		private static void AppendDailySumWorkTimeStats(DateTime startDate, DateTime endDate, UserEmailStats userStats, DetailedWorkTimeStatsBuilder statsBuilder, DetailedWorkTimeStats resultFullTimeSpan, Dictionary<DateTime, FullWorkTimeStats> workTimesForUser, EmailBuilder sb)
		{
			var table = new EmailTable();
			table.SetColumnStyle(1, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(2, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(3, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(4, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(5, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(6, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(7, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });

			table.Title = EmailStats.TimeStatByTime;
			table.SetHeader(EmailStats.TimeStatHeaderDate, EmailStats.TimeStatHeaderAll, EmailStats.TimeStatHeaderPhone, EmailStats.TimeStatHeaderSmartPhone, EmailStats.TimeStatHeaderComputerTime, EmailStats.TimeStatHeaderManuallyAdded, EmailStats.TimeStatHeaderHoliday, EmailStats.TimeStatHeaderSickLeave);


			var localReportDate = CalculatorHelper.GetLocalReportDate(startDate, userStats.UserStatInfo.TimeZone,
													 userStats.UserStatInfo.StartOfDayOffset);
			var startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localReportDate, userStats.UserStatInfo);
			while (startEnd.StartDate < endDate)
			{
				var currStartDate = startEnd.StartDate < startDate ? startDate : startEnd.StartDate;
				var currEndDate = startEnd.EndDate < endDate ? startEnd.EndDate : endDate;
				var detailedWorkTimeStats = statsBuilder.GetDetailedWorkTime(currStartDate, currEndDate);

				AppendBriefWorkTimeInfo(table, detailedWorkTimeStats, userStats.UserStatInfo, localReportDate);
				AddFullWorkTimeStats(workTimesForUser, detailedWorkTimeStats, userStats.UserStatInfo);

				localReportDate = localReportDate.AddDays(1);
				startEnd = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localReportDate, userStats.UserStatInfo);
			}

			table.AddRow();
			table.AddRow(
				EmailStats.TimeStatTotal,
				resultFullTimeSpan.SumWorkTime.ToHourMinuteSecondString(),
				resultFullTimeSpan.MobileWorkTime == TimeSpan.Zero ? "" : resultFullTimeSpan.MobileWorkTime.ToHourMinuteSecondString(),
				resultFullTimeSpan.ComputerWorkTime == TimeSpan.Zero ? "" : resultFullTimeSpan.ComputerWorkTime.ToHourMinuteSecondString(),
				resultFullTimeSpan.ManuallyAddedWorkTime == TimeSpan.Zero ? "" : resultFullTimeSpan.ManuallyAddedWorkTime.ToHourMinuteSecondString(),
				resultFullTimeSpan.HolidayTime == TimeSpan.Zero ? "" : resultFullTimeSpan.HolidayTime.ToHourMinuteSecondString(),
				resultFullTimeSpan.SickLeaveTime == TimeSpan.Zero ? "" : resultFullTimeSpan.SickLeaveTime.ToHourMinuteSecondString()
				);

			sb.AppendTable(table);
			sb.AppendLine();
		}

		private static void AppendBriefWorkTimeInfo(EmailTable tb, DetailedWorkTimeStats detailedStats, UserStatInfo userStatInfo, DateTime localReportDay)
		{
			if (detailedStats.SumWorkTime == TimeSpan.Zero
				&& detailedStats.ComputerWorkTime == TimeSpan.Zero
				&& detailedStats.ManuallyAddedTime == TimeSpan.Zero
				&& detailedStats.MobileWorkTime == TimeSpan.Zero
				)
			{
				return;
			}
			tb.AddRow(
				localReportDay.ToString("d") + localReportDay.ToString(" (ddd)"/*, new CultureInfo("hu-HU")*/), // to make date culture specific
				detailedStats.SumWorkTime.ToHourMinuteSecondString(),
				detailedStats.MobileWorkTime == TimeSpan.Zero ? "" : detailedStats.MobileWorkTime.ToHourMinuteSecondString(),
				detailedStats.ComputerWorkTime == TimeSpan.Zero ? "" : detailedStats.ComputerWorkTime.ToHourMinuteSecondString(),
				detailedStats.ManuallyAddedWorkTime == TimeSpan.Zero ? "" : detailedStats.ManuallyAddedWorkTime.ToHourMinuteSecondString(),
				detailedStats.HolidayTime == TimeSpan.Zero ? "" : detailedStats.HolidayTime.ToHourMinuteSecondString(),
				detailedStats.SickLeaveTime == TimeSpan.Zero ? "" : detailedStats.SickLeaveTime.ToHourMinuteSecondString()
				);
		}

		internal static EmailTable GetWorkTimeStatsTable(EmailBuilder sb, IEnumerable<IWorkTimeStat> workTimeStats, DateTime utcStartDate, DateTime utcEndDate, UserStatInfo userStatInfo)
		{
			if (!workTimeStats.Any()) return null; //Max will throw on Empty enumerable
			var table = new EmailTable { Title = EmailStats.TimeStatByWork };
			table.SetHeader(EmailStats.TimeStatHeaderWorkname, EmailStats.TimeStatHeaderTime, EmailStats.TimeStatHeaderPriority, EmailStats.TimeStatHeaderStart, EmailStats.TimeStatHeaderEnd, EmailStats.TimeStatHeaderElapsed, EmailStats.TimeStatHeaderTarget, EmailStats.TimeStatHeaderWorked, EmailStats.TimeStatHeaderWorkedPct, EmailStats.TimeStatHeaderStatus);
			table.SetColumnStyle(1, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(2, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(3, new EmailTable.HtmlStyle() { NoWrap = true, });
			table.SetColumnStyle(4, new EmailTable.HtmlStyle() { NoWrap = true, });
			table.SetColumnStyle(6, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(7, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			var workItemsNameLength = workTimeStats.OrderByDescending(n => n.WorkTime).ToArray();
			foreach (var itemsByWorkName in workItemsNameLength)
			{
				EmailTable.CellData name = null, workTime = null, pri = null, start = null, end = null, sePct = null, targetWorkTime = null, sumWorkTime = null, tPct = null, status = null;
				name = EmailTable.CellData.CreateFrom(itemsByWorkName.WorkName);
				workTime = EmailTable.CellData.CreateFrom(itemsByWorkName.WorkTime.ToHourMinuteSecondString());
				if (itemsByWorkName.DetailedWork != null)
				{
					var dWork = itemsByWorkName.DetailedWork; //for readability
					targetWorkTime = EmailTable.CellData.CreateFrom(dWork.TargetTotalWorkTime.ToHourMinuteString());
					status = GetStatusCellData(utcStartDate, utcEndDate, dWork.CloseDate, dWork.EndDate.HasValue && dWork.StartDate.HasValue, userStatInfo); //CloseDate is in utc
					if (dWork.Priority.HasValue)
					{
						pri = EmailTable.CellData.CreateFrom(dWork.Priority.ToString());
					}
					if (dWork.StartDate.HasValue)
					{
						start = EmailTable.CellData.CreateFrom(dWork.StartDate.Value.ToString("d"/*, new CultureInfo("hu-HU")*/)); // to make date culture specific
					}
					if (dWork.EndDate.HasValue)
					{
						end = EmailTable.CellData.CreateFrom(dWork.EndDate.Value.ToString("d"/*, new CultureInfo("hu-HU")*/)); // to make date culture specific
					}
					if (dWork.EndDate.HasValue && dWork.StartDate.HasValue)
					{
						//if the work is closed we only calculate until it was closed
						var endDate = dWork.CloseDate.HasValue ? dWork.CloseDate.Value : utcEndDate;
						var localEndDate = endDate.FromUtcToLocal(userStatInfo.TimeZone);

						var pct = CalculatorHelper.GetTargetEndDatePct(dWork.StartDate.Value,
								dWork.EndDate.Value,
								localEndDate);
						var delay = localEndDate - dWork.EndDate.Value.Date.AddDays(1);
						if (pct > 1 && delay > TimeSpan.Zero)
						{
							sePct = GetDateRangeCellData(delay, pct);
						}
						else
						{
							sePct = GetPercentageCellData(pct);
						}
					}
					TotalWorkTimeStat totalWorkTime = itemsByWorkName.TotalWorkTimeStat;
					if (dWork.TargetTotalWorkTime.HasValue && totalWorkTime != null)
					{
						sumWorkTime = EmailTable.CellData.CreateFrom(totalWorkTime.TotalWorkTime.ToHourMinuteString());
						var pct = CalculatorHelper.GetTargetWorkTimePct(dWork.TargetTotalWorkTime.Value,
								totalWorkTime.TotalWorkTime);
						var delay = totalWorkTime.TotalWorkTime - dWork.TargetTotalWorkTime.Value;
						if (pct > 1 && delay > TimeSpan.Zero)
						{
							tPct = GetDateRangeCellData(delay, pct);
						}
						else
						{
							tPct = GetPercentageCellData(pct);
						}
					}
				}

				table.AddRow(name, workTime, pri, start, end, sePct, targetWorkTime, sumWorkTime, tPct, status);
			}

			return table;
		}

		private const string dateRangeFormatString = "<span title=\"{1}\">{2}<span style=\"white-space:nowrap;color:{3};\">{0}</span></span>";
		private static EmailTable.CellData GetDateRangeCellData(TimeSpan range, float pct)
		{
			var color = pct > 1 ? "red" : "black";
			var text = DateRangeFormatter.Current.GetApproxRangeString(range);
			var barWithText = GetPercentageCellData(pct, "");
			var tooltip = range.ToHourMinuteSecondString() + " (" + pct.ToString("0.0%") + ")";
			return new EmailTable.CellData()
			{
				AsciiValue = text,
				HtmlValue = string.Format(dateRangeFormatString, "&nbsp;" + HttpUtility.HtmlEncode(text), HttpUtility.HtmlEncode(tooltip), barWithText.HtmlValue, color),
			};
		}

		//height=\"12px\" is needed for gmail, so it won't collapse it
		private const string pctTableFormatString = ""
			+ "<TABLE height=\"12px\" cellspacing=\"0\" cellpadding=\"0\" style=\"width: 3em; height: 1em; border: 1px solid black; float: left;\">"
			+ "<TR>"
			+ "<TD width=\"{0}\" style=\"background-color: {2};\">{3}</TD>"
			+ "<TD width=\"{1}\" style=\"background-color: white;\">{4}</TD>"
			+ "</TR>"
			+ "</TABLE>";
		private static EmailTable.CellData GetPercentageCellData(float pct, string htmlAfterBar = null)
		{
			var pctStr = pct.ToString("0.0%");

			var color = pct > 1 ? "red" : "blue";
			var leftStr = pct > 0.5 ? "&nbsp;" : "";
			var rightStr = pct > 0.5 ? "" : "&nbsp;";
			var compPct = (int)(CalculatorHelper.Clamp(pct, 0, 1) * 100);

			var result = new EmailTable.CellData()
			{
				AsciiValue = pctStr,
				HtmlValue = string.Format(pctTableFormatString, compPct + "%", (100 - compPct) + "%", color, leftStr, rightStr)
					+ (htmlAfterBar ?? "&nbsp;" + pctStr),
			};
			return result;
		}

		private const string openHtmlFormatString = "{0}";
		private const string recentlyClosedHtmlFormatString = "<span style=\"color:blue;\">{0}</span> (<span style=\"white-space:nowrap;\">{1}</span> <span style=\"white-space:nowrap;\">{2}</span>)";
		private const string oldClosedHtmlFormatString = "<span>{0}</span> (<span style=\"white-space:nowrap;\">{1}</span> <span style=\"white-space:nowrap;\">{2}</span>)";
		private static EmailTable.CellData GetStatusCellData(DateTime startDate, DateTime endDate, DateTime? closeDate, bool hasStartEndDate, UserStatInfo userStatInfo)
		{
			if (!closeDate.HasValue || endDate < closeDate.Value) //open
			{
				return new EmailTable.CellData()
				{
					AsciiValue = hasStartEndDate ? EmailStats.WorkStateOpened : EmailStats.WorkStateContiguous,
					HtmlValue = string.Format(openHtmlFormatString, HttpUtility.HtmlEncode(hasStartEndDate ? EmailStats.WorkStateOpened : EmailStats.WorkStateContiguous)),
				};
			}
			else if (startDate <= closeDate.Value && closeDate.Value <= endDate) // recently closed
			{
				var localCloseDate = closeDate.Value.FromUtcToLocal(userStatInfo.TimeZone).ToString("d"/*, new CultureInfo("hu-HU")*/); // to make date culture specific
				var localCloseTime = closeDate.Value.FromUtcToLocal(userStatInfo.TimeZone).ToString("T"/*, new CultureInfo("hu-HU")*/);
				return new EmailTable.CellData()
				{
					AsciiValue = EmailStats.WorkStateClosed + " (" + localCloseDate + ")",
					HtmlValue = string.Format(recentlyClosedHtmlFormatString, HttpUtility.HtmlEncode(EmailStats.WorkStateClosed), localCloseDate, localCloseTime),
				};
			}
			else //long time closed, we should not see this often, because you should not work on a work if its closed
			{    //but it could happen if the (offline) user was notified later that the work was closed
				var localCloseDate = closeDate.Value.FromUtcToLocal(userStatInfo.TimeZone).ToString("d"/*, new CultureInfo("hu-HU")*/); // to make date culture specific
				var localCloseTime = closeDate.Value.FromUtcToLocal(userStatInfo.TimeZone).ToString("T"/*, new CultureInfo("hu-HU")*/);
				return new EmailTable.CellData()
				{
					AsciiValue = EmailStats.WorkStateClosedAgo + " (" + localCloseDate + ")",
					HtmlValue = string.Format(oldClosedHtmlFormatString, HttpUtility.HtmlEncode(EmailStats.WorkStateClosedAgo), localCloseDate, localCloseTime),
				};
			}
		}

		private static string GetSubjectForUser(UserStatInfo userStatInfo, int userId, DateTime startDate,
												ReportType reportType, CultureInfo culture)
		{
			Debug.Assert(userStatInfo != null);
			var localReportDay = CalculatorHelper.GetLocalReportDate(startDate, userStatInfo.TimeZone,
																	 userStatInfo.StartOfDayOffset);
			var subject = GetSubjectInt(localReportDay, reportType, culture);
			var name = string.IsNullOrEmpty(userStatInfo.FirstName) && string.IsNullOrEmpty(userStatInfo.LastName)
						   ? ("userId: " + userId)
						   : culture.GetCultureSpecificName(userStatInfo.FirstName, userStatInfo.LastName);
			subject += EmailStats.SubjectStatistics + " " + name;
			return subject;
		}

		public static string GetSubjectForAggregatedEmail(DateTime startDate, ReportType reportType, CultureInfo culture)
		{
			var localReportDay = CalculatorHelper.GetLocalReportDate(startDate, TimeZoneInfo.Local,
																	 TimeSpan.FromHours(0));	//TODO: Consider to use UserStatInfo
			var subject = GetSubjectInt(localReportDay, reportType, culture);
			subject += EmailStats.AggregatedStatisticsSubject;
			return subject;
		}

		public static string GetSubjectForMobilLocationAggregatedEmail(UserStatInfo userStatInfo, DateTime startDate, ReportType reportType, CultureInfo culture)
		{
			var localReportDay = CalculatorHelper.GetLocalReportDate(startDate, userStatInfo.TimeZone,
														 userStatInfo.StartOfDayOffset);
			var subject = GetSubjectInt(localReportDay, reportType, culture);
			subject += EmailStats.AggregatedMobilLocationStatisticsSubject;
			return subject;
		}

		private static string GetSubjectInt(DateTime localReportDay, ReportType reportType, CultureInfo culture)
		{
			string reportTypeStr;
			switch (reportType)
			{
				case ReportType.Daily:
					reportTypeStr = localReportDay.ToString("yyyy-MM-dd");
					break;
				case ReportType.Weekly:
					reportTypeStr = string.Format(EmailStats.SubjectWeekly, localReportDay.Year, new GregorianCalendar().GetWeekOfYear(localReportDay, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek));
					break;
				case ReportType.Monthly:
					reportTypeStr = string.Format(EmailStats.SubjectMonthly, localReportDay.Year, localReportDay.ToString("MMMM"));
					break;
				default:
					throw new ArgumentOutOfRangeException("reportType");
			}
			var subject = "[JobCTRL] - " + reportTypeStr + " - ";
			return subject;
		}

		private static void AppendManualWorkItemStats(EmailBuilder sb, DetailedWorkTimeStats detailedStats, UserStatInfo userStatInfo)
		{
			var manunalItemsByType = detailedStats.ManualWorkItems
				.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddWork //users don't care about others
					|| n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddHoliday
					|| n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.AddSickLeave)
				.ToLookup(n => n.ManualWorkItemTypeId)
				.OrderBy(n => n.Key);
			foreach (var manunalItems in manunalItemsByType)
			{
				var table = new EmailTable();
				table.Title = GetManualWorkItemTypeEnumName(manunalItems.Key);

				foreach (var manunalItem in manunalItems)
				{
					var nameOrDesc = manunalItem.WorkId.HasValue ? WorkHierarchyService.Instance.GetWorkNameWithProjects(manunalItem.WorkId.Value) : GetManualWorkItemTypeEnumName(manunalItem.ManualWorkItemTypeId);
					var timeOrEmpty = manunalItems.Key == ManualWorkItemTypeEnum.AddWork
						? manunalItem.StartDate.FromUtcToLocal(userStatInfo.TimeZone).ToString("t") + " - " + manunalItem.EndDate.FromUtcToLocal(userStatInfo.TimeZone).ToString("t")
						: ""; //users don't care (don't confuse them)

					table.AddRow(
						EmailTable.CellData.CreateFrom(timeOrEmpty),
						EmailTable.CellData.CreateFrom(nameOrDesc),
						EmailTable.CellData.CreateFrom((manunalItem.EndDate - manunalItem.StartDate).ToHourMinuteSecondString()),
						EmailTable.CellData.CreateFrom(manunalItem.Comment)
						);
				}

				sb.AppendTable(table);
				sb.AppendLine();
			}
		}

		private static string GetManualWorkItemTypeEnumName(ManualWorkItemTypeEnum value)
		{
			switch (value)
			{
				case ManualWorkItemTypeEnum.AddWork:
					return EmailStats.ManualWorkItemAddWork;
				case ManualWorkItemTypeEnum.DeleteInterval:
					return EmailStats.ManualWorkItemDeleteInterval;
				case ManualWorkItemTypeEnum.DeleteIvrInterval:
					return EmailStats.ManualWorkItemDeleteIvrInterval;
				case ManualWorkItemTypeEnum.DeleteComputerInterval:
					return EmailStats.ManualWorkItemDeleteComputerInterval;
				case ManualWorkItemTypeEnum.DeleteMobileInterval:
					return EmailStats.ManualWorkItemDeleteMobileInverval;
				case ManualWorkItemTypeEnum.AddHoliday:
					return EmailStats.ManualWorkItemAddHoliday;
				case ManualWorkItemTypeEnum.AddSickLeave:
					return EmailStats.ManualWorkItemAddSickLeave;
				default:
					return EmailStats.ManualWorkItemUnknown;
			}
		}

		private static void LogInfoAndVerbose(string text, string opt)
		{
			log.Info(text);
			// detailed info for verbose logging
			log.Verbose(text + " " + opt);
		}

		/*
		private static void AppendWorkItemStats(EmailBuilder sb, DetailedWorkTimeStats detailedStats, WorkHierarchy works, Dictionary<int, DetailedWork> detailedWorks, UserStatInfo userStatInfo)
		{
			try
			{
				var table = new EmailTable() { Title = EmailStats.ComputerActivityByHour };
				table.SetColumnStyle(0, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
				table.SetColumnStyle(2, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
				table.SetColumnStyle(3, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
				table.SetColumnStyle(4, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
				table.SetColumnStyle(5, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
				table.SetHeader(EmailStats.ComputerActivityHeaderTime, EmailStats.ComputerActivityHeaderWork, EmailStats.ComputerActivityHeaderDuration, EmailStats.ComputerActivityHeaderKeyboardAct, EmailStats.ComputerActivityHeaderMouseAct, EmailStats.ComputerActivityHeaderComputer);

				var workItemsByTime = detailedStats.AggregateWorkItems
											   .OrderBy(n => n.StartDate)
											   .ThenBy(n => n.ComputerId)
											   .ThenBy(n => n.GroupId)
											   .ThenBy(n => n.CompanyId)
											   .ThenByDescending(n => n.WorkTime);

				string lastTime = null;

				foreach (var itemsByTime in workItemsByTime)
				{
					string time = itemsByTime.StartDate.FromUtcToLocal(userStatInfo.TimeZone).ToString("t") + " - ";
					if (lastTime == time)
					{
						time = "      - ";
					}
					else
					{
						lastTime = time;
					}
					table.AddRow(
						EmailTable.CellData.CreateFrom(time),
						EmailTable.CellData.CreateFrom(GetWorkName(itemsByTime.WorkId, works)),
						EmailTable.CellData.CreateFrom(GetWorkTimeFromMilliseconds(itemsByTime.WorkTime)),
						EmailTable.CellData.CreateFrom(GetAverageActivity(itemsByTime.KeyboardActivity, itemsByTime.WorkTime).ToString("0.00")),
						EmailTable.CellData.CreateFrom(GetAverageActivity(itemsByTime.MouseActivity, itemsByTime.WorkTime).ToString("0.00")),
						EmailTable.CellData.CreateFrom(itemsByTime.ComputerId.ToString())
						);
				}

				table.GetAsciiTable(sb.Body); // we only add this table to the plain/text part
				sb.Body.AppendLine();
				//sb.AppendTable(table);
				//sb.AppendLine();
			}
			catch (Exception ex)
			{
				log.Error("Unexpected error in AppendWorkItemStatsForUser", ex);
				throw;
			}
		}
		*/

		internal static TimeSpan GetTotalWorkTimeOrDefault(int workId, IDictionary<int, TotalWorkTimeStat> totalWorkTimeStats)
		{
			var totalWorkTimeStat = totalWorkTimeStats.GetValueOrDefault(workId);
			return totalWorkTimeStat == null ? TimeSpan.Zero : totalWorkTimeStat.TotalWorkTime;
		}

		private static string GetWorkTimeFromMilliseconds(int ms)
		{
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(ms);
			return timeSpan.ToHourMinuteSecondString();
		}

		private static double GetAverageActivity(int sumActivity, int workTimeInMs)
		{
			double timeInMin = workTimeInMs / 1000d / 60d;
			return sumActivity / timeInMin;
		}

		internal class WorkTimeStat : IWorkTimeStat
		{
			public string WorkName { get; set; }
			public TimeSpan WorkTime { get; set; }
			public DetailedWork DetailedWork { get; set; }
			public TotalWorkTimeStat TotalWorkTimeStat { get; set; }
		}

		private class UserEmailStats
		{
			public int UserId { get; set; }
			public IEnumerable<ManualWorkItem> ManualStats { get; set; }
			public IEnumerable<AggregateWorkItemInterval> IntervalStats { get; set; }
			public IEnumerable<MobileWorkItem> MobileStats { get; set; }
			public UserStatInfo UserStatInfo { get; set; }
		}
	}
}
