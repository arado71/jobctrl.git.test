using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.CustomReporting;
using Reporter.Model;
using Reporter.Model.ProcessedItems;

namespace CustomReport
{
	public class Class1 : IReport
	{
		private readonly Dictionary<Device, TimeSpan> worktimeAggregates = new Dictionary<Device, TimeSpan>(); 

		public Class1(WebApi api)
		{ }

		public string Name 
		{
			get
			{
				return "Activity Report";
			}
		}

		public void Process(WorkItem data)
		{
			AddWorkTime(data);
		}

		public DataSet GetResults()
		{
			var result = new DataSet();
			WriteWorktimeByUser(result.Tables.Add("WorktimeByUser"));
			return result;
		}

		private void WriteWorktimeByUser(DataTable table)
		{
			table.Columns.Add("UserId");
			table.Columns.Add("TotalTime");
			foreach (var userWorktimes in worktimeAggregates.GroupBy(x => x.Key.UserId, x => x.Value.Ticks))
			{
				table.Rows.Add(userWorktimes.Key, new TimeSpan(userWorktimes.Sum()));
			}
		}

		private void AddWorkTime(WorkItem data)
		{
			var device = Device.FromProcessedItem(data);
			TimeSpan totalTime;
			if (!worktimeAggregates.TryGetValue(device, out totalTime))
			{
				worktimeAggregates.Add(device, data.Duration);
			}
			else
			{
				worktimeAggregates[device] = totalTime + data.Duration;
			}
		}
	}
}
