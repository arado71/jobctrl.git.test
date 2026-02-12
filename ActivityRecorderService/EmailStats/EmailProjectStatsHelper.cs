using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using log4net;
using Tct.ActivityRecorderService.Caching.Works;
using Tct.ActivityRecorderService.Stats;

namespace Tct.ActivityRecorderService.EmailStats
{
	//Input params: (do we need to batch these?)
	//reportUserId     - requester that needs the report (and will receive it in email) this will be used for authorization
	//to               - also send to these addresses (as separate emails)
	//projectRootId(s) - that should be in the report (only if the authorization passes)
	//isInternal       - what type of wage are we looking for. (for inside report, for outside client report etc...)
	//subject?         - if we have more projectIds what should be the subject (l8r)
	//startDate
	//endDate
	//
	//Queries:
	//List<int> GetReportableProjectRootsForUser(int reportUserId, bool isInternal) - to which projects has the user access
	//Wage GetWageData(int workId, int userId, int reportUserId, bool isInternal) - what wage does the user see
	//List<reportUserId, to, projectRootId(s), isInternal, subj?, reportType(flags): daily1/weekly2/monthly4 > GetProjectEmailRequests() - which reports should be sent automatically
	// that means 3 result sets:
	// 1: reportId, reportUserId, isInternal, reportType
	// 2: reportId, projectRootId
	// 3: reportId, to
	//
	//
	//Types: internalWithWage, internalWithoutWage, externalWithWage
	//instead of type use bool isInternal ?
	//
	//we calculate work times many times for a given user which is not ideal (even for users that are not in the report!)
	//todo cache totalWorkTimes (1sec/user/email), cache workTimes (or IntervalWorkTimeStatsBuilder), ignore users without worktime ?
	//it takes 1:16h to generate ~340 project reports for a Sunday and 1:40h for the whole week (2013-10-13)
	//work names are cached now (2014-02-12)
	public static class EmailProjectStatsHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Send(DateTime utcStartDate, DateTime utcEndDate, int reportUserId, bool isInternal, List<int> projectRootIds, List<EmailTarget> toAddresses, ILookup<int?, Project> projectLookup = null)
		{
			try
			{
				if (projectLookup == null) projectLookup = StatsDbHelper.GetProjectsById().Values.ToLookup(n => n.ParentId);
				var applicableUsers = new HashSet<int>(StatsDbHelper.GetUserIdsInSameCompany(reportUserId));
				var allEmailStatUsers = StatsDbHelper.GetUserStatsInfo(null).Where(n => applicableUsers.Contains(n.Id)).ToDictionary(n => n.Id);
				var culturesNeeded = toAddresses.Select(a => string.IsNullOrEmpty(a.CultureId) ? EmailStatsHelper.DefaultCulture : a.CultureId).Distinct().ToList();
				var emailSkeletonDict = GetEmailToSend(utcStartDate, utcEndDate, reportUserId, isInternal, projectRootIds,
												   allEmailStatUsers, culturesNeeded, projectLookup);

				if (emailSkeletonDict == null)
				{
					log.Info("Nothing to report in project email for user: " + reportUserId + " from: " + utcStartDate + " to: " + utcEndDate);
					return;
				}

				foreach (var emailTo in toAddresses)
				{
					var cultureId = string.IsNullOrEmpty(emailTo.CultureId) ? EmailStatsHelper.DefaultCulture : emailTo.CultureId;
					var emailSkeleton = emailSkeletonDict[cultureId];
					var emailToSend = new EmailToSendBase()
										{
											To = emailTo.Address,
											Body = emailSkeleton.Body,
											BodyHtml = emailSkeleton.BodyHtml,
											Subject = emailSkeleton.Subject,
										};
					log.Info("Sending project email To: " + emailToSend.To + " Sub: " + emailToSend.Subject + " Culture: " + emailTo.CultureId + " Body: " +
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
			}
			catch (Exception ex)
			{
				log.Error("Unable to generate project email for user: " + reportUserId + " from: " + utcStartDate + " to: " + utcEndDate, ex);
			}
		}

		internal static Dictionary<string, EmailToSendBase> GetEmailToSend(DateTime utcStartDate, DateTime utcEndDate, int reportUserId, bool isInternal, List<int> projectRootIds, Dictionary<int, UserStatInfo> userIdsFilterDict, List<string> culturesNeeded, ILookup<int?, Project> projectLookup)
		{
			var projTree = GetEmptyProjectCostTree(projectLookup);

			var reportableProjects = StatsDbHelper.GetReportableProjectRootsForUser(reportUserId, isInternal);
			var filteredProjectRootIds = GetFilteredProjectRoots(projectRootIds, reportableProjects, projTree);
			if (filteredProjectRootIds.Count == 0)
			{
				log.Warn("Nothing to report for user: " + reportUserId);
				return null;
			}

			return GetEmailToSend(utcStartDate, utcEndDate, reportUserId, isInternal, filteredProjectRootIds, projTree, userIdsFilterDict, culturesNeeded);
		}

		internal static Dictionary<string, EmailToSendBase> GetEmailToSend(DateTime startDate, DateTime endDate, int reportUserId, bool isInternal, List<int> projectRootIds, ProjectCostTree projTree, Dictionary<int, UserStatInfo> userIdsFilterDict, List<string> culturesNeeded)
		{
			var allUsersStats = userIdsFilterDict
				.Select(n => new
				{
					UserId = n.Key,
					UserStatInfo = n.Value,
					Stats = StatsDbHelper.GetAggregateWorkItemIntervalsForUserCovered(n.Key, startDate, endDate),
					ManualStats = StatsDbHelper.GetManualWorkItemsForUserCovered(n.Key, startDate, endDate),
					MobileStats = StatsDbHelper.GetMobileWorkItemsForUserCovered(n.Key, startDate, endDate),
				});

			var allWorkCost = new List<UserWorkCost>();
			var userWithoutCredit = new HashSet<int>();

			foreach (var userStats in allUsersStats)
			{
				try
				{
					log.Debug("Fetching work details for user " + userStats.UserId);
					var detailedWorks = StatsDbHelper.GetDetailedWorkForUserCached(userStats.UserId);
					log.Debug("Fetching total work times for user " + userStats.UserId);
					var totalWorkTimes = StatsDbHelper.GetTotalWorkTimeByWorkIdForUserCached(userStats.UserId, endDate);
					log.Debug("Checking credits for user " + userStats.UserId);
					var localDays = EmailStatsHelper.GetLocalDaysForInterval(userStats.UserStatInfo, startDate, endDate).ToList();
					var hasCredits = StatsDbHelper.HasUserGotCreditForInterval(userStats.UserId, localDays.First(), localDays.Last());
					if (!hasCredits) userWithoutCredit.Add(userStats.UserId);

					var statsBuilder = new IntervalWorkTimeStatsBuilder();
					//statsBuilder.AddAggregateWorkItems(userStats.Stats);
					statsBuilder.RefreshManualWorkItems(userStats.ManualStats);
					statsBuilder.RefreshAggregateWorkItemIntervals(userStats.Stats);
					statsBuilder.RefreshMobileWorkItems(userStats.MobileStats);
					log.Debug("Created IntervalWorkTimeStatsBuilder for user " + userStats.UserId);
					var resultFullTimeSpan = statsBuilder.GetIntervalWorkTime(startDate, endDate);
					log.Debug("Created interval stats for the entire timespan for user " + userStats.UserId + " stats: " + resultFullTimeSpan);

					var workCosts = GetWorkCostsForUser(reportUserId, isInternal, userStats.UserId, userStats.UserStatInfo, resultFullTimeSpan.WorkIntervalsById, detailedWorks, totalWorkTimes);

					allWorkCost.AddRange(workCosts);
				}
				catch (Exception ex)
				{
					log.Error("Unable to create interval stat for userId: " + userStats.UserId, ex);
#if DEBUG
					throw;
#endif
				}
			}

			var emailDict = new Dictionary<string, EmailToSendBase>(culturesNeeded.Count);

			foreach (string cultureId in culturesNeeded)
			{
				var culture = CultureInfo.GetCultureInfo(cultureId);
				Thread.CurrentThread.CurrentCulture = culture;
				Thread.CurrentThread.CurrentUICulture = culture;

				AddUserWorkCostsToProjectCostTree(projTree, allWorkCost);
				ProjectCost projToReport;
				var subject = new StringBuilder("[JobCTRL] - " + EmailStats.ProjectStatistics);
				if (projectRootIds.Count == 1)
				{
					projToReport = projTree.Dict[projectRootIds[0]];
					subject.Append(" - ").Append(projToReport.ProjectName);
				}
				else
				{
					//artificial project for easy summary
					projToReport = new ProjectCost()
						{
							ProjectName = EmailStats.ProjectStatSummaryProjectName,
							ProjectId = 0,
							Childrens = new List<ProjectCost>(),
							UserWorkCosts = new List<UserWorkCost>()
						};
					var prefix = " - ";
					foreach (var projId in projectRootIds)
					{
						var proj = projTree.Dict[projId];
						projToReport.Childrens.Add(proj);
						subject.Append(prefix).Append(proj.ProjectName);
						prefix = ", ";
					}
				}

				var reportUserTimeZone = GetUserTimeZoneOrUtc(userIdsFilterDict, reportUserId);
				subject.Append(" - (")
					   .Append(startDate.FromUtcToLocal(reportUserTimeZone).ToString("yyyy-MM-dd"))
					   .Append(" - ")
					   .Append(endDate.FromUtcToLocal(reportUserTimeZone).ToString("yyyy-MM-dd"))
					   .Append(")");

				subject.Append(" (" + EmailStats.AllWorkShort + ": ")
					   .Append(projToReport.GetProjectWorkTimeAndCost().WorkTime.ToHourMinuteSecondString())
					   .Append(")");

				if (projToReport.GetProjectWorkTimeAndCost().WorkTime == TimeSpan.Zero)
				{
					log.Info("Total work time was zero for project email so skipping it");
					return null;
				}

				var email = new EmailToSendProject()
					{
						ProjectId = projToReport.ProjectId,
						Name = projToReport.ProjectName,
						Cost = projToReport,
						//To = will be set later
						Subject = subject.ToString(),
						EmailBuilder = new EmailBuilder(),
					};

				email.EmailBuilder.BodyHtml.Append("<A name=\"top\">&nbsp;</A>");
				AppendProjectCostSummary(email.EmailBuilder, projToReport, startDate, endDate, DateTime.UtcNow, reportUserTimeZone);
				AppendProjectCostTree(email.EmailBuilder, projToReport, true);
				AppendProjectCostTree(email.EmailBuilder, projToReport, false);
				AppendSumWorkTimeForEveryUserInProject(email.EmailBuilder, projToReport, userIdsFilterDict, culture);
				AppendWorkTimeStatsForEveryUserInProject(email.EmailBuilder, projToReport, userIdsFilterDict, startDate, endDate, culture);

				email.EmailBuilder.AppendLine();
				EmailStatsHelper.AppendWorkTimeDescriptions(email.EmailBuilder);

				if (isInternal)
				{
					email.EmailBuilder.AppendLine();
					email.EmailBuilder.AppendLine(EmailStats.IsInternal);
				}

				if (projToReport.GetFlattenedWorkCosts().Select(n => n.UserId).Distinct().Any(n => userWithoutCredit.Contains(n))) //if there is a user without credit
				{
					email.Subject = EmailStatsHelper.ObfuscateWorkTimes(email.Subject);
					EmailStatsHelper.ObfuscateWorkTimes(email.EmailBuilder.Body);
					EmailStatsHelper.ObfuscateWorkTimes(email.EmailBuilder.BodyHtml);

					email.EmailBuilder.AppendLine();
					email.EmailBuilder.AppendLines(EmailStats.NoCreditText);
				}

				string body = email.EmailBuilder.Body.ToString();
				string htmlBody = "<HTML><HEAD></HEAD><BODY>"
								  + email.EmailBuilder.BodyHtml
								  + "</BODY></HTML>";

				email.Body = body;
				email.BodyHtml = htmlBody;

				emailDict.Add(cultureId, email);
			}

			return emailDict;
		}

		private static TimeZoneInfo GetUserTimeZoneOrUtc(Dictionary<int, UserStatInfo> userIdsFilterDict, int reportUserId)
		{
			UserStatInfo reportUserStatInfo;
			if (userIdsFilterDict.TryGetValue(reportUserId, out reportUserStatInfo))
			{
				return reportUserStatInfo.TimeZone ?? TimeZoneInfo.Utc;
			}
			return TimeZoneInfo.Utc;
		}

		private const string NameStyleLink = " style=\"color: rgb(0, 0, 0); text-decoration:underline;\"";
		private static void AppendSumWorkTimeForEveryUserInProject(EmailBuilder sb, ProjectCost projectCost, Dictionary<int, UserStatInfo> userStatInfos, CultureInfo culture)
		{
			var table = new EmailTable();
			table.SetHeader(EmailStats.ProjectStatWorkerName, EmailStats.ProjectStatWorkTime, EmailStats.ProjectStatCost);
			table.SetColumnStyle(1, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(2, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.Title = EmailStats.ProjectStatWorkers;
			foreach (var workCost in projectCost.GetFlattenedWorkCosts()
				.ToLookup(n => n.UserId)
				.Select(n => new
				{
					Id = n.Key,
					Name = culture.GetCultureSpecificName(userStatInfos[n.Key].FirstName, userStatInfos[n.Key].LastName),
					WorkTime = new TimeSpan(n.Sum(m => m.WorkTime.Ticks)),
					Cost = n.Aggregate(new decimal?(0m), (prev, m) => prev + m.Cost),
					//CostNotNull = n.Aggregate(0m, (prev, m) => prev + m.Cost ?? 0m)
				})
				.OrderByDescending(n => n.WorkTime))
			{
				table.AddRow(
					new EmailTable.CellData()
					{
						AsciiValue = workCost.Name,
						HtmlValue = "<A" + NameStyleLink + " href=\"#" + workCost.Id + "\">" + HttpUtility.HtmlEncode(workCost.Name) + "</A>",
					},
					EmailTable.CellData.CreateFrom(workCost.WorkTime.ToHourMinuteSecondString()),
					EmailTable.CellData.CreateFrom(FormatDecimal(workCost.Cost))
				);
			}
			sb.AppendTable(table);
			sb.AppendLine();
		}

		private static void AppendWorkTimeStatsForEveryUserInProject(EmailBuilder sb, ProjectCost projectCost, Dictionary<int, UserStatInfo> userStatInfos, DateTime utcStartDate, DateTime utcEndDate, CultureInfo culture)
		{
			foreach (var workCost in projectCost.GetFlattenedWorkCosts().ToLookup(n => n.UserId).OrderBy(n => userStatInfos[n.Key].Name))
			{
				var userId = workCost.Key;
				var userStatInfo = userStatInfos[userId];
				var workTimeStats = workCost.Cast<IWorkTimeStat>(); //no 4.0 :(
				var table = EmailStatsHelper.GetWorkTimeStatsTable(sb, workTimeStats, utcStartDate, utcEndDate, userStatInfo);
				if (table == null) continue;
				var name = culture.GetCultureSpecificName(userStatInfo.FirstName, userStatInfo.LastName);
				table.Title = string.Format(EmailStats.ProjectStatAnybodysWorks, name);
				sb.BodyHtml.Append("<HR><A name=\"").Append(userId).Append("\">"); ;
				sb.AppendTable(table);
				sb.BodyHtml.Append("</A>");
				sb.BodyHtml.AppendLine("<BR/><a style=\"font-family: Arial,sans-serif; font-size: 0.75em;\" href=\"#top\">" + EmailStats.LinkBackToTop + "</a>");
				sb.AppendLine();
			}
		}

		private static readonly EmailTable.Style rightAlign = new EmailTable.Style() { Align = EmailTable.TextAlign.Right };
		private static void AppendProjectCostSummary(EmailBuilder sb, ProjectCost projectCost, DateTime startDate, DateTime endDate, DateTime generateTime, TimeZoneInfo reportUsersTimeZone)
		{
			var table = new EmailTable();
			var timeAndCost = projectCost.GetProjectWorkTimeAndCost();
			table.AddRow(EmailTable.CellData.CreateFrom(EmailStats.ProjectStatAllWorktime), EmailTable.CellData.CreateFrom(timeAndCost.WorkTime.ToHourMinuteSecondString(), rightAlign));
			if (timeAndCost.Cost.HasValue)
			{
				table.AddRow(EmailTable.CellData.CreateFrom(EmailStats.ProjectStatAllCost), EmailTable.CellData.CreateFrom(FormatDecimal(timeAndCost.Cost), rightAlign));
			}
			table.AddRow(EmailTable.CellData.CreateFrom(EmailStats.ProjectStatStartDate), EmailTable.CellData.CreateFrom(startDate.FromUtcToDateTimeOffset(reportUsersTimeZone).ToString("G"), rightAlign));
			table.AddRow(EmailTable.CellData.CreateFrom(EmailStats.ProjectStatEndDate), EmailTable.CellData.CreateFrom(endDate.FromUtcToDateTimeOffset(reportUsersTimeZone).ToString("G"), rightAlign));
			table.AddRow(EmailTable.CellData.CreateFrom(EmailStats.ProjectStatCreated), EmailTable.CellData.CreateFrom(generateTime.FromUtcToDateTimeOffset(reportUsersTimeZone).ToString("G"), rightAlign));
			sb.AppendTable(table);
			sb.AppendLine();
		}

		private const string indent = "    ";
		private const string projectPrefix = "\u25CF ";
		private const string workPrefix = "\u2022 ";
		private const string userPrefix = "- ";
		private static readonly EmailTable.HtmlStyle boldHtmlStyle = new EmailTable.HtmlStyle() { Bold = true };
		private static readonly EmailTable.HtmlStyle workHtmlStyle = new EmailTable.HtmlStyle() { EscapeSpace = true };
		private static readonly EmailTable.HtmlStyle userHtmlStyle = new EmailTable.HtmlStyle() { EscapeSpace = true, ForeColor = "#068" };
		private static readonly EmailTable.HtmlStyle userNameHtmlStyle = new EmailTable.HtmlStyle() { EscapeSpace = true, Italic = true, ForeColor = "#068" };
		private static void AppendProjectCostTree(EmailBuilder sb, ProjectCost projectCost, bool projectsOnly)
		{
			var table = new EmailTable();
			table.SetHeader(EmailStats.ProjectStatWorkerName, EmailStats.ProjectStatWorkTime, EmailStats.ProjectStatCost);
			table.SetColumnStyle(1, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(2, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(3, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.SetColumnStyle(4, new EmailTable.Style() { Align = EmailTable.TextAlign.Right });
			table.Title = projectsOnly ? EmailStats.ProjectStatSummaryProjects : EmailStats.ProjectStatSummaryWorkers;

			var stack = new Stack<ProjectCostWithIndent>();
			stack.Push(new ProjectCostWithIndent() { ProjectCost = projectCost, Indent = "", });
			while (stack.Count > 0)
			{
				var currProj = stack.Pop();
				foreach (var child in currProj.ProjectCost.Childrens.OrderByDescending(n => n.ProjectName))
				{
					if (child.GetProjectWorkTimeAndCost().WorkTime == TimeSpan.Zero) continue; //don't display empty child projects
					stack.Push(new ProjectCostWithIndent() { ProjectCost = child, Indent = currProj.Indent + indent, });
				}
				var timeAndCost = currProj.ProjectCost.GetProjectWorkTimeAndCost();

				var linkType = projectsOnly ? NameStyleLink + " href=\"#p" : " name=\"p";
				table.AddRow(
					new EmailTable.CellData()
					{
						AsciiValue = currProj.Indent + projectPrefix + currProj.ProjectCost.ProjectName,
						HtmlValue = (currProj.Indent + projectPrefix).Replace(" ", "&nbsp;")
							+ "<A" + linkType + currProj.ProjectCost.ProjectId + "\">"
							+ HttpUtility.HtmlEncode(currProj.ProjectCost.ProjectName) + "</A>",
						CellStyle = projectsOnly ? null : boldHtmlStyle,
					},
					EmailTable.CellData.CreateFrom(timeAndCost.WorkTime.ToHourMinuteSecondString(), projectsOnly ? null : boldHtmlStyle),
					EmailTable.CellData.CreateFrom(FormatDecimal(timeAndCost.Cost), projectsOnly ? null : boldHtmlStyle)
					);

				if (projectsOnly) continue;
				var usersByWorkId = currProj.ProjectCost.UserWorkCosts.ToLookup(n => n.WorkId).OrderBy(n => n.First().WorkName);
				foreach (var users in usersByWorkId)
				{
					var first = users.First();
					var workName = first.WorkShortName;
					var cost = users.Aggregate(new decimal?(0m), (prev, n) => prev + n.Cost);//Sum will return 0 on null decimal
					var workTime = new TimeSpan(users.Sum(n => n.WorkTime.Ticks));
					table.AddRow(
						EmailTable.CellData.CreateFrom(currProj.Indent + indent + workPrefix + workName, workHtmlStyle),
						EmailTable.CellData.CreateFrom(workTime.ToHourMinuteSecondString(), workHtmlStyle),
						EmailTable.CellData.CreateFrom(FormatDecimal(cost), workHtmlStyle));
					foreach (var user in users.OrderBy(n => n.UserName))
					{
						table.AddRow(
							EmailTable.CellData.CreateFrom(currProj.Indent + indent + indent + userPrefix + user.UserName, userNameHtmlStyle),
							EmailTable.CellData.CreateFrom(user.WorkTime.ToHourMinuteSecondString(), userHtmlStyle),
							EmailTable.CellData.CreateFrom(FormatDecimal(user.Cost), userHtmlStyle));
					}
				}
			}
			sb.AppendTable(table);
			sb.AppendLine();
		}

		private static string FormatDecimal(decimal? value)
		{
			return value.HasValue ? value.Value.ToString("#,##0.0") : "";
		}

		private class ProjectCostWithIndent
		{
			public ProjectCost ProjectCost { get; set; }
			public string Indent { get; set; }
		}

		private static void AddUserWorkCostsToProjectCostTree(ProjectCostTree projTree, List<UserWorkCost> allWorkCost)
		{
			foreach (var workCost in allWorkCost.ToLookup(n => n.WorkId))
			{
				var workId = workCost.Key;
				Work work;
				if (!WorkHierarchyService.Instance.TryGetWork(workId, out work))
				{
					log.Error("Unable to get work for workId: " + workId);
					continue;
				}
				ProjectCost projCost;
				if (!projTree.Dict.TryGetValue(work.ProjectId, out projCost))
				{
					log.Error("Unable to get ProjectCost for ProjectId: " + work.ProjectId);
					continue;
				}
				//todo test wrong method projCost.WorkCosts = workCost.ToList();
				projCost.UserWorkCosts.AddRange(workCost);
			}
		}

		internal static ProjectCostTree GetEmptyProjectCostTree(ILookup<int?, Project> parentLookup)
		{
			var newItems = parentLookup[null];
			var roots = newItems.Select(n => GetProjectCost(n)).ToList();
			var added = roots.ToDictionary(n => n.ProjectId);
			var addedParents = new Queue<int>(roots.Select(n => n.ProjectId));
			while (addedParents.Count > 0)
			{
				var currParentId = addedParents.Dequeue();
				newItems = parentLookup[currParentId];
				foreach (var item in newItems)
				{
					addedParents.Enqueue(item.Id);
					var add = GetProjectCost(item);
					added[item.ParentId.Value].Childrens.Add(add);
					added.Add(add.ProjectId, add);
				}
			}
			return new ProjectCostTree() { Tree = roots, Dict = added };
		}

		private static ProjectCost GetProjectCost(Project project)
		{
			if (project == null) return null;
			return new ProjectCost()
			{
				ProjectId = project.Id,
				ProjectName = project.Name,
				Childrens = new List<ProjectCost>(),
				UserWorkCosts = new List<UserWorkCost>(),
			};
		}

		private static List<UserWorkCost> GetWorkCostsForUser(int reportUserId, bool isInternal, int userId, UserStatInfo userStatInfo, Dictionary<int, List<IntervalConcatenator.Interval>> workIntervalsById, Dictionary<int, DetailedWork> detailedWorks, Dictionary<int, TotalWorkTimeStat> totalWorkStats)
		{
			var resultList = new List<UserWorkCost>();
			foreach (var workIntervals in workIntervalsById)
			{
				var result = new UserWorkCost()
				{
					UserId = userId,
					UserName = userStatInfo.Name,
					WorkId = workIntervals.Key,
					WorkShortName = WorkHierarchyService.Instance.GetWorkName(workIntervals.Key), //we don't need project names in the tree view
					WorkName = WorkHierarchyService.Instance.GetWorkNameWithProjects(workIntervals.Key),
					DetailedWork = detailedWorks.GetValueOrDefault(workIntervals.Key),
					TotalWorkTimeStat = totalWorkStats.GetValueOrDefault(workIntervals.Key),
				};
				var wage = StatsDbHelper.GetWageData(result.WorkId, result.UserId, reportUserId, isInternal); //todo too much roundtrips we have to reduce them
				if (wage != null) result.Cost = 0;

				long ticks = 0;
				foreach (var interval in workIntervals.Value)
				{
					if (wage != null) result.Cost += wage.GetCostFor(interval.StartDate, interval.EndDate);
					ticks += interval.EndDate.Ticks - interval.StartDate.Ticks;
				}
				result.WorkTime = new TimeSpan(ticks);

				resultList.Add(result);
			}
			return resultList;
		}

		private static List<int> GetFilteredProjectRoots(List<int> interestedProjectIds, List<int> reportableProjectIds, ProjectCostTree projTree)
		{
			//we don't trust input so we won't assume that interestedProjectIds and reportableProjectIds only contains roots
			var result = new List<int>(); //result will only contain roots
			var allReportableIds = new HashSet<int>();
			foreach (var repId in reportableProjectIds)
			{
				ProjectCost proj;
				if (!projTree.Dict.TryGetValue(repId, out proj))
				{
					log.Error("Cannot find reportable project with id: " + repId);
				}
				else
				{
					allReportableIds.Add(proj.ProjectId);
					foreach (var childProj in proj.GetFlattenedChildrenProjects())
					{
						allReportableIds.Add(childProj.ProjectId);
					}
				}
			}
			//we've gathered allReportableIds so intersect it with the ones we are interested in
			var interestedAndReportableIds = allReportableIds;
			allReportableIds = null;
			interestedAndReportableIds.IntersectWith(interestedProjectIds);

			//get the root ids of reportable and interested projects
			var stack = new Stack<ProjectCost>();
			foreach (var proj in projTree.Tree)
			{
				stack.Push(proj);
			}
			while (stack.Count > 0)
			{
				var proj = stack.Pop();
				if (interestedAndReportableIds.Contains(proj.ProjectId))
				{
					result.Add(proj.ProjectId);
				}
				else
				{
					foreach (var childProj in proj.Childrens)
					{
						stack.Push(childProj);
					}
				}
			}
			return result;
		}

		internal class ProjectCostTree
		{
			public List<ProjectCost> Tree { get; set; }
			public Dictionary<int, ProjectCost> Dict { get; set; }
		}
	}
}
