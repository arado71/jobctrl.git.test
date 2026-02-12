using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.Capturing.Meeting;

namespace Tct.ActivityRecorderClient.Meeting
{
	//todo this kind of structure is very common (e.g. CloseReasonsManager, LearningRuleManager etc...) we should move it to a generic base class... ServerDataManager<T> ?
	/// <summary>
	/// Class for querying pending meetings
	/// </summary>
	public class MeetingManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#if DEBUG || DEV
		private const int callbackInterval = 1 * 60 * 1000;
#else
		private const int callbackInterval = 5 * 60 * 1000;  //5 mins /**/2 ManageMeetings 115 bytes/call inside, in variables; but 60 packets 25352 bytes/call outside, in Ethernet packets
#endif
		private const int callbackRetryInterval = 60 * 1000;  //60 secs
		private static string FilePath { get { return "MeetingData-" + ConfigManager.UserId; } }
		public event EventHandler<SingleValueEventArgs<MeetingData>> MeetingDataChanged;

		private volatile bool sendingBlocked;
		public bool SendingBlocked
		{
			get { return sendingBlocked; }
			set { sendingBlocked = value; }
		}

		private MeetingData meetingData = new MeetingData();
		private MeetingData MeetingData
		{
			get
			{
				return meetingData;
			} //we don't need to expose this atm. (and it's not thread-safe)
			set
			{
				if (value == null) //cannot save null value
				{
					MeetingData = new MeetingData();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(meetingData, value))
					return;
				log.Info(String.Format("MeetingDataChanged ({0})", value.ToString()));
				value.UpcomingMeetings = (meetingData?.UpcomingMeetings?.Where(m => m.EndDate >= DateTime.UtcNow.Date && value.UpcomingMeetings.All(u => u.Id != m.Id)) ?? new List<MeetingEntry>()).Concat(value.UpcomingMeetings ?? new List<MeetingEntry>()).OrderBy(m => m.StartDate).ToList();
				meetingData = value;
				IsolatedStorageSerializationHelper.Save(FilePath, value);
				OnMeetingDataChanged(value);
			}
		}

		public List<MeetingEntry> UpcomingMeetings => meetingData?.UpcomingMeetings;

		private bool lastSendFailed;

		private readonly IMeetingCaptureService meetingCaptureService;

		public MeetingManager(IMeetingCaptureService meetingCaptureService)
			: base(log)
		{
			if (meetingCaptureService == null) throw new ArgumentNullException();
			this.meetingCaptureService = meetingCaptureService;
			SendingBlocked = true; // sending blocked while credit runout state isn't determined
			LoadData();
		}

		protected override int ManagerCallbackInterval
		{
			get { return lastSendFailed ? callbackRetryInterval : callbackInterval; }
		}

		//TODO: clean/refactor this method!!!
		protected override void ManagerCallbackImpl()
		{
			if (!ConfigManager.IsMeetingTrackingEnabled || SendingBlocked) return;

			try
			{
				int userId = ConfigManager.UserId;
				int computerId = ConfigManager.EnvironmentInfo.ComputerId;
				lastSendFailed = false;

				var data = ActivityRecorderClientWrapper.Execute(n => n.ManageMeetings(userId, computerId, null));

				if (data.LastSuccessfulSyncDate.HasValue)	//LastSuccessfulSync date was null but now we get a value. Don't wait for next run to capture finished meetings.
				{
					var finishedMeetingData = CaptureMeetings(data.CalendarEmailAccounts, data.LastSuccessfulSyncDate.Value);
					if (finishedMeetingData != null)
					{
						log.Info("Sending finished meetings: " + finishedMeetingData.ToString());

						data = ActivityRecorderClientWrapper.Execute(n => n.ManageMeetings(userId, computerId, finishedMeetingData));	//If there is no finished meeting than it is not necessary to call ManageMeetings, becuse we have called it just now.
						if (data.LastSuccessfulSyncDate.ToString() != finishedMeetingData.LastQueryIntervalEndDate.ToString())	//1. should use return code to check success 2. using ToStirng to eliminate small differences (maybe sql cause these diffs on website?)
						{
							log.Info("Server refused to store finished meetings.");
						}
					}
				}

				if (!data.LastSuccessfulSyncDate.HasValue) log.Warn("Meeting tracking is enabled but we can't get LastSuccessfulSyncDate.");

				MeetingData = data;
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("(*supressed*)")) return;
				WcfExceptionLogger.LogWcfError("get Meeting Data OR send finished/manual meetings", log, ex);
				lastSendFailed = true; //retry shortly
			}
		}

		public override void Start(int first = 0)
		{
			meetingCaptureService.Initialize();
			base.Start(first);
		}

		public override void Stop()
		{
			base.Stop();	//This will wait for actual run of ManagerCallbackImpl to finsh. (This could be even 30-60 sec!!! (Because of Smtp address retrevieving for account in Redemption.))
			meetingCaptureService.Dispose();
		}


		private FinishedMeetingData CaptureMeetings(List<string> calendarEmailAccounts, DateTime startDate)
		{
			DateTime serverNow = DateTime.UtcNow;	//TODO: Get valid server time for now.
			//TODO: Check Outlook<->Exchange last sync time and meeting's creation time!!!

			List<FinishedMeetingEntry> finishedMeetings = meetingCaptureService.CaptureMeetings(calendarEmailAccounts, startDate.ToLocalTime(), serverNow.ToLocalTime());

			FinishedMeetingData finishedMeetingData = finishedMeetings == null || finishedMeetings.Count == 0 ? null : new FinishedMeetingData()
			{
				FinishedMeetings = finishedMeetings,
				QueryIntervalStartDate = startDate,
				QueryIntervalEndDate = serverNow,
				CalendarEmailAccounts = calendarEmailAccounts,
			};

			return finishedMeetingData;
		}

		public void LoadData()
		{
			MeetingData value;
			if (IsolatedStorageSerializationHelper.Exists(FilePath)
			    && IsolatedStorageSerializationHelper.Load(FilePath, out value))
			{
				meetingData = value;
				log.InfoFormat("MeetingData has been loaded from disk ({0})", value);
			}
			else
			{
				log.Info("There is no previous MeetingData on disk.");
			}
			OnMeetingDataChanged(meetingData); //always raise so we know the initial state
		}

		private void OnMeetingDataChanged(MeetingData value)
		{
			Debug.Assert(value != null);
			var del = MeetingDataChanged;
			if (del == null) return;
			del(this, SingleValueEventArgs.Create(value));
		}
	}
}
