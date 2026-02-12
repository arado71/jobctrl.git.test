using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Meeting;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.Stats
{
	/// <summary>
	/// Class for coordinating local SimpleWorkTimeStats calculation.
	///  - Makes sure the SimpleWorkTimeStatsBuilder is acessed only on one thread
	///  - Understands and converts IWorkItems to the appropriate format
	///  - Deals with localWorkId assignments (more or less)
	/// </summary>
	public class StatsCoordinator
	{
		public event EventHandler<SingleValueEventArgs<SimpleWorkTimeStats>> SimpleWorkTimeStatsCalculated;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly object thisLock = new object();
		private readonly Dictionary<AssignData, int> localWorkIds = new Dictionary<AssignData, int>(); //todo make it more water-tight (persist, handle unassignable works) or KISS ?
		private readonly SimpleWorkTimeStatsBuilder simpleWorkTimeStatsBuilder = new SimpleWorkTimeStatsBuilder(); //accessed only on the gui thread
		private readonly DailyWorkTimeStatsManager dailyWorkTimeStatsManager = new DailyWorkTimeStatsManager();
		private readonly SynchronizationContext context;

		public StatsCoordinator(SynchronizationContext guiContext)
		{
			context = guiContext;
			simpleWorkTimeStatsBuilder.SimpleWorkTimeStatsCalculated += (_, e) => OnSimpleWorkTimeStatsCalculated(e);
			dailyWorkTimeStatsManager.DailyWorkTimeStatsChanged +=
				(_, e) => context.Post(__ =>
					{
						simpleWorkTimeStatsBuilder.UpdateDailyStats(e.Value.DailyWorkTimes);
						simpleWorkTimeStatsBuilder.SaveData(); //save on the UI Thread is not ideal, but KISS atm.
					}, null);
		}

		public void LoadData()
		{
			simpleWorkTimeStatsBuilder.LoadData();
			dailyWorkTimeStatsManager.LoadData();
		}

		public void Start()
		{
			dailyWorkTimeStatsManager.Start();
		}

		public void Stop()
		{
			dailyWorkTimeStatsManager.Stop();
			simpleWorkTimeStatsBuilder.SaveData();
		}

		public void UpdateStats(IWorkItem item) //called on a bg thread
		{
			var workItem = item as WorkItem;
			if (workItem != null)
			{
				if (!MenuCoordinator.IsWorkIdFromServer(workItem.WorkId))
				{
					lock (thisLock)
					{
						localWorkIds[workItem.AssignData] = workItem.WorkId;
						context.Post(_ => AddWorkItem(workItem), null); //avoid race, post in lock
					}
				}
				else
				{
					context.Post(_ => AddWorkItem(workItem), null);
				}
				return;
			}

			var meetingWorkItem = item as ManualMeetingItem;
			if (meetingWorkItem != null)
			{
				context.Post(_ => AddMeetingWorkItem(meetingWorkItem), null);
				return;
			}

			var manualWorkItem = item as ManualWorkItem;
			if (manualWorkItem != null)
			{
				context.Post(_ => AddManualWorkItem(manualWorkItem), null);
				return;
			}
		}

		public void UpdateMenu(ClientMenuLookup menuLookup) //called on the gui thread
		{
			lock (thisLock)
			{
				var keysToRemove = new List<AssignData>();
				foreach (var kvp in localWorkIds)
				{
					bool ignored;
					var wd = menuLookup.GetWorkForAssignData(kvp.Key, out ignored);
					if (wd != null && MenuCoordinator.IsWorkIdFromServer(wd.WorkData.Id.Value))
					{
						var localId = kvp.Value;
						context.Post(_ => simpleWorkTimeStatsBuilder.ChangeWorkId(localId, wd.WorkData.Id.Value), null);
						keysToRemove.Add(kvp.Key);
					}
				}
				foreach (var keyToRemove in keysToRemove)
				{
					localWorkIds.Remove(keyToRemove);
				}
			}
		}

		private void AddWorkItem(WorkItem workItem)
		{
			simpleWorkTimeStatsBuilder.AddWorkInterval(new WorkInterval()
			{
				WorkType = WorkType.Computer,
				WorkId = workItem.GetWorkId(),
				StartDate = workItem.StartDate,
				EndDate = workItem.EndDate
			});
		}

		private void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			if (manualWorkItem.OriginalEndDate == null) //first time insert
			{
				simpleWorkTimeStatsBuilder.AddWorkInterval(new WorkInterval()
				{
					WorkType = WorkType.Manual,
					WorkId = manualWorkItem.GetWorkId(),
					StartDate = manualWorkItem.StartDate,
					EndDate = manualWorkItem.EndDate
				});
			}
			else // update during process
			{
				if (manualWorkItem.OriginalEndDate.Value < manualWorkItem.EndDate)
				{
					simpleWorkTimeStatsBuilder.AddWorkInterval(new WorkInterval()
					{
						WorkType = WorkType.Manual,
						WorkId = manualWorkItem.GetWorkId(),
						StartDate = manualWorkItem.StartDate,
						EndDate = manualWorkItem.EndDate
					});
				}
				else
				{
					simpleWorkTimeStatsBuilder.DeleteWorkInterval(new IntervalWithType()
					{
						WorkType = WorkType.Manual,
						StartDate = manualWorkItem.EndDate,
						EndDate = manualWorkItem.OriginalEndDate.Value
					});
				}
			}
		}
		private void AddMeetingWorkItem(ManualMeetingItem workItem)
		{
			if (workItem.ManualMeetingData.OnGoing || workItem.StartDate == workItem.EndDate) return; //we don't care about ongoing meetings

			//delete intersection with computer time
			if (workItem.ManualMeetingData.OriginalStartTime.HasValue && workItem.ManualMeetingData.IncludedIdleMinutes > 0)
			{
				simpleWorkTimeStatsBuilder.DeleteWorkInterval(new IntervalWithType()
				{
					WorkType = WorkType.Computer,
					StartDate = workItem.ManualMeetingData.OriginalStartTime.Value,
					EndDate = workItem.ManualMeetingData.OriginalStartTime.Value.AddMinutes(workItem.ManualMeetingData.IncludedIdleMinutes),
				});
			}

			simpleWorkTimeStatsBuilder.AddWorkInterval(new WorkInterval()
			{
				WorkType = WorkType.Meeting,
				WorkId = workItem.GetWorkId(),
				StartDate = workItem.StartDate,
				EndDate = workItem.EndDate
			});
		}

		private void OnSimpleWorkTimeStatsCalculated(SingleValueEventArgs<SimpleWorkTimeStats> currentStats)
		{
			var del = SimpleWorkTimeStatsCalculated;
			if (del != null) del(this, currentStats);
		}
	}
}
