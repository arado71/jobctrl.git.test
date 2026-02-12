using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.UI.DataVisualization.Charting;

namespace Tct.ActivityRecorderService.EmailStats
{
	//todo refactor (merge?) EmailBuilder and EmailToSendBase...
	//Ascii is improper naming, it should be plain text...
	public class EmailBuilder
	{
		public StringBuilder Body { get; private set; }
		public StringBuilder BodyHtml { get; private set; }
		public readonly List<EmailResource> HtmlResources = new List<EmailResource>();

		public EmailBuilder()
		{
			Body = new StringBuilder();
			BodyHtml = new StringBuilder();
		}

		public string GetPlainText()
		{
			return Body.ToString();
		}

		public string GetHtmlText()
		{
			return "<HTML><HEAD></HEAD><BODY>" + BodyHtml.ToString() + "</BODY></HTML>";
		}

		public EmailBuilder Append(string text)
		{
			Body.Append(text);
			BodyHtml.Append(HttpUtility.HtmlEncode(text));
			return this;
		}

		public EmailBuilder AppendLine()
		{
			AppendLine(string.Empty);
			return this;
		}

		public EmailBuilder AppendLine(string line)
		{
			Body.AppendLine(line);
			BodyHtml.Append(HttpUtility.HtmlEncode(line));
			BodyHtml.Append("<BR/>");
			return this;
		}

		public EmailBuilder AppendLines(string lines)
		{
			foreach (var line in lines.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
			{
				AppendLine(line);
			}
			return this;
		}

		public EmailBuilder AppendFormat(string format, string text)
		{
			Body.AppendFormat(format, text);
			BodyHtml.AppendFormat(format, HttpUtility.HtmlEncode(text));
			return this;
		}

		public EmailBuilder AppendTable(EmailTable table)
		{
			if (table == null) return this;
			table.GetAsciiTable(Body);
			table.GetHtmlTable(BodyHtml);
			return this;
		}

		public EmailBuilder AppendHtmlChart(Chart chart, string title)
		{
			if (chart == null) return this;
			var resourceChart = new EmailResource(chart.ToPng(), "image/png");
			BodyHtml
				.Append("<TABLE border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><TR><TD style=\"text-align:center;\"><B><EM>")
				.Append(HttpUtility.HtmlEncode(title))
				.Append("</EM></B></TD></TR><TR><TD>")
				.Append("<img src=\"cid:")
				.Append(resourceChart.ContentId)
				.Append("\"/></TD></TR></TABLE>");
			HtmlResources.Add(resourceChart);
			return this;
		}

		public EmailBuilder AppendLink(string url, string text = null)
		{
			var plainText = string.IsNullOrWhiteSpace(text) ? "" : text.Trim() + " ";
			Body.Append(plainText + "(" + url + ")");
			var htmlText = string.IsNullOrWhiteSpace(text) ? url : text.Trim();
			BodyHtml.Append("<A href=\"" + HttpUtility.HtmlAttributeEncode(url) + "\">" + HttpUtility.HtmlEncode(htmlText) + "</A>");
			return this;
		}
	}
}