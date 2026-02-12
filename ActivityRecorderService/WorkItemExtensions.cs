using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService
{
	public static class WorkItemExtensions
	{

		public static void DeleteIntervals(this List<WorkItem> workItems, IEnumerable<ManualWorkItem> manualWorkItems)
		{
			foreach (var deleteWorkItem in manualWorkItems
				.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteComputerInterval
							|| n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteInterval))
			{
				var start = deleteWorkItem.StartDate;
				var end = deleteWorkItem.EndDate;
				workItems.RemoveOrAdd(n => RemoveOrAddWorkItem(n, start, end));
			}
		}

		public static void DeleteIntervals(this List<MobileWorkItem> workItems, IEnumerable<ManualWorkItem> manualWorkItems)
		{
			foreach (var deleteWorkItem in manualWorkItems
				.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteMobileInterval
							|| n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteInterval))
			{
				var start = deleteWorkItem.StartDate;
				var end = deleteWorkItem.EndDate;
				workItems.RemoveOrAdd(n => RemoveOrAddMobileWorkItem(n, start, end));
			}
		}

		public static void DeleteIntervals(this List<AggregateWorkItemInterval> workItems, IEnumerable<ManualWorkItem> manualWorkItems)
		{
			foreach (var deleteWorkItem in manualWorkItems
				.Where(n => n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteComputerInterval
							|| n.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteInterval))
			{
				var start = deleteWorkItem.StartDate;
				var end = deleteWorkItem.EndDate;
				workItems.RemoveOrAdd(n => RemoveOrAddAggregateWorkItemInterval(n, start, end));
			}
		}

		private static RemoveOrAddData<WorkItem> RemoveOrAddWorkItem(WorkItem workItem, DateTime startDate, DateTime endDate)
		{
			if (!(startDate < workItem.EndDate && workItem.StartDate < endDate)) return null;
			WorkItem workItemToInsert = null;
			var origDuration = workItem.EndDate - workItem.StartDate;
			var origMouseActivity = workItem.MouseActivity;
			var origKeyboardActivity = workItem.KeyboardActivity;
			var modStart = workItem.StartDate < startDate;
			var modEnd = workItem.EndDate > endDate;

			if (modEnd)
			{
				if (modStart) //we have to insert a new workitem
				{
					workItemToInsert = new WorkItem()
					{
						//Id = 0,
						StartDate = endDate,
						EndDate = workItem.EndDate,
						CompanyId = workItem.CompanyId,
						ComputerId = workItem.ComputerId,
						GroupId = workItem.GroupId,
						IPAddress = workItem.IPAddress,
						IsRemoteDesktop = workItem.IsRemoteDesktop,
						IsVirtualMachine = workItem.IsVirtualMachine,
						PhaseId = workItem.PhaseId,
						ReceiveDate = workItem.ReceiveDate,
						UserId = workItem.UserId,
						WorkId = workItem.WorkId,
					};
					UpdateWorkItem(workItemToInsert, origDuration, origKeyboardActivity, origMouseActivity, null);
				}
				else //we can update current workitem
				{
					var origStart = workItem.StartDate;
					workItem.StartDate = endDate;
					UpdateWorkItem(workItem, origDuration, origKeyboardActivity, origMouseActivity, new StartEndDateTime(origStart, workItem.StartDate));
				}
			}

			if (modStart) //we can update current workitem
			{
				var origEnd = workItem.EndDate;
				workItem.EndDate = startDate;
				UpdateWorkItem(workItem, origDuration, origKeyboardActivity, origMouseActivity, new StartEndDateTime(workItem.EndDate, origEnd));
			}

			return new RemoveOrAddData<WorkItem>(workItemToInsert, !modStart & !modEnd);
		}

		public static void UpdateWorkItem(WorkItem workItem, TimeSpan origDuration, int origKeyboardActivity, int origMouseActivity, StartEndDateTime? removedInterval)
		{
			//probably we should store capture at the right workitem, but KISS atm. (so all captures will be stored in the first workitem)
			var newDuration = workItem.EndDate - workItem.StartDate;
			Debug.Assert(newDuration.Ticks > 0);
			Debug.Assert(origDuration.Ticks > 0);
			var mul = newDuration.Ticks / (double)origDuration.Ticks;
			workItem.KeyboardActivity = (int)Math.Ceiling(origKeyboardActivity * mul);
			workItem.MouseActivity = (int)Math.Ceiling(origMouseActivity * mul);
			if (workItem.DesktopCaptures == null || workItem.DesktopCaptures.Count == 0 || !removedInterval.HasValue) return;
			for (int i = 0; i < workItem.DesktopCaptures.Count; i++)
			{
				var currCap = workItem.DesktopCaptures[i];
				if (
					(currCap.Screens != null && currCap.Screens.Any(n => removedInterval.Value.StartDate <= n.CreateDate && n.CreateDate <= removedInterval.Value.EndDate))
					|| (currCap.DesktopWindows != null && currCap.DesktopWindows.Any(n => removedInterval.Value.StartDate <= n.CreateDate && n.CreateDate <= removedInterval.Value.EndDate))
					)
				{
					workItem.DesktopCaptures.RemoveAt(i--); //remove interval is inclusive (like in the GetActiveWindowsGroupped sproc, but only the CreateDate of IsActive matters there)
				}
			}
		}

		private static RemoveOrAddData<MobileWorkItem> RemoveOrAddMobileWorkItem(MobileWorkItem workItem, DateTime startDate, DateTime endDate)
		{
			if (!(startDate < workItem.EndDate && workItem.StartDate < endDate)) return null;
			MobileWorkItem workItemToInsert = null;
			var modStart = workItem.StartDate < startDate;
			var modEnd = workItem.EndDate > endDate;

			if (modEnd)
			{
				if (modStart) //we have to insert a new workitem
				{
					workItemToInsert = new MobileWorkItem()
					{
						//Id = 0,
						StartDate = endDate,
						EndDate = workItem.EndDate,
						Imei = workItem.Imei,
						UserId = workItem.UserId,
						WorkId = workItem.WorkId,
					};
				}
				else //we can update current workitem
				{
					workItem.StartDate = endDate;
				}
			}

			if (modStart) //we can update current workitem
			{
				workItem.EndDate = startDate;
			}

			return new RemoveOrAddData<MobileWorkItem>(workItemToInsert, !modStart & !modEnd);
		}

		private static RemoveOrAddData<AggregateWorkItemInterval> RemoveOrAddAggregateWorkItemInterval(AggregateWorkItemInterval workItem, DateTime startDate, DateTime endDate)
		{
			if (!(startDate < workItem.EndDate && workItem.StartDate < endDate)) return null;
			AggregateWorkItemInterval workItemToInsert = null;
			var modStart = workItem.StartDate < startDate;
			var modEnd = workItem.EndDate > endDate;

			if (modEnd)
			{
				if (modStart) //we have to insert a new workitem
				{
					workItemToInsert = new AggregateWorkItemInterval()
					{
						//Id = 0,
						StartDate = endDate,
						EndDate = workItem.EndDate,
						CompanyId = workItem.CompanyId,
						ComputerId = workItem.ComputerId,
						CreateDate = workItem.CreateDate,
						GroupId = workItem.GroupId,
						PhaseId = workItem.PhaseId,
						UpdateDate = workItem.UpdateDate,
						UserId = workItem.UserId,
						WorkId = workItem.WorkId,
					};
				}
				else //we can update current workitem
				{
					workItem.StartDate = endDate;
				}
			}

			if (modStart) //we can update current workitem
			{
				workItem.EndDate = startDate;
			}

			return new RemoveOrAddData<AggregateWorkItemInterval>(workItemToInsert, !modStart & !modEnd);
		}

		private class RemoveOrAddData<T> where T : class
		{
			public readonly T NewItem;
			public readonly bool RemoveOriginal;

			public RemoveOrAddData(T newItem, bool removeOriginal)
			{
				NewItem = newItem;
				RemoveOriginal = removeOriginal;
			}
		}

		private static void RemoveOrAdd<T>(this List<T> list, Func<T, RemoveOrAddData<T>> removeOrAddFunc) where T : class
		{
			for (int i = 0; i < list.Count; i++)
			{
				var data = removeOrAddFunc(list[i]);
				if (data == null) continue; //do nothing
				if (data.RemoveOriginal)
				{
					list.RemoveAt(i--);
				}
				if (data.NewItem != default(T))
				{
					list.Insert(++i, data.NewItem);
				}
			}
		}
	}
}
