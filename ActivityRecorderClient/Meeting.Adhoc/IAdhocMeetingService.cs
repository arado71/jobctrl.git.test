using System;
using System.Collections.Generic;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.Meeting.Adhoc
{
	public interface IAdhocMeetingService : IMutualWorkTypeService
	{
		bool ManualMeeting { set; get; }
		bool IsGuiShown { get; }
		bool IsStopped { get; }
		void StartWork(int? includedIdleMins, int? workId);
		void PauseWork();
		void ResumeWork();
		void UpdatePostponedMeetingWorkItem();
		void OnClosed(OfflineWindowCloseReason reason);
		void CloseGui();
		void CheckUnfinishedOnGoingMeeting(Action<Action> invoke);
		void CheckPostponedMeetings(Action<Action> invoke);

		Func<WorkData> GetCurrentMeetingWork { get; set; }
		Func<ManualMeetingItem[]> GetConfirmedMeetings { get; set; }
		Func<ManualMeetingItem[]> GetDeletedMeetings { get; set; }
		Func<ManualMeetingItem[]> GetModifiedMeetings { get; set; }
		
		event EventHandler<SingleValueEventArgs<bool>> OnAbortAndClose;
		event EventHandler OnShowGui;
		event EventHandler<SingleValueEventArgs<ClientMenuLookup>> OnUpdateMenu;
		event EventHandler<SingleValueEventArgs<Tuple<Guid, WorkDataWithParentNames>>> OnSetWork;
		event EventHandler<SingleValueEventArgs<KeyValuePair<string, string>>> OnShowMessageBox;
		event EventHandler OnKickWork;
		event EventHandler<SingleValueEventArgs<bool>> OnUserActivityWhileMeeting;
		event EventHandler OnOverCountedAndDeleted;
		void AutoReturnFromMeeting();
	}

	public enum OfflineWindowCloseReason
	{
		// system or application want to stop
		QueryShutdown,
		// user wants to stop work
		RequestStop,
		// user submits recorded offline works
		SubmitWorks,
		// user cancels all recorded offline works
		CancelWorks,
	}
}
