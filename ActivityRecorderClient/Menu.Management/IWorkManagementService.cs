using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu.Management
{
	public interface IWorkManagementService : IDisposable
	{
		event EventHandler MenuRefreshNeeded;
		event EventHandler<SingleValueEventArgs<TaskReasons>> OnTaskReasonsChanged;

		void DisplayCloseWorkGui(WorkData workToClose);
		void DisplayReasonWorkGui(WorkData workToComment);
		void DisplayWorkDetailsGui(WorkData workToShow);
		void DisplayCreateWorkGui();
		void DisplayUpdateWorkGui(WorkData workToUpdate);
		bool DisplayWarnNotificationIfApplicable();
		void SetSimpleWorkTimeStats(SimpleWorkTimeStats stats);
		void SetCannedCloseReasons(CannedCloseReasons reasons);
		TimeSpan? GetTotalWorkTimeForWork(int workId);
		void UpdateMenu(ClientMenuLookup menuLookup);
		void Start();
		void Stop();
	}
}
