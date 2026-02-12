using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Reporter.Interfaces;
using Reporter.Model.WorkItems;

namespace Reporter.Model
{
	public class NetQueryResult
	{
		private NetQueryResult() { }

		public static NetQueryResult CreateFrom(IEnumerable<ICollectedItem> collectedItems, IEnumerable<IWorkItemDeletion> deletes, IEnumerable<IWorkItem> workItems)
		{
			Contract.Requires<NullReferenceException>(collectedItems != null, "Parameter collectedItems can't be null");
			Contract.Requires<NullReferenceException>(deletes != null, "Parameter deletes can't be null");
			Contract.Requires<NullReferenceException>(workItems != null, "Parameter workItems can't be null");
			Contract.Ensures(Contract.Result<NetQueryResult>() != null);
			Contract.Ensures(Contract.Result<NetQueryResult>().Devices.IsSorted(x => x.UserId));
			Contract.Ensures(Contract.Result<NetQueryResult>().EffectiveWorkItems.All(x => x.Value.IsSorted(y => y.StartDate)));

			using (Profiler.Measure())
			{
				var effective = WorkItemHelper.GetEffectiveWorkItems(workItems.OrderBy(x => x.StartDate).ToArray(), deletes.OrderBy(x => x.StartDate).ToArray());
				var res = new NetQueryResult
				{
					CollectedItems = collectedItems.OrderBy(x => x.CreateDate).ToArray(),
					EffectiveWorkItems = effective,
				};

				res.Devices = res.EffectiveWorkItems.Keys.OrderBy(x => x.UserId).ToArray();
				return res;
			}
		}

		internal Device[] Devices { get; private set; }
		internal ICollectedItem[] CollectedItems { get; private set; }
		internal Dictionary<Device, WorkItem[]> EffectiveWorkItems { get; private set; }
	}
}
