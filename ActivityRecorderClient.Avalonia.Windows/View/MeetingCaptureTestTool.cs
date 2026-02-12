using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Meeting;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using System.Threading;

namespace Tct.ActivityRecorderClient.View
{
	public partial class MeetingCaptureTestTool : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;
		private readonly IMeetingCaptureService meetingCaptureService;
		private readonly bool needToInitializeAndDispose;

		public MeetingCaptureTestTool() : this(new MeetingCaptureWinService())
		{
			needToInitializeAndDispose = true;
		}

		public MeetingCaptureTestTool(IMeetingCaptureService meetingCaptureService)
		{
			this.meetingCaptureService = meetingCaptureService;
			InitializeComponent();
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			dtpStartDate.Value = DateTime.Now.AddMonths(-1);
			dtpEndDate.Value = DateTime.Now;
		}

		private void MeetingCaptureClientForm_Load(object sender, EventArgs e)
		{
			log.Info("Meeting Tool loading...");

			lblVersionInfo.Text = "Getting version info...";
			btnCaptureMeetings.Enabled = false;

			ThreadPool.QueueUserWorkItem(_ =>
				{
					try
					{
						if (needToInitializeAndDispose) meetingCaptureService.Initialize();
						string versionInfo = meetingCaptureService.GetVersionInfo();
						context.Post(__ =>
						{
							if (IsDisposed) return;
							lblVersionInfo.Text = versionInfo;
							btnCaptureMeetings.Enabled = !String.IsNullOrEmpty(versionInfo);
						}, null);
					}
					catch (Exception ex)
					{
						context.Post(__ =>
						{
							MessageBox.Show(ex.ToString());
							Close();
						}, null);
					}
				}, null);
		}

		private void MeetingCaptureClientForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			log.Info("Meeting Tool closing...");

			if (!needToInitializeAndDispose) return;

			lblVersionInfo.Text = "Uninitialize capture service...";
			btnCaptureMeetings.Enabled = false;

			ThreadPool.QueueUserWorkItem(_ => meetingCaptureService.Dispose(), null);
		}

		private void btnCaptureMeetings_Click(object sender, EventArgs e)
		{
			string calendarEmailAccountsStr = txbCalendarEmailAccounts.Text;
			List<string> calendarEmailAccouns = calendarEmailAccountsStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(email => email.Trim()).ToList();
			DateTime startDate = dtpStartDate.Value;
			DateTime endDate = dtpEndDate.Value;

			txbFinishedMeetings.Text = "Capturing meetings...";
			btnCaptureMeetings.Enabled = false;

			ThreadPool.QueueUserWorkItem(_ =>
				{
					FinishedMeetingData finishedMeetingData = null;
					for (var i = 0; i < 1; i++)
					{
						finishedMeetingData = CaptureMeetings(calendarEmailAccouns, startDate, endDate);
					}
					context.Post((__) =>
						{
							if (IsDisposed) return;
							txbFinishedMeetings.Text = finishedMeetingData == null
														   ? "There are no meetings finished or created in the given time interval for the given emails."
														   : "Finished meetings: " + finishedMeetingData.ToString();
							btnCaptureMeetings.Enabled = true;
						}, null);
				}, null);
		}

		private FinishedMeetingData CaptureMeetings(List<string> calendarEmailAccounts, DateTime queryStartDate, DateTime queryEndDate)
		{
			log.InfoFormat("Capturing meetings from Meeting Tool... ({0}, {1}, {2})", String.Join(", ", calendarEmailAccounts.ToArray()), queryStartDate, queryStartDate);

			List<FinishedMeetingEntry> finishedMeetings = meetingCaptureService.CaptureMeetings(calendarEmailAccounts, queryStartDate, queryEndDate);

			FinishedMeetingData finishedMeetingData = finishedMeetings == null || finishedMeetings.Count == 0 ? null : new FinishedMeetingData()
			{
				FinishedMeetings = finishedMeetings,
				QueryIntervalStartDate = queryStartDate,
				QueryIntervalEndDate = queryEndDate,
				CalendarEmailAccounts = calendarEmailAccounts,
			};

			log.Info(finishedMeetingData == null
				? "There are no meetings finished or created in the given time interval for the given emails."
				: "Finished meetings: " + finishedMeetingData);

			return finishedMeetingData;
		}
	}
}
