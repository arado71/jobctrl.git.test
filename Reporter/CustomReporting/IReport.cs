using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Model.ProcessedItems;

namespace Reporter.CustomReporting
{
	public interface IReport
	{
		string Name { get; }

		void Process(WorkItem data);

		DataSet GetResults();
	}
}
