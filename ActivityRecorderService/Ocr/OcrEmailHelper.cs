using log4net;
using Ocr.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Tct.ActivityRecorderService.EmailStats;

namespace Tct.ActivityRecorderService.Ocr
{
	public class OcrEmailHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void SendEmail(OcrStatsHelper recogStats, int ruleId, bool isLearningSuccessful)
		{
			using (var context = new JobControlDataClassesDataContext())
			{
				List<EmailResource> HtmlResources = new List<EmailResource>();
				var admins = (from e in context.GetUserStatsInfo()
							  where e.CompanyId == recogStats.stats[StatsTypeEnum.CompanyId] && e.AccessLevel == UserAccessLevel.Adm
							  select e.Email).ToList();
				if (!String.IsNullOrEmpty(ConfigManager.OcrLearningIssueNotificationEmailAddress))
				{
					admins.Clear();
					admins.Add(ConfigManager.OcrLearningIssueNotificationEmailAddress);
				}

				log.Debug("OCR Sending Email");
				var eb = new EmailBuilder();
				eb.Body.AppendLine("OCR results: " + ruleId);
				eb.BodyHtml.AppendLine("<h1>OCR results: " + ruleId + "</h1>");
				string resultText = isLearningSuccessful ? "Learning finished without errors" : "Learning finished with errors";
				eb.Body.AppendLine(resultText);
				eb.BodyHtml.AppendLine("<h2>" + resultText + "</h2><hr>");
				eb.Body.AppendLine("Stats of this rule:");
				eb.BodyHtml.AppendLine("<h2>" + "Stats of this rule:" + "</h2>");
				AddStatLine(eb, "New samples count", recogStats.stats[StatsTypeEnum.NewSnippetsCount]);
				AddStatLine(eb, "Number of iterations", recogStats.stats[StatsTypeEnum.Iterations]);
				AddStatLine(eb, "Elapsed minutes", recogStats.stats[StatsTypeEnum.ElapsedMinutes]);
				AddStatLine(eb, "All snippets", GetSnippetCount(ruleId));
				AddStatLine(eb, "Marked as incorrect", GetBadSnippetCount(ruleId));
				AddStatLine(eb, "Days since the collection started", GetMaxSnippetAge(ruleId));

				eb.Body.AppendLine("Training stats:");                              //just in dev stage, will be removed from email
				eb.BodyHtml.AppendLine("<h2>" + "Training stats:" + "</h2>");
				AddTrainingStats(eb);                                     

				if (0 < recogStats.SampleStorage.ErrorCounter)
				{
					eb.BodyHtml.AppendLine("<hr><h2>The following images couldn't be recognized:</h2>");
					eb.BodyHtml.Append(
						"<TABLE border=\"0\" cellspacing=\"2\"><TR><TD><B>Image</B></TD><TD><B>Quality</B></TD><TD><B>Expected value</B></TD><TD><B>Recognized as</B></TD></TR>");
					foreach (var item in recogStats.SampleStorage.GetProblematicItems)
					{
						var img = new EmailResource(item.Key, "image/png");
						var userName = context.GetUserStatInfoById(item.Value.Item3).Name;
						eb.BodyHtml
							.Append("<TR><TD><img src=\"cid:")
							.Append(img.ContentId)
							.Append("\"/></TD><TD>")
							.Append(HttpUtility.HtmlEncode(item.Value.Item5))
							.Append("</TD><TD><div style=\"font-family:Verdana;\">")
							.Append(HttpUtility.HtmlEncode(item.Value.Item1))
							.Append("</div></TD><TD><div style=\"font-family:Verdana;\">")
							.Append(HttpUtility.HtmlEncode(item.Value.Item2))
							.Append("</div></TD><TD>")
							.Append(HttpUtility.HtmlEncode(userName))
							.Append("<br>")
							.Append(HttpUtility.HtmlEncode(item.Value.Item4))
							.Append("</TD></TR>");
						HtmlResources.Add(img);
					}
					eb.BodyHtml.Append("</TABLE>");
				}
				ThreadPool.QueueUserWorkItem(_ =>
				{
					foreach (var admin in admins)
						EmailHelper.Send(admin, "OCR results: " + ruleId, eb.GetPlainText(), eb.GetHtmlText(), HtmlResources);
				}, null);
				recogStats.RemoveStats();
				log.Debug("OCR email sent");
			}
		}

		public static Guid GetRegistratorUsersTicket(int companyId)
		{
			using (var context = new JobControlDataClassesDataContext())
			{
				var userId = (from e in context.GetUserStatsInfo()
							  where e.CompanyId == companyId && e.AccessLevel == UserAccessLevel.Reg
							  select e.Id).FirstOrDefault();
				return userId != 0
					? new Guid(context.GetAuthTicket(userId))
					: Guid.Empty;
			}
		}

		private void AddStatLine(EmailBuilder eb, string title, int data)
		{
			eb.Body.AppendLine(String.Format("{0}: {1}", title, data));
			eb.BodyHtml.AppendLine(String.Format("{0}: {1} <br />", title, data));
		}

		private int GetSnippetCount(int ruleId)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				return context.Snippets.Count(snippet => snippet.RuleId == ruleId);
			}
		}

		private int GetBadSnippetCount(int ruleId)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				return context.Snippets.Count(snippet => snippet.RuleId == ruleId && snippet.IsBadData == true);
			}
		}

		private int GetMaxSnippetAge(int ruleId)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var firstSnippet = context.Snippets.Where(snippet => snippet.RuleId == ruleId).OrderBy(snippet => snippet.CreatedAt).FirstOrDefault();
				return (DateTime.UtcNow - firstSnippet.CreatedAt).Days;
			}
		}

		//just in dev stage, will be removed
		private void AddTrainingStats(EmailBuilder eb)
		{
			AddStatLine(eb, "Number of timeouts", OcrEngineStatsHelper.GetTimeouts());
			AddStatLine(eb, "Number of trainings", OcrEngineStatsHelper.GetTotal());
			AddStatLine(eb, "Max training length", OcrEngineStatsHelper.GetMax());
			AddStatLine(eb, "Average training length", OcrEngineStatsHelper.GetAverage());
		}

		//just in dev stage, will be removed
		private void AddStatLine(EmailBuilder eb, string title, double data)
		{
			eb.Body.AppendLine(String.Format("{0}: {1}", title, data));
			eb.BodyHtml.AppendLine(String.Format("{0}: {1} <br />", title, data));
		}
	}
}
