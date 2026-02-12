using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Menu
{
	public class RecentUrlQuery
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static RecentUrlQuery instance;

		private string currentUrl = null;
		public event EventHandler RecentChanged;

		private static string RecentUrlPath
		{
			get { return "RecentUrl-" + ConfigManager.UserId; }
		}

		public static RecentUrlQuery Instance
		{
			get { return instance ?? (instance = new RecentUrlQuery()); }
		}

		public Dictionary<string, string> Lookup;

		private RecentUrlQuery()
		{
			Localize();
			currentUrl = Lookup.First().Value;
			if (IsolatedStorageSerializationHelper.Exists(RecentUrlPath))
			{
				IsolatedStorageSerializationHelper.Load(RecentUrlPath, out currentUrl);
				if (Lookup.All(x => x.Value != currentUrl))
				{
					currentUrl = Lookup.First().Value;
				}
			}
		}

		public void Localize()
		{
			Lookup = new Dictionary<string, string>(){
				{ Labels.Menu_AcMain, GetUrlFormatString("MyAccount.aspx") },
				{ Labels.Menu_AcHolSick, GetUrlFormatString("Holidays.aspx") },
				{ Labels.Menu_AcManual, GetUrlFormatString("ModifyReports.aspx") },
				{ Labels.Menu_AcWorks, GetUrlFormatString("MyTasks.aspx") },
				{ Labels.Menu_AcOnlineMon, GetUrlFormatString("RealTimeActivity/Default.aspx") },
				{ Labels.Menu_AcMeetings, GetUrlFormatString("Meetings/MeetingApproval.aspx") },
				{ Labels.Menu_AcReports, GetUrlFormatString("Reports/CentralReports.aspx") },
				{ Labels.Menu_AcDashboard, GetUrlFormatString("Dashboard/") },
				{ Labels.TODOs, GetUrlFormatString("TodoLists/") }
			};
		}

		public string GetCurrentName()
		{
			return Lookup.FirstOrDefault(x => x.Value == currentUrl).Key;
		}

		public IEnumerable<string> GetNames()
		{
			return Lookup.Keys;
		}

		public void OpenLink(string targetUrl = null)
		{
			if (currentUrl == null) return;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				string url = "";
				try
				{
					string ticket = AuthenticationHelper.GetAuthTicket();
					url = string.Format(string.IsNullOrEmpty(targetUrl) ? currentUrl : targetUrl, ticket);
					if (OperatingSystem.IsWindows())
					{
						var sInfo = new ProcessStartInfo(url);
						Process.Start(sInfo);
					}
					else
					{
						Process.Start("open", url);
					}
				}
				catch (Exception ex)
				{
					log.Error("Unable to open url: " + url, ex);
				}
			});
		}

		public void SetLink(string urlName)
		{
			if (string.IsNullOrEmpty(urlName)) return;
			string text;
			if (!Lookup.TryGetValue(urlName, out text))
			{
				Debug.Fail(urlName);
				return;
			}

			currentUrl = text;
			EventHandler evt = RecentChanged;
			if (evt != null) evt(this, EventArgs.Empty);
			IsolatedStorageSerializationHelper.Save(RecentUrlPath, text);
		}

		private static string GetUrlFormatString(string page)
		{
			return ConfigManager.WebsiteUrlFormatString + page;
		}

		public void OpenUrl(string url)
		{
			url = ConfigManager.WebsiteUrl + "Account/Login.aspx?ticket={0}&url=" + Uri.EscapeDataString(url);
			OpenLink(url);
		}
	}
}