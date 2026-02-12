using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class TodoListToken
	{
		public TodoListToken(bool isAcquired, string editedByLastName = "", string editedByFirstName = "")
		{
			IsAcquired = isAcquired;
			EditedByLastName = editedByLastName;
			EditedByFirstName = editedByFirstName;
		}
	}
}
