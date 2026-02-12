using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Serialization;
using log4net;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class RecentUrlMenuItem : ToolStripMenuItem
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string RecentUrlPath { get { return "RecentUrl-" + ConfigManager.UserId; } }
		public List<ToolStripMenuItem> UrlItems { get; private set; }
		private readonly Dictionary<string, string> urlNameLookup;

		public RecentUrlMenuItem()
		{
			UrlItems = new List<ToolStripMenuItem>() 
			{
				new ToolStripMenuItem(Labels.Menu_AcMain + "...", null, UrlClicked) { Tag = GetUrlFormatString("MyAccount.aspx"), },
				new ToolStripMenuItem(Labels.Menu_AcHolSick + "...", null, UrlClicked) { Tag = GetUrlFormatString("Holidays.aspx"), },
				new ToolStripMenuItem(Labels.Menu_AcManual + "...", null, UrlClicked) { Tag = GetUrlFormatString("ModifyReports.aspx"), },
				new ToolStripMenuItem(Labels.Menu_AcWorks + "...", null, UrlClicked) { Tag = GetUrlFormatString("MyTasks.aspx"), },
				new ToolStripMenuItem(Labels.Menu_AcOnlineMon + "...", null, UrlClicked) { Tag = GetUrlFormatString("OnlineMonitoringHtml/Default.aspx"), },
				new ToolStripMenuItem(Labels.Menu_AcMeetings + "...", null, UrlClicked) { Tag = GetUrlFormatString("Meetings/MeetingApproval.aspx"), },
				new ToolStripMenuItem(Labels.Menu_AcReports + "...", null, UrlClicked) { Tag = GetUrlFormatString("Reports/CentralReports.aspx"), },
				new ToolStripMenuItem(Labels.Menu_AcDashboard + "...", null, UrlClicked) { Tag = GetUrlFormatString("Dashboard/"), },
				new ToolStripMenuItem(Labels.TODOs + "...", null, UrlClicked) {Tag =  GetUrlFormatString("TodoLists/") },
			};
			Debug.Assert(UrlItems.Count > 1);
			Debug.Assert(UrlItems.All(n => !string.IsNullOrEmpty(n.Tag as string)));
			urlNameLookup = UrlItems.Where(n => !string.IsNullOrEmpty(n.Tag as string)).ToDictionary(n => (string)n.Tag, n => n.Text);
			SetNewLink(UrlItems[0].Tag as string, false); //fallback text and url
			string urlFormat;
			if (!IsolatedStorageSerializationHelper.Exists(RecentUrlPath)) return;
			if (!IsolatedStorageSerializationHelper.Load(RecentUrlPath, out urlFormat)) return;
			SetNewLink(urlFormat, false); //this will not set the link if invalid
		}

		private static string GetUrlFormatString(string page)
		{
			return ConfigManager.WebsiteUrlFormatString + page;
		}

		private void UrlClicked(object sender, EventArgs e)
		{
			OpenLink(((ToolStripMenuItem)sender).Tag as string, true);
		}

		private void OpenLink(string urlFormat, bool setNewLink)
		{
			if (urlFormat == null) return;
			if (setNewLink) SetNewLink(urlFormat, true);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var url = "";
				try
				{
					var ticket = AuthenticationHelper.GetAuthTicket();
					url = string.Format(urlFormat, ticket);
					var sInfo = new ProcessStartInfo(url);
					Process.Start(sInfo);
				}
				catch (Exception ex)
				{
					log.Error("Unable to open url: " + url, ex);
				}
			});
		}

		private void SetNewLink(string urlFormat, bool saveLink)
		{
			if (urlFormat == null) return;
			string text;
			if (!urlNameLookup.TryGetValue(urlFormat, out text))
			{
				Debug.Fail(urlFormat);
				return;
			}
			this.Text = Labels.Menu_MainPage + " " + text;
			this.Tag = urlFormat;
			if (!saveLink) return;
			IsolatedStorageSerializationHelper.Save(RecentUrlPath, urlFormat);
		}

		public void RaiseClick()
		{
			OnClick(EventArgs.Empty);
		}

		protected override void OnClick(EventArgs e)
		{
			OpenLink(this.Tag as string, false);

			base.OnClick(e);
		}
	}
}
