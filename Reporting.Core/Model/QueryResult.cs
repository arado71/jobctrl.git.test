using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;

namespace Reporter.Model
{
	public class QueryResult
	{
		public QueryResult()
		{
		}

		public QueryResult(IEnumerable<ICollectedItem> collectedItems, IEnumerable<IWorkItemDeletion> manualWorkItems,
			IEnumerable<IWorkItem> workItems)
		{
			CollectedItems = collectedItems.ToList();
			ManualWorkItems = manualWorkItems.ToList();
			WorkItems = workItems.ToList();
		}

		public List<ICollectedItem> CollectedItems { get; set; }
		public List<IWorkItemDeletion> ManualWorkItems { get; set; }
		public List<IWorkItem> WorkItems { get; set; }

		public NetQueryResult CalculateNet()
		{
			return NetQueryResult.CreateFrom(CollectedItems, ManualWorkItems, WorkItems);
		}
	}
}
