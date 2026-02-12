using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Model.ProcessedItems;

namespace Reporter.Model
{
	public class TransformResult
	{
		public List<WorkItem> Items { get; set; }
		public HashSet<string> Columns { get; set; } 
	}
}
