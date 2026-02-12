using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public interface IWorkTimeService
	{
		void ShowModification(DateTime? localDay = null);
		void ShowModifyWork(DeviceWorkInterval workInterval);
		void ShowModifyInterval(Interval localInterval);
		GeneralResult<DeviceWorkIntervalLookup> GetStats(Interval interval);
		GeneralResult<bool> DeleteInterval(Interval interval, string comment, bool force = false);
		GeneralResult<bool> CreateWork(WorkDataWithParentNames work, Interval interval, string comment, bool force = false);
		GeneralResult<bool> DeleteWork(DeviceWorkInterval originalInterval, string comment, bool force = false);
		GeneralResult<bool> ModifyWork(DeviceWorkInterval originalInterval, WorkDataWithParentNames workData, IEnumerable<Interval> newIntervals, string comment, bool force = false);
		GeneralResult<bool> ModifyInterval(Interval interval, WorkDataWithParentNames newWork, string comment, bool force = false);
		GeneralResult<bool> UndeleteWork(DeviceWorkInterval workInterval);
		GeneralResult<IList<Interval>> GetFreeIntervals(Interval interval, Interval localException = null);
		GeneralResult<Interval> GetLocalDayInterval(DateTime localDate);
		GeneralResult<IEnumerable<WorkOrProjectWithParentNames>> GetWorkOrProjectWithParentNames(IEnumerable<int> workIds);
	}
}
