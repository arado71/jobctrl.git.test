using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Controller
{
	public class WorkChangeEventArgs : EventArgs
	{
		public WorkData Work { get; set; }
	}
}
