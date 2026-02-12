using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderService.Properties;
using Tct.ActivityRecorderService.WebsiteServiceReference;
using log4net;

namespace Tct.ActivityRecorderService.EmailStats
{
	public static class EmailMobileLocationStatsHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static EmailResource markerImage = new EmailResource((byte[])new ImageConverter().ConvertTo(Resources.google_marker_blue, typeof(byte[])), "image/png");

		public static void Send(UserStatInfo user, DateTime utcStartDate, DateTime utcEndDate, ReportType reportType)
		{
#if DEBUG
			return;
#endif
			string ticket;
			using (var context = new JobControlDataClassesDataContext())
			{
				ticket = context.GetAuthTicket(user.Id);
			}
			StayingLocationsReportItem[] reportItems;
			using (var client = new Website.WebsiteClientWrapper())
			{
				reportItems = client.Client.GetMobileStayingLocationsReport(new Guid(ticket), utcStartDate.FromUtcToLocal(user.TimeZone), utcEndDate.FromUtcToLocal(user.TimeZone));
				if (reportItems.Length == 0) return;
			}

			var culture = CultureInfo.GetCultureInfo(user.CultureId);
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;

			var emailBuilder = new EmailBuilder();

			var subject = EmailStatsHelper.GetSubjectForMobilLocationAggregatedEmail(user, utcStartDate, reportType, culture);
			var users = reportItems.Length > 0
				            ? StatsDbHelper.GetUserStatsInfo(reportItems.Select(r => r.UserId).ToList()).ToDictionary(k => k.Id, v => v)
				            : new Dictionary<int, UserStatInfo>();

			foreach (var reportUser in reportItems)
			{
				CreateLocationReport(emailBuilder, reportUser, users[reportUser.UserId], culture, user);
			}
			if (emailBuilder.BodyHtml.Length == 0) return;
			emailBuilder.HtmlResources.Add(markerImage);

			var body = emailBuilder.Body.ToString();
			var bodyHtml = emailBuilder.BodyHtml.ToString();
			var htmlResources = emailBuilder.HtmlResources;

			var mailTo = user.Email;
			log.Info("Sending " + reportType + " email To: " + mailTo + " Sub: " + subject + " Body: " +
			         Environment.NewLine + body);
			EmailManager.Instance.Send(new EmailMessage()
				{
					To = mailTo,
					Subject = subject,
					PlainBody = body,
					HtmlBody = bodyHtml,
					HtmlResources = htmlResources,
				});

		}

		private static void CreateLocationReport(EmailBuilder emailBuilder, StayingLocationsReportItem reportUser, UserStatInfo reportUserStatInfo, CultureInfo culture, UserStatInfo user)
		{
			if (string.IsNullOrEmpty(reportUser.ImageUrl) && reportUser.SummaryOfUser.Length == 0 && reportUser.LocationsOfUser.Length == 0) return;
			emailBuilder.BodyHtml.AppendLine("<div>");
			emailBuilder.BodyHtml.AppendLine("<div>");
			emailBuilder.AppendLine(string.Format(EmailStats.EmailMobileLocationStatsHelper_TitleFormat, culture.GetCultureSpecificName(reportUserStatInfo.FirstName, reportUserStatInfo.LastName), reportUser.LocalStartDate.ToString("d"), reportUser.LocalEndDate.ToString("d"), reportUser.SummaryOfUser.Length, reportUser.LocationsOfUser.Length));
			emailBuilder.BodyHtml.AppendLine("</div>");
			emailBuilder.BodyHtml.AppendLine("<div style=\"display: block;\">");
			emailBuilder.BodyHtml.AppendLine("<img src=\"" + reportUser.ImageUrl + "\" style=\"height:600px;width:900px;\"/>");
			emailBuilder.BodyHtml.AppendLine("<table style=\"width:900px;\" cellspacing=\"5\"><tbody><tr><td></td><td></td></tr>");
			foreach (var item in reportUser.SummaryOfUser)
			{
				AddScene(emailBuilder, item, user);
			}
			emailBuilder.BodyHtml.AppendLine("</tbody></table><p3><b>");
			emailBuilder.AppendLine(EmailStats.EmailMobileLocationStatsHelper_EventsInTimeOrder);
			emailBuilder.BodyHtml.AppendLine("</b></p3><table style=\"width:900px;\" cellspacing=\"5\"><tbody>");
			var flf = false;
			foreach (var location in reportUser.LocationsOfUser)
			{
				if (flf)
					emailBuilder.BodyHtml.AppendLine("<tr><td colspan=\"3\"><hr></td></tr>");
				else
					flf = true;
				AddEvent(emailBuilder, location, user);
			}
			emailBuilder.BodyHtml.AppendLine("</tbody></table>");
			emailBuilder.BodyHtml.AppendLine("<br/><br/>");
			emailBuilder.BodyHtml.AppendLine("</div>");
			emailBuilder.BodyHtml.AppendLine("</div>");
		}

		private static void AddScene(EmailBuilder emailBuilder, LongStayingMarkerTitleSummaryReportItem item, UserStatInfo user)
		{
			emailBuilder.BodyHtml.AppendLine("<tr>");
			AddMarker(emailBuilder, item.MarkerTitle);
			emailBuilder.BodyHtml.AppendLine("<td valign=\"top\">");
			emailBuilder.AppendLine(EmailStats.EmailMobileLocationStatsHelper_Address + ": " + item.Address);
			emailBuilder.AppendLine(string.Format(EmailStats.EmailMobileLocationStatsHelper_SceneCount, item.NumberOfItems));
			var totalTime = TimeSpan.FromMinutes(item.TotalTimeInMins);
			emailBuilder.AppendLine(EmailStats.EmailMobileLocationStatsHelper_TotalTime + ": " +
			                        string.Format(EmailStats.EmailMobileLocationStatsHelper_TimeSpanFormatHourMinutes,
			                                      (int) totalTime.TotalHours, totalTime.Minutes));
			emailBuilder.BodyHtml.Append("<br/><b>");
			emailBuilder.AppendLine(EmailStats.EmailMobileLocationStatsHelper_Events + ":");
			emailBuilder.BodyHtml.AppendLine("</b>");
			emailBuilder.BodyHtml.AppendLine("<table style=\"width:100%\" cellspacing=\"5\"><tbody>");
			var index = 1;
			foreach (var eventItem in item.Events)
			{
				emailBuilder.BodyHtml.AppendLine("<tr><td valign=\"top\">");
				emailBuilder.Append(index++ + ".");
				emailBuilder.BodyHtml.AppendLine("</td>");
				AddSceneEvent(emailBuilder, eventItem, user);
				emailBuilder.BodyHtml.AppendLine("</tr>");
			}
			emailBuilder.BodyHtml.AppendLine("</tbody></table>");
			emailBuilder.BodyHtml.AppendLine("</td>");
			emailBuilder.BodyHtml.AppendLine("</tr>");
		}

		private static void AddMarker(EmailBuilder emailBuilder, string marker)
		{
			emailBuilder.BodyHtml.AppendLine("<td style=\"width:30px\" valign=\"top\"><table><tbody><tr>");
			emailBuilder.BodyHtml.AppendLine("<td style=\"" +
											 // css background-image not supported by the most email readers
			                                 //"background-image:url('cid:" + markerImage.ContentId + "');background-repeat:no-repeat;background-position:0px 8px;" +
			                                 "width:19px;height:36px;text-align:center;color:#000000;font-weight:bold;\">" +
											 "<img src=\"cid:"+markerImage.ContentId+"\" />" +
			                                 marker + "</td>");
			emailBuilder.Body.AppendLine(EmailStats.EmailMobileLocationStatsHelper_Scene + marker);
			emailBuilder.BodyHtml.AppendLine("</tr></tbody></table></td>");
		}

		private static void AddSceneEvent(EmailBuilder emailBuilder, MarkedLocationReportItem eventItem, UserStatInfo user)
		{
			emailBuilder.BodyHtml.AppendLine("<td valign=\"top\">");
			emailBuilder.Append(eventItem.StartTime.FromUtcToLocal(user.TimeZone).ToString("f") + " - " + eventItem.EndTime.FromUtcToLocal(user.TimeZone).ToString("t"));
			emailBuilder.AppendLine(" (" +
									DateRangeFormatter.Current.GetApproxRangeString(eventItem.StartTime, eventItem.EndTime) + ")");	//Don't need to convert from utc to local, because the difference between end and start is the same.
			emailBuilder.BodyHtml.AppendLine(
				"<table cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td style=\"white-space:nowrap; padding-right:10px\" valign=\"top\">");
			emailBuilder.Append(EmailStats.EmailMobileLocationStatsHelper_TasksUserWorkedOn + ": ");
			emailBuilder.BodyHtml.AppendLine("</td><td>");
			emailBuilder.AppendLine(string.Join(", ", eventItem.WorksWithFullName));
			emailBuilder.BodyHtml.AppendLine("</td></tr></tbody></table>");
			emailBuilder.BodyHtml.AppendLine(
				"<table cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td style=\"white-space:nowrap; padding-right:10px\" valign=\"top\">");
			emailBuilder.Append(EmailStats.EmailMobileLocationStatsHelper_Reasons + ":");
			emailBuilder.BodyHtml.AppendLine("</td><td>");
			AddReasonTable(emailBuilder, eventItem, user);
			emailBuilder.BodyHtml.AppendLine("</td></tr></tbody></table></td>");
		}

		private static void AddEvent(EmailBuilder emailBuilder, MarkedLocationReportItem location, UserStatInfo user)
		{
			emailBuilder.BodyHtml.AppendLine("<tr>");
			AddMarker(emailBuilder, location.MarkerTitle);
			emailBuilder.BodyHtml.AppendLine("<td valign=\"top\">");
			emailBuilder.Append(location.StartTime.FromUtcToLocal(user.TimeZone).ToString("f") + " - " + location.EndTime.FromUtcToLocal(user.TimeZone).ToString("t"));
			emailBuilder.AppendLine(" (" +
									DateRangeFormatter.Current.GetApproxRangeString(location.StartTime, location.EndTime) + ")");	//Don't need to convert from utc to local, because the difference between end and start is the same.
			emailBuilder.AppendLine();
			emailBuilder.AppendLine(EmailStats.EmailMobileLocationStatsHelper_Address + ": " + location.Address);
			emailBuilder.BodyHtml.AppendLine(
				"<table cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td style=\"white-space:nowrap; padding-right:10px\" valign=\"top\">");
			emailBuilder.Append(EmailStats.EmailMobileLocationStatsHelper_TasksUserWorkedOn + ": ");
			emailBuilder.BodyHtml.AppendLine("</td><td>");
			emailBuilder.AppendLine(string.Join(", ", location.WorksWithFullName));
			emailBuilder.BodyHtml.AppendLine("</td></tr></tbody></table>");
			emailBuilder.BodyHtml.AppendLine(
				"<table cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td style=\"white-space:nowrap; padding-right:10px\" valign=\"top\">");
			emailBuilder.Append(EmailStats.EmailMobileLocationStatsHelper_Reasons + ":");
			emailBuilder.BodyHtml.AppendLine("</td><td>");
			AddReasonTable(emailBuilder, location, user);
			emailBuilder.BodyHtml.AppendLine("</td></tr></tbody></table>");
			emailBuilder.BodyHtml.AppendLine("</td>");
			emailBuilder.BodyHtml.AppendLine("</tr>");
		}

		private static void AddReasonTable(EmailBuilder emailBuilder, MarkedLocationReportItem eventItem, UserStatInfo user)
		{
			if (eventItem.Reasons.Length > 0)
			{
				var reasonsTable = new EmailTable();
				foreach (var reason in eventItem.Reasons)
				{
					reasonsTable.AddRow(new[] { reason.Date.FromUtcToLocal(user.TimeZone).ToString("G") + " ", reason.ReasonItemWithFullName + " ", reason.ReasonText });
				}
				emailBuilder.AppendTable(reasonsTable);
			}
			else
				emailBuilder.AppendLine("-");
		}

	}
}
