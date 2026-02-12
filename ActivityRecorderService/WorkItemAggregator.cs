using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService
{
	public class WorkItemAggregator : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public event EventHandler<EventArgs> WorkItemsAggregated;

		private void OnWorkItemsAggregated()
		{
			EventHandler<EventArgs> aggregated = WorkItemsAggregated;
			if (aggregated != null) aggregated(this, EventArgs.Empty);;
		}

		public WorkItemAggregator()
			: base(log)
		{
			ManagerCallbackInterval = ConfigManager.AggregateInterval;
		}

		protected override void ManagerCallbackImpl()
		{
			using (var context = new AggregateDataClassesDataContext())
			{
				context.CommandTimeout = 10 * 60 * 60; //10 hours poor man's hax
				var sw = Stopwatch.StartNew();
				log.Info("Calling UpdateHourlyAggregateWorkItems sproc");
				context.UpdateHourlyAggregateWorkItems();
				log.Info("Successfully called UpdateHourlyAggregateWorkItems sproc which finished in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
				var lastItem = context.AggregateLastWorkItems.Select(n => n.LastAggregatedId).FirstOrDefault();
				log.Info("Aggregated WorkItem Data till id: " + lastItem);
			}
			OnWorkItemsAggregated();
		}
	}
}
