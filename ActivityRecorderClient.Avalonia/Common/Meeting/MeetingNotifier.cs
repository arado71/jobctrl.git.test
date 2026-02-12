using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.Meeting
{
	public class MeetingNotifier
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string KeyMeetingNotifier = "MeetingNotifier";
		private readonly SynchronizationContext context;
		private readonly INotificationService notificationService;
		private readonly MeetingManager meetingManager = new MeetingManager(Platform.Factory.GetMeetingCaptureService());

		public bool SendingBlocked
		{
			get { return meetingManager.SendingBlocked; }
			set { meetingManager.SendingBlocked = value; }
		}

		public MeetingNotifier(SynchronizationContext guiSynchronizationContext, INotificationService notificationService)
		{
			context = guiSynchronizationContext;
			this.notificationService = notificationService;
		}

		public void Start()
		{
			meetingManager.MeetingDataChanged += MeetingDataChanged;
			//meetingManager.LoadData(); //we don't want to load data this time to avoid popup for stale data
			meetingManager.Start();
		}

		public void Stop()
		{
			meetingManager.Stop();
			meetingManager.MeetingDataChanged -= MeetingDataChanged;
		}

		public List<MeetingEntry> UpcomingMeetings => meetingManager.UpcomingMeetings;

		private void MeetingDataChanged(object sender, SingleValueEventArgs<MeetingData> e)
		{
			//invoked on BG Thread (and GUI when loading rules)
			if (e.Value.PendingMeetings == null || e.Value.PendingMeetings.Count == 0) return;
			var sb = new StringBuilder();
			var first = true;
			foreach (var pendingMeeting in e.Value.PendingMeetings.Take(10))
			{
				if (!first) sb.AppendLine();
				first = false;
				sb.Append(pendingMeeting.OrganizerId == ConfigManager.UserId ? "* " : "- ");
				sb.AppendFormat(Labels.NotificationMeetingPendingFormatBody,
					pendingMeeting.Title.Ellipse(25),
					TimeZone.CurrentTimeZone.ToLocalTime(pendingMeeting.StartDate),
					pendingMeeting.OrganizerName.Ellipse(30));
			}
			if (e.Value.PendingMeetings.Count > 10)
			{
				sb.AppendLine();
				sb.Append("...");
			}
			var body = sb.ToString();
			log.Info("MeetingDataChanged " + body);
			context.Post(_ =>
							{
								notificationService.HideNotification(KeyMeetingNotifier);
								notificationService.ShowNotification(KeyMeetingNotifier, TimeSpan.Zero,
									Labels.NotificationMeetingPendingTitle, body, null, GoToApprovalWebsite);
							}, null);
		}

		//todo move url opening to a dedicated service?
		private static readonly string urlFormat = ConfigManager.WebsiteUrlFormatString + "Meetings/MeetingApproval.aspx";

		private static void GoToApprovalWebsite()
		{
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
	}
}
