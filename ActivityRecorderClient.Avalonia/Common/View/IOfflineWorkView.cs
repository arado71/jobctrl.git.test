using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View
{
	public interface IOfflineWorkView
	{
		void RunOnGui(Action action);
		void SetAlternativeMenu(Action<WorkDataEventArgs> click, string caption);
		void ShowView();
		void ActivateView();
		void PopupView();
		void AbortAndClose(bool isForce);
		void ShowMessageBox(string body, string title);
		void AddMeetingCard(MeetingInfo info, Guid after);
		void DeleteMeetingCard(Guid id);
		void UpdateTotal(string totalSumText);
		void HandleUserActivity(bool isMouseActivity);
		bool ShowCloseConfirmationDialog();
		bool IsRecordActionAvailable { get; set; }
		void UpdateStopWatch(bool visible);
		int AddInterval(DateTime start, CardStyle type, bool isDraggable = false);
		void ModifyIntervalColor(int index, CardStyle type);
		void ModifyIntervalEnd(int index, bool isDraggable);
		void ModifyIntervalTime(int index, DateTime time);
		int InsertInterval(int index, DateTime start, CardStyle type, bool isDraggable);
		void RemoveInterval(int index);
		bool IsSplitButtonEnabled { get; set; }
		bool? IsMergeButtonEnabled { get; set; }
		bool IsIntervalSplitterEnabled { get; set; }
		void DropdownTaskList(Guid id);
	}

}
