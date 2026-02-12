using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using Tct.ActivityRecorderService.Stats;

namespace Tct.ActivityRecorderService.EmailStats
{
	public static class EmailStatsAggregateHelper
	{
		//we have to use inline styles so Gmail can properly display these messages
		private const string ColorSum = "color: rgb(255, 0, 0);";
		private const string ColorIvr = "color: rgb(18, 175, 255);";
		private const string ColorComputer = "color: rgb(9, 125, 75);";
		private const string ColorManual = "color: rgb(124, 124, 124);";
		private const string ColorBgHoli = "background-color: rgb(224, 234, 239);";
		private const string ColorBgSick = "background-color: rgb(239, 224, 225);";
		private const string ColorBgWeekend = "background-color: rgb(234, 234, 215);";
		private const string ColorBgSum = "background-color: rgb(234, 225, 215);";
		private const string LeftStyle = " style=\"text-align: left;\"";
		private const string NameStyle = " style=\"color: rgb(0, 0, 0); font-weight: bold;\"";
		private const string NameStyleLink = " style=\"color: rgb(0, 0, 0); font-weight: bold; text-decoration:underline;\"";
		private const string SumWorkTimeStyle = " style=\"white-space:nowrap; " + ColorSum + " font-weight: bold;\"";
		private const string IvrWorkTimeStyle = " style=\"white-space:nowrap; " + ColorIvr + " font-size: 0.9em;\"";
		private const string ComputerWorkTimeStyle = " style=\"white-space:nowrap; " + ColorComputer + " font-size: 0.9em;\"";
		private const string ManuallyAddedTimeStyle = " style=\"white-space:nowrap; " + ColorManual + " font-size: 0.9em;\"";
		private const string WeekendStyle = " style=\"" + ColorBgWeekend + "\"";
		private const string SumStyle = " style=\"" + ColorBgSum + "\"";
		private const string HolidayStyle = " style=\"" + ColorBgHoli + "\"";
		private const string SickStyle = " style=\"" + ColorBgSick + "\"";
		private const string TableStyle = " style=\"font-family: Arial,sans-serif; font-size: 0.9em; background-color: rgb(244, 244, 235); text-align: right;\"";
		private const string TableHeaderStyle = " style=\"text-align: center;\"";

		internal static EmailToSendBase GetAggregateEmailToSend(DateTime startDate, DateTime endDate, ReportType reportType, IGrouping<string, EmailToSend> userEmails, string cultureId)
		{
			if (userEmails == null || !userEmails.Any()) return null;
			var culture = CultureInfo.GetCultureInfo(cultureId);
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
			var temp = DateTime.Now.ToLongDateString();
			var to = userEmails.Key;
			var subject = EmailStatsHelper.GetSubjectForAggregatedEmail(startDate, reportType, culture);

			var sbBodyHtml = new StringBuilder();
			sbBodyHtml.AppendLine("<HTML><HEAD></HEAD><BODY><A name=\"top\">&nbsp;</A>");
			AppendOrderedWorkTimeStat(sbBodyHtml, userEmails, culture);
			sbBodyHtml.Append("<BR/>");
			AppendAggregateTableData(sbBodyHtml, userEmails, culture);
			var resources = new List<EmailResource>();
			foreach (var userEmail in userEmails)
			{
				sbBodyHtml.Append("<HR><A" + NameStyle + " name=\"");
				sbBodyHtml.Append(userEmail.UserId);
				sbBodyHtml.Append("\">");
				sbBodyHtml.Append(HttpUtility.HtmlEncode(string.Format(EmailStats.AnybodysStatistic, culture.GetCultureSpecificName(userEmail.FirstName, userEmail.LastName))));
				sbBodyHtml.AppendLine("</A><HR>");
				sbBodyHtml.AppendLine(userEmail.EmailBuilder.BodyHtml.ToString());
				sbBodyHtml.AppendLine("<a style=\"font-family: Arial,sans-serif; font-size: 0.75em;\" href=\"#top\">" + EmailStats.LinkBackToTop + "</a>");
				if (userEmail.HtmlResources == null) continue;
				resources.AddRange(userEmail.HtmlResources);
			}
			sbBodyHtml.AppendLine("</BODY></HTML>");

			if (userEmails.All(n => !n.HasCredits)) //we only have users without credit
			{
				subject = EmailStatsHelper.ObfuscateWorkTimes(subject);
				//no plain text body
				EmailStatsHelper.ObfuscateWorkTimes(sbBodyHtml);
			}

			return new EmailToSendBase()
					{
						To = to,
						Subject = subject,
						Body = EmailStats.DefaultPlainTextBody,
						BodyHtml = sbBodyHtml.ToString(),
						HtmlResources = resources,
					};
		}

		private static void AppendOrderedWorkTimeStat(StringBuilder sbBodyHtml, IGrouping<string, EmailToSend> userEmails, CultureInfo culture)
		{
			sbBodyHtml.AppendLine("<TABLE BORDER=\"1\" cellspacing=\"0\" cellpadding=\"2\"" + TableStyle + ">");
			sbBodyHtml.AppendLine("<TR><TH" + TableHeaderStyle + ">" + EmailStats.WorkTimeName + "</TH><TH" + TableHeaderStyle + ">" + EmailStats.WorkTimeAll + "</TH></TR>");
			foreach (var email in userEmails.OrderByDescending(n => n.FullWorkTime.SumWorkTime))
			{
				sbBodyHtml.Append("<TR><TD" + LeftStyle + "><A" + NameStyleLink + " href=\"#");
				sbBodyHtml.Append(email.UserId);
				sbBodyHtml.Append("\">");
				sbBodyHtml.Append(HttpUtility.HtmlEncode(culture.GetCultureSpecificName(email.FirstName, email.LastName)));
				sbBodyHtml.AppendLine("</A></TD>");
				sbBodyHtml.Append("<TD><SPAN" + SumWorkTimeStyle + ">");
				sbBodyHtml.Append(email.FullWorkTime.SumWorkTime.ToHourMinuteSecondString());
				sbBodyHtml.AppendLine("</SPAN></TD></TR>");
			}
			sbBodyHtml.AppendLine("</TABLE>");
		}

		private static void AppendAggregateTableData(StringBuilder sbBodyHtml, IGrouping<string, EmailToSend> userEmails, CultureInfo culture)
		{
			var days = (from email in userEmails
						from day in email.WorkTimes.Keys
						select day).Distinct().ToList();
			days.Sort();

			sbBodyHtml.AppendLine("<TABLE BORDER=\"1\" cellspacing=\"0\" cellpadding=\"2\"" + TableStyle + ">");
			//Table header
			sbBodyHtml.AppendLine("<TR><TH" + TableHeaderStyle + ">" + EmailStats.WorkTimeName + "</TH>");
			foreach (var day in days)
			{
				sbBodyHtml.Append("<TH" + TableHeaderStyle + ">");
				sbBodyHtml.Append(HttpUtility.HtmlEncode(day.ToString("MMM", culture)));
				sbBodyHtml.Append("<BR/>");
				sbBodyHtml.Append(HttpUtility.HtmlEncode(day.ToString("d.", culture)));
				sbBodyHtml.Append("<BR/>");
				sbBodyHtml.Append(HttpUtility.HtmlEncode(day.ToString("ddd.", culture)));
				sbBodyHtml.AppendLine("</TH>");
			}
			if (days.Count > 1) //sum stats
			{
				sbBodyHtml.Append("<TH" + TableHeaderStyle + ">" + EmailStats.TimeStatTotal + "</TH>");
			}
			sbBodyHtml.AppendLine("</TR>");

			//Table data
			foreach (var email in userEmails)
			{
				sbBodyHtml.Append("<TR><TD" + LeftStyle + "><A" + NameStyleLink + " href=\"#");
				sbBodyHtml.Append(email.UserId);
				sbBodyHtml.Append("\">");
				sbBodyHtml.Append(HttpUtility.HtmlEncode(culture.GetCultureSpecificName(email.FirstName, email.LastName)));
				sbBodyHtml.AppendLine("</A></TD>");
				foreach (var day in days)
				{
					sbBodyHtml.Append("<TD");
					if (email.WorkTimes.ContainsKey(day)
						&& (email.WorkTimes[day].HolidayTime != TimeSpan.Zero || email.WorkTimes[day].SickLeaveTime != TimeSpan.Zero))
					{
						if (email.WorkTimes[day].HolidayTime != TimeSpan.Zero)
						{
							sbBodyHtml.Append(HolidayStyle);
						}
						else
						{
							sbBodyHtml.Append(SickStyle);
						}
					}
					else
					{
						sbBodyHtml.Append(GetWeekStyle(day.DayOfWeek));
					}
					sbBodyHtml.Append(">");
					if (email.WorkTimes.ContainsKey(day))
					{
						AppendAggregateCellData(sbBodyHtml, email.WorkTimes[day]);
					}
					sbBodyHtml.AppendLine("</TD>");
				}
				if (days.Count > 1) //sum stats
				{
					sbBodyHtml.Append("<TD" + SumStyle + ">");
					AppendAggregateCellData(sbBodyHtml, email.FullWorkTime);
					sbBodyHtml.AppendLine("</TD>");
				}
				sbBodyHtml.AppendLine("</TR>");
			}

			sbBodyHtml.AppendLine("</TABLE>");

			//Table desc
			sbBodyHtml.Append("<BR/>");
			sbBodyHtml.Append("<SPAN style=\"" + ColorSum + "font-size: 0.9em; font-weight: bold;\">" + EmailStats.WorkTimeAll + "</SPAN><BR/>");
			sbBodyHtml.Append("<SPAN style=\"" + ColorIvr + "font-size: 0.9em; font-weight: bold;\">" + EmailStats.WorkTimePhoneAndSmartPhone + "</SPAN><BR/>");
			sbBodyHtml.Append("<SPAN style=\"" + ColorComputer + "font-size: 0.9em; font-weight: bold;\">" + EmailStats.WorkTimeComputer + "</SPAN><BR/>");
			sbBodyHtml.Append("<SPAN style=\"" + ColorManual + "font-size: 0.9em; font-weight: bold;\">" + EmailStats.ManualWorkItemAddWork + "</SPAN><BR/>");
			sbBodyHtml.Append("<SPAN style=\"" + ColorBgHoli + ColorManual + "font-size: 0.9em; font-weight: bold;\">" + EmailStats.HolidayTime + "</SPAN><BR/>");
			sbBodyHtml.Append("<SPAN style=\"" + ColorBgSick + ColorManual + "font-size: 0.9em; font-weight: bold;\">" + EmailStats.SickLeaveTime + "</SPAN><BR/>");
			//sbBodyHtml.Append("<SPAN style=\"" + ColorBgWeekend + "font-size: 0.9em; font-weight: bold;\">Hétvége</SPAN><BR/>");
			sbBodyHtml.Append("<BR/>");
		}

		private static string GetWeekStyle(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Sunday:
					return WeekendStyle;
				case DayOfWeek.Saturday:
					return WeekendStyle;
				default:
					return "";
			}
		}

		private static void AppendAggregateCellData(StringBuilder sbBodyHtml, FullWorkTimeStats stats)
		{
			sbBodyHtml.Append("<SPAN" + SumWorkTimeStyle + ">");
			sbBodyHtml.Append(stats.SumWorkTime.ToHourMinuteString());
			sbBodyHtml.Append("</SPAN><BR/>");
			if (stats.ComputerWorkTime != TimeSpan.Zero)
			{
				sbBodyHtml.Append("<SPAN" + ComputerWorkTimeStyle + ">");
				sbBodyHtml.Append(stats.ComputerWorkTime.ToHourMinuteString());
				sbBodyHtml.Append("</SPAN>");
			}
			else
			{
				sbBodyHtml.Append("&nbsp;");
			}
			sbBodyHtml.Append("<BR/>");
			if (stats.ManuallyAddedTime != TimeSpan.Zero)
			{
				sbBodyHtml.Append("<SPAN" + ManuallyAddedTimeStyle + ">");
				sbBodyHtml.Append(stats.ManuallyAddedTime.ToHourMinuteString());
				sbBodyHtml.Append("</SPAN>");
			}
			else
			{
				sbBodyHtml.Append("&nbsp;");
			}
		}

		internal static IEnumerable<AggregateEmailGroup> GetAggregateEmailGroups(IEnumerable<EmailToSend> userEmails, ReportType reportType)
		{
			var aggregateEmailRequests = StatsDbHelper.GetAggregateEmailRequests();
			return GetAggregateEmailGroups(userEmails, reportType, aggregateEmailRequests);
		}


		internal static IEnumerable<AggregateEmailGroup> GetAggregateEmailGroups(IEnumerable<EmailToSend> allEmails, ReportType reportType, IEnumerable<AggregateEmailRequest> aggregateEmailRequests)
		{
			var freq = EmailStatsAutoSendHelper.GetReportFrequencyFromType(reportType);
			var availableUserIds = new HashSet<int>(allEmails.Select(n => n.UserId));
			var relevantAggrRequests = aggregateEmailRequests
				.Where(n => (n.Frequency & freq) != 0)
				.Where(n => n.UserIds.Any(i => availableUserIds.Contains(i)))
				.ToList();

			//merge aggregate reports that contains the same users so we don't have to generate them separately
			var mergedRequests = relevantAggrRequests
				.Select(n => new
				{
					UserIds = n.UserIds.Where(i => availableUserIds.Contains(i)).OrderBy(i => i).ToList(),
					EmailsTo = n.EmailsTo
				})
				.ToLookup(n => string.Join("_", n.UserIds)); //unique userid set

			foreach (var mergedRequest in mergedRequests)
			{
				var currentUserIds = new HashSet<int>(mergedRequest.First().UserIds); //UserId list is the same for all
				var currentEmails = mergedRequest.Select(n => n.EmailsTo).SelectMany(n => n).Distinct(); //Flatten emails and we don't want to send the same email more than once

				yield return new AggregateEmailGroup()
				{
					EmailsTo = currentEmails.ToList(),
					EmailsToAggregate = allEmails.Where(n => currentUserIds.Contains(n.UserId)).OrderBy(n => n.SortKey).ToList(),
				};
			}
		}

		internal class AggregateEmailGroup
		{
			public List<EmailTarget> EmailsTo { get; set; }
			public List<EmailToSend> EmailsToAggregate { get; set; }

			public Dictionary<string, EmailToSendBase> GenerateAggregatedEmail(DateTime startDate, DateTime endDate, ReportType reportType)
			{
				return EmailsTo.Select(e => string.IsNullOrEmpty(e.CultureId)
					                            ? EmailStatsHelper.DefaultCulture
					                            : e.CultureId)
				               .Distinct()
				               .ToDictionary(culture => culture,
				                             culture =>
				                             EmailStatsAggregateHelper.GetAggregateEmailToSend(startDate, endDate, reportType,
					                             EmailsToAggregate.GroupBy(n => "").First(), culture));
			}
		}
	}
}