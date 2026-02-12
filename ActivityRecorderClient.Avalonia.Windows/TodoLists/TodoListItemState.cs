using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.TodoLists
{
	public enum TodoListItemState
	{
		Unspecified=0,
		Opened=1,
		Finished=2,
		Postponed=3,
		Canceled=4,
	}
}
