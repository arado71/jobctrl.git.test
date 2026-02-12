using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;
using Reporter.Model;
using Reporter.Model.ProcessedItems;
using Reporter.Model.WorkItems;
using WorkItem = Reporter.Model.ProcessedItems.WorkItem;

namespace Reporter.Processing
{
	public static class ReportHelper
	{
		public static IEnumerable<WorkItem> Transform(IEnumerable<ICollectedItem> collectedItems, IEnumerable<IWorkItemDeletion> deletes,
			IEnumerable<IWorkItem> workItems)
		{
			Contract.Requires<NullReferenceException>(collectedItems != null, "Parameter collectedItems can't be null");
			Contract.Requires<NullReferenceException>(deletes != null, "Parameter deletes can't be null");
			Contract.Requires<NullReferenceException>(workItems != null, "Parameter workItems can't be null");

			using (Profiler.Measure())
			{
				return Transform(NetQueryResult.CreateFrom(collectedItems, deletes, workItems));
			}
		}

		public static IEnumerable<WorkItem> Transform(NetQueryResult netResult)
		{
			Contract.Requires<ArgumentNullException>(netResult != null, "Parameter netResult can't be null");
			Contract.Requires(netResult.Devices.IsSorted(x => x.UserId));
			Contract.Ensures(Contract.Result<IEnumerable<WorkItem>>().IsSorted(x => x.UserId));
			Contract.Ensures(Contract.Result<IEnumerable<WorkItem>>().GroupBy(Device.FromProcessedItem).All(x => x.IsSorted(y => y.StartDate)));

			using (Profiler.Measure())
			{
				foreach (var device in netResult.Devices)
				{
					IEnumerable<Model.WorkItems.WorkItem> deviceWorkItems = netResult.EffectiveWorkItems[device];
					if (device.HasCollectedItems && device.IsFlatteningRequired) // Merging requires flattened workitems
					{
						var collectedItemIntervals = CollectedItemHelper.GetCollectedItemIntervals(netResult.CollectedItems, device);
						foreach (var processedItem in MergeSorted(collectedItemIntervals, deviceWorkItems))
						{
							yield return processedItem;
						}
					}
					else
					{
						foreach (var processedItem in Convert(deviceWorkItems))
						{
							yield return processedItem;
						}
					}
				}
			}
		}

		private static IEnumerable<WorkItem> Convert(IEnumerable<Model.WorkItems.WorkItem> workitems)
		{
			Contract.Requires(workitems.IsSorted(x => x.StartDate));
			Contract.Ensures(Contract.Result<IEnumerable<WorkItem>>().IsSorted(x => x.StartDate));

			using (Profiler.Measure())
			{
				var workItemIterator = workitems.GetEnumerator();
				var workItemHasElements = workItemIterator.MoveNext();
				while (workItemHasElements)
				{
					var currentWorkItem = workItemIterator.Current;
					yield return
						currentWorkItem.GetProcessedItem(currentWorkItem.StartDate, currentWorkItem.EndDate,
							new Dictionary<string, string>());
					workItemHasElements = workItemIterator.MoveNext();
				}
			}
		}

		private static IEnumerable<WorkItem> MergeSorted(IEnumerable<CollectedItemInterval> measurements,
			IEnumerable<Model.WorkItems.WorkItem> workitems)
		{
			Contract.Requires(measurements.IsSortedAndNonOverlapping());
			Contract.Requires(workitems.IsSortedAndNonOverlapping());
			Contract.Ensures(Contract.Result<IEnumerable<WorkItem>>().IsSortedAndNonOverlapping());

			using (Profiler.Measure())
			{
				var workItemIterator = workitems.GetEnumerator();
				var measurementIterator = measurements.GetEnumerator();
				var workItemHasElements = workItemIterator.MoveNext();
				CollectedItemInterval lastProcessed = null;
				var measurementHasElements = measurementIterator.MoveNext();
				while (workItemHasElements && measurementHasElements)
				{
					var currentWorkItem = workItemIterator.Current;
					var currentMeasurement = measurementIterator.Current;
					if (currentWorkItem.StartDate < currentMeasurement.EndDate &&
					    currentWorkItem.EndDate > currentMeasurement.StartDate) // Is overlapping?
					{
						var newStart = new DateTime(Math.Max(currentMeasurement.StartDate.Ticks, currentWorkItem.StartDate.Ticks));
						var newEnd = new DateTime(Math.Min(currentMeasurement.EndDate.Ticks, currentWorkItem.EndDate.Ticks));
						var values = new Dictionary<string, string>(currentMeasurement.Values);
						lastProcessed = currentMeasurement;
						yield return currentWorkItem.GetProcessedItem(newStart, newEnd, values);
					}
					else
					{
						if (currentWorkItem.EndDate < currentMeasurement.StartDate)
						{
							var newStart = currentWorkItem.StartDate;
							var newEnd = currentWorkItem.EndDate;
							var values = lastProcessed != null
								? new Dictionary<string, string>(lastProcessed.Values)
								: new Dictionary<string, string>();

							yield return currentWorkItem.GetProcessedItem(newStart, newEnd, values);
						}
					}

					if (currentWorkItem.EndDate <= currentMeasurement.EndDate) workItemHasElements = workItemIterator.MoveNext();
					if (currentWorkItem.EndDate >= currentMeasurement.EndDate) measurementHasElements = measurementIterator.MoveNext();
				}
			}
		}
	}
}
