using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Menu
{
	public class MenuDifference
	{
		public MenuDifference(IEnumerable<WorkDataWithParentNames> newWorks, IEnumerable<WorkDataWithParentNames> deletedWorks,
			IEnumerable<WorkDifference> existingWorks)
		{
			NewWorks = newWorks.ToArray();
			DeletedWorks = deletedWorks.ToArray();
			ExistingWorks = existingWorks.ToArray();
		}

		public WorkDataWithParentNames[] NewWorks { get; private set; }
		public WorkDataWithParentNames[] DeletedWorks { get; private set; }
		public WorkDifference[] ExistingWorks { get; private set; }

		public class WorkDifference
		{
			public WorkDataWithParentNames OldWork { get; set; }
			public WorkDataWithParentNames NewWork { get; set; }
		}
	}
}
