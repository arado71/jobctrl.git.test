using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using MonoMac.AppKit;
using Tct.ActivityRecorderClient.Serialization;
using System.Threading;
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public class RecentUrlNSMenuItem : TaggableNSMenuItem
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string RecentUrlPath { get { return "RecentUrl-" + ConfigManager.UserId; } }

		public IList<TaggableNSMenuItem> UrlItems { get; private set; }

		private readonly Dictionary<string, string> urlNameLookup;

		public RecentUrlNSMenuItem()
			: base("", OnClick)
		{
			var urlItems = new List<TaggableNSMenuItem>()
			{
				new TaggableNSMenuItem(Labels.Menu_AcMain, UrlClicked) { TagObject = GetUrlFormatString("MyAccount.aspx"), },
				new TaggableNSMenuItem(Labels.Menu_AcHolSick, UrlClicked) { TagObject = GetUrlFormatString("Holidays.aspx"), },
				new TaggableNSMenuItem(Labels.Menu_AcManual, UrlClicked) { TagObject = GetUrlFormatString("ModifyReports.aspx"), },
				new TaggableNSMenuItem(Labels.Menu_AcWorks, UrlClicked) { TagObject = GetUrlFormatString("MyTasks.aspx"), },
				new TaggableNSMenuItem(Labels.Menu_AcOnlineMon, UrlClicked) { TagObject = GetUrlFormatString("OnlineMonitoringHtml/Default.aspx"), },
			};
			UrlItems = urlItems.AsReadOnly();
			Debug.Assert(UrlItems.Count > 1);
			Debug.Assert(UrlItems.All(n => !string.IsNullOrEmpty(n.TagObject as string)));
			urlNameLookup = UrlItems.Where(n => !string.IsNullOrEmpty(n.TagObject as string)).ToDictionary(n => (string)n.TagObject, n => n.Title);
			SetNewLink(UrlItems[0].TagObject as string, false); //fallback text and url
			string urlFormat;
			if (!IsolatedStorageSerializationHelper.Exists(RecentUrlPath))
				return;
			if (!IsolatedStorageSerializationHelper.Load(RecentUrlPath, out urlFormat))
				return;
			SetNewLink(urlFormat, false); //this will not set the link if invalid
		}

		private static string GetUrlFormatString(string page)
		{
			return "https://jobctrl.com/Account/Login.aspx?ticket={0}&url=/UserCenter/" + page;
		}

		private void UrlClicked(object sender, EventArgs e)
		{
			OpenLink(((TaggableNSMenuItem)sender).TagObject as string, true);
		}

		private void OpenLink(string urlFormat, bool setNewLink)
		{
			if (urlFormat == null)
				return;
			if (setNewLink)
				SetNewLink(urlFormat, true);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var url = "";
				try
				{
					var ticket = AuthenticationHelper.GetAuthTicket();
					url = string.Format(urlFormat, ticket);
					NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(url));
				}
				catch (Exception ex)
				{
					log.Error("Unable to open url: " + url, ex);
				}
			});
		}

		private void SetNewLink(string urlFormat, bool saveLink)
		{
			if (urlFormat == null)
				return;
			string text;
			if (!urlNameLookup.TryGetValue(urlFormat, out text))
			{
				Debug.Fail(urlFormat);
				return;
			}
			this.Title = Labels.Menu_MainPage + " " + text;
			this.TagObject = urlFormat;
			if (!saveLink)
				return;
			IsolatedStorageSerializationHelper.Save(RecentUrlPath, urlFormat);
		}

		private static void OnClick(object sender, EventArgs e)
		{
			var item = (RecentUrlNSMenuItem)sender;
			item.OpenLink(item.TagObject as string, false);
		}
	}
}

