using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;
using Reporter.Model;
using Reporter.Model.WorkItems;

namespace Reporter
{
	internal static class WorkItemHelper
	{
		private static IEnumerable<WorkItemDeletion> Flatten(IEnumerable<IWorkItemDeletion> intervals)
		{
			Contract.Requires(intervals.IsOrderedBy(x => x.StartDate));
			Contract.Ensures(Contract.Result<IEnumerable<WorkItemDeletion>>().IsSorted(x => x.StartDate));

			DateTime? lastEnd = null;
			foreach (var deletion in intervals)
			{
				if (lastEnd != null && deletion.StartDate <= lastEnd.Value)
				{
					if (deletion.EndDate > lastEnd.Value)
					{
						yield return new WorkItemDeletion() { UserId = deletion.UserId, StartDate = lastEnd.Value, EndDate = deletion.EndDate, Type = deletion.Type };
					}
				}
				else
				{
					yield return new WorkItemDeletion() { UserId = deletion.UserId, StartDate = deletion.StartDate, EndDate = deletion.EndDate, Type = deletion.Type };
				}

				lastEnd = deletion.EndDate;
			}
		}

		private static IEnumerable<WorkItem> Flatten(IEnumerable<IWorkItem> intervals)
		{
			Contract.Requires(intervals.IsOrderedBy(x => x.StartDate));
			Contract.Ensures(Contract.Result<IEnumerable<WorkItem>>().IsSorted(x => x.StartDate));

			using (Profiler.Measure())
			{
				DateTime? lastEnd = null;
				foreach (var workitem in intervals)
				{
					if (lastEnd != null && workitem.StartDate <= lastEnd.Value)
					{
						if (workitem.EndDate > lastEnd.Value)
						{
							var res = GetInternalWorkItem(workitem);
							res.StartDate = lastEnd.Value;
							yield return res;
						}
					}
					else
					{
						yield return GetInternalWorkItem(workitem);
					}

                    if (lastEnd == null || lastEnd < workitem.EndDate)
					    lastEnd = workitem.EndDate;
				}
			}
		}

		private static IEnumerable<WorkItem> RemoveDeletedWorkItems(IEnumerable<WorkItem> workitems, IEnumerable<IInterval> deletions)
		{
			Contract.Requires(workitems.IsSorted(x => x.StartDate));
			Contract.Requires(deletions.IsSorted(x => x.StartDate));
			Contract.Ensures(Contract.Result<IEnumerable<WorkItem>>().IsSorted(x => x.StartDate));

			var workitemIterator = workitems.GetEnumerator();
			var deletionIterator = deletions.GetEnumerator();
			var workitemHasElements = workitemIterator.MoveNext();
			var deleteHasElements = deletionIterator.MoveNext();
			WorkItem remainingWorkItem = null;
			while (workitemHasElements)
			{
				WorkItem currentWorkItem;
				if (remainingWorkItem == null)
				{
					currentWorkItem = workitemIterator.Current;
				}
				else
				{
					currentWorkItem = remainingWorkItem;
					remainingWorkItem = null;
				}

				if (!deleteHasElements)
				{
					yield return currentWorkItem;
					workitemHasElements = workitemIterator.MoveNext();
					continue;
				}

				var currentDeletion = deletionIterator.Current;
				if (currentWorkItem.StartDate < currentDeletion.EndDate && currentWorkItem.EndDate > currentDeletion.StartDate) // Overlapping intervals?
				{
					if (currentWorkItem.StartDate < currentDeletion.StartDate && currentWorkItem.EndDate > currentDeletion.EndDate)     // cuts into 2 pieces (ending part remains)
					{
						var res = currentWorkItem.Clone();
						res.Resize(currentWorkItem.StartDate, currentDeletion.StartDate);
						yield return res;
						remainingWorkItem = currentWorkItem.Clone();
						remainingWorkItem.Resize(currentDeletion.EndDate, currentWorkItem.EndDate);
					}

					if (currentWorkItem.StartDate < currentDeletion.StartDate && currentWorkItem.EndDate <= currentDeletion.EndDate)    // cuts the ending part
					{
						var res = currentWorkItem.Clone();
						res.Resize(currentWorkItem.StartDate, currentDeletion.StartDate);
						yield return res;
					}

					if (currentWorkItem.StartDate >= currentDeletion.StartDate && currentWorkItem.EndDate > currentDeletion.EndDate) // Delete start (ending part remains)
					{
						remainingWorkItem = currentWorkItem.Clone();
						remainingWorkItem.Resize(currentDeletion.EndDate, currentWorkItem.EndDate);
					}
				}
				else
				{
					if (currentDeletion.StartDate >= currentWorkItem.EndDate)   // deletion starts after the interval
                        yield return currentWorkItem;
					else                                                        // deletion ends before the interval (keep remaining part (bug fix))
					    remainingWorkItem = currentWorkItem;
                
				}

				if (remainingWorkItem == null && currentWorkItem.EndDate <= currentDeletion.EndDate) workitemHasElements = workitemIterator.MoveNext();
				if (currentWorkItem.EndDate >= currentDeletion.EndDate) deleteHasElements = deletionIterator.MoveNext();
			}
		}

		internal static Dictionary<Device, WorkItem[]> GetEffectiveWorkItems(IWorkItem[] workItems,
			IWorkItemDeletion[] deletions)
		{
			Contract.Requires<NullReferenceException>(workItems != null);
			Contract.Requires<NullReferenceException>(deletions != null);
			Contract.Requires(deletions.IsOrderedBy(x => x.StartDate));
			Contract.Requires(workItems.GroupBy(x => Device.FromWorkItem(x)).All(x => x.IsSorted(y => y.StartDate)), "Not all WorkItems are ordered by StartDate after grouping by Device");

			using (Profiler.Measure())
			{
				var result = new Dictionary<Device, WorkItem[]>();
				var deviceBuckets = workItems.GroupBy(x => Device.FromWorkItem(x));
				foreach (var deviceWorkItems in deviceBuckets)
				{
					var processedDeviceWorkItems = deviceWorkItems.Key.IsFlatteningRequired ? Flatten(deviceWorkItems).ToList() : deviceWorkItems.Select(GetInternalWorkItem).ToList();
					WorkItem[] effectiveWorkItems;
					if (deviceWorkItems.Key.IsDeletionSupported && deviceWorkItems.Key.IsFlatteningRequired) // Deletion calculation requires flattening
					{

						var deviceDeletions = deletions.Where(x => x.UserId == deviceWorkItems.Key.UserId).ToArray();
						var isDeleteApplicableFunc = GetCanDeleteFunc(deviceWorkItems.Key.Type);
						var flatDeviceDeletions = Flatten(deviceDeletions.Where(x => isDeleteApplicableFunc(x.Type))).ToList();
						effectiveWorkItems = RemoveDeletedWorkItems(processedDeviceWorkItems, flatDeviceDeletions).ToArray();
					}
					else
					{
						effectiveWorkItems = processedDeviceWorkItems.ToArray();
					}
					
					result.Add(deviceWorkItems.Key, effectiveWorkItems);
				}

				return result;
			}
		}

		private static WorkItem GetInternalWorkItem(IWorkItem obj)
		{
			Contract.Requires<NullReferenceException>(obj != null);

			if (obj is IComputerWorkItem)
			{
				return new ComputerWorkItem((IComputerWorkItem)obj);
			}

			if (obj is IAdhocMeetingWorkItem)
			{
				return new AdhocMeetingWorkItem((IAdhocMeetingWorkItem)obj);
			}

			if (obj is ICalendarMeetingWorkItem)
			{
				return new CalendarMeetingWorkItem((ICalendarMeetingWorkItem)obj);
			}

			if (obj is IManualWorkItem)
			{
				return new ManualWorkItem((IManualWorkItem)obj);
			}

			if (obj is IMobileWorkItem)
			{
				return new MobileWorkItem((IMobileWorkItem) obj);
			}

			if (obj is ISickLeaveWorkItem)
			{
				return new SickLeaveWorkItem((ISickLeaveWorkItem)obj);
			}

			if (obj is IHolidayWorkItem)
			{
				return new HolidayWorkItem((IHolidayWorkItem)obj);
			}

			Debug.Fail("Unknown type");
			return null;
		}

		private static Func<DeletionTypes, bool> GetCanDeleteFunc(ItemType itemType)
		{
			switch (itemType)
			{
				case ItemType.Pc:
					return x => x == DeletionTypes.Computer || x == DeletionTypes.All;
				case ItemType.Mobile:
					return x => x == DeletionTypes.Mobile || x == DeletionTypes.All;
				default:
					return x => false;
			}
		}

		internal static ItemType GetWorkType(IWorkItem workItem)
		{
			Contract.Requires<NullReferenceException>(workItem != null);

			if (workItem is IComputerWorkItem)
			{
				return ItemType.Pc;
			}

			if (workItem is IMobileWorkItem)
			{
				return ItemType.Mobile;
			}

			if (workItem is IManualWorkItem)
			{
				return ItemType.Manual;
			}

			if (workItem is IAdhocMeetingWorkItem)
			{
				return ItemType.AdhocMeeting;
			}

			return ItemType.Unknown;
		}

		internal static string GetDeviceId(IWorkItem workItem)
		{
			Contract.Requires<ArgumentNullException>(workItem != null);

			if (workItem is IComputerWorkItem)
			{
				return ((IComputerWorkItem)workItem).ComputerId.ToString(CultureInfo.InvariantCulture);
			}

			if (workItem is IMobileWorkItem)
			{
				return ((IMobileWorkItem)workItem).Imei.ToString(CultureInfo.InvariantCulture);
			}

			return "";
		}
	}
}
