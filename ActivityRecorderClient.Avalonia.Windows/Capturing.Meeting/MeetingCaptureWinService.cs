using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Capturing.Meeting
{
	public class MeetingCaptureWinService : IMeetingCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
		private readonly List<IMeetingCaptureService> meetingCaptureServices = new List<IMeetingCaptureService>();

	    private readonly Dictionary<Type, Func<IMeetingCaptureService>> meetingCaptureServiceFactories = new Dictionary<Type, Func<IMeetingCaptureService>>
	    {
			// TODO: mac
	        //{typeof (OutlookMeetingCaptureService), () => new OutlookMeetingCaptureService()},
	        //{typeof (LotusNotesMeetingCaptureService), () => new LotusNotesMeetingCaptureService(LotusNotesLoginForm.DisplayLoginForm)},
	    };

		private bool? isOutlookRunning;

		public void Initialize()
		{
		}

		public void Dispose()
		{
			foreach (var meetingCaptureService in meetingCaptureServices)
			{
				meetingCaptureService.Dispose();
			}
		}

		public List<FinishedMeetingEntry> CaptureMeetings(IList<string> calendarAccountEmails, DateTime startDate, DateTime endDate)
		{
			EnsureInitialized();
			return meetingCaptureServices.SelectMany(s => s.CaptureMeetings(calendarAccountEmails, startDate, endDate)).ToList();
		}

		public string GetVersionInfo()
		{
			EnsureInitialized();
			return String.Join(Environment.NewLine, meetingCaptureServices.Select(s => s.GetVersionInfo()).ToArray());
		}

		public string[] ProcessNames => meetingCaptureServices.SelectMany(m => m.ProcessNames).ToArray();

		private void EnsureInitialized()
		{
			//EnsureInitialized(ConfigManager.IsOutlookMeetingTrackingEnabled, typeof(OutlookMeetingCaptureService));
			//EnsureInitialized(ConfigManager.IsLotusNotesMeetingTrackingEnabled, typeof(LotusNotesMeetingCaptureService));
		}

		private void EnsureInitialized(bool shouldBeInitialized, Type meetingCaptureServiceType)
		{
			if (shouldBeInitialized) InitializeIfApplicable(meetingCaptureServiceType);
			else UninitializeIfApplicable(meetingCaptureServiceType);
		}

		private void InitializeIfApplicable(Type meetingCaptureServiceType)
		{
			var found = meetingCaptureServices.SingleOrDefault(n => n.GetType() == meetingCaptureServiceType);
			if (found != null)
			{
				return;
			}

		    Func<IMeetingCaptureService> factory;
            if (!meetingCaptureServiceFactories.TryGetValue(meetingCaptureServiceType, out factory)) return;

			log.Info("Creating meeting capture service for a given type: " + meetingCaptureServiceType.Name);

		    var service = factory();
			service.Initialize();
			meetingCaptureServices.Add(service);
		}

		private void UninitializeIfApplicable(Type meetingCaptureServiceType)
		{
			var captureService = meetingCaptureServices.FirstOrDefault(n => n.GetType() == meetingCaptureServiceType);
			if (captureService == null) return;

			log.Info("Disposing meeting capture service for a given type: " + meetingCaptureServiceType.Name);

			captureService.Dispose();
			meetingCaptureServices.Remove(captureService);
		}
	}
}
