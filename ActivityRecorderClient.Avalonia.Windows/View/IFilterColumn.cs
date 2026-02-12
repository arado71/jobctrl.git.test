using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.View
{
	public interface IFilterColumn
	{
		bool FilteringEnabled { get; set; }
		string FilterString { get; set; }
		void ApplyFilters();
		bool ShouldShowRow(int rowIndex);
	}
}
